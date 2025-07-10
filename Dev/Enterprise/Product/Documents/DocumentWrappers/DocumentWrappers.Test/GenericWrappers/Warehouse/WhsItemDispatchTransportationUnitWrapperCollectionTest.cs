using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WhsItemDispatchTransportationUnitWrapperCollection))]
	sealed class WhsItemDispatchTransportationUnitWrapperCollectionTest : GenericWrapperCollectionTest<WhsItemDispatchTransportationUnitWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WhsItemDispatchTransportationUnitWrapper(Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>(), Factory);
		}

		protected override WhsItemDispatchTransportationUnitWrapperCollection GetNewDocumentWrapperCollection()
		{
			var dll = Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>();
			return new WhsItemDispatchTransportationUnitWrapperCollection(new[] { dll }, Factory);
		}
	}
}
