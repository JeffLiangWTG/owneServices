using System;
using System.IO;

namespace CargoWise.IO
{
	public interface IFtpProcessor
	{
		string ServerName { get; set; }
		string Username { get; set; }
		string Password { get; set; }
		TimeSpan ReadTimeout { get; set; }
		TimeSpan ConnectTimeout { get; set; }

		void AppendToFile(string localFilePath, string remoteFilePath);
		bool DeleteRemoteFile(string remoteFilePath);
		long DownloadFile(string localFilePath, string remoteFilePath);
		string[] ListDirectory(string remoteDirectoryPath);
		void RenameRemoteFile(string remoteSourcePath, string fileName);
		void RenameRemoteFileSeveralAttempts(string remoteSourcePath, string fileName, int tries, int pauseBetweenTriesInSeconds);
		void UploadFile(string localFileFullNameAndPath, string remotePathAndName);
		void UploadFileSeveralAttempts(string localFileFullNameAndPath, string remotePathAndName, int tries, int pauseBetweenTriesInSeconds);
		void UploadStreamSeveralAttempts(Stream stream, string remotePathAndName, int tries, int pauseBetweenTriesInSeconds);
		void UploadFileUnique(string localFileFullNameAndPath, string remotePath);
	}
}
