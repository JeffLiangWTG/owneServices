using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CustomisedColumn : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CustomisedColumn({columnname})>",
				ResString.GetMultilingualString("eecb8211-2d9c-449c-958a-f57de6f27497", @"Returns the value for the Customized Column specified by the column name."),
				new List<(string example, object expectedResult)> { ("<CustomisedColumn(ShoeSize)>", "5") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			return report.ColumnHeadingManager.GetHeadingText(report.WorkSheetCurrentlyBeingProcessed == null ? string.Empty : report.WorkSheetCurrentlyBeingProcessed.SheetName, match.Groups[1].Value);
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		internal static Regex MacroRegex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex("^" + RegexProvider.CustomisedColumnRegex.ToString() + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
