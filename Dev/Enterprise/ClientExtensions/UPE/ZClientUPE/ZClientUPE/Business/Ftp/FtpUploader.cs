using System;
using System.IO;
using System.Net.Sockets;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Renci.SshNet.Common;

namespace Enterprise.Client.UPE.Business.Ftp
{
	public abstract class FtpUploader : FtpTransfer
	{
		public FtpUploader(INotifications notifications, bool useRename = true)
			: base(notifications)
		{
			this.useRename = useRename;
		}
		readonly bool useRename;

		public virtual bool CanUpload()
		{
			bool result = false;
			try
			{
				result = CanOverwriteTargetFile(GetTargetFileSize());
				if (!result)
				{
					WarnTargetFileCannotBeOverwritten();
				}
			}
			catch (Exception ex)
			{
				if (ex is SshException || ex is InvalidOperationException || ex is SocketException)
				{
					ErrorNotification error = new ErrorNotification(ErrorType.Error, ex.Message);
					Notifications.Notify(error);
					return result;
				}
				throw;
			}
			return result;
		}

		public bool UploadToFtpServerAndArchive(TextReader reader)
		{
			bool result;
			try
			{
				ZString tempFileName = Env.GetTempFileName();
				using (StreamWriter writer = new StreamWriter(tempFileName))
				{
					CopyReaderToWriter(reader, writer);
				}
				result = UploadToFtpServerAndArchive(tempFileName);
			}
			catch (IOException ex)
			{
				Notifications.Notify(new ErrorNotification(ErrorType.Error, "IO error writing temporary file to upload; " + ex.Message));
				result = false;
			}
			return result;
		}

		public bool UploadToFtpServerAndArchive(ZString localFileName)
		{
			bool result = false;

			if (localFileName.IsEmpty)
			{
				Notifications.Notify(new ErrorNotification(ErrorType.Error, "File name to be uploaded has to be specified."));
			}
			else
			{
				result = TryUploadToFtpServerRetryThreeTimes(localFileName);
				TryMoveOrDeleteFile(localFileName, result);
			}
			return result;
		}

		bool TryUploadToFtpServerRetryThreeTimes(string localFileName)
		{
			for (int i = 0; i < 3; i++)
			{
				if (i != 0)
				{
					Notifications.Notify(new InfoNotification("Retrying..."));
				}
				if (TryUploadToFtpServer(localFileName))
				{
					return true;
				}
			}
			return false;
		}

		bool TryUploadToFtpServer(string localFileName)
		{
			bool result = false;
			try
			{
				result = DoUpload(localFileName);
			}
			catch (Exception ex)
			{
				if (ex is SshException || ex is InvalidOperationException || ex is SocketException || ex is IOException)
				{
					ErrorNotification error = new ErrorNotification(ErrorType.Error, ex.Message);
					Notifications.Notify(error);
					return result;
				}
				throw;
			}
			return result;
		}

		protected virtual bool CanOverwriteTargetFile(long targetFileSize)
		{
			return false;
		}

		protected virtual ZString WarningMessageWhenFileCannotBeOverwritten
		{
			get { return "Target file already exists and cannot be deleted"; }
		}

		protected long GetTargetFileSize()
		{
			long result = 0;

			using (var sftpClient = GetNewFtpClient())
			{
				sftpClient.Connect();
				sftpClient.ChangeDirectory(UploadDirectory);
				result = GetTargetFileSize(sftpClient);
			}

			return result;
		}

		protected void WarnTargetFileCannotBeOverwritten()
		{
			WarningNotification warning = new WarningNotification(WarningMessageWhenFileCannotBeOverwritten);
			Notifications.Notify(warning);
		}

		protected virtual bool DoUpload(string localFileName)
		{
			bool result = true;

			using (var sftpClient = GetNewFtpClient())
			{
				sftpClient.Connect();
				sftpClient.ChangeDirectory(UploadDirectory);

				if (useRename)
				{
					var tempFileName = GetTempFileName();
					using (var file = File.OpenRead(localFileName))
					{
						sftpClient.UploadFile(file, tempFileName, true);
					}
					try
					{
						if (!sftpClient.Exists(UploadFilename) || DeleteExistingTargetFile(sftpClient))
						{
							sftpClient.RenameFile(tempFileName, UploadFilename);
						}
						else
						{
							WarnTargetFileCannotBeOverwritten();
							result = false;
						}
					}
					finally
					{
						if (sftpClient.Exists(tempFileName))
						{
							sftpClient.Delete(tempFileName);
							result = false;
						}
					}
				}
				else
				{
					using (var file = File.OpenRead(localFileName))
					{
						sftpClient.UploadFile(file, UploadFilename, true);
					}
				}
			}

			return result;
		}

		bool DeleteExistingTargetFile(ISftpClient sftpClient)
		{
			bool result = false;

			if (CanOverwriteTargetFile(GetTargetFileSize(sftpClient)))
			{
				try
				{
					sftpClient.Delete(UploadFilename);
					result = true;
				}
				catch (SshException) { }
			}

			return result;
		}

		protected virtual long GetTargetFileSize(ISftpClient sftpClient)
		{
			long result;

			try
			{
				result = sftpClient.Get(UploadFilename).Length;
			}
			catch (SshException)
			{
				result = 0;
			}

			return result;
		}

		void TryMoveOrDeleteFile(ZString localFileName, bool archive)
		{
			try
			{
				if (archive && !ArchiveDirectory.IsEmpty)
				{
					string destFileName = ZGuid.NewZGuid().ToString() + ".txt";
					string destFile = Path.Combine(ArchiveDirectory, destFileName);
					File.Move(localFileName, destFile);
				}
				else
				{
					File.Delete(localFileName);
				}
			}
			catch (IOException)
			{
				WarningNotification warning = new WarningNotification("Cannot archive or delete file \"" + localFileName + "\"");
				Notifications.Notify(warning);
			}
		}

		void CopyReaderToWriter(TextReader reader, TextWriter writer)
		{
			char[] buffer = new char[255];
			int count = 0;
			do
			{
				count = reader.Read(buffer, 0, buffer.Length);
				writer.Write(buffer, 0, count);
			}
			while (count > 0);
		}

		#region Abstract

		public abstract ZString UploadDirectory { get; }
		public abstract ZString UploadFilename { get; }
		public abstract ZString ArchiveDirectory { get; }

		#endregion
	}
}
