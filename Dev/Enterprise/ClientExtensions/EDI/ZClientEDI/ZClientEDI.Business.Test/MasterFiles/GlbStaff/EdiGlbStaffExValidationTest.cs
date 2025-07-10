using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	internal class EdiGlbStaffExValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateGS9_GS()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_Code = "FOO";
			staff.GS_LoginName = "foo";
			staff.GS_FullName = "Foo";
			staff.GS_IsActive = false;
			Factory.Save();
			AssertNull("PRE", staff.ReadonlyStaffEx);

			var staffEx = staff.StaffEx;
			staffEx.Validation.ValidateGS9_GS();
			AssertNoErrors(staffEx.GS9_GSInfo);
		}

		public void TestNameValidation()
		{
			var staff = Factory.New<EDIGlbStaff>();
			var staffEx = staff.StaffEx;
			staffEx.Validation.ValidateAll();
			AssertNoNotifications(staffEx.GS9_FirstNameInfo);
			AssertNoNotifications(staffEx.GS9_MiddleNameInfo);
			AssertNoNotifications(staffEx.GS9_LastNameInfo);
			AssertNoNotifications(staffEx.GS9_DomesticNameInfo);
		}
	}
}
