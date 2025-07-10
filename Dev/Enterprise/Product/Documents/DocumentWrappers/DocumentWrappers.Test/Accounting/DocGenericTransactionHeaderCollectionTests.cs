using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocGenericTransactionHeaderCollection))]
	public class DocGenericTransactionHeaderCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocGenericTransactionHeaderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var aRInvoice = Factory.New<ARInvoice>();
			return DocGenericTransactionHeader.New(aRInvoice, Factory);
		}

		protected override DocGenericTransactionHeaderCollection GetCollectionToTest()
		{
			return new DocGenericTransactionHeaderCollection(Factory);
		}
	}
}
