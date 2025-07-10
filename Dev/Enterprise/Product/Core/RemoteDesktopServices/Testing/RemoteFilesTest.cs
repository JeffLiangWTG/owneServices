using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class RemoteFilesTest : RemoteDesktopServicesTest
	{
		[RequiresSoftware(RequiredSoftware.CanRunGUITests)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcess", Justification = "Testing")]
		public void TestOpenFile()
		{
			string testFile = TestMessage.GetTestFilePath("TestOpenFile.exe");
			string testFileUniqueName = Path.Combine(Path.GetDirectoryName(testFile), Guid.NewGuid().ToString() + ".EXE");
			string remoteFileName;
			using (var remoteFile = new RemoteFile(testFileUniqueName, File.ReadAllBytes(testFile), false))
			{
				Assert("File should open successfully", remoteFile.Open());

				Process openProcess;
				DateTime startTime = DateTime.UtcNow;
				do
				{
					Thread.Sleep(TimeSpan.FromSeconds(1));
					openProcess = Process.GetProcesses().FirstOrDefault(p => p.MainWindowTitle.Contains(Path.GetFileNameWithoutExtension(testFileUniqueName)));
				}
				while (openProcess == null && DateTime.UtcNow.Subtract(startTime) < TimeSpan.FromSeconds(30));

				AssertNotNull(openProcess);
				OpenFileStatusMessage status;
				try
				{
					status = remoteFile.GetStatus();
					AssertEquals(false, status.isChanged);

					remoteFileName = GetRemoteFilePath(testFileUniqueName);
					AssertFileSameAsBytes(testFile, File.ReadAllBytes(remoteFileName));
				}
				finally
				{
					openProcess.Kill();
				}

				startTime = DateTime.UtcNow;
				do
				{
					Thread.Sleep(TimeSpan.FromSeconds(1));
					status = remoteFile.GetStatus();
				}
				while (status.isOpen && DateTime.UtcNow.Subtract(startTime) < TimeSpan.FromSeconds(30));

				AssertEquals(false, status.isOpen);
				AssertEquals(false, status.isChanged);

				File.WriteAllBytes(remoteFileName, new byte[] { 1, 2, 3, 4, 5, 6 });
				Thread.Sleep(100);
				status = remoteFile.GetStatus();
				AssertEquals(false, status.isOpen);
				AssertEquals(true, status.isChanged);

				byte[] data = remoteFile.FetchFileData();
				AssertEquals(new byte[] { 1, 2, 3, 4, 5, 6 }, data);
			}
		}

		public void TestOpenFileInMicrosoftOffice365DoesNotBlockUI()
		{
			EnvProxy.Instance.Registry.MicrosoftOffice365ApplicationIdForDragDrop = "TestAppId";
			EnvProxy.Instance.Registry.OpenInMicrosoftOffice365FileTypeList = new string[] { ".*" };
			var taskCompleteHandle = new ManualResetEvent(false);
			var mockFile = new Mock<RemoteFile>("testFile.txt", new byte[1] { 0x20 }, true);
			mockFile
				.Protected()
				.Setup("UploadToMicrosoftOffice365AndSendUriToClient")
				.Callback(() =>
				{
					WaitFunction().GetAwaiter().GetResult();
					taskCompleteHandle.Set();
				});
			using (mockFile.Object)
			{
				Assert(mockFile.Object.Open());
				AssertEquals(expected: 0, WaitHandle.WaitAny(new WaitHandle[] { taskCompleteHandle }, TimeSpan.FromSeconds(3)));
				mockFile.Protected().Verify("UploadToMicrosoftOffice365AndSendUriToClient", Times.AtLeastOnce());
			}
		}

		async Task WaitFunction()
		{
			await Task.Delay(TimeSpan.FromMilliseconds(1));
		}

		protected override void SetUp()
		{
			base.SetUp();
			testDocDirectory = TestMessage.ReleaseResourcesFiles();
		}

		protected override void TearDown()
		{
			try
			{
				testDocDirectory.Dispose();
			}
			finally
			{
				base.TearDown();
			}
		}

		IDisposable testDocDirectory;
	}
}
