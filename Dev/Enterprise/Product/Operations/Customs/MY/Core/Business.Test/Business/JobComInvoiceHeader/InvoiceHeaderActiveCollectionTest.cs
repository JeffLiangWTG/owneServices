using NUnit.Framework;

namespace Enterprise.Customs.MY.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	class InvoiceHeaderActiveCollectionTest : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
	{
	}
}
