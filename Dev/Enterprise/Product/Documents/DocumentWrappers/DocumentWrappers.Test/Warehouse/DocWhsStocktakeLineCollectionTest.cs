using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsStocktakeLineCollection))]
	sealed class DocWhsStocktakeLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsStocktakeLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocWhsStocktakeLine.New(Factory.New<WhsStocktakeLine>(), Factory);
		}

		protected override DocWhsStocktakeLineCollection GetCollectionToTest()
		{
			return new DocWhsStocktakeLineCollection(Factory);
		}
	}
}
