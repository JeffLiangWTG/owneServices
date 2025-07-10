using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	class EDISendEmailActionMenuStrategy : SendEmailActionMenuStrategy
	{
		protected override void AddSendEmailActionMenuIfApplicableCore(ZForm zForm)
		{
			var sendEmailSource = GetSendEmailSource(zForm);
			var incident = sendEmailSource as SupportIncident;
			if (incident != null)
			{
				if (!(LicenceDatabase.CanSupportBiDirectionIncidentMessage(incident.ClientReportedOnVersion) != Enterprise.Customs.Business.TriState.True))
				{
					return;
				}
			}
			base.AddSendEmailActionMenuIfApplicableCore(zForm);
		}
	}
}
