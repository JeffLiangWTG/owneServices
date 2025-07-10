using System;
using System.IO;
using CargoWise.IO;
using Enterprise.Integration;

namespace Enterprise.Accounting.Business.ComplianceReport.IDEA
{
	public class IDEATempFiles : IDisposable
	{
		public IDEATempFiles(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;
		}

		public string FolderName => TempDir ?? "";
		public long TotalSizeUncompressed { get; private set; }

		TempDirectory TempDir;
		readonly ILogger ServiceLogger;

		public void WriteFileToTempFolder(string filename, string data)
		{
			if (TempDir == null)
			{
				TempDir = new TempDirectory();
				ServiceLogger.Log(LogType.Debug, $"Using temp folder {TempDir.DirectoryName} on computer {System.Environment.MachineName}");
			}
			File.AppendAllText(Path.Combine(TempDir, filename), data, System.Text.Encoding.UTF8);
		}

		public void AddFile(string filename, string filedata, int maxSize, bool fileIsComplete, IDEAZipFile zipCreator, IDEAeDocs docManager)
		{
			WriteFileToTempFolder(filename, filedata);
			TotalSizeUncompressed += filedata.Length;
			if (fileIsComplete && TotalSizeUncompressed * (1 - IDEAZipFile.EstimatedCompressionRate / 100.0f) >= maxSize)
			{
				zipCreator.CreateZipFileForEDocs(FolderName, docManager);
				DeleteAllFiles();
				TotalSizeUncompressed = 0;
			}
		}

		public void DeleteAllFiles()
		{
			if (TempDir != null)
			{
				var directoryInfo = new DirectoryInfo(FolderName);
				foreach (var file in directoryInfo.GetFiles())
				{
					file.Delete();
				}
			}
		}

		public void Dispose()
		{
			if (TempDir != null)
			{
				try
				{
					TempDir.Dispose();
				}
				catch (IOException e)
				{
					ServiceLogger.Log(LogType.Warning, $"Unable to remove temp folder: {e.Message}");
				}
			}
		}
	}
}
