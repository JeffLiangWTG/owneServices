using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class GetCategoriesForReport : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			var registryName = AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.Caption;
			var path = AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.HumanReadableRegistryPath();
			return new ValueProviderDocumenter("<GetCategoriesForReport({ReportType})>",
				ResString.GetMultilingualString("973aaa62-a40e-4062-8e5a-335acd579f07", "Returns the Report Categories from the Registry item '{0}', this registry item can be found at {1}.", registryName, path),
				new List<(string example, object expectedResult)> { ((NoResString)"<GetCategoriesForReport(\"TT0\")>", (NoResString)"A01, A02, A03, A04, A05, A01_YED, A02_YED, A03_YED, A04_YED, A05_YED") });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			ZString reportType = match.Groups[1].Value;
			var reportTypes = AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.Value;

			var reportCategory_List = reportTypes.GetReportTypeCategoriesFromCode(reportType);

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.China)
			{
				reportTypes = AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.Value;

				if (reportCategory_List.Count == 0)
				{
					reportCategory_List = reportTypes.GetReportTypeCategoriesFromCode(reportType);
				}
			}

#if DEBUG
			var newReportCategory = new ComplianceReportsSetupCategory("A05", "Balance Sheet5", 3, false);
			if (reportType == "TT0")
			{
				reportCategory_List.Add(newReportCategory);
			}
#endif

			ZString categories = reportCategory_List.CodesAsString.Replace(" " + AccountingMasterFilesConstants.DefaultReportCategory.Undefined + ",", "").Replace(AccountingMasterFilesConstants.DefaultReportCategory.Undefined, "").Trim(',', ' ');

			ZStringBuilder builder = new ZStringBuilder();
			foreach (ComplianceReportsSetupCategory element in
				reportCategory_List.Cast<ComplianceReportsSetupCategory>().Where(element => element.Category != AccountingMasterFilesConstants.DefaultReportCategory.Undefined))
			{
				builder.Append(element.Category);
			}

			ZString result = ZString.Empty;
			if (!categories.IsEmpty)
			{
				result = categories + ", " + builder.ToStringWithDelimiterBetweenAppends("_YED, ") + "_YED";
			}
			return result;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*GetCategoriesForReport\s*\(\s*\""*([^\""]+)\""*\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
