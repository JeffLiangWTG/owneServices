using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.ContainerYard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(GateTransportCFSDetailWrapperCollection))]
	sealed class GateTransportCFSDetailWrapperCollectionTest : GenericWrapperCollectionTest<GateTransportCFSDetailWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return GetNewTransportDetailWrapper();
		}

		protected override GateTransportCFSDetailWrapperCollection GetNewDocumentWrapperCollection()
		{
			var collection = new GateTransportCFSDetailWrapperCollection(Factory);
			collection.Add(GetNewTransportDetailWrapper());
			return collection;
		}

		GateTransportCFSDetailWrapper GetNewTransportDetailWrapper()
		{
			var gateTransportDetail = Factory.New<GateTransportCFSDetail>();
			return new GateTransportCFSDetailWrapper(gateTransportDetail, Factory);
		}
	}
}
