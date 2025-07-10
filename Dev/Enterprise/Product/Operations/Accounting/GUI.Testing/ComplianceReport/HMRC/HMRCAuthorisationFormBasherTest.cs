using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ComplianceReport.HMRC.Testing
{
	[TestedType(typeof(HMRCAuthorisationForm))]
	public class HMRCAuthorisationFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new HMRCAuthorisationForm();

		protected override bool AllowFormSizeFixed => true;

		public void TestHMRCAuthorisationFormControls()
		{
			using var form = new HMRCAuthorisationForm();
			form.Show();

			var promptLabel = form.Controls.Find("zLabelPrompt", searchAllChildren: true).FirstOrDefault() as ZArchitecture.ZLabel;
			AssertNotNull("Prompt Label", promptLabel);
			AssertEquals("Prompt Label Text", ExpectedPromptString, promptLabel.Text);

			var cancelButton = form.Controls.Find("zButtonCancel", searchAllChildren: true).FirstOrDefault() as ZArchitecture.GUI.ZButton;
			AssertNotNull("Cancel Button", cancelButton);
			AssertEquals("Cancel Button Text", "Cancel", cancelButton.Text);
			AssertEquals("Default Cancel Button", cancelButton, form.CancelButton);

			var connectButton = form.Controls.Find("zButtonConnect", searchAllChildren: true).FirstOrDefault() as ZArchitecture.GUI.ZButton;
			AssertNotNull("Connect Button", connectButton);
			AssertEquals("Connect Button Text", "Connect with HMRC...", connectButton.Text);
			AssertEquals("Default Accept Button", connectButton, form.AcceptButton);

			form.CancelButton.PerformClick();
			AssertEquals(DialogResult.Cancel, form.DialogResult);
		}

		const string ExpectedPromptString = """
			CargoWise needs your permission to interact with HMRC web services on your behalf.

			Click "Connect with HMRC" button to open "Allow your software to connect with HMRC" web page in your browser.
			Once the page loads, follow the prompts to authorize CargoWise and return to this screen when done.
			""";
	}
}
