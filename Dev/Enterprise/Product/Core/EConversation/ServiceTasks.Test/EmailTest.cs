using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using MimeKit;
using MimeKit.Utils;
using NUnit.Framework;

namespace Enterprise.EConversation.Testing
{
	sealed class EmailTest : TestCase
	{
		public void TestChineseChars()
		{
			var builder = new EmailBuilderForTesting();
			var email = builder.Body(@"I'm using some 汉字 Chinese characters").GetEmail();

			AssertEquals(@"I'm using some 汉字 Chinese characters", email.LatestMessageInBody);
		}

		public void TestReplyInEmlBody()
		{
			var email = new Email(resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.test.eml"));
			AssertEquals(@"Hi Luke,
If I want to deliver these emails to a Microsoft teams sub channel that I've created, I don't believe there is an internal group.   I could be mistaken?

Appreciate your time.

Regards
Cisco


Sent from my iPhone", email.LatestMessageInBody);
		}

		public void TestGetAttachmentTextWithRawXml()
		{
			Email email = new EmailBuilderForTesting().WithAttachment("attachment.xml", "<metrics></metrics>").GetEmail();
			AssertEquals("<metrics></metrics>\r\n", email.GetFirstAttachmentOrVisualText());
		}

		public void TestGetAttachmentTextWithZippedXml()
		{
			var bytes = resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.EDI-E00000003.zip");
			var email = new EmailBuilderForTesting().WithAttachment("EDI-E00000003.zip", bytes).GetEmail();
			AssertEquals("<EDI_Exception_Report></EDI_Exception_Report>", email.GetFirstAttachmentOrVisualText());
		}

		public void TestGetAttachmentTextWithUnknownZipFile()
		{
			var bytes = resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.EDI-E00000003.zip");
			var email = new EmailBuilderForTesting().WithAttachment("EDI-E00000004.zip", bytes).GetEmail();
			AssertEquals("", email.GetFirstAttachmentOrVisualText());
		}

		public void TestGetAttachmentTextWithInvalidZipFile()
		{
			var bytes = resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.InvalidZip.zip");
			var email = new EmailBuilderForTesting().WithAttachment("InvalidZip.zip", bytes).GetEmail();
			AssertEquals("", email.GetFirstAttachmentOrVisualText());
		}

		public void TestGetVisualTextWithRawXml()
		{
			Email email = new EmailBuilderForTesting().WithVisual("attachment.xml", "<metrics></metrics>").GetEmail();
			AssertEquals("<metrics></metrics>", email.GetFirstAttachmentOrVisualText());
		}

		public void TestGetVisualTextWithZippedXml()
		{
			var bytes = resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.EDI-E00000003.zip");
			var email = new EmailBuilderForTesting().WithVisual("EDI-E00000003.zip", bytes).GetEmail();
			AssertEquals("<EDI_Exception_Report></EDI_Exception_Report>", email.GetFirstAttachmentOrVisualText());
		}

		public void TestGetVisualTextWithUnknownZipFile()
		{
			var bytes = resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.EDI-E00000003.zip");
			var email = new EmailBuilderForTesting().WithVisual("EDI-E00000004.zip", bytes).GetEmail();
			AssertEquals("", email.GetFirstAttachmentOrVisualText());
		}

		public void TestGetVisualTextWithInvalidZipFile()
		{
			var bytes = resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.InvalidZip.zip");
			var email = new EmailBuilderForTesting().WithVisual("InvalidZip.zip", bytes).GetEmail();
			AssertEquals("", email.GetFirstAttachmentOrVisualText());
		}

		public void TestAttachmentCount()
		{
			Email email = new EmailBuilderForTesting().GetEmail();
			AssertEquals(0, email.NonVisualCount);
			AssertEquals(0, email.VisualCount);

			email = new EmailBuilderForTesting()
				.WithAttachment("attachment1.xml", "<metrics></metrics>")
				.WithVisual("visual1.xml", "<metrics></metrics>")
				.WithAttachment("attachment2.xml", "<metrics></metrics>")
				.WithVisual("visual2.xml", "<metrics></metrics>")
				.WithVisual("visual3.xml", "<metrics></metrics>")
				.GetEmail();
			AssertEquals(2, email.NonVisualCount);
			AssertEquals(3, email.VisualCount);
		}

		public void TestSender()
		{
			Email email = new EmailBuilderForTesting().GetEmail();
			AssertEquals("", email.SenderAddress);
			AssertEquals("", email.SenderName);
			AssertEquals("", email.SenderNameAddress);

			var message = new MimeMessage();
			message.Sender = new MailboxAddress("Name", "address@test.com");
			email = new Email(message.GetData());
			AssertEquals("address@test.com", email.SenderAddress);
			AssertEquals("Name", email.SenderName);
			AssertEquals("\"Name\" <address@test.com>", email.SenderNameAddress);

			message = new MimeMessage();
			message.Sender = new MailboxAddress("", "address@test.com");
			email = new Email(message.GetData());
			AssertEquals("address@test.com", email.SenderAddress);
			AssertEquals("", email.SenderName);
			AssertEquals("<address@test.com>", email.SenderNameAddress);
			email = new EmailBuilderForTesting().From("From", "from@test.com").GetEmail();
			AssertEquals("from@test.com", email.SenderAddress);
			AssertEquals("From", email.SenderName);
			AssertEquals("\"From\" <from@test.com>", email.SenderNameAddress);
		}

		public void TestGetAttachmentTextWithPasswordZippedXml()
		{
			byte[] bytes = CreatePasswordZip("Report.xml", "<metrics></metrics>", "somePassword");
			Email email = new EmailBuilderForTesting().WithAttachment("Report.zip", bytes).GetEmail();
			AssertEquals("<metrics></metrics>", email.GetFirstAttachmentOrVisualText("somePassword"));
		}

		public void TestGetAttachmentMovedWithinAttachmentByAntiVirus()
		{
			var bytes = CreatePasswordZip("Report.xml", "<metrics></metrics>", "somePassword");
			var attachedEml = new EmailBuilderForTesting().WithAttachment("Report.zip", bytes).Message;

			var message = new MimeMessage();
			var bodyBuilder = new BodyBuilder();
			bodyBuilder.Attachments.Add("Report", attachedEml.GetData(), new ContentType("message", "rfc822"));
			message.Body = bodyBuilder.ToMessageBody();

			var email = new Email(message.GetData());
			AssertEquals("<metrics></metrics>", email.GetFirstAttachmentOrVisualText("somePassword"));
		}

		public void TestGetAttachmentByIndex()
		{
			var builder = new EmailBuilderForTesting();
			builder.WithAttachment("name0.txt", Encoding.ASCII.GetBytes("content0"));
			builder.WithAttachment("name1.txt", Encoding.ASCII.GetBytes("content1"));
			builder.WithAttachment("name2.txt", Encoding.ASCII.GetBytes("content2"));
			builder.WithVisual("name3.txt", Encoding.ASCII.GetBytes("content3"));
			builder.WithVisual("name4.txt", Encoding.ASCII.GetBytes("content4"));
			var email = builder.GetEmail();
			AssertEquals("content3", email.GetAttachmentOrVisualText(0));
			AssertEquals("content4", email.GetAttachmentOrVisualText(1));
			AssertEquals("content0", email.GetAttachmentOrVisualText(2));
			AssertEquals("content1", email.GetAttachmentOrVisualText(3));
			AssertEquals("content2", email.GetAttachmentOrVisualText(4));
		}

		public void TestRemoveDodgySymbols()
		{
			var builder = new EmailBuilderForTesting();
			var email = builder.Body(@"I’m using all sorts of horrible “symbols”").GetEmail();

			AssertEquals(@"I'm using all sorts of horrible ""symbols""", email.LatestMessageInBody);
		}

		public void TestConvertLinksToHrefs()
		{
			var builder = new EmailBuilderForTesting();
			var email1 = builder.Body(@"first line
some text https://antongorlin.com
other text").GetEmail();

			var email2 = builder.Body(@"first line
some text <https://antongorlin.com>
other text").GetEmail();

			var email3 = builder.Body(@"first line
some text <www.antongorlin.com>
other text").GetEmail();

			var email4 = builder.Body(@"first line
some text https://antongorlin.com, https://500px.com
other text").GetEmail();

			var email5 = builder.Body(@"first line
some text https://antongorlin.com
https://500px.com
other text").GetEmail();

			var email6 = builder.Body(@"first line
some text https://dailymail.co.uk
other text").GetEmail();

			var email7 = builder.Body(@"first line
some text www.dailymail.co.uk
other text").GetEmail();

			var email8 = builder.Body(@"first line
some text www.dailymail.co.uk, www.asp.net, antongorlin.com
other text").GetEmail();

			var email9 = builder.Body(@"first line
some text anchor<https://antongorlin.com>
other text").GetEmail();

			var email10 = builder.Body(@"first line
some text [anchor]<https://antongorlin.com>
other text").GetEmail();

			var email11 = builder.Body(@"first line
some text anchor.com<https://antongorlin.com>
other text").GetEmail();

			var email12 = builder.Body(@"first line
some text [https://anchor.com]<https://antongorlin.com>
other text").GetEmail();

			var email13 = builder.Body(@"first line
<https://antongorlin.com>
other text").GetEmail();

			var email14 = builder.Body(@"first line
woohoo<https://antongorlin.com>
other text").GetEmail();

			var email15 = builder.Body(@"Cell: +61 402 826 524 | DDI: + 613 9122 2210 | Tel: +61-3-9120 4444

E-mail: agorlin@chest.com.au |  www.chest.com.au
<https://urldefense.com/v3/__http:/www.chest.com.au/__;!!Na5NE8kfbBOR6Ys!9GBn1R3U-GuVygSGfh2HZJVhtQPqd0Hh1HUQfF3b6JyVYKivbMooXmZkYIQFTGaErQlTFg$>").GetEmail();

			AssertEquals(@"first line
some text [link 1](https://antongorlin.com)
other text", email1.LatestMessageInBody);

			AssertEquals(@"first line
some text [link 1](https://antongorlin.com)
other text", email2.LatestMessageInBody);

			AssertEquals(@"first line
some text [link 1](www.antongorlin.com)
other text", email3.LatestMessageInBody);

			AssertEquals(@"first line
some text [link 1](https://antongorlin.com), [link 2](https://500px.com)
other text", email4.LatestMessageInBody);

			AssertEquals(@"first line
some text [link 1](https://antongorlin.com)
[link 2](https://500px.com)
other text", email5.LatestMessageInBody);

			AssertEquals(@"first line
some text [link 1](https://dailymail.co.uk)
other text", email6.LatestMessageInBody);

			AssertEquals(@"first line
some text [link 1](www.dailymail.co.uk)
other text", email7.LatestMessageInBody);

			AssertEquals(@"first line
some text [link 1](www.dailymail.co.uk), [link 2](www.asp.net), antongorlin.com
other text", email8.LatestMessageInBody);

			AssertEquals(@"first line
some text [anchor](https://antongorlin.com)
other text", email9.LatestMessageInBody);

			AssertEquals(@"first line
some text [anchor](https://antongorlin.com)
other text", email10.LatestMessageInBody);

			AssertEquals(@"first line
some text [anchor.com](https://antongorlin.com)
other text", email11.LatestMessageInBody);

			AssertEquals(@"first line
some text [https://anchor.com](https://antongorlin.com)
other text", email12.LatestMessageInBody);

			AssertEquals(@"first [line](https://antongorlin.com)
other text", email13.LatestMessageInBody);

			AssertEquals(@"first line
[woohoo](https://antongorlin.com)
other text", email14.LatestMessageInBody);

			AssertEquals(@"Cell: +61 402 826 524 | DDI: + 613 9122 2210 | Tel: +61-3-9120 4444

E-mail: agorlin@chest.com.au |  [www.chest.com.au](https://urldefense.com/v3/__http:/www.chest.com.au/__;!!Na5NE8kfbBOR6Ys!9GBn1R3U-GuVygSGfh2HZJVhtQPqd0Hh1HUQfF3b6JyVYKivbMooXmZkYIQFTGaErQlTFg$)", email15.LatestMessageInBody);
		}

		public void TestLatestMessageInBody()
		{
			var builder = new EmailBuilderForTesting();
			var email1 = builder.Body(@"reply

---original message---
original here!").WithForward("1").GetEmail();

			var email2 = builder.Body(@"forward 1
---forwarded message---
something forwarded 1").WithForward("1").GetEmail();

			var email3 = builder.Body(@"forward 2
--- forwarded message ---
something forwarded 2").WithForward("1").GetEmail();

			var email4 = builder.Body(@"reply 1

on 07-05-2020 don antonio wrote:
old message 1").WithForward("1").GetEmail();

			var email5 = builder.Body(@"reply 2

on 07-05-2020 123@email.com wrote:
old message 2").WithForward("1").From("123@email.com").GetEmail();

			var email6 = new EmailBuilderForTesting().Body(@"reply

---original message---
original here!").GetEmail();

			var email7 = builder.Body(@"reply 3

From: 123@email.com
old message 2").WithForward("1").From("123@email.com").GetEmail();

			var email8 = builder.Body(@"reply 4

De: Pablo Picasso [mailto:support@wisetechglobal.com] 
Enviado el: quinco, 22 de mayo de 2020 3:33
Para: asa.vaba@globalartist.es
Asunto: eRequest: CS00000001 has been scheduled for development work").WithForward("1").From("123@email.com").GetEmail();

			var email9 = builder.Body(@"reply 5

De: Customer Service <support@wisetechglobal.com>
Enviado: Tuesday, 22 May 2020 3:33 PM
Para: Aaron Dillon <asa.vaba@globalartist.es>
Asunto: Update on Incident: CS00000001 - Calendar not working").WithForward("1").From("123@email.com").GetEmail();

			var email10 = builder.Body(@"reply 6

--REPLY above this line--
blah").WithForward("1").From("123@email.com").GetEmail();

			AssertEquals("reply", email1.LatestMessageInBody.Trim());
			AssertEquals("forward 1", email2.LatestMessageInBody.Trim());
			AssertEquals("forward 2", email3.LatestMessageInBody.Trim());
			AssertEquals("reply 1", email4.LatestMessageInBody.Trim());
			AssertEquals("reply 2", email5.LatestMessageInBody.Trim());
			AssertEquals("reply", email6.LatestMessageInBody.Trim());
			AssertEquals("reply 3", email7.LatestMessageInBody.Trim());
			AssertEquals("reply 4", email8.LatestMessageInBody.Trim());
			AssertEquals("reply 5", email9.LatestMessageInBody.Trim());
			AssertEquals("reply 6", email10.LatestMessageInBody.Trim());
		}

		public void TestLatestMessageInBodyShouldReportIssueIfRegexMatchTimesOut()
		{
			var builder = new EmailBuilderForTesting();
			var veryLongMessage = @"VeryLongMessage
VeryLongMessage
VeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessage
";
			ErrorReporter.Clear();
			var email = builder.Subject("subj").Body(veryLongMessage).WithForward("1").GetEmail(TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(1));

			AssertEquals(veryLongMessage.Trim(), email.LatestMessageInBody.Trim());

			AssertEquals(@"Aggregated Regex Timeout Error while processing email. Subject: subj".Trim(), ErrorReporter.LastMessageReported);
			AssertType(typeof(RegexMatchTimeoutException), ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
		}

		public void TestLatestMessageInBody_NoExceptionIfHandledByTimeoutIncrease()
		{
			var builder = new EmailBuilderForTesting();
			var veryLongMessage = @"VeryLongMessage
VeryLongMessage
VeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessageVeryLongMessage
";
			ErrorReporter.Clear();
			var email = builder.Subject("subj").Body(veryLongMessage).WithForward("1").GetEmail(TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(1));

			AssertEquals(veryLongMessage.Trim(), email.LatestMessageInBody.Trim());

			AssertEquals(null, ErrorReporter.LastExceptionReported);
		}

		public void TestLatestMessageInBody_RegexMatchTimesOut_LongLines()
		{
			var builder = new EmailBuilderForTesting();
			var veryLongMessage = @"Hi Nevashen,This account is already rollout with Cargowise since 2nd March 2021.Alias: CGWEFLMYThank you.Best regards,Vernon LeongSenior Customer Service SpecialistINTTRA Global Customer Service Center--------------- Original Message ---------------From: Nevashen Anamalay [support@wisetechglobal.com]Sent: 15/04/2021 22:03To: customersupport@e2open.comCc: nevashen.anamalay@wisetechglobal.comSubject: INTTRA connection:EXK | EFL MALAYSIA SDN. BHD| WiseTechGlobal| CS00938431 |INTTRA connection:EXK | EFL MALAYSIA SDN. BHD| WiseTechGlobal| CS00938431 |
[Inline image name : Banner.jpg]
Good day INTTRA team,Our mutual customer has contacted us about setting them up for INTTRA messaging.INTTRA ID: 871664INTTRA ALIAS: COMPANY NAME: EFL MALAYSIA SDN. BHD, D-08-07, BLOCK D, LEVEL 8, SKY PARK @ ONE CITY, SUBANG JAYA, MYPlease have them set up to:EDI ID: CARGOWISEFTP: c0235380 Thanks in advance.Nevashen AnamalayCargoWise | Installation Engineer | Carrier Connection TeamWiseTech Global support@wisetechglobal.comwisetechglobal.com [Inline image name : Footer.jpg]

ref:_00D30N7k._5004z1Vy8a4:ref";
			var email = builder.Subject("subj").Body(veryLongMessage).WithForward("1").GetEmail(TimeSpan.FromSeconds(1));
			AssertEquals("Hi Nevashen,This account is already rollout with Cargowise since 2nd March 2021.Alias: CGWEFLMYThank you.Best regards,Vernon LeongSenior Customer Service SpecialistINTTRA Global Customer Service Center--------------- Original Message ---------------", email.LatestMessageInBody.Trim());
		}

		public void TestSuperLargeMessagesAreTruncated()
		{
			var builder = new EmailBuilderForTesting();
			var email = builder.Body("a ".PadRight(50 * 1024) + @"

---original message---
original here!").WithForward("1").GetEmail();

			string truncateMessage = "\r\n----------\r\nMessage truncated. See eDocs for a full message.";
			AssertEquals(10240 + truncateMessage.Length, email.LatestMessageInBody.Length);
		}

		byte[] CreatePasswordZip(string fileName, string fileContents, string password)
		{
			var data = Encoding.UTF8.GetBytes(fileContents);
			using (var contentStream = new MemoryStream(data))
			using (var zipStream = new MemoryStream())
			{
				var creator = new ZipCreator(password);
				creator.ZipStream(new ZipStream[] { new ZipStream(fileName, contentStream) }, zipStream);
				return zipStream.ToArray();
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}

	public class EmailBuilderForTesting
	{
		public EmailBuilderForTesting Subject(string subject)
		{
			message.Subject = subject;
			return this;
		}

		public EmailBuilderForTesting From(string address)
		{
			return From(null, address);
		}

		public EmailBuilderForTesting From(string name, string address)
		{
			message.From.Add(new MailboxAddress(name, address));
			return this;
		}

		public EmailBuilderForTesting To(string name, string address)
		{
			message.To.Add(new MailboxAddress(name, address));
			return this;
		}

		public EmailBuilderForTesting Cc(string name, string address)
		{
			message.Cc.Add(new MailboxAddress(name, address));
			return this;
		}

		public EmailBuilderForTesting Bcc(string name, string address)
		{
			message.Bcc.Add(new MailboxAddress(name, address));
			return this;
		}

		public EmailBuilderForTesting Body(string bodyText)
		{
			if (bodyText != null)
			{
				bodyBuilder.TextBody = bodyText;
				message.Body = bodyBuilder.ToMessageBody();
			}
			return this;
		}

		public EmailBuilderForTesting WithForward(string mailId)
		{
			message.InReplyTo = mailId;
			return this;
		}

		public EmailBuilderForTesting HtmlBody(string bodyText)
		{
			if (bodyText != null)
			{
				var html2Text = new HtmlToTextUtility();
				bodyBuilder.HtmlBody = bodyText;
				bodyBuilder.TextBody = html2Text.GetPlainText(bodyText);
				message.Body = bodyBuilder.ToMessageBody();
			}
			return this;
		}

		public EmailBuilderForTesting WithAttachment(string fileName, byte[] data)
		{
			bodyBuilder.Attachments.Add(fileName, data);
			message.Body = bodyBuilder.ToMessageBody();
			return this;
		}

		public EmailBuilderForTesting WithAttachment(string fileName, byte[] data, ContentType contentType)
		{
			bodyBuilder.Attachments.Add(fileName, data, contentType);
			message.Body = bodyBuilder.ToMessageBody();
			return this;
		}

		public EmailBuilderForTesting WithAttachment(string fileName, string dataString)
		{
			return WithAttachment(fileName, Encoding.ASCII.GetBytes(dataString));
		}

		public EmailBuilderForTesting WithAttachment(string fileName, Email email)
		{
			bodyBuilder.Attachments.Add(fileName, email.GetEmlBytes(), new ContentType("message", "rfc822"));
			message.Body = bodyBuilder.ToMessageBody();
			return this;
		}

		public EmailBuilderForTesting WithVisual(string fileName, byte[] data, string contentId = null)
		{
			if (string.IsNullOrEmpty(message.HtmlBody))
			{
				bodyBuilder.HtmlBody = "";
			}
			bodyBuilder.LinkedResources.Add(fileName, data).ContentId = contentId ?? MimeUtils.GenerateMessageId();
			message.Body = bodyBuilder.ToMessageBody();
			return this;
		}

		public EmailBuilderForTesting WithVisual(string fileName, string dataString)
		{
			return WithVisual(fileName, Encoding.ASCII.GetBytes(dataString));
		}

		public EmailBuilderForTesting WithVisual(string fileName, Email email)
		{
			if (string.IsNullOrEmpty(message.HtmlBody))
			{
				bodyBuilder.HtmlBody = "";
			}
			bodyBuilder.LinkedResources.Add(fileName, email.GetEmlBytes(), new ContentType("message", "rfc822"));
			message.Body = bodyBuilder.ToMessageBody();
			return this;
		}

		public Email GetEmail(TimeSpan? regexTimeout = null, TimeSpan? timeoutIncrease = null)
		{
			return new EmailForTesting(message.GetData(), regexTimeout ?? TimeSpan.FromSeconds(1), timeoutIncrease ?? TimeSpan.FromSeconds(2));
		}

		class EmailForTesting : Email
		{
			public EmailForTesting(byte[] email, TimeSpan timeout, TimeSpan aggregatedTimeout) : base(email)
			{
				RegexTimeout = timeout;
				AggregatedTimeout = aggregatedTimeout;
			}

			protected override TimeSpan RegexTimeout
			{
				get;
			}

			protected override TimeSpan AggregatedTimeout
			{
				get;
			}
		}

		internal MimeMessage Message
		{
			get { return message; }
			set { message = value; }
		}

		MimeMessage message = new MimeMessage();
		readonly BodyBuilder bodyBuilder = new BodyBuilder();
	}

	public class EmailForTest : Email
	{
		public EmailForTest(byte[] eml)
			: base(eml)
		{ }

		public override string GetFirstAttachmentOrVisualText()
		{
			if (Body == "test index out of range on decoding zip")
			{
				throw new System.IndexOutOfRangeException("Index was outside the bounds of the array.");
			}
			return base.GetFirstAttachmentOrVisualText(null);
		}
	}
}
