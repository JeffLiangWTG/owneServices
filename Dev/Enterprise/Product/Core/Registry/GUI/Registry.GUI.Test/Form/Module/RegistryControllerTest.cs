using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RegistryController))]
	sealed class RegistryControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Registry;
		}

		public void TestDisplayModeForNew()
		{
			AssertEquals(ODisplayMode.NewSaved, Controller.ShowNewForm().DisplayMode);
		}

		public void TestNoMessageWhenCloseWithNoChanges()
		{
			ZForm form = Controller.ShowNewForm() as ZForm;
			AssertNotNull(form);
			Assert("Has no changes", !form.BusinessEntity.HasChanges);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			form.Close();
			Assert("No message should be shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestFormReadOnly()
		{
			Env.Security.SystemRegistryEdit.IsAllowed = false;
			using (RegistryForm form = Controller.ShowNewForm() as RegistryForm)
			{
				Assert("Form should be read only if security checkpoint is denied", form.IsFormReadOnly);
			}

			Env.Security.SystemRegistryEdit.IsAllowed = true;
			using (RegistryForm form = Controller.ShowNewForm() as RegistryForm)
			{
				Assert("Form should be not read only if security checkpoint is granted", !form.IsFormReadOnly);
			}
		}
	}
}
