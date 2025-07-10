using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceFormLayoutProvider))]
	internal class CommercialInvoiceFormLayoutProviderTest : CommercialInvoiceFormLayoutProviderAbstractTest<CommercialInvoiceFormLayoutProvider, BaseJobComInvoiceHeader>
	{
		protected override Type ExpectedCommercialInvoiceHeaderDetailsLayoutType => typeof(GUI.CommercialInvoice.InvoiceHeaderDetailsLayout);
	}
}
