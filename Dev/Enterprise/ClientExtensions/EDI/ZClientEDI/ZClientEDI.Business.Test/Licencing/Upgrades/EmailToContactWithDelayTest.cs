using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Mail.Business.Test;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(EmailToContactWithDelay))]
	class EmailToContactWithDelayTest : CustomerServiceEmailTestCase<EmailToContactWithDelay>
	{
		class TestEmailToContactWithDelay : EmailToContactWithDelay
		{
			public TestEmailToContactWithDelay(BusinessObject businessObjectSendingEmail, TimeSpan sendingDelay)
				: base(businessObjectSendingEmail, sendingDelay)
			{
			}

			public new bool ShouldSaveToEDocs
			{
				get
				{
					return base.ShouldSaveToEDocs;
				}
			}
		}

		public void TestShouldSaveToEDocs()
		{
			TestEmailToContactWithDelay emailContactObject = new TestEmailToContactWithDelay(BusinessObjectSendingEmail, TimeSpan.MinValue);
			AssertEquals("true for CustomerServiceEmailTestCase", false, emailContactObject.ShouldSaveToEDocs);
		}

		[ExpectException(typeof(InvalidCastException))]
		public void TestIDocManagerSupport()
		{
			//If it supports this interface EDocs should be tested.
			Enterprise.MasterFiles.Business.IDocManagerSupport docManagerSupport = (Enterprise.MasterFiles.Business.IDocManagerSupport)BusinessObjectSendingEmail;
		}

		protected override void AssertEmailPriority(string priority)
		{
			Assert("Does not support Priority.", true);
		}

		protected override void AssertEmailSent()
		{
			AssertNotNull("MailItem should exist.", Factory.LoadTop1<MailItem>(FindTestMailItemQuery));
		}

		protected override void AssertNoEmailSent()
		{
			AssertNull("MailItem should not exist.", Factory.LoadTop1<MailItem>(FindTestMailItemQuery));
		}

		protected override void AssertSentEmail()
		{
			MailItem email = Factory.LoadTop1<MailItem>(FindTestMailItemQuery);
			AssertNotNull("MailItem should exist.", email);
			AssertEquals("MI_From", "From Me <From@Me.com>", email.MI_From);
			AssertEquals("MailRecipients.Count", 4, email.MailRecipients.Count);
			AssertEquals("MailRecipients(0)", "1@1.com", email.MailRecipients[0].EmailAddress);
			AssertEquals("MailRecipients(0).RecipientType", nameof(MailRecipient.RecipientTypes.TO), email.MailRecipients[0].MR_RecipientType);
			AssertEquals("MailRecipients(1)", "2@2.com", email.MailRecipients[1].EmailAddress);
			AssertEquals("MailRecipients(1).RecipientType", nameof(MailRecipient.RecipientTypes.TO), email.MailRecipients[1].MR_RecipientType);
			AssertEquals("MailRecipients(2)", "3@3.com", email.MailRecipients[2].EmailAddress);
			AssertEquals("MailRecipients(2).RecipientType", nameof(MailRecipient.RecipientTypes.CC), email.MailRecipients[2].MR_RecipientType);
			AssertEquals("MailRecipients(3)", "4@4.com", email.MailRecipients[3].EmailAddress);
			AssertEquals("MailRecipients(3).RecipientType", nameof(MailRecipient.RecipientTypes.CC), email.MailRecipients[3].MR_RecipientType);
			AssertEquals("MI_Subject", "This is the subject.", email.MI_Subject);
			AssertEquals("MI_Body.Contains(EmailContactObject.HtmlStyleSheet)", true, email.MI_Body.Contains(SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value));
			AssertEquals("MI_Body.Contains(EmailContactObject.Body)", true, email.MI_Body.Contains(EmailContactObject.Body));
			Assert("email.MI_SendDateTime should be delayed.", email.MI_SendDateTime > ZDateTime.UtcNow.AddMinutes(25));
		}

		protected override void AssertAttachment(int index, string fileName, byte[] content)
		{
			MailItem email = Factory.LoadTop1<MailItem>(FindTestMailItemQuery);
			AssertNotNull("MailItem should exist.", email);
			AssertEquals(string.Format(CultureInfo.CurrentCulture, "email.MailAttachments[{0}].MA_FileName", index), fileName, email.MailAttachments[index].MA_FileName);
			AssertEquals(string.Format(CultureInfo.CurrentCulture, "email.MailAttachments[{0}].MA_Data", index), content, email.MailAttachments[index].MA_Data);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EmailToContactWithDelay(BusinessObjectSendingEmail, new TimeSpan(0, 30, 0));
		}

		public override void TestSendEmailSaveStrategies()
		{
			Assert("Does not apply for EmailToContactWithDelay", true);
		}

		public override void TestOverwriteEmailFromAddressAndName()
		{
			EmailContactObject.ToEmailAddress = "1@1.com";
			EmailContactObject.Subject = "This is the subject.";
			EmailContactObject.Body = "This is the body.";

			EmailContactObject.FromEmailAddress = "";
			EmailContactObject.FromDisplayName = "";
			EmailContactObject.SendEmail();
			MailItem email = Factory.LoadTop1<MailItem>(FindTestMailItemQuery);
			AssertNotEquals("EmailContactObject.FromDisplayName and FromAddress", "", email.MI_From);

			EmailContactObject.FromEmailAddress = "Test@cargowise.com";
			EmailContactObject.FromDisplayName = "Developer";
			EmailContactObject.SendEmail();
			email = Factory.LoadTop1<MailItem>(FindTestMailItemQuery);
			AssertEquals("EmailContactObject.FromDisplayName and FromAddress", "Developer <Test@cargowise.com>", email.MI_From);
		}

		ZQuery FindTestMailItemQuery
		{
			get
			{
				if (findTestMailItemQuery == null)
				{
					findTestMailItemQuery = new ZQuery(MailDBItemsSchema.MI_Direction, MailDirection.Transmit);
					findTestMailItemQuery.AddToFilter(MailDBItemsSchema.MI_Status, MailStatus.Queued);
					findTestMailItemQuery.AddToFilter(MailDBItemsSchema.MI_Subject, "This is the subject.");
					findTestMailItemQuery.OrderBy = MailDBItemsSchema.MI_SendDateTime.Name + " DESC";
				}
				return findTestMailItemQuery;
			}
		}

		ZQuery findTestMailItemQuery;
	}
}
