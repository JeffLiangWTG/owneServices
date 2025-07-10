using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class KoreaSouthEInvoicingSubmitRequestBatchCreator : KoreaSouthEInvoicingBatchCreator
	{
		public KoreaSouthEInvoicingSubmitRequestBatchCreator(GlbCompany company) : base(company)
		{
		}

		public override string ActionType => EInvoicingPivotActionType.Submit;
	}
}
