using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentCancelEmailNotificationAction : SupportIncidentAction
	{
		public SupportIncidentCancelEmailNotificationAction(SupportIncident incident) : base(incident)
		{
		}

		protected override void PerformAction()
		{
			var message = Res.GetString("F91E5D7C-0744-43E8-A3B4-B7FD313A5013", "{0} canceled the customer email notification.", GlbStaff.CurrentUser.GS_FullName);

			if (!Comment.IsEmpty)
			{
				message += " " + Res.GetString("8B6EEDF3-DB79-4BBD-98B5-E8C44A344FDC", "Reason: {0}", Comment);
			}

			try
			{
				AddMessageAndSave(Incident, message);
			}
			catch (ZSaveConcurrencyException)
			{
				var newFactory = new BusinessObjectFactory();
				var loadedIncident = newFactory.Load<SupportIncident>(Incident.PK);
				AddMessageAndSave(loadedIncident, message);
			}
		}

		void AddMessageAndSave(SupportIncident incident, string message)
		{
			incident.AddInternalMessage(message);
			incident.Factory.Save();
		}
	}
}
