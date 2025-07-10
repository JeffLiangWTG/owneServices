using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Interop.DataObjects;
using CargoWise.IO;
using Enterprise.RemoteDesktopServices.Server;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class DragDropTest : RemoteDesktopServicesTest
	{
		[ExpectNoExceptions]
		public void TestMultipleSessions()
		{
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.TestDragAndDropMultipleSessions));
		}

		[ExpectNoExceptions]
		public void TestDragEffects()
		{
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.TestDragAndDropDragEffects));
		}

		[ExpectNoExceptions]
		public void TestDragUnsupported()
		{
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.TestDragAndDropDragUnsupported));
		}

		public void TestDropTextFile()
		{
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.TestDragAndDropDropTextFile));
			AssertReceivedDropEvent();
			var fileDrop = (string[])dropEvent.Data.GetData(DataFormats.FileDrop);
			AssertEquals(1, fileDrop.Length);
			AssertFile(tempDirectory, "TestTextFile.txt", fileDrop[0]);
		}

		public void TestDropMultipleFiles()
		{
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.TestDragAndDropDropMultipleFiles));
			AssertReceivedDropEvent();
			var fileDrop = (string[])dropEvent.Data.GetData(DataFormats.FileDrop);
			AssertEquals(3, fileDrop.Length);
			AssertFile(tempDirectory, "Test.xls", fileDrop[0]);
			AssertFile(tempDirectory, "Sample.PDF", fileDrop[1]);
			AssertFile(tempDirectory, "Email with Image.msg", fileDrop[2]);
		}

		public void TestDropBitmap()
		{
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.TestDragAndDropDropBitmap));
			AssertReceivedDropEvent();
			var fileDrop = (string[])dropEvent.Data.GetData(DataFormats.FileDrop);
			AssertEquals(1, fileDrop.Length);
			using (var expectedImage = Bitmap.FromFile(TestMessage.GetTestFilePath("TestBitmap.bmp")))
			using (var actualImage = Bitmap.FromFile(Path.Combine(tempDirectory.DirectoryName, Path.GetFileName(fileDrop[0]))))
			{
				AssertEquals(expectedImage.Size, actualImage.Size);
			}
		}

		public void TestDropFilesWithLongNames()
		{
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.TestDragAndDropDropFilesWithLongNames));
			AssertReceivedDropEvent();
			var fileDrop = (string[])dropEvent.Data.GetData(DataFormats.FileDrop);
			AssertEquals(3, fileDrop.Length);
			var copiedFiles = Directory.GetFiles(tempDirectory.DirectoryName);
			AssertEquals(3, copiedFiles.Length);
			var one = copiedFiles.Single(file => string.IsNullOrEmpty(Path.GetExtension(file)));
			AssertFileSameAsBytes(one, Encoding.UTF8.GetBytes("One"));
			var two = copiedFiles.Single(file => Path.GetExtension(file) == ".msg");
			AssertFileSameAsBytes(two, Encoding.UTF8.GetBytes("Two"));
			var three = copiedFiles.Single(file => Path.GetFileNameWithoutExtension(file) == "_");
			AssertFileSameAsBytes(three, Encoding.UTF8.GetBytes("Three"));
		}

		public void TestDropMessageIsNull()
		{
			try
			{
				EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.TestDragAndDropNullMessage));
				AssertReceivedDropEvent(true);
				AssertEquals(2, ErrorReporter.TotalErrorCount); // IOException is reported again inside the initial report under different keys but both handled.
				Assert(ErrorReporter.ExceptionsThrown.All(exception => exception.Contains("Data was unsuccessfully transferred")));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		protected void AssertReceivedDropEvent(bool expectNoEvent = false)
		{
			for (int i = 0; i < 500 && dropEvent == null; i++)
			{
				Thread.Sleep(50);
				Application.DoEvents();
			}

			if (dropEvent == null && !expectNoEvent)
			{
				Assert($"dropEvent - should not be null{System.Environment.NewLine}{GetFormattedLogs()}", false);
			}
			else if (dropEvent != null && expectNoEvent)
			{
				Assert($"dropEvent - should be null{System.Environment.NewLine}{GetFormattedLogs()}", false);
			}
		}

		string GetFormattedLogs()
		{
			return new StringBuilder()
				.AppendLine("Errors:")
				.AppendLine(clientProcessErrors.ToString())
				.AppendLine("Logs:")
				.AppendLine(clientProcessOutput.ToString()).ToString();
		}

		protected void AssertFile(TempDirectory tempDirectory, string testFileName, string fileDrop)
		{
			AssertEquals(testFileName, Path.GetFileName(fileDrop));
			if (!IsLite)
			{
				AssertNotEquals(TestMessage.GetTestFilePath(testFileName), fileDrop);
				Assert(!File.Exists(fileDrop));
				AssertFileSameAsBytes(TestMessage.GetTestFilePath(testFileName), File.ReadAllBytes(Path.Combine(tempDirectory.DirectoryName, testFileName)));
			}
		}

		public void TestDragDropOnFormCreationThread()
		{
			DisposeTestForm();
			var mainThreadEvent = new AutoResetEvent(false);
			var thread = new Thread(() =>
			{
				SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
				DragDropHandler.testForm = new Form();

				DragDropHandler.testForm.AllowDrop = true;
				DragDropHandler.testForm.DragDrop += TestForm_DragDrop;
				mainThreadEvent.Set();
				DragDropHandler.testForm.ShowDialog();
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Start();

			mainThreadEvent.WaitOne();
			Application.DoEvents();
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.SetDragAndDrop));
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.TestDragAndDropDropTextFile));

			AssertReceivedDropEvent();
			var fileDrop = (string[])dropEvent.Data.GetData(DataFormats.FileDrop);
			AssertEquals(1, fileDrop.Length);
			AssertFile(tempDirectory, "TestTextFile.txt", fileDrop[0]);
		}

		protected override void SetUp()
		{
			base.SetUp();

			tempDirectory = new TempDirectory();
			testDocDirectory = TestMessage.ReleaseResourcesFiles();
			CreateTestForm();
		}

		protected virtual void CreateTestForm()
		{
			DragDropHandler.testForm = new Form();

			DragDropHandler.testForm.AllowDrop = true;
			DragDropHandler.testForm.DragDrop += TestForm_DragDrop;
			DragDropHandler.testForm.Show();
			Application.DoEvents();
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.Test, TestMessage.GetMessage(TestMessage.Action.SetDragAndDrop));
		}

		protected virtual void TestForm_DragDrop(object sender, DragEventArgs e)
		{
			dropEvent = e;
			foreach (string file in (string[])e.Data.GetData(DataFormats.FileDrop))
			{
				File.Copy(file, TempFileHelper.SafePath(tempDirectory.DirectoryName, Path.GetFileName(file)), true);
			}
		}

		protected override void TearDown()
		{
			try
			{
				testDocDirectory.Dispose();
				tempDirectory.Dispose();
				DisposeTestForm();
			}
			finally
			{
				base.TearDown();
			}
		}

		protected virtual void DisposeTestForm()
		{
			DragDropHandler.testForm.Dispose();
			DragDropHandler.testForm = null;
		}

		protected DragEventArgs dropEvent;
		protected TempDirectory tempDirectory;
		protected virtual bool IsLite => false;

		IDisposable testDocDirectory;
	}
}
