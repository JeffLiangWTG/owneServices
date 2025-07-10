using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocTransportCollection))]
	sealed class DocTransportCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocTransportCollection>
	{
		protected override DocTransportCollection GetCollectionToTest()
		{
			return new DocTransportCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Transport transport = shipment.Transports.AddNew();
			return DocTransport.New(shipment, transport, Factory);
		}
	}
}
