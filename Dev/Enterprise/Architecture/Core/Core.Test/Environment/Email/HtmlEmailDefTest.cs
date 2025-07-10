using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class HtmlEmailDefTest : TransactionedTestCase
	{
		public void TestLoadHtmlUsingTemplateWithoutBannerFooterImage()
		{
			using (Bitmap banner = new Bitmap(10, 20))
			using (Bitmap footer = new Bitmap(20, 40))
			{
				SystemDataRegistryForTest.Get().HtmlEmailBannerImage = banner;
				SystemDataRegistryForTest.Get().HtmlEmailFooterImage = footer;

				HtmlEmailDef email = new HtmlEmailDef();
				email.Subject = "Email Subject";
				email.LoadHtmlUsingTemplate(UnformattedEmailHtmlBody, null, Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals(0, email.Attachments.Count);
				AssertNotContains("Banner.jpg", email.Body);
				AssertNotContains("Footer.jpg", email.Body);
			}
		}

		[ExpectNoExceptions]
		public void TestLoadHtmlUsingTemplateInMultiThread()
		{
			using (Bitmap banner = new Bitmap(10, 20))
			using (Bitmap footer = new Bitmap(20, 40))
			{
				SystemDataRegistryForTest.Get().HtmlEmailBannerImage = banner;
				SystemDataRegistryForTest.Get().HtmlEmailFooterImage = footer;

				var task1 = Task.Run(() =>
				{
					using (CargoWise.Data.Db.DisposableActionForDbConnection())
					{
						HtmlEmailDef email = new HtmlEmailDef();
						email.Subject = "Email Subject";
						var loopCount = 100;
						while (loopCount-- > 0)
						{
							email.LoadHtmlUsingTemplate(UnformattedEmailHtmlBody);
						}
					}
				});

				var task2 = Task.Run(() =>
				{
					using (CargoWise.Data.Db.DisposableActionForDbConnection())
					{
						HtmlEmailDef email = new HtmlEmailDef();
						email.Subject = "Email Subject";
						var loopCount = 100;
						while (loopCount-- > 0)
						{
							email.LoadHtmlUsingTemplate(UnformattedEmailHtmlBody);
						}
					}
				});

				Task.WaitAll(task1, task2);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoesNotCrashIfImagesAreNull()
		{
			SystemDataRegistryForTest.Get().HtmlEmailBannerImage = null;
			SystemDataRegistryForTest.Get().HtmlEmailFooterImage = null;
			HtmlEmailDef email = new HtmlEmailDef();
			email.Subject = "Email Subject";
			email.LoadHtmlUsingTemplate(UnformattedEmailHtmlBody);
			AssertEmailContent(email, new Size(1, 1), new Size(1, 1));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadUsingTemplate()
		{
			using (Bitmap banner = new Bitmap(10, 20))
			using (Bitmap footer = new Bitmap(20, 40))
			{
				SystemDataRegistryForTest.Get().HtmlEmailBannerImage = banner;
				SystemDataRegistryForTest.Get().HtmlEmailFooterImage = footer;

				HtmlEmailDef email = new HtmlEmailDef();
				email.Subject = "Email Subject";
				email.LoadHtmlUsingTemplate(UnformattedEmailHtmlBody);
				AssertEmailContent(email, new Size(10, 20), new Size(20, 40));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadUsingTemplate_MultipleUseOfBitmaps()
		{
			using (Bitmap banner = new Bitmap(10, 20))
			using (Bitmap footer = new Bitmap(20, 40))
			{
				SystemDataRegistryForTest.Get().HtmlEmailBannerImage = banner;
				SystemDataRegistryForTest.Get().HtmlEmailFooterImage = footer;

				HtmlEmailDef email = new HtmlEmailDef { Subject = "Email Subject" };
				email.LoadHtmlUsingTemplate(UnformattedEmailHtmlBody);
				AssertEmailContent(email, new Size(10, 20), new Size(20, 40));

				email = new HtmlEmailDef { Subject = "Email Subject" };
				email.LoadHtmlUsingTemplate(UnformattedEmailHtmlBody);
				AssertEmailContent(email, new Size(10, 20), new Size(20, 40));

				email = new HtmlEmailDef { Subject = "Email Subject" };
				email.LoadHtmlUsingTemplate(UnformattedEmailHtmlBody);
				AssertEmailContent(email, new Size(10, 20), new Size(20, 40));
			}
		}

		public void TestLoadUsingTemplate_AdditionalCssElements()
		{
			HtmlEmailDef email = new HtmlEmailDef();
			string additionalCssElements = @"
.newStyle
{
	font-weight: bold;
}
".Trim();
			email.LoadHtmlUsingTemplate(UnformattedEmailHtmlBody, additionalCssElements);
			Assert(email.Body.Contains(UnformattedEmailHtmlBody));
			Assert(email.Body.Contains(ObjectFactory.Get<ISystemDataRegistry>().HtmlEmailStyleSheet));
			Assert(email.Body.Contains(additionalCssElements));
		}

		public void TestLoadPlainTextUsingTemplate_MultipleLines()
		{
			var email = new HtmlEmailDef();
			email.LoadPlainTextUsingTemplate(@"line1
line2
line3

line4
");
			Assert(email.Body.Contains("line1"));
			Assert(email.Body.Contains("line2"));
			Assert(email.Body.Contains("line3"));
			Assert(email.Body.Contains("line4"));
		}

		public void TestLoadPlainText_MultipleLines()
		{
			var plainTextBody = @"line1
line2
line3

line4
";
			var html = new HtmlEmailDef().GenerateHtmlBody(plainTextBody);
			Assert(html.Equals("<p style=\"margin: 1em\">line1<br />line2<br />line3<br /><br />line4<br /></p>"));
		}

		#region Implementation

		ZString UnformattedEmailHtmlBody
		{
			get { return "This text will go into the body.\r\n"; }
		}

		ZString ExpectedEmailHtmlBody
		{
			get
			{
				ZString result = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\Testing\DocumentParsing\EmailBodyTestFile.htm");
				result = result.Replace("(*StyleSheet*)", ObjectFactory.Get<ISystemDataRegistry>().HtmlEmailStyleSheet);
				result = result.Replace("(*Body*)", "This text will go into the body.");
				return result;
			}
		}

		void AssertAttachment(AttachmentDef attachment, Size expectedSize)
		{
			using (MemoryStream stream = new MemoryStream(attachment.Data))
			using (Image image = Image.FromStream(stream))
			{
				AssertEquals(attachment.DisplayName + " Size", expectedSize, image.Size);
			}
		}

		void AssertEmailContent(EmailDef email, Size expectedBannerSize, Size expectedFooterSize)
		{
			AssertEquals("Subject", "Email Subject", email.Subject);
			AssertMultilineASCIIEquals("Body", ExpectedEmailHtmlBody, email.Body);
			AssertEquals("ContentType", EmailContentTypes.HTML, email.ContentType);
			AssertEquals("Attachments.Count", 2, email.Attachments.Count);
			AssertEquals("Attachments[0].DisplayName", "Banner.jpg", email.Attachments[0].DisplayName);
			AssertEquals("Attachments[1].DisplayName", "Footer.jpg", email.Attachments[1].DisplayName);
			AssertAttachment(email.Attachments[0], expectedBannerSize);
			AssertAttachment(email.Attachments[1], expectedFooterSize);
		}

		#endregion
	}
}
