using System;
using System.Diagnostics;
using System.IO;
using CargoWise.Application;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "This is the implemenation of the alternative to Process.Start")]
	public static class FileOpener
	{
		public static object Open(string filePath)
		{
			object handle = null;
			if (ObjectFactory.Get<TerminalService>().IsRemoteAppSession && RemoteFile.IsSupported)
			{
				handle = new RemoteFile(filePath, File.ReadAllBytes(filePath), false);
				try
				{
					((RemoteFile)handle).Open();
				}
				catch (OperationCanceledException ex)
				{
					var innerException = ex.InnerException;
					var innerExceptionInformation = innerException == null ? "" : "\r\n" + ResString.GetMultilingualString("A1D95247-DC57-4BFC-BA32-43808B9CF5C0", "Inner Exception Type: {0}\r\nInner Exception message: {1}", innerException.GetType().ToString(), innerException.Message);
					Globals.Message.ShowWarning(ResString.GetMultilingualString("90479F7F-B140-4565-8B7D-62E80670F0AC", "Failed to send file to remote client. Operation canceled due to network issue.\r\nException Type: {0}\r\nException message: {1}{2}", ex.GetType().ToString(), ex.Message, innerExceptionInformation));
				}
			}
			else if (DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode == RemoteConnectingModes.ConnectorOnly)
			{
				Enterprise.ZArchitecture.Environment.Globals.Message.ShowError(ZTerminalService.ClientPluginApplicationNotInstalledError);
			}
			else
			{
				handle = Process.Start(filePath);// This is the implemenation of the alternative to Process.Start
			}
			return handle;
		}
	}
}
