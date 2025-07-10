using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocAccComplianceReport))]
	sealed class DocAccComplianceReportTest : DocumentWrapperTestCase
	{
		public void TestProperties()
		{
			var wrapper = (DocAccComplianceReport)GetDocumentWrappers()[0];
			AssertEquals("ReportType", Report.ACR_ReportType, wrapper.ReportType);
			AssertEquals("Description", Report.ACR_Description, wrapper.Description);
			AssertEquals("Periodicity", Report.ACR_Periodicity, wrapper.Periodicity);
			AssertEquals("DateFrom", Report.ACR_DateFrom, wrapper.DateFrom);
			AssertEquals("DateTo", Report.ACR_DateTo, wrapper.DateTo);
			AssertEquals("UniqueReportID", Report.UniqueReportID, wrapper.UniqueReportID);
			AssertEquals("PageNumberFrom", Report.ACR_PageNumberFrom, wrapper.PageNumberFrom);
			AssertEquals("PageNumberTo", Report.ACR_PageNumberTo, wrapper.PageNumberTo);
			AssertEquals("ReportBaseTablePrefix", Report.ReportBaseTablePrefix, wrapper.ReportBaseTablePrefix);
			AssertEquals("ReportLineGrouping", Report.ReportLineGrouping, wrapper.ReportLineGrouping);
			AssertEquals("GLOpeningBalanceDR", Report.GLOpeningBalanceDR, wrapper.GLOpeningBalanceDR);
			AssertEquals("GLOpeningBalanceCR", Report.GLOpeningBalanceCR, wrapper.GLOpeningBalanceCR);
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			var period = periodCalculator.GetPeriodFromDate(Report.ACR_DateFrom, GlbCompany.CurrentCompany.PK);
			AssertEquals("AccountingPeriodYear", period.ToString().Substring(0, 4), wrapper.AccountingPeriodYear);
			AssertEquals("MaxPageNumber", 10, wrapper.MaxPageNumber);
			AssertEquals("OrgITIVARegNo", "787898", wrapper.OrgITIVARegNo);
			AssertEquals("OrgITCODRegNo", "565656", wrapper.OrgITCODRegNo);
			AssertEquals("CompanyAddress", GlbCompany.CurrentCompany.GC_Address1 + " " + GlbCompany.CurrentCompany.GC_Address2, wrapper.CompanyAddress);

			AssertEquals("APVatSummaryPageNumberFrom", Report.APVatSummaryPageNumberFrom, wrapper.APVatSummaryPageNumberFrom);
			AssertEquals("APVatSummaryPageNumberTo", Report.APVatSummaryPageNumberTo, wrapper.APVatSummaryPageNumberTo);
			AssertEquals("ARVatSummaryPageNumberFrom", Report.ARVatSummaryPageNumberFrom, wrapper.ARVatSummaryPageNumberFrom);
			AssertEquals("ARVatSummaryPageNumberTo", Report.ARVatSummaryPageNumberTo, wrapper.ARVatSummaryPageNumberTo);
			AssertEquals("LiquidazioneIvaPageNumberFrom", Report.LiquidazioneIvaPageNumberFrom, wrapper.LiquidazioneIvaPageNumberFrom);
			AssertEquals("LiquidazioneIvaPageNumberTo", Report.LiquidazioneIvaPageNumberTo, wrapper.LiquidazioneIvaPageNumberTo);
		}

		public void TestMaxPageNumber()
		{
			var helper = new AccountingPeriodTestHelper();
			helper.PostPeriodsForEntireYear(2018, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.FinancialYear);

			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			var firstPeriod = periodCalculator.GetFirstPeriodForYear(2018);
			var firstDay = periodCalculator.GetLastDayForPeriod(firstPeriod);
			Assert("Must test day within 2017 while accounting year is set to 2018", firstDay.Year == 2017);

			var wrapper = (DocAccComplianceReport)GetDocumentWrappers()[0];
			Report.ACR_DateFrom = firstDay.Date;
			Report.ACR_DateTo = Report.ACR_DateFrom.AddDays(1);
			AssertNoExceptionThrown("Max Number check should not throw error in .Max() method", () => { var testNum = wrapper.MaxPageNumber; });
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocAccComplianceReport.New(Report, Factory) };
		}

		AccComplianceReport Report;
		AccComplianceReport Report2;

		protected override void SetUp()
		{
			var creator = new TestObjectCreator(Factory);
			Report = Factory.NewWithValidTestData<AccComplianceReport>();
			Report.ACR_PageNumberFrom = 1;
			Report.ACR_PageNumberTo = 5;

			Report.APVatSummaryPageNumberFrom = 1;
			Report.APVatSummaryPageNumberTo = 11;
			Report.ARVatSummaryPageNumberFrom = 2;
			Report.ARVatSummaryPageNumberTo = 33;
			Report.LiquidazioneIvaPageNumberFrom = 4;
			Report.LiquidazioneIvaPageNumberTo = 44;

			Report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			Report2.ACR_ReportType = Report.ACR_ReportType;
			Report2.ACR_PageNumberFrom = 6;
			Report2.ACR_PageNumberTo = 10;
			creator.CreateTestPeriods(Report.ACR_DateFrom.AddDays(-Report.ACR_DateFrom.Day + 1));
			var invoice = creator.CreateAPInvoice<APInvoice>("I0001", creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line = invoice.Lines[0];
			line.AL_AT = creator.GST1.PK;

			var ivaCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			ivaCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
			ivaCode.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			ivaCode.OK_CustomsRegNo = "787898";

			var codCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			codCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
			codCode.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
			codCode.OK_CustomsRegNo = "565656";

			Factory.Save();

			creator.CreateComplianceReportTransactionPivot(Report, line);
			creator.CreateConfigurationForComplianceReport(Report);

			base.SetUp();
		}
	}
}
