using System.IO;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	public sealed class FileMapperForTest : IFileMapper
	{
		public string GetFolderPath(System.Environment.SpecialFolder folder)
		{
			return System.Environment.GetFolderPath(folder);
		}

		public Stream OpenWrite(string unmappedPath)
		{
			return File.Create(unmappedPath);
		}

		public Stream OpenRead(string unmappedPath)
		{
			return File.OpenRead(unmappedPath);
		}

		public bool IsRemote
		{
			get { return false; }
		}
	}
}
