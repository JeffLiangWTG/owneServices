using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentChangePropertyEvent : IncidentEvent
	{
		public SupportIncidentChangePropertyEvent(SupportIncident incident)
			: base(incident)
		{
		}

		public override ZString Code
		{
			get { return IncidentEventFactory.Codes.ChangeProperty; }
		}

		protected override bool ApplyWorkflowTemplate()
		{
			bool result;
			var supportIncident = incident as SupportIncident;
			using (supportIncident.SuspendCalculateStatusAndDisposition())
			{
				result = base.ApplyWorkflowTemplate();
			}
			return result;
		}

		protected override bool IsProcessHeaderMetCondition(BufferManagement.Integration.IProcessHeader processHeader)
		{
			return true;
		}
	}
}

