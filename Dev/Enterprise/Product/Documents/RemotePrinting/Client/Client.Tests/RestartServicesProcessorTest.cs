using System;
using System.IO;
using CargoWise.Common;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class RestartServicesProcessorTest : TestCase
	{
		public void TestRestartService_NoConfigName()
		{
			var directoryPathForTest = new DirectoryInfo(Path.Combine(Constants.WebPrintClientDataPath, "RestartLogForTest"));
			var fileName = new FileInfo(Path.Combine(directoryPathForTest.ToString(), "RestartLogFileForTest.txt")).FullName;
			using (CleanLogFiles(directoryPathForTest))
			{
				var helper = new WindowsServicesHelper();
				var processor = new RestartServicesProcessorForTest(helper);
				processor.RegisterDirectoryPath = directoryPathForTest.FullName;
				processor.RegisterFileName = fileName;
				processor.RestartService(new[] { "" });

				using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("No Service configuration name!", log);
				}
			}
		}

		public void TestRestartService_ServiceNotFound()
		{
			var directoryPathForTest = new DirectoryInfo(Path.Combine(Constants.WebPrintClientDataPath, "RestartLogForTest"));
			var fileName = new FileInfo(Path.Combine(directoryPathForTest.ToString(), "RestartLogFileForTest.txt")).FullName;
			using (CleanLogFiles(directoryPathForTest))
			{
				var helper = new Mock<IWindowsServicesHelper>();
				helper.Setup(x => x.GetConfigNameArgumentValue(It.IsAny<string[]>())).Returns("Jerry Test Config");
				helper.Setup(x => x.CheckServiceControllerStatus(It.IsAny<string>())).Returns(WindowsServicesHelper.ServiceNotFound);
				var processor = new RestartServicesProcessorForTest(helper.Object);
				processor.RegisterDirectoryPath = directoryPathForTest.FullName;
				processor.RegisterFileName = fileName;
				processor.RestartService(new[] { "ConfigName:Jerry Test Config" });

				using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("No Service installed for this configuration! Please use Configurator and install a service.", log);
				}
			}
		}

		public void TestRestartService_StopProcess_Failed()
		{
			var directoryPathForTest = new DirectoryInfo(Path.Combine(Constants.WebPrintClientDataPath, "RestartLogForTest"));
			var fileName = new FileInfo(Path.Combine(directoryPathForTest.ToString(), "RestartLogFileForTest.txt")).FullName;
			using (CleanLogFiles(directoryPathForTest))
			{
				var errorMessage = "";
				var helper = new Mock<IWindowsServicesHelper>();
				helper.Setup(x => x.GetConfigNameArgumentValue(It.IsAny<string[]>())).Returns("Jerry Test Config");
				helper.Setup(x => x.CheckServiceControllerStatus(It.IsAny<string>())).Returns(WindowsServicesHelper.ServiceControllerStatusRunning);
				helper.Setup(x => x.StartProcess(It.IsAny<string>(), out errorMessage)).Returns(false);
				var processor = new RestartServicesProcessorForTest(helper.Object);
				processor.RegisterDirectoryPath = directoryPathForTest.FullName;
				processor.RegisterFileName = fileName;
				processor.RestartService(new[] { "ConfigName:Jerry Test Config" });

				using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("Failed to stop the WebPrint Client Service, error message:", log);
				}
			}
		}

		public void TestRestartService_StopProcess_Success()
		{
			var directoryPathForTest = new DirectoryInfo(Path.Combine(Constants.WebPrintClientDataPath, "RestartLogForTest"));
			var fileName = new FileInfo(Path.Combine(directoryPathForTest.ToString(), "RestartLogFileForTest.txt")).FullName;
			using (CleanLogFiles(directoryPathForTest))
			{
				var errorMessage = "";
				var helper = new Mock<IWindowsServicesHelper>();
				helper.Setup(x => x.GetConfigNameArgumentValue(It.IsAny<string[]>())).Returns("Jerry Test Config");
				helper.Setup(x => x.CheckServiceControllerStatus(It.IsAny<string>())).Returns(WindowsServicesHelper.ServiceControllerStatusRunning);
				helper.Setup(x => x.StopProcess(It.IsAny<string>(), out errorMessage)).Returns(true);
				helper.Setup(x => x.CheckServiceControllerStatusWithRetry(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Func<string, bool>>())).Returns(WindowsServicesHelper.ServiceControllerStatusStopped);
				var processor = new RestartServicesProcessorForTest(helper.Object);
				processor.RegisterDirectoryPath = directoryPathForTest.FullName;
				processor.RegisterFileName = fileName;
				processor.RestartService(new[] { "ConfigName:Jerry Test Config" });

				using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("The Service has been stopped.", log);
				}
			}
		}

		public void TestRestartService_StartProcess_Failed()
		{
			var directoryPathForTest = new DirectoryInfo(Path.Combine(Constants.WebPrintClientDataPath, "RestartLogForTest"));
			var fileName = new FileInfo(Path.Combine(directoryPathForTest.ToString(), "RestartLogFileForTest.txt")).FullName;
			using (CleanLogFiles(directoryPathForTest))
			{
				var errorMessage = "";
				var helper = new Mock<IWindowsServicesHelper>();
				helper.Setup(x => x.GetConfigNameArgumentValue(It.IsAny<string[]>())).Returns("Jerry Test Config");
				helper.Setup(x => x.CheckServiceControllerStatus(It.IsAny<string>())).Returns(WindowsServicesHelper.ServiceControllerStatusRunning);
				helper.Setup(x => x.StopProcess(It.IsAny<string>(), out errorMessage)).Returns(true);
				helper.Setup(x => x.CheckServiceControllerStatusWithRetry(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Func<string, bool>>())).Returns(WindowsServicesHelper.ServiceControllerStatusStopped);
				helper.Setup(x => x.StartProcess(It.IsAny<string>(), out errorMessage)).Returns(false);
				var processor = new RestartServicesProcessorForTest(helper.Object);
				processor.RegisterDirectoryPath = directoryPathForTest.FullName;
				processor.RegisterFileName = fileName;
				processor.RestartService(new[] { "ConfigName:Jerry Test Config" });

				using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("Failed to start the WebPrint Client Service, error message:", log);
				}
			}
		}

		public void TestRestartService_StartProcess_Success()
		{
			var directoryPathForTest = new DirectoryInfo(Path.Combine(Constants.WebPrintClientDataPath, "RestartLogForTest"));
			var fileName = new FileInfo(Path.Combine(directoryPathForTest.ToString(), "RestartLogFileForTest.txt")).FullName;
			using (CleanLogFiles(directoryPathForTest))
			{
				var errorMessage = "";
				var helper = new Mock<IWindowsServicesHelper>();
				helper.Setup(x => x.GetConfigNameArgumentValue(It.IsAny<string[]>())).Returns("Jerry Test Config");
				helper.Setup(x => x.CheckServiceControllerStatus(It.IsAny<string>())).Returns(WindowsServicesHelper.ServiceControllerStatusRunning);
				helper.Setup(x => x.StopProcess(It.IsAny<string>(), out errorMessage)).Returns(true);
				helper.Setup(x => x.CheckServiceControllerStatusWithRetry(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Func<string, bool>>())).Returns(WindowsServicesHelper.ServiceControllerStatusStopped);
				helper.Setup(x => x.StartProcess(It.IsAny<string>(), out errorMessage)).Returns(true);
				var processor = new RestartServicesProcessorForTest(helper.Object);
				processor.RegisterDirectoryPath = directoryPathForTest.FullName;
				processor.RegisterFileName = fileName;
				processor.RestartService(new[] { "ConfigName:Jerry Test Config" });

				using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("The Service has been stopped.", log);
					AssertContains("The Service has been started.", log);
				}
			}
		}

		public void TestRestartService_Exception()
		{
			var directoryPathForTest = new DirectoryInfo(Path.Combine(Constants.WebPrintClientDataPath, "RestartLogForTest"));
			var fileName = new FileInfo(Path.Combine(directoryPathForTest.ToString(), "RestartLogFileForTest.txt")).FullName;
			using (CleanLogFiles(directoryPathForTest))
			{
				var helper = new Mock<IWindowsServicesHelper>();
				helper.Setup(x => x.GetConfigNameArgumentValue(It.IsAny<string[]>())).Returns("Jerry Test Config");
				helper.Setup(x => x.CheckServiceControllerStatus(It.IsAny<string>())).Throws(new ApplicationException("Jerry Test Error"));
				var processor = new RestartServicesProcessorForTest(helper.Object);
				processor.RegisterDirectoryPath = directoryPathForTest.FullName;
				processor.RegisterFileName = fileName;
				processor.RestartService(new[] { "ConfigName:Jerry Test Config" });

				using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("Restart WebPrint Client Service Error", log);
				}
			}
		}

		IDisposable CleanLogFiles(DirectoryInfo directoryInfo)
		{
			return new DisposableAction(() =>
			{
				LogWriter.UnregisterAllLogTargets();
				if (directoryInfo.Exists)
				{
					foreach (var tempfile in directoryInfo.EnumerateFiles())
					{
						File.Delete(tempfile.FullName);
					}
					directoryInfo.Delete();
				}
			});
		}

		class RestartServicesProcessorForTest : RestartServicesProcessor
		{
			public RestartServicesProcessorForTest(IWindowsServicesHelper servicesHelper) : base(servicesHelper)
			{
			}

			protected override void RegisterFileTarget(string filePath, string filePrefix)
			{
				var target = LogWriter.RegisterFileTarget(RegisterDirectoryPath, filePrefix, WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error, "*", 500);
				target.FileName = RegisterFileName;
			}

			public string RegisterDirectoryPath { get; set; }

			public string RegisterFileName { get; set; }
		}
	}
}
