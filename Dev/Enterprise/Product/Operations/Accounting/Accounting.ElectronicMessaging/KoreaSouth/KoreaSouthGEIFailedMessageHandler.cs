using System.Linq;
using CargoWise.Common;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class KoreaSouthGEIFailedMessageHandler : GEIFailedMessageHandler, IFailedMessageHandler
	{
		public override void UpdatePivot(AccEInvoicingTransactionPivot pivot)
		{
			var finder = new KoreaSouthEInvoicingDataFinder();

			var submitBatch = finder.FindSubmitBatch(pivot.Batch);
			UpdateSubmitBatchPivots(submitBatch, pivot.AIP_ActionType);

			var statusCheckBatch = finder.FindStatusCheckBatch(pivot.Batch);
			statusCheckBatch?.TransactionPivots.DeleteAll();
		}

		void UpdateSubmitBatchPivots(AccEInvoicingBatch submitBatch, string actionType)
		{
			submitBatch?.TransactionPivots.OfType<AccEInvoicingTransactionPivot>().ForEach(x =>
			{
				x.AIP_Status = EInvoicingPivotState.Failed;
				x.AIP_ErrorDescription = actionType == EInvoicingPivotActionType.Submit ? ErrorMessageForSUB : ErrorMessageForSTA;
			});
		}

		string ErrorMessageForSUB => Res.GetString("2539463B-3923-4236-AD85-839228398183", "Unexpected error when sending the 'GEN - Generate Invoice Submission' type Message. Please check the Notes 'eHub Server Error' on this EDI Message for more details");

		string ErrorMessageForSTA => Res.GetString("1BA39E2B-1642-4C2A-A0E2-67F222DBEAFA", "Unexpected error when sending the 'GEQ - Query Invoice Request' type Message.Please check the Notes 'eHub Server Error' on this EDI Message for more details.");
	}
}
