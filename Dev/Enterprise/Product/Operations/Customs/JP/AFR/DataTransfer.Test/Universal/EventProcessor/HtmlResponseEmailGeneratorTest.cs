using System;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	class HtmlResponseEmailGeneratorTest : TestCaseWithFactory
	{
		public void TestGenerateEmail()
		{
			var generator = new HtmlResponseEmailGenerator();
			var htmlTable = new HtmlTableCreator(new string[] { "Column1", "Column2" }).ToHtml();
			var responseDescription = generator.ResponseDescription;
			var footerHtml = "<strong>Hello Everyone</strong>";
			var email = generator.GenerateEmail("silly subject", "silly header", "silly description", htmlTable, footerHtml, GlbBranch.CurrentBranch);
			AssertEquals("Subject", "silly subject", email.Subject);
			AssertContains("silly header", email.Body);
			AssertContains("silly description", email.Body);
			AssertContains(htmlTable, email.Body);
			AssertContains(footerHtml, email.Body);

			AssertContains(TestHtmlEmailStyleSheet, email.Body);
			var banner = email.Attachments.Cast<AttachmentDef>().Single(x => x.DisplayName == "Banner.jpg");
			var footer = email.Attachments.Cast<AttachmentDef>().Single(x => x.DisplayName == "Footer.jpg");
			var bannerImage = new Bitmap(new MemoryStream(banner.Data));
			var footerImage = new Bitmap(new MemoryStream(footer.Data));
			AssertEquals(1, bannerImage.Width);
			AssertEquals(2, footerImage.Width);
		}

		public void TestGenerateEmailWithUri()
		{
			var generator = new HtmlResponseEmailGenerator();
			var htmlTable = new HtmlTableCreator(new string[] { "Column1", "Column2" }).ToHtml();
			var responseDescription = generator.ResponseDescription;
			var footerHtml = "<strong>Hello Everyone</strong>";
			var email = generator.GenerateEmail("uristuff", "B0011123", "Test Type", htmlTable, footerHtml, false, GlbBranch.CurrentBranch);
			AssertEquals("Subject", "Test Type Response for B0011123", email.Subject);
			AssertContains(email.Subject, email.Body);
			AssertContains(responseDescription, email.Body);
			AssertContains("B0011123", email.Body);
			AssertContains(htmlTable, email.Body);
			AssertContains(footerHtml, email.Body);

			generator.ResponseDescription = "Hello People";
			email = generator.GenerateEmail("uriStuff", "B0011123", "Test Type", htmlTable, "", true, GlbBranch.CurrentBranch);
			AssertEquals("Subject", "Test Type Response (Failure) for B0011123", email.Subject);
			AssertContains(email.Subject, email.Body);
			AssertNotContains(responseDescription, email.Body);
			AssertContains("Hello People", email.Body);
			AssertContains("B0011123", email.Body);
			AssertContains(htmlTable, email.Body);
			AssertNotContains(footerHtml, email.Body);

			AssertContains(TestHtmlEmailStyleSheet, email.Body);
			var banner = email.Attachments.Cast<AttachmentDef>().Single(x => x.DisplayName == "Banner.jpg");
			var footer = email.Attachments.Cast<AttachmentDef>().Single(x => x.DisplayName == "Footer.jpg");
			var bannerImage = new Bitmap(new MemoryStream(banner.Data));
			var footerImage = new Bitmap(new MemoryStream(footer.Data));
			AssertEquals(1, bannerImage.Width);
			AssertEquals(2, footerImage.Width);
		}

		public void TestGenerateEmailWithoutJob()
		{
			var generator = new HtmlResponseEmailGenerator();
			var htmlTable = new HtmlTableCreator(new string[] { "Column1", "Column2" }).ToHtml();
			var responseDescription = generator.ResponseDescription;
			var footerHtml = "<strong>Hello Everyone</strong>";
			var email = generator.GenerateEmail("", "", "Test Type", htmlTable, footerHtml, false, GlbBranch.CurrentBranch);
			AssertEquals("Subject", "Test Type Response", email.Subject);
			AssertContains(email.Subject, email.Body);

			AssertContains(TestHtmlEmailStyleSheet, email.Body);
			var banner = email.Attachments.Cast<AttachmentDef>().Single(x => x.DisplayName == "Banner.jpg");
			var footer = email.Attachments.Cast<AttachmentDef>().Single(x => x.DisplayName == "Footer.jpg");
			var bannerImage = new Bitmap(new MemoryStream(banner.Data));
			var footerImage = new Bitmap(new MemoryStream(footer.Data));
			AssertEquals(1, bannerImage.Width);
			AssertEquals(2, footerImage.Width);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var bannerImage = new Bitmap(1, 2);
			var footerImage = new Bitmap(2, 1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, bannerImage);
			SystemDataRegistry.Instance.HtmlEmailFooterImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, footerImage);
			SystemDataRegistry.Instance.HtmlEmailStyleSheet.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, TestHtmlEmailStyleSheet);
		}

		const string TestHtmlEmailStyleSheet = "HtmlEmailStyleSheet Bla Bla";
	}
}
