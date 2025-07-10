using CargoWise.Definitions;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.Module.Testing
{
	sealed class ArchiveEDocsModuleTest : TestCaseWithDocumentFactory
	{
		public void TestShow()
		{
			using (var module = new ArchiveEDocsModule())
			{
				var parent = MasterFactory.New<StorageMain>();
				parent.SM_DB = 200; // a ridiculous number that will never be reached...
				MasterFactory.Save();

				module.Show();
				var lastController = ((IPopupModuleInternalsForTesting)module).LastController;

				AssertNull("Last controller is null because the form is not created", lastController);

				parent.Delete();
				MasterFactory.Save();

				module.Show();
				lastController = ((IPopupModuleInternalsForTesting)module).LastController;

				AssertNotNull("Last controller is not null", lastController);

				var archiveEDocsForm = lastController.LastShownForm as ArchiveEDocsForm;

				AssertNotNull("Last shown form is not null and is type of ArchiveEDocsForm", archiveEDocsForm);
				AssertEquals("Create eDocs CD", archiveEDocsForm.Text);

				module.CloseFormForTestingOnly();
			}
		}

		public void TestShow_NotShownForHosted()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals("System should be hosted with CargoWise", expected: true, EnvProxy.IsHostedWithCargowise);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var module = new ArchiveEDocsModule())
			{
				var parent = MasterFactory.New<StorageMain>();
				parent.SM_DB = 200; // a ridiculous number that will never be reached...

				MasterFactory.Save();

				module.Show();
				var lastController = ((IPopupModuleInternalsForTesting)module).LastController;

				AssertNull("Last controller is null because the form is not created", lastController);
				AssertEquals("This functionality is not available for CargoWise Cloud clients.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShow_NotShownForEDI()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("EDIProd should be not hosted with CargoWise", expected: false, EnvProxy.IsHostedWithCargowise);

			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (var module = new ArchiveEDocsModule())
			{
				var parent = MasterFactory.New<StorageMain>();
				parent.SM_DB = 200; // a ridiculous number that will never be reached...

				MasterFactory.Save();

				module.Show();
				var lastController = ((IPopupModuleInternalsForTesting)module).LastController;

				AssertNull("Last controller is null because the form is not created", lastController);
				AssertEquals("This functionality is not available for CargoWise Cloud clients.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
