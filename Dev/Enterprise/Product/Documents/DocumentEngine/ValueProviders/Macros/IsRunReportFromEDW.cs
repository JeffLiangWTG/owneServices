using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class IsRunReportFromEDW : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<IsRunReportFromEDW>",
				ResString.GetMultilingualString("552300CF-706D-46A9-B0FA-D60C146B8989", "If user click on \"Use EDW as Report Data Source\" when running reports, the result will be \"Y\" otherwise the result will be \"N\"."),
				new List<(string example, object expectedResult)> { ("<IsRunReportFromEDW>", "N") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return report?.IsEdwDataSource.ToYN() ?? "N";
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)IsRunReportFromEDW(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
