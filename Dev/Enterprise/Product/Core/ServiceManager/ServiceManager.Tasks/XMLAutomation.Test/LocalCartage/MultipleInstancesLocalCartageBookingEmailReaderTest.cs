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
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	sealed class MultipleInstancesLocalCartageBookingEmailReaderTest : MultipleInstancesTest
	{
		protected override void AssertEmailIncludedInFilter(MailItem mail)
		{
			ZString[] parts = mail.MI_Subject.Split(new char[] { ' ' });
			string id = parts[parts.Length - 1];

			string pathToFile = Path.Combine(Env.TempPath, string.Format(Culture.Invariant, "TempFile {0}.xml", id));
			string pathToWrongFile = Path.Combine(Env.TempPath, string.Format(Culture.Invariant, "TempFile {0} X.xml", id));
			try
			{
				AssertEquals(string.Format(Culture.Invariant, "File should have been created: {0}", pathToFile), true, File.Exists(pathToFile));
				AssertEquals(string.Format(Culture.Invariant, "File should not have been created: {0}", pathToWrongFile), false, File.Exists(pathToWrongFile));
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
				var mail1 = Factory.New<MailItem>();
				mail1.MI_Subject = FreightConstants.LocalCartageXmlEmailSubject + " " + i;
				mail1.MI_Status = MailStatus.Queued;
				mail1.MI_Direction = MailDirection.Receive;
				mail1.MI_From = string.Format(Culture.Invariant, "test{0}@edi.com.au", i);
				mail1.MI_ReceivedDateTime = ZDateTime.Now;
				mail1.MI_SystemCreateTimeUtc = ZDateTime.Now;
				mail1.MI_LastAttemptDateTime = ZDateTime.Now;
				mail1.MI_SendDateTime = ZDateTime.Now;

				var attachment1 = mail1.MailAttachments.AddNew();
				attachment1.MA_Data = ZBlob.FromAscii(string.Format(Culture.Invariant, "AttachmentData", i));
				attachment1.MA_FileName = string.Format(Culture.Invariant, "TempFile {0}.xml", i);

				var mail2 = Factory.New<MailItem>();
				mail2.MI_Subject = string.Format(Culture.Invariant, "Subject {0}", i);
				mail2.MI_Status = MailStatus.Queued;
				mail2.MI_Direction = MailDirection.Receive;
				mail2.MI_From = string.Format(Culture.Invariant, "test{0}@edi.com.au", i);
				mail2.MI_ReceivedDateTime = ZDateTime.Now;
				mail2.MI_SystemCreateTimeUtc = ZDateTime.Now;
				mail2.MI_LastAttemptDateTime = ZDateTime.Now;
				mail2.MI_SendDateTime = ZDateTime.Now;

				var attachment2 = mail2.MailAttachments.AddNew();
				attachment2.MA_Data = ZBlob.FromAscii(string.Format(Culture.Invariant, "AttachmentData", i));
				attachment2.MA_FileName = string.Format(Culture.Invariant, "TempFile {0} X.xml", i);

				MailFilterLocatorTestHelper.SetApplication(mail1, MailFilterCodes.LocalCartageBooking);
				MailFilterLocatorTestHelper.SetApplication(mail2, MailFilterCodes.LocalCartageBooking, false);

				Factory.Save();
			}
		}

		protected override EmailReaderBatchProcess GetEmailReaderProcessor()
		{
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.TempPath);
			return new LocalCartageBookingEmailReader(SystemDataRegistry.Instance.ConsolsDataImportDirectory, new NotificationBuffer());
		}

		protected override int NumberOfEmailsExpectedToBeProcessed
		{
			get { return 500; }
		}
	}
}
