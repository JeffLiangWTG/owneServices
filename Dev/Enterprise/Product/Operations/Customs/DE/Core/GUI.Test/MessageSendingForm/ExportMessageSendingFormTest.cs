using System.Windows.Forms;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ExportMessageSendingForm))]
	sealed class ExportMessageSendingFormTest : ZFormBasherTest
	{
		public void TestWarningsStillAllowSend()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OwnerRef = string.Empty;
			declaration.CustomsEntryHeaders.AddNew().CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;
			declaration.CustomsEntryHeaders.AddNew().CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;

			var parent = new ExportDeclarationMessageSendingActionParent(declaration);
			parent.SendingObjectsCollection[0].ShouldSend = true;
			using (var form = new ExportMessageSendingForm(parent))
			{
				form.Show();
				var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWithValidationErrorsCheckBox.Checked = true;
				AssertHasWarnings(declaration.JE_OwnerRefInfo);
				AssertEquals("Even with warnings, send should be enabled", true, form.FindSingle<ZButton>("SendButton").Enabled);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();

			var parent = new ExportDeclarationMessageSendingActionParent(declaration);
			return new ExportMessageSendingForm(parent);
		}
	}
}
