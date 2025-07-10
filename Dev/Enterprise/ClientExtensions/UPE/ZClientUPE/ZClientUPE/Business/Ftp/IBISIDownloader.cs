using System.Collections.Generic;

namespace Enterprise.Client.UPE.Business.Ftp
{
	public interface IBISIDownloader
	{
		string DownloadedFileName { get; }
		bool DownloadFile(string remoteFileFullPathName);
		void DeleteRemoteFile(List<string> remoteFilesFullPath);
		List<string> ListZipFiles();
	}
}
