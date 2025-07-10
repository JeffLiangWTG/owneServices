using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.MailManager.Integration;
using MailKit.Security;
using Microsoft.Identity.Client;
using Moq;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class Ms365OAuth2SaslMechanismStrategyTest : TransactionedTestCase
	{
		public void TestGetSaslMechanismOAuth2()
		{
			var account = new Mock<IAccount>();
			account.Setup(m => m.Username).Returns("userName");
			account.Setup(m => m.HomeAccountId).Returns(new AccountId("identifier", "830DC300-BE69-4769-88DC-F4A7BD0C7751", "D226563D-77C0-4082-8467-32FC7CBD5E46"));

			var authenticationResult = new AuthenticationResult("accessToken", false, null, DateTimeOffset.Now,
				DateTimeOffset.Now, null, account.Object, null, ["https://graph.microsoft.com/.default"], Guid.Empty, null, "Bearer");

			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(new Func<CancellationToken, Task<AuthenticationResult>>(c => Task.FromResult(authenticationResult)));

			using (ObjectFactory.Substitute(helper.Object))
			{
				var oAuth2Configuration = new Ms365OAuth2Configuration(
					tenantId: "19D6C9DE-4A85-4A42-A018-12F84FEF5BE3",
					applicationId: "90D4AFAA-75D8-47F1-A16C-07347C798C68",
					permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
					cachedToken: [],
					identifier: "Id");

				var expected = new SaslMechanismOAuth2("userName", "accessToken");
				var result = oAuth2Configuration.GetSaslMechanism();
				AssertEquals(expected.Credentials.UserName, result.Credentials.UserName);
			}
		}
	}
}
