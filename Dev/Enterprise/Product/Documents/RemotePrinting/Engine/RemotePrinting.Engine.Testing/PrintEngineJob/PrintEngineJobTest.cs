using System;
using System.Drawing;
using CargoWise.IO;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	sealed class PrintEngineJobTest : PrintEngineTestCase
	{
		public void TestConstructor()
		{
			var data = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0x0 };
			var jobPK = Guid.NewGuid();
			var serialisableJob = GetNewPrintJob(jobPK, data);
			var queue = GetNewPrintQueue();
			var watermark = new TextWatermark("hello", WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Middle, 0, 0, 45, Color.Gray, "Arial", 120, FontStyle.Bold);

			var job1 = new PrintEngineJob(serialisableJob, queue, watermark, 2m);
			TestConstructor_AssertJobProperties(job1, jobPK, "NeedNameToSaveToFile", data, watermark);

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var documentXlsPath = resourceRetriever.SaveResourceToFile("Enterprise.RemotePrinting.Engine.Testing.TestFiles.Document.xls", "Document.xls");
				serialisableJob.EmailSubjectLine = documentXlsPath;

				var job2 = new PrintEngineJob(serialisableJob, queue, watermark, 2m);
				TestConstructor_AssertJobProperties(job2, jobPK, documentXlsPath, data, watermark);
			}
		}

		void TestConstructor_AssertJobProperties(PrintEngineJob job, Guid jobPK, string documentName, byte[] data, Watermark watermark)
		{
			AssertEquals(nameof(job.BlobType), "XLS", job.BlobType);
			AssertEquals(nameof(job.PrinterName), "TestPrinter", job.PrinterName);
			AssertEquals(nameof(job.JobPk), jobPK, job.JobPk);
			AssertEquals(nameof(job.Copies), 3, job.Copies);
			AssertArrayEqualsByElements(nameof(job.Contents), data, job.Contents);
			AssertEquals(nameof(job.DocumentName), documentName, job.DocumentName);
			AssertEquals(nameof(job.PrinterDriverTemplate), PrinterDriverTemplate, job.PrinterDriverTemplate);
			AssertEquals(nameof(job.DeleteCompanyLogo), true, job.DeleteCompanyLogo);
			AssertEquals(nameof(job.EscapeSequence), EscapeSequence, job.EscapeSequence);
			AssertEquals(nameof(job.Watermark), watermark, job.Watermark);
			AssertEquals(nameof(job.LineSpacing), 2m, job.LineSpacing);
			AssertEquals(nameof(job.LeftMargin), 2, job.LeftMargin);
			AssertEquals(nameof(job.TopMargin), 3, job.TopMargin);
			AssertEquals(nameof(job.Scale), 4m, job.Scale);
			AssertEquals(nameof(job.VerticalScale), 5m, job.VerticalScale);
			AssertEquals(nameof(job.HorizontalScale), 6m, job.HorizontalScale);
			Assert(job.IsRollPaper);
		}

		SerialisablePrintQueue GetNewPrintQueue()
		{
			var queue = new SerialisablePrintQueue();
			queue.XlsTemplate = PrinterDriverTemplate;
			queue.LeftMargin = 2;
			queue.TopMargin = 3;
			queue.SuppressLetterhead = true;
			queue.Name = "TestPrinter";
			queue.Scale = 4;
			queue.RowScale = 5;
			queue.ColumnScale = 6;
			queue.IsRollPaper = true;

			return queue;
		}

		SerialisablePrintJob GetNewPrintJob(Guid jobPK, byte[] data)
		{
			var job = new SerialisablePrintJob();
			job.JobPk = jobPK;
			job.EmailSubjectLine = "NeedNameToSaveToFile";
			job.BlobType = "XLS";
			job.Contents = data;
			job.Copies = 3;
			job.EscapeSequence = EscapeSequence;

			return job;
		}

		byte[] PrinterDriverTemplate => new byte[] { 1, 2, 3, 4, 5 };
		byte[] EscapeSequence => new byte[] { 1, 2, 3, 4, 5 };
	}
}
