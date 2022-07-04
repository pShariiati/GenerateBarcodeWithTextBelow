using System.Drawing;
using System.Drawing.Imaging;
using BarcodeLib;
using DNTPersianUtils.Core;

namespace GenerateBarcodeWithTextBelow.Web;

public class BarcodeHelper
{
    public static string GenerateBarcodeWithText(string input, string textBelow)
    {
        // Type 39 doesn't support lower case letters, for prevent exception, we convert all input letters to upper case
        // more details: https://www.dntips.ir/newsarchive/details/18019
        input = input.ToUpperInvariant();

        // barcode: 50 pixels
        // margin: top 5 pixels
        // height of each text line is 17 pixels
        // text: maximum 3 lines
        // each 30 letters is: 1 line

        var eachLineHeight = 17;

        var eachLineLetters = 30;

        var maximumLines = 3;

        var maximumTextHeight = eachLineHeight * maximumLines;

        var resultWidth = 250;

        var barcodeHeight = 50;

        var textY = barcodeHeight + 5;

        // each 30 letters is: 1 line for example input length is 150 letters and for show 100 letters we need (150 / 30) 5 lines
        // each line is 17 pixels and text height will be (17 * 5) 102 pixels
        var textHeight = (textBelow.Length / eachLineLetters) * eachLineHeight;

        // if height of text be greater than (eachLineHeight * maximumLines) we use maximum text height (eachLineHeight * maximumLines)
        textHeight = textHeight > maximumTextHeight ? maximumTextHeight : textHeight;

        // if text height be less than 1 line we set 1 line height (17 pixels) to the text height
        // text height minimum is equal 1 linle (17 pixels)
        textHeight = textHeight < eachLineHeight ? eachLineHeight : textHeight;

        var resultHeight = textY + textHeight;

        #region MainBitmap

        var mainBitmap = new Bitmap(resultWidth, resultHeight);
        using var rectangleGraphics = Graphics.FromImage(mainBitmap);
        {
            var rectangle = new Rectangle(0, 0, resultWidth, resultHeight);
            rectangleGraphics.FillRectangle(Brushes.OrangeRed, rectangle);
        }

        using var rectangleStream = new MemoryStream();
        {
            mainBitmap.Save(rectangleStream, ImageFormat.Png);
        }

        #endregion

        #region Barcode

        var barcodeImage = GenerateBarcodeImage(input, resultWidth, barcodeHeight);

        #endregion

        #region MergedRectangleAndBarcode

        var newMainBitmap = (Bitmap)Image.FromStream(rectangleStream);
        var newBarcodeBitmap = barcodeImage;
        using var newRectangleGraphics = Graphics.FromImage(newMainBitmap);
        {
            newRectangleGraphics.DrawImage(newBarcodeBitmap, 0, 0);
        }

        using var mergedRectangleAndBarcodeStream = new MemoryStream();
        {
            newMainBitmap.Save(mergedRectangleAndBarcodeStream, ImageFormat.Png);
        }

        #endregion

        #region WriteText

        var barcodeBitmap = (Bitmap)Image.FromStream(mergedRectangleAndBarcodeStream);
        using var graphics = Graphics.FromImage(barcodeBitmap);
        {
            using var font = new Font("Tahoma", 10);
            {
                var rect = new Rectangle(0, textY, resultWidth, textHeight);
                var sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.EllipsisCharacter;
                if (textBelow.ContainsFarsi())
                    sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                sf.LineAlignment = StringAlignment.Center;
                graphics.DrawString(textBelow, font, Brushes.Black, rect, sf);
                //graphics.DrawRectangle(Pens.Green, rect);
            }
        }

        using var finalStream = new MemoryStream();
        {
            barcodeBitmap.Save(finalStream, ImageFormat.Png);
        }

        #endregion

        return Convert.ToBase64String(finalStream.ToArray());
    }

    private static Bitmap GenerateBarcodeImage(string input, int width, int height)
    {
        // BarcodeLib package
        var barcodeInstance = new Barcode();
        var barcodeImage = barcodeInstance.Encode(BarcodeLib.TYPE.CODE39, input, Color.Black,
            Color.OrangeRed, width, height);
        using var barcodeStream = new MemoryStream();
        {
            barcodeImage.Save(barcodeStream, ImageFormat.Png);
        }
        return (Bitmap)Image.FromStream(barcodeStream);
    }
}