using System.Diagnostics;

namespace CargoWise.Loader.Common
{
	sealed class FileVersionInfoProxy : IFileVersionInfoProxy
	{
		readonly FileVersionInfo versionInfo;

		public FileVersionInfoProxy(string fileName)
		{
			versionInfo = FileVersionInfo.GetVersionInfo(fileName);
		}

		public int FileBuildPart
		{
			get { return versionInfo.FileBuildPart; }
		}
	}
}