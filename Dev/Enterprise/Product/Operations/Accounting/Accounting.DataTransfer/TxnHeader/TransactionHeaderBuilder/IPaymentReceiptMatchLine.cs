using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public interface IPaymentReceiptMatchLine
	{
		ZString Ledger { get; }
		ZString TransactionType { get; }
		ZString TransactionNumber { get; }
		ZString OSCurrencyCode { get; }
		decimal AmountPaid { get; }
		ZString OrganizationCode { get; }
		ZString PaymentReference { get; }
		ZString Description { get; }
		ZDateTime InvoiceDate { get; }
		ZDateTime PostDate { get; }
		ZDateTime DueDate { get; }
		ZString LocalCurrencyCode { get; }
		decimal AmountPaidInLocalCurrency { get; }
		ZString MatchStatus { get; }
		ZString MatchStatusReasonCode { get; }
	}
}
