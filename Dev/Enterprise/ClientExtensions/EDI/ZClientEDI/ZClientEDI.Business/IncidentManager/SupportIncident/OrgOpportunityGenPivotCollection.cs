using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class OrgOpportunityGenPivotCollection : GenPivotCollection
	{
		public OrgOpportunityGenPivotCollection(BusinessObject master, bool includeChildren = true, bool includeParents = true)
			: base(master, Constants.GenPivotTypes.Opportunity, includeChildren, includeParents)
		{
		}
	}
}
