using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(BrokeragePlugIn))]
sealed class BrokeragePlugInTest : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
{
	public void TestMenuIsCorrectType()
	{
		using (var plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
		{
			AssertType<EDIMenu>(plugin.TopLevelMenu);
		}
	}

	protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);
}
