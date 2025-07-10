using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class ShipmentSCDPlugInTest : BaseSCDPlugInTest
	{
		protected override SCDPlugIn GetPlugIn() => new SCDShipmentPlugIn(Factory.New<CFSShipment>());
	}
}
