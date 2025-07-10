
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public interface IDocPaymentVoucherWithAuthorisationSupport
	{
		ZDateTime InvoiceDate { get; }
		ZString ReceiptType { get; }
		ZString ReceiptTypeDescription { get; }
		ZString ChequeOrReference { get; }
		ZString TransactionType { get; }
		ZString TransactionNumber { get; }
		ZString Desc { get; }
		ZBool ShowNewAuthorisationFooter { get; }
		ZString PreparedBy { get; }
		ZString ApprovalStatus { get; }
		ZString FirstAuthorisationDescription { get; }
		ZString SecondAuthorisationDescription { get; }
		ZString ThirdAuthorisationDescription { get; }
		ZString FirstAuthorisation { get; }
		ZString SecondAuthorisation { get; }
		ZString ThirdAuthorisation { get; }
		ZDecimal ExchangeRate { get; }
		ZDecimal InvoiceAmountForPaymentVoucher { get; }
		ZDecimal RemittanceExchangeRate { get; }
		ZDecimal OSTotalForRemittanceAdvice { get; }
		DocCurrency Currency { get; }
		DocOrganisation Organisation { get; }
		DocBankAccount BankAccount { get; }
		DocumentWrapperCollection Payments { get; }
		DocCurrency PaymentCurrency { get; }
	}
}
