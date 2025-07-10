using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Messaging.Business.Testing
{
	public class HtmlResponseEmailGeneratorTest : TestCaseWithFactory
	{
		public void TestGenerateEmailFail()
		{
			var generator = new HtmlResponseEmailGenerator();
			generator.SetCreatingEmailDefFailForTest = true;
			var htmlTable = new HtmlTableCreator(new string[] { "Column1", "Column2" }).ToHtml();
			var footerHtml = "<strong>Hello Everyone</strong>";
			Assert(!generator.TryGenerateEmail("silly subject", "silly header", "silly description", htmlTable, footerHtml, out var email));
			AssertNull(email);
		}

		public void TestGenerateEmailWithCensusWarning()
		{
			var msgCensusWarning = "(Census Warning) ";
			var generator = new HtmlResponseEmailGenerator();
			var htmlTable = new HtmlTableCreator(new string[] { "Column1", "Column2" }).ToHtml();
			var footerHtml = "<strong>Hello Everyone</strong>";
			var censusWarning = true;
			Assert(generator.TryGenerateEmail("Subject", "B0011123", "Test Type", htmlTable, footerHtml, false, out var email, GlbBranch.CurrentBranch, censusWarning, msgCensusWarning));
			AssertEquals("Subject", "Test Type Response (Census Warning) for B0011123", email.Subject);
			AssertContains("Body", "Test Type Response (Census Warning) for B0011123", email.Body);
		}

		public void TestGenerateEmail()
		{
			var generator = new HtmlResponseEmailGenerator();
			var htmlTable = new HtmlTableCreator(new string[] { "Column1", "Column2" }).ToHtml();
			var footerHtml = "<strong>Hello Everyone</strong>";
			Assert(generator.TryGenerateEmail("silly subject", "silly header", "silly description", htmlTable, footerHtml, out var email));
			AssertEquals("Subject", "silly subject", email.Subject);
			AssertContains("silly header", email.Body);
			AssertContains("silly description", email.Body);
			AssertContains(htmlTable, email.Body);
			AssertContains(footerHtml, email.Body);
		}

		public void TestGenerateEmailWithUri()
		{
			var generator = new HtmlResponseEmailGenerator();
			var htmlTable = new HtmlTableCreator(new string[] { "Column1", "Column2" }).ToHtml();
			var responseDescription = generator.ResponseDescription;
			var footerHtml = "<strong>Hello Everyone</strong>";
			Assert(generator.TryGenerateEmail("uristuff", "B0011123", "Test Type", htmlTable, footerHtml, false, out var email, GlbBranch.CurrentBranch));
			AssertEquals("Subject", "Test Type Response for B0011123", email.Subject);
			AssertContains(email.Subject, email.Body);
			AssertContains(responseDescription, email.Body);
			AssertContains("B0011123", email.Body);
			AssertContains(htmlTable, email.Body);
			AssertContains(footerHtml, email.Body);

			generator.ResponseDescription = "Hello People";
			Assert(generator.TryGenerateEmail("uriStuff", "B0011123", "Test Type", htmlTable, "", true, out email, GlbBranch.CurrentBranch));
			AssertEquals("Subject", "Test Type Response (Failure) for B0011123", email.Subject);
			AssertContains(email.Subject, email.Body);
			AssertNotContains(responseDescription, email.Body);
			AssertContains("Hello People", email.Body);
			AssertContains("B0011123", email.Body);
			AssertContains(htmlTable, email.Body);
			AssertNotContains(footerHtml, email.Body);
		}

		public void TestGenerateEmailSetsDisplayName()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "TestCompany";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			Factory.Save();

			using (new User.IsBatchProcessorOverride(EnvProxy.Instance.CurrentUser))
			{
				var generator = new HtmlResponseEmailGenerator();
				var htmlTable = new HtmlTableCreator(new string[] { "Column1", "Column2" }).ToHtml();

				Assert(generator.TryGenerateEmail("uristuff", "B0011123", "Test Type", htmlTable, "", false, out var email, branch));
				AssertEquals("From Display Name", "TestCompany", email.FromDisplayName);

				DataRegistry.Instance.RawRegistry.MailboxDisplayName.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "New Display Name");

				Assert(generator.TryGenerateEmail("uriStuff", "B0011123", "Test Type", htmlTable, "", false, out email, branch));
				AssertEquals("From Display Name", "New Display Name", email.FromDisplayName);
			}
		}

		public void TestGenerateEmailWithoutJob()
		{
			var generator = new HtmlResponseEmailGenerator();
			var htmlTable = new HtmlTableCreator(new string[] { "Column1", "Column2" }).ToHtml();
			var footerHtml = "<strong>Hello Everyone</strong>";
			Assert(generator.TryGenerateEmail("", "", "Test Type", htmlTable, footerHtml, false, out var email, GlbBranch.CurrentBranch));
			AssertEquals("Subject", "Test Type Response", email.Subject);
			AssertContains(email.Subject, email.Body);
		}
	}
}
