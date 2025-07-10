#if DEBUG

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class Invoice
	{
		public ReceiptPayment.ReceiptPaymentBase NewReceiptPayment_ForTestOnly => NewReceiptPayment;

		public ZQuery BankAccountCollectionFilter_ForTestOnly => BankAccountCollectionFilter;

		public ZQuery CheckBookCollectionFilter_ForTestOnly => CheckBookCollectionFilter;

		public ZGuid DefaultBankAccount_ForTestOnly => DefaultBankAccount;

		public ReceiptPayment.ReceiptPaymentBase CreateReceiptPayment_ForTestOnly()
		{
			return CreateReceiptPayment();
		}

		public void ClearReceiptPaymentFields_ForTestOnly()
		{
			ClearReceiptPaymentFields();
		}

		public void JobRelatedLogicOnCopiedLine_ForTestOnly(InvoiceLine oldLine, InvoiceLine newLine)
		{
			JobRelatedLogicOnCopiedLine(oldLine, newLine);
		}
	}
}

#endif
