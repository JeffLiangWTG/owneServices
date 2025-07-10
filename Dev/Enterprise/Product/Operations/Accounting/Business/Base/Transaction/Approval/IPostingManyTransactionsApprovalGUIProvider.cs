using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public interface IPostingJobTransactionsApprovalGUIProvider : IPostingTransactionApprovalGUIProvider
	{
		bool IsForPreviewOnly { get; }

		APInvoiceChargesApprovalRequest RequestToCompare { get; }

		ZDialogResult ShowPostingConfirmationForm(APInvoiceCharges[] apInvoiceCharges);
	}
}
