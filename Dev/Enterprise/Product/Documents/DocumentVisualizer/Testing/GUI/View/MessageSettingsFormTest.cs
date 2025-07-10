using System.Windows.Forms;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	[TestedType(typeof(MessageSettingsForm))]
	sealed class MessageSettingsFormTest : ZFormBasherTest
	{
		public void TestFormText()
		{
			using (var form = new MessageSettingsForm(Factory, DummyWorkflowDescriptor.Instance.Code))
			{
				form.Show();
				AssertEquals("form.Text", "Select Message Settings", form.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new MessageSettingsForm(Factory, DummyWorkflowDescriptor.Instance.Code);
		}

		protected override void SetUp()
		{
			base.SetUp();

			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MasterFiles.Business.MessageRecipientPartyType.OrgProxy;
		}
	}
}
