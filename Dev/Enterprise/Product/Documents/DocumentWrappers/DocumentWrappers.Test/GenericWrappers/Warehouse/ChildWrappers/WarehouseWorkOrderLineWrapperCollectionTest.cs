using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse.ChildWrappers;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseWorkOrderLineWrapperCollection))]
	sealed class WarehouseWorkOrderLineWrapperCollectionTest : GenericWrapperCollectionTest<WarehouseWorkOrderLineWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			var workOrder = Factory.New<WhsWorkOrder>();
			var workOrderLine = workOrder.Lines.AddNew();
			var workOrderLineWrapper = new WarehouseWorkOrderLineWrapper(workOrderLine, Factory);
			return workOrderLineWrapper;
		}

		protected override WarehouseWorkOrderLineWrapperCollection GetNewDocumentWrapperCollection()
		{
			var workOrder = Factory.New<WhsWorkOrder>();
			var workOrderLine = workOrder.Lines.AddNew();
			var workOrderLineCollection = workOrder.Lines;
			var workOrderLineCollectionWrapper = new WarehouseWorkOrderLineWrapperCollection(workOrderLineCollection, Factory);
			return workOrderLineCollectionWrapper;
		}
	}
}
