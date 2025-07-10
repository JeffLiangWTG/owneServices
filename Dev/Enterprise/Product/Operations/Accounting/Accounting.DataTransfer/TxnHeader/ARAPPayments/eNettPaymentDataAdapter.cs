using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class eNettPaymentDataAdapter : ARAPPaymentDataAdapter
	{
		protected override bool ShouldIncludeThisTransaction(ZString transactionType)
		{
			return transactionType == TransactionTypes.Invoice;
		}
	}
}
