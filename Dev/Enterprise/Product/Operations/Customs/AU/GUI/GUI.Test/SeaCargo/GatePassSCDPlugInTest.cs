using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class GatePassSCDPlugInTest : BaseSCDPlugInTest
	{
		GatePassShipment gatePassShipment;
		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			base.SetUp();
			gatePassShipment = Factory.New<GatePassShipment>();
		}

		protected override SCDPlugIn GetPlugIn() => new SCDGatePassPlugIn(gatePassShipment);
	}
}
