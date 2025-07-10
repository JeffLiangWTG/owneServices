using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.IO;
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
	[TestedType(typeof(PtrsReport))]
	public class PtrsReportTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPopulatingReportCompanyData()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var orgProxy = Factory.Load<OrgHeader>(company.GC_OH_OrgProxy);
			var orgABN = orgProxy.CustomsCodes.AddNew();
			orgABN.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			orgABN.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			orgABN.OK_CustomsRegNo = "22-255-588-899";
			Factory.Save();

			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			SetupComplianceReport(complianceReport, ZDateTime.Now);

			var ptrsReport = Factory.New<PtrsReport>();
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			AssertEquals("ATR_CompanyName", "EDI CUSTOMS BROKERS", ptrsReport.ATR_CompanyName);
			AssertEquals("ATR_VATRegNo", "22255588899", ptrsReport.ATR_VATRegNo);
			AssertEquals("ATR_Address1", "184 Bourke Road", ptrsReport.ATR_Address1);
			AssertEquals("ATR_Address2", "", ptrsReport.ATR_Address2);
			AssertEquals("ATR_City", "Alexandria", ptrsReport.ATR_City);
			AssertEquals("ATR_State", "NSW", ptrsReport.ATR_State);
			AssertEquals("ATR_PostCode", "2015", ptrsReport.ATR_PostCode);
			AssertEquals("ATR_RN_NKCountryCode", "AU", ptrsReport.ATR_RN_NKCountryCode);
			Assert("ACN is ReadOnly", ptrsReport.C_ValueInfo.ReadOnly);

			orgABN.Delete();
			company.GC_Address1 = "74 O'Riordan Street";
			company.GC_Address2 = "Main Reception";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			ptrsReport = newFactory.Load<PtrsReport>(ptrsReport.PK);

			Assert("ATR_VATRegNo", ptrsReport.ATR_VATRegNo.IsEmpty);
			Assert("ACN is editable", !ptrsReport.C_ValueInfo.ReadOnly);
			AssertEquals("ATR_Address1", "74 O'Riordan Street", ptrsReport.ATR_Address1);
			AssertEquals("ATR_Address2", "Main Reception", ptrsReport.ATR_Address2);
		}

		public void TestPopulatingReportableSmallBusinessAndAllPaymentsData()
		{
			var date = ZDateTime.Now;
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			SetupComplianceReport(complianceReport, date);
			var reportableInvoices = AddDataToComplianceReport(complianceReport, date);

			var allPaymentsReport = Factory.NewWithValidTestData<AccComplianceReport>();
			PtrsAllReportTest.AddDataToComplianceReport(allPaymentsReport, date, Creator, reportableInvoices.ToArray());
			var ptrsAllReport = Factory.New<PtrsAllPaymentsReport>();
			ptrsAllReport.ATR_ACR_ComplianceReport = allPaymentsReport.PK;

			var ptrsReport = Factory.New<PtrsReport>();
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			AssertValuesCalculatedFromReports(ptrsReport);

			AssertEquals("V", 28.6m, ptrsReport.V.Value);
			AssertEquals("W", 23.8m, ptrsReport.W.Value);
			AssertEquals("X", 19m, ptrsReport.X.Value);
			AssertEquals("Y", 14.3m, ptrsReport.Y.Value);
			AssertEquals("Z", 9.5m, ptrsReport.Z.Value);
			AssertEquals("AA", 4.8m, ptrsReport.AA.Value);

			AssertEquals("AB", 32.1m, ptrsReport.AB.Value);
			AssertEquals("AC", 25.5m, ptrsReport.AC.Value);
			AssertEquals("AD", 18.5m, ptrsReport.AD.Value);
			AssertEquals("AE", 13.2m, ptrsReport.AE.Value);
			AssertEquals("AF", 8.6m, ptrsReport.AF.Value);
			AssertEquals("AG", 2.1m, ptrsReport.AG.Value);

			AssertEquals("AM", 0m, ptrsReport.AM.Value);
			AssertEquals("AN", 0m, ptrsReport.AN.Value);

			AssertEquals("AK", 48.6m, ptrsReport.AK.Value);

			#region Set Overrides: Numbers + 1, Amount + 100

			assertUpdatingNumber(() => ptrsReport.NumberInvoicesPaidWithin20DaysWithOverride = 7, () => ptrsReport.ReasonToOverrideInvoicesPaidWithin20DaysInfo,
				(reason) => ptrsReport.ReasonToOverrideInvoicesPaidWithin20Days = reason, 22);
			assertUpdatingNumber(() => ptrsReport.NumberInvoicesPaidBetween21And30DaysWithOverride = 6, () => ptrsReport.ReasonToOverrideInvoicesPaidBetween21And30DaysInfo,
				(reason) => ptrsReport.ReasonToOverrideInvoicesPaidBetween21And30Days = reason, 23);
			assertUpdatingNumber(() => ptrsReport.NumberInvoicesPaidBetween31And60DaysWithOverride = 5, () => ptrsReport.ReasonToOverrideInvoicesPaidBetween31And60DaysInfo,
				(reason) => ptrsReport.ReasonToOverrideInvoicesPaidBetween31And60Days = reason, 24);
			assertUpdatingNumber(() => ptrsReport.NumberInvoicesPaidBetween61And90DaysWithOverride = 4, () => ptrsReport.ReasonToOverrideInvoicesPaidBetween61And90DaysInfo,
				(reason) => ptrsReport.ReasonToOverrideInvoicesPaidBetween61And90Days = reason, 25);
			assertUpdatingNumber(() => ptrsReport.NumberInvoicesPaidBetween91And120DaysWithOverride = 3, () => ptrsReport.ReasonToOverrideInvoicesPaidBetween91And120DaysInfo,
				(reason) => ptrsReport.ReasonToOverrideInvoicesPaidBetween91And120Days = reason, 26);
			assertUpdatingNumber(() => ptrsReport.NumberInvoicesPaidInMoreThan120DaysWithOverride = 2, () => ptrsReport.ReasonToOverrideInvoicesPaidInMoreThan120DaysInfo,
				(reason) => ptrsReport.ReasonToOverrideInvoicesPaidInMoreThan120Days = reason, 27);

			assertUpdatingNumber(() => ptrsReport.NumberInvoicesPaidSupplyChainFinanceArrangements = 1, () => ptrsReport.ReasonForInvoicesPaidSupplyChainFinanceArrangementsInfo,
				(reason) => ptrsReport.ReasonForInvoicesPaidSupplyChainFinanceArrangements = reason, 27);
			AssertEquals("No changes to TotalNumberInvoicesPaidWithOverride", 27, ptrsReport.TotalNumberInvoicesPaidWithOverride);

			void assertUpdatingNumber(Action numberSetter, Func<ZPropertyInfo> infoGetter, Action<string> reasonSetter, ZInt expectedTotalNumber)
			{
				numberSetter();
				var info = infoGetter();
				AssertHasError(info, "Reason must be entered when values calculated by compliance report are overridden.");
				reasonSetter("Reason");
				AssertNoErrors(info);
				AssertEquals("TotalNumberInvoicesPaidWithOverride", expectedTotalNumber, ptrsReport.TotalNumberInvoicesPaidWithOverride);
			}

			assertUpdatingValue(() => ptrsReport.ValueInvoicesPaidWithin20DaysWithOverride = 1630m, () => ptrsReport.ReasonToOverrideInvoicesPaidWithin20DaysInfo,
				(reason) => ptrsReport.ReasonToOverrideInvoicesPaidWithin20Days = reason, 4866m);
			assertUpdatingValue(() => ptrsReport.ValueInvoicesPaidBetween21And30DaysWithOverride = 1315m, () => ptrsReport.ReasonToOverrideInvoicesPaidBetween21And30DaysInfo,
				(reason) => ptrsReport.ReasonToOverrideInvoicesPaidBetween21And30Days = reason, 4966m);
			assertUpdatingValue(() => ptrsReport.ValueInvoicesPaidBetween31And60DaysWithOverride = 981m, () => ptrsReport.ReasonToOverrideInvoicesPaidBetween31And60DaysInfo,
				(reason) => ptrsReport.ReasonToOverrideInvoicesPaidBetween31And60Days = reason, 5066m);
			assertUpdatingValue(() => ptrsReport.ValueInvoicesPaidBetween61And90DaysWithOverride = 727m, () => ptrsReport.ReasonToOverrideInvoicesPaidBetween61And90DaysInfo,
				(reason) => ptrsReport.ReasonToOverrideInvoicesPaidBetween61And90Days = reason, 5166m);
			assertUpdatingValue(() => ptrsReport.ValueInvoicesPaidBetween91And120DaysWithOverride = 512m, () => ptrsReport.ReasonToOverrideInvoicesPaidBetween91And120DaysInfo,
				(reason) => ptrsReport.ReasonToOverrideInvoicesPaidBetween91And120Days = reason, 5266m);
			assertUpdatingValue(() => ptrsReport.ValueInvoicesPaidInMoreThan120DaysWithOverride = 201m, () => ptrsReport.ReasonToOverrideInvoicesPaidInMoreThan120DaysInfo,
				(reason) => ptrsReport.ReasonToOverrideInvoicesPaidInMoreThan120Days = reason, 5366m);

			assertUpdatingValue(() => ptrsReport.ValueInvoicesPaidSupplyChainFinanceArrangements = 100m, () => ptrsReport.ReasonForInvoicesPaidSupplyChainFinanceArrangementsInfo,
				(reason) => ptrsReport.ReasonForInvoicesPaidSupplyChainFinanceArrangements = reason, 5366m);
			AssertEquals("No changes to TotalValueInvoicePaidWithOverride", 5366m, ptrsReport.TotalValueInvoicePaidWithOverride);

			void assertUpdatingValue(Action valueSetter, Func<ZPropertyInfo> infoGetter, Action<string> reasonSetter, ZDecimal expectedTotalValue)
			{
				valueSetter();
				var info = infoGetter();
				AssertNoErrors(info);
				reasonSetter("");
				AssertHasError(info, "Reason must be entered when values calculated by compliance report are overridden.");
				AssertEquals("TotalValueInvoicePaidWithOverride", expectedTotalValue, ptrsReport.TotalValueInvoicePaidWithOverride);
				reasonSetter("Reason");
				AssertNoErrors(info);
			}

			#endregion

			assertPercentColumns();

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			ptrsReport = newFactory.Load<PtrsReport>(ptrsReport.PK);
			AssertEquals(29, ptrsReport.Columns.Count);

			assertPercentColumns();

			void assertPercentColumns()
			{
				AssertEquals("V", 25.9m, ptrsReport.V.Value);
				AssertEquals("W", 22.2m, ptrsReport.W.Value);
				AssertEquals("X", 18.5m, ptrsReport.X.Value);
				AssertEquals("Y", 14.8m, ptrsReport.Y.Value);
				AssertEquals("Z", 11.1m, ptrsReport.Z.Value);
				AssertEquals("AA", 7.4m, ptrsReport.AA.Value);

				AssertEquals("AB", 30.4m, ptrsReport.AB.Value);
				AssertEquals("AC", 24.5m, ptrsReport.AC.Value);
				AssertEquals("AD", 18.3m, ptrsReport.AD.Value);
				AssertEquals("AE", 13.5m, ptrsReport.AE.Value);
				AssertEquals("AF", 9.5m, ptrsReport.AF.Value);
				AssertEquals("AG", 3.7m, ptrsReport.AG.Value);

				AssertEquals("AM", 3.7m, ptrsReport.AM.Value);
				AssertEquals("AN", 1.9m, ptrsReport.AN.Value);

				AssertEquals("AK", 54.8m, ptrsReport.AK.Value);
			}
		}

		public void TestReportableSmallBusinessCreatesAccTaxReturnColumnsOnUpdatingNumbersAndValuesWithOverride()
		{
			var date = ZDateTime.Now;
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			SetupComplianceReport(complianceReport, date);
			var reportableInvoices = AddDataToComplianceReport(complianceReport, date);

			var allPaymentsReport = Factory.NewWithValidTestData<AccComplianceReport>();
			PtrsAllReportTest.AddDataToComplianceReport(allPaymentsReport, date, Creator, reportableInvoices.ToArray());
			var ptrsAllReport = Factory.New<PtrsAllPaymentsReport>();
			ptrsAllReport.ATR_ACR_ComplianceReport = allPaymentsReport.PK;

			var ptrsReport = Factory.New<PtrsReport>();
			AssertEquals(0, ptrsReport.Columns.Count);
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			AssertEquals("Columns created for all calculated percent columns", 15, ptrsReport.Columns.Count);

			AssertValuesCalculatedFromReports(ptrsReport);
			AssertEquals("No new columns added", 15, ptrsReport.Columns.Count);

			#region Updating every Override column creates new AccTaxReturnColumn

			ptrsReport.NumberInvoicesPaidWithin20DaysWithOverride = 7;
			Assert("ReasonToOverrideInvoicesPaidWithin20DaysInfo.ReadOnly", !ptrsReport.ReasonToOverrideInvoicesPaidWithin20DaysInfo.ReadOnly);
			AssertEquals(16, ptrsReport.Columns.Count);
			ptrsReport.NumberInvoicesPaidBetween21And30DaysWithOverride = 6;
			Assert("ReasonToOverrideInvoicesPaidBetween21And30DaysInfo.ReadOnly", !ptrsReport.ReasonToOverrideInvoicesPaidBetween21And30DaysInfo.ReadOnly);
			AssertEquals(17, ptrsReport.Columns.Count);
			ptrsReport.NumberInvoicesPaidBetween31And60DaysWithOverride = 5;
			Assert("ReasonToOverrideInvoicesPaidBetween31And60DaysInfo.ReadOnly", !ptrsReport.ReasonToOverrideInvoicesPaidBetween31And60DaysInfo.ReadOnly);
			AssertEquals(18, ptrsReport.Columns.Count);
			ptrsReport.NumberInvoicesPaidBetween61And90DaysWithOverride = 4;
			Assert("ReasonToOverrideInvoicesPaidBetween61And90DaysInfo.ReadOnly", !ptrsReport.ReasonToOverrideInvoicesPaidBetween61And90DaysInfo.ReadOnly);
			AssertEquals(19, ptrsReport.Columns.Count);
			ptrsReport.NumberInvoicesPaidBetween91And120DaysWithOverride = 3;
			Assert("ReasonToOverrideInvoicesPaidBetween91And120DaysInfo.ReadOnly", !ptrsReport.ReasonToOverrideInvoicesPaidBetween91And120DaysInfo.ReadOnly);
			AssertEquals(20, ptrsReport.Columns.Count);
			ptrsReport.NumberInvoicesPaidInMoreThan120DaysWithOverride = 2;
			Assert("ReasonToOverrideInvoicesPaidInMoreThan120DaysInfo.ReadOnly", !ptrsReport.ReasonToOverrideInvoicesPaidInMoreThan120DaysInfo.ReadOnly);
			AssertEquals(21, ptrsReport.Columns.Count);

			ptrsReport.NumberInvoicesPaidSupplyChainFinanceArrangements = 1;
			Assert("ReasonForInvoicesPaidSupplyChainFinanceArrangementsInfo.ReadOnly", !ptrsReport.ReasonForInvoicesPaidSupplyChainFinanceArrangementsInfo.ReadOnly);
			AssertEquals(22, ptrsReport.Columns.Count);

			ptrsReport.ValueInvoicesPaidWithin20DaysWithOverride = 1630m;
			AssertEquals(23, ptrsReport.Columns.Count);
			ptrsReport.ValueInvoicesPaidBetween21And30DaysWithOverride = 1315m;
			AssertEquals(24, ptrsReport.Columns.Count);
			ptrsReport.ValueInvoicesPaidBetween31And60DaysWithOverride = 981m;
			AssertEquals(25, ptrsReport.Columns.Count);
			ptrsReport.ValueInvoicesPaidBetween61And90DaysWithOverride = 727m;
			AssertEquals(26, ptrsReport.Columns.Count);
			ptrsReport.ValueInvoicesPaidBetween91And120DaysWithOverride = 512m;
			AssertEquals(27, ptrsReport.Columns.Count);
			ptrsReport.ValueInvoicesPaidInMoreThan120DaysWithOverride = 201m;
			AssertEquals(28, ptrsReport.Columns.Count);

			ptrsReport.ValueInvoicesPaidSupplyChainFinanceArrangements = 100m;
			AssertEquals(29, ptrsReport.Columns.Count);

			#endregion

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			ptrsReport = newFactory.Load<PtrsReport>(ptrsReport.PK);
			AssertEquals(29, ptrsReport.Columns.Count);

			AssertEquals(7, ptrsReport.NumberInvoicesPaidWithin20DaysWithOverride);
			AssertEquals(6, ptrsReport.NumberInvoicesPaidBetween21And30DaysWithOverride);
			AssertEquals(5, ptrsReport.NumberInvoicesPaidBetween31And60DaysWithOverride);
			AssertEquals(4, ptrsReport.NumberInvoicesPaidBetween61And90DaysWithOverride);
			AssertEquals(3, ptrsReport.NumberInvoicesPaidBetween91And120DaysWithOverride);
			AssertEquals(2, ptrsReport.NumberInvoicesPaidInMoreThan120DaysWithOverride);
			AssertEquals(1, ptrsReport.NumberInvoicesPaidSupplyChainFinanceArrangements);
			AssertEquals(1630m, ptrsReport.ValueInvoicesPaidWithin20DaysWithOverride);
			AssertEquals(1315m, ptrsReport.ValueInvoicesPaidBetween21And30DaysWithOverride);
			AssertEquals(981m, ptrsReport.ValueInvoicesPaidBetween31And60DaysWithOverride);
			AssertEquals(727m, ptrsReport.ValueInvoicesPaidBetween61And90DaysWithOverride);
			AssertEquals(512m, ptrsReport.ValueInvoicesPaidBetween91And120DaysWithOverride);
			AssertEquals(201m, ptrsReport.ValueInvoicesPaidInMoreThan120DaysWithOverride);
			AssertEquals(100m, ptrsReport.ValueInvoicesPaidSupplyChainFinanceArrangements);
		}

		public void TestReportableSmallBusinessCreatesAccTaxReturnColumnsOnSettingReasonToOverride()
		{
			var date = ZDateTime.Now;
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			SetupComplianceReport(complianceReport, date);
			var reportableInvoices = AddDataToComplianceReport(complianceReport, date);

			var allPaymentsReport = Factory.NewWithValidTestData<AccComplianceReport>();
			PtrsAllReportTest.AddDataToComplianceReport(allPaymentsReport, date, Creator, reportableInvoices.ToArray());
			var ptrsAllReport = Factory.New<PtrsAllPaymentsReport>();
			ptrsAllReport.ATR_ACR_ComplianceReport = allPaymentsReport.PK;

			var ptrsReport = Factory.New<PtrsReport>();
			AssertEquals(0, ptrsReport.Columns.Count);
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			AssertEquals("Columns created for all calculated percent columns", 15, ptrsReport.Columns.Count);

			AssertValuesCalculatedFromReports(ptrsReport);
			AssertEquals("No new columns added", 15, ptrsReport.Columns.Count);

			#region Updating every Reason to Override column creates two AccTaxReturnColumns as it is applied to both Number and Value

			// Setting Override reason creates the Value and Number column, causing increament of 2 columns
			ptrsReport.ReasonToOverrideInvoicesPaidWithin20Days = "Because";
			AssertEquals(17, ptrsReport.Columns.Count);
			ptrsReport.ReasonToOverrideInvoicesPaidBetween21And30Days = "we";
			AssertEquals(19, ptrsReport.Columns.Count);
			ptrsReport.ReasonToOverrideInvoicesPaidBetween31And60Days = "need";
			AssertEquals(21, ptrsReport.Columns.Count);
			ptrsReport.ReasonToOverrideInvoicesPaidBetween61And90Days = "to";
			AssertEquals(23, ptrsReport.Columns.Count);
			ptrsReport.ReasonToOverrideInvoicesPaidBetween91And120Days = "test";
			AssertEquals(25, ptrsReport.Columns.Count);
			ptrsReport.ReasonToOverrideInvoicesPaidInMoreThan120Days = "it";
			AssertEquals(27, ptrsReport.Columns.Count);

			ptrsReport.ReasonForInvoicesPaidSupplyChainFinanceArrangements = "now!";
			AssertEquals(29, ptrsReport.Columns.Count);

			#endregion

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			ptrsReport = newFactory.Load<PtrsReport>(ptrsReport.PK);
			AssertEquals(29, ptrsReport.Columns.Count);

			AssertEquals("Because", ptrsReport.ReasonToOverrideInvoicesPaidWithin20Days);
			AssertEquals("we", ptrsReport.ReasonToOverrideInvoicesPaidBetween21And30Days);
			AssertEquals("need", ptrsReport.ReasonToOverrideInvoicesPaidBetween31And60Days);
			AssertEquals("to", ptrsReport.ReasonToOverrideInvoicesPaidBetween61And90Days);
			AssertEquals("test", ptrsReport.ReasonToOverrideInvoicesPaidBetween91And120Days);
			AssertEquals("it", ptrsReport.ReasonToOverrideInvoicesPaidInMoreThan120Days);
			AssertEquals("now!", ptrsReport.ReasonForInvoicesPaidSupplyChainFinanceArrangements);
		}

		public void TestReportableSmallBusinessCreatesAccTaxReturnColumnsOnSettingManualEntryColumns()
		{
			var date = ZDateTime.Now;
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			SetupComplianceReport(complianceReport, date);
			var reportableInvoices = AddDataToComplianceReport(complianceReport, date);

			var allPaymentsReport = Factory.NewWithValidTestData<AccComplianceReport>();
			PtrsAllReportTest.AddDataToComplianceReport(allPaymentsReport, date, Creator, reportableInvoices.ToArray());
			var ptrsAllReport = Factory.New<PtrsAllPaymentsReport>();
			ptrsAllReport.ATR_ACR_ComplianceReport = allPaymentsReport.PK;

			var ptrsReport = Factory.New<PtrsReport>();
			AssertEquals(0, ptrsReport.Columns.Count);
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			AssertEquals("Columns created for all calculated percent columns", 15, ptrsReport.Columns.Count);

			AssertValuesCalculatedFromReports(ptrsReport);
			AssertEquals("No new columns added", 15, ptrsReport.Columns.Count);

			#region Updating every manual entry column creates new AccTaxReturnColumn

			ptrsReport.D.Value = "ControllingCorporationName";
			AssertEquals(16, ptrsReport.Columns.Count);

			ptrsReport.E.Value = "12345678901";
			AssertEquals(17, ptrsReport.Columns.Count);

			ptrsReport.G.Value = "HeadEntityName";
			AssertEquals(18, ptrsReport.Columns.Count);

			#endregion

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			ptrsReport = newFactory.Load<PtrsReport>(ptrsReport.PK);
			AssertEquals(18, ptrsReport.Columns.Count);

			AssertEquals("ControllingCorporationName", ptrsReport.D.Value);
			AssertEquals("12345678901", ptrsReport.E.Value);
			AssertEquals("HeadEntityName", ptrsReport.G.Value);
		}

		[TestDate(2021, 8, 6, 1, 33, 11)]
		public void TestGenerateReport()
		{
			var date = ZDateTime.Now;
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			SetupComplianceReport(complianceReport, date);
			var reportableInvoices = AddDataToComplianceReport(complianceReport, date);

			var allPaymentsReport = Factory.NewWithValidTestData<AccComplianceReport>();
			PtrsAllReportTest.AddDataToComplianceReport(allPaymentsReport, date, Creator, reportableInvoices.ToArray());
			var ptrsAllReport = Factory.New<PtrsAllPaymentsReport>();
			ptrsAllReport.ATR_ACR_ComplianceReport = allPaymentsReport.PK;

			var ptrsReport = Factory.New<PtrsReport>();
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			AssertValuesCalculatedFromReports(ptrsReport);

			ptrsReport.NumberInvoicesPaidSupplyChainFinanceArrangements = 1;
			ptrsReport.ValueInvoicesPaidSupplyChainFinanceArrangements = 100m;

			AssertEquals("V", 28.6m, ptrsReport.V.Value);
			AssertEquals("W", 23.8m, ptrsReport.W.Value);
			AssertEquals("X", 19m, ptrsReport.X.Value);
			AssertEquals("Y", 14.3m, ptrsReport.Y.Value);
			AssertEquals("Z", 9.5m, ptrsReport.Z.Value);
			AssertEquals("AA", 4.8m, ptrsReport.AA.Value);

			AssertEquals("AB", 32.1m, ptrsReport.AB.Value);
			AssertEquals("AC", 25.5m, ptrsReport.AC.Value);
			AssertEquals("AD", 18.5m, ptrsReport.AD.Value);
			AssertEquals("AE", 13.2m, ptrsReport.AE.Value);
			AssertEquals("AF", 8.6m, ptrsReport.AF.Value);
			AssertEquals("AG", 2.1m, ptrsReport.AG.Value);

			AssertEquals("AM", 4.8m, ptrsReport.AM.Value);
			AssertEquals("AN", 2.1m, ptrsReport.AN.Value);

			AssertEquals("AK", 48.6m, ptrsReport.AK.Value);

			ptrsReport.AL.Value = "Supply,Chain,Finance,Arrangements \"Test\",'Test'";

			Factory.Save();

			AssertEquals(0, ptrsReport.DocManagerInfo.AllEDocs.Count);
			var fileName = ptrsReport.GenerateReport();
			AssertEquals("Filename", "PTRS-EDI 06-Feb-21-06-Aug-21-20210806013311.csv", fileName);
			AssertEquals(1, ptrsReport.DocManagerInfo.AllEDocs.Count);
			var generated = ptrsReport.DocManagerInfo.AllEDocs[0].GetImageDataReader().ConvertToAsciiStringAndCloseStream();

			var expected = string.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS.expected.csv"))
			using (var reader = new StreamReader(stream))
			{
				expected = reader.ReadToEnd();
			}
			AssertEquals("Compare 'normalised' strings", expected.Replace("\r\n", "\n"), generated.Replace("\r\n", "\n"));
		}

		public void TestDependentColumns()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var orgProxy = Factory.Load<OrgHeader>(company.GC_OH_OrgProxy);
			var orgABN = orgProxy.CustomsCodes.AddNew();
			orgABN.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			orgABN.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			orgABN.OK_CustomsRegNo = "22-255-588-899";
			Factory.Save();

			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			SetupComplianceReport(complianceReport, ZDateTime.Now);

			var ptrsReport = Factory.New<PtrsReport>();
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			AssertEquals("ATR_VATRegNo", "22255588899", ptrsReport.ATR_VATRegNo);
			Assert("ACN is ReadOnly", ptrsReport.C_ValueInfo.ReadOnly);

			ptrsReport.ATR_VATRegNo = ZString.Empty;
			Assert("ACN is not ReadOnly", !ptrsReport.C_ValueInfo.ReadOnly);

			#region E and F depending on D

			Assert("D IsEmpty", ptrsReport.D.Value.IsEmpty);
			Assert("E IsEmpty", ptrsReport.E.Value.IsEmpty);
			Assert("F IsEmpty", ptrsReport.F.Value.IsEmpty);
			Assert("E ReadOnly", ptrsReport.E_ValueInfo.ReadOnly);
			Assert("F ReadOnly", ptrsReport.F_ValueInfo.ReadOnly);

			ptrsReport.D_Value = "Controlling Corporation Name";
			Assert("E not ReadOnly", !ptrsReport.E_ValueInfo.ReadOnly);
			Assert("F not ReadOnly", !ptrsReport.F_ValueInfo.ReadOnly);

			ptrsReport.E_Value = "123";
			ptrsReport.F_Value = "345";

			ptrsReport.D_Value = ZString.Empty;
			Assert("E reset to Empty", ptrsReport.E.Value.IsEmpty);
			Assert("F reset to Empty", ptrsReport.F.Value.IsEmpty);
			Assert("E ReadOnly", ptrsReport.E_ValueInfo.ReadOnly);
			Assert("F ReadOnly", ptrsReport.F_ValueInfo.ReadOnly);

			#endregion

			#region H and I depending on G

			Assert("G IsEmpty", ptrsReport.G.Value.IsEmpty);
			Assert("H IsEmpty", ptrsReport.H.Value.IsEmpty);
			Assert("I IsEmpty", ptrsReport.I.Value.IsEmpty);
			Assert("H ReadOnly", ptrsReport.H_ValueInfo.ReadOnly);
			Assert("I ReadOnly", ptrsReport.I_ValueInfo.ReadOnly);

			ptrsReport.G_Value = "Controlling Corporation Name";
			Assert("H not ReadOnly", !ptrsReport.H_ValueInfo.ReadOnly);
			Assert("I not ReadOnly", !ptrsReport.I_ValueInfo.ReadOnly);

			ptrsReport.H_Value = "123";
			ptrsReport.I_Value = "345";

			ptrsReport.G_Value = ZString.Empty;
			Assert("H reset to Empty", ptrsReport.H.Value.IsEmpty);
			Assert("I reset to Empty", ptrsReport.I.Value.IsEmpty);
			Assert("H ReadOnly", ptrsReport.H_ValueInfo.ReadOnly);
			Assert("I ReadOnly", ptrsReport.I_ValueInfo.ReadOnly);

			#endregion
		}

		public void TestOverrideReasonsAlwaysEditable()
		{
			var date = ZDateTime.Now;
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			SetupComplianceReport(complianceReport, date);
			var reportableInvoices = AddDataToComplianceReport(complianceReport, date);

			var allPaymentsReport = Factory.NewWithValidTestData<AccComplianceReport>();
			PtrsAllReportTest.AddDataToComplianceReport(allPaymentsReport, date, Creator, reportableInvoices.ToArray());
			var ptrsAllReport = Factory.New<PtrsAllPaymentsReport>();
			ptrsAllReport.ATR_ACR_ComplianceReport = allPaymentsReport.PK;

			var ptrsReport = Factory.New<PtrsReport>();
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			AssertValuesCalculatedFromReports(ptrsReport);

			ptrsReport.ValueInvoicesPaidWithin20DaysWithOverride = 1630m;
			Assert("ReasonToOverrideInvoicesPaidWithin20DaysInfo.ReadOnly", !ptrsReport.ReasonToOverrideInvoicesPaidWithin20DaysInfo.ReadOnly);
			ptrsReport.ValueInvoicesPaidBetween21And30DaysWithOverride = 1315m;
			Assert("ReasonToOverrideInvoicesPaidBetween21And30DaysInfo.ReadOnly", !ptrsReport.ReasonToOverrideInvoicesPaidBetween21And30DaysInfo.ReadOnly);
			ptrsReport.ValueInvoicesPaidBetween31And60DaysWithOverride = 981m;
			Assert("ReasonToOverrideInvoicesPaidBetween31And60DaysInfo.ReadOnly", !ptrsReport.ReasonToOverrideInvoicesPaidBetween31And60DaysInfo.ReadOnly);
			ptrsReport.ValueInvoicesPaidBetween61And90DaysWithOverride = 727m;
			Assert("ReasonToOverrideInvoicesPaidBetween61And90DaysInfo.ReadOnly", !ptrsReport.ReasonToOverrideInvoicesPaidBetween61And90DaysInfo.ReadOnly);
			ptrsReport.ValueInvoicesPaidBetween91And120DaysWithOverride = 512m;
			Assert("ReasonToOverrideInvoicesPaidBetween91And120DaysInfo.ReadOnly", !ptrsReport.ReasonToOverrideInvoicesPaidBetween91And120DaysInfo.ReadOnly);
			ptrsReport.ValueInvoicesPaidInMoreThan120DaysWithOverride = 201m;
			Assert("ReasonToOverrideInvoicesPaidInMoreThan120DaysInfo.ReadOnly", !ptrsReport.ReasonToOverrideInvoicesPaidInMoreThan120DaysInfo.ReadOnly);

			ptrsReport.ValueInvoicesPaidSupplyChainFinanceArrangements = 100m;
			Assert("ReasonForInvoicesPaidSupplyChainFinanceArrangementsInfo.ReadOnly", !ptrsReport.ReasonForInvoicesPaidSupplyChainFinanceArrangementsInfo.ReadOnly);

			ptrsReport.ReasonToOverrideInvoicesPaidWithin20Days = "Because";
			ptrsReport.ReasonToOverrideInvoicesPaidBetween21And30Days = "we";
			ptrsReport.ReasonToOverrideInvoicesPaidBetween31And60Days = "need";
			ptrsReport.ReasonToOverrideInvoicesPaidBetween61And90Days = "to";
			ptrsReport.ReasonToOverrideInvoicesPaidBetween91And120Days = "test";
			ptrsReport.ReasonToOverrideInvoicesPaidInMoreThan120Days = "it";

			ptrsReport.ReasonForInvoicesPaidSupplyChainFinanceArrangements = "now!";

			ptrsReport.ValueInvoicesPaidWithin20DaysWithOverride = ptrsReport.ValueInvoicesPaidWithin20Days;
			ptrsReport.ValueInvoicesPaidBetween21And30DaysWithOverride = ptrsReport.ValueInvoicesPaidBetween21And30Days;
			ptrsReport.ValueInvoicesPaidBetween31And60DaysWithOverride = ptrsReport.ValueInvoicesPaidBetween31And60Days;
			ptrsReport.ValueInvoicesPaidBetween61And90DaysWithOverride = ptrsReport.ValueInvoicesPaidBetween61And90Days;
			ptrsReport.ValueInvoicesPaidBetween91And120DaysWithOverride = ptrsReport.ValueInvoicesPaidBetween91And120Days;
			ptrsReport.ValueInvoicesPaidInMoreThan120DaysWithOverride = ptrsReport.ValueInvoicesPaidInMoreThan120Days;

			ptrsReport.ValueInvoicesPaidSupplyChainFinanceArrangements = ZDecimal.Zero;

			AssertOverrideReasonsEditable(ptrsReport); // We reset the overridden values but the reasons are not empty;

			// Rewset the reasons
			ptrsReport.ReasonToOverrideInvoicesPaidWithin20Days = ZString.Empty;
			ptrsReport.ReasonToOverrideInvoicesPaidBetween21And30Days = ZString.Empty;
			ptrsReport.ReasonToOverrideInvoicesPaidBetween31And60Days = ZString.Empty;
			ptrsReport.ReasonToOverrideInvoicesPaidBetween61And90Days = ZString.Empty;
			ptrsReport.ReasonToOverrideInvoicesPaidBetween91And120Days = ZString.Empty;
			ptrsReport.ReasonToOverrideInvoicesPaidInMoreThan120Days = ZString.Empty;

			ptrsReport.ReasonForInvoicesPaidSupplyChainFinanceArrangements = ZString.Empty;

			AssertOverrideReasonsEditable(ptrsReport);
		}

		#region Implementation

		void AssertValuesCalculatedFromReports(PtrsReport ptrsReport)
		{
			AssertEquals("TotalNumberInvoicesPaid", 21, ptrsReport.TotalNumberInvoicesPaid);
			AssertEquals("NumberInvoicesPaidWithin20Days", 6, ptrsReport.NumberInvoicesPaidWithin20Days);
			AssertEquals("NumberInvoicesPaidBetween21And30Days", 5, ptrsReport.NumberInvoicesPaidBetween21And30Days);
			AssertEquals("NumberInvoicesPaidBetween31And60Days", 4, ptrsReport.NumberInvoicesPaidBetween31And60Days);
			AssertEquals("NumberInvoicesPaidBetween61And90Days", 3, ptrsReport.NumberInvoicesPaidBetween61And90Days);
			AssertEquals("NumberInvoicesPaidBetween91And120Days", 2, ptrsReport.NumberInvoicesPaidBetween91And120Days);
			AssertEquals("NumberInvoicesPaidInMoreThan120Days", 1, ptrsReport.NumberInvoicesPaidInMoreThan120Days);

			AssertEquals("TotalNumberInvoicesPaidWithOverride", 21, ptrsReport.TotalNumberInvoicesPaidWithOverride);
			AssertEquals("NumberInvoicesPaidWithin20Days", 6, ptrsReport.NumberInvoicesPaidWithin20DaysWithOverride);
			AssertEquals("NumberInvoicesPaidBetween21And30Days", 5, ptrsReport.NumberInvoicesPaidBetween21And30DaysWithOverride);
			AssertEquals("NumberInvoicesPaidBetween31And60Days", 4, ptrsReport.NumberInvoicesPaidBetween31And60DaysWithOverride);
			AssertEquals("NumberInvoicesPaidBetween61And90Days", 3, ptrsReport.NumberInvoicesPaidBetween61And90DaysWithOverride);
			AssertEquals("NumberInvoicesPaidBetween91And120Days", 2, ptrsReport.NumberInvoicesPaidBetween91And120DaysWithOverride);
			AssertEquals("NumberInvoicesPaidInMoreThan120Days", 1, ptrsReport.NumberInvoicesPaidInMoreThan120DaysWithOverride);

			AssertEquals("NumberInvoicesPaidSupplyChainFinanceArrangements", 0, ptrsReport.NumberInvoicesPaidSupplyChainFinanceArrangements);

			AssertEquals("TotalValueInvoicePaid", 4766m, ptrsReport.TotalValueInvoicesPaid);
			AssertEquals("ValueInvoicesPaidWithin20Days", 1530m, ptrsReport.ValueInvoicesPaidWithin20Days);
			AssertEquals("ValueInvoicesPaidBetween21And30Days", 1215m, ptrsReport.ValueInvoicesPaidBetween21And30Days);
			AssertEquals("ValueInvoicesPaidBetween31And60Days", 881m, ptrsReport.ValueInvoicesPaidBetween31And60Days);
			AssertEquals("ValueInvoicesPaidBetween61And90Days", 627m, ptrsReport.ValueInvoicesPaidBetween61And90Days);
			AssertEquals("ValueInvoicesPaidBetween91And120Days", 412m, ptrsReport.ValueInvoicesPaidBetween91And120Days);
			AssertEquals("ValueInvoicesPaidInMoreThan120Days", 101m, ptrsReport.ValueInvoicesPaidInMoreThan120Days);

			AssertEquals("TotalValueInvoicePaidWithOverride", 4766m, ptrsReport.TotalValueInvoicePaidWithOverride);
			AssertEquals("ValueInvoicesPaidWithin20Days", 1530m, ptrsReport.ValueInvoicesPaidWithin20DaysWithOverride);
			AssertEquals("ValueInvoicesPaidBetween21And30Days", 1215m, ptrsReport.ValueInvoicesPaidBetween21And30DaysWithOverride);
			AssertEquals("ValueInvoicesPaidBetween31And60Days", 881m, ptrsReport.ValueInvoicesPaidBetween31And60DaysWithOverride);
			AssertEquals("ValueInvoicesPaidBetween61And90Days", 627m, ptrsReport.ValueInvoicesPaidBetween61And90DaysWithOverride);
			AssertEquals("ValueInvoicesPaidBetween91And120Days", 412m, ptrsReport.ValueInvoicesPaidBetween91And120DaysWithOverride);
			AssertEquals("ValueInvoicesPaidInMoreThan120Days", 101m, ptrsReport.ValueInvoicesPaidInMoreThan120DaysWithOverride);

			AssertEquals("ValueInvoicesPaidSupplyChainFinanceArrangements", 0m, ptrsReport.ValueInvoicesPaidSupplyChainFinanceArrangements);

			AssertEquals("AllPaymentsValue", 9798m, ptrsReport.AllPaymentsValue);

			AssertOverrideReasonsEditable(ptrsReport);
		}

		void AssertOverrideReasonsEditable(PtrsReport ptrsReport)
		{
			AssertEquals("ReasonToOverrideInvoicesPaidWithin20DaysInfo.ReadOnly", false, ptrsReport.ReasonToOverrideInvoicesPaidWithin20DaysInfo.ReadOnly);
			AssertEquals("ReasonToOverrideInvoicesPaidBetween21And30DaysInfo.ReadOnly", false, ptrsReport.ReasonToOverrideInvoicesPaidBetween21And30DaysInfo.ReadOnly);
			AssertEquals("ReasonToOverrideInvoicesPaidBetween31And60DaysInfo.ReadOnly", false, ptrsReport.ReasonToOverrideInvoicesPaidBetween31And60DaysInfo.ReadOnly);
			AssertEquals("ReasonToOverrideInvoicesPaidBetween61And90DaysInfo.ReadOnly", false, ptrsReport.ReasonToOverrideInvoicesPaidBetween61And90DaysInfo.ReadOnly);
			AssertEquals("ReasonToOverrideInvoicesPaidBetween91And120DaysInfo.ReadOnly", false, ptrsReport.ReasonToOverrideInvoicesPaidBetween91And120DaysInfo.ReadOnly);
			AssertEquals("ReasonToOverrideInvoicesPaidInMoreThan120DaysInfo.ReadOnly", false, ptrsReport.ReasonToOverrideInvoicesPaidInMoreThan120DaysInfo.ReadOnly);

			AssertEquals("ReasonForInvoicesPaidSupplyChainFinanceArrangementsInfo.ReadOnly", false, ptrsReport.ReasonForInvoicesPaidSupplyChainFinanceArrangementsInfo.ReadOnly);
		}

		void SetupComplianceReport(AccComplianceReport report, ZDateTime date)
		{
			report.ACR_ReportType = "PTR";
			report.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange;
			report.ACR_DateFrom = date.AddMonths(-6).Date;
			report.ACR_DateTo = date.Date;
			Creator.CreateConfigurationForComplianceReport(report, "AH", "PTR");
		}

		IEnumerable<AccTransactionHeader> AddDataToComplianceReport(AccComplianceReport report, ZDateTime date)
		{
			Creator.CreateTestPeriods(date.AddMonths(-6));
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);

			SetUpCreditor(Creator.ABIGAS);
			SetUpCreditor(Creator.AALSHI);

			var invoices = new List<AccTransactionHeader>();

			AddPaidInvoice(Creator.ABIGAS,"I0001", -101m, 121);

			AddPaidInvoice(Creator.AALSHI, "I0001", -210m, 120);
			AddPaidInvoice(Creator.ABIGAS, "I0002", -202m, 91);

			AddPaidInvoice(Creator.ABIGAS, "I0003", -203m, 90);
			AddPaidInvoice(Creator.ABIGAS, "I0004", -204m, 70);
			AddPaidInvoice(Creator.AALSHI, "I0002", -220m, 61);

			AddPaidInvoice(Creator.ABIGAS, "I0005", -205m, 60);
			AddPaidInvoice(Creator.ABIGAS, "I0006", -206m, 50);
			AddPaidInvoice(Creator.AALSHI, "I0003", -230m, 40);
			AddPaidInvoice(Creator.AALSHI, "I0004", -240m, 31);

			AddPaidInvoice(Creator.ABIGAS, "I0007", -207m, 30);
			AddPaidInvoice(Creator.ABIGAS, "I0008", -208m, 27);
			AddPaidInvoice(Creator.AALSHI, "I0005", -250m, 25);
			AddPaidInvoice(Creator.AALSHI, "I0007", -270m, 23);
			AddPaidInvoice(Creator.AALSHI, "I0008", -280m, 21);

			AddPaidInvoice(Creator.ABIGAS, "I0009", -209m, 20);
			AddPaidInvoice(Creator.ABIGAS, "I0010", -210m, 17);
			AddPaidInvoice(Creator.ABIGAS, "I0011", -211m, 15);
			AddPaidInvoice(Creator.AALSHI, "I0009", -290m, 10);
			AddPaidInvoice(Creator.AALSHI, "I0010", -300m, 5);
			AddPaidInvoice(Creator.AALSHI, "I0011", -310m, 0);

			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(report, invoices.ToArray(), (x) => x.AH_FullyPaidDate.Date, (x) => $"{x.AH_OSTotal}|{(x.AH_FullyPaidDate - x.AH_InvoiceDate).Days}" );
			report.GenerateFromQueue();

			report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 21, report.ReportLines.Count);

			return invoices;

			void SetUpCreditor(OrgHeader creditor)
			{
				creditor.CompanyData.OB_IsCreditor = true;
				var doc = creditor.RequiredDocuments.AddNew("RSB");
				doc.EQ_ValidToDate = date;
			}

			APInvoice AddPaidInvoice(OrgHeader creditor, string numPrefix, decimal amount, int payDays)
			{
				var result = Creator.CreateAPInvoice<APInvoice>(numPrefix + report.ReportLines.Count.ToString(), Creator.AUD, 1m, amount, 0m, 0m, amount, 0m, 0m, creditor);
				result.AH_InvoiceDate = date.AddDays(-payDays);
				invoices.Add(result);
				Assert(numPrefix + " has lines", result.Lines.Any());
				Creator.CreateAndMatchAPPaymentForAPInvoice(result, result.AH_InvoiceDate.AddDays(payDays));
				return result;
			}
		}

		[TestDate(2021, 8, 19)]
		public void TestDatesNotBeforeReportDateToAndBHNotEarlierBE()
		{
			var date = ZDateTime.Now;
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			SetupComplianceReport(complianceReport, date);
			var ptrsReport = Factory.New<PtrsReport>();
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			var expectedError = "The Date must be not earlier than '19/08/2021'.";
			Assert(ptrsReport.BE.Value.IsEmpty);
			AssertNoErrors(ptrsReport.BE_ValueInfo);
			ptrsReport.BE.Value = date.AddDays(-1);
			AssertHasError(ptrsReport.BE_ValueInfo, expectedError);
			ptrsReport.BE.Value = date;
			AssertNoErrors(ptrsReport.BE_ValueInfo);
			ptrsReport.BE.Value = date.AddDays(1);
			AssertNoErrors(ptrsReport.BE_ValueInfo);

			expectedError = "The Date must be not earlier than '20/08/2021'.";
			Assert(ptrsReport.BH.Value.IsEmpty);
			AssertNoErrors(ptrsReport.BH_ValueInfo);
			ptrsReport.BH.Value = date.AddDays(-1);
			AssertHasError(ptrsReport.BH_ValueInfo, expectedError);
			ptrsReport.BH.Value = date;
			AssertHasError(ptrsReport.BH_ValueInfo, expectedError);
			ptrsReport.BH.Value = ptrsReport.BE_Value;
			AssertNoErrors(ptrsReport.BH_ValueInfo);

			expectedError = "The Date must be not earlier than '19/08/2021'.";
			ptrsReport.BE.Value = ZDateTime.Empty;
			ptrsReport.BH.Value = date.AddDays(-1);
			AssertHasError(ptrsReport.BH_ValueInfo, expectedError);
			ptrsReport.BH.Value = date;
			AssertNoErrors(ptrsReport.BH_ValueInfo);

			ptrsReport.BE.Value = date;
			AssertNoErrors(ptrsReport.BE_ValueInfo);
			AssertNoErrors(ptrsReport.BH_ValueInfo);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var complianceReport = factory.NewWithValidTestData<AccComplianceReport>();
			SetupComplianceReport(complianceReport, ZDateTime.Now);

			var ptrsReport = (PtrsReport)base.GetNewBusinessObjectForDeleteTest(factory);
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			return ptrsReport;
		}

		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		#endregion
	}
}
