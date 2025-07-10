using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class OpenFileChangedHandler : IMessageHandler
	{
		public void Handle(IEnterpriseChannel channel, Stream messageData)
		{
			ApplicationDispatcher.Current?.BeginInvoke(new OpenFileChangedHandlerDelegate(DoHandle), messageData);
		}

		void DoHandle(Stream messageData)
		{
			Guid id = messageData.ReadGuid();
			RemoteFile remoteFile;
			RemoteFiles.TryGetValue(id, out remoteFile);
			if (remoteFile != null)
			{
				remoteFile.OnFileChanged();
			}
		}

		internal static Dictionary<Guid, RemoteFile> RemoteFiles = new Dictionary<Guid, RemoteFile>();
		delegate void OpenFileChangedHandlerDelegate(Stream messageData);
	}
}
