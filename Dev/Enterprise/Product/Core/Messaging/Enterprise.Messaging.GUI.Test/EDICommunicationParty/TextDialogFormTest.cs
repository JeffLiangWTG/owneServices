using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.GUI.Test.KeySetter
{
	[TestedType(typeof(TextDialogForm))]
	public sealed class TextDialogFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new TextDialogForm("Showing CSR", string.Empty);
		}

		public void TestDialogFormSecretLabel()
		{
			using (var form = new TextDialogForm("Showing CSR", string.Empty, TextDialogForm.SecretLabelTypes.CSR))
			{
				var secretLabel = (ZGroupBox)form.Controls.Find("SecretLabel", true).First();
				AssertEquals("CSR", secretLabel.CaptionResourceString.Caption);
			}

			using (var form = new TextDialogForm("Showing Certificate", string.Empty, TextDialogForm.SecretLabelTypes.Certificate))
			{
				var secretLabel = (ZGroupBox)form.Controls.Find("SecretLabel", true).First();
				AssertEquals("Certificate", secretLabel.CaptionResourceString.Caption);
			}
		}
	}
}
