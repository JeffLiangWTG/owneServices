using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(CommitmentLicenceSetting))]
	internal class CommitmentLicenceSettingTest : EdiLicenceSettingTest
	{
	}

	internal class CommitmentLicenceSettingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUnusedFieldsNotValidated()
		{
			var db = Factory.New<LicenceDatabase>();
			var setting = Factory.New<CommitmentLicenceSetting>();
			setting.LS9_LD = db.PK;
			setting.LS9_ValidFrom = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 1);
			setting.LicenceUnits = 100;
			setting.Validation.ValidateAll();
			AssertNoErrors(setting);
		}
	}
}
