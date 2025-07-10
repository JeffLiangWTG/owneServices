using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(GUI.CommercialInvoice.CommercialInvoiceForm))]
	sealed class EuCommercialInvoiceFormBasher : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm()
		{
			// CommercialInvoiceForm needs to be passed an invoice in its constructor.
			// We need a declaration so that we know which flavour (ie for which country) of commerical invoice we need to make.
			JobDeclaration dec = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceWithWhichToBash = dec.Invoices.AddNew();
			invoiceWithWhichToBash.JobComInvoiceLines.AddNew();
			return new GUI.CommercialInvoice.CommercialInvoiceForm(invoiceWithWhichToBash);
		}
	}
}
