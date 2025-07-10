using System.IO;
using System.Runtime.InteropServices;

namespace Enterprise.DataTransfer.IO
{
	public class FileInformation : FileSystemInformation
	{
		public FileInformation(string uri)
			: base(uri)
		{
			FileSystemInfo = new FileInfo(uri);
		}

		public string Extension
		{
			get { return FileSystemInfo.Extension; }
		}

		public Stream Open()
		{
			Stream streamOfFile = null;
			try
			{
				streamOfFile = new FileStream(FileSystemInfo.FullName, FileMode.Open);
			}
			catch (IOException ex)
			{
				var errorCode = Marshal.GetHRForException(ex) & ((1 << 16) - 1);
				if (!(errorCode == 32 || errorCode == 33)) // File is locked
				{
					throw;
				}
			}
			return streamOfFile;
		}

		public DirectoryInfo ParentDirectory
		{
			get
			{
				var dirName = Path.GetDirectoryName(FullName);
				return new DirectoryInfo(dirName);
			}
		}

		public FileInfo FileInfo
		{
			get { return (FileInfo)FileSystemInfo; }
		}
	}
}
