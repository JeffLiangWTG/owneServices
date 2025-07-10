using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public class SecurityLogin : NonPersistentBusinessObject, IObsoleteValidation, ISecurityLogin
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public SecurityLogin(List<Func<SecurityCore, SecurityCheckpoint>> securityCheckpoints)
			: base()
		{
			Argument.NotNull(securityCheckpoints, "securityCheckpoints");
			SecurityCheckpoints = new List<Func<SecurityCore, SecurityCheckpoint>>(securityCheckpoints);
		}

		public SecurityLogin(Func<SecurityCore, SecurityCheckpoint> getSecurityCheckpoint)
			: base()
		{
			SecurityCheckpoints = new List<Func<SecurityCore, SecurityCheckpoint>>();
			Argument.NotNull(getSecurityCheckpoint, "getSecurityCheckpoint");
			SecurityCheckpoints.Add(getSecurityCheckpoint);
		}

		#region Properties

		public static readonly MultilingualString CancelledText = ResString.GetMultilingualString("BA476479-4897-40FF-9B4C-C04A71FF01FF", "canceled.");

		#region Login

		[MaxLength(35)]
		public ZString Login
		{
			get { return fLogin; }
			set
			{
				if (value != Login)
				{
					CheckMaximumLength(LoginInfo, value);
					fLogin = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidateLogin();
					}
					LoginInfo.RefreshBinding();
				}
			}
		}

		ZString fLogin = ZString.Empty;

		public void ValidateLogin()
		{
			LoginInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LoginInfo);
		}

		public ZPropertyInfo LoginInfo
		{
			get { return GetZPropertyInfo(nameof(Login)); }
		}

		#endregion

		#region Password

		[Password]
		public ZString Password
		{
			get { return fPassword; }
			set
			{
				if (value != Password)
				{
					fPassword = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidatePassword();
					}
					PasswordInfo.RefreshBinding();
				}
			}
		}

		ZString fPassword = ZString.Empty;

		public void ValidatePassword()
		{
			PasswordInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PasswordInfo);
		}

		public ZPropertyInfo PasswordInfo
		{
			get { return GetZPropertyInfo(nameof(Password)); }
		}

		#endregion

		public ZString MessageToShowWhenNotPrinting { get; set; }

		public static new string TableName => "";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public List<Func<SecurityCore, SecurityCheckpoint>> SecurityCheckpoints { get; }

		public ZBool HideApprovalRequestButton { get; set; }

		#endregion

		#region Login

		public ZBool CheckIfValidLoginForDocumentPrinting()
		{
			var result = ZBool.False;
			var loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());
			if (loginController.ValidateUserLoginAndPasswordAndExpiredPassword(Login, Password).LoginValidated)
			{
				var userSecurity = (SecurityCore)loginController.GetSecurityForUser(Login, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
				for (var i = 0; i < SecurityCheckpoints.Count; i++)
				{
					var checkpoint = SecurityCheckpoints[i];
					if (checkpoint(userSecurity).IsAllowed)
					{
						result = ZBool.True;
						SecurityCheckpoints.RemoveAt(i--);
					}
				}
			}

			return result;
		}

		public bool IsRestrictedByCheckpoint(IZSecurity security, params CheckpointLookupKey[] lookupKeys)
		{
			var securityImpl = security as SecurityCore;

			if (securityImpl == null)
			{
				return false;
			}

			return SecurityCheckpoints.Any(checkpoint =>
			{
				var checkPointLookupKey = checkpoint(securityImpl).LookupKey;
				return lookupKeys?.Any(u => checkPointLookupKey == u) ?? false;
			});
		}

		#endregion
	}
}
