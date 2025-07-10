using System;
using System.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class EmailImportBatchProcessorTest : EmailReaderBatchProcess_Test
	{
		public void TestIsEnvironmentValid()
		{
			AssertEquals("Precondition", 0, SystemDataRegistry.Instance.ConsolsDataImportDirectory.Value.Trim().Length);

			var processor = new DummyProcessor(SystemDataRegistry.Instance.ConsolsDataImportDirectory, Buffer);
			AssertEquals(false, processor.IsEnvironmentDataValidInternal());
			AssertEquals("Should have warnings", true, Buffer.HasWarnings);
			AssertEquals("Warning contains", true, Buffer.AsString.IndexOf("No " + SystemDataRegistry.Instance.ConsolsDataImportDirectory.Name + " registry item set. Associated import not running.") > -1);

			Buffer.Clear();
			string nonExistentDirectory = Env.TempPath + "NonExistentDirectory";
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, nonExistentDirectory);
			processor = new DummyProcessor(SystemDataRegistry.Instance.ConsolsDataImportDirectory, Buffer);
			AssertEquals(false, processor.IsEnvironmentDataValidInternal());
			AssertEquals("Should have warnings", true, Buffer.HasWarnings);
			AssertEquals("Warning contains", true, Buffer.AsString.IndexOf(@"The directory " + nonExistentDirectory + " does not exist") > -1);

			Buffer.Clear();
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.TempPath);
			processor = new DummyProcessor(SystemDataRegistry.Instance.ConsolsDataImportDirectory, Buffer);
			AssertEquals(true, processor.IsEnvironmentDataValidInternal());
			AssertEquals("Should not have warnings", true, !Buffer.HasWarnings);
		}

		protected override void AssertEmailIncludedInFilter(MailItem mail)
		{
			string pathToFile = Path.Combine(Env.TempPath, "TempFile.xml");
			try
			{
				AssertEquals("File has been downloaded", true, File.Exists(pathToFile));
			}
			finally
			{
				DeleteIfExists(pathToFile);
			}
		}

		protected override void CreateMailItemsForEMailFilter()
		{
			var mail = Factory.New<MailItem>();
			mail.MI_Subject = "Subject";
			mail.MI_Status = MailStatus.Queued;
			mail.MI_Direction = MailDirection.Receive;
			mail.MI_From = "test@edi.com.au";
			mail.MI_ReceivedDateTime = ZDateTime.Now;
			mail.MI_SystemCreateTimeUtc = ZDateTime.Now;
			mail.MI_LastAttemptDateTime = ZDateTime.Now;
			mail.MI_SendDateTime = ZDateTime.Now;

			var attachment = mail.MailAttachments.AddNew();
			attachment.MA_Data = ZBlob.FromAscii("AttachmentData");
			attachment.MA_FileName = "TempFile.xml";
			//(using a test filter so can't be generic)
			if (QueryMailFilter.AllQueuedItems_ForTesting.CanProcess(mail))
			{
				mail.MI_Application = QueryMailFilter.AllQueuedItems_ForTesting.Code;
			}

			Factory.Save();
		}

		protected override EmailReaderBatchProcess GetEmailReaderProcessor()
		{
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.TempPath);
			return new DummyProcessor(SystemDataRegistry.Instance.ConsolsDataImportDirectory, Buffer);
		}

		protected override int NumberOfEmailsExpectedToBeProcessed
		{
			get { return 1; }
		}

		NotificationBuffer buffer;
		protected override NotificationBuffer Buffer
		{
			get
			{
				return buffer ?? (buffer = new NotificationBuffer(new NotificationBuffer()));
			}
		}
	}
}
