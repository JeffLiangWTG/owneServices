using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class WorkItemCascadeGenPivotCollection : GenPivotCollection
	{
		public WorkItemCascadeGenPivotCollection(BusinessObject master, bool includeChildren = true, bool includeParents = true)
			: base(master, Constants.GenPivotTypes.WorkItemCascade, includeChildren, includeParents)
		{
		}
	}
}
