using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.GUI.EInvoicing;
using Enterprise.Accounting.GUI.EInvoicing.HardwareTokenSigning;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing.EInvoicing
{
	[TestedType(typeof(ESigningForm))]
	class ESigningFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ESigningForm(new ESigningBusinessObject(null, null, null));
		}

		public void TestPinTextBoxEnableness()
		{
			var eSigningBusinessObject = new ESigningBusinessObject(null, null, null);
			using (var signForm = new ESigningForm(eSigningBusinessObject))
			{
				signForm.Show();
				var pinTextBox = signForm.Controls.Find("PinTextBox", true).Single() as ZTextBox;
				AssertNotNull("Pin Text Box", pinTextBox);
				eSigningBusinessObject.ChipsetType = ESigningBusinessObject.WindowsToken;
				Assert("Pin Text Box should be disabled", !pinTextBox.Enabled);
				eSigningBusinessObject.ChipsetType = "EPASS2003";
				Assert("Pin Text Box should be enabled", pinTextBox.Enabled);
			}
		}
	}
}
