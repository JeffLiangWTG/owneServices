using CargoWise.Application;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security
{
	public class AlternativeCredentials
	{
		public readonly string Login;
		protected readonly string password;

		public AlternativeCredentials(string userName, string userPassword)
		{
			Login = userName;
			password = userPassword;
		}

		public SecurityCore UserSecurity
		{
			get
			{
				if (userSecurity == null)
				{
					var loginController = new UserLoginController();

					loginAuthentication = ValidateAlternativeCredentials(loginController);

					if (loginAuthentication.LoginValidated)
					{
						userSecurity = loginController.GetSecurityForUser(Login, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
					}
				}

				return userSecurity;
			}
		}
		SecurityCore userSecurity;

		protected LoginAuthenticationInfo ValidateAlternativeCredentials(UserLoginController loginController)
		{
			var loginAuthentication = ShouldValidateSecurityOverrideToken ? ValidateSecurityOverrideToken() : loginController.ValidateUserLoginAndPasswordAndExpiredPassword(Login, password);
			return loginAuthentication;
		}

		bool ShouldValidateSecurityOverrideToken => SystemDataRegistry.Instance.EnableSecurityOverrideToken.Value && ObjectFactory.Get<IOIDCConfig>().IsOIDCEnabled;

		LoginAuthenticationInfo ValidateSecurityOverrideToken()
		{
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, Login));
			if (staff == null)
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserNotFound, Res.GetString("6ADEA230-331D-4DB3-9F9A-9DF95B5D9DCF", "Invalid user login name."));
			}

			var tokenizedAccessControl = ObjectFactory.Get<ITokenizedAccessControl>();
			if (tokenizedAccessControl.TryPeek(password, AccessTokenTypes.SecurityOverrideToken, out var accessTokenInfo))
			{
				if (accessTokenInfo.ParentTableCode == GlbStaffSchema.Constants.Prefix && accessTokenInfo.ParentId == staff.PK)
				{
					if (tokenizedAccessControl.TryConsume(password, AccessTokenTypes.SecurityOverrideToken, out accessTokenInfo))
					{
						return LoginAuthenticationInfo.NewSuccessfulLogin(staff);
					}
				}
			}

			return LoginAuthenticationInfo.NewFailedLogin(Res.GetString("5D8F3B24-8664-483C-95B4-3D0DA3B2875B", "Invalid Security Override Token"));
		}

		public LoginAuthenticationInfo LoginAuthentication => loginAuthentication;
		protected LoginAuthenticationInfo loginAuthentication;

		#region Factory

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		#endregion
	}
}
