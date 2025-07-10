using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WhsItemDispatchLoadListWrapperCollection))]
	sealed class WhsItemDispatchLoadListWrapperCollectionTest : GenericWrapperCollectionTest<WhsItemDispatchLoadListWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WhsItemDispatchLoadListWrapper(Factory.NewWithValidTestData<WhsItemDispatchLoadList>(), Factory);
		}

		protected override WhsItemDispatchLoadListWrapperCollection GetNewDocumentWrapperCollection()
		{
			var dll = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			return new WhsItemDispatchLoadListWrapperCollection(new[] { dll }, Factory);
		}
	}
}
