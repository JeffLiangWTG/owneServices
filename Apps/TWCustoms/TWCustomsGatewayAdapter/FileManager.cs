using System;
using System.IO;

namespace CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter
{
	public interface IFileManager
	{
		bool FileExists(string path);
		bool DirectoryExists(string path);
		byte[] GetBytes(string filePath);
		string GetText(string filePath);
		void Delete(string filePath);
		FileInfo[] GetTWCustomsResponseFiles(string folderPath);
		void CreateFile(string filePath, string content, bool isBase64Content);
	}

	public class FileManager : IFileManager
	{
		public void CreateFile(string filePath, string content, bool isBase64Content)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(filePath));

			if (isBase64Content)
			{
				var bytes = Convert.FromBase64String(content);
				File.WriteAllBytes(filePath, bytes);
			}
			else
			{
				File.WriteAllText(filePath, content);
			}
		}

		public bool FileExists(string filePath)
		{
			return File.Exists(filePath);
		}

		public byte[] GetBytes(string filePath)
		{
			if (string.IsNullOrEmpty(filePath)) return null;
			if (!File.Exists(filePath)) return null;
			return File.ReadAllBytes(filePath);
		}

		public string GetText(string filePath)
		{
			if (string.IsNullOrEmpty(filePath)) return null;
			if (!File.Exists(filePath)) return null;
			return File.ReadAllText(filePath);
		}

		public void Delete(string filePath)
		{
			if (string.IsNullOrEmpty(filePath)) return;
			File.Delete(filePath);
		}

		public FileInfo[] GetTWCustomsResponseFiles(string folderPath)
		{
			var directory = new DirectoryInfo(folderPath);
			return directory.GetFiles();
		}

		public bool DirectoryExists(string path)
		{
			return Directory.Exists(path);
		}
	}
}
