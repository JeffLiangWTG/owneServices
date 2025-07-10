using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(DiscountLicenceSetting))]
	internal class DiscountLicenceSettingTest : EdiLicenceSettingTest
	{
		public void TestIsManualOverrideApplicable()
		{
			var bizo = (DiscountLicenceSetting)Factory.New(GetExpectedBusinessObjectType());
			bizo.FillWithValidTestData();
			var db = Factory.New<LicenceDatabase>();
			db.FillWithValidTestData();
			bizo.LS9_LD = db.PK;
			var link = db.PriceHeaderLinks.AddNew();
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_ValidFrom = ZDateTime.Now.AddYears(-1);
			priceHeader.L6_ValidTo = ZDateTime.Now.AddYears(1);
			link.PHL_ValidFrom = priceHeader.L6_ValidFrom;
			link.PHL_ValidTo = priceHeader.L6_ValidTo;
			var discount = priceHeader.StlDiscounts.AddNew();
			discount.PHD_Type = "PRE";
			discount.PHD_Name = "PREPAY";
			link.PHL_L6 = priceHeader.PK;

			bizo.LS9_Name = "A001";
			AssertEquals(false, bizo.IsManualOverrideApplicable);

			bizo.LS9_Name = "PREPAY";
			AssertEquals(true, bizo.IsManualOverrideApplicable);
			bizo.LS9_IsManualOverride = true;
			AssertEquals(true, bizo.LS9_IsManualOverride);

			bizo.LS9_Name = "A002";
			AssertEquals(false, bizo.IsManualOverrideApplicable);
			AssertEquals(false, bizo.LS9_IsManualOverride);
		}

		public void TestLS9_Percent_ReadOnly()
		{
			var bizo = (DiscountLicenceSetting)Factory.New(GetExpectedBusinessObjectType());
			bizo.FillWithValidTestData();
			var db = Factory.New<LicenceDatabase>();
			db.FillWithValidTestData();
			bizo.LS9_LD = db.PK;
			var link = db.PriceHeaderLinks.AddNew();
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_ValidFrom = ZDateTime.Now.AddYears(-1);
			priceHeader.L6_ValidTo = ZDateTime.Now.AddYears(1);
			link.PHL_ValidFrom = priceHeader.L6_ValidFrom;
			link.PHL_ValidTo = priceHeader.L6_ValidTo;
			var discount = priceHeader.StlDiscounts.AddNew();
			discount.PHD_Type = "PRE";
			discount.PHD_Name = "Domestic";
			link.PHL_L6 = priceHeader.PK;

			bizo.LS9_Percent = 10m;
			bizo.LS9_Name = "PREPAY";
			AssertEquals(false, bizo.LS9_Percent_ReadOnly);
			AssertEquals(10m, bizo.LS9_Percent);

			discount.PHD_Type = "DOM";
			discount.PHD_Name = "DOMESTIC";

			bizo.LS9_Percent = 12m;
			bizo.LS9_Name = "DOMESTIC";
			AssertEquals(true, bizo.LS9_Percent_ReadOnly);
			AssertEquals(0m, bizo.LS9_Percent);

			bizo.LS9_Percent = 15m;
			bizo.LS9_Name = "ABC1";
			AssertEquals(false, bizo.LS9_Percent_ReadOnly);
			AssertEquals(15m, bizo.LS9_Percent);
		}
	}

	internal class DiscountLicenceSettingValidationTest : BusinessObjectValidationTestCase
	{
	}
}
