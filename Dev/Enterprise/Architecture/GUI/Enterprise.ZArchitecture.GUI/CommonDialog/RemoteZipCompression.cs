using System;
using System.IO;
using System.IO.Compression;

namespace Enterprise.ZArchitecture.GUI
{
	public static class RemoteZipCompression
	{
		public static bool Zip(string sourceFolder, string targetZipFile)
		{
			try
			{
				using (Stream zipStream = ZSaveFileDialog.OpenFile(targetZipFile))
				using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
				{
					if (zipStream == Stream.Null)
					{
						return false;
					}

					foreach (string fPath in Directory.GetFiles(sourceFolder))
					{
						if (File.Exists(fPath))
						{
							archive.CreateEntryFromFile(fPath, Path.GetFileName(fPath));
						}
					}
				}
			}
			catch (Exception ex) when (ex is ArgumentException || ex is IOException)
			{
				return false;
			}
			return true;
		}
	}
}
