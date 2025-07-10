using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.CRM.Common
{
	public class CrmOpportunityDocManagerInfo : DocManagerInfo
	{
		public CrmOpportunityDocManagerInfo(CrmOpportunity crmOpportunity, ZString code)
			: base(crmOpportunity, code)
		{
		}
	}
}
