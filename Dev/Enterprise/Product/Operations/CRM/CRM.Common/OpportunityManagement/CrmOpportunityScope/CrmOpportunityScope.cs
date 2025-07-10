using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.CRM.Common
{
	public class CrmOpportunityScope : AutoCrmOpportunityScope
	{
		public CrmOpportunityScope(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public abstract new class Schema : AutoCrmOpportunityScope.Schema
		{
		}
	}
}
