using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class PDF417 : Barcode
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(FormattableString.Invariant($"<{CodeType}(\"{{InputText}}\",{{WidthInColumns}},{{HeightInRows}}[,\"{{ErrorCorrectionLevel}}\",\"{{Charset}}\"])>"),
				ResString.GetMultilingualString("6783E88E-43E3-4C54-8F89-45599F7B72AE",
					@"Returns a {0} code image that contains the text with the following supplied specifications:
- {1}: Number of columns to span in Excel;
- {2}: Number of rows to span in Excel;
- {3} (optional): {0} code error correction level (L0, L1, L2, L3, L4, L5, L6, L7, L8, AUTO);
- {4} (optional): The name of the text character set.",
				CodeType, "WidthInColumns", "HeightInRows", "ErrorCorrectionLevel", "Charset"),
				new List<(string example, object expectedResult)>
				{
					($"<{CodeType}(\"<BarCodeText>\", 2, 4)>", ExampleContentForDocumentation),
					($"<{CodeType}(\"<BarCodeText>\", 2, 4, \"L1\", \"UTF-8\")>", ExampleContentForDocumentation)
				}
			);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "No specific exception can be specified from 3rd party libraries.")]
		protected override object GetReplacementCore(string macro, Report report)
		{
			object result;
			try
			{
				var matchedGroups = Regex.Match(macro).Groups;
				var inputText = matchedGroups["InputText"].Value;
				var widthInColumns = Convert.ToInt32(matchedGroups["WidthInColumns"].Value, CultureInfo.InvariantCulture);
				var heightInColumns = Convert.ToInt32(matchedGroups["HeightInRows"].Value, CultureInfo.InvariantCulture);
				var sizeInPixels = GetImageSize(report, widthInColumns, heightInColumns);
				var errorCorrectionLevelGroup = matchedGroups["ErrorCorrectionLevel"];
				var errorCorrectionLevel = !string.IsNullOrEmpty(errorCorrectionLevelGroup.Value) ? (Pdf417CodeErrorCorrectionLevel)Enum.Parse(typeof(Pdf417CodeErrorCorrectionLevel), errorCorrectionLevelGroup.Value) : (Pdf417CodeErrorCorrectionLevel?)null;
				var charset = !string.IsNullOrEmpty(matchedGroups["Charset"].Value) ? matchedGroups["Charset"].Value : null;

				var codeBitmap = BarcodeProcessor.CreateCode(inputText, new Pdf417CodeCreationOptions
				{
					CharacterSet = charset,
					ErrorCorrectionLevel = errorCorrectionLevel,
					Height = sizeInPixels.Height,
					Width = sizeInPixels.Width
				});
				result = new ExcelImage(codeBitmap, sizeInPixels.Height, sizeInPixels.Width, CodeType, true, true);
			}
			catch (Exception ex)
			{
				ReportError(ex, CodeType, macro, report);
				result = null;
			}
			return result;
		}

		protected override string CodeType { get; } = "PDF417";
		protected internal override IBarcodeProcessor BarcodeProcessor { get; } = new Pdf417CodeProcessor();

		#region Regex

		public override Regex Regex => regex;
		static readonly Regex regex = new Regex(
			@"^<\s*pdf417\s*\(\s*""(?<InputText>.*)""\s*,\s*(?<WidthInColumns>\d+)\s*,\s*(?<HeightInRows>\d+)(?:\s*,\s*""(?<ErrorCorrectionLevel>(L0|L1|L2|L3|L4|L5|L6|L7|L8|AUTO))""(?:\s*,\s*""(?<Charset>[\w-]+)"")?)?\s*\)\s*>$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		#endregion
	}
}
