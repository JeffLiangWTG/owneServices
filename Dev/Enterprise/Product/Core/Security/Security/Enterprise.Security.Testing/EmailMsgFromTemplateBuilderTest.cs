using System;
using System.Drawing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Security.Testing
{
	sealed class EmailMsgFromTemplateBuilderTest : TransactionedTestCase
	{
		public void TestBuildEmailDefFromTemplate()
		{
			Guid companyPK = Guid.Empty;
			EmailDef email = new EmailMsgFromTemplateBuilder().BuildEmailDefFromTemplate("My Heading", "My Message", "My Footer", companyPK);
			AssertNotNull("Email should be created", email);

			AssertEquals("ContentType", EmailContentTypes.HTML, email.ContentType);
			AssertEquals("FromDisplayName", Enterprise.Core.Constants.ProductName, email.FromDisplayName);
			AssertEquals("Two Attachments", 2, email.Attachments.Count);

			AssertEquals("Subject", "My Heading", email.Subject);

			AssertEquals("Email Body should be not empty", false, string.IsNullOrEmpty(email.Body));

			Assert("Contains HtmlStyleSheet", email.Body.Contains(SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value));
			Assert("Contains Heading", email.Body.Contains("My Heading"));
			Assert("Contains Message", email.Body.Contains("My Message"));
			Assert("Contains Footer", email.Body.Contains("My Footer"));
		}

		public void TestHeaderAttachment()
		{
			Guid companyPK = Guid.NewGuid();
			Image img = new Bitmap(1, 1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(companyPK, Guid.Empty, Guid.Empty, img);
			EmailDef email = new EmailMsgFromTemplateBuilder().BuildEmailDefFromTemplate("My Heading", "My Message", "My Footer", companyPK);
			AssertNotNull("Email should be created", email);
			AssertEquals("Two Attachments", 2, email.Attachments.Count);

			companyPK = Guid.Empty;
			EmailDef email2 = new EmailMsgFromTemplateBuilder().BuildEmailDefFromTemplate("My Heading", "My Message", "My Footer", companyPK);
			AssertNotNull("Email should be created", email2);
			AssertEquals("Two Attachments", 2, email2.Attachments.Count);

			AssertNotEquals(email.Attachments[0].Data, email2.Attachments[0].Data);
			AssertEquals(email.Attachments[1].Data, email2.Attachments[1].Data);
		}

		public void TestBuildEmailDefFromTemplateForExistingEmailDef()
		{
			EmailDef email = new EmailDef();
			email.FromDisplayName = "Exisitng From";
			email.FromAddress = "from@cargowise.com";
			email.AddRecipientForUserCommunication("Existing.Recipient@cargowise.com");
			email.AddRecipientForUserCommunication("Existing.Recipient@cargowise.com", RecipientDef.RecipientTypes.CC);
			email.Subject = "Existing Subject";
			email.Attachments.Add(new AttachmentDef("Existing Attachment", new byte[] { 1, 2, 3 }));
			email.ContentType = EmailContentTypes.PlainText;
			email.Body = "Existing Body";

			Guid companyPK = Guid.Empty;

			new EmailMsgFromTemplateBuilder().BuildEmailDefFromTemplate(email, "My Heading", "My Message", "My Footer", companyPK);

			AssertEquals("FromDisplayName should not be set to ProductName", Enterprise.Core.Constants.ProductName, email.FromDisplayName);
			AssertEquals("FromAddresse should not changed", "from@cargowise.com", email.FromAddress);
			AssertEquals("Recipients should not be changed", 1, email.Recipients.Count);
			AssertEquals("Recipients should not be changed", "Existing.Recipient@cargowise.com", email.Recipients[0]);
			AssertEquals("ContentType should be set to HTML", EmailContentTypes.HTML, email.ContentType);
			AssertEquals("Should be two extra attachments", 3, email.Attachments.Count);
			AssertEquals("Subject should not be changed", "Existing Subject", email.Subject);

			AssertNotEquals("Email Body should be changed", "Existing Body", string.IsNullOrEmpty(email.Body));
			AssertEquals("Email Body should be not empty", false, string.IsNullOrEmpty(email.Body));

			Assert("Contains HtmlStyleSheet", email.Body.Contains(SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value));
			Assert("Contains Heading", email.Body.Contains("My Heading"));
			Assert("Contains Message", email.Body.Contains("My Message"));
			Assert("Contains Footer", email.Body.Contains("My Footer"));
		}
	}
}
