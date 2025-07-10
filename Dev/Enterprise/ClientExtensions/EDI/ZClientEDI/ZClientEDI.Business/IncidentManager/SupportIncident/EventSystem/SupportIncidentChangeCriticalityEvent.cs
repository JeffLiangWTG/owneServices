using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentChangeCriticalityEvent : SupportIncidentEvent
	{
		public SupportIncidentChangeCriticalityEvent(SupportIncident incident)
			: base(incident)
		{
		}

		public override ZString Code
		{
			get { return IncidentEventFactory.Codes.ChangeCriticality; }
		}

		protected override bool AlwaysCloseExistingTasks
		{
			get { return false; }
		}
	}
}

