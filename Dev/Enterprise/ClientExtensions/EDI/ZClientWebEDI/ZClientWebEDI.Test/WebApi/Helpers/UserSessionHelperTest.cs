using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	internal class UserSessionHelperTest : TestCaseWithFactory
	{
		public void TestUserSessionHelper()
		{
			var webUser = new OrgContactWebUser();
			webUser.LoginSupportForTest("ASALOG");
			var session = UserSessionHelper.GenerateSessionString(webUser);
			var userSession = UserSessionHelper.DecodeSessionString(session);
			AssertEquals(webUser.LoggedInOrganisation.PK, userSession.LoggedInOrganisationPK);
			AssertEquals(webUser.LoggedInUserPK, userSession.LoggedInUserPK);
			AssertEquals(webUser.IsSuperUser, userSession.IsSuperUser);
			AssertEquals(webUser.SupportStaffCode, userSession.SupportStaffCode);
		}
	}
}