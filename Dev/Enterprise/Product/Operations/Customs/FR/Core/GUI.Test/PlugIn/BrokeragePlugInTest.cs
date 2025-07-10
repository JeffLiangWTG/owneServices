using System;

namespace Enterprise.Customs.FR.GUI.PlugIn.Testing
{
	sealed class BrokeragePlugInTest : EU.GUI.Testing.BrokeragePlugInTest
	{
		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);
		protected override Type ExpectedTopLevelMenuType => typeof(EDIMenu);
	}
}
