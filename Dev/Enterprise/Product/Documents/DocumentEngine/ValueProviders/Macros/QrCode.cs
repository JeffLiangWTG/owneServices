using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class QrCode : Barcode
	{
		#region Documentation

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<QRCode(\"{InputText}\",{WidthInColumns},{HeightInRows}[,{Version}][,\"{ErrorCorrectionLevel}\"][,\"{Charset}\"][,\"Image:{LogoCode}\"][,{ShowWarning}])>",
				ResString.GetMultilingualString("CD3522A5-48A8-48AD-8316-A10542C483FE",
				@"Returns a QR code image that contains the text with the following supplied specifications:
- {0}: Number of columns to span in Excel;
- {1}: Number of rows to span in Excel;
- {2} (optional): QR code version (1-40);
- {3} (optional): QR code margin in pixels;
- {4} (optional): QR code error correction level (L, M, Q, H);
- {5} (optional): The name of the text character set;
- {6} (optional): The code of an image defined in the {6} registry setting. This image will appear in the center of the QR code.
- {7} (optional): Enable/Disable a warning popup on blank QR code (Default: false);",
			"WidthInColumns", "HeightInRows", "Version", "MarginInPixels", "ErrorCorrectionLevel", "Charset", "LogoCode", "ShowWarning", DocumentsDataRegistry.Instance.DocumentImages.HumanReadableRegistryPath()),
				new List<(string example, object expectedResult)>
				{
					("<QrCode(\"<BarCodeText>\", 3, 3)>", ExampleContentForDocumentation),
					("<QrCode(\"<BarCodeText>\", 3, 3, 2)>", ExampleContentForDocumentation),
					((NoResString)"<QrCode(\"<BarCodeText>\", 3, 3, 2, 5, \"L\", \"UTF-8\")>", ExampleContentForDocumentation),
					((NoResString)"<QrCode(\"<BarCodeText>\", 3, 3, 2, \"L\")>", ExampleContentForDocumentation),
					((NoResString)"<QrCode(\"<BarCodeText>\", 3, 3, 2, \"UTF-8\")>", ExampleContentForDocumentation),
					((NoResString)"<QrCode(\"<BarCodeText>\", 3, 3, \"UTF-8\")>", ExampleContentForDocumentation),
					((NoResString)"<QrCode(\"<BarCodeText>\", 3, 3, \"L\")>", ExampleContentForDocumentation),
					((NoResString)"<QrCode(\"<BarCodeText>\", 3, 3, \"L\", \"UTF-8\")>", ExampleContentForDocumentation),
					((NoResString)"<QrCode(\"<BarCodeText>\", 3, 3, \"Image:CRS\")>", ExampleContentForDocumentation),
					((NoResString)"<QrCode(\"<BarCodeText>\", 3, 3, \"UTF-8\", \"Image:CRS\")>", ExampleContentForDocumentation),
					((NoResString)"<QrCode(\"<BarCodeText>\", 3, 3, True)>", ExampleContentForDocumentation)
				}
				);
		}

		protected override string CodeType { get; } = "QR";

		#endregion

		#region Replacement

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "No specific exception can be specified from 3rd party libraries.")]
		protected override object GetReplacementCore(string macro, Report report)
		{
			object result;
			bool showWarning = false;

			try
			{
				// Parse parameters.
				var matchedGroups = Regex.Match(macro).Groups;
				var inputText = matchedGroups["InputText"].Value;
				var widthInColumns = Convert.ToInt32(matchedGroups["WidthInColumns"].Value, CultureInfo.InvariantCulture);
				var heightInColumns = Convert.ToInt32(matchedGroups["HeightInRows"].Value, CultureInfo.InvariantCulture);
				var sizeInPixels = GetImageSize(report, widthInColumns, heightInColumns);
				var version = GetNullableIntValue(matchedGroups["Version"]);
				var marginInPixels = GetNullableIntValue(matchedGroups["MarginInPixels"]);
				var errorCorrectionLevelGroup = matchedGroups["ErrorCorrectionLevel"];
				var errorCorrectionLevel = !string.IsNullOrEmpty(errorCorrectionLevelGroup.Value) ? (QrCodeErrorCorrectionLevel)Enum.Parse(typeof(QrCodeErrorCorrectionLevel), errorCorrectionLevelGroup.Value) : (QrCodeErrorCorrectionLevel?)null;
				var charset = !string.IsNullOrEmpty(matchedGroups["Charset"].Value) ? matchedGroups["Charset"].Value : null;
				var logoCode = matchedGroups["LogoCode"].Value;
				var showWarningGroup = matchedGroups["ShowWarning"];
				showWarning = bool.TryParse(showWarningGroup.Value, out var parsedValue) && parsedValue;

				if (string.IsNullOrEmpty(inputText))
				{
					result = Res.GetString("03bb8221-be78-44f2-9bac-d085a18792fd", "Not possible to create QR Code, missing required data");

					if (showWarning)
					{
						ReportError(new Exception(result.ToString()), "QR", macro, report);
					}
					return result;
				}

				// Generate the QR code.
				int lengthInPixels = sizeInPixels.Width > sizeInPixels.Height ? sizeInPixels.Height : sizeInPixels.Width;
				var qrCodeBitmap = BarcodeProcessor.CreateCode(inputText, new QrCodeCreationOptions
				{
					Width = lengthInPixels,
					Height = lengthInPixels,
					Version = version,
					Margin = marginInPixels ?? 0,
					ErrorCorrectionLevel = errorCorrectionLevel,
					CharacterEncoding = charset
				});
				qrCodeBitmap.SetResolution(ControlDpiScalingHelper.BaseDpiX, ControlDpiScalingHelper.BaseDpiY);

				SetCenterLogo(qrCodeBitmap, logoCode);

				result = new ExcelImage(qrCodeBitmap, lengthInPixels, lengthInPixels, "QRCode", true, true);
			}
			catch (Exception ex)
			{
				ReportError(ex, "QR", macro, report);
				result = null;
			}
			return result;
		}

		int? GetNullableIntValue(Group group)
		{
			var intNull = (int?)null;
			var result = !string.IsNullOrEmpty(group?.Value) ? Convert.ToInt32(group.Value, CultureInfo.InvariantCulture) : intNull;
			return result;
		}

		#endregion

		#region Set Center Logo

		internal void SetCenterLogo(Bitmap qrCodeBitmap, string logoCode)
		{
			if (!string.IsNullOrEmpty(logoCode))
			{
				var logo = GetLogo(logoCode);
				if (logo != null)
				{
					using (var graphics = Graphics.FromImage(qrCodeBitmap))
					{
						graphics.DrawImage(logo,
							(qrCodeBitmap.Width - logo.Width * qrCodeBitmap.HorizontalResolution / logo.HorizontalResolution) / 2,
							(qrCodeBitmap.Height - logo.Height * qrCodeBitmap.VerticalResolution / logo.VerticalResolution) / 2);
					}
				}
			}
		}

		internal System.Drawing.Image GetLogo(string logoCode)
		{
			var images = DocumentsDataRegistry.Instance.DocumentImages.Value
				.OfType<SystemDefinableRegistryImage>()
				.Where(t => t.Code.EqualsIgnoringCase(logoCode))
				.OrderBy(x => x.SystemDefined)
				.FirstOrDefault();

			return images?.Image;
		}

		#endregion

		#region Regex

		public override Regex Regex => regex;
		static readonly Regex regex = new Regex(@"^<\s*qrcode\s*\(\s*""(?<InputText>(.|\n)*)""\s*,\s*(?<WidthInColumns>\d+)\s*,\s*(?<HeightInRows>\d+)(?:\s*,\s*(?<Version>\d+))?(?:\s*,\s*(?<MarginInPixels>\d+))?(?:\s*,\s*""(?<ErrorCorrectionLevel>[LMQH])"")?(?:\s*,\s*""(?<Charset>[\w-]+)"")?(?:\s*,\s*""(\s*image(?:\s*):(?:\s*)(?<LogoCode>\S+)(?:\s*))"")?(?:\s*,\s*(?<ShowWarning>true|false))?\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		#endregion

		protected internal override IBarcodeProcessor BarcodeProcessor { get; } = new QrCodeProcessor();
	}
}
