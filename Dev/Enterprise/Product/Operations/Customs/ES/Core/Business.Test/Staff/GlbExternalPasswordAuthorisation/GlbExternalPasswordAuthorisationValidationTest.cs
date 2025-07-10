using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordAuthorisationValidation))]
	public class GlbExternalPasswordAuthorisationValidationTest : GlbExternalPasswordAuthorisationValidationTest<GlbExternalPasswordAuthorisation, GlbExternalPasswordAuthorisationValidation>
	{
		public void TestCheckGEA_GS_AuthorisedStaff()
		{
			var collection = new GlbExternalPasswordAuthorisationCollection(Factory.New<GlbExternalPassword>());
			var sub1 = collection.AddNew();
			var sub2 = collection.AddNew();

			sub1.GEA_GS_AuthorisedStaff = ZGuid.Empty;
			sub2.GEA_GS_AuthorisedStaff = ZGuid.Empty;

			var userAlreadyRegistered = "This user has already been registered, it must be unique.";
			AssertEquals(true, sub1.GEA_GS_AuthorisedStaffInfo.HasError(userAlreadyRegistered));
			AssertEquals(true, sub2.GEA_GS_AuthorisedStaffInfo.HasError(userAlreadyRegistered));

			sub1.GEA_GS_AuthorisedStaff = GlbStaff.CurrentUser.PK;
			AssertEquals(false, sub1.GEA_GS_AuthorisedStaffInfo.HasError(userAlreadyRegistered));

			sub2.GEA_GS_AuthorisedStaff = GlbStaff.CurrentUser.PK;
			AssertEquals(true, sub2.GEA_GS_AuthorisedStaffInfo.HasError(userAlreadyRegistered));
		}
	}
}
