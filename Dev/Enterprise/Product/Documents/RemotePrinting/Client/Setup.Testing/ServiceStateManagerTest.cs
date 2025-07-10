using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using Enterprise.RemotePrinting.Client.Setup.Custom;
using Moq;
using NUnit.Framework;

using Constants = Enterprise.RemotePrinting.Client.Constants;

namespace Setup.Testing
{
	[TestRequiresAdministrativePrivileges("Requires admin rights to install a Windows Service")]
	public class ServiceStateManagerTest : TestCase
	{
		public void TestSaveCurrentState()
		{
			var expectedLogMessage = @"Service Name: Service1. State info: Running|Automatic
Service Name: Service1. State file saved successfully, and the file exists is True";
			AssertSaveCurrentState(ServiceControllerStatus.Running, ServiceStartMode.Automatic, expectedLogMessage);

			expectedLogMessage = @"Service Name: Service1. State info: Stopped|Manual
Service Name: Service1. State file saved successfully, and the file exists is True";
			AssertSaveCurrentState(ServiceControllerStatus.Stopped, ServiceStartMode.Manual, expectedLogMessage);
		}

		void AssertSaveCurrentState(ServiceControllerStatus status, ServiceStartMode startType, string expectedLogMessage)
		{
			const string ServiceName = "Service1";
			const string StateFileName = "Service1StateFile.txt";

			var serviceHelper = new WindowsServiceHelperForTest();

			var serviceController = new ServiceControllerForTest(status, startType);
			serviceHelper.AddServiceControllerForTest(ServiceName, serviceController);

			var stateFilePath = ServiceStateManager.GetStateFilePath(StateFileName);
			var logMessages = new List<string>();
			try
			{
				ServiceStateManager.SaveCurrentState(ServiceName, StateFileName, serviceHelper, (message, shouldAddCommandInfo) => logMessages.Add(message));

				Assert(File.Exists(stateFilePath));
				AssertEquals(status + "|" + startType, File.ReadAllText(stateFilePath));
				AssertEquals(expectedLogMessage, string.Join("\r\n", logMessages));
			}
			finally
			{
				DeleteTempFile(stateFilePath);
			}
		}

		public void TestSaveCurrentState_DeleteOldStateFile_LogMessage()
		{
			const string ServiceName = "Service1";
			const string StateFileName = "Service1StateFile.txt";

			var serviceHelper = new WindowsServiceHelperForTest();

			var serviceController = new ServiceControllerForTest(ServiceControllerStatus.Running, ServiceStartMode.Automatic);
			serviceHelper.AddServiceControllerForTest(ServiceName, serviceController);

			var stateFilePath = ServiceStateManager.GetStateFilePath(StateFileName);
			File.WriteAllText(stateFilePath, "Running|Automatic");
			var logMessages = new List<string>();
			try
			{
				ServiceStateManager.SaveCurrentState(ServiceName, StateFileName, serviceHelper, (message, shouldAddCommandInfo) => logMessages.Add(message));

				var expectedLogMessage = @$"Service Name: Service1. The old state file needs to be deleted. File path: {stateFilePath}
Service Name: Service1. The old state file was deleted failed.
Service Name: Service1. State info: Running|Automatic
Service Name: Service1. State file saved successfully, and the file exists is True";
				AssertEquals(expectedLogMessage, string.Join("\r\n", logMessages));
			}
			finally
			{
				DeleteTempFile(stateFilePath);
			}
		}

		public void TestSaveCurrentState_CouldNotGetServiceController_LogMessage()
		{
			var logMessages = new List<string>();
			var serviceHelper = new Mock<Enterprise.RemotePrinting.Client.CustomAction.IWindowsServicesHelper>();
			serviceHelper.Setup(s => s.GetServiceController(It.IsAny<string>())).Returns((Enterprise.RemotePrinting.Client.CustomAction.IServiceController)null);
			ServiceStateManager.SaveCurrentState("Service1", "Service1StateFile.txt", serviceHelper.Object, (message, shouldAddCommandInfo) => logMessages.Add(message));

			AssertEquals("Service Name: Service1. Could not get service controller.", string.Join("\r\n", logMessages));
		}

		static void DeleteTempFile(string filename)
		{
			if (File.Exists(filename))
			{
				File.Delete(filename);
			}
		}

