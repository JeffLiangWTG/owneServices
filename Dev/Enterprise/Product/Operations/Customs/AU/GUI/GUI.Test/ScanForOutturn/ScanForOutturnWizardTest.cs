using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(ScanForOutturnWizard))]
	class ScanForOutturnWizardTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new ScanForOutturnWizard(new SeaScanForOutturnManager(new ScanCusSCAOceanBill(Factory.New<CusSCAOceanBill>())));
	}
}
