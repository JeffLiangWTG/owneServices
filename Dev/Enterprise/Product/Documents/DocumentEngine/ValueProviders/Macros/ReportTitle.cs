using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ReportTitle : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ReportTitle>",
				ResString.GetMultilingualString("4e1fe92f-1349-4561-894b-0e03eca80eab", "This macro gives the availability to set user defined report titles depend on specific optional sheet or/and column configuration"),
				new List<(string example, object expectedResult)> { ("<ReportTitle>", (NoResString)"Sales Trade Profile") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			object returnValue = null;
			if (report.WorkSheetCurrentlyBeingProcessed == null)
			{
				return null;
			}

			try
			{
				returnValue = report.ColumnHeadingManager.CurrentConfiguration.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].Title;
			}
			catch (ArgumentOutOfRangeException)
			{
				// if the worksheet is not present, just ignore this behavior and return a
				// null object, meaning there is no replacement
			}
			return returnValue;
		}

		#region Implementation

		static readonly Regex fRegex = new Regex(@"^<\s*Report\s*Title\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		public override Regex Regex
		{
			get { return fRegex; }
		}
	}
}

#endregion
