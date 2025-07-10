using System;
using System.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	sealed class LocalCartageBookingEmailReaderTest : EmailImportBatchProcessorTest
	{
		protected override void AssertEmailIncludedInFilter(MailItem mail)
		{
			string pathToFile = Path.Combine(Env.TempPath, "TempFile.xml");
			try
			{
				AssertEquals("File should have been created", true, File.Exists(Path.Combine(Env.TempPath, "TempFile.xml")));
				AssertEquals("File should not have been created", false, File.Exists(Path.Combine(Env.TempPath, "TempFile1.xml")));
			}
			finally
			{
				DeleteIfExists(pathToFile);
			}
		}

		protected override void CreateMailItemsForEMailFilter()
		{
			var mail1 = Factory.New<MailItem>();
			mail1.MI_Subject = FreightConstants.LocalCartageXmlEmailSubject;
			mail1.MI_Status = MailStatus.Queued;
			mail1.MI_Direction = MailDirection.Receive;
			mail1.MI_From = "test@edi.com.au";
			mail1.MI_ReceivedDateTime = ZDateTime.Now;
			mail1.MI_SystemCreateTimeUtc = ZDateTime.Now;
			mail1.MI_LastAttemptDateTime = ZDateTime.Now;
			mail1.MI_SendDateTime = ZDateTime.Now;

			var attachment1 = mail1.MailAttachments.AddNew();
			attachment1.MA_Data = ZBlob.FromAscii("AttachmentData");
			attachment1.MA_FileName = "TempFile.xml";

			var mail2 = Factory.New<MailItem>();
			mail2.MI_Subject = "Subject";
			mail2.MI_Status = MailStatus.Queued;
			mail2.MI_Direction = MailDirection.Receive;
			mail2.MI_From = "test@edi.com.au";
			mail2.MI_ReceivedDateTime = ZDateTime.Now;
			mail2.MI_SystemCreateTimeUtc = ZDateTime.Now;
			mail2.MI_LastAttemptDateTime = ZDateTime.Now;
			mail2.MI_SendDateTime = ZDateTime.Now;

			var attachment2 = mail2.MailAttachments.AddNew();
			attachment2.MA_Data = ZBlob.FromAscii("AttachmentData");
			attachment2.MA_FileName = "TempFile1.xml";

			MailFilterLocatorTestHelper.SetApplication(mail1, MailFilterCodes.LocalCartageBooking);
			MailFilterLocatorTestHelper.SetApplication(mail2, MailFilterCodes.LocalCartageBooking, false);

			Factory.Save();
		}

		protected override EmailReaderBatchProcess GetEmailReaderProcessor()
		{
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.TempPath);
			return new LocalCartageBookingEmailReader(SystemDataRegistry.Instance.ConsolsDataImportDirectory, Buffer);
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
