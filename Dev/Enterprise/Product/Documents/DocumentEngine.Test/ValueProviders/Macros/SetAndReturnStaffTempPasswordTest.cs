using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(SetAndReturnStaffTempPassword))]
	sealed class SetAndReturnStaffTempPasswordTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider() => new SetAndReturnStaffTempPassword();

		public void TestInvalidPK()
		{
			AssertEquals("Expecting macro to translate", string.Empty, new MacroTranslator(Report).GetValue("<SetAndReturnStaffTempPassword(invalidguid)>", Passes.FirstPass));
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Could not convert to Guid: invalidguid", Report.ErrorManager.ToString());
		}

		public void TestStafNotFound()
		{
			AssertEquals("Expecting macro to translate", string.Empty, new MacroTranslator(Report).GetValue("<SetAndReturnStaffTempPassword(a1a58f8d-b6ee-414a-9921-844124b64188)>", Passes.FirstPass));
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Staff not found: a1a58f8d-b6ee-414a-9921-844124b64188", Report.ErrorManager.ToString());
		}

		public void TestStaffAlreadyHasPassword()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Test";
			staff.ChangeLocalPassword(null, "fortest");
			Factory.Save();

			AssertEquals("Expecting macro to translate", string.Empty, new MacroTranslator(Report).GetValue($"<SetAndReturnStaffTempPassword({staff.PK})>", Passes.FirstPass));
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Staff record is found but password cannot be reset", Report.ErrorManager.ToString());
		}

		public void TestSystemStaffAlreadyHasPassword()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			staff.GS_FullName = "systemuser";
			staff.ChangeLocalPassword(null, "Password");
			staff.LocalPasswordMustBeReset = true;
			staff.GS_IsSystemAccount = true;

			AssertEquals("Expecting macro to translate", string.Empty, new MacroTranslator(Report).GetValue($"<SetAndReturnStaffTempPassword({staff.PK})>", Passes.FirstPass));
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Staff record is found but the password of the system account cannot be reset", Report.ErrorManager.ToString());
		}

		public void TestSetsStaffPassword()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			staff.LocalPasswordMustBeReset = true;
			Factory.Save();

			AssertNotEquals("Expecting macro to translate", string.Empty, new MacroTranslator(Report).GetValue($"<SetAndReturnStaffTempPassword({staff.PK})>", Passes.FirstPass));
			Assert(!Report.ErrorManager.HasErrors);
			AssertNotEquals(ZBlob.Empty, staff.GS_PasswordHash);
			AssertNotEquals(ZBlob.Empty, staff.GS_PasswordSalt);
			AssertEquals("Reset password at login flag is true", true, staff.GS_ChangePasswordAtNextLogin);
		}
	}
}
