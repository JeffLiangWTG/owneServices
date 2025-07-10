using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Netting;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocNettingMatchedInvoiceCollection))]
	sealed class DocNettingMatchedInvoiceCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocNettingMatchedInvoiceCollection>
	{
		protected override DocNettingMatchedInvoiceCollection GetCollectionToTest()
		{
			return DocNettingMatchedInvoiceCollection.New(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocNettingMatchedInvoice.New(new NettingMatchedInvoice(Factory), Factory);
		}
	}
}
