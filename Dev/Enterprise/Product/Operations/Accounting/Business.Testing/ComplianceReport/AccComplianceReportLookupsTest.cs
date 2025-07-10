using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Environment;
	using Enterprise.ZArchitecture.Core;

	internal class AccComplianceReportLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReportTypeList()
		{
			Assert("No Report Configurations", !AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Any());
			AssertEquals("No ReportTypes", 0, Lookups.ReportTypeList.Count);

			AddReportConfiguration(Report.ACR_ReportType);
			AssertEquals("One ReportType", 1, Lookups.ReportTypeList.Count);
			AssertEquals(Report.ACR_ReportType, Lookups.ReportTypeList[0].Code);

			AddReportConfiguration("$$$");
			AssertEquals("Two ReportType", 2, Lookups.ReportTypeList.Count);
			AssertNotNull(Lookups.ReportTypeList.ContainsCode("$$$"));
		}

		public void TestReportTypeListGermany()
		{
			AddReportConfiguration("U11");
			AddReportConfiguration("UVA");
			AddReportConfiguration("ZMD");
			AssertEquals("Number of reports visible for Australian company", 3, Lookups.ReportTypeList.Count);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				AssertEquals("Number of reports visible for German company when all disabled", 0, Lookups.ReportTypeList.Count);

				var reportTypeList = new CodeDescriptionBoolCollection()
				{
						{ "U11", (NoResString)"Umsatzsteuer-Sondervorauszahlung", true },
						{ "UVA", (NoResString)"Umsatzsteuervoranmeldung", true },
						{ "ZMD", (NoResString)"Zusammenfassende Meldung", false }
				};
				AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reportTypeList);
				AssertEquals("Number of reports visible for German company when some enabled", 2, Lookups.ReportTypeList.Count);
				AssertNotNull(Lookups.ReportTypeList.ContainsCode("U11"));
				AssertNotNull(Lookups.ReportTypeList.ContainsCode("UVA"));
			}
		}

		public void TestPeriodicityListComplianceFYCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AddReportConfiguration(Report.ACR_ReportType);
				AssertNotNull(Lookups.PeriodicityList);
				var expected = new List<string>
				{
					ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod,
					ComplianceReportConfigurationLookups.ReportPeriodicityCodes.CalendarMonth,
					ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange,
					ComplianceReportConfigurationLookups.ReportPeriodicityCodes.FinancialYear,
					ComplianceReportConfigurationLookups.ReportPeriodicityCodes.ComplianceFinancialYear,
					ComplianceReportConfigurationLookups.ReportPeriodicityCodes.RangeAccountingPeriod,
					ComplianceReportConfigurationLookups.ReportPeriodicityCodes.MonthlyQuarterlyYearly
				};

				AssertContainsExactElementsInAnyOrder(expected, Lookups.PeriodicityList.Cast<CodeDescriptionPair>().Select(a => a.Code));
			}
		}

		public void TestPeriodicityListNonComplianceFYCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				AddReportConfiguration(Report.ACR_ReportType);
				AssertNotNull(Lookups.PeriodicityList);
				var expected = new List<string>
				{
					ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod,
					ComplianceReportConfigurationLookups.ReportPeriodicityCodes.CalendarMonth,
					ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange,
					ComplianceReportConfigurationLookups.ReportPeriodicityCodes.FinancialYear,
					ComplianceReportConfigurationLookups.ReportPeriodicityCodes.RangeAccountingPeriod,
					ComplianceReportConfigurationLookups.ReportPeriodicityCodes.MonthlyQuarterlyYearly
				};

				AssertContainsExactElementsInAnyOrder(expected, Lookups.PeriodicityList.Cast<CodeDescriptionPair>().Select(a => a.Code));
			}
		}

		public void TestGetReportStatusList()
		{
			var reportStatusList = AccComplianceReportLookups.GetReportStatusList();
			AssertNotNull(reportStatusList);
			AssertEquals(8, reportStatusList.Count);
			Assert(reportStatusList.ContainsCode(AccComplianceReport.Status.ReportCreated));
			Assert(reportStatusList.ContainsCode(AccComplianceReport.Status.ReportPendingQueueing));
			Assert(reportStatusList.ContainsCode(AccComplianceReport.Status.ReportDataQueued));
			Assert(reportStatusList.ContainsCode(AccComplianceReport.Status.ReportGenerated));
			Assert(reportStatusList.ContainsCode(AccComplianceReport.Status.ReportOutputGenerated));
			Assert(reportStatusList.ContainsCode(AccComplianceReport.Status.ReportInvalidated));
			Assert(reportStatusList.ContainsCode(AccComplianceReport.Status.ReportFinalised));
			Assert(reportStatusList.ContainsCode(AccComplianceReport.Status.ReportError));
		}

		#region Implementation

		void AddReportConfiguration(string reportType)
		{
			var countryCode = Env.CurrentCompany.Country.Code;
			var taxRegistrationCodes = new OrgCodeLists().CustomsCodes_List(RefCountry.LoadFromCountryCode(Factory, countryCode));
			var taxRegistration = taxRegistrationCodes.ToArray().First(x => !string.IsNullOrEmpty(x.Code));

			var configs = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var config = configs.AddNew();
			config.Country = countryCode;
			config.ReportCode = reportType;
			config.ReportBaseTablePrefix = "AL";
			config.ReportLineGrouping = "";
			config.ReportPeriodicity = "RNG";
			config.TaxRegistrationType = taxRegistration.Code;

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configs);
		}

		AccComplianceReport Report;

		AccComplianceReportLookups Lookups
		{
			get { return Report.Lookups; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			Report = Factory.NewWithValidTestData<AccComplianceReport>();
		}

		#endregion
	}
}

