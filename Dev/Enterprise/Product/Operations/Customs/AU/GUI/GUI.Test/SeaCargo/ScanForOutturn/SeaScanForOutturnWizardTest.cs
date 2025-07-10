using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	[TestedType(typeof(SeaScanForOutturnWizard))]
	sealed class SeaScanForOutturnWizardTest : ScanForOutturnWizardTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			return new SeaScanForOutturnWizard(new ScanCusSCAOceanBill(oceanBill));
		}
	}
}
