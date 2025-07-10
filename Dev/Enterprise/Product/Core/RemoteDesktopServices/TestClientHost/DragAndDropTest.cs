using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using CargoWise.Interop.DataObjects;
using Enterprise.RemoteDesktopServices.Client;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Testing;
using NUnit.Framework;
using Win32.WtsApi32;

namespace Enterprise.RemoteDesktopServices.TestClientHost
{
	class DragAndDropTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Testing")]
		public void TestMultipleSessions()
		{
			WtsPlugin plugin1 = new WtsPluginForTest();
			WtsPlugin plugin2 = new WtsPluginForTest();
			IWTSPlugin iPlugin1 = plugin1;
			IWTSPlugin iPlugin2 = plugin2;
			DummyManager manager = new DummyManager();
			iPlugin1.Initialize(manager);
			int messageCount = MessageHandlers.RegisteredMessageTypes.Length;
			iPlugin2.Initialize(manager);
			if (messageCount != MessageHandlers.RegisteredMessageTypes.Length)
			{
				Console.Error.WriteLine("TestMultipleSessions: Multiple plugins should register message handlers once only");
			}
			if (manager.callsToCreateListener != 2)
			{
				Console.Error.WriteLine("TestMultipleSessions: Plugin should allow multiple instances to be initialized (one is needed per session)");
			}
		}

		class DummyManager : IWTSVirtualChannelManager
		{
			internal int callsToCreateListener;

