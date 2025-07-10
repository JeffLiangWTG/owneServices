using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	sealed class ShrinkToFitForBillOfLading : ShrinkToFit, INonVisualisableValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ShrinkToFitForBillOfLading[({MinimumFontSize})]>",
				ResString.GetMultilingualString("dd91c714-5b0f-470a-8566-38b55123f6eb",
				@"Used as a prefix to a subsequent macro. This macro should only be used on bill of lading template, under circumstances that the size of cell on template cannot be adjusted. If the value inserted by the subsequent macro does not fit in the cell in the template, the font size will be progressively reduced until it fits, to the minimum of {0}. If no minimum font size is specified, the text will be reduced to a minimum of 8 point.
Note: {1} Will be ignored if {2} is also specified in the same cell or the cell Wrap Text property is not checked. ",
				"{MinimumFontSize}", "<ShrinkToFitForBillOfLading>", "<AutoHeight>"),
				new List<(string example, object expectedResult)> { ("<ShrinkToFitForBillOfLading><ShipmentNumber>", null), ("<ShrinkToFitForBillOfLading(4.5)><ShipmentNumber>", null) });
		}

		internal static void ShrinkForBillOfLading(string cellContent, float minimumFontSize, Report report)
		{
			ShrinkToFit.Shrink(cellContent, minimumFontSize, report, ShrinkToFitForBillOfLading.GetTextHeightMultiplier(report));
		}

		static float GetTextHeightMultiplier(Report report)
		{
			const float defaultMultiplier = 1f;
			const float xlsMultiplier = 0.65f;
			const float pdfMultiplier = 0.92f;

			if (report.DeliveryContact != null)
			{
				// The height of text is bigger on Excel and PDF than on TIF and preview (same font and size). Thus, need to use a bit smaller size on PDF,
				// and even smaller size on Excel. The multiplier 0.92 and 0.65 come from experiments.
				// This way works but looks a bit cheating. Need to be replaced when find a way to accurately measure the size of multiple lines text on PDF and Excel,
				// when text is wrapped or not wrapped.
				switch (report.DeliveryContact.AttachmentType)
				{
					case AttachmentTypeList.Codes.Xls:
					case AttachmentTypeList.Codes.Xlsx:
						return xlsMultiplier;

					case OrgConstants.AttachmentType.PDF:
					case OrgConstants.AttachmentType.PDFA:
					case OrgConstants.AttachmentType.PDFC:
					case "":
						return pdfMultiplier;

					default:
						return defaultMultiplier;
				}
			}

			return defaultMultiplier;
		}

		static readonly Regex regex = new Regex("^" + RegexPattern + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		public override Regex Regex
		{
			get { return regex; }
		}

		/// <summary>
		/// This is the regex really used to interpret this line in the class Enterprise.DocumentEngine.Areas.Area
		/// and by the visualiser to remove the macro text before visualising.
		/// </summary>
		internal new static readonly Regex RegexToFindMacroAnyWhereInString = new Regex(RegexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular Expression")]
		public new const string RegexPattern = @"<[\s]*Shrink[\s]*To[\s]*Fit[\s]*For[\s]*Bill[\s]*Of[\s]*Lading[\s]*(|\([\s]*(?<MinimumFontSize>[0-9.]+)[\s]*\))[\s]*>";

		public new Regex RegexToReplaceMacro => RegexToFindMacroAnyWhereInString;
	}
}
