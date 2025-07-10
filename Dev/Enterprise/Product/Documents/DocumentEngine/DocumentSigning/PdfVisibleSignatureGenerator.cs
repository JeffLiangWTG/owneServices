using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using FlexCel.Core;
using FlexCel.Pdf;

namespace Enterprise.DocumentEngine.DigitalSignature
{
	public class PdfVisibleSignatureGenerator
	{
		public PdfVisibleSignatureGenerator(TPaperDimensions paperDimensions, string signerName, int page, ZGuid? branch = null)
		{
			PaperDimensions = paperDimensions;
			Page = page;
			Branch = branch == null || branch.Value.IsEmpty ? Guid.Empty : branch.Value.ToGuid();
			SignerName = DocumentsDataRegistry.Instance.OverrideSignedByUsername.GetFallBackValueAtAllLevels(Guid.Empty, Branch, Guid.Empty) ?? signerName;
			SigningEntity = DocumentsDataRegistry.Instance.SigningServiceSignedByLabel.GetFallBackValueAtAllLevels(Guid.Empty, Branch, Guid.Empty);
			SignatureBackgroundImage = DocumentsDataRegistry.Instance.SignatureBackgroundImage.GetFallBackValueAtAllLevels(Guid.Empty, Branch, Guid.Empty);

			GenerateSignatureString();
			ConfigureFontAndWidth();
		}

		TPaperDimensions PaperDimensions { get; }
		Image SignatureBackgroundImage { get; }
		int SignatureImageWidth { get; set; }
		Font SignatureFont { get; set; }
		string SignatureText { get; set; }
		protected internal string SignerName { get; }
		string SigningEntity { get; }
		int Page { get; }
		Guid Branch { get; }

		const int ImageHeight = 100;
		const int RectangleHeight = 36;
		const int MaxFontSize = 15;
		const int MinFontSize = 7;
		const int MaxWidth = 700;
		const int MinWidth = 150;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string FontFamily = "Tahoma";

		public static string SignedByUsernameOnBehalfOf => ResString.GetMultilingualString("8BB624F9-48D1-4BAD-99FE-34F2F2E78E64", "Signed by {0} on behalf of");

		byte[] ImageData => (byte[])new ImageConverter().ConvertTo(GetSignatureImage(), typeof(byte[]));

		TUIRectangle Rectangle => GetSignatureRectangle();

		public TPdfVisibleSignature GenerateTPdfVisibleSignature(TPdfSignerFactory signerFactory,
			string name = null,
			string reason = null,
			string location = null,
			string contactInfo = null,
			TPdfAllowedChanges allowedChanges = TPdfAllowedChanges.None)
		{
			return new TPdfVisibleSignature(signerFactory, name ?? SigningEntity,
				reason, location, contactInfo, Page, Rectangle, ImageData)
				{ AllowedChanges = allowedChanges };
		}

		TUIRectangle GetSignatureRectangle()
		{
			var rectangleWidth = RectangleHeight * SignatureImageWidth / ImageHeight;

			var location  = GetRectangleLocation(rectangleWidth, RectangleHeight);

			return new TUIRectangle(
				location.X,
				location.Y,
				rectangleWidth,
				RectangleHeight);
		}

		TPointF GetRectangleLocation(double rectangleWidthPoints, double rectangleHeightPoints)
		{
			var maxHorizontalMargin = PaperDimensions.SizeInPoints.Width - rectangleWidthPoints;
			var maxVerticalMargin = PaperDimensions.SizeInPoints.Height - rectangleHeightPoints;
			var xMargin = MillimetersToPoints(DocumentsDataRegistry.Instance.SignatureImagePositioningHorizontalMargin.GetFallBackValueAtAllLevels(Guid.Empty, Branch, Guid.Empty));
			var yMargin = MillimetersToPoints(DocumentsDataRegistry.Instance.SignatureImagePositioningVerticalMargin.GetFallBackValueAtAllLevels(Guid.Empty, Branch, Guid.Empty));

			var point = TPointF.Empty;

			switch (DocumentsDataRegistry.Instance.SignatureImagePositioningAnchor.GetFallBackValueAtAllLevels(Guid.Empty, Branch, Guid.Empty))
			{
				case DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.BottomLeft:
					point.X = Math.Min(maxHorizontalMargin, xMargin);
					point.Y = Math.Min(maxVerticalMargin, yMargin);
					break;
				case DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.TopLeft:
					point.X = Math.Min(maxHorizontalMargin, xMargin);
					point.Y = Math.Max(0, maxVerticalMargin - yMargin);
					break;
				case DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.BottomRight:
					point.X = Math.Max(0, maxHorizontalMargin - xMargin);
					point.Y = Math.Min(maxVerticalMargin, yMargin);
					break;
				case DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.TopRight:
					point.X = Math.Max(0, maxHorizontalMargin - xMargin);
					point.Y = Math.Max(0, maxVerticalMargin - yMargin);
					break;
			}

			return point;
		}

		Image GetSignatureImage()
		{
			var image = new Bitmap(SignatureImageWidth, ImageHeight);
			var rect = new Rectangle(0, 0, SignatureImageWidth, ImageHeight);
			var fontAndBackColor = DocumentsDataRegistry.Instance.VisibleSignatureFontAndBackColor.GetFallBackValueAtAllLevels(Guid.Empty, Branch, Guid.Empty);

			using (var gx = Graphics.FromImage(image))
			{
				if (SignatureBackgroundImage != null)
				{
					gx.DrawImage(SignatureBackgroundImage, rect, 0, 0, SignatureImageWidth, ImageHeight, GraphicsUnit.Pixel);
				}
				else
				{
					gx.FillRectangle(new SolidBrush(fontAndBackColor.SecondaryColor), rect);
				}
				gx.DrawString(SignatureText, SignatureFont, new SolidBrush(fontAndBackColor.PrimaryColor), rect);
				gx.Flush();
			}

			return image;
		}

		void GenerateSignatureString()
		{
			var now = ZDateTime.Now;
			SignatureText = $@"{string.Format(SignedByUsernameOnBehalfOf, SignerName)}
{SigningEntity}
Date: {PdfSignatureFactory.GetSignatureDate(now)}
{now.ToShortTimeString()} {now.ToOffset().OffsetString}";
		}

		void ConfigureFontAndWidth()
		{
			var maxImageWidth = SignatureBackgroundImage?.Width ?? MaxWidth;

			var fontSize = (float)MaxFontSize;
			var wrapText = true;

			var selectedFont = new Font(FontFamily, MinFontSize);
			var selectedWidth = (float)MaxWidth;

			using (var gx = Graphics.FromImage(new Bitmap(1, 1)))
			{
				while (fontSize >= MinFontSize)
				{
					var font = new Font(FontFamily, fontSize);
					var size = gx.MeasureString(SignatureText, font, new SizeF(wrapText ? maxImageWidth : 0, 0));

					if (size.Width <= maxImageWidth && size.Height <= ImageHeight)
					{
						selectedFont = font;
						selectedWidth = size.Width;

						if (!wrapText)
						{
							break;
						}

						wrapText = false;
					}
					else
					{
						fontSize -= 0.5f;
					}
				}
			}

			SignatureFont = selectedFont;
			SignatureImageWidth = SignatureBackgroundImage?.Width ?? Math.Max(MinWidth, (int)Math.Ceiling(selectedWidth));
		}

		public static double MillimetersToPoints(int value)
		{
			const double millimetersToPoints = 2.835;
			return millimetersToPoints * value;
		}
	}
}
