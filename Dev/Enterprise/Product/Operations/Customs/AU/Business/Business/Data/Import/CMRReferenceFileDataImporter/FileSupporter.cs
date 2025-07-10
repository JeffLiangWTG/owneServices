using System.IO;
using System.Text;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class FileSupporter
	{
		#region Unzip

		public void UnzipFiles(ZBlob fileToUnzip)
		{
			try
			{
				using (MemoryStream stream = new MemoryStream(fileToUnzip, false))
				using (GZipInputStream gZipStream = new GZipInputStream(stream))
				using (TarArchive archive = TarArchive.CreateInputTarArchive(gZipStream, Encoding.UTF8))
				{
					archive.ExtractContents(UnzipFilePath);
				}
			}
			catch (SharpZipBaseException e)
			{
				throw new InvalidCompressedFileException(ZString.Format("Unable to decompress file[{0}].", UnzipFilePath), e);
			}
		}

		public void DeleteTempDirectory()
		{
			if (Directory.Exists(UnzipFilePath))
			{
				DirectoryInfo directoryInfo = Directory.GetParent(UnzipFilePath);
				TempDirectory.DeleteDirectory(directoryInfo);
				UnzipFilePath = null;
			}
		}

		public string UnzipFilePath
		{
			get
			{
				if (fUnzipFilePath == null)
				{
					fUnzipFilePath = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString() + @"\CMRReferenceFiles");
				}
				return fUnzipFilePath;
			}
			private set
			{
				fUnzipFilePath = value;
			}
		}

		string fUnzipFilePath;

		#endregion

		#region Files

		public string[] GetFilesInDirectory(string fileExtention, string directoryPath)
		{
			string[] allFiles = Directory.GetFiles(directoryPath);
			string[] result = new string[allFiles.Length];
			int resultIndex = 0;

			for (int i = 0; i < allFiles.Length; i++)
			{
				if (allFiles[i].EndsWith(fileExtention) && IsFileToParse(allFiles[i]))
				{
					result[resultIndex] = allFiles[i];
					resultIndex++;
				}
			}

			return result;
		}

		bool IsFileToParse(string filePath)
		{
			return !filePath.EndsWith("reference_files.htm")
				&& !filePath.EndsWith("quantity_unit_conversion.htm");
		}

		#endregion
	}
}
