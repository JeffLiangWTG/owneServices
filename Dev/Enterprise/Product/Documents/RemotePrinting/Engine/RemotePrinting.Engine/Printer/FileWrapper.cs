using System;
using System.IO;
using CargoWise.Common;

namespace Enterprise.RemotePrinting.Engine
{
	public sealed class FileWrapper : IDisposable
	{
		public FileWrapper(string fullPathAndFilename)
		{
			FullPathAndFilename = Argument.NotNullOrEmpty(fullPathAndFilename, nameof(fullPathAndFilename));
		}

		public string FullPathAndFilename { get; }

		public void Dispose()
		{
			if (!disposed)
			{
				DeleteFileToPrint();
				disposed = true;
			}
		}

		void DeleteFileToPrint()
		{
			if (File.Exists(FullPathAndFilename))
			{
				try
				{
					File.Delete(FullPathAndFilename);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					// could not delete temp file, this is ok.
				}
			}
		}

		bool disposed;
	}
}
