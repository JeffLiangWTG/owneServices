using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IBatchQueueInvoicesForEInvoicingProvider
	{
		IEnumerable<AccTransactionHeader> GetTransactionsToBeQueued(BusinessObjectFactory factory, ZGuid companyPK, DateTime startDate, int batchSize);
	}
}
