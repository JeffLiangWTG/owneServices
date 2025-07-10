using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class DataMatrix : Barcode
	{
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "No specific exception can be specified from 3rd party libraries.")]
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

				var lengthInPixels = sizeInPixels.Width > sizeInPixels.Height ? sizeInPixels.Height : sizeInPixels.Width;
				var codeBitmap = BarcodeProcessor.CreateCode(inputText, new BarCodeCreationOptions
				{
					Width = lengthInPixels,
					Height = lengthInPixels
				});
				result = new ExcelImage(codeBitmap, lengthInPixels, lengthInPixels, CodeType, true, true);
			}
			catch (Exception e)
			{
				ReportError(e, CodeType, macro, report);
				result = null;
			}

			return result;
		}

		protected override string CodeType { get; } = "DataMatrix";
		protected internal override IBarcodeProcessor BarcodeProcessor { get; } = new DataMatrixCodeProcessor();
		public override Regex Regex => regex;
		static readonly Regex regex = new Regex(
			@"^<\s*datamatrix\s*\(\s*""(?<InputText>.*)""\s*,\s*(?<WidthInColumns>\d+)\s*,\s*(?<HeightInRows>\d+)\s*\)\s*>$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
