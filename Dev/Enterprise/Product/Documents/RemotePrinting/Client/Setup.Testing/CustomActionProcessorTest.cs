using System;
using System.Collections.Generic;
using System.IO;
using Enterprise.RemotePrinting.Client.Setup.Custom;
using NUnit.Framework;
using IWindowsServicesHelper = Enterprise.RemotePrinting.Client.CustomAction.IWindowsServicesHelper;

namespace Setup.Testing
{
	public class CustomActionProcessorTest : TestCase
	{
		public void TestWriteLog()
		{
			var processor = new CustomActionProcessorForTest("Abc", "Xyz");
			processor.WriteLogExposed("Test message", new List<string> { "abc", "def" }, new ArgumentException("Some error"), true);

			AssertEquals("InstallLog_20240725_144615_Abc.txt", Path.GetFileName(processor.LastLogFileName));

			AssertContains("Test message", processor.LastLogMessage);
			AssertContains("Command: Abc", processor.LastLogMessage);
			AssertContains("Location: Xyz", processor.LastLogMessage);
			AssertContains($"Number of services found: 2{Environment.NewLine}  abc{Environment.NewLine}  def", processor.LastLogMessage);
			AssertContains("Some error", processor.LastLogMessage);
			AssertContains(typeof(ArgumentException).FullName, processor.LastLogMessage);
		}

		public void TestExceptionInGetWindowsServicesHelper()
		{
			var processor = new CustomActionProcessorForTest("Abc", "Xyz");

			AssertExceptionThrown<ApplicationException>(processor.Process);

			AssertContains("Error getting WindowsServicesHelper", processor.LastLogMessage);
			AssertContains("Error while creating Windows Service Helper.", processor.LastLogMessage);
		}

		public void TestExceptionInGetWindowsServicesHelper_NoRethrowForRestore()
		{
			var processor = new CustomActionProcessorForTest(CustomActionProcessor.CommandRestore, "Xyz");

			AssertNoExceptionThrown("Should not rethrow exception for Restore action", processor.Process);

			AssertContains("Error getting WindowsServicesHelper", processor.LastLogMessage);
			AssertContains("Error while creating Windows Service Helper.", processor.LastLogMessage);
		}

		public void TestExceptionInGetServiceNames()
		{
			var processor = new CustomActionProcessorForTest(CustomActionProcessor.CommandSave, "Xyz");
			processor.WindowsServicesHelperOverride = new WindowsServiceHelperForTest();

			AssertExceptionThrown<ApplicationException>(processor.Process);

			AssertContains("Error getting service names", processor.FullLogMessage);
			AssertContains("Error while retrieving list of installed Service Clients.", processor.FullLogMessage);
			AssertNotContains("ServiceHelper returned <null> list of installed Service Clients.", processor.FullLogMessage);
		}

		public void TestExceptionInGetServiceNames_NoRethrowForRestore()
		{
			var processor = new CustomActionProcessorForTest(CustomActionProcessor.CommandRestore, "Xyz");
			processor.WindowsServicesHelperOverride = new WindowsServiceHelperForTest();

			AssertNoExceptionThrown("Should not rethrow exception for Restore action", processor.Process);

			AssertContains("Error getting service names", processor.FullLogMessage);
			AssertContains("Error while retrieving list of installed Service Clients.", processor.FullLogMessage);
			AssertContains("ServiceHelper returned <null> list of installed Service Clients.", processor.FullLogMessage);
		}

		public void TestExceptionInSaveServicesStateAndStop()
		{
			var serviceHelper = new WindowsServiceHelperForTest();
			serviceHelper.InstalledServicesOverride = new List<string> { "abc", "def" };

			var processor = new CustomActionProcessorForTest(CustomActionProcessor.CommandSave, "Xyz");
			processor.WindowsServicesHelperOverride = serviceHelper;
			processor.ShouldThrowInSaveServicesStateAndStop = true;

			AssertExceptionThrown<ApplicationException>(processor.Process);
			AssertContains("Error in SaveServicesStateAndStop", processor.LastLogMessage);
			AssertNotContains("Action succeeded.", processor.LastLogMessage);

			processor.ShouldThrowInSaveServicesStateAndStop = false;
			AssertNoExceptionThrown(processor.Process);
			AssertNotContains("Error in SaveServicesStateAndStop", processor.LastLogMessage);
			AssertContains("Action succeeded.", processor.LastLogMessage);
		}

		public void TestExceptionInRestoreServices()
		{
			var serviceHelper = new WindowsServiceHelperForTest();
			serviceHelper.InstalledServicesOverride = new List<string> { "abc", "def" };

			var processor = new CustomActionProcessorForTest(CustomActionProcessor.CommandRestore, "Xyz");
			processor.WindowsServicesHelperOverride = serviceHelper;
			processor.ShouldThrowInRestoreServices = true;

			AssertNoExceptionThrown("Should not rethrow exception in Restore action", processor.Process);
			AssertContains("Error in RestoreServices", processor.LastLogMessage);
			AssertNotContains("Action succeeded.", processor.LastLogMessage);

			processor.ShouldThrowInRestoreServices = false;
			AssertNoExceptionThrown(processor.Process);
			AssertNotContains("Error in RestoreServices", processor.LastLogMessage);
			AssertContains("Action succeeded.", processor.LastLogMessage);
		}

