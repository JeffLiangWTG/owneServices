using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentGenericEvent : SupportIncidentEvent
	{
		public SupportIncidentGenericEvent(SupportIncident incident, ZString eventCode)
			: base(incident)
		{
			this.eventCode = eventCode;
		}
		readonly ZString eventCode;

		public override ZString Code
		{
			get { return eventCode; }
		}
	}
}

