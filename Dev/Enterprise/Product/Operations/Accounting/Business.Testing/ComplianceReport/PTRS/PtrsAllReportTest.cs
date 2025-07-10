using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS
{
	[TestedType(typeof(PtrsAllPaymentsReport))]
	public class PtrsAllReportTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPopulatingReportCompanyData()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var orgProxy = Factory.Load<OrgHeader>(company.GC_OH_OrgProxy);
			var orgABN = orgProxy.CustomsCodes.AddNew();
			orgABN.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			orgABN.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			orgABN.OK_CustomsRegNo = "22-255-588-899-972";
			Factory.Save();

			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();

			var ptrsReport = Factory.New<PtrsAllPaymentsReport>();
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			AssertEquals("ATR_CompanyName", "EDI CUSTOMS BROKERS", ptrsReport.ATR_CompanyName);
			AssertEquals("ATR_VATRegNo", "22255588899972", ptrsReport.ATR_VATRegNo);
			AssertEquals("ATR_Address1", "184 Bourke Road", ptrsReport.ATR_Address1);
			AssertEquals("ATR_Address2", "", ptrsReport.ATR_Address2);
			AssertEquals("ATR_City", "Alexandria", ptrsReport.ATR_City);
			AssertEquals("ATR_State", "NSW", ptrsReport.ATR_State);
			AssertEquals("ATR_PostCode", "2015", ptrsReport.ATR_PostCode);
			AssertEquals("ATR_RN_NKCountryCode", "AU", ptrsReport.ATR_RN_NKCountryCode);

			company.GC_Address1 = "74 O'Riordan Street";
			company.GC_Address2 = "Main Reception";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			ptrsReport = newFactory.Load<PtrsAllPaymentsReport>(ptrsReport.PK);
			AssertEquals("ATR_Address1", "74 O'Riordan Street", ptrsReport.ATR_Address1);
			AssertEquals("ATR_Address2", "Main Reception", ptrsReport.ATR_Address2);
		}

		public void TestPopulatingAllPaymentsData()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			AddDataToComplianceReport(complianceReport, ZDateTime.Now, Creator);

			var ptrsReport = Factory.New<PtrsAllPaymentsReport>();
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			AssertEquals("AllInvoicesPaid", 5032m, ptrsReport.AllInvoicesPaid);
			AssertEquals("AllInvoicesPaidWithOverride", 5032m, ptrsReport.AllInvoicesPaidWithOverride);
			Assert("ReasonToOverrideAllInvoicesPaidInfo.ReadOnly", ptrsReport.ReasonToOverrideAllInvoicesPaidInfo.ReadOnly);
			AssertNoErrors(ptrsReport.ReasonToOverrideAllInvoicesPaidInfo);

			ptrsReport.AllInvoicesPaidWithOverride = 5555m;
			Assert("ReasonToOverrideAllInvoicesPaidInfo.ReadOnly", !ptrsReport.ReasonToOverrideAllInvoicesPaidInfo.ReadOnly);
			AssertHasError(ptrsReport.ReasonToOverrideAllInvoicesPaidInfo, "Reason must be entered when values calculated by compliance report are overridden.");

			AssertEquals("AllInvoicesPaid", 5032m, ptrsReport.AllInvoicesPaid);
			AssertEquals("AllInvoicesPaidWithOverride", 5555m, ptrsReport.AllInvoicesPaidWithOverride);

			ptrsReport.ReasonToOverrideAllInvoicesPaid = "Override!";
			AssertNoErrors(ptrsReport.ReasonToOverrideAllInvoicesPaidInfo);
			AssertEquals("ReasonToOverrideAllInvoicesPaid", "Override!", ptrsReport.ReasonToOverrideAllInvoicesPaid);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			ptrsReport = newFactory.Load<PtrsAllPaymentsReport>(ptrsReport.PK);
			AssertEquals("AllInvoicesPaid", 5032m, ptrsReport.AllInvoicesPaid);
			AssertEquals("AllInvoicesPaidWithOverride", 5555m, ptrsReport.AllInvoicesPaidWithOverride);
			AssertEquals("ReasonToOverrideAllInvoicesPaid", "Override!", ptrsReport.ReasonToOverrideAllInvoicesPaid);

			ptrsReport.AllInvoicesPaidWithOverride = ptrsReport.AllInvoicesPaid;
			AssertNoErrors(ptrsReport.ReasonToOverrideAllInvoicesPaidInfo);
			Assert("ReasonToOverrideAllInvoicesPaidInfo.ReadOnly", !ptrsReport.ReasonToOverrideAllInvoicesPaidInfo.ReadOnly);

			ptrsReport.ReasonToOverrideAllInvoicesPaid = ZString.Empty;
			AssertNoErrors(ptrsReport.ReasonToOverrideAllInvoicesPaidInfo);
			Assert("ReasonToOverrideAllInvoicesPaidInfo.ReadOnly", ptrsReport.ReasonToOverrideAllInvoicesPaidInfo.ReadOnly);
		}

		public void TestAllPaymentsReportCreatesAccTaxReturnColumnsOnPopulatingDataFromComplianceReport()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			AddDataToComplianceReport(complianceReport, ZDateTime.Now, Creator);

			var ptrsReport = Factory.New<PtrsAllPaymentsReport>();
			AssertEquals("No columns", 0, ptrsReport.Columns.Count);
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			AssertEquals("New column", 1, ptrsReport.Columns.Count);

			AssertEquals("AllInvoicesPaid", 5032m, ptrsReport.AllInvoicesPaid);
			AssertEquals("AllInvoicesPaidWithOverride", 5032m, ptrsReport.AllInvoicesPaidWithOverride);

			ptrsReport.AllInvoicesPaidWithOverride = 5555m;
			AssertEquals("Still one column", 1, ptrsReport.Columns.Count);
		}

		public void TestAllPaymentsReportUsesSameAccTaxReturnColumnForReasonToOverride()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			AddDataToComplianceReport(complianceReport, ZDateTime.Now, Creator);

			var ptrsReport = Factory.New<PtrsAllPaymentsReport>();
			AssertEquals("No columns", 0, ptrsReport.Columns.Count);
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			AssertEquals("New column", 1, ptrsReport.Columns.Count);

			AssertEquals("ReasonToOverrideAllInvoicesPaid", ZString.Empty, ptrsReport.ReasonToOverrideAllInvoicesPaid);

			ptrsReport.ReasonToOverrideAllInvoicesPaid = "Override!";
			AssertEquals("Still one column", 1, ptrsReport.Columns.Count);
		}

		#region Implementation

		internal static IEnumerable<AccTransactionHeader> AddDataToComplianceReport(AccComplianceReport report, ZDateTime date, TestObjectCreator creator, params AccTransactionHeader[] extraInvoices)
		{
			SetupComplianceReport(report, date, creator);

			var invoices = new List<AccTransactionHeader>(extraInvoices);

			AddPaidInvoice(creator.Creditor1,"IA001", -1010m, 121);

			AddPaidInvoice(creator.Creditor2, "IA001", -2020m, 10);
			AddPaidInvoice(creator.Creditor1, "IA002", -2002m, 20);

			AddPaidInvoice(creator.Creditor2, "IA003", -3030m, 0);

			creator.Factory.Save();

			creator.CreateComplianceReportQueueEntry(report, invoices.ToArray(), (x) => x.AH_FullyPaidDate.Date, (x) => $"{x.AH_OSTotal}" );
			report.GenerateFromQueue();

			report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", extraInvoices.Length + 3, report.ReportLines.Count);

			return invoices;

			APInvoice AddPaidInvoice(OrgHeader creditor, string number, decimal amount, int days)
			{
				var result = creator.CreateAPInvoice<APInvoice>(number, creator.AUD, 1m, amount, 0m, 0m, amount, 0m, 0m, creditor);
				result.AH_PostDate = result.AH_InvoiceDate = date.AddDays(-days);
				invoices.Add(result);
				Assert(number + " has lines", result.Lines.Any());
				creator.CreateAndMatchAPPaymentForAPInvoice(result, result.AH_InvoiceDate.AddDays(1));
				return result;
			}
		}

		static void SetupComplianceReport(AccComplianceReport report, ZDateTime date, TestObjectCreator creator)
		{
			creator.CreateTestPeriods(date.AddMonths(-6));
			var periodCalculator = new AccountingPeriodCalculator(creator.Factory);
			periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);

			report.ACR_ReportType = "PTA";
			report.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange;
			report.ACR_DateFrom = date.AddMonths(-6).Date;
			report.ACR_DateTo = date.Date;

			creator.CreateConfigurationForComplianceReport(report, "AH", "PTA");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var complianceReport = factory.NewWithValidTestData<AccComplianceReport>();
			var creator = new TestObjectCreator(Factory);
			SetupComplianceReport(complianceReport, ZDateTime.Now, creator);

			var ptrsReport = (PtrsAllPaymentsReport)base.GetNewBusinessObjectForDeleteTest(factory);
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			return ptrsReport;
		}

		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		#endregion
	}
}
