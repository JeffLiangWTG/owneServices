using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordValidation_BRS))]
	public class GlbExternalPasswordValidation_BRSTest : MasterFiles.Business.Testing.GlbExternalPasswordValidationTest<GlbExternalPassword_BRS, GlbExternalPasswordValidation_BRS>
	{
		public void TestCheckGP_UserID()
		{
			Factory.New<OrgHeader>().OH_Code = "DEC1";
			Factory.Save();
			var staff = Factory.New<GlbStaff>();
			var staffWrapper = BRGlbStaffWrapper.Get(staff);
			var externalPassword = staffWrapper.EventSubscriptions.AddNew();
			externalPassword.GP_UserID = "";
			externalPassword.Validation.ValidateGP_UserID();
			AssertHasErrorContaining(externalPassword.GP_UserIDInfo, "Please enter a value.");
			externalPassword.GP_UserID = "XXX";
			externalPassword.Validation.ValidateGP_UserID();
			AssertHasError(externalPassword.GP_UserIDInfo, "Enter a valid selection.");
			externalPassword.GP_UserID = EventIdList.Codes.CctReleasedCargo;
			externalPassword.Validation.ValidateGP_UserID();
			AssertNoError(externalPassword.GP_UserIDInfo, "Enter a valid selection.");
		}
	}
}
