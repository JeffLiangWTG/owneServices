using System;

namespace Enterprise.DocumentScanning.Business
{
	public class FilePermanentlyDeletedEventArgs : EventArgs
	{
		public FilePermanentlyDeletedEventArgs(int completedCount, int totalCount)
		{
			Completed = completedCount;
			Total = totalCount;
		}

		public int Completed { get; private set; }

		public int Total { get; private set; }
	}
}
