using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class GmailOAuth2AuthenticationHelperTest : TransactionedTestCase
	{
		readonly IGmailAuthenticationResult byRefreshTokenResult = new GmailAuthenticationResult("access_token", "email");

		public void TestAcquireTokenSilentlyAsync()
		{
			var mockServer = new Mock<IAcquireGmailTokenInteractiveServer>();
			mockServer.Setup(s => s.AcquireByServiceAccountAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(byRefreshTokenResult));

			using (ObjectFactory.Substitute(mockServer.Object))
			{
				var configuration = new GmailOAuth2Configuration(null, null);
				var helper = new GmailOAuth2AuthenticationHelper(configuration);
				var result = helper.AcquireTokenSilentlyAsync();
				AssertEquals(byRefreshTokenResult, result.Result);
			}
		}
	}
}
