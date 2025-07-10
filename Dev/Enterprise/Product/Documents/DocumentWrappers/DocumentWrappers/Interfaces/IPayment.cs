using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public interface IPayment
	{
		ZDecimal OSTotalForRemittanceAdvice { get; }
		ZDecimal InvoiceAmountForPaymentVoucher { get; }
		ZDecimal RemittanceExchangeRate { get; }
		ZInt NumberOfDifferentCurrencies { get; }
		ZBool IsReversal { get; }
		ZBool ShowOriginalAmount { get; }
		ZBool PrintRemittanceOnCheque { get; }
		DocCurrency PaymentCurrency { get; }
		DocCheque Cheque { get; }
		ZString ChequePayTo { get; }
		ZString PaymentTransactionSummary { get; }
	}
}