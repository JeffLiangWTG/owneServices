using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Environment;
using MailKit.Security;
using Moq;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class GmailOAuth2SaslMechanismStrategyTest : TransactionedTestCase
	{
		public void TestGetSaslMechanismOAuth2()
		{
			var mockResult = new Mock<IGmailAuthenticationResult>();
			mockResult.Setup(m => m.Email).Returns("userName");
			mockResult.Setup(m => m.AccessToken).Returns("accessToken");

			var helper = new Mock<IGmailOAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenSilentlyAsync(It.IsAny<CancellationToken>())).Returns(new Func<CancellationToken, Task<IGmailAuthenticationResult>>(c => Task.FromResult(mockResult.Object)));

			using (ObjectFactory.Substitute(helper.Object))
			{
				var oAuth2Configuration = new GmailOAuth2Configuration("Test Mail", new GmailOAuth2JsonFile());

				var expected = new SaslMechanismOAuth2("userName", "accessToken");
				var result = oAuth2Configuration.GetSaslMechanism();
				AssertEquals(expected.Credentials.UserName, result.Credentials.UserName);
			}
		}
	}
}
