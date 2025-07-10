using System;

namespace Enterprise.DocumentScanning.Business
{
	public delegate void FileImportedEventHandler(object sender, FileImportedEventArgs args);

	public class FileImportedEventArgs : EventArgs
	{
		public FileImportedEventArgs(int completedCount, int totalCount)
		{
			Completed = completedCount;
			Total = totalCount;
		}

		public int Completed { get; private set; }

		public int Total { get; private set; }
	}
}
