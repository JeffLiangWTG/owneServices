using System;
using System.Net;
using CargoWise.ActiveDirectory;
using CargoWise.Data;
using Enterprise.Registry.Business;

namespace CargoWise.Bi.Registration.Common
{
	public class BiReportUser
	{
		public BiReportUser()
		{
#if DEBUG
			if (Db.ServerNameIsInitialized)
#endif
			{
				var reportUserCredential = SystemDataRegistry.Instance.BiReportUserCredential.Value;
				if (reportUserCredential != null && !string.IsNullOrWhiteSpace(reportUserCredential.UserName) && !string.IsNullOrWhiteSpace(reportUserCredential.Domain))
				{
					UserName = reportUserCredential.UserName;
					Domain = reportUserCredential.Domain;
					Password = reportUserCredential.Password;
					DomainAndUsername = Domain + "\\" + UserName;
					NetworkCredential = new NetworkCredential(UserName, Password, Domain);
				}
			}
		}

		public readonly NetworkCredential NetworkCredential;
		public readonly string UserName;
		public readonly string Domain;
		public readonly string Password;
		public readonly string DomainAndUsername;

		public bool IsNull => NetworkCredential == null || string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Domain);

		public IDisposable Impersonate()
		{
			if (!IsNull &&
				(!Environment.UserDomainName.Equals(Domain, StringComparison.OrdinalIgnoreCase) ||
				 !Environment.UserName.Equals(UserName, StringComparison.OrdinalIgnoreCase)))
			{
				return new WindowsIdentityImpersonator(DomainAndUsername, Password, () => { });
			}
			else
			{
				return null;
			}
		}
	}
}
