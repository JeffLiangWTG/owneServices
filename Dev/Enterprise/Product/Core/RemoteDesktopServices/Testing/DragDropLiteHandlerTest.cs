using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class DragDropLiteHandlerTest : TestCase
	{
		public void TestDoHandle_Success()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				var filePath = Temp.GetTempFileName(tempDir);
				var stream = CreateStream(filePath);
				var handler = new DragDropLiteHandler();

				// Act
				handler.Handle(new EnterpriseChannelMock(), stream);
				WaitForDragDropEventToBeTriggered();

				// Assert
				AssertNotNull(filePaths);
				AssertEquals(1, filePaths.Length);
				AssertEquals(filePath, filePaths[0]);
				AssertEquals(true, File.Exists(filePaths[0]));
				AssertNullOrEmpty(ErrorReporter.LastKeyReported);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		public void TestDoHandle_VolumeSeparatorFormatError()
		{
			// Arrange
			const string FilePath = ":\\a.txt";
			var stream = CreateStream(FilePath);
			var handler = new DragDropLiteHandler();

			var terminalService = ObjectFactory.Get<TerminalService>();
			var supportedClientVersion = terminalService.IsCitrixICA ? ClientCitrixVersion.Version : ClientVersion.Version;

			// Act
			handler.Handle(new EnterpriseChannelMock(), stream);
			WaitForDragDropEventToBeTriggered();

			// Assert
			AssertNull(filePaths);
			AssertEquals("DragDropLiteHandlerEmptyString", ErrorReporter.LastKeyReported);
			AssertEquals(
				$"Unable to get mapped path of [{FilePath}] due to exception, " +
				$"IsCitrix: False, IsRemoteAppSession: False, IsWTSSession: False, ClientSessionProtocolType: 0, " +
				$"TerminalService.LastWin32Error: 0, SupportedClientVersion: {supportedClientVersion}",
				ErrorReporter.LastMessageReported);
			AssertEquals($"Cannot find file. Mapping Result: {DriveMappingResult.VolumeSeparatorFormatError}", ErrorReporter.LastExceptionReported.Message);

			// Cleanup
			ErrorReporter.Clear();
		}

		public void TestDoHandle_ParentDirectoryDoesNotExist()
		{
			// Arrange
			var tempDir = new TempDirectory();
			tempDir.Dispose();

			var filePath = Path.Combine(tempDir, "a.txt");
			var stream = CreateStream(filePath);
			var handler = new DragDropLiteHandler();

			var terminalService = ObjectFactory.Get<TerminalService>();
			var supportedClientVersion = terminalService.IsCitrixICA ? ClientCitrixVersion.Version : ClientVersion.Version;
			var errorReporterMock = new Mock<IErrorReporter>();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				// Act
				handler.Handle(new EnterpriseChannelMock(), stream);
				WaitForDragDropEventToBeTriggered();

				// Assert
				var expectedErrorMessage = $"Unable to get mapped path of [{filePath}] due to exception, " +
					$"IsCitrix: False, IsRemoteAppSession: False, IsWTSSession: False, ClientSessionProtocolType: 0, " +
					$"TerminalService.LastWin32Error: 0, SupportedClientVersion: {supportedClientVersion}";

				AssertNull(filePaths);
				errorReporterMock.Verify(x => x.Report("DragDropLiteHandlerEmptyString", expectedErrorMessage, It.Is<FileNotFoundException>(x => x.Message.Equals($"File Existence State: {ExistenceState.ParentDirectoryNotExisting}, Directory Existence State:{ExistenceState.NotExisting}"))), Times.Once);
			}
		}

		public void TestDoHandle_FileDoesNotExist()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				var filePath = Path.Combine(tempDir, "a.txt");
				var stream = CreateStream(filePath);
				var handler = new DragDropLiteHandler();

				var terminalService = ObjectFactory.Get<TerminalService>();
				var supportedClientVersion = terminalService.IsCitrixICA ? ClientCitrixVersion.Version : ClientVersion.Version;
				var errorReporterMock = new Mock<IErrorReporter>();

				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				{
					// Act
					handler.Handle(new EnterpriseChannelMock(), stream);
					WaitForDragDropEventToBeTriggered();

					// Assert
					var expectedErrorMessage = $"Unable to get mapped path of [{filePath}] due to [{DriveMappingResult.FileDoesNotExist}], " +
						$"IsCitrix: False, IsRemoteAppSession: False, IsWTSSession: False, ClientSessionProtocolType: 0, " +
						$"TerminalService.LastWin32Error: 0, SupportedClientVersion: {supportedClientVersion}";
					AssertNull(filePaths);
					errorReporterMock.Verify(x => x.Report("DragDropLiteHandlerEmptyString", expectedErrorMessage, It.IsAny<Exception>()), Times.Never);
				}
			}

			// Cleanup
			ErrorReporter.Clear();
		}

		public void TestDoHandle_DoesNotHaveReadPermissionToExistingFile()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				var filePath = Temp.GetTempFileName(tempDir);

				var fileInfo = new FileInfo(filePath);
				var fileAcls = fileInfo.GetAccessControl();
				fileAcls.SetAccessRuleProtection(true, false);
				fileInfo.SetAccessControl(fileAcls);

				var dirInfo = new DirectoryInfo(tempDir);
				var dirAcls = dirInfo.GetAccessControl();
				dirAcls.SetAccessRuleProtection(true, false);
				dirInfo.SetAccessControl(dirAcls);

				var terminalService = ObjectFactory.Get<TerminalService>();
				var supportedClientVersion = terminalService.IsCitrixICA ? ClientCitrixVersion.Version : ClientVersion.Version;

				using (new DisposableAction(
					() =>
					{
						var newDirAcls = new DirectorySecurity();
						newDirAcls.SetAccessRuleProtection(false, true);
						dirInfo.SetAccessControl(newDirAcls);

						var newFileAcls = new FileSecurity();
						newFileAcls.SetAccessRuleProtection(false, true);
						fileInfo.SetAccessControl(newFileAcls);
					}))
				{
					var stream = CreateStream(filePath);
					var handler = new DragDropLiteHandler();

					// Act
					handler.Handle(new EnterpriseChannelMock(), stream);
					WaitForDragDropEventToBeTriggered();

					// Assert
					AssertNull(filePaths);
					AssertEquals("DragDropLiteHandlerEmptyString", ErrorReporter.LastKeyReported);
					AssertEquals(
						$"Unable to get mapped path of [{filePath}] due to exception, " +
						$"IsCitrix: False, IsRemoteAppSession: False, IsWTSSession: False, ClientSessionProtocolType: 0, " +
						$"TerminalService.LastWin32Error: 0, SupportedClientVersion: {supportedClientVersion}",
						ErrorReporter.LastMessageReported);
					AssertEquals($"File Existence State: {ExistenceState.ExistingButNoPermission}, Directory Existence State:{ExistenceState.ExistingNormally}", ErrorReporter.LastExceptionReported.Message);
				}
			}

			// Cleanup
			ErrorReporter.Clear();
		}

		Stream CreateStream(string filePath)
		{
			var liteMessage = new DragDropLiteMessage();
			liteMessage.fileDrop.Add(filePath);

			var ms = new MemoryStream();
			new XmlSerializer(typeof(DragDropLiteMessage)).Serialize(ms, liteMessage);

			ms.Position = 0;
			return ms;
		}

		string[] filePaths;
		bool dragDropEventTriggered;

		void WaitForDragDropEventToBeTriggered()
		{
			var maxWait = TimeSpan.FromSeconds(30d);
			var sw = Stopwatch.StartNew();
			while (!dragDropEventTriggered && sw.Elapsed < maxWait)
			{
				Thread.Sleep(50);
				Application.DoEvents();
			}
		}

		void TestForm_DragDrop(object sender, DragEventArgs e)
		{
			dragDropEventTriggered = true;
			filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
		}

		protected override void SetUp()
		{
			base.SetUp();

			DragDropLiteHandler.testForm = new Form();
			DragDropLiteHandler.testForm.AllowDrop = true;
			DragDropLiteHandler.testForm.DragDrop += TestForm_DragDrop;
			DragDropLiteHandler.testForm.Show();
			Application.DoEvents();

			MappedClientPath.LeaveThePathUnMappedForTesting = true;
		}

		protected override void TearDown()
		{
			filePaths = null;
			dragDropEventTriggered = false;

			MappedClientPath.LeaveThePathUnMappedForTesting = false;

			DragDropLiteHandler.testForm.DragDrop -= TestForm_DragDrop;
			DragDropLiteHandler.testForm.Dispose();
			DragDropLiteHandler.testForm = null;
			ErrorReporter.Clear();
			base.TearDown();
		}

		class EnterpriseChannelMock : IEnterpriseChannel
		{
			public bool IsConnected => throw new NotImplementedException();

			public bool Send(byte[] data)
			{
				throw new NotImplementedException();
			}
		}
	}
}
