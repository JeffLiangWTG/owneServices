using Enterprise.Customs.KR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.CommercialInvoice.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm() => new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());
	}
}
