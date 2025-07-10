using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Renci.SshNet;

namespace Enterprise.Client.UPE.Business.Ftp
{
	public abstract class FtpTransfer
	{
		public FtpTransfer(INotifications notifications)
		{
			this.Notifications = notifications;
		}

		public void DeleteRemoteFile(List<string> remoteFilesFullPath)
		{
			if (remoteFilesFullPath != null)
			{
				remoteFilesFullPath.Sort();
				using (var sftpClient = GetNewFtpClient())
				{
					sftpClient.Connect();
					foreach (string remoteFileFullPath in remoteFilesFullPath)
					{
						sftpClient.Delete(remoteFileFullPath);
					}
				}
			}
		}

		protected virtual ZString GetTempFileName()
		{
			return string.Concat(ZGuid.NewZGuid(), ".txt");
		}

		protected virtual ISftpClient GetNewFtpClient()
		{
			return new SftpClientWrapper(new PasswordConnectionInfo(ServerAddress, ServerPort, Username, Password));
		}

		#region Abstract

		public abstract ZString ServerName { get; }
		public abstract ZString ServerAddress { get; }
		public abstract int ServerPort { get; }
		public abstract ZString Username { get; }
		public abstract ZString Password { get; }

		#endregion

		public readonly INotifications Notifications;
	}
}
