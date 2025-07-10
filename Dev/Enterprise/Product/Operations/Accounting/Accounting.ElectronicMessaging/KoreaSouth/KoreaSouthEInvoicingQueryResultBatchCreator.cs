using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class KoreaSouthEInvoicingQueryResultBatchCreator : KoreaSouthEInvoicingBatchCreator
	{
		public KoreaSouthEInvoicingQueryResultBatchCreator(GlbCompany company) : base(company)
		{
		}

		public override string ActionType => EInvoicingPivotActionType.StatusCheck;
	}
}
