using Enterprise.Customs.AsycudaCustoms.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.GUI.CommercialInvoice.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm() => new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());
	}
}
