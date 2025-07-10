using System;
using Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework;

namespace Enterprise.Accounting.Export.Business.TaxFramework
{
	public class TaxTransactionRow
	{
		public Guid TransactionHeaderPK { get; set; }
		public TaxTransaction TaxTransaction { get; set; }
	}
}
