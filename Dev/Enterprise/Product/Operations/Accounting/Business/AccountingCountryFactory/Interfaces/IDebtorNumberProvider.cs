using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IDebtorNumberProvider
	{
		ZString GetDebtorName(AccTransactionHeaderAuthorisationRecord transactionHeaderAuthorisationRecord);
	}
}
