using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IAPPaymentApprovalAmountUpdater
	{
		void UpdateAmountsOnAllPaymentsAndInvoiceLinks(TransactionCreatorHashtable transactions);
	}
	class APPaymentApprovalAmountUpdater : IAPPaymentApprovalAmountUpdater
	{
		void IAPPaymentApprovalAmountUpdater.UpdateAmountsOnAllPaymentsAndInvoiceLinks(TransactionCreatorHashtable transactions)
		{
			var paymentApprovals = transactions.GetAllAPPaymentApprovals();
			foreach (var paymentApproval in paymentApprovals)
			{
				ZDecimal osTotal = default, localTotal = default;
				var paymentApprovalItems = paymentApproval.Factory.Load<PaymentApprovalItem>(new PaymentApprovalItemCollection(paymentApproval).CompleteFilter);
				foreach (var item in paymentApprovalItems)
				{
					var transaction = item.Header;
					if (transaction != null) //null happens when AP Credit Note creator deletes AP invoice linked to this item. At the moment of writing we have UT like this and Consol Cost AP CreditNote posting allows this. Such posting is prevented eventually.
					{
						PaymentApprovalItemOSAmountProvider.SetPaymentAmounts(item, transaction.AH_LocalTotal, transaction.AH_OSTotal);
						osTotal += transaction.AH_OSTotalAmount;
						localTotal += transaction.AH_LocalTotalAmount;
					}
				}

				paymentApproval.AV_Amount = osTotal;
				paymentApproval.AV_Calc_LocalAmount = localTotal;

				if (paymentApproval is PaymentApprovalWithAuthorisation paymentApprovalWithAuthorisation)
				{
					paymentApprovalWithAuthorisation.FullyApprove();
				}
			}
		}
	}
}
