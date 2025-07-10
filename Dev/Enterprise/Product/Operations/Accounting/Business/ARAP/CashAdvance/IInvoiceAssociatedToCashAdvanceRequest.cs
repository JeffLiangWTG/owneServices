using System.Collections.Generic;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public interface IInvoiceAssociatedToCashAdvanceRequest
	{
		void Accept(ICashAdvanceRequestProcessingByInvoiceVisitor visitor);

		bool IsOutstandingAmountPaidOnlyViaCashAdvance();

		void SetCashAdvanceMatchDetails(List<CashAdvanceMatchingTransactionDetail> cahMatchingTransactionDetails);
	}
}
