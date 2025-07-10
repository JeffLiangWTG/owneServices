using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(PriceLicenceSetting))]
	internal class PriceLicenceSettingTest : EdiLicenceSettingTest
	{
		public void TestUnits()
		{
			var setting = Factory.New<PriceLicenceSetting>();
			AssertEquals(true, setting.EnableUnits);
			AssertEquals(0m, setting.LS9_Units);
			AssertEquals(0m, setting.UnitsForBinding);
			AssertEquals(false, setting.LS9_Units_ReadOnly);
			AssertEquals(false, setting.UnitsForBinding_ReadOnly);

			setting.LS9_Units = 100m;
			AssertEquals(100m, setting.UnitsForBinding);

			setting.EnableUnits = false;
			AssertEquals(-1m, setting.LS9_Units);
			AssertEquals(0m, setting.UnitsForBinding);
			AssertEquals(true, setting.LS9_Units_ReadOnly);
			AssertEquals(true, setting.UnitsForBinding_ReadOnly);
		}

		public void TestGetPriceAndUnits()
		{
			var setting = Factory.New<PriceLicenceSetting>();
			setting.LS9_Price = 10m;
			setting.LS9_Units = 100m;
			AssertEquals(false, setting.SupportUnitBreak);
			var result = setting.GetPriceAndUnits();
			AssertEquals(10m, result.Price.Value);
			AssertEquals(100m, result.Units.Value);

			AssertExceptionThrown<NotImplementedException>(() => setting.GetPriceAndUnits(100));
		}
	}

	internal class PriceLicenceSettingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidation_PriceTierCodes()
		{
			var price = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			price.L7_Category = "STL";
			price.L7_Code = "ABC";
			price.L7_UnitBreak = 100;
			Factory.Save();

			var setting = Factory.New<PriceLicenceSetting>();
			setting.PriceCode = "ABC";
			AssertHasWarning(setting.PriceCodeInfo, "This feature code has multiple tiers. Price setting will apply to all tiers. Use 'Price (Tier)' to setup each tier.");
			setting.PriceCode = "CCC";
			AssertNoWarnings(setting.PriceCodeInfo);
		}
	}
}
