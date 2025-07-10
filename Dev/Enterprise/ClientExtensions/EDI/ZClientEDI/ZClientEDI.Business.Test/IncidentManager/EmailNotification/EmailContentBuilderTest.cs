using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.ZArchitecture;

namespace ZClientEDI.Business.Test.IncidentManager.EmailNotification
{
	public abstract class EmailContentBuilderTest<TBuilder> : TestCaseWithFactory where TBuilder : IEDIEmailTemplateBuilder
	{
		protected abstract TBuilder GetEmailTemplateBuilder();

		TBuilder EmailTemplateBuilder
		{
			get
			{
				if (emailTemplateBuilder == null)
				{
					emailTemplateBuilder = GetEmailTemplateBuilder();
				}

				return emailTemplateBuilder;
			}
		}
		TBuilder emailTemplateBuilder;

		protected abstract string ExpectedEmailBody { get; }

		protected abstract string ExpectedEmailSubject { get; }

		void AssertGetTemplateFromBuilderNotNull(string message, TBuilder builder)
		{
			AssertNotNull($"Cannot get template from builder:{message}", builder.GetIEDIEmailTemplate());
		}

		public void TestEmailBody()
		{
			var builder = EmailTemplateBuilder;
			AssertGetTemplateFromBuilderNotNull(string.Empty, builder);
			var body = builder.BuildBody();

			AssertEquals("The actual content does not match the expectation", ExpectedEmailBody, body);
		}

		public void TestEmailSubject()
		{
			var builder = EmailTemplateBuilder;
			AssertGetTemplateFromBuilderNotNull(string.Empty, builder);
			var subject = builder.BuildSubject();

			AssertEquals("The actual content does not match the expectation", ExpectedEmailSubject, subject);
		}

		public void TestTemplateCodeAndDescription()
		{
			var builder = EmailTemplateBuilder;
			AssertGetTemplateFromBuilderNotNull(string.Empty, builder);
			var template = builder.GetIEDIEmailTemplate();
			AssertNotNull("Cannot find template from builder", template);

			AssertNotNullOrEmpty("Template code should not be empty", template.TemplateCode);
			AssertNotNullOrEmpty("Template description should not be empty", template.TemplateDescription);
		}

		public void TestGenerateEmail()
		{
			var builder = EmailTemplateBuilder;
			AssertGetTemplateFromBuilderNotNull(string.Empty, builder);

			var email = EmailBuilder.BuildEmailDefByTemplate(builder);
			var htmlEmail = EmailBuilder.BuildHtmlEmailDefByTemplate(builder);
			CombineAssertions(() =>
			{
				AssertNotNull("Email should be generated", email);
				AssertNotNull("Email should be generated", htmlEmail);

				using (PreventGeneration(builder))
				{
					email = EmailBuilder.BuildEmailDefByTemplate(builder);
					htmlEmail = EmailBuilder.BuildHtmlEmailDefByTemplate(builder);
					AssertNull("Email should not be generated", email);
					AssertNull("Email should not be generated", htmlEmail);
				}
			});
		}

		readonly MockEDIEmailTriggeringRules rules = new MockEDIEmailTriggeringRules();
		protected EDIEmailBuilder EmailBuilder
		{
			get
			{
				if (emailBuilder == null)
				{
					emailBuilder = EDIEmailBuilder.GetInstance(rules);
				}

				return emailBuilder;
			}
		}
		EDIEmailBuilder emailBuilder;

		protected void ClearEmailCodeBlackList()
		{
			rules.BlackList.Clear();
		}

		protected BlockedEmail PreventGeneration(IEDIEmailTemplateBuilder templateBuilder)
		{
			var code = templateBuilder.GetIEDIEmailTemplate().TemplateCode;
			rules.BlackList.Add(code);
			return new BlockedEmail(rules, code);
		}

		protected void AllowGeneration(string code)
		{
			rules.BlackList.Remove(code);
		}

		protected bool Allow(string code)
		{
			return !rules.BlackList.Contains(code);
		}
	}

	public class MockEDIEmailTriggeringRules : IEDIEmailTriggeringRules
	{
		public EnterpriseBusinessObject DataSource => null;

		public bool Allow(IEDIEmailTemplateBuilder emailTemplateBuilder)
		{
			return !BlackList.Contains(emailTemplateBuilder.GetIEDIEmailTemplate().TemplateCode);
		}

		public HashSet<string> BlackList = new HashSet<string>();
	}

	public class BlockedEmail : IDisposable
	{
		public BlockedEmail(MockEDIEmailTriggeringRules rules, string blockedCode)
		{
			this.code = blockedCode;
			this.rules = rules;
		}
		readonly string code;
		readonly MockEDIEmailTriggeringRules rules;

		public void Dispose()
		{
			rules.BlackList.Remove(code);
		}
	}
}
