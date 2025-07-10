using System.IO;

namespace Enterprise.ZArchitecture.DataMapping
{
	public interface IFileMapper
	{
		string GetFolderPath(System.Environment.SpecialFolder folder);
		Stream OpenWrite(string unmappedPath);
		Stream OpenRead(string unmappedPath);
		bool IsRemote { get; }
	}
}
