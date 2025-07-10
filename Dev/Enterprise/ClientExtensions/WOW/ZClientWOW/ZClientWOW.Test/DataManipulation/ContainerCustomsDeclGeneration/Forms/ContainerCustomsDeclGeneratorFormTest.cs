using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow
{
	[TestedType(typeof(ContainerCustomsDeclGeneratorForm))]
	public class ContainerCustomsDeclGeneratorFormTest : ZFormBasherTest
	{
		[GuiTest]
		public void TestRegistryIsNotSetup()
		{
			AssertEquals("registry is not set up", Guid.Empty, WowDataRegistry.Instance.DeclarationImporter);
			using (ContainerCustomsDeclGeneratorFormForTest form = new ContainerCustomsDeclGeneratorFormForTest())
			{
				UnitTestUserNotification userNotify = UnitTestUserNotification.Instance;
				userNotify.ClearMessagesAndAnswers();
				form.Execute();
				AssertContains("progress textbox should contains error message", ContainerCustomsDeclGeneratorForm.RegistryErrorMessage, form.ProgressTextBox.Text);
			}

			WowDataRegistry.Instance.DeclarationImporter = Guid.NewGuid();
			using (ContainerCustomsDeclGeneratorFormForTest form = new ContainerCustomsDeclGeneratorFormForTest())
			{
				UnitTestUserNotification userNotify = UnitTestUserNotification.Instance;
				userNotify.ClearMessagesAndAnswers();
				form.Execute();
				AssertNotEquals("progress textbox should contains error message", ContainerCustomsDeclGeneratorForm.RegistryErrorMessage, form.ProgressTextBox.Text);
			}
		}

		class ContainerCustomsDeclGeneratorFormForTest : ContainerCustomsDeclGeneratorForm
		{
			public new void Execute()
			{
				base.Execute();
			}

			public new Enterprise.Client.Wow.ThreadSafeTextBox ProgressTextBox
			{
				get
				{
					return base.ProgressTextBox;
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new ContainerCustomsDeclGeneratorForm();
		}
	}
}
