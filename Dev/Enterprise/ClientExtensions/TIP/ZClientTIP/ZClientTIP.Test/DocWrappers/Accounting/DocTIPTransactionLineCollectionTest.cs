using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Client.TIP.DocWrappers
{
	[TestedType(typeof(DocTIPHeaderLineTransactionCollection))]
	public class DocTIPTransactionLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocTIPHeaderLineTransactionCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			APInvoiceLine line = Factory.New<APInvoiceLine>();
			return DocTIPHeaderLineTransaction.New(line, Factory);
		}

		protected override DocTIPHeaderLineTransactionCollection GetCollectionToTest()
		{
			return new DocTIPHeaderLineTransactionCollection(Factory);
		}
	}
}
