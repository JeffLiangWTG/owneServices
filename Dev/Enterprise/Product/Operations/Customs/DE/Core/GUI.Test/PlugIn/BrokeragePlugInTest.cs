using System;
using Enterprise.Customs.DE.GUI.PlugIn;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class BrokeragePlugInTest : EU.GUI.Testing.BrokeragePlugInTest
	{
		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);

		protected override Type ExpectedTopLevelMenuType => typeof(EDIMenu);
	}
}
