using System;
using Enterprise.Environment;
using Enterprise.MailManager.Integration;
using Microsoft.Identity.Client;
using Moq;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class TestMs365OAuth2AuthenticationHelper
	{
		public static AuthenticationResult GetAuthenticationResult(string identifier, EmailType emailType)
		{
			var account = new Mock<IAccount>();
			account.Setup(a => a.Username).Returns(identifier + (emailType == EmailType.Incoming ? Env.Instance.Registry.Ms365ApplicationIdForIncoming : Env.Instance.Registry.Ms365ApplicationIdForOutgoing));
			account.Setup(a => a.HomeAccountId).Returns(new AccountId(identifier, "830DC300-BE69-4769-88DC-F4A7BD0C7751", "D226563D-77C0-4082-8467-32FC7CBD5E46"));
			return new AuthenticationResult("accessToken", false, "uniqueId", DateTimeOffset.Now,
				DateTimeOffset.Now, "tenantId", account.Object, "idToken", new[] { "blah" }, Guid.Empty, null, "blah");
		}

		public static AuthenticationResult GetAuthenticationResult(string identifier, string applicationId)
		{
			var account = new Mock<IAccount>();
			account.Setup(a => a.Username).Returns(identifier + applicationId);
			account.Setup(a => a.HomeAccountId).Returns(new AccountId(identifier, "830DC300-BE69-4769-88DC-F4A7BD0C7751", "D226563D-77C0-4082-8467-32FC7CBD5E46"));
			return new AuthenticationResult("accessToken", false, "uniqueId", DateTimeOffset.Now,
				DateTimeOffset.Now, "tenantId", account.Object, "idToken", new[] { "blah" }, Guid.Empty, null, "blah");
		}

		public static AuthenticationResult GetAuthenticationResult()
		{
			return new AuthenticationResult("accessToken", false, null, DateTimeOffset.Now,
			DateTimeOffset.Now, null, null, null, new[] { "https://graph.microsoft.com/.default" }, Guid.Empty, null, "Bearer");
		}
	}
}
