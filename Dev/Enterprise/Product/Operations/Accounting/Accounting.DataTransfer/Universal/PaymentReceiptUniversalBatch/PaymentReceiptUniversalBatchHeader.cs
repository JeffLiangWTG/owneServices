using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class PaymentReceiptUniversalBatchHeader : IPaymentReceiptHeader
	{
		public PaymentReceiptUniversalBatchHeader(TransactionInfo receiptPayment, OrgAddress orgAddress)
		{
			Argument.NotNull(receiptPayment, nameof(receiptPayment));
			ReceiptPayment = receiptPayment;
			OrgAddress = orgAddress;
		}

		readonly TransactionInfo ReceiptPayment;
		readonly OrgAddress OrgAddress;

		#region IPaymentReceiptHeader

		public ZString Type => ReceiptPayment.TransactionType.GetValueOrDefault().ToString();

		public ZString Ledger => ReceiptPayment.Ledger.GetValueOrDefault();

		public ZString TransactionType => ReceiptPayment.TransactionType.GetValueOrDefault().ToString();

		public ZString OrganizationCode => OrgAddress?.Header?.OH_Code ?? ZString.Empty;

		public ZString Description => ReceiptPayment.Description.GetValueOrDefault();

		public ZString ReceiptPaymentType => ReceiptPayment.PaymentOrReceiptType.GetValueOrDefault().ToString();

		public ZString BankAccountCode => ReceiptPayment.BankAccount.GetValueOrDefault();

		public ZString CheckBookCode => ReceiptPayment.CheckBookCode.GetValueOrDefault();

		public ZString CheckOrReference => ReceiptPayment.CheckNumberOrPaymentRef.GetValueOrDefault();

		public ZString OSCurrencyCode => ReceiptPayment.OSCurrency?.Code.GetValueOrDefault() ?? ZString.Empty;

		public ZString LocalCurrencyCode => ReceiptPayment.LocalCurrency?.Code.GetValueOrDefault() ?? ZString.Empty;

		public ZDecimal OSTotal => ReceiptPayment.OSTotal.GetValueOrDefault();

		public ZDecimal OSExGSTVATAmount => ReceiptPayment.OSExGSTVATAmount.GetValueOrDefault();

		public ZDecimal LocalTotal => ReceiptPayment.LocalTotal.GetValueOrDefault();

		public ZDecimal LocalExVATAmount => ReceiptPayment.LocalExVATAmount.GetValueOrDefault();

		public ZString BranchCode => ReceiptPayment.Branch?.Code.GetValueOrDefault() ?? ZString.Empty;

		public ZString DepartmentCode => ReceiptPayment.Department?.Code.GetValueOrDefault() ?? ZString.Empty;

		public ZString CheckDrawer => ReceiptPayment.CheckDrawer.GetValueOrDefault();

		public ZString DrawerBank => ReceiptPayment.DrawerBank.GetValueOrDefault();

		public ZString DrawerBranch => ReceiptPayment.DrawerBranch.GetValueOrDefault();

		public ZString OverrideAddress => ZString.Empty;

		public ZString OverrideContact => ZString.Empty;

		public bool IsContainOverrideAddress => false;

		public bool IsContainOverrideContact => false;

		public ZDateTime InvoiceDate => ReceiptPayment.TransactionDate.GetValueOrDefault();

		public ZDateTime PostDate => ReceiptPayment.PostDate.GetValueOrDefault();

		public ZString ThirdPartyReference => ReceiptPayment.OrganizationsTransactionID.GetValueOrDefault();

		#endregion
	}
}
