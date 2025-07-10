using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(DeclarationAgreementForm))]
	sealed class DeclarationAgreementFormTest : ZFormBasherTest
	{
		public void TestSendButton()
		{
			using (var form = new DeclarationAgreementForm(new COLSDeclarationAcceptance()))
			{
				form.DeclarationAgreementControl.AcceptCheckBox.Checked = true;
				AssertEquals("Send button is enabled", true, form.SendButton.Enabled);

				form.DeclarationAgreementControl.AcceptCheckBox.Checked = false;
				AssertEquals("Send button is disabled", false, form.SendButton.Enabled);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new DeclarationAgreementForm(new COLSDeclarationAcceptance());
		}
	}
}
