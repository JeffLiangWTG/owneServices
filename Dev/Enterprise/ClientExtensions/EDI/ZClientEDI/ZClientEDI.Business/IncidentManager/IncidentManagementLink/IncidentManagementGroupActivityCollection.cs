using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupActivityCollection : PivotBusinessObjectCollection<IncidentManagementLink>
	{
		public IncidentManagementGroupActivityCollection(BusinessObject master, bool includeChildren = true, bool includeParents = true) : base(master, new IncidentManagementGroupActivityCollectionRelationship(master, includeChildren, includeParents), includeChildren, includeParents)
		{
		}
	}
}
