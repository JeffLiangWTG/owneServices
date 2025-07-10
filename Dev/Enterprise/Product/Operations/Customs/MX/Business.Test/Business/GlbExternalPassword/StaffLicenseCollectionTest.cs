using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(StaffLicenseCollection))]
	class StaffLicenseCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var staff = Factory.New<GlbStaff>();
			var staffWrapper = MXGlbStaffWrapper.Get(staff);
			var externalPassword = staffWrapper.StaffLicenses.AddNew();
			AssertEquals("GP_PasswordType", PasswordTypesList.Codes.MXL, externalPassword.GP_PasswordType);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var staff = Factory.New<GlbStaff>();
			return new StaffLicenseCollection(staff);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<GlbExternalPassword_MXL>();
		}
	}
}
