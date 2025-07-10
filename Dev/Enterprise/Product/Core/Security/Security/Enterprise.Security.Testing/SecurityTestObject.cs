using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.Testing
{
	public static class SecurityTestObject
	{
		public static void CreateTestUser(bool grant, string securityItem, string code, string login, string password)
		{
			var factory = new BusinessObjectFactory();

			var staff = factory.New<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_LoginName = login;
			staff.StaffPlainTextPassword = password;
			staff.GS_ChangePasswordAtNextLogin = false;

			AddNewSecurityRowToStaff(factory, ((SecurityCore)EnvProxy.Instance.Security).StaffOwnDetails.Code, grant, staff.PK);
			if (!string.IsNullOrEmpty(securityItem))
			{
				AddNewSecurityRowToStaff(factory, securityItem, grant, staff.PK);
			}

			factory.Save();
		}

		static void AddNewSecurityRowToStaff(BusinessObjectFactory factory, string securityRight, bool isAllowed, ZGuid staffPK)
		{
			var security = factory.New<GlbSecurity>();
			security.GU_GS = staffPK;
			security.GU_SecurityRight = securityRight;
			security.GU_SecurityItemIsAllowed = isAllowed;
		}
	}
}
