using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	public class GenericWrapperLoaderGetFromDataContextTest : TestCaseWithFactory
	{
		public void TestGenericWrapperLoadersDictionaryNotNull()
		{
			AssertNotNull(ObjectFactory.Get("GenericWrapperLoaders"));
		}

		public void TestUnsupportedDataContextReturnsDefaultProvider()
		{
			GenericWrapperLoader loader = GenericWrapperLoader.GetFromDataContext(Core.Constants.DataContext.ForwardingShipment);

			AssertNotNull(loader);
			AssertEquals(typeof(EmptyGenericWrapperLoader), loader.GetType());
			AssertNull(loader.GetWrappers(null, null));
			AssertNull(loader.GetWrappers(null, null, null));
			AssertNull(loader.GetWrapperType());
		}

		public void TestGetWrappers()
		{
			GenericWrapperLoader loader = GenericWrapperLoader.GetFromDataContext(Core.Constants.DataContext.GenericFreightJobRouting);
			BusinessObject transport = (BusinessObject)Factory.New<Integration.Freight.ITransport>();
			BusinessObject shipment = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingShipment>();

			DocumentWrapper[] freightJobWrapperArray = loader.GetWrappers(shipment, transport, Factory);
			AssertNotNull("freightJobWrapperArray should not be null", freightJobWrapperArray);
			AssertEquals("freightJobWrapperArray.Length", 1, freightJobWrapperArray.Length);
			DocumentWrapper freightJobWrapper = freightJobWrapperArray[0];
			AssertNotNull("freightJobWrapper should not be null", freightJobWrapper);
			AssertEquals("freightJobWrapper.WrappedObject", shipment, freightJobWrapper.WrappedObject);
		}
	}
}
