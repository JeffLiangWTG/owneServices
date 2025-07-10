using System;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// Base class for writing an index file for the contents of a CD.
	/// </summary>
	public abstract class IndexFile : IDisposable
	{
		public IndexFile(string filenameToWrite)
		{
			fOutputFile = filenameToWrite;
		}

		public abstract void Start();
		public abstract void Finish();
		public abstract void AddFile(StorageDocsBase document, string basePath, string relativePath);
		public abstract void AddDirectory(string directoryName);
		public abstract void CloseDirectory();

		#region Output File

		public string OutputFile
		{
			get { return fOutputFile; }
		}

		readonly string fOutputFile;

		#endregion

		#region IDisposable Members

		public virtual void Dispose()
		{
		}

		#endregion
	}
}
