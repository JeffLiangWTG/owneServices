using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Security
{
	public class SecurityOverridenLogin : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SecurityOverridenLogin()
		{
		}

		public SecurityOverridenLogin(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Login

		[MaxLength(GlbStaff.Schema.GS_LoginNameMaxLength)]
		public ZString Login
		{
			get { return login; }
			set
			{
				if (value != Login)
				{
					CheckMaximumLength(LoginInfo, value);
					login = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidateLogin();
					}
					LoginInfo.RefreshBinding();

					userSecurity = null;
				}
			}
		}
		ZString login;

		public void ValidateLogin()
		{
			LoginInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LoginInfo, Res.GetString("7278c260-51d5-43a0-a99e-3fab08f2de40", "login name"));
		}

		public ZPropertyInfo LoginInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Login));
			}
		}

		#endregion

		#region Password

		[Password]
		public ZString Password
		{
			get { return password; }
			set
			{
				if (value != password)
				{
					password = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidatePassword();
					}
					PasswordInfo.RefreshBinding();

					userSecurity = null;
				}
			}
		}
		ZString password;

		public void ValidatePassword()
		{
			PasswordInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PasswordInfo, Res.GetString("7deb142f-9307-4148-9f9a-0491a3db86b4", "password"));
		}

		public ZPropertyInfo PasswordInfo
		{
			get { return GetZPropertyInfo(nameof(Password)); }
		}

		#endregion

		#region UserSecurity

		public SecurityCore UserSecurity
		{
			get
			{
				if (userSecurity == null)
				{
					var loginController = new UserLoginController();
					if (loginController.ValidateUserLoginAndPasswordAndExpiredPassword(Login, Password).LoginValidated)
					{
						userSecurity = loginController.GetSecurityForUser(Login, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
					}
				}

				return userSecurity;
			}
		}

		SecurityCore userSecurity;

		#region Testing Only
#if DEBUG
		public void SetUserSecurityForTests(SecurityCore security)
		{
			userSecurity = security;
		}
#endif
		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateLogin();
			ValidatePassword();
		}

		#endregion
	}
}
