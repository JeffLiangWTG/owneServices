using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business
{
	public class CusLPCOHeaderInvoicingSupporter : JobInvoicingSupporter
	{
		public CusLPCOHeaderInvoicingSupporter(IJobHeaderParent parent) : base(parent)
		{
		}

		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.BRLPCO;

		protected override Security.SecurityCheckpoint GetJobInvoicingSecurityCore() => Env.Security.BRLPCOJobInvoicing;
	}
}
