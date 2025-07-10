using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WhsItemReceiveTransportationUnitWrapperCollection))]
	sealed class WhsItemReceiveTransportationUnitWrapperCollectionTest : GenericWrapperCollectionTest<WhsItemReceiveTransportationUnitWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WhsItemReceiveTransportationUnitWrapper(Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>(), Factory);
		}

		protected override WhsItemReceiveTransportationUnitWrapperCollection GetNewDocumentWrapperCollection()
		{
			var rtu = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			var asn = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			return new WhsItemReceiveTransportationUnitWrapperCollection(new[] { rtu.PK }, new[] { asn.PK }, Factory);
		}
	}
}
