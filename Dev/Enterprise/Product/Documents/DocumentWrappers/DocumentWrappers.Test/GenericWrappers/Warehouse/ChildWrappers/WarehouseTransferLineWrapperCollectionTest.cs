using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseTransferLineWrapperCollection))]
	sealed class WarehouseTransferLineWrapperCollectionTest : GenericWrapperCollectionTest<WarehouseTransferLineWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WarehouseTransferLineWrapper(Factory.NewWithValidTestData<WhsTransferLine>(), Factory);
		}

		protected override WarehouseTransferLineWrapperCollection GetNewDocumentWrapperCollection()
		{
			WhsTransfer docket = Factory.NewWithValidTestData<WhsTransfer>();
			WhsTransferLine line = docket.Lines.AddNew();
			return new WarehouseTransferLineWrapperCollection(docket.Lines, Factory);
		}
	}
}
