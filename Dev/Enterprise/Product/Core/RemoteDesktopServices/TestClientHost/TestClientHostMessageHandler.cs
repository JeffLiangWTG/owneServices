using System.IO;
using Enterprise.RemoteDesktopServices.Client;
using Enterprise.RemoteDesktopServices.Testing;

namespace Enterprise.RemoteDesktopServices.TestClientHost
{
	public class TestClientHostMessageHandler : IMessageHandler
	{
		public void Handle(IEnterpriseChannel channel, Stream messageData)
		{
			ShellHook.Initialize();

			var (action, filePath) = TestMessage.ParseMessage(messageData);
			switch (action)
			{
				case TestMessage.Action.TestDragAndDropMultipleSessions:
					new DragAndDropTest().TestMultipleSessions();
					break;

				case TestMessage.Action.TestDragAndDropDragEffects:
					new DragAndDropTest().TestDragEffects();
					break;

				case TestMessage.Action.TestDragAndDropDragUnsupported:
					new DragAndDropTest().TestDragUnsupported();
					break;

				case TestMessage.Action.TestDragAndDropDropTextFile:
					new DragAndDropTest().TestDropTextFile();
					break;

				case TestMessage.Action.TestDragAndDropDropMultipleFiles:
					new DragAndDropTest().TestDropMultipleFiles();
					break;

				case TestMessage.Action.TestDragAndDropDropBitmap:
					new DragAndDropTest().TestDropBitmap();
					break;

				case TestMessage.Action.TestDragAndDropDropFilesWithLongNames:
					new DragAndDropTest().TestDropFilesWithLongNames();
					break;
				case TestMessage.Action.SetDragAndDropLite:
					DropTarget.IsDragDropLiteTest = true;
					break;
				case TestMessage.Action.SetDragAndDrop:
					DropTarget.IsDragDropTest = true;
					break;

				case TestMessage.Action.TestDragAndDropWithFilePath:
					new DragAndDropTest().TestDropFile(filePath);
					break;

				case TestMessage.Action.TestDragAndDropNullMessage:
					new DragAndDropTest().TestNullMessage();
					break;
			}
		}
	}
}
