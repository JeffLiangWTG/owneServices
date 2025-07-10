using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport.TPAR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.TPAR
{
	[TestedType(typeof(TparReportCreditorLine))]
	public class TparReportCreditorLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLineStateAndSaving()
		{
			var tparReport = Factory.NewWithValidTestData<TparReport>();

			var line = Factory.NewWithValidTestData<TparReportCreditorLine>();
			line.ARL_ATR_AccTaxReturn = tparReport.PK;
			AssertEquals(TparReportCreditorLine.LineState.Added, line.State);
			line.ARL_TotalAmountIncludingTax = 10m;
			line.ARL_OverriddenTotalAmountIncludingTax = 10m;
			line.ARL_PaymentsBasisWithholdingTaxAmount = 20m;
			line.ARL_OverriddenPaymentsBasisWithholdingTaxAmount = 20m;
			line.ARL_GSTAmount = 30m;
			line.ARL_OverriddenGSTAmount = 30m;
			AssertEquals(TparReportCreditorLine.LineState.Added, line.State);
			Assert(line.HasChanges);
			Factory.Save();
			Assert("We don't save a line if amounts are not overridden and comment is empty", !line.IsInDatabase);
			line.ARL_OverriddenTotalAmountIncludingTax = 15m;
			AssertEquals(TparReportCreditorLine.LineState.Modified, line.State);
			Factory.Save();
			Assert(line.IsInDatabase);
			AssertEquals(TparReportCreditorLine.LineState.Saved, line.State);

			var line2 = Factory.NewWithValidTestData<TparReportCreditorLine>();
			line2.ARL_ATR_AccTaxReturn = tparReport.PK;
			AssertEquals(TparReportCreditorLine.LineState.Added, line2.State);
			line2.ARL_OverriddenPaymentsBasisWithholdingTaxAmount = 25m;
			AssertEquals(TparReportCreditorLine.LineState.Modified, line2.State);
			Factory.Save();
			Assert(line2.IsInDatabase);
			AssertEquals(TparReportCreditorLine.LineState.Saved, line2.State);

			var line3 = Factory.NewWithValidTestData<TparReportCreditorLine>();
			line3.ARL_ATR_AccTaxReturn = tparReport.PK;
			AssertEquals(TparReportCreditorLine.LineState.Added, line3.State);
			line3.ARL_OverriddenGSTAmount = 35m;
			AssertEquals(TparReportCreditorLine.LineState.Modified, line3.State);
			Factory.Save();
			Assert(line3.IsInDatabase);
			AssertEquals(TparReportCreditorLine.LineState.Saved, line3.State);

			var line4 = Factory.NewWithValidTestData<TparReportCreditorLine>();
			line4.ARL_ATR_AccTaxReturn = tparReport.PK;
			AssertEquals(TparReportCreditorLine.LineState.Added, line4.State);
			line4.ARL_Comment = "newComment";
			AssertEquals(TparReportCreditorLine.LineState.Modified, line4.State);
			Factory.Save();
			Assert(line4.IsInDatabase);
			AssertEquals(TparReportCreditorLine.LineState.Saved, line4.State);
			line4.ARL_Comment = "newComment 02";
			AssertEquals(TparReportCreditorLine.LineState.Modified, line4.State);

			tparReport.ATR_Status = TparReport.Status.Generated;
			AssertEquals(TparReportCreditorLine.LineState.Generated, line.State);

			tparReport.ATR_Status = TparReport.Status.Submitted;
			AssertEquals(TparReportCreditorLine.LineState.Submitted, line.State);
		}

		public void TestFieldsReadOnly()
		{
			var tparReport = Factory.NewWithValidTestData<TparReport>();
			var line = Factory.NewWithValidTestData<TparReportCreditorLine>();
			line.ARL_ATR_AccTaxReturn = tparReport.PK;

			tparReport.ATR_Status = ZString.Empty;
			assertReadOnly(true);

			tparReport.ATR_Status = TparReport.Status.Saved;
			Assert(tparReport.IsSaved);
			assertReadOnly(true);

			tparReport.ATR_Status = TparReport.Status.Generated;
			Assert(tparReport.IsGenerated);
			assertReadOnly(true);

			tparReport.ATR_Status = TparReport.Status.Submitted;
			Assert(tparReport.IsSubmitted);
			assertReadOnly(false);

			void assertReadOnly(bool editable)
			{
				AssertEquals(!editable, line.ARL_CommentInfo.ReadOnly);
				AssertEquals(!editable, line.ARL_OverriddenGSTAmountInfo.ReadOnly);
				AssertEquals(!editable, line.ARL_OverriddenPaymentsBasisWithholdingTaxAmountInfo.ReadOnly);
				AssertEquals(!editable, line.ARL_OverriddenTotalAmountIncludingTaxInfo.ReadOnly);

				Assert(line.ARL_Address1Info.ReadOnly);
				Assert(line.ARL_Address2Info.ReadOnly);
				Assert(line.ARL_ATR_AccTaxReturnInfo.ReadOnly);
				Assert(line.ARL_CityInfo.ReadOnly);
				Assert(line.ARL_GSTAmountInfo.ReadOnly);
				Assert(line.ARL_OH_OrganisationInfo.ReadOnly);
				Assert(line.ARL_OrgMergeCounterInfo.ReadOnly);
				Assert(line.ARL_OrgNameInfo.ReadOnly);
				Assert(line.ARL_OrgRegNoInfo.ReadOnly);
				Assert(line.ARL_PaymentsBasisWithholdingTaxAmountInfo.ReadOnly);
				Assert(line.ARL_PostCodeInfo.ReadOnly);
				Assert(line.ARL_RN_NKCountryCodeInfo.ReadOnly);
				Assert(line.ARL_StateInfo.ReadOnly);
				Assert(line.ARL_TotalAmountIncludingTaxInfo.ReadOnly);
			}
		}

		public void TestSetLineDetails()
		{
			var line = SetupTestTparLine();
			line.SetLineDetails();

			AssertEquals("ARL_RN_NKCountryCode", "AU", line.ARL_RN_NKCountryCode);
			AssertEquals("ARL_OrgRegNo", "44477766621", line.ARL_OrgRegNo);
			AssertEquals("ARL_OrgName", "ABI GAS & TOOLS", line.ARL_OrgName);
			AssertEquals("ARL_Address1", "171 ABBOTSFORD ROAD", line.ARL_Address1);
			AssertEquals("ARL_Address2", "Corporate Park", line.ARL_Address2);
			AssertEquals("ARL_City", "Sydney", line.ARL_City);
			AssertEquals("ARL_State", "NSW", line.ARL_State);
			AssertEquals("ARL_PostCode", "2015", line.ARL_PostCode);
		}

		public void TestGetPayeeDataRecord()
		{
			var line = SetupTestTparLine();
			line.SetLineDetails();
			line.ARL_PostCode = "201"; // overriding with invalid 3-digit postcode to ensure that the FixWidth method works

			var result = line.GetPayeeDataRecord();

			AssertContains("44477766621", result);
			AssertContains("ABI GAS & TOOLS".PadRight(200), result);
			AssertContains("171 ABBOTSFORD ROAD".PadRight(38), result);
			AssertContains("Corporate Park".PadRight(38), result);
			AssertContains("Sydney".PadRight(27), result);
			AssertContains("NSW".PadRight(3), result);
			AssertContains("2010", result);
			AssertContains("Australia".PadRight(20), result);
		}

		TparReportCreditorLine SetupTestTparLine()
		{
			var creator = new TestObjectCreator(Factory);
			var org = creator.ABIGAS;
			var orgAbn = org.CustomsCodes.AddNew();
			orgAbn.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			orgAbn.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			orgAbn.OK_CustomsRegNo = "44 477/766\\621";

			org.MainAddress.OA_Address2 = "Corporate Park";
			org.MainAddress.OA_City = "Sydney";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2015";

			var tparReport = Factory.NewWithValidTestData<TparReport>();
			var line = tparReport.Lines.AddNew();

			line.ARL_OH_Organisation = org.PK;

			return line;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}
	}
}
