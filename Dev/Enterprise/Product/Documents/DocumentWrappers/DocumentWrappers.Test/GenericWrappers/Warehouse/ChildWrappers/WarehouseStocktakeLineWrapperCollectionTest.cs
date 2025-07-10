using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse.ChildWrappers;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseStocktakeLineWrapperCollection))]
	sealed class WarehouseStocktakeLineWrapperCollectionTest : GenericWrapperCollectionTest<WarehouseStocktakeLineWrapperCollection>
	{
		protected override WarehouseStocktakeLineWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new WarehouseStocktakeLineWrapperCollection(Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WarehouseStocktakeLineWrapper(Factory.NewWithValidTestData<WhsStocktakeLine>(), Factory);
		}
	}
}
