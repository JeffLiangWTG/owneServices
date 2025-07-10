using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class GetCategoryDescForReport : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			var registryName = AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.Caption;
			var path = AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.HumanReadableRegistryPath();
			return new ValueProviderDocumenter("<GetCategoryDescForReport({ReportCategory},{ReportType})>",
				ResString.GetMultilingualString("8c9a1b3d-68fb-4d89-9555-0d3cd68e5ddb", "Returns the Report Category description from the Registry item '{0}', this registry item can be found at {1}.", registryName, path),
				new List<(string example, object expectedResult)> { ((NoResString)"<GetCategoryDescForReport(\"A01\",\"TT0\")>", (NoResString)"Balance Sheet1") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			ZString reportType = match.Groups[2].Value;
			ZString reportCategory = match.Groups[1].Value;

			var reportTypes = AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.Value;
			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.China)
			{
				reportTypes.AddRange(AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.Value);
			}

			var reportCategories = reportTypes.GetReportTypeCategoriesFromCode(reportType);
			return reportCategories.GetDescriptionFromCode(reportCategory);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*GetCategoryDescForReport\(\s*\""*([^\""]+)\""*\s*\,\s*\""*([^\""]+)\""*\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
