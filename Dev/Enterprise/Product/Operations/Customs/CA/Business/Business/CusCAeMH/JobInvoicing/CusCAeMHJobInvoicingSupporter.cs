using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHJobInvoicingSupporter : JobInvoicingSupporter
	{
		public CusCAeMHJobInvoicingSupporter(CusCAeMHMaster parent)
			: base(parent)
		{
			this.parent = parent;
		}
		readonly CusCAeMHMaster parent;

		public override ZDateTime ATA
		{
			get { return parent.BP_ATA; }
		}

		public override ZString MasterBillNumber
		{
			get { return parent.BP_MasterBill; }
		}

		public override RefUNLOCO Destination
		{
			get { return parent.DischargePort; }
		}

		public override ZString TransportMode
		{
			get { return parent.BP_ModeOfTransport; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.CAeManifest; }
		}

		protected override Security.SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.ConsolCAeManifestInvoicing;
		}

		protected override Security.SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.ConsolCAeManifestAuditBilling;
		}

		protected override Security.SecurityCheckpoint GetEditSecurityCheckpointCore()
		{
			return Env.Security.None;
		}
	}
}
