using System.Collections.Generic;
using System.Linq;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	static class MessageProcessorNotificationTestHelper
	{
		internal static void AssertEmail(string expectingSubject, string[] expectingBodyTexts, string[] expectingRecipients)
		{
			var emailDef = GetEmailDef(expectingSubject);
			string body = emailDef.Body;
			foreach (string expected in expectingBodyTexts)
			{
				Assertion.AssertContains("Email body should contain", expected, body);
			}
			Assertion.AssertContainsExactElementsInAnyOrder("Recipients", expectingRecipients, from RecipientDef x in emailDef.Recipients select x.Email);
		}

		internal static void AssertEmailNotContains(string expectingSubject, string[] expectingBodyTexts)
		{
			var emailDef = GetEmailDef(expectingSubject);
			string body = emailDef.Body;
			foreach (string expected in expectingBodyTexts)
			{
				Assertion.AssertNotContains("Email body should not contain", expected, body);
			}
		}

		static EmailDef GetEmailDef(string expectingSubject)
		{
			List<EmailDef> emailsCreated = Env.OutgoingCustomsMailManager.EmailsCreated;
			Assertion.Assert("No Emails Created", emailsCreated.Count > 0);
			return emailsCreated.Single((EmailDef x) => x.Subject.StartsWith(expectingSubject));
		}
	}
}
