using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITransactionWithLinesForDisplayOtherTaxes
	{
		AccTransactionHeader Transaction { get; }
		IEnumerable<TransactionLineForOtherTaxesDisplay> Lines { get; }
		bool IsTaxTransactionsCalculatedBeforePosting { get; }
		event EventHandler OnOtherTaxesCalculatedBeforePosting_Changed;
		bool ShouldShowTransactionLines { get; }
	}
}