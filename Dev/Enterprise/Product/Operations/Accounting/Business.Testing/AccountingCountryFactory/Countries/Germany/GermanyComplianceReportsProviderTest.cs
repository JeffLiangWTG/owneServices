using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class GermanyComplianceReportsProviderTest : TestCaseWithFactory
	{
		IComplianceReportsProvider GetComplianceReportsProvider() => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Germany) as IInstanceProvider<IComplianceReportsProvider>).Get();

		public void TestIComplianceReportsProvider()
		{
			var complianceReportsProvider = GetComplianceReportsProvider();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				var result = complianceReportsProvider.GetReportTypeList(GlbCompany.CurrentCompany.PK.ToGuid());
				AssertEquals(1, result.Count);

				var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				var reportConfig = complianceConfig.AddNew();
				reportConfig.ReportCode = AccComplianceReport.ReportTypes.ZMGermany;
				reportConfig.ReportTitle = "Zusammenfassende Meldung";
				reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
				reportConfig.Country = CountryCodes.Germany;
				reportConfig.TaxRegistrationType = "UST";
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

				var reportTypeList = AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				reportTypeList.Cast<CodeDescriptionBool>().Where(v => !v.Bool).ForEach(x => x.Bool = true);
				AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reportTypeList);

				result = complianceReportsProvider.GetReportTypeList(GlbCompany.CurrentCompany.PK.ToGuid());
				AssertEquals(2, result.Count);
			}
		}
	}
}
