using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseAdjustmentLineWrapperCollection))]
	sealed class WarehouseAdjustmentLineWrapperCollectionTest : WarehouseGenericWrapperCollectionTest<WarehouseAdjustmentLineWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WarehouseAdjustmentLineWrapper(Factory.NewWithValidTestData<WhsAdjustmentLine>(), Factory);
		}

		protected override WarehouseAdjustmentLineWrapperCollection GetNewDocumentWrapperCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
			return new WarehouseAdjustmentLineWrapperCollection(adjustment.Lines, Factory);
		}
	}
}