		public void TestExceptionInDeleteServices()
		{
			var serviceHelper = new WindowsServiceHelperForTest();
			serviceHelper.InstalledServicesOverride = new List<string> { "abc", "def" };

			var processor = new CustomActionProcessorForTest(CustomActionProcessor.CommandDeleteService, "Xyz");
			processor.WindowsServicesHelperOverride = serviceHelper;
			processor.ShouldThrowInDeleteServices = true;

			AssertExceptionThrown<ApplicationException>(processor.Process);
			AssertContains("Error in DeleteServices", processor.LastLogMessage);
			AssertNotContains("Action succeeded.", processor.LastLogMessage);

			processor.ShouldThrowInDeleteServices = false;
			serviceHelper.DeleteServiceResultOverride = true;
			AssertNoExceptionThrown(processor.Process);
			AssertNotContains("Error in DeleteServices", processor.LastLogMessage);
			AssertContains("Action succeeded.", processor.LastLogMessage);
		}

		public void TestErrorInDeleteIndividualService()
		{
			var serviceHelper = new WindowsServiceHelperForTest();
			serviceHelper.InstalledServicesOverride = new List<string> { "abc", "def" };

			var processor = new CustomActionProcessorForTest(CustomActionProcessor.CommandDeleteService, "Xyz");
			processor.WindowsServicesHelperOverride = serviceHelper;
			processor.ShouldThrowInDeleteServices = false;
			serviceHelper.DeleteServiceResultOverride = false;

			AssertNoExceptionThrown(processor.Process);

			AssertContains("Error while deleting service abc: Something", processor.FullLogMessage);
			AssertContains("Error while deleting service def: Something", processor.FullLogMessage);
			AssertContains("Action succeeded.", processor.FullLogMessage);
		}

		public void TestUnknownCustomAction()
		{
			var serviceHelper = new WindowsServiceHelperForTest();
			serviceHelper.InstalledServicesOverride = new List<string> { "abc", "def" };

			var processor = new CustomActionProcessorForTest("Do it!", "Xyz");
			processor.WindowsServicesHelperOverride = serviceHelper;

			AssertNoExceptionThrown(processor.Process);

			AssertContains("Unknown custom action.", processor.LastLogMessage);
			AssertNotContains("Action succeeded.", processor.LastLogMessage);
		}
	}

	class CustomActionProcessorForTest : CustomActionProcessor
	{
		public CustomActionProcessorForTest(string action, string installLocation) : base(action, installLocation)
		{
		}

		protected override IWindowsServicesHelper GetWindowsServicesHelperCore()
		{
			if (WindowsServicesHelperOverride == null)
			{
				throw new ApplicationException("Error getting WindowsServicesHelper");
			}
			return WindowsServicesHelperOverride;
		}
		public IWindowsServicesHelper WindowsServicesHelperOverride { get; set; }

		protected override void SaveServicesStateAndStop(List<string> serviceNames, IWindowsServicesHelper servicesHelper)
		{
			if (ShouldThrowInSaveServicesStateAndStop)
			{
				throw new ApplicationException("Error in SaveServicesStateAndStop");
			}
		}
		public bool ShouldThrowInSaveServicesStateAndStop { get; set; }

		protected override void RestoreServices(List<string> serviceNames, IWindowsServicesHelper servicesHelper)
		{
			if (ShouldThrowInRestoreServices)
			{
				throw new ApplicationException("Error in RestoreServices");
			}
		}
		public bool ShouldThrowInRestoreServices { get; set; }

		protected override void DeleteServices(List<string> serviceNames, IWindowsServicesHelper servicesHelper)
		{
			if (ShouldThrowInDeleteServices)
			{
				throw new ApplicationException("Error in DeleteServices");
			}

			base.DeleteServices(serviceNames, servicesHelper);
		}
		public bool ShouldThrowInDeleteServices { get; set; }

		public string LastLogFileName => ((LogWriterForSetupTest.LogWriterForSetupTesting)logWriter).LastLogFileName;
		public string LastLogMessage => ((LogWriterForSetupTest.LogWriterForSetupTesting)logWriter).LastLogMessage;
		public string FullLogMessage => ((LogWriterForSetupTest.LogWriterForSetupTesting)logWriter).FullLogMessage;

		protected override LogWriterForSetup CreateLogWriter()
		{
			return new LogWriterForSetupTest.LogWriterForSetupTesting(action);
		}

		public void WriteLogExposed(string message, List<string> serviceNames, Exception ex, bool shouldAddCommandInfo)
		{
			WriteLog(message, serviceNames, ex, shouldAddCommandInfo);
		}
	}
}
