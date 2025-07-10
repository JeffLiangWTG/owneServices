using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ClientSharedComponents.ServiceTasks.Testing
{
	public abstract class MailDataImportServiceTaskTestCase<T> : ServiceTaskTestCase<T> where T : MailDataImportServiceTask
	{
		public void TestRunTask()
		{
			_ = InitialiseTaskSchedule(ServiceTask);
			RunTaskSchedule(ServiceTask);
			AssertEquals("Empty log", ZString.Empty, ServiceTask.Notify.AsString);

			var mailItem = GetValidMailItem();
			AddFileAsAttachmentToMailItem(ValidTestFile, mailItem);
			AddFileAsAttachmentToMailItem(ValidTestFile, mailItem);
			Factory.Save();
			RunTaskSchedule(ServiceTask);
			mailItem.Reload();
			Assert("All Environment setups need to be in place", ServiceTask.Importer.CheckEnvironmentValid(Factory, ServiceTask.Notify));
			Assert("has no errors", ServiceTask.Notify.AsString.Contains("1 email(s) found for import. Processing email(s)..."));
			Assert("has no errors", ServiceTask.Notify.AsString.Contains(string.Format("1. Email received on {0} contains 2 attachment(s). Processing attachment(s)...", TestDate.ToLongTimeString())));

			var mailItems = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.PK, SQLComparisonOperator.NotEqual, mailItem.PK));
			AssertEquals("there should be only 1 - the email send out to notify the group of the import", 1, mailItems.Length);
			Assert("Successful import", mailItems[0].MI_Subject.Contains("SUCCESS"));
			Assert("Notification body", mailItems[0].MI_Body.Contains("The following attachment(s) from test@test.com have been imported successfully."));
			Assert("files should be listed", mailItems[0].MI_Body.Contains(Path.GetFileName(ValidTestFile)));
			AssertEquals("mail is processed", MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestRunTaskCausesErrors()
		{
			_ = InitialiseTaskSchedule(ServiceTask);
			RunTaskSchedule(ServiceTask);
			AssertEquals("has no errors", ZString.Empty, ServiceTask.Notify.AsString);

			var mailItem = GetValidMailItem();
			AddFileAsAttachmentToMailItem(ValidTestFile, mailItem);
			AddFileAsAttachmentToMailItem(ValidTestFile, mailItem);
			Factory.Save();
			ServiceTask.Notify.Notify(new ErrorNotification(ErrorType.Error, "this is to force error notification"));
			RunTaskSchedule(ServiceTask);
			mailItem.Reload();
			Assert("All Environment setups need to be in place", ServiceTask.Importer.CheckEnvironmentValid(Factory, ServiceTask.Notify));

			var mailItems = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.PK, SQLComparisonOperator.NotEqual, mailItem.PK));
			AssertEquals("there should be only 1 - the email send out to notify the group of the import", 1, mailItems.Length);
			Assert("Successful import", mailItems[0].MI_Subject.Contains("ERROR"));
			Assert("Notification body", mailItems[0].MI_Body.Contains("Error(s) have occurred while processing email from test@test.com."));
			AssertEquals("files should be listed", 2, mailItems[0].MailAttachments.Count);
			AssertEquals("mail is processed", MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestRunTaskCausesNoAttachmentError()
		{
			_ = InitialiseTaskSchedule(ServiceTask);
			RunTaskSchedule(ServiceTask);
			AssertEquals("has no errors", ZString.Empty, ServiceTask.Notify.AsString);

			Assert("Precondition: Email table should be empty", Env.OutgoingMailManager.EmailsCreated.Count == 0);

			var mailItem = GetValidMailItem();
			Factory.Save();
			RunTaskSchedule(ServiceTask);
			mailItem.Reload();

			AssertEquals("there should be only 1 - the email send out to notify the group of the import", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert("Successful import", Env.OutgoingMailManager.EmailsCreated[0].Subject.Contains("ERROR"));
			Assert("Notification body", Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("Error(s) have occurred while processing email from test@test.com."));
			Assert("Notification details", Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("does not have an attachment"));
			AssertEquals("files should be listed", 0, Env.OutgoingMailManager.EmailsCreated[0].Attachments.Count);
			AssertEquals("mail is processed", MailStatus.Processed, mailItem.MI_Status);
		}

		#region Implementation

		protected abstract void SetupValidEnvironment();
		protected abstract T ServiceTask
		{
			get;
		}
		protected abstract ZString SubjectIdentifier
		{
			get;
		}
		protected abstract ZString ValidTestFile
		{
			get;
		}

		ZDateTime TestDate;

		protected override void SetUpCore()
		{
			base.SetUpCore();
			TestDate = ZDateTime.Now;
			SetupValidEnvironment();
		}

		protected MailItem GetValidMailItem()
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.MI_Subject = SubjectIdentifier;
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_ReceivedDateTime = TestDate;
			mailItem.MI_SystemCreateTimeUtc = TestDate;
			mailItem.MI_LastAttemptDateTime = TestDate;
			mailItem.MI_SendDateTime = TestDate;
			mailItem.MI_From = "test@test.com";
			MailFilterLocatorTestHelper.SetApplication(mailItem, ServiceTask.MailFilter.Code);
			return mailItem;
		}

		protected void AddFileAsAttachmentToMailItem(string filename, MailItem mailItem)
		{
			using (StreamReader reader = new(filename))
			{
				var mailAttachment = mailItem.MailAttachments.AddNew();
				mailAttachment.MA_Data = ZBlob.FromAscii(reader.ReadToEnd());
				mailAttachment.MA_FileName = Path.GetFileName(filename);
			}
		}

		#endregion
	}
}
