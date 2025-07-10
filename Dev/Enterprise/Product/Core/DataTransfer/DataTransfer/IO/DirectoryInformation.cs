using System;
using System.Collections;
using System.IO;

namespace Enterprise.DataTransfer.IO
{
	public class DirectoryInformation : FileSystemInformation
	{
		public DirectoryInformation(string uri)
			: base(uri)
		{
			FileSystemInfo = new DirectoryInfo(uri);
		}

		#region GetFiles

		public FileInformation[] GetFiles()
		{
			return GetFiles("");
		}

		public FileInformation[] GetFiles(String searchPattern)
		{
			var fileList = new ArrayList();

			var files = !string.IsNullOrEmpty(searchPattern) ? Directory.GetFiles(FileSystemInfo.FullName, searchPattern) : Directory.GetFiles(FileSystemInfo.FullName);
			foreach (var pathToFile in files)
			{
				var fileInDirectory = new FileInformation(pathToFile);
				var stream = fileInDirectory.Open();

				if (stream == null)
				{
					continue;
				}

				stream.Close();
				fileList.Add(fileInDirectory);
			}

			return (FileInformation[])fileList.ToArray(typeof(FileInformation));
		}

		#endregion

		#region Delete

		public void Delete(bool recursive)
		{
			((DirectoryInfo)FileSystemInfo).Delete(recursive);
		}

		#endregion
	}
}
