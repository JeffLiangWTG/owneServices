using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Warehouse;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WhsItemTransferLineWrapperCollection))]
	sealed class WhsItemTransferLineWrapperCollectionTest : GenericWrapperCollectionTest<WhsItemTransferLineWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WhsItemTransferLineWrapper(Factory.NewWithValidTestData<WhsItemTransferLine>(), Factory);
		}

		protected override WhsItemTransferLineWrapperCollection GetNewDocumentWrapperCollection()
		{
			var transferHeader = Factory.NewWithValidTestData<WhsItemTransferHeader>();
			transferHeader.Lines.AddNew();

			return new WhsItemTransferLineWrapperCollection(transferHeader.Lines, Factory);
		}
	}
}
