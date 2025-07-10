using System;
using System.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	sealed class MultipleInstancesEmailImportBatchProcessTest : MultipleInstancesTest
	{
		protected override void AssertEmailIncludedInFilter(MailItem mail)
		{
			string pathToFile = Path.Combine(Env.TempPath, string.Format(Culture.Invariant, "TempFile {0}.xml", mail.MI_Subject));
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
			for (int i = 0; i < NumberOfEmailsExpectedToBeProcessed; ++i)
			{
				var mail = Factory.New<MailItem>();
				var id = i.ToString();

				mail.MI_Subject = id;
				mail.MI_Status = MailStatus.Queued;
				mail.MI_Direction = MailDirection.Receive;
				mail.MI_From = string.Format(Culture.Invariant, "test{0}@edi.com.au", id);
				mail.MI_ReceivedDateTime = ZDateTime.Now;
				mail.MI_SystemCreateTimeUtc = ZDateTime.Now;
				mail.MI_LastAttemptDateTime = ZDateTime.Now;
				mail.MI_SendDateTime = ZDateTime.Now;
				mail.MI_Application = "ALL"; //debug only filter with no logic to check
				var attachment = mail.MailAttachments.AddNew();
				attachment.MA_Data = ZBlob.FromAscii(string.Format(Culture.Invariant, "AttachmentData {0}", i));
				attachment.MA_FileName = string.Format(Culture.Invariant, "TempFile {0}.xml", i);
			}
			Factory.Save();
		}

		protected override EmailReaderBatchProcess GetEmailReaderProcessor()
		{
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.TempPath);
			return new DummyProcessor(SystemDataRegistry.Instance.ConsolsDataImportDirectory, new NotificationBuffer());
		}

		protected override int NumberOfEmailsExpectedToBeProcessed
		{
			get { return 500; }
		}
	}
}
