using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupCollection : BusinessObjectCollection<IncidentManagementGroup>
	{
		public IncidentManagementGroupCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public IncidentManagementGroupCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}
	}
}
