using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(DiscountSuspensionPolicyLicenceSetting))]
	internal class DiscountSuspensionPolicyLicenceSettingTest : EdiLicenceSettingTest
	{
		public void TestDiscountSuspensionPolicyLicenceSetting()
		{
			var obj = Factory.New<DiscountSuspensionPolicyLicenceSetting>();
			AssertEquals(BillingConstants.LicenceSetting.DiscountSuspensionPolicy, obj.LS9_Type);
			AssertEquals("NVR", obj.PolicyCode);
			AssertEquals("NVR", obj.LS9_Name);

			AssertEquals("NVR - Never Suspend", obj.Summary);
			AssertType<DiscountSuspensionPolicyLicenceSettingValidation>(obj.Validation);

			obj.PolicyCode = "ALW";
			AssertEquals("ALW", obj.PolicyCode);
			AssertEquals(obj.LS9_Name, obj.PolicyCode);
		}
	}

	internal class DiscountSuspensionPolicyLicenceSettingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPolicyCode()
		{
			var db = Factory.New<LicenceDatabase>();
			db.FillWithValidTestData();

			var setting = Factory.New<DiscountSuspensionPolicyLicenceSetting>();
			AssertEquals("NVR", setting.PolicyCode);
			AssertEquals(setting.LS9_Name, setting.PolicyCode);
			AssertNoErrors(setting.PolicyCodeInfo);

			setting.PolicyCode = "";
			AssertHasError(setting.PolicyCodeInfo, "Please enter a value.");
			setting.PolicyCode = "xxx";
			AssertHasError(setting.PolicyCodeInfo, "Enter a valid selection.");

			setting.PolicyCode = "ALW";
			AssertEquals("ALW", setting.PolicyCode);
			AssertEquals(setting.LS9_Name, setting.PolicyCode);
			AssertNoErrors(setting.PolicyCodeInfo);
		}
	}
}
