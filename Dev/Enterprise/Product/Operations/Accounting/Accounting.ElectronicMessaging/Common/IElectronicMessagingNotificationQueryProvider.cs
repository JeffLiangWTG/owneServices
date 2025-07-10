using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public interface IElectronicMessagingNotificationQueryProvider
	{
		ZQuery GetQueryForCompany(GlbCompany company, DateTime localTimeNow, int alertDays);

		ZQuery GetQueryForBranch(GlbCompany company);
	}
}
