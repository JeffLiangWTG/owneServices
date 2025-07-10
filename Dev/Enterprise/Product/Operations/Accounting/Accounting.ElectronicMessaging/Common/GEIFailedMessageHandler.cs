using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class GEIFailedMessageHandler : IFailedMessageHandler
	{
		public void UpdateFailedMessage(EDIInterchange interchange)
		{
			var query = new ZDBOnlyQuery(typeof(AccEInvoicingTransactionPivot));

			var batchQuery = new ZDBOnlySubQuery(typeof(AccEInvoicingBatch), AccEInvoicingBatchSchema.PK);

			var messageQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID);
			messageQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, "AccEInvoicingBatch");

			var interchangeQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
			interchangeQuery.AddToFilter(EDIInterchangeSchema.PK, interchange.PK);

			messageQuery.AddSubQuery(EDIMessageSchema.EM_EI, interchangeQuery, JoinCondition.And);
			batchQuery.AddSubQuery(AccEInvoicingBatchSchema.PK, messageQuery, JoinCondition.And);
			query.AddSubQuery(AccEInvoicingTransactionPivotSchema.AIP_AIB, batchQuery, JoinCondition.And);

			var pivot = interchange.Factory.LoadTop1<AccEInvoicingTransactionPivot>(query);
			if (pivot != null)
			{
				UpdatePivot(pivot);
				interchange.Factory.Save();
			}
		}

		public virtual void UpdatePivot(AccEInvoicingTransactionPivot pivot)
		{
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Failed;
			pivot.AIP_ErrorDescription = Res.GetString("FDFE3027-8B32-46C4-B8E2-C0A0441BA0D2",
				"Unexpected error when sending the Interchange. Please check the Notes 'eHub Server Error' on this EDI Interchange for more details.");
		}
	}
}
