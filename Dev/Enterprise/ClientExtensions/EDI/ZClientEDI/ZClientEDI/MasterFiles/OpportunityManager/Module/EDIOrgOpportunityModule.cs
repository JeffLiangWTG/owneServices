using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIOrgOpportunityModule : OrgOpportunityModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDIOrgOpportunityFilterBusinessObject();
		}
		internal FilterBusinessObject InternalGetNewFilterBusinessObject() => GetNewFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl()
		{
			return new EDIOrgOpportunityFilterControl(GridCollection, (OrgOpportunityFilterBusinessObject)FilterBusinessObject);
		}
	}
}
