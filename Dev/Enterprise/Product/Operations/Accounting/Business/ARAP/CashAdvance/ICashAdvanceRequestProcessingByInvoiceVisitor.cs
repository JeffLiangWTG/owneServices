using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public interface ICashAdvanceRequestProcessingByInvoiceVisitor
	{
		void Visit(ARInvoice arInvoice);

		void Visit(APInvoice apInvoice);
	}
}
