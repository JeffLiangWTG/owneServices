using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security
{
	public abstract class SecurityOverrideProvider : ISecurityOverrideProvider
	{
		public static string InvalidLoginErrorMsg
		{
			get { return Res.GetString("91f9458f-9525-4baa-a3cb-d9a3fb31daf2", "The user name (or password) entered is incorrect or password is expired. Please re-enter the correct username and password."); }
		}

		public static string DeniedSecurityRightsErrorMsg
		{
			get { return Res.GetString("c7ddec69-de91-4ab3-b148-5915c77b0c74", "The Authorizing user does not have security rights for this Branch or Department. Your system administrator maintains each user's security rights."); }
		}

		#region ISecurityOverrideProvider Members

		ISecurityCertificateProvider ISecurityOverrideProvider.SecurityCertificates
		{
			get
			{
				if (fSecurityCertificates == null)
				{
					fSecurityCertificates = CreateSecurityCertificateProvider();
				}

				return fSecurityCertificates;
			}
		}

		public SecurityCore UserSecurityOverride { get; set; }

		public virtual SecurityCertificate PromptForTemporaryAccessCore(SecurityCheckpoint checkPoint)
		{
			UserSecurityOverride = RequestLoginCredentials(checkPoint);
			return GetSecurityCertificate(checkPoint, UserSecurityOverride);
		}

		protected SecurityCertificate GetSecurityCertificate(SecurityCheckpoint checkPoint, SecurityCore userSecurityOverride)
		{
			SecurityCertificate result = SecurityCertificate.Denied;
			if (userSecurityOverride != null)
			{
				ISecurityCheckpoint checkPointOverride;

				if (checkPoint.LookupKey.CodeEquals(Environment.Env.Security.None.Code))
				{
					checkPointOverride = new NoneSecurityCheckpoint();
				}
				else
				{
					checkPointOverride = userSecurityOverride.FindCheckPoint(checkPoint.LookupKey);
				}

				if (checkPointOverride != null)
				{
					if (!checkPointOverride.IsAllowed)
					{
						Globals.Message.Show(DeniedSecurityRightsErrorMsg);
					}
					else
					{
						result = SecurityCertificate.Granted;
					}
				}
				else
				{
					Globals.Message.ShowDeveloperErrorOnce("SecurityOverrideProvider.PromptForTemporaryAccess", "Can't get value from a field [" + checkPoint.LookupKey.Code + "]", "");
				}
			}

			return result;
		}

		SecurityCertificate ISecurityOverrideProvider.PromptForTemporaryAccess(SecurityCheckpoint checkPoint)
		{
			return PromptForTemporaryAccessCore(checkPoint);
		}

		SecurityCertificate ISecurityOverrideProvider.PromptForAccessGrantConfirmation(SecurityCheckpoint checkPoint)
		{
			SecurityCertificate result = SecurityCertificate.Granted;

			if (ShouldPromptForGranted)
			{
				result = RequestGrantedConfirmation(checkPoint);
			}

			return result;
		}

		bool ISecurityOverrideProvider.ShouldPromptForGrantedConfirmation
		{
			get { return ShouldPromptForGranted; }
		}

		bool ISecurityOverrideProvider.IsUserInitiatorAndNotAllowedToApprove
		{
			get { return UserInitiatorAndNotAllowedToApprove; }
		}

		#endregion

		#region Implementation

		protected abstract SecurityCore RequestLoginCredentials(SecurityCheckpoint checkPoint);
		protected abstract SecurityCertificate RequestGrantedConfirmation(SecurityCheckpoint checkPoint);

		protected virtual ISecurityCertificateProvider CreateSecurityCertificateProvider()
		{
			return new SecurityCertificateHashtable(this);
		}

		protected virtual string GetSecurityOverrideMessage(SecurityCheckpoint checkPoint)
		{
			return checkPoint.DefaultSecurityOverrideMessage;
		}

		protected virtual string GetSecurityGrantedMessage(SecurityCheckpoint checkPoint)
		{
			return string.Empty;
		}

		protected virtual bool ShouldPromptForGranted
		{
			get { return false; }
		}

		protected virtual bool UserInitiatorAndNotAllowedToApprove
		{
			get { return false; }
		}

		ISecurityCertificateProvider fSecurityCertificates;

		#endregion
	}
}
