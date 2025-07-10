using System.IO;

namespace Enterprise.DataTransfer.IO
{
	public abstract class FileSystemInformation
	{
		protected FileSystemInformation(string uri)
		{
		}

		public string FullName
		{
			get { return FileSystemInfo.FullName; }
		}

		public string Name
		{
			get { return FileSystemInfo.Name; }
		}

		public bool Exists
		{
			get { return FileSystemInfo.Exists; }
		}

		public void Delete()
		{
			if (FileSystemInfo.Exists)
			{
				FileSystemInfo.Attributes = FileAttributes.Normal;
				FileSystemInfo.Delete();
			}
		}

		protected FileSystemInfo FileSystemInfo;
	}
}
