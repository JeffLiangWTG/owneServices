using System;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.MasterFiles.Integration;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Enterprise.DocumentEngine.Imaging
{
	/// <summary>
	/// Adds a new watermark across an EXISTING .pdf file.  Not the same as adding "test only" across the doc before it's rendered.  
	/// Allows you to mark old/spent PDF documents as "void" rather than deleting them permenently (which is messy).
	/// To see it in action, see C:\dev\Enterprise\Product\Operations\Customs\GB\Enterprise.Customs.GB.Ccsuk\AirCargoInventory\BusinessObjects\Helpers\EDocsDeleter.cs
	/// Uses PdfSharp.dll - free even for commerical use - http://www.pdfsharp.net/PDFsharp_License.ashx
	/// </summary>
	public static class WaterMarkOnExistingPfdAdder
	{
		public static void UpdateStorageDocPdfToAddRedWatermark(IeDoc existingStorageDocsBaseWhichYouWantWatermarking, string watermarkText = "VOID", int textFontSize = 200, bool rotateAntiClockwise = true, string fontName = "Verdana")
		{
			PdfDocument pdfDocument = null;
			using (var outStream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				using (var stream = existingStorageDocsBaseWhichYouWantWatermarking.GetImageDataReader())
				{
					try
					{
						pdfDocument = PdfReader.Open(stream);
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						return;
					}

					try
					{
						var page = pdfDocument.Pages[0];
						var font = new XFont(fontName, textFontSize, XFontStyle.BoldItalic);
						var graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend);
						var sizeOfText = graphics.MeasureString(watermarkText, font);
						graphics.TranslateTransform(page.Width / 2, page.Height / 2);
						int rotationDirection = rotateAntiClockwise ? 1 : -1;
						graphics.RotateTransform(rotationDirection * Math.Atan(page.Height / page.Width) * 180 / Math.PI);
						graphics.TranslateTransform(-page.Width / 2, -page.Height / 2);
						var stringFormat = new XStringFormat();
						stringFormat.Alignment = XStringAlignment.Near;
						stringFormat.LineAlignment = XLineAlignment.Near;
						var solidBrush = new XSolidBrush(XColor.FromArgb(128, 255, 0, 0));
						var position = new XPoint((page.Width - sizeOfText.Width) / 2, (page.Height - sizeOfText.Height) / 2);
						graphics.DrawString(watermarkText, font, solidBrush, position, stringFormat);

						pdfDocument.Save(outStream);
						outStream.Position = 0;
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						return;
					}
				}

				existingStorageDocsBaseWhichYouWantWatermarking.ImageData = outStream.ToByteArray();
			}
		}
	}
}

/*
 PDFsharp is published under the MIT License.

Copyright (c) 2005-2012 empira Software GmbH, Troisdorf (Germany)

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions: 

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software. 

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
PDFsharp is Open Source.
You can copy, modify and integrate the source code of PDFsharp in your application without restrictions at all.
This also applies to commercial products (both open source and closed source).
 
 */
