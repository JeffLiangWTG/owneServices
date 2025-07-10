using System.Collections.Generic;
using System.Globalization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.EConversation.Testing;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	public abstract class BusinessObjectEmailProcessorTestCase<P, T> : TestCaseWithFactory
		where P : BusinessObjectEmailProcessor<T>
		where T : BusinessObject, IAllowAttachEmailsToEDocs
	{
		public void TestProcessWithNoLoggingInfo()
		{
			P processor = GetNewBusinessObjectEmailProcessor();

			Email email = new EmailBuilderForTesting()
				.Subject("Hello fix this")
				.To("Someone", "test@edi.com.au")
				.Body("please fix this incident now")
				.GetEmail();
			Assert("Should be processed successfully regardless of LoggingInfo", processor.CreateAndProcessMailItem(email, LoggerForTest));
		}

		public void TestAbstractProperties()
		{
			P processor = GetNewBusinessObjectEmailProcessor();
			AssertEquals(ExpectedEmailTypeName, processor.EmailTypeName);
			AssertEquals(ExpectedMailApplicationCode, processor.MailApplicationCode);
		}

		public void TestProcessWithInvalidEmailFormat()
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.AddRecipientForUserCommunication("123@123.com", MailRecipient.RecipientTypes.TO);
			AssertEquals("Address should not be overwritten by registry", "123@123.com", mailItem.MailRecipients[0].EmailAddress);

			var processor = GetNewBusinessObjectEmailProcessor();
			var email = new EmailBuilderForTesting()
				.Subject("Hello fix this")
				.To("Someone", "test@edi.com.au")
				.Cc("Long Name", new string('O', MailDBRecipientsSchema.MR_RecipientMailAddress.MaxLength) + "@TEST.LOCAL")
				.Body("please fix this incident now")
				.GetEmail();
			Assert(!processor.CreateAndProcessMailItem(email, LoggerForTest));
			Assert(email.IsEmailProcessingSkipped);
			AssertContainsLogEntry("Should log invalid email address", "The format of email is invalid.", false);
		}

		#region TestCreateAndProcessBusinessObjectEmail

		public void TestCreateAndProcessBusinessObjectEmail_FoundExistingBusinessObjectAndAttach()
		{
			string identifier = NumberFountainForBusinessObject.PeekPreliminaryFormatted(Factory);
			TestCreateAndProcessBusinessObjectEmail(
				true,
				identifier,
				string.Format(CultureInfo.CurrentCulture, "{0} : Email Enquiry", identifier));
		}

		public void TestCreateAndProcessBusinessObjectEmail_SubjectDoesNotContainIdentifier()
		{
			string identifier = NumberFountainForBusinessObject.PeekPreliminaryFormatted(Factory);
			TestCreateAndProcessBusinessObjectEmail(false, identifier, "09823oi)(*#IUJDSFO");
		}

		public void TestCreateAndProcessBusinessObjectEmail_ExistingBusinessObjectNotFound()
		{
			try
			{
				Db.Connection.BeginTransaction();
				string identifier = NumberFountainForBusinessObject.GetNextFormatted(Factory);
				string identifier2 = NumberFountainForBusinessObject.GetNextFormatted(Factory);
				TestCreateAndProcessBusinessObjectEmail(
					false,
					identifier,
					string.Format(CultureInfo.CurrentCulture, "{0}: Email Enquiry", identifier2));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		void TestCreateAndProcessBusinessObjectEmail(bool shouldAttachToExistingBusinessObject, string identifier, string emailSubject)
		{
			T bizO = Factory.NewWithValidTestData<T>();
			SetReferenceNumber(bizO, identifier);
			Factory.Save();

			Email email = new EmailBuilderForTesting()
				.From("From User", "fromuser@cargowise.com")
				.To("To User", "touser@cargowise.com")
				.Cc("Cc User", "ccuser@cargowise.com")
				.Body("MEH MEH")
				.Subject(emailSubject)
				.GetEmail();

			ZQuery query = new ZQuery(MailDBItemsSchema.MI_Application, ExpectedMailApplicationCode);
			AssertEquals("Precondition", 0, Factory.Load<MailItem>(query).Length);
			AssertEquals(0, bizO.DocManagerInfo.AllEDocs.Count);

			P processor = GetNewBusinessObjectEmailProcessor();
			Assert("Should be processed successfully", processor.CreateAndProcessMailItem(email, LoggerForTest));
			MailItem[] mailItems = Factory.Load<MailItem>(query);
			AssertEquals(1, mailItems.Length);
			Assert(mailItems[0].IsInDatabase && !mailItems[0].HasChanges);
			AssertMailItemStatus(mailItems[0], shouldAttachToExistingBusinessObject);
			AssertEquals("From User <fromuser@cargowise.com>", mailItems[0].MI_From);
			AssertEquals("MEH MEH\r\n", mailItems[0].MI_Body);
			AssertEquals(email.Subject, mailItems[0].MI_Subject);
			AssertEquals(2, mailItems[0].MailRecipients.Count);

			IMailRecipient recTO, recCC;
			if (mailItems[0].MailRecipients[0].MR_RecipientType == nameof(MailRecipient.RecipientTypes.TO))
			{
				recTO = mailItems[0].MailRecipients[0];
				recCC = mailItems[0].MailRecipients[1];
			}
			else
			{
				recTO = mailItems[0].MailRecipients[1];
				recCC = mailItems[0].MailRecipients[0];
			}
			AssertEquals("\"To User\" <touser@cargowise.com>", recTO.EmailAddress);
			AssertEquals("\"Cc User\" <ccuser@cargowise.com>", recCC.EmailAddress);

			// Load in new Factory
			bizO = new BusinessObjectFactory().Load<T>(bizO.PK);
			if (shouldAttachToExistingBusinessObject)
			{
				AssertEquals(1, bizO.DocManagerInfo.AllEDocs.Count);
				var eDoc = bizO.DocManagerInfo.AllEDocs[0];
				AssertEquals("COR", eDoc.DocType);
				AssertEquals(BusinessObjectEmailAttacher.GetEmailTextForOutlookExpressMessage(mailItems[0]), eDoc.GetImageDataReader().ConvertToAsciiStringAndCloseStream());
				AssertEmailAttachedLogEntry(identifier);
			}
			else
			{
				AssertEquals(0, bizO.DocManagerInfo.AllEDocs.Count);
				AssertEmptyLogEntries();
			}
		}

		protected virtual void AssertMailItemStatus(MailItem mailItem, bool isMailAttached)
		{
			AssertEquals((isMailAttached) ? MailStatus.Processed : MailStatus.Unprocessed, mailItem.MI_Status);
		}

		protected abstract INumberFountainProxy NumberFountainForBusinessObject { get; }

		#endregion

		protected MailItem GetLastCreatedMailItem()
		{
			ZQuery query = new ZQuery(MailDBItemsSchema.MI_Application, ExpectedMailApplicationCode);
			query.OrderBy = MailDBItemsSchema.Constants.MI_ReceivedDateTime + " DESC";
			return Factory.LoadTop1<MailItem>(query);
		}

		protected abstract void SetReferenceNumber(T bizO, string refNumber);
		protected abstract string ExpectedEmailTypeName { get; }
		protected abstract string ExpectedMailApplicationCode { get; }
		protected abstract P GetNewBusinessObjectEmailProcessor();

		#region LoggerForTest

		protected void AssertEmptyLogEntries()
		{
			AssertEquals(0, ((EmailProcessorLoggerForTest)LoggerForTest).LogEntries.Count);
		}

		protected void AssertContainsLogEntry(string message, string logEntry, bool shouldMatchWholeSentence)
		{
			if (shouldMatchWholeSentence)
			{
				AssertCollectionContains(message, logEntry, ((EmailProcessorLoggerForTest)LoggerForTest).LogEntries);
			}
			else
			{
				Assert(message, ((EmailProcessorLoggerForTest)LoggerForTest).LogEntries.Exists(e => e.Contains(logEntry)));
			}
		}

		protected void AssertNotContainsLogEntry(string message, string logEntry, bool shouldMatchWholeSentence)
		{
			if (shouldMatchWholeSentence)
			{
				AssertCollectionNotContains(message, logEntry, ((EmailProcessorLoggerForTest)LoggerForTest).LogEntries);
			}
			else
			{
				Assert(message, !((EmailProcessorLoggerForTest)LoggerForTest).LogEntries.Exists(e => e.Contains(logEntry)));
			}
		}

		protected void AssertEmailAttachedLogEntry(string identifier)
		{
			string message = string.Format(CultureInfo.CurrentCulture, "The email should be attached to {0}", identifier);
			AssertContainsLogEntry(message, string.Format(CultureInfo.CurrentCulture, "Email attached to: {0}", identifier), true);
		}

		protected void AssertNoEmailAttachedLogEntry()
		{
			AssertNotContainsLogEntry("Should not attach to business object", "Email attached to:", false);
		}

		protected IEmailProcessorLogger LoggerForTest
		{
			get { return loggerForTest ?? (loggerForTest = new EmailProcessorLoggerForTest()); }
		}
		IEmailProcessorLogger loggerForTest;

		protected class EmailProcessorLoggerForTest : IEmailProcessorLogger
		{
			public void Log(LogType logType, bool verboseModeOnly, string format, params object[] args)
			{
				if (!verboseModeOnly || EDIDataRegistry.Instance.EnableVerboseModeOnEmailProcessors.Value)
				{
					LogEntries.Add(string.Format(format, args));
				}
			}

			public List<string> LogEntries
			{
				get { return logEntries ?? (logEntries = new List<string>()); }
			}

			List<string> logEntries;
		}

		#endregion
	}
}
