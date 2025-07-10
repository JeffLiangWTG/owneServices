using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.IO;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.Utilities
{
	public class ZipEDocsCreator : IDisposable
	{
		public ZipEDocsCreator()
		{
			tempDirectory = new TempDirectory();
			files = new List<ZipStream>();
			disposed = false;
			generator = new UniqueFilenameGenerator();
		}

		readonly TempDirectory tempDirectory;
		readonly List<ZipStream> files;
		bool disposed;
		readonly UniqueFilenameGenerator generator;

		public void AddEDoc(byte[] eDocData, string fileName)
		{
			if (eDocData != null)
			{
				AddEDoc((fileStream) => fileStream.Write(eDocData, 0, eDocData.Length), fileName);
			}
		}

		public void AddEDoc(Stream eDocDataStream, string fileName)
		{
			AddEDoc((fileStream) => eDocDataStream.CopyTo(fileStream), fileName);
		}

		void AddEDoc(Action<FileStream> populateData, string fileName)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(nameof(ZipEDocsCreator));
			}

			var safeFileName = GetSafeFileName(fileName);
			var fileStream = File.Create(Path.Combine(tempDirectory.DirectoryName, safeFileName), 0x1000, FileOptions.DeleteOnClose);

			populateData(fileStream);
			fileStream.Position = 0;

			files.Add(new ZipStream(safeFileName, fileStream));
		}

		public bool WriteTo(Stream outputStream)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(nameof(ZipEDocsCreator));
			}

			if (files.Count == 0)
			{
				return false;
			}

			var creator = new ZipCreator();
			creator.ZipStream(files, outputStream);

			return true;
		}

		public void Dispose()
		{
			if (disposed)
			{
				return;
			}

			disposed = true;

			foreach (var file in files)
			{
				file.Stream.Dispose();
			}

			tempDirectory.Dispose();
		}

		string GetSafeFileName(string fileName)
		{
			var safeFileName = MakeFilenameSafe.MakeSafeAndFixFileExtension(fileName, '_');

			return generator.GetNewUniqueFilePath(tempDirectory.DirectoryName, safeFileName);
		}
	}
}
