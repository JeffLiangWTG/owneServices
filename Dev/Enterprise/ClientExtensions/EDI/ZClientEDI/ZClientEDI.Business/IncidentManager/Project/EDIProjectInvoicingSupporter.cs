using Enterprise.Security;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDIProjectInvoicingSupporter : ProcessManagement.Business.ProjectInvoicingSupporter
	{
		public EDIProjectInvoicingSupporter(EDIProject parent)
			: base(parent)
		{
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return EDISecurityCheckpoints.InstallationTaskJobInvoicing;
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return EDISecurityCheckpoints.InstallationTaskAuditBilling;
		}
	}
}
