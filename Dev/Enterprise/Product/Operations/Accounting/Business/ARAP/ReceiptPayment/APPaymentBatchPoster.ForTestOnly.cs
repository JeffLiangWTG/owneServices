#if DEBUG

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class APPaymentBatchPoster
	{
		public void RunPreSaveValidationCore_ForTestOnly()
		{
			RunPreSaveValidationCore();
		}

		public void SetDefaultValues_ForTestOnly()
		{
			SetDefaultValues();
		}

		public void ResetPaymentMatchingCollection_ForTestOnly(IMatchingCollection transactionCollection)
		{
			ResetPaymentMatchingCollection(transactionCollection);
		}

		public ZBool InitializeTransactionCollection_ForTestOnly(TransactionHeaderCollection unsortedTransactionCollection)
		{
			return InitializeTransactionCollection(unsortedTransactionCollection);
		}

		public override void ClearPaymentApprovalCollection_ForTestOnly()
		{
			paymentApprovalCollection = null;
		}

		public void SetHasChangesToFalse_ForTestOnly()
		{
			SetHasChangesToFalse();
		}

		public static APPaymentBatchPoster Create_ForTestOnly(BusinessObjectFactory factory, bool groupByInvoicePaymentCriticality, bool groupByInvoiceRelatedDebtorOrganisation, bool groupByUser)
		{
			var poster = factory.New<APPaymentBatchPoster>();
			poster.groupByInvoicePaymentCriticality = groupByInvoicePaymentCriticality;
			poster.groupByInvoiceRelatedDebtorOrganisation = groupByInvoiceRelatedDebtorOrganisation;
			poster.groupByUser = groupByUser;
			return poster;
		}
	}
}

#endif
