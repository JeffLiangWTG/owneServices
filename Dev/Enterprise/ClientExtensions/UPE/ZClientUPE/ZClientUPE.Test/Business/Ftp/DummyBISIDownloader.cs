using System;
using System.Collections.Generic;
using System.IO;
using Enterprise.Client.UPE.Testing;
using Enterprise.Environment;
using Renci.SshNet.Common;

namespace Enterprise.Client.UPE.Business.Ftp.Testing
{
	internal class DummyBISIDownloader : IBISIDownloader
	{
		public void PrepareZipFilesForTest()
		{
			string zipFile1 = UPETestHelper.TestFiles.BISI.Import.Folder + "CODZipFile1.zip";
			string zipFile2 = UPETestHelper.TestFiles.BISI.Import.Folder + "CODZipFile2.zip";
			string dir1 = Path.Combine(SourcePath, "Dir1");
			Directory.CreateDirectory(dir1);
			File.Copy(zipFile1, SourceFile1Path, true);
			File.Copy(zipFile2, SourceFile2Path, true);
			Directory.CreateDirectory(TargetPath);
			UPEDataRegistry.Instance.BISIDownloadArchiveDirectory = ArchiveDir;
			Directory.CreateDirectory(ArchiveDir);
		}

		public bool IsExpectingDownloadFailure;
		public bool IsExpectingEmptyZipDirectory;
		#region IBISIDownloader Members
		public string ArchiveDir
		{
			get
			{
				return Path.Combine(TempPath, "ARCHIVE");
			}
		}

		public List<string> ListZipFiles()
		{
			return (IsExpectingEmptyZipDirectory) ? new List<string>() : new List<string>()
			{ SourceFile1Path, SourceFile2Path };
		}

		public string DownloadedFileName
		{
			get
			{
				return fDownloadedFileName;
			}
		}

		public string TempPath
		{
			get
			{
				return Path.Combine(Env.TempPath, "BISIDOWNLOADTEST");
			}
		}

		public string SourceFile1Path
		{
			get
			{
				return Path.Combine(SourcePath, @"Dir1\CODZipFile1.zip");
			}
		}

		public string SourceFile2Path
		{
			get
			{
				return Path.Combine(SourcePath, "CODZipFile2.zip");
			}
		}

		public bool DownloadFile(string remoteFileFullPathName)
		{
			bool result = false;
			if (!IsExpectingDownloadFailure)
			{
				fDownloadedFileName = Path.Combine(TargetPath, Path.GetFileName(remoteFileFullPathName));
				using (FileStream source = File.OpenRead(remoteFileFullPathName))
				using (FileStream target = new FileStream(DownloadedFileName, FileMode.OpenOrCreate, FileAccess.Write))
				{
					byte[] buffer = new byte[source.Length];
					if (source.Read(buffer, 0, (int)source.Length) < buffer.Length)
					{
						throw new Exception("I/O read error");
					}

					target.Write(buffer, 0, buffer.Length);
				}

				result = true;
			}

			return result;
		}

		public bool ThrowExceptionInDeleteRemoteFile;
		public void DeleteRemoteFile(List<string> filesToDelete)
		{
			if (ThrowExceptionInDeleteRemoteFile)
			{
				throw new SshException("Could not delete an FTP file");
			}

			foreach (string fileToDelete in filesToDelete)
			{
				File.SetAttributes(fileToDelete, ~FileAttributes.ReadOnly);
				File.Delete(fileToDelete);
			}
		}

		string TargetPath
		{
			get
			{
				return Path.Combine(TempPath, "TARGET");
			}
		}

		string SourcePath
		{
			get
			{
				return Path.Combine(TempPath, "SOURCE");
			}
		}

		string fDownloadedFileName;
		#endregion
	}
}
