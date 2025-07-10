using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(VersionSurchargeLicenceSetting))]
	internal class VersionSurchargeLicenceSettingTest : EdiLicenceSettingTest
	{
		public void TestVersionSurchargeLicenceSetting()
		{
			var reg = EDIDataRegistry.Instance;
			reg.VersionSurchargePercent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			reg.VersionSurchargeAdditionalPercent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			var obj = Factory.New<VersionSurchargeLicenceSetting>();
			AssertEquals(BillingConstants.LicenceSetting.VersionSurcharge, obj.LS9_Type);
			AssertEquals(2m, obj.LS9_Percent);
			AssertEquals(1m, obj.LS9_Price);

			AssertEquals("2% + 1% per Version", obj.Summary);

			AssertType<VersionSurchargeLicenceSettingValidation>(obj.Validation);
		}
	}

	internal class VersionSurchargeLicenceSettingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckLS9_Percent()
		{
			var db = Factory.New<LicenceDatabase>();
			db.FillWithValidTestData();

			var setting = Factory.New<VersionSurchargeLicenceSetting>();
			setting.LS9_LD = db.PK;

			setting.LS9_Percent = -1;
			setting.RunPreSaveValidation();
			AssertHasErrors(setting.LS9_PercentInfo);

			setting.LS9_Percent = 0;
			setting.RunPreSaveValidation();
			AssertNoErrors(setting.LS9_PercentInfo);

			setting.LS9_Percent = 2;
			setting.RunPreSaveValidation();
			AssertNoErrors(setting.LS9_PercentInfo);
		}

		public void TestCheckLS9_Price()
		{
			var db = Factory.New<LicenceDatabase>();
			db.FillWithValidTestData();

			var setting = Factory.New<VersionSurchargeLicenceSetting>();
			setting.LS9_LD = db.PK;

			setting.AdditionalPercent = -1;
			setting.RunPreSaveValidation();
			AssertHasErrors(setting.AdditionalPercentInfo);

			setting.AdditionalPercent = 0;
			setting.RunPreSaveValidation();
			AssertNoErrors(setting.AdditionalPercentInfo);

			setting.AdditionalPercent = 1;
			setting.RunPreSaveValidation();
			AssertNoErrors(setting.AdditionalPercentInfo);
		}
	}
}
