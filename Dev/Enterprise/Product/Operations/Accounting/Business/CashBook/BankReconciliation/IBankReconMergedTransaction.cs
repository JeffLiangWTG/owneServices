using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook
{
	public interface IBankReconMergedTransaction : IBusiness
	{
		ZDateTime TransactionDate { get; }
		ZPropertyInfo TransactionDateInfo { get; }

		ZDateTime InvoiceDate { get; }
		ZPropertyInfo InvoiceDateInfo { get; }

		ZString Type { get; }
		ZPropertyInfo TypeInfo { get; }

		ZString Method { get; }
		ZPropertyInfo MethodInfo { get; }

		ZString ChequeRef { get; }
		ZPropertyInfo ChequeRefInfo { get; }

		ZString BatchNo { get; }
		ZPropertyInfo BatchNoInfo { get; }

		ZString Payee { get; }
		ZPropertyInfo PayeeInfo { get; }

		ZInt BankCurrencyDecimals { get; }
		ZPropertyInfo BankCurrencyDecimalsInfo { get; }

		ZDecimal Debit { get; }
		ZPropertyInfo DebitInfo { get; }

		ZDecimal Credit { get; }
		ZPropertyInfo CreditInfo { get; }

		ZDecimal StatementDebit { get; }
		ZPropertyInfo StatementDebitInfo { get; }

		ZDecimal StatementCredit { get; }
		ZPropertyInfo StatementCreditInfo { get; }

		ZBool IsCleared { get; set; }
		ZPropertyInfo IsClearedInfo { get; }

		ZString LineType { get; }
		ZPropertyInfo LineTypeInfo { get; }

		ZDateTime ClearedDate { get; set; }
		ZPropertyInfo ClearedDateInfo { get; }

		BankReconciliation MasterBankRecon { get; set; }

		void CreateCustomLog();
	}
}
