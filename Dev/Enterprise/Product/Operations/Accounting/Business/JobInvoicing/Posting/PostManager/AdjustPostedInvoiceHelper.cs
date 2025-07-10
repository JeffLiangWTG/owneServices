using CargoWise.Application;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IAdjustPostedInvoiceHelper
	{
		void AdjustPostedInvoice(InvoicingBase invoicingBase);
	}

	public class AdjustPostedInvoiceHelper : IAdjustPostedInvoiceHelper
	{
		public AdjustPostedInvoiceHelper()
		{
			invoiceRoundingLineCreator_constructorInitializedOnly = new InvoiceRoundingLineCreator();
		}

		IInvoiceRoundingLineCreator InvoiceRoundingLineCreator => invoiceRoundingLineCreator_constructorInitializedOnly;
		IInvoiceRoundingLineCreator invoiceRoundingLineCreator_constructorInitializedOnly;

		void IAdjustPostedInvoiceHelper.AdjustPostedInvoice(InvoicingBase invoice)
		{
			var surchargeLineCreator = ObjectFactory.Get<ISurchargeLineCreator>();

			surchargeLineCreator.AddSurchargeLine(invoice);
			InvoiceRoundingLineCreator.AddRoundingLine(invoice);
		}

#if DEBUG
		public void SubstituteInvoiceRoundingLineCreator_ForTestOnly(IInvoiceRoundingLineCreator replacement) => invoiceRoundingLineCreator_constructorInitializedOnly = replacement;
		public IInvoiceRoundingLineCreator InvoiceRoundingLineCreator_ExposedForTestOnly => InvoiceRoundingLineCreator;
#endif
	}
}
