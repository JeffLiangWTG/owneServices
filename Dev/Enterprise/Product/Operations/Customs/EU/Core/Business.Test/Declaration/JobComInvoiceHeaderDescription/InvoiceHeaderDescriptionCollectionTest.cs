using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceHeaderDescriptionCollection))]
	public class InvoiceHeaderDescriptionCollectionTest : CusCodeDataCollectionTest<InvoiceHeaderDescription>
	{
		protected override CusCodeDataCollection<InvoiceHeaderDescription> GetCusCodeDataCollection() => GetNewDecriptionCollection();

		protected virtual InvoiceHeaderDescriptionCollection GetNewDecriptionCollection()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			return new InvoiceHeaderDescriptionCollection(invoice);
		}
	}
}
