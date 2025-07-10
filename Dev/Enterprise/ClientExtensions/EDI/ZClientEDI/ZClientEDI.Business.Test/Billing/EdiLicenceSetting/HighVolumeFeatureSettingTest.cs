using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(HighVolumeFeatureSetting))]
	internal class HighVolumeFeatureSettingTest : EdiLicenceSettingTest
	{
		public void TestHighVolumeFeatureSetting()
		{
			var obj = Factory.New<HighVolumeFeatureSetting>();
			AssertEquals("HVF", obj.LS9_Type);

			obj.LS9_Name = "#HD";
			AssertEquals("#HD", obj.Summary);

			AssertType<HighVolumeFeatureSettingValidation>(obj.Validation);
		}
	}

	internal class HighVolumeFeatureSettingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckLS9_Name()
		{
			var db = Factory.New<LicenceDatabase>();
			db.FillWithValidTestData();

			var link = db.PriceHeaderLinks.AddNew();
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_ValidFrom = ZDateTime.Now.AddYears(-1);
			priceHeader.L6_ValidTo = ZDateTime.Now.AddYears(1);
			var priceItem = priceHeader.Items.AddNew();
			priceItem.L7_Code = "#HD";
			priceItem.L7_Category = BillingConstants.BillingSystem.HostingStorage;
			link.PHL_ValidFrom = priceHeader.L6_ValidFrom;
			link.PHL_ValidTo = priceHeader.L6_ValidTo;
			link.PHL_L6 = priceHeader.PK;

			var hvfSetting = Factory.New<HighVolumeFeatureSetting>();
			hvfSetting.LS9_LD = db.PK;

			hvfSetting.PriceKey = new UsageCodeKey("", "");
			hvfSetting.RunPreSaveValidation();
			AssertHasError(hvfSetting.PriceCodeInfo, "Please enter a Price Code.");

			hvfSetting.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.HostingStorage, "#AA");
			hvfSetting.RunPreSaveValidation();
			AssertHasError(hvfSetting.PriceCodeInfo, "Enter a valid Price Code.");

			hvfSetting.PriceKey = priceItem.CodeKey;
			hvfSetting.RunPreSaveValidation();
			AssertNoErrors(hvfSetting.PriceCodeInfo);
		}

		public void TestCheckLS9_Type()
		{
			var db = Factory.New<LicenceDatabase>();
			db.FillWithValidTestData();

			var link = db.PriceHeaderLinks.AddNew();
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_ValidFrom = ZDateTime.Now.AddYears(-1);
			priceHeader.L6_ValidTo = ZDateTime.Now.AddYears(1);
			var priceItem = priceHeader.Items.AddNew();
			priceItem.L7_Code = "#HD";
			priceItem.L7_Category = BillingConstants.BillingSystem.HostingStorage;
			link.PHL_ValidFrom = priceHeader.L6_ValidFrom;
			link.PHL_ValidTo = priceHeader.L6_ValidTo;
			link.PHL_VolumeCode = "HV";
			link.PHL_L6 = priceHeader.PK;

			var hvf = Factory.New<HighVolumeFeatureSetting>();
			hvf.LS9_LD = db.PK;
			var price = Factory.New<PriceLicenceSetting>();
			price.LS9_LD = db.PK;

			hvf.PriceKey = priceItem.CodeKey;
			hvf.LS9_ValidFrom = priceHeader.L6_ValidFrom;
			hvf.LS9_ValidTo = priceHeader.L6_ValidTo;
			hvf.RunPreSaveValidation();
			AssertNoErrors(hvf.LS9_NameInfo);

			link.PHL_VolumeCode = "LV";
			price.PriceKey = priceItem.CodeKey;
			price.LS9_ValidFrom = priceHeader.L6_ValidFrom;
			price.LS9_ValidTo = priceHeader.L6_ValidTo;

			hvf.RunPreSaveValidation();
			AssertHasError(hvf.LS9_TypeInfo, "Can't have price setting and high volume setting for the same feature in the same period.");
			AssertHasError(hvf.LS9_TypeInfo, "Can't have a High Volume feature without a price list link of type \"HV\" - \"High Volume\" in the same period.");
		}
	}
}
