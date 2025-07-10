using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientLicenceFeeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckL8_Type()
		{
			ClientLicenceFee licenceFee = Factory.New<ClientLicenceFee>();
			licenceFee.Validation.ValidateAll();
			AssertHasErrors(licenceFee.L8_TypeInfo);

			licenceFee.L8_Type = "XXX";
			AssertHasErrors(licenceFee.L8_TypeInfo);

			licenceFee.L8_Type = "ESV";
			AssertNoErrors(licenceFee.L8_TypeInfo);
		}

		public void TestCheckL8_Description()
		{
			ClientLicenceFee licenceFee = Factory.New<ClientLicenceFee>();
			licenceFee.Validation.ValidateAll();
			AssertHasErrors(licenceFee.L8_DescriptionInfo);

			licenceFee.L8_Description = "XXX";
			AssertNoErrors(licenceFee.L8_DescriptionInfo);

			licenceFee.L8_Description = "";
			AssertHasErrors(licenceFee.L8_DescriptionInfo);
		}

		public void TestCheckL8_Amount()
		{
			ClientLicenceFee licenceFee = Factory.New<ClientLicenceFee>();
			licenceFee.Validation.ValidateAll();
			AssertHasErrors(licenceFee.L8_AmountInfo);

			licenceFee.L8_Amount = 10.24;
			AssertNoErrors(licenceFee.L8_AmountInfo);

			licenceFee.L8_Amount = 0;
			AssertHasErrors(licenceFee.L8_AmountInfo);

			licenceFee.L8_Amount = -10.24;
			AssertNoErrors(licenceFee.L8_AmountInfo);

			licenceFee.L8_Amount = 13;
			AssertNoErrors(licenceFee.L8_AmountInfo);
		}

		public void TestCheckL8_ChargeCode()
		{
			AccChargeCode good1 = BillingTestHelper.CreateChargeCode(Factory, null, "GOOD1");
			good1.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			good1.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;

			AccChargeCode bad1 = BillingTestHelper.CreateChargeCode(Factory, null, "BAD1");
			bad1.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			bad1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;

			AccChargeCode good2 = BillingTestHelper.CreateChargeCode(Factory, null, "GOOD2");
			good2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			good2.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;

			AccChargeCode bad2 = BillingTestHelper.CreateChargeCode(Factory, null, "BAD2");
			bad2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			bad2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;

			Factory.Save();

			ClientLicenceFee licenceFee = Factory.New<ClientLicenceFee>();

			licenceFee.L8_ChargeCode = "XXX";
			AssertHasErrors(licenceFee.L8_ChargeCodeInfo);

			licenceFee.L8_ChargeCode = "";
			AssertHasErrors(licenceFee.L8_ChargeCodeInfo);

			licenceFee.L8_ChargeCode = "BAD1";
			AssertHasErrors(licenceFee.L8_ChargeCodeInfo);

			licenceFee.L8_ChargeCode = "BAD2";
			AssertHasErrors(licenceFee.L8_ChargeCodeInfo);

			licenceFee.L8_ChargeCode = "GOOD1";
			AssertNoErrors(licenceFee.L8_ChargeCodeInfo);

			licenceFee.L8_ChargeCode = "GOOD2";
			AssertNoErrors(licenceFee.L8_ChargeCodeInfo);
		}

		[TestDate(2019, 5, 1)]
		public void TestCheckL8_StartDate()
		{
			var chargeCode = BillingTestHelper.CreateChargeCode(Factory, null, "GOOD1");
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;

			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "COM");
			Factory.Save();

			var fee = BillingTestHelper.CreateMaintenanceFee(licCompany, "Fee 1", 100m, "GOOD1");
			fee.L8_StartDate = ZDateTime.Empty;
			AssertHasErrors(fee.L8_StartDateInfo);

			fee.L8_StartDate = new ZDateTime(2019, 01, 01);
			AssertNoErrors(fee.L8_StartDateInfo);

			fee.L8_StartDate = new ZDateTime(2019, 01, 01);
			fee.L8_EndDate = new ZDateTime(2020, 01, 01);
			AssertNoErrors(fee.L8_StartDateInfo);

			fee.L8_EndDate = new ZDateTime(2018, 12, 01);
			fee.L8_StartDate = new ZDateTime(2019, 01, 01);
			AssertHasErrors(fee.L8_StartDateInfo);

			fee.L8_EndDate = ZDateTime.Empty;
			fee.Validation.ValidateL8_StartDate();
			AssertNoErrors(fee.L8_StartDateInfo);

			fee.L8_StartDate = ZDateTime.Empty;
			AssertHasErrors(fee.L8_StartDateInfo);

			string expectedErrorMessage = "Fee should start on the first day of the month.";

			fee.L8_StartDate = new ZDateTime(2010, 08, 02);
			AssertHasError(fee.L8_StartDateInfo, expectedErrorMessage);

			fee.L8_StartDate = new ZDateTime(2010, 08, 15);
			AssertHasError(fee.L8_StartDateInfo, expectedErrorMessage);

			fee.L8_StartDate = new ZDateTime(2010, 08, 01);
			AssertNoErrors(fee.L8_StartDateInfo);
		}

		[TestDate(2015, 1, 1)]
		public void TestCheckL8_EndDate()
		{
			ClientLicenceFee licenceFee = Factory.New<ClientLicenceFee>();
			licenceFee.L8_EndDate = new ZDateTime(2012, 01, 31);
			AssertNoErrors(licenceFee.L8_EndDateInfo);

			licenceFee.L8_StartDate = new ZDateTime(2010, 01, 01);
			licenceFee.L8_EndDate = new ZDateTime(2012, 01, 31);
			AssertNoErrors(licenceFee.L8_EndDateInfo);

			licenceFee.L8_StartDate = new ZDateTime(2013, 01, 01);
			licenceFee.L8_EndDate = new ZDateTime(2012, 01, 31);
			AssertHasErrors(licenceFee.L8_EndDateInfo);

			licenceFee = Factory.New<ClientLicenceFee>();
			licenceFee.L8_EndDate = new ZDateTime(2010, 08, 31);
			AssertNoErrors(licenceFee.L8_EndDateInfo);

			licenceFee.L8_EndDate = ZDateTime.Empty;
			AssertNoErrors(licenceFee.L8_EndDateInfo);

			string expectedErrorMessage = "Fee should end on the last day of the month.";

			licenceFee.L8_EndDate = new ZDateTime(2010, 08, 30);
			AssertHasError(licenceFee.L8_EndDateInfo, expectedErrorMessage);

			licenceFee.L8_EndDate = new ZDateTime(2010, 08, 15);
			AssertHasError(licenceFee.L8_EndDateInfo, expectedErrorMessage);

			licenceFee.L8_EndDate = new ZDateTime(2010, 10, 31);
			AssertNoErrors(licenceFee.L8_EndDateInfo);
		}

		public void TestCheckL8_RenewalMonths()
		{
			ClientLicenceFee fee = Factory.New<ClientLicenceFee>();
			fee.Validation.ValidateAll();
			AssertNoErrors(fee.L8_RenewalMonthsInfo);

			fee.L8_RenewalMonths = -1;
			AssertHasErrors(fee.L8_RenewalMonthsInfo);

			fee.L8_RenewalMonths = 0;
			AssertHasErrors(fee.L8_RenewalMonthsInfo);

			fee.L8_RenewalMonths = 1;
			AssertNoErrors(fee.L8_RenewalMonthsInfo);

			fee.L8_RenewalMonths = 12;
			AssertNoErrors(fee.L8_RenewalMonthsInfo);
		}

		public void TestCheckL8_OH_RemitToOrg()
		{
			var fee = Factory.New<ClientLicenceFee>();

			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "DDDABCSYD";
			fee.L8_OH_RemitToOrg = org.PK;
			AssertHasErrors(fee.L8_OH_RemitToOrgInfo);

			org.CreateAndLoadLicenceForOrg();
			fee.Validation.ValidateL8_OH_RemitToOrg();
			AssertNoErrors(fee.L8_OH_RemitToOrgInfo);
		}

		public void TestCheckL8_RX_NKCurrency()
		{
			var fee = Factory.New<ClientLicenceFee>();
			fee.Validation.ValidateAll();
			AssertHasErrors(fee.L8_RX_NKCurrencyInfo);

			fee.L8_RX_NKCurrency = "XXX";
			AssertHasErrors(fee.L8_RX_NKCurrencyInfo);

			fee.L8_RX_NKCurrency = "AUD";
			AssertNoErrors(fee.L8_RX_NKCurrencyInfo);
		}
	}
}