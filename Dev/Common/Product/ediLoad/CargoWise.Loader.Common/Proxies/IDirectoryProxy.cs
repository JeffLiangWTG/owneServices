using System.IO;

namespace CargoWise.Loader.Common
{
	public interface IDirectoryProxy
	{
		DirectoryInfo CreateDirectory(string path);
		bool Exists(string path);
	}
}