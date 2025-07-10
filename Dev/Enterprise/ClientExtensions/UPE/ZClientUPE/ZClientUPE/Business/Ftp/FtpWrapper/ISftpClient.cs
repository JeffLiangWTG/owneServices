using System;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet.Sftp;

namespace Enterprise.Client.UPE.Business.Ftp
{
	public interface ISftpClient : IDisposable
	{
		void Connect();
		void Disconnect();
		bool IsConnected { get; }
		void ChangeDirectory(string path);
		SftpFile Get(string path);
		void UploadFile(Stream input, string path, bool canOverride, Action<ulong> uploadCallback = null);
		void UploadFile(Stream input, string path, Action<ulong> uploadCallback = null);
		bool Exists(string path);
		void RenameFile(string oldPath, string newPath);
		void Delete(string path);
		void DownloadFile(string path, Stream output, Action<ulong> downloadCallback = null);
		IEnumerable<SftpFile> ListDirectory(string path, Action<int> listCallback = null);
	}
}
