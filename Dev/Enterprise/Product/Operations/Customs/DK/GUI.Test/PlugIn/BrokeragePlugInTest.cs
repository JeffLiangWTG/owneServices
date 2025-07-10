namespace Enterprise.Customs.DK.GUI.Testing
{
	sealed class BrokeragePlugInTest : EU.GUI.Testing.BrokeragePlugInTest
	{
		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);
	}
}
