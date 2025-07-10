using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Renci.SshNet.Common;

namespace Enterprise.Client.UPE.Business.Ftp
{
	public class BISIDownloader : FtpTransfer, IBISIDownloader
	{
		public BISIDownloader(INotifications notifications)
			: base(notifications)
		{
		}

		public string DownloadedFileName
		{
			get
			{
				if (fDownloadedFileName == null)
				{
					fDownloadedFileName = "";
				}
				return fDownloadedFileName;
			}
		}

		public bool DownloadFile(string remoteFileFullPathName)
		{
			return DownloadFile(remoteFileFullPathName, false);
		}

		public bool DownloadFile(string remoteFileFullPathName, bool deleteRemoteFileWhenFinished)
		{
			try
			{
				fDownloadedFileName = DoDownload(remoteFileFullPathName, deleteRemoteFileWhenFinished);
			}
			catch (Exception ex)
			{
				if (ex is SshException || ex is InvalidOperationException || ex is SocketException)
				{
					fDownloadedFileName = "";
					ErrorNotification errorNotification = new ErrorNotification(ErrorType.Error, ex.Message);
					Notifications.Notify(errorNotification);
					return false;
				}
				throw;
			}

			return true;
		}

		public List<string> ListZipFiles()
		{
			List<string> result = new List<string>();
			try
			{
				using (var sftpClient = GetNewFtpClient())
				{
					sftpClient.Connect();
					string rootDir = UPEDataRegistry.Instance.CODFilesParentFolder;
					ListZipFilesInDirectory(sftpClient, rootDir, ref result);
				}
			}
			catch (Exception ex)
			{
				if (ex is OutOfMemoryException)
				{
					Notifications.AddWarning(string.Format(CultureInfo.InvariantCulture, "Out of memory! Please reduce the amount of files or directories. The number of zip files is {0}.", result.Count));
				}
				else if (ex is SshException || ex is InvalidOperationException || ex is SocketException)
				{
					ErrorNotification errorNotification = new ErrorNotification(ErrorType.Error, ex.Message);
					Notifications.Notify(errorNotification);
				}
				else
				{
					throw;
				}
			}
			return result;
		}

		#region Overrides

		public override ZString ServerAddress
		{
			get { return UPEDataRegistry.Instance.BISISftpServerAddress; }
		}

		public override int ServerPort
		{
			get { return UPEDataRegistry.Instance.BISISftpServerPort; }
		}

		public override ZString ServerName
		{
			get { return "BISI Mainframe"; }
		}

		public override ZString Username
		{
			get { return UPEDataRegistry.Instance.BISISftpServerUsername; }
		}

		public override ZString Password
		{
			get { return UPEDataRegistry.Instance.BISISftpServerPassword; }
		}

		#endregion

		#region Implementation

		string DoDownload(string remoteFileFullPath, bool deleteRemoteFileWhenFinished)
		{
			string result = null;
			using (var sftpClient = GetNewFtpClient())
			{
				sftpClient.Connect();
				sftpClient.ChangeDirectory(Path.GetDirectoryName(remoteFileFullPath));
				string remoteFileName = Path.GetFileName(remoteFileFullPath);
				string localPath = Path.Combine(Env.TempPath, "BISITemp");
				string localFileName = GetTempFileName();
				if (!Directory.Exists(localPath))
				{
					Directory.CreateDirectory(localPath);
				}

				using (var localFile = File.Create(Path.Combine(localPath, localFileName)))
				{
					sftpClient.DownloadFile(remoteFileName, localFile);
				}

				result = Path.Combine(localPath, localFileName);
				if (deleteRemoteFileWhenFinished)
				{
					sftpClient.Delete(remoteFileName);
				}
			}
			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "Recursion! It's better to build upon the same list.")]
		protected virtual void ListZipFilesInDirectory(ISftpClient ftpClient, string directoryName, ref List<string> zipFileList)
		{
			var files = ftpClient.ListDirectory(directoryName).Where(file => !file.IsDirectory);
			foreach (var file in files)
			{
				bool isZipFile = file.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
				if (isZipFile)
				{
					zipFileList.Add(Path.Combine(directoryName, file.Name));
				}
			}

			var directories = ftpClient.ListDirectory(directoryName).Where(file => file.IsDirectory);
			foreach (var directory in directories)
			{
				ListZipFilesInDirectory(ftpClient, Path.Combine(directoryName, directory.Name), ref zipFileList);
			}
		}

		string fDownloadedFileName;

		#endregion
	}
}
