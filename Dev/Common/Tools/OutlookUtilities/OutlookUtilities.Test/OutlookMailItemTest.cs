using System;
using System.IO;
using System.Threading;
using CargoWise.Data;
using Enterprise.Environment;
using NUnit.Framework;
namespace Enterprise.Interop.OutlookIntegration.Testing
{
	[GuiTest]
	sealed class OutlookMailItemTest : TestCase
	{
		public void TestSubject()
		{
			MockOutlookMailItem mailItem = new MockOutlookMailItem(null);
			mailItem.Subject = "Subject";
			AssertEquals("Subject", mailItem.Subject);

			mailItem.SimulateMailItemSendEvent();

			// the subject can't be accessed after send in Outlook, it must be cached
			string subjectAfterSent = mailItem.Subject;
			AssertEquals("Subject", subjectAfterSent);
		}

		public void TestBody()
		{
			MockOutlookMailItem mailItem = new MockOutlookMailItem(null);
			mailItem.Body = "Body";
			AssertEquals("Body", mailItem.Body);
		}

		[ExpectException(typeof(OutlookDialogBoxOpenException))]
		public void TestOutlookDialogBoxOpenException_ThrownOnDisplay()
		{
			MockOutlookMailItem mailItem = new MockOutlookMailItem(null);
			mailItem.Inner.ThrowDialogBoxOpenExceptionOnDisplay = true;
			mailItem.Display(false);
		}

		[ExpectNoExceptions]
		public void TestAddRecipient()
		{
			MockOutlookMailItem mailItem = new MockOutlookMailItem(new MockNativeMailItem());
			mailItem.AddRecipient("poopoo@edi.com.au");

			mailItem.Inner.IsSecurityDialogEnabled = false;
			AssertEquals("Recipient should be successfully added", true, mailItem.Inner.Recipients.RecipientAdded);
		}

		[ExpectNoExceptions]
		public void TestAddReplyRecipient()
		{
			MockOutlookMailItem mailItem = new MockOutlookMailItem(new MockNativeMailItem());
			mailItem.AddReplyRecipient("poopoo@edi.com.au");

			mailItem.Inner.IsSecurityDialogEnabled = false;
			AssertEquals("ReplyRecipient should be successfully added", true, mailItem.Inner.ReplyRecipients.RecipientAdded);
		}

		[ExpectNoExceptions]
		public void TestAddAttachment()
		{
			var tempFilePath = $"{Guid.NewGuid()}.txt";
			try
			{
				File.Create(tempFilePath).Close();
				var mailItem = new MockOutlookMailItem(null);
				mailItem.AddAttachment(tempFilePath);
				mailItem.Inner.IsSecurityDialogEnabled = false;
				AssertEquals("Attachment should be successfully added", true,
					mailItem.Inner.Attachments.AttachmentAdded);
			}
			finally
			{
				File.Delete(tempFilePath);
			}
		}

		[ExpectException(typeof(OutlookException))]
		public void TestAddAttachment_EmptyString()
		{
			MockOutlookMailItem mailItem = new MockOutlookMailItem(new MockNativeMailItem());
			mailItem.AddAttachment("");
		}

		[ExpectException(typeof(OutlookException))]
		public void TestAddRecipient_EmptyString()
		{
			MockOutlookMailItem mailItem = new MockOutlookMailItem(new MockNativeMailItem());
			mailItem.AddRecipient("");
		}

		public void TestMailFileCreatedOnSend()
		{
			MockOutlookMailItem mailItem = new MockOutlookMailItem(null);
			mailItem.Display(false);
			mailItem.FileNameToSaveOnSend = Env.GetTempFileName();

			try
			{
				File.Delete(mailItem.FileNameToSaveOnSend);
			}
			catch (IOException) { }
			catch (UnauthorizedAccessException) { }

			try
			{
				mailItem.SimulateMailItemSendEvent();
				AssertEquals(true, File.Exists(mailItem.FileNameToSaveOnSend));
			}
			finally
			{
				try
				{
					File.Delete(mailItem.FileNameToSaveOnSend);
				}
				catch (IOException) { }
				catch (UnauthorizedAccessException) { }
			}
		}

		[ExpectNoExceptions]
		public void TestSubjectFromAnotherThread()
		{
			var threadstart = new ThreadStart(delegate
			{
				var mailItem = new MockOutlookMailItem(null);

				mailItem.MailItemSend += (data) =>
				{
					using (Db.Connection)
					{ }
				};

				mailItem.Display(false);
				mailItem.SimulateMailItemSendEvent();
			});
			var thread = new Thread(threadstart);
			thread.Start();
			thread.Join();
		}
	}
}
