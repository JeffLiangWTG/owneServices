using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class PaymentReceiptUniversalMatchLine : IPaymentReceiptMatchLine
	{
		public PaymentReceiptUniversalMatchLine(MatchLine matchLine, TransactionInfo linkedTransaction, OrgAddress orgAddress, ZString[] splitedKey)
		{
			Argument.NotNull(matchLine, nameof(matchLine));

			MatchLine = matchLine;
			LinkedTransaction = linkedTransaction;
			OrgAddress = orgAddress;
			SetValueFromSplitedKey(splitedKey);
		}

		readonly MatchLine MatchLine;
		readonly TransactionInfo LinkedTransaction;
		readonly OrgAddress OrgAddress;

		void SetValueFromSplitedKey(ZString[] splitedKey)
		{
			if (splitedKey.Length == 3)
			{
				ledger = splitedKey[0];
				transactionType = splitedKey[1];
				transactionNumber = splitedKey[2];
			}
		}

		public ZString Ledger => ledger;
		ZString ledger;

		public ZString TransactionType => transactionType;
		ZString transactionType;

		public ZString TransactionNumber => transactionNumber;
		ZString transactionNumber;

		public ZString OSCurrencyCode => LinkedTransaction?.OSCurrency?.Code.GetValueOrDefault() ?? ZString.Empty;

		public decimal AmountPaid => MatchLine.OSPaidAmount.GetValueOrDefault();

		public ZString OrganizationCode => OrgAddress?.Header?.OH_Code ?? ZString.Empty;

		public ZString PaymentReference => LinkedTransaction?.CheckNumberOrPaymentRef.GetValueOrDefault() ?? ZString.Empty;

		public ZString Description => LinkedTransaction?.Description.GetValueOrDefault() ?? ZString.Empty;

		public ZDateTime InvoiceDate => LinkedTransaction?.TransactionDate.GetValueOrDefault() ?? ZDateTime.Empty;

		public ZDateTime PostDate => LinkedTransaction?.PostDate.GetValueOrDefault() ?? ZDateTime.Empty;

		public ZDateTime DueDate => LinkedTransaction?.DueDate.GetValueOrDefault() ?? ZDateTime.Empty;

		public ZString LocalCurrencyCode => LinkedTransaction?.LocalCurrency?.Code.GetValueOrDefault() ?? ZString.Empty;

		public decimal AmountPaidInLocalCurrency => LinkedTransaction?.LocalTotal.GetValueOrDefault() ?? ZDecimal.Zero;

		public ZString MatchStatus => LinkedTransaction?.MatchStatus?.Code.GetValueOrDefault() ?? ZString.Empty;

		public ZString MatchStatusReasonCode => LinkedTransaction?.MatchStatusReason?.Code.GetValueOrDefault() ?? ZString.Empty;

		public ZString TransactionCategory => LinkedTransaction?.Category.GetValueOrDefault() ?? ZString.Empty;
	}
}
