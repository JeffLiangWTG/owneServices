using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using Renci.SshNet.Common;

namespace CargoWise.IO
{
	public class SftpProcessor : ISftpProcessor
	{
		public string ServerName { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public TimeSpan ReadTimeout { get; set; }
		public TimeSpan ConnectTimeout { get; set; }
		readonly Action<string> ErrorLoggingMethod;
		public ISftpClientWrapper SftpClient { get; set; }
		public string MainFolderPath { get; set; }
		public int Port { get; set; }

		public SftpProcessor()
		{
		}

		public SftpProcessor(string serverName, string username, string password, TimeSpan readTimeout, TimeSpan connectTimeout, bool usePassive = true, bool useSecureConnection = false)
			: this(serverName, username, password, null, readTimeout, connectTimeout, usePassive, useSecureConnection)
		{
		}

		public SftpProcessor(string serverName, string username, string password, Action<string> errorLoggingMethod, TimeSpan readTimeout, TimeSpan connectTimeout, bool usePassive = true, bool useSecureConnection = false)
		{
			if (!serverName.StartsWith("sftp://"))
			{
				serverName = "sftp://" + serverName;
			}
			var uri = new Uri(serverName);

			ServerName = uri.Host;
			MainFolderPath = uri.AbsolutePath.Substring(1);
			if (uri.Port > 65535 || uri.Port <= 0)
			{
				Port = 22;
			}
			else
			{
				Port = uri.Port;
			}

			Username = username;
			Password = password;
			ErrorLoggingMethod = errorLoggingMethod;
			ReadTimeout = readTimeout;
			ConnectTimeout = connectTimeout;
			SftpClient = new SftpClientWrapper(ServerName, Port, username, password);
		}

		public void Connect()
		{
			SftpClient.Connect();
			if (!MainFolderPath.IsNullOrEmpty())
			{
				try
				{
					SftpClient.ChangeDirectory(MainFolderPath);
				}
				catch (SftpPathNotFoundException)
				{
					throw new FtpException(FtpException.FtpExceptionType.CheckFileExists, $"The remote path: {MainFolderPath} doesn't exist");
				}
			}
		}

		public void AppendToFile(string localFilePath, string remoteFilePath)
		{
			using (var localFileStream = File.Open(localFilePath, FileMode.Open))
			{
				using (var remoteFileStream = new MemoryStream())
				{
					SftpClient.DownloadFile(remoteFilePath, remoteFileStream);
					remoteFileStream.Position = remoteFileStream.Length;
					localFileStream.CopyTo(remoteFileStream);
					remoteFileStream.Position = 0;
					SftpClient.UploadFile(remoteFileStream, remoteFilePath, true);
				}
			}
		}

		public bool DeleteRemoteFile(string remoteFilePath)
		{
			var result = false;
			try
			{
				SftpClient.DeleteFile(remoteFilePath);
				result = true;
			}
			catch (Exception ex)
			{
				LogError($"Error deleting file {remoteFilePath}: {ex.Message}");
			}
			return result;
		}

		public long DownloadFile(string localFilePath, string remoteFilePath)
		{
			long totalBytesRead = 0;
			using (var remoteFileStream = new MemoryStream())
			{
				SftpClient.DownloadFile(remoteFilePath, remoteFileStream);
				using (var outputStream = new FileStream(localFilePath, FileMode.Create))
				{
					remoteFileStream.CopyTo(outputStream);
					totalBytesRead = outputStream.Length;
				}
			}
			return totalBytesRead;
		}

		public string[] ListDirectory(string remoteDirectoryPath)
		{
			return SftpClient.ListDirectory(remoteDirectoryPath)?.ToArray();
		}

		public void RenameRemoteFile(string remoteSourcePath, string fileName)
		{
			SftpClient.RenameFile(remoteSourcePath, fileName);
		}

#pragma warning disable CW1050 // Use System.TimeSpan Type For A Duration
		public void RenameRemoteFileSeveralAttempts(string remoteSourcePath, string fileName, int tries, int pauseBetweenTriesInSeconds)
#pragma warning restore CW1050 // Use System.TimeSpan Type For A Duration
		{
			var triesLeft = tries;
			while (triesLeft-- > 0)
			{
				try
				{
					RenameRemoteFile(remoteSourcePath, fileName);
					triesLeft = 0;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (triesLeft == 0)
					{
						throw new FtpException(FtpException.FtpExceptionType.RenameFile, string.Format(CultureInfo.InvariantCulture, "Could not rename file after {0} tries.", tries), ex);
					}
					Thread.Sleep(pauseBetweenTriesInSeconds * 1000);
				}
			}
		}

		public void UploadFile(string localFileFullNameAndPath, string remotePathAndName)
		{
			using (var localFileStream = File.Open(localFileFullNameAndPath, FileMode.Open))
			{
				UploadFileStream(localFileStream, remotePathAndName);
			}
		}

		void UploadFileStream(Stream stream, string remotePathAndName)
		{
			SftpClient.UploadFile(stream, remotePathAndName, false);
		}

#pragma warning disable CW1050 // Use System.TimeSpan Type For A Duration
		public void UploadFileSeveralAttempts(string localFileFullNameAndPath, string remotePathAndName, int tries, int pauseBetweenTriesInSeconds)
#pragma warning restore CW1050 // Use System.TimeSpan Type For A Duration
		{
			var triesLeft = tries;
			while (triesLeft-- > 0)
			{
				try
				{
					UploadFile(localFileFullNameAndPath, remotePathAndName);
					triesLeft = 0;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (triesLeft == 0)
					{
						throw new FtpException(FtpException.FtpExceptionType.RenameFile, string.Format(CultureInfo.InvariantCulture, "Could not upload file after {0} tries.", tries), ex);
					}
					Thread.Sleep(pauseBetweenTriesInSeconds * 1000);
				}
			}
		}

#pragma warning disable CW1050 // Use System.TimeSpan Type For A Duration
		public void UploadStreamSeveralAttempts(Stream stream, string remotePathAndName, int tries, int pauseBetweenTriesInSeconds)
#pragma warning restore CW1050 // Use System.TimeSpan Type For A Duration
		{
			var triesLeft = tries;
			while (triesLeft-- > 0)
			{
				try
				{
					UploadFileStream(stream, remotePathAndName);
					triesLeft = 0;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (triesLeft == 0)
					{
						throw new FtpException(FtpException.FtpExceptionType.RenameFile, string.Format(CultureInfo.InvariantCulture, "Could not upload file stream after {0} tries.", tries), ex);
					}
					Thread.Sleep(pauseBetweenTriesInSeconds * 1000);
				}
			}
		}

		public void UploadFileUnique(string localFileFullNameAndPath, string remotePath)
		{
			var fileName = Path.GetFileName(localFileFullNameAndPath);
			if (remotePath.StartsWith("/"))
			{
				remotePath = remotePath.Substring(1);
			}
			var remoteFilePathAndName = remotePath + "/" + fileName;
			UploadFile(localFileFullNameAndPath, remoteFilePathAndName);
		}

		void LogError(string errorMessage)
		{
			if (ErrorLoggingMethod != null)
			{
				ErrorLoggingMethod(errorMessage);
			}
		}
	}
}
