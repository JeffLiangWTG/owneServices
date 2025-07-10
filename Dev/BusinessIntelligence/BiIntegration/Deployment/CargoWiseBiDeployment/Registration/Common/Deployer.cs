using System;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration.Licensing;

namespace CargoWise.Bi.Registration.Common
{
	#region SuppressResourceStringsCheckRegion
	public abstract class Deployer
	{
		protected const string prodDomainName = "PROD";
		protected const string testDomainName = "TEST";
		protected const string sandDomainName = "SAND";
		protected const string corpDomainName = "CORP";
		protected const string developmentGroup = @"CORP\g_Development";
		protected const string administratorGroup = @"BUILTIN\Administrators";
		protected const string contentManagerRoleName = "Content Manager";
		protected const string browserRoleName = "Browser";

		string clientSystemGroup;
		IProductRegistration registration;
		protected virtual string ClientSystemGroup
		{
			get
			{
				if (clientSystemGroup == null)
				{
					var userDomain = WindowsIdentity.GetCurrent().Name.Split(new char[] { '\\' },StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
					if (prodDomainName.Equals(userDomain, StringComparison.OrdinalIgnoreCase) ||
						testDomainName.Equals(userDomain, StringComparison.OrdinalIgnoreCase))
					{
						clientSystemGroup = string.Format(CultureInfo.InvariantCulture, @"{0}\g_{1}", prodDomainName, Registration.Key.EnterpriseCode);
					}
					else if (sandDomainName.Equals(userDomain, StringComparison.OrdinalIgnoreCase) ||
						corpDomainName.Equals(userDomain, StringComparison.OrdinalIgnoreCase))
					{
						clientSystemGroup = developmentGroup;
					}
					else
					{
						clientSystemGroup = string.Empty;
					}

					if (!CheckIfGroupExists(clientSystemGroup))
					{
						clientSystemGroup = string.Empty;
					}
				}
				return clientSystemGroup;
			}
		}

		bool CheckIfGroupExists(string groupName)
		{
			var result = false;
			if (!string.IsNullOrEmpty(groupName))
			{
				try
				{
					using (var context = new PrincipalContext(ContextType.Domain))
					{
						result = (GroupPrincipal.FindByIdentity(context, groupName) != null);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException()) { }
			}
			return result;
		}

		protected IProductRegistration Registration
		{
			get
			{
				return registration ?? (registration = ObjectFactory.Get<IProductRegistration>());
			}
		}
	}
	#endregion
}
