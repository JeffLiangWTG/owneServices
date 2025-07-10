using System.IO;

namespace CargoWise.Loader.Common
{
	public sealed class DirectoryProxy : IDirectoryProxy
	{
		public DirectoryProxy()
		{
		}

		public DirectoryInfo CreateDirectory(string path)
		{
			return Directory.CreateDirectory(path);
		}

		public bool Exists(string path)
		{
			return Directory.Exists(path);
		}
	}
}
