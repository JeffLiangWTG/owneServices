using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class FreightWrapperFromSupportIncident : FreightWrapperEDI<SupportIncident>
	{
		public FreightWrapperFromSupportIncident(SupportIncident supportIncident, BusinessObjectFactory factory)
			: base(supportIncident, factory)
		{ }
	}
}
