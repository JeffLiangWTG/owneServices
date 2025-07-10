using System;
using System.IO;
using Enterprise.RemotePrinting.Types;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	public class PrintEngineTestCase : TestCase
	{
		protected static FileWrapper CopyFileForTesting(string fileToCopy)
		{
			var enterpriseTempPath = TempForTest.TempPath;
			var tempFile = Path.Combine(enterpriseTempPath, Guid.NewGuid() + ".tif");
			File.Copy(fileToCopy, tempFile, true);

			var info = new FileInfo(tempFile);
			info.Attributes = FileAttributes.Normal; // so we can delete it later
			return new FileWrapper(tempFile);
		}

		protected static SerialisablePrintJob GetSerialisablePrintJob(string filename)
		{
			string extension = Path.GetExtension(filename).Substring(1);

			SerialisablePrintJob serialisableJob = new SerialisablePrintJob();
			serialisableJob.JobPk = Guid.NewGuid();
			serialisableJob.BlobType = extension;
			serialisableJob.Contents = ByteRetriever.GetFileAsBytes(filename);
			serialisableJob.EmailSubjectLine = Path.GetFileNameWithoutExtension(filename);
			return serialisableJob;
		}

		protected static SerialisablePrintQueue GetSerialisablePrintQueue() => new SerialisablePrintQueue { Name = NullPrintQueueNameForTesting };

		public static PrintEngineJob GetPrintEngineJob(string filename, decimal scale = 100m)
		{
			return GetPrintEngineJob(GetSerialisablePrintJob(filename), scale);
		}

		protected static PrintEngineJob GetPrintEngineJob(SerialisablePrintJob printJob, decimal scale = 100m)
		{
			var queue = GetSerialisablePrintQueue();
			queue.Scale = scale;
			return new PrintEngineJob(printJob, queue, null);
		}

		protected static readonly string NullPrintQueueNameForTesting = "TestNullQueue";
	}
}
