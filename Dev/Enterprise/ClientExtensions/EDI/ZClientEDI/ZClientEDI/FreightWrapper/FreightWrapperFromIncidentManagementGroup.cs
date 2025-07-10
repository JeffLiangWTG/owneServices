using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class FreightWrapperFromIncidentManagementGroup : FreightWrapperEDI<IncidentManagementGroup>
	{
		public FreightWrapperFromIncidentManagementGroup(IncidentManagementGroup incidentGroup, BusinessObjectFactory factory)
			: base(incidentGroup, factory)
		{
		}
	}
}
