using System;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet;

namespace CargoWise.IO
{
	public class SftpClientWrapper : ISftpClientWrapper, IDisposable
	{
		readonly SftpClient SftpClient;

		public SftpClientWrapper(string serverName, int port, string username, string password)
		{
			SftpClient = new SftpClient(serverName, port, username, password);
		}

		public void Connect()
		{
			SftpClient.Connect();
		}

		public void DownloadFile(string filePath, Stream stream)
		{
			SftpClient.DownloadFile(filePath, stream);
			stream.Position = 0;
		}

		public IEnumerable<string> ListDirectory(string path)
		{
			var result = new List<string>();
			foreach (var fileOrDirectory in SftpClient.ListDirectory(path))
			{
				result.Add(fileOrDirectory.Name);
			}
			return result;
		}

		public void UploadFile(Stream stream, string path, bool canOverride)
		{
			SftpClient.UploadFile(stream, path, canOverride);
		}

		public void DeleteFile(string path)
		{
			SftpClient.DeleteFile(path);
		}

		public void RenameFile(string remoteFilePath, string newName)
		{
			var remoteFile = SftpClient.Get(remoteFilePath);
			if (remoteFile == null)
			{
				return;
			}
			var newFilePath = remoteFile.FullName.Replace(remoteFile.Name, newName);
			SftpClient.RenameFile(remoteFilePath, newFilePath);
		}

		public void ChangeDirectory(string path)
		{
			SftpClient.ChangeDirectory(path);
		}

		public void Dispose()
		{
			SftpClient?.Dispose();
		}
	}
}
