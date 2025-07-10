using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Uruguay
{
	class UruguayEInvoicingPivotsToRequeueInfo : IEInvoicingPivotsToRequeueFilterProvider
	{
		public Func<AccEInvoicingTransactionPivot, bool> GetPivotsToRequeueFilter(BusinessObjectFactory factory)
		{
			return null;
		}

		public string GetPivotsToRequeueMessage()
		{
			return Res.GetString("5C620B7F-ED1D-4D9B-9ED5-F663E24BFC74",
				@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:
- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights), or
- 'DLV' - Delivered (when you have appropriate security rights).");
		}
	}
}