		public void TestStopServicesIfNeeded()
		{
			var serviceHelper = new WindowsServiceHelperForTest();

			var serviceController1 = new ServiceControllerForTest(ServiceControllerStatus.Running, ServiceStartMode.Automatic);
			var serviceController2 = new ServiceControllerForTest(ServiceControllerStatus.Stopped, ServiceStartMode.Automatic);
			var serviceController3 = new ServiceControllerForTest(ServiceControllerStatus.Running, ServiceStartMode.Manual);
			var serviceController4 = new ServiceControllerForTest(ServiceControllerStatus.Paused, ServiceStartMode.Automatic);
			var serviceController5 = new ServiceControllerForTest(ServiceControllerStatus.Running, ServiceStartMode.System);
			var serviceController6 = new ServiceControllerForTest(ServiceControllerStatus.Running, ServiceStartMode.Automatic);

			serviceHelper.AddServiceControllerForTest("Service1", serviceController1);
			serviceHelper.AddServiceControllerForTest("Service2", serviceController2);
			serviceHelper.AddServiceControllerForTest("Service3", serviceController3);
			serviceHelper.AddServiceControllerForTest("Service4", serviceController4);
			serviceHelper.AddServiceControllerForTest("Service5", serviceController5);
			serviceHelper.AddServiceControllerForTest("Service6", serviceController6);

			var servicesToStop = new List<string>
			{
				"Service1",
				"Service2",
				"Service3",
				"Service4",
				"Service5"
			};
			var logMessages = new List<string>();

			ServiceStateManager.StopServicesIfNeeded(servicesToStop, serviceHelper, (message, shouldAddCommandInfo) => logMessages.Add(message));

			AssertEquals(true, serviceController1.Stopped);
			AssertEquals("Not was running", false, serviceController2.Stopped);
			AssertEquals(true, serviceController3.Stopped);
			AssertEquals("Not was running", false, serviceController4.Stopped);
			AssertEquals(true, serviceController5.Stopped);
			AssertEquals("Not in list", false, serviceController6.Stopped);

			AssertEquals(true, serviceController1.Disposed);
			AssertEquals(true, serviceController2.Disposed);
			AssertEquals(true, serviceController3.Disposed);
			AssertEquals(true, serviceController4.Disposed);
			AssertEquals(true, serviceController5.Disposed);
			AssertEquals("Not in list", false, serviceController6.Disposed);

			var expectedLogMessage = @"Stop running service.
Service Name: Service1. Status is Running.
Service Name: Service2. Status is Stopped.
Service Name: Service3. Status is Running.
Service Name: Service4. Status is Paused.
Service Name: Service5. Status is Running.
All running services have been stopped.";
			AssertEquals(expectedLogMessage, string.Join("\r\n", logMessages));
		}

		public void TestStopServicesIfNeeded_CouldNotGetServiceController_LogMessage()
		{
			var logMessages = new List<string>();
			var serviceHelper = new Mock<Enterprise.RemotePrinting.Client.CustomAction.IWindowsServicesHelper>();
			serviceHelper.Setup(s => s.GetServiceController(It.IsAny<string>())).Returns((Enterprise.RemotePrinting.Client.CustomAction.IServiceController)null);
			ServiceStateManager.StopServicesIfNeeded(new List<string> { "Service1" }, serviceHelper.Object, (message, shouldAddCommandInfo) => logMessages.Add(message));

			var expectedLogMessage = @"Stop running service.
Service Name: Service1. Could not get service controller.
All running services have been stopped.";
			AssertEquals(expectedLogMessage, string.Join("\r\n", logMessages));
		}

