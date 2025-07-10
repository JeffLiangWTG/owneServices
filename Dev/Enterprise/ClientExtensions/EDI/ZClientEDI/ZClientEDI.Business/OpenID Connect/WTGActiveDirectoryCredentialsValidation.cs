using CargoWise.EntityFramework;

namespace ZClientEDI.Business
{
	public class WTGActiveDirectoryCredentialsValidation
	{
		public WTGActiveDirectoryCredentialsValidation(WTGActiveDirectoryCredentials parent)
		{
			this.parent = parent;
		}

		readonly WTGActiveDirectoryCredentials parent;

		#region Base Validation

		public void ValidateDomainName()
		{
			parent.DomainNameInfo.ClearAllNotifications();
			if (parent.IsEnabled)
			{
				MandatoryValidation.CheckEntered(parent.DomainNameInfo);
			}
		}

		public void ValidateDomainUserName()
		{
			parent.DomainUserNameInfo.ClearAllNotifications();
			if (parent.IsEnabled)
			{
				MandatoryValidation.CheckEntered(parent.DomainUserNameInfo);
			}
		}

		public void ValidateDomainUserPassword()
		{
			parent.DomainUserPasswordInfo.ClearAllNotifications();
			if (parent.IsEnabled)
			{
				MandatoryValidation.CheckEntered(parent.DomainUserPasswordInfo);
			}
		}

		public void ValidateOrganizationalUnitPath()
		{
			parent.OrganizationalUnitPathInfo.ClearAllNotifications();
			if (parent.IsEnabled)
			{
				MandatoryValidation.CheckEntered(parent.OrganizationalUnitPathInfo);
			}
		}

		#endregion
	}
}
