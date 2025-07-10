using System;
using System.IO;

namespace CargoWise.Loader.Common
{
	public sealed class FileProxy : IFileProxy
	{
		public FileProxy()
		{
		}

		public void Copy(string sourceFileName, string destFileName, bool overwrite)
		{
			File.Copy(sourceFileName, destFileName, overwrite);
		}

		public FileStream Create(string path)
		{
			return File.Create(path);
		}

		public void Delete(string path)
		{
			File.Delete(path);
		}

		public bool Exists(string path)
		{
			return File.Exists(path);
		}

		public DateTime GetLastWriteTimeUtc(string path)
		{
			return File.GetLastWriteTimeUtc(path);
		}

		public void Move(string sourceFileName, string destFileName)
		{
			File.Move(sourceFileName, destFileName);
		}

		public FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			return File.Open(path, mode, access, share);
		}

		public FileStream OpenRead(string path)
		{
			return File.OpenRead(path);
		}

		public void SetAttributes(string path, FileAttributes fileAttributes)
		{
			File.SetAttributes(path, fileAttributes);
		}

		public void DeleteOrRename(string path)
		{
			try
			{
				File.Delete(path);
			}
			catch (UnauthorizedAccessException)
			{
				if (Path.GetExtension(path).ToLower() != ".~bk")
				{
					File.Move(path, Path.Combine(Path.GetDirectoryName(path), Guid.NewGuid().ToString() + ".~bk"));
				}
			}
		}
	}
}