		public void TestRestoreCurrentState()
		{
			const string StateFileName = "Service1StateFile.txt";
			var stateFilePath = ServiceStateManager.GetStateFilePath(StateFileName);

			var savedState = ServiceControllerStatus.Running + "|Automatic";
			var expectedLogMessage = @$"Service Name: Service1.
Saved state file location: {stateFilePath}
Saved state: {savedState}

Service current status: Stopped
Running restore action.
Restore action succeeded.
Service new status: Running.";
			AssertRestoreCurrentState(savedState, ServiceControllerStatus.Stopped, true, expectedLogMessage);

			savedState = ServiceControllerStatus.Running + "|Automatic";
			expectedLogMessage = @$"Service Name: Service1.
Saved state file location: {stateFilePath}
Saved state: {savedState}

Service current status: Running";
			AssertRestoreCurrentState(savedState, ServiceControllerStatus.Running, false, expectedLogMessage);

			savedState = ServiceControllerStatus.Running + "|Manual";
			expectedLogMessage = @$"Service Name: Service1.
Saved state file location: {stateFilePath}
Saved state: {savedState}

Service current status: Stopped
Running restore action.
Restore action succeeded.
Service new status: Running.";
			AssertRestoreCurrentState(ServiceControllerStatus.Running + "|Manual", ServiceControllerStatus.Stopped, true, expectedLogMessage);

			savedState = ServiceControllerStatus.Stopped + "|Automatic";
			expectedLogMessage = @$"Service Name: Service1.
Saved state file location: {stateFilePath}
Saved state: {savedState}

Service current status: Stopped";
			AssertRestoreCurrentState(ServiceControllerStatus.Stopped + "|Automatic", ServiceControllerStatus.Stopped, false, expectedLogMessage);

			expectedLogMessage = @$"Service Name: Service1.
Saved state file location: {stateFilePath}
Saved state: {savedState}

Service current status: Running";
			AssertRestoreCurrentState(ServiceControllerStatus.Stopped + "|Automatic", ServiceControllerStatus.Running, false, expectedLogMessage);

			void AssertRestoreCurrentState(string savedState, ServiceControllerStatus serviceStatus, bool expectRestart, string expectedLogMessage)
			{
				const string ServiceName = "Service1";

				var serviceHelper = new WindowsServiceHelperForTest();

				var serviceController = new ServiceControllerForTest(serviceStatus, ServiceStartMode.Automatic);
				serviceHelper.AddServiceControllerForTest(ServiceName, serviceController);
				var logMessages = new List<string>();
				try
				{
					File.WriteAllText(stateFilePath, savedState);

					ServiceStateManager.RestoreCurrentState(ServiceName, StateFileName, serviceHelper, message => logMessages.Add(message));

					AssertEquals(expectRestart, serviceController.Started);
					AssertEquals(true, serviceController.Disposed);
					Assert("State file should be deleted", !File.Exists(stateFilePath));
					AssertEquals(expectedLogMessage, string.Join("\r\n", logMessages));
				}
				finally
				{
					DeleteTempFile(stateFilePath);
				}
			}
		}

		public void TestRestoreCurrentStateAndRenameOldService()
		{
			const string ServiceName = Constants.RegistryManager.DefaultWindowsServiceName;
			const string StateFileName = "Service1StateFile.txt";
			const string ConfigForService = "Config1";

			var stateFilePath = ServiceStateManager.GetStateFilePath(StateFileName);

			var serviceHelper = new WindowsServiceHelperForTest();

			var serviceController = new ServiceControllerForTest(ServiceControllerStatus.Stopped, ServiceStartMode.Automatic);
			serviceHelper.AddServiceControllerForTest(ServiceName, serviceController);
			serviceHelper.ConfigNameForTest = null;

			ServiceStateManager.SelectedConfigForWindowsServiceForTest = ConfigForService;
			var logMessages = new List<string>();
			try
			{
				File.WriteAllText(stateFilePath, "Running|Automatic");

				ServiceStateManager.RestoreCurrentState(ServiceName, StateFileName, serviceHelper, message => logMessages.Add(message));

				AssertEquals(true, serviceHelper.ChangedConfigName);
				AssertEquals(ServiceName, serviceHelper.ChangedServiceName);
				AssertEquals(ConfigForService, serviceHelper.ChangedNewConfigName);
				AssertEquals("Test Service - " + ConfigForService, serviceHelper.ChangedNewDisplayName);

				var expectedLogMessage = @$"Service Name: ewpcsrv. Try to change service name to Config1.
Service Name: ewpcsrv. Service name has been changed to Config1.
Service Name: ewpcsrv.
Saved state file location: {stateFilePath}
Saved state: Running|Automatic

Service current status: Stopped
Running restore action.
Restore action succeeded.
Service new status: Running.";
				AssertEquals(expectedLogMessage, string.Join("\r\n", logMessages));
			}
			finally
			{
				ServiceStateManager.SelectedConfigForWindowsServiceForTest = null;
				DeleteTempFile(stateFilePath);
			}
		}

		public void TestRestoreCurrentState_CouldNotGetServiceController_LogMessage()
		{
			const string ServiceName = "Service1";
			const string StateFileName = "Service1StateFile.txt";

			var stateFilePath = ServiceStateManager.GetStateFilePath(StateFileName);
			File.WriteAllText(stateFilePath, "Running|Automatic");
			var logMessages = new List<string>();
			try
			{
				var serviceHelper = new Mock<Enterprise.RemotePrinting.Client.CustomAction.IWindowsServicesHelper>();
				serviceHelper.Setup(s => s.GetServiceController(It.IsAny<string>())).Returns((Enterprise.RemotePrinting.Client.CustomAction.IServiceController)null);
				ServiceStateManager.RestoreCurrentState(ServiceName, StateFileName, serviceHelper.Object, message => logMessages.Add(message));

				AssertEquals("Service Name: Service1. Could not get the service controller.", string.Join("\r\n", logMessages));
			}
			finally
			{
				DeleteTempFile(stateFilePath);
			}
		}
	}
}
