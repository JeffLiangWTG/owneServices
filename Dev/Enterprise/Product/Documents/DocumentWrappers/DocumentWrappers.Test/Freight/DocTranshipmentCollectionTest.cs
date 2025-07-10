using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocTranshipmentCollection))]
	sealed class DocTranshipmentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocTranshipmentCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var forwardingShipment = Factory.New<ForwardingShipment>();
			var forwardingConsol = Factory.New<ForwardingConsol>();
			return new DocTranshipment(DocForwardingConsol.New(forwardingConsol, Factory), DocForwardingShipment.New(forwardingShipment, Factory));
		}

		protected override DocTranshipmentCollection GetCollectionToTest()
		{
			return new DocTranshipmentCollection(Factory);
		}
	}
}
