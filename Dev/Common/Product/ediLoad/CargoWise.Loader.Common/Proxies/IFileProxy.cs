using System;
using System.IO;

namespace CargoWise.Loader.Common
{
	public interface IFileProxy
	{
		void Copy(string sourceFileName, string destFileName, bool overwrite);
		FileStream Create(string path);
		void Delete(string path);
		bool Exists(string path);
		DateTime GetLastWriteTimeUtc(string path);
		void Move(string sourceFileName, string destFileName);
		FileStream Open(string path, FileMode mode, FileAccess access, FileShare share);
		FileStream OpenRead(string path);
		void SetAttributes(string path, FileAttributes fileAttributes);
		void DeleteOrRename(string path);
	}
}