using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.TPAR;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.TPAR
{
	public class TparReportHelperTest : TestCaseWithFactory
	{
		public void TestFixWidth()
		{
			AssertEquals("ExactLength", TparReportHelper.FixWidth("ExactLength", 11));
			AssertEquals("Short     ", TparReportHelper.FixWidth("Short", 10));
			AssertEquals("Short00000", TparReportHelper.FixWidth("Short", 10, true));
			AssertEquals("LongSt", TparReportHelper.FixWidth("LongString", 6));
			AssertEquals("   ", TparReportHelper.FixWidth(string.Empty, 3));
			AssertEquals("00000", TparReportHelper.FixWidth(string.Empty, 5, true));
		}

		public void TestTruncate()
		{
			AssertEquals("TruncateMe", TparReportHelper.Truncate("TruncateMe", 10));
			AssertEquals("TruncateMe", TparReportHelper.Truncate("TruncateMe", 12));
			AssertEquals("Truncate", TparReportHelper.Truncate("TruncateLongerThan8", 8));

			AssertEquals("SpecialCharacters", TparReportHelper.Truncate("SpeciàlCharàctèrs", 17));
			AssertEquals(17, Encoding.UTF8.GetByteCount(TparReportHelper.Truncate("SpeciàlCharàctèrs", 17)));

			AssertEquals("Specia", TparReportHelper.Truncate("SpeciàlCharàctèrs", 6));
			AssertEquals(6, Encoding.UTF8.GetByteCount(TparReportHelper.Truncate("SpeciàlCharàctèrs", 6)));
			AssertEquals("Special", TparReportHelper.Truncate("SpeciàlCharàctèrs", 7));
			AssertEquals(7, Encoding.UTF8.GetByteCount(TparReportHelper.Truncate("SpeciàlCharàctèrs", 7)));

			AssertEquals("Un0kno0wn0", TparReportHelper.Truncate("UnłknoøwnÆ", 10));
			AssertEquals(10, Encoding.UTF8.GetByteCount(TparReportHelper.Truncate("UnłknoøwnÆ", 10)));
		}

		public void TestGetState()
		{
			AssertEquals("QLD", TparReportHelper.GetState("QLD", Core.Constants.CountryCodes.Australia));
			AssertEquals("NSW", TparReportHelper.GetState("QLD", Core.Constants.CountryCodes.NorfolkIsland));
			AssertEquals("OTH", TparReportHelper.GetState("QLD", Core.Constants.CountryCodes.France));
		}

		public void TestGetPostCode()
		{
			AssertEquals("2005", TparReportHelper.GetPostCode("2005", Core.Constants.CountryCodes.Australia));
			AssertEquals("2005", TparReportHelper.GetPostCode("2005", Core.Constants.CountryCodes.NorfolkIsland));
			AssertEquals("9999", TparReportHelper.GetPostCode("2005", Core.Constants.CountryCodes.France));
		}

		public void TestGetCity()
		{
			AssertEquals("sydney", TparReportHelper.GetCity("sydney", "2005", "NSW", Core.Constants.CountryCodes.Australia));
			AssertEquals("sydney", TparReportHelper.GetCity("sydney", "2005", "NSW", Core.Constants.CountryCodes.NorfolkIsland));
			AssertEquals("Bidart AQU 64210", TparReportHelper.GetCity("Bidart", "64210", "AQU", Core.Constants.CountryCodes.France));
		}

		public void TestFormatDecimal()
		{
			AssertEquals("00000000010", TparReportHelper.FormatDecimal(10m, 11));
			AssertEquals("00000000010", TparReportHelper.FormatDecimal(10.999m, 11));
			AssertEquals("00010", TparReportHelper.FormatDecimal(10m, 5));
		}

		[TestDate(2017, 6, 1)]
		public void TestGetFinancialYear()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodManagementFromDate(complianceReport.ACR_DateTo, complianceReport.ACR_GC_Company);
			complianceReport.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
			complianceReport.AccountingPeriod = currentPeriod.AM_Period;

			AssertEquals("2018", TparReportHelper.GetFinancialYear(complianceReport));

			complianceReport.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.FinancialYear;
			complianceReport.AccountingPeriod = currentPeriod.AM_Year;

			AssertEquals("2018", TparReportHelper.GetFinancialYear(complianceReport));

			complianceReport.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange;
			complianceReport.ACR_DateTo = new ZDate(2019, 6, 30);
			AssertEquals("2019", TparReportHelper.GetFinancialYear(complianceReport));

			complianceReport.ACR_DateTo = new ZDate(2018, 12, 31);
			AssertEquals("2018", TparReportHelper.GetFinancialYear(complianceReport));
		}

		public void TestGetAustralianBusinessNumber()
		{
			var orgAbn = Creator.ABIGAS.CustomsCodes.AddNew();
			orgAbn.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			orgAbn.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			orgAbn.OK_CustomsRegNo = "22 255-588-899/972";

			AssertEquals("22255588899972", TparReportHelper.GetAustralianBusinessNumber(Creator.ABIGAS));
			AssertEquals(string.Empty, TparReportHelper.GetAustralianBusinessNumber(Creator.AALSHI));
			AssertEquals(string.Empty, TparReportHelper.GetAustralianBusinessNumber(null));
		}

		public void TestGetCountryName()
		{
			var refCountry1 = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			var refCountry2 = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "FR"));

			AssertEquals("Australia", TparReportHelper.GetCountryName(refCountry1));
			AssertEquals("France", TparReportHelper.GetCountryName(refCountry2));
		}

		[TestDate(2020, 2, 17, 1, 0, 0)]
		public void TestGetStatementBySupplierProvidedByPayee()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_DateFrom = new ZDate(2020, 1, 1);
			complianceReport.ACR_DateTo = new ZDate(2020, 1, 31);

			var creditor = Creator.CreateOrgHeader("CRD001", creditor: true, false);
			Assert("Pre-condition: no RequiredDocuments", !creditor.RequiredDocuments.Any());
			Assert(!TparReportHelper.GetStatementBySupplierProvidedByPayee(creditor, complianceReport));

			var document = Factory.New<JobRequiredDocument>();
			document.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
			document.EQ_DocType = RefDocTypes.WithholdingTaxExemption;
			document.EQ_RN_NKRelatedCountry = complianceReport.Company.GC_RN_NKCountryCode;
			document.EQ_DateReceived = new ZDateTimeOffset(2019, 11, 1);
			document.EQ_ValidToDate = new ZDate(2019, 12, 31);
			creditor.RequiredDocuments.Add(document);

			Assert("Doc period is before report period", !TparReportHelper.GetStatementBySupplierProvidedByPayee(creditor, complianceReport));
			AssertResult("Doc period end is equal to report period start (doc period start before the report period)", true, new ZDateTimeOffset(2019, 11, 1), new ZDate(2020, 1, 1));
			AssertResult("Doc period end is equal to the report period end (doc period start before the report period)", true, new ZDateTimeOffset(2019, 11, 1), new ZDate(2020, 1, 31));
			AssertResult("Doc period end is after the report period end (doc period start before the report period)", true, new ZDateTimeOffset(2019, 11, 1), new ZDate(2020, 3, 31));
			AssertResult("Doc period start is equal to report period start, doc period end is equal to the report period end", true, new ZDateTimeOffset(2020, 1, 1), new ZDate(2020, 1, 31));
			AssertResult("Doc period start is equal to report period start, doc period end is after the report period end", true, new ZDateTimeOffset(2020, 1, 1), new ZDate(2020, 3, 31));
			AssertResult("Doc period start is after the report period end", false, new ZDateTimeOffset(2020, 2, 1), new ZDate(2020, 3, 31));

			void AssertResult(string testDescription, bool expectedResult, ZDateTimeOffset docReceivedDate, ZDate docValidToDate)
			{
				document.EQ_ValidToDate = docValidToDate;
				document.EQ_DateReceived = docReceivedDate;
				AssertEquals(testDescription, expectedResult, TparReportHelper.GetStatementBySupplierProvidedByPayee(creditor, complianceReport));
			}
		}

		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;
	}
}
