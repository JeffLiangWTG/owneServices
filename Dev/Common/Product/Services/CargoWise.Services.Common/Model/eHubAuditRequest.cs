using System;
#if NETFRAMEWORK
using System.Data;
#else
using System.Data.Entity;
#endif

namespace CargoWise.Services.Common.Model
{
	public partial class eHubAuditRequest
	{
		public eHubAuditRequest()
		{
			if (EntityState == EntityState.Detached)
			{
				B0_PK = Guid.NewGuid();
				B0_RequestUTC = DateTime.UtcNow;
				B0_UserName = "";
				B0_TransactionType = "";
				B0_TransactionSubType = "";
				B0_RequestIP = "";
				B0_LicenceCode = "";
				B0_ClientSpecifiedIdentifierType = "";
			}
		}
	}
}
