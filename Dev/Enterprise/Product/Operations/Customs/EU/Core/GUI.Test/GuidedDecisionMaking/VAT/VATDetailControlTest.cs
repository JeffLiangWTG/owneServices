using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class VATDetailControlTest : TestCaseWithFactory
	{
		public void TestVATDetailLabelText()
		{
			var gdmBasic = VATContainerPanelTest.SetUpGDMBasicWithVATForTest(Factory);
			var vat1 = gdmBasic.VATApplicabilities[0];

			using (var control = new VATDetailControl(vat1))
			{
				var label = control.FindSingle<ZLabel>("VATDetailLabel");
				AssertNotNull(label);
				AssertEquals("RED - RED DESC - 3.30%\r\nVAT1 - VAT1 Desc\r\nA001", label.Text);
			}
		}
	}
}
