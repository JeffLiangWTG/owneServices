using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using CargoWise.Interop.DataObjects;
using Enterprise.RemoteDesktopServices.MessageElements;

namespace Enterprise.RemoteDesktopServices.Server.COM
{
	class DragDropHandler : XmlMessageHandler<DragDropMessage>
	{
		protected override void Handle(IEnterpriseChannel channel, DragDropMessage message)
		{
			var activeWindow = GetForegroundWindow();
			uint activeWindowProcess;
			GetWindowThreadProcessId(activeWindow, out activeWindowProcess);
			if (Process.GetCurrentProcess().Id == activeWindowProcess)
			{
				string[] fileNames = Array.ConvertAll(message.fileDrop.ToArray(), item => TempFileHelper.GetTempFile(item.fileName));
				for (int i = 0; i < message.fileDrop.Count; i++)
				{
					File.WriteAllBytes(fileNames[i], message.fileDrop[i].fileData);
				}
				remoteDropHandler.HandleRemoteFileDrop(fileNames);
			}
		}

		internal static void RegisterRemoteDropHandler(IRemoteFileDropHandler handler)
		{
			remoteDropHandler = handler;
		}

		static IRemoteFileDropHandler remoteDropHandler;

		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", SetLastError = true)]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
	}
}
