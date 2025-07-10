using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IEInvoicingPivotsToRequeueFilterProvider
	{
		Func<AccEInvoicingTransactionPivot, bool> GetPivotsToRequeueFilter(BusinessObjectFactory factory);

		string GetPivotsToRequeueMessage();
	}
}
