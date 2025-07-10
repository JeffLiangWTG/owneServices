using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Interop.DataObjects;
using CargoWise.IO;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class DragDropLiteTest : DragDropTest
	{
		protected override void CreateTestForm()
		{
			DragDropLiteHandler.testForm = new Form();
			EnvProxy.Instance.Registry.RemoteAppEnableDragDropLite = true;
			DragDropLiteHandler.testForm.AllowDrop = true;
			DragDropLiteHandler.testForm.DragDrop += TestForm_DragDrop;
			DragDropLiteHandler.testForm.Show();
			Application.DoEvents();
			MappedClientPath.LeaveThePathUnMappedForTesting = true;
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.SetDragAndDropLite));
		}

		protected override void DisposeTestForm()
		{
			EnvProxy.Instance.Registry.RemoteAppEnableDragDropLite = false;
			MappedClientPath.LeaveThePathUnMappedForTesting = false;
			DragDropLiteHandler.testForm.Dispose();
			DragDropLiteHandler.testForm = null;
		}

		protected override void TestForm_DragDrop(object sender, DragEventArgs e)
		{
			if (dropEvent == null)
			{
				dropEvent = e;
				foreach (string file in (string[])e.Data.GetData(DataFormats.FileDrop))
				{
					File.Copy(new MappedClientPath().GetUnmappedPath(file), TempFileHelper.SafePath(tempDirectory.DirectoryName, Path.GetFileName(file)), true);
				}
			}
		}

		public new void TestDropFilesWithLongNames()
		{
			Assert(true); //File with long names will always create a DragDropMessage.
		}

		public new void TestDragDropOnFormCreationThread()
		{
			DisposeTestForm();
			EnvProxy.Instance.Registry.RemoteAppEnableDragDropLite = true;

			var mainThreadEvent = new AutoResetEvent(false);
			var thread = new Thread(() =>
			{
				SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
				DragDropLiteHandler.testForm = new Form();

				DragDropLiteHandler.testForm.AllowDrop = true;
				DragDropLiteHandler.testForm.DragDrop += TestForm_DragDrop;
				mainThreadEvent.Set();
				DragDropLiteHandler.testForm.ShowDialog();
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Start();

			mainThreadEvent.WaitOne();
			Application.DoEvents();
			MappedClientPath.DoesParentDirectoryExistTestMock = _ => ExistenceState.ExistingNormally;
			MappedClientPath.LeaveThePathUnMappedForTesting = true;
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.SetDragAndDropLite));
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.TestDragAndDropDropTextFile));

			AssertReceivedDropEvent();
			var fileDrop = (string[])dropEvent.Data.GetData(DataFormats.FileDrop);
			AssertEquals(1, fileDrop.Length);
			AssertFile(tempDirectory, "TestTextFile.txt", fileDrop[0]);

			MappedClientPath.DoesParentDirectoryExistTestMock = null;
		}

		public void TestFallbackToDragDropWhenNotAbleToMap_Disabled()
		{
			// Arrange
			DisposeTestForm();

			var helper = new FallbackWhenFailureHelper();
			helper.Prepare();
			EnvProxy.Instance.Registry.RemoteAppEnableDragDropLite = false;

			// Act
			EnterpriseChannel.Instance.SendMessage(
				EnterpriseChannelMessageTypes.Test,
				TestMessage.GetMessage(TestMessage.Action.TestDragAndDropDropTextFile));

			helper.WaitForDragDropEventToBeTriggered();

			// Assert
			AssertEquals(nameof(DragDropHandler), helper.FormNameDroppedOn);
		}

		public void TestBlocked_VolumeSeparatorFormatError()
		{
			// Arrange
			DisposeTestForm();

			var helper = new FallbackWhenFailureHelper();
			helper.Prepare();

			// Act
			EnterpriseChannel.Instance.SendMessage(
				EnterpriseChannelMessageTypes.Test,
				TestMessage.GetMessage(":\\a.txt"));

			helper.WaitForDragDropEventToBeTriggered();

			// Assert
			AssertNullOrEmpty("[No file is dropped on any form]", helper.FormNameDroppedOn);
			AssertContains("System.NotSupportedException: The given path's format is not supported.", clientProcessErrors.ToString());

			// Cleanup
			clientProcessErrors.Clear();
		}

		public void TestBlocked_ParentDirectoryDoesNotExist()
		{
			// Arrange
			DisposeTestForm();

			var helper = new FallbackWhenFailureHelper();
			helper.Prepare();

			var tempDir = new TempDirectory();
			tempDir.Dispose();

			// Act
			EnterpriseChannel.Instance.SendMessage(
				EnterpriseChannelMessageTypes.Test,
				TestMessage.GetMessage(Path.Combine(tempDir, "a.txt")));

			helper.WaitForDragDropEventToBeTriggered();

			// Assert
			AssertExceptionThrown<DirectoryNotFoundException>(() => new DirectoryInfo(tempDir).GetAccessControl());
			AssertNullOrEmpty("[No file is dropped on any form]", helper.FormNameDroppedOn);
		}

		public void TestBlocked_FileDoesNotExist()
		{
			// Arrange
			DisposeTestForm();

			var helper = new FallbackWhenFailureHelper();
			helper.Prepare();

			using (var tempDir = new TempDirectory())
			{
				var filePath = Path.Combine(tempDir, "a.txt");

				// Act
				EnterpriseChannel.Instance.SendMessage(
					EnterpriseChannelMessageTypes.Test,
					TestMessage.GetMessage(filePath));

				helper.WaitForDragDropEventToBeTriggered();

				// Assert
				AssertExceptionThrown<FileNotFoundException>(() => new FileInfo(filePath).GetAccessControl());
				AssertNullOrEmpty("[No file is dropped on any form]", helper.FormNameDroppedOn);
			}
		}

		public void TestBlocked_DoesNotHaveReadPermissionToExistingFile()
		{
			// Arrange
			DisposeTestForm();

			var helper = new FallbackWhenFailureHelper();
			helper.Prepare();

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

				using (new DisposableAction(() =>
				{
					var newDirAcls = new DirectorySecurity();
					newDirAcls.SetAccessRuleProtection(false, true);
					dirInfo.SetAccessControl(newDirAcls);

					var newFileAcls = new FileSecurity();
					newFileAcls.SetAccessRuleProtection(false, true);
					fileInfo.SetAccessControl(newFileAcls);
				}))
				{
					// Act
					EnterpriseChannel.Instance.SendMessage(
						EnterpriseChannelMessageTypes.Test,
						TestMessage.GetMessage(filePath));

					helper.WaitForDragDropEventToBeTriggered();

					// Assert
					AssertEquals("File doesn't appear to exist", false, File.Exists(filePath));
					AssertNotNull("File does exist", fileInfo.GetAccessControl());
					AssertNullOrEmpty("[No file is dropped on any form]", helper.FormNameDroppedOn);
				}
			}
		}

		public void TestSuccess_DoesNotHaveReadPermissionToDirectory()
		{
			// Arrange
			DisposeTestForm();

			var helper = new FallbackWhenFailureHelper();
			helper.Prepare();

			using (var rootTempDir = new TempDirectory())
			{
				var parentDir = Path.Combine(rootTempDir, Path.GetRandomFileName());
				Directory.CreateDirectory(parentDir);

				var filePath = Temp.GetTempFileName(parentDir);
				File.WriteAllText(filePath, "hello");

				var fileInfo = new FileInfo(filePath);
				var fileAcls = fileInfo.GetAccessControl();
				fileAcls.SetAccessRuleProtection(true, true);
				fileInfo.SetAccessControl(fileAcls);

				var parentDirInfo = new DirectoryInfo(parentDir);
				var parentDirAcls = parentDirInfo.GetAccessControl();
				parentDirAcls.SetAccessRuleProtection(true, false);
				parentDirInfo.SetAccessControl(parentDirAcls);

				var rootDirInfo = new DirectoryInfo(rootTempDir);
				var rootDirAcls = rootDirInfo.GetAccessControl();
				rootDirAcls.SetAccessRuleProtection(true, false);
				rootDirInfo.SetAccessControl(rootDirAcls);

				using (new DisposableAction(() =>
				{
					var newRootDirAcls = new DirectorySecurity();
					newRootDirAcls.SetAccessRuleProtection(false, true);
					rootDirInfo.SetAccessControl(newRootDirAcls);

					var newParentDirAcls = new DirectorySecurity();
					newParentDirAcls.SetAccessRuleProtection(false, true);
					parentDirInfo.SetAccessControl(newParentDirAcls);
				}))
				{
					// Act
					EnterpriseChannel.Instance.SendMessage(
						EnterpriseChannelMessageTypes.Test,
						TestMessage.GetMessage(filePath));

					helper.WaitForDragDropEventToBeTriggered();

					// Assert
					AssertEquals("File does exist", true, File.Exists(filePath));
					AssertEquals("Parent directory seems not existing", false, Directory.Exists(parentDir));
					AssertNotNull("Parent directory does exist", parentDirInfo.GetAccessControl());
					AssertEquals(nameof(DragDropLiteHandler), helper.FormNameDroppedOn);
				}
			}
		}

		protected override bool IsLite => true;

		class FallbackWhenFailureHelper
		{
			public void Prepare()
			{
				DragDropHandler.testForm = CreateForm(nameof(DragDropHandler));
				DragDropLiteHandler.testForm = CreateForm(nameof(DragDropLiteHandler));

				MappedClientPath.LeaveThePathUnMappedForTesting = true;
				EnvProxy.Instance.Registry.RemoteAppEnableDragDropLite = true;

				EnterpriseChannel.Instance.SendMessage(
					EnterpriseChannelMessageTypes.Test,
					TestMessage.GetMessage(TestMessage.Action.SetDragAndDropLite));
			}

			Form CreateForm(string name)
			{
				var form = new Form
				{
					Name = name,
					AllowDrop = true,
				};

				form.DragDrop += TestForm_DragDropForBoth;
				form.Show();
				Application.DoEvents();

				return form;
			}

			public void WaitForDragDropEventToBeTriggered()
			{
				var maxWait = TimeSpan.FromSeconds(20d);
				var sw = Stopwatch.StartNew();
				while (string.IsNullOrEmpty(FormNameDroppedOn) && sw.Elapsed < maxWait)
				{
					Thread.Sleep(50);
					Application.DoEvents();
				}
			}

			public string FormNameDroppedOn { get; private set; }

			void TestForm_DragDropForBoth(object sender, DragEventArgs e)
			{
				FormNameDroppedOn = ((Form)sender).Name;
			}
		}
	}
}
