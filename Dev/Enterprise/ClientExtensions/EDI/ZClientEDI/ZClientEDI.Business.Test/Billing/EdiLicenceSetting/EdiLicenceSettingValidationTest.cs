using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class EdiLicenceSettingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidTo_MustBeAfterValidFrom()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA", "AAA", "AAA", false);
			var setting = Factory.New<BuyingGroupLicenceSetting>();
			setting.LS9_LD = lic.LA_LD;
			setting.LS9_Name = "BUYBIG";
			setting.LS9_ValidFrom = new ZDateTime(2019, 3, 1);
			setting.LS9_ValidTo = new ZDateTime(2018, 4, 30);
			AssertHasErrors(setting.LS9_ValidToInfo);

			setting.LS9_ValidTo = new ZDateTime(2019, 4, 30);
			AssertNoNotifications(setting.LS9_ValidToInfo);
		}

		public void TestValidFrom()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA", "AAA", "AAA", false);
			var setting = Factory.New<BuyingGroupLicenceSetting>();
			setting.LS9_LD = lic.LA_LD;
			setting.LS9_Name = "BUYBIG";
			setting.Validation.ValidateLS9_ValidFrom();
			AssertHasErrors(setting.LS9_ValidFromInfo);

			Factory.Save();
			setting.Validation.ValidateLS9_ValidFrom();
			AssertNoErrors(setting.LS9_ValidFromInfo);
			AssertHasWarnings(setting.LS9_ValidFromInfo);
		}

		public void TestValidFromTo_FirstOrLastDayOfMonth()
		{
			var now = ZDateTime.Now;

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA", "AAA", "AAA", false);
			var setting = Factory.New<BuyingGroupLicenceSetting>();
			setting.LS9_LD = lic.LA_LD;
			setting.LS9_Name = "BUYBIG";
			setting.LS9_ValidFrom = new ZDateTime(now.Year, now.Month, 1);
			setting.LS9_ValidTo = new ZDateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
			setting.RunPreSaveValidation();
			AssertNoErrors(setting.LS9_ValidFromInfo);
			AssertNoErrors(setting.LS9_ValidToInfo);

			setting.LS9_ValidFrom = new ZDateTime(now.Year, now.Month, 2);
			setting.LS9_ValidTo = new ZDateTime(now.Year, now.Month, 10);
			setting.RunPreSaveValidation();
			AssertHasError(setting.LS9_ValidFromInfo, "Date must be the first day of the month.");
			AssertHasError(setting.LS9_ValidToInfo, "Date must be the last day of the month.");

			Factory.Save();
			setting.RunPreSaveValidation();
			AssertNoErrors(setting.LS9_ValidFromInfo);
			AssertNoErrors(setting.LS9_ValidToInfo);

			setting.LS9_ValidFrom = new ZDateTime(now.Year, now.Month, 3);
			setting.LS9_ValidTo = new ZDateTime(now.Year, now.Month, 8);
			setting.RunPreSaveValidation();
			AssertHasError(setting.LS9_ValidFromInfo, "Date must be the first day of the month.");
			AssertHasError(setting.LS9_ValidToInfo, "Date must be the last day of the month.");
		}

		public void TestCheckLS9_Type_PRI_PRT()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA", "AAA", "AAA", false);
			var setting = Factory.New<PriceLicenceSetting>();
			setting.LS9_LD = lic.LA_LD;
			setting.PriceKey = new UsageCodeKey("STL", "CTR");
			setting.LS9_ValidFrom = new ZDateTime(2023, 1, 1);
			setting.LS9_ValidTo = new ZDateTime(2023, 1, 31);

			var setting2 = Factory.New<PriceTierLicenceSetting>();
			setting2.LS9_LD = lic.LA_LD;
			setting2.PriceKey = new UsageCodeKey("STL", "CTR");
			setting2.LS9_ValidFrom = new ZDateTime(2023, 1, 1);
			setting2.LS9_ValidTo = new ZDateTime(2023, 1, 31);

			setting2.RunPreSaveValidation();
			AssertHasError(setting2.LS9_TypeInfo, "Another setting with overlapping dates exists. There can only be one setting valid at a time.");

			setting2.LS9_ValidFrom = new ZDateTime(2024, 1, 1);
			setting2.LS9_ValidTo = new ZDateTime(2024, 1, 31);
			setting2.RunPreSaveValidation();
			AssertNoErrors(setting2.LS9_TypeInfo);
		}
	}
}
