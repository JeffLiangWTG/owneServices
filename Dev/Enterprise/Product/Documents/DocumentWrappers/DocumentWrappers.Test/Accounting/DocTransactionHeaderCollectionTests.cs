using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocTransactionHeaderCollection))]
	public class DocTransactionHeaderCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocTransactionHeaderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var aRInvoice = Factory.New<ARInvoice>();
			return DocTransactionHeader.New(aRInvoice, Factory);
		}

		protected override DocTransactionHeaderCollection GetCollectionToTest()
		{
			return new DocTransactionHeaderCollection(Factory);
		}
	}
}
