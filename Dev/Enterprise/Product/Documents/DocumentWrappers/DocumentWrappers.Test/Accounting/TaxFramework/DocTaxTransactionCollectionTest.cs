using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.TaxFramework.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocTaxTransactionCollection))]
	sealed class DocTaxTransactionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocTaxTransactionCollection>
	{
		protected override DocTaxTransactionCollection GetCollectionToTest()
		{
			return DocTaxTransactionCollection.New(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocTaxTransaction.New(Factory.New<AccTaxTransaction>(), Factory);
		}
	}
}
