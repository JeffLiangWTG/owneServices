using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordAuthorisation))]
	public class GlbExternalPasswordAuthorisationTest : GlbExternalPasswordAuthorisationTest<GlbExternalPasswordAuthorisation, GlbExternalPassword>
	{
		public void TestNewGrantedAuthorisation()
		{
			var auth1 = CreateNewGlbExternalPasswordAuthorisation(Factory);
			Staff.GS_Code = "ZZZ";
			ExternalPassword.GP_Name = "TestCert";
			Factory.Save();
			AssertEquals("User ZZZ has been authorised to use certificate TestCert.", ExternalPassword.Logs.MostRecentLogByEventTime(Events.Authorised).SL_Reference);
		}

		public void TestWithdrawnAuthorisation()
		{
			var auth1 = CreateNewGlbExternalPasswordAuthorisation(Factory);
			Staff.GS_Code = "ZZZ";
			ExternalPassword.GP_Name = "TestCert";
			Factory.Save();
			auth1.Delete();
			Factory.Save();
			AssertEquals("Authorisation for user ZZZ to use certificate TestCert has been withdrawn.", ExternalPassword.Logs.MostRecentLogByEventTime(Events.AuthorisationWithdrawn).SL_Reference);
		}

		public void TestNotNullableParent()
		{
			var auth1 = CreateNewGlbExternalPasswordAuthorisation(Factory);
			auth1.GEA_GS_AuthorisedStaff = ZGuid.NewZGuid();
			AssertExceptionThrown<ZSaveException>("Exception thrown due to invalid FK", () => Factory.Save());
		}

		public void TestAuthorisedStaffCode()
		{
			var auth1 = CreateNewGlbExternalPasswordAuthorisation(Factory);
			Staff.GS_Code = "ZZZ";
			CombineAssertions(() =>
			{
				AssertEquals("AuthorisedStaffCode is the code of the Staff", "ZZZ", auth1.AuthorisedStaffCode);
				var guid = ZGuid.NewZGuid();
				auth1.GEA_GS_AuthorisedStaff = guid;
				AssertEquals("AuthorisedStaffCode is the guid due to the Staff doesn't exist", guid.ToString(), auth1.AuthorisedStaffCode);
			});
		}

		public void TestStaffName()
		{
			Staff.GS_FullName = "ZAC Full Name";
			Factory.Save();
			var authorisation = Factory.New<GlbExternalPasswordAuthorisation>();
			AssertEquals(ZString.Empty, authorisation.StaffName);
			AssertEquals(ZGuid.Empty, authorisation.GEA_GS_AuthorisedStaff);

			authorisation.GEA_GS_AuthorisedStaff = Staff.PK;

			AssertEquals("ZAC Full Name", authorisation.StaffName);
			AssertEquals(Staff.PK, authorisation.GEA_GS_AuthorisedStaff);
		}

		public void TestReadOnlyAuthorisation()
		{
			var auth1 = CreateNewGlbExternalPasswordAuthorisation(Factory);
			AssertEquals("This authorisation isn't readonly", false, auth1.GEA_GS_AuthorisedStaff_ReadOnly);
			Factory.Save();
			AssertEquals("This authorisation is readonly", true, auth1.GEA_GS_AuthorisedStaff_ReadOnly);
			auth1.GEA_GS_AuthorisedStaff = ZGuid.Invalid;
			AssertEquals("This authorisation isn't readonly", false, auth1.GEA_GS_AuthorisedStaff_ReadOnly);
		}

		protected override GlbExternalPasswordAuthorisation CreateNewGlbExternalPasswordAuthorisation(BusinessObjectFactory factory)
		{
			var result = base.CreateNewGlbExternalPasswordAuthorisation(factory);
			result.GEA_GS_AuthorisedStaff = Staff.PK;
			result.GEA_GP = ExternalPassword.PK;
			return result;
		}
	}
}
