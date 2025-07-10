using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Testing
{
	[TestedType(typeof(JASForwardingConsolShipmentCollection))]
	internal class JASForwardingConsolShipmentCollectionTest : ConsolShipmentCollectionBOCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			return consol.Shipments;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(JASForwardingShipment));
		}
	}
}
