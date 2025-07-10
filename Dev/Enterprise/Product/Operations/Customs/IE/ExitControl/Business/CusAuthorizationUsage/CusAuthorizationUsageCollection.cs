using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusAuthorizationUsageCollection<TCusAuthorizationUsage, TMaster> : EU.Business.CusAuthorizationUsageCollection<TCusAuthorizationUsage, TMaster>
	where TCusAuthorizationUsage : CusAuthorizationUsage
	where TMaster : BusinessObject, ICusAuthorizationUsageMaster, ILinkable
	{
		public CusAuthorizationUsageCollection(TMaster master, BusinessObjectFactory factory)
			: base(master, master.Factory)
		{
		}
	}
}
