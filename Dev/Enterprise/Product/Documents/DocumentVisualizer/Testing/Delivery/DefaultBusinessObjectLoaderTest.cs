using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DefaultBusinessObjectLoaderTest : TestCaseWithFactory
	{
		public void TestLoadBusinessObject_SameFactory()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			Factory.Save();

			Factory.ResetDatabaseLoadCount();

			var loader = new DefaultBusinessObjectLoader();
			var res = loader.LoadBusinessObject(Factory, (IBusiness)shipment);

			AssertNotNull("loaded business object", res);
			AssertEquals("we got the same instance of business object since we're working with the same factory", shipment, res);
			AssertMaxDbHits(0, Factory);
		}

		public void TestLoadBusinessObject_DifferentFactory()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			Factory.Save();

			Factory.ResetDatabaseLoadCount();

			var loader = new DefaultBusinessObjectLoader();

			var otherFactory = new BusinessObjectFactory();
			var res = loader.LoadBusinessObject(otherFactory , (IBusiness)shipment);

			AssertNotNull("loaded business object", res);
			AssertEquals("loaded business object is loaded in the other factory", res.Factory._Instance, otherFactory._Instance);

			//JobShipment: 1
			AssertMaxDbHits(1, otherFactory);
		}

		public void TestLoadBusinessObject_NullFactory()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			Factory.Save();

			Factory.ResetDatabaseLoadCount();

			var loader = new DefaultBusinessObjectLoader();
			var res = loader.LoadBusinessObject(null , (IBusiness)shipment);

			AssertNull("did not load business object", res);
		}

		public void TestLoadBusinessObject_NullBusinessObject()
		{
			var loader = new DefaultBusinessObjectLoader();
			var otherFactory = new BusinessObjectFactory();
			var res = loader.LoadBusinessObject(otherFactory , null);

			AssertNull("did not load business object", res);
		}

		public void TestLoadBusinessObject_DifferentFactory_BusinessObjectNotInDatabase()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();

			Factory.ResetDatabaseLoadCount();

			var loader = new DefaultBusinessObjectLoader();

			var otherFactory = new BusinessObjectFactory();
			var res = loader.LoadBusinessObject(otherFactory , (IBusiness)shipment);

			AssertNotNull("loaded business object", res);
			AssertEquals("loaded business object is loaded in the other factory", res.Factory._Instance, otherFactory._Instance);

			AssertMaxDbHits(0, otherFactory);
		}
	}
}
