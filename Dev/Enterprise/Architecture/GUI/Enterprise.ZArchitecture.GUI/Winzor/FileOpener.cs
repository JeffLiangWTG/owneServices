using System;
using System.IO;
using CargoWise.Application;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public static class FileOpener
	{
		public static object Open(string filePath)
		{
			var handle = ObjectFactory.Get<IRemoteFile>(nameof(IRemoteFile), filePath, File.ReadAllBytes(filePath), false, false);
			try
			{
				handle.Open();
			}
			catch (OperationCanceledException ex)
			{
				var innerException = ex.InnerException;
				var innerExceptionInformation = innerException == null ? "" : "\r\n" + ResString.GetMultilingualString("A1D95247-DC57-4BFC-BA32-43808B9CF5C0", "Inner Exception Type: {0}\r\nInner Exception message: {1}", innerException.GetType().ToString(), innerException.Message);
				Globals.Message.ShowWarning(ResString.GetMultilingualString("90479F7F-B140-4565-8B7D-62E80670F0AC", "Failed to send file to remote client. Operation canceled due to network issue.\r\nException Type: {0}\r\nException message: {1}{2}", ex.GetType().ToString(), ex.Message, innerExceptionInformation));
			}
			return handle;
		}
	}
}
