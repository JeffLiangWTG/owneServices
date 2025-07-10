using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	[TestedType(typeof(ScanForHeldShipmentsWizard))]
	sealed class ScanForHeldShipmentsWizardTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new ScanForHeldShipmentsWizard(new AirScanForOutturnHeldShipmentManager(Factory));
	}
}
