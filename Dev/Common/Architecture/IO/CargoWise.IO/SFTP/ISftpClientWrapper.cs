using System.Collections.Generic;
using System.IO;

namespace CargoWise.IO
{
	public interface ISftpClientWrapper
	{
		void Connect();
		void DownloadFile(string filePath, Stream stream);
		IEnumerable<string> ListDirectory(string path);
		void UploadFile(Stream stream, string path, bool canOverride);
		void DeleteFile(string path);
		void RenameFile(string remoteFilePath, string newName);
		public void ChangeDirectory(string path);
	}
}