			public void CreateListener(string pszChannelName, int ulFlags, IWTSListenerCallback pListenerCallback, out IWTSListener ppListener)
			{
				++callsToCreateListener;
				ppListener = null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "(UIntPtr)1 is not a redundant cast in .Net Framework")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Testing")]
		public void TestDragEffects()
		{
			string tempFile = TempForTest.GetTempFileName();
			using (var form1 = new Form())
			using (var form2 = new Form())
			using (var dataObject = new ZAutoDeleteFileDropDataObject(tempFile, true))
			{
				var form1DropTarget = new DropTargetTest("TestDragEffects", (UIntPtr)1);
				form1.Show();
				form2.Show();
				int effect;
				form1DropTarget.DragEnter(dataObject, 0, 0, out effect);
				if (effect != 1)
				{
					Console.Error.WriteLine("TestDragEffects: DragEnter effect not 1");
				}
				form1DropTarget.DragOver(0, 0, out effect);
				if (effect != 1)
				{
					Console.Error.WriteLine("TestDragEffects: DragOver effect not 1");
				}
				form1DropTarget.Drop(dataObject, 0, 0, out effect);
				form1DropTarget.DragLeave();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "(UIntPtr)2 is not a redundant cast in .Net Framework")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Testing")]
		public void TestDragUnsupported()
		{
			DataObject dataObject = new DataObject();
			using (var form = new Form())
			{
				form.Show();
				var dropTarget = new DropTargetTest("TestDragUnsupported", (UIntPtr)2);
				int effect;
				dropTarget.DragEnter(dataObject, 0, 0, out effect);
				if (effect != 0)
				{
					Console.Error.WriteLine("TestDragUnsupported: DragEnter effect not 0");
				}
				dropTarget.DragOver(0, 0, out effect);
				if (effect != 0)
				{
					Console.Error.WriteLine("TestDragUnsupported: DragOver effect not 0");
				}
				dropTarget.Drop(dataObject, 0, 0, out effect);
				if (effect != 0)
				{
					Console.Error.WriteLine("TestDragUnsupported: Drop effect not 0");
				}
			}
		}

		public void TestDropTextFile()
		{
			TestDropFiles(3, TestMessage.GetTestFilePath("TestTextFile.txt"));
		}

		public void TestDropMultipleFiles()
		{
			TestDropFiles(4, TestMessage.GetTestFilePath("Test.xls"), TestMessage.GetTestFilePath("Sample.PDF"), TestMessage.GetTestFilePath("Email with Image.msg"));
		}

		void TestDropFiles(int hwnd, params string[] filePath)
		{
			if (DropTarget.IsDragDropLiteTest)
			{
				for (int i = 0; i < filePath.Length; i++)
				{
					if (filePath[i].IndexOf(Path.VolumeSeparatorChar) == -1)
					{
						var localFile = Path.Combine(TempForTest.TempPath, Path.GetFileName(filePath[i]));
						File.Copy(filePath[i], localFile, true);
						filePath[i] = localFile;
					}
				}
			}

			using (var form = new Form())
			{
				form.Show();
				var dataObject = new DataObject(DataFormats.FileDrop, filePath);
				var dropTarget = new DropTargetTest("TestDropFiles", (UIntPtr)hwnd);
				int effect;
				dropTarget.Drop(dataObject, 0, 0, out effect);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "(UIntPtr)5 is not a redundant cast in .Net Framework")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Testing")]
		public void TestDropBitmap()
		{
			using (var form = new Form())
			using (var bitmap = Bitmap.FromFile(TestMessage.GetTestFilePath("TestBitmap.bmp")))
			{
				form.Show();
				var dataObject = new DataObject(DataFormats.Bitmap, bitmap);
				var dropTarget = new DropTargetTest("TestDropBitmap", (UIntPtr)5);
				int effect;
				dropTarget.Drop(dataObject, 0, 0, out effect);
				if (effect != 1)
				{
					Console.Error.WriteLine("TestDropBitmap: Drop effect not 1");
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "(UIntPtr)6 is not a redundant cast in .Net Framework")]
		public void TestDropFilesWithLongNames()
		{
			var dragDropMessage = new DragDropMessage();
			string name =
"I was a Flower of the mountain yes when I put the rose in my hair like the Andalusian girls used or shall I wear a red yes and how he kissed me under the Moorish wall and I thought well as well him as another and then I asked him with my eyes to ask again yes and then he asked me would I yes to say yes my mountain flower and first I put my arms around him yes and drew him down to me so he could feel";
			dragDropMessage.fileDrop.Add(new DragDropMessage.FileDropData(name, Encoding.UTF8.GetBytes("One")));
			dragDropMessage.fileDrop.Add(new DragDropMessage.FileDropData(name + ".msg", Encoding.UTF8.GetBytes("Two")));
			dragDropMessage.fileDrop.Add(new DragDropMessage.FileDropData("_." + name, Encoding.UTF8.GetBytes("Three")));
			WtsPlugin.AllChannels[0].Send(MessageChannel.ConstructMessage(EnterpriseChannelMessageTypes.StartDrop, Encoding.UTF8.GetBytes(WindowCaptionUtils.Base64EncodeCaptionAndHandle("TestDropFilesWithLongNames", (UIntPtr)6))));
			WtsPlugin.AllChannels[0].Send(MessageChannel.Serialize(EnterpriseChannelMessageTypes.DragDrop, dragDropMessage));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "(UIntPtr)1 is not a redundant cast in .Net Framework")]
		public void TestDropFile(string filePath)
		{
			using (var form = new Form())
			{
				form.Show();

				var dataObject = new DataObject(DataFormats.FileDrop, new[] { filePath });
				var dropTarget = new DropTargetTest(nameof(TestDropFile), (UIntPtr)1);
				dropTarget.Drop(dataObject, 0, 0, out _);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "(UIntPtr)7 is not a redundant cast in .Net Framework")]
		public void TestNullMessage()
		{
			var dragDropMessage = new DragDropMessage();
			dragDropMessage.fileDrop.Add(null);
			WtsPlugin.AllChannels[0].Send(MessageChannel.ConstructMessage(EnterpriseChannelMessageTypes.StartDrop, Encoding.UTF8.GetBytes(WindowCaptionUtils.Base64EncodeCaptionAndHandle("TestNullMessage", (UIntPtr)7))));
			WtsPlugin.AllChannels[0].Send(MessageChannel.Serialize(EnterpriseChannelMessageTypes.DragDrop, dragDropMessage));
		}

		[DllImport("ole32.dll")]
		static extern int RevokeDragDrop(IntPtr hwnd);
	}

	internal class DropTargetTest : DropTarget
	{
		public DropTargetTest(string testFormText, UIntPtr hWnd) : base(hWnd)
		{
			this.testFormText = Convert.ToBase64String(Encoding.UTF8.GetBytes(testFormText));
		}
		readonly string testFormText;

		protected override string CaptionOfTargetWindow()
		{
			return testFormText;
		}
	}
}
