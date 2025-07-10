using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.DocumentWrappers.Accounting.KR;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.KR.Testing
{
	[TestedType(typeof(DocElectronicInvoiceLineCollection))]
	sealed class DocElectronicInvoiceLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocElectronicInvoiceLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var lineItem = new TaxInvoiceTradeLineItem();
			return DocElectronicInvoiceLine.New(lineItem, Factory);
		}

		protected override DocElectronicInvoiceLineCollection GetCollectionToTest()
		{
			return new DocElectronicInvoiceLineCollection(Factory);
		}
	}
}
