using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Identity.Client;
using Moq;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class GraphMailDownloaderTest : TransactionedTestCase
	{
		public void TestDownloadFromServerWhenAuthenticationFailed()
		{
			var log = new StringBuilder();

			var helper1 = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper1.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Throws(new MsalUiRequiredException("test", "No account or login hint was passed to the AcquireTokenSilent call. "));

			using (Env.Registry.RawRegistry.UseOAuth2ForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365))
			using (Env.Registry.RawRegistry.UseGraphApiForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (Env.Registry.RawRegistry.Ms365OAuth2TokenForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Ms365OAuth2Token { Identifier = "Id", Token = new byte[] { 1, 2 }, User = "email@test.com" }))
				using (ObjectFactory.Substitute(helper1.Object))
				{
					AssertEquals("Pre-Condition", true, ((Ms365OAuth2Configuration)MailServerConfiguration.Default.GetOAuth2Configuration()).IsUserToken);

					using var mailDownloader = new GraphMailDownloader();
					mailDownloader.LogMessage += (type, message) => log.Append($"Type: {type}, Message: {message}");
					mailDownloader.DownloadFromServer();
					AssertContains("Type: Error, Message: Error downloading email from mail server - No account or login hint was passed to the AcquireTokenSilent call.", log.ToString());
				}

				var helper2 = new Mock<IMs365OAuth2AuthenticationHelper>();
				helper2.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Throws(new MsalServiceException("test", "TenantID / ApplicationID / AppSecret is incorrect. "));
				using (Env.Registry.RawRegistry.Ms365OAuth2AppTokenForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new byte[] { 1, 2 }))
				using (ObjectFactory.Substitute(helper2.Object))
				{
					AssertEquals("Pre-Condition", false, ((Ms365OAuth2Configuration)MailServerConfiguration.Default.GetOAuth2Configuration()).IsUserToken);

					using var mailDownloader = new GraphMailDownloader();
					mailDownloader.LogMessage += (type, message) => log.Append($"Type: {type}, Message: {message}");
					mailDownloader.DownloadFromServer();
					AssertContains("Type: Error, Message: Error downloading email from mail server - TenantID / ApplicationID / AppSecret is incorrect.", log.ToString());
				}
			}
		}

		public void TestDownloadFromServer()
		{
			var log = new StringBuilder();
			var authResult = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult("Id", EmailType.Incoming);
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(authResult));

			using (Env.Registry.RawRegistry.UseOAuth2ForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365))
			using (Env.Registry.RawRegistry.UseGraphApiForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (Env.Registry.RawRegistry.Ms365OAuth2TokenForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Ms365OAuth2Token { Identifier = "Id", Token = [1, 2], User = "email@test.com" }))
				using (ObjectFactory.Substitute(helper.Object))
				using (Globals.TemporaryOverrideForIsTest(true))
				{
					AssertEquals("Pre-Condition", true, ((Ms365OAuth2Configuration)MailServerConfiguration.Default.GetOAuth2Configuration()).IsUserToken);

					using var mailDownloader = new GraphMailDownloader();
					mailDownloader.UserRequestBuilderForTest = new GraphServiceClientForTest(authResult.GetAuthenticationProvider()).GetUserRequestBuilder();
					mailDownloader.LogMessage += (type, message) => log.Append($"Type: {type}, Message: {message}.");
					mailDownloader.DownloadFromServer();
					AssertEquals("Type: Information, Message: Downloading 1 email with Graph API.Type: Information, Message: 1 email downloaded in this batch.Type: Information, Message: Downloading 1 email with Graph API.Type: Information, Message: 1 email downloaded in this batch.", log.ToString());
				}

				var authResult2 = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult();
				var helper2 = new Mock<IMs365OAuth2AuthenticationHelper>();
				helper2.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(authResult2));
				using (Env.Registry.RawRegistry.Ms365OAuth2AppTokenForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new byte[] { 1, 2 }))
				using (ObjectFactory.Substitute(helper2.Object))
				using (Globals.TemporaryOverrideForIsTest(true))
				{
					AssertEquals("Pre-Condition", false, ((Ms365OAuth2Configuration)MailServerConfiguration.Default.GetOAuth2Configuration()).IsUserToken);

					using (RawDataRegistry.Instance.MailboxEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@email.com"))
					{
						log = new StringBuilder();
						using var mailDownloader = new GraphMailDownloader();
						mailDownloader.UserRequestBuilderForTest = new GraphServiceClientForTest(authResult2.GetAuthenticationProvider()).GetUserRequestBuilder();
						mailDownloader.LogMessage += (type, message) => log.Append($"Type: {type}, Message: {message}.");
						mailDownloader.DownloadFromServer();
						AssertEquals("Type: Information, Message: Downloading 1 email with Graph API.Type: Information, Message: 1 email downloaded in this batch.Type: Information, Message: Downloading 1 email with Graph API.Type: Information, Message: 1 email downloaded in this batch.", log.ToString());
					}
				}
			}
		}

		[DeveloperOnlyTest]
		public void TestDownloadFromServerWithNonExistentUser()
		{
			var authResult = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult();
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(authResult));
			using (Env.Registry.RawRegistry.UseOAuth2ForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365))
			using (Env.Registry.RawRegistry.UseGraphApiForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.Registry.RawRegistry.Ms365OAuth2AppTokenForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new byte[] { 1, 2 }))
			using (ObjectFactory.Substitute(helper.Object))
			using (Globals.TemporaryOverrideForIsTest(true))
			{
				AssertEquals("Pre-Condition", false, ((Ms365OAuth2Configuration)MailServerConfiguration.Default.GetOAuth2Configuration()).IsUserToken);

				using (RawDataRegistry.Instance.MailboxEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@email.com"))
				{
					var log = new StringBuilder();
					using var mailDownloader = new GraphMailDownloader();
					mailDownloader.UserRequestBuilderForTest = new GraphServiceClientForTest(authResult.GetAuthenticationProvider()).GetUserRequestBuilder();
					mailDownloader.LogMessage += (type, message) => log.AppendLine($"Type: {type}, Message: {message}");
					mailDownloader.DownloadFromServer();
					AssertEquals(
						@"Type: Information, Message: Downloading 1 email with Graph API
Type: Information, Message: 1 email downloaded in this batch
Type: Information, Message: Downloading 1 email with Graph API
Type: Information, Message: 1 email downloaded in this batch
", log.ToString());
				}

				using (RawDataRegistry.Instance.MailboxEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test2@email.com"))
				{
					var log = new StringBuilder();
					using var mailDownloader = new GraphMailDownloader();
					mailDownloader.UserRequestBuilderForTest = new GraphServiceClientForTest(authResult.GetAuthenticationProvider()).GetUserRequestBuilder();
					mailDownloader.LogMessage += (type, message) => log.Append($"Type: {type}, Message: {message}");
					mailDownloader.DownloadFromServer();
					AssertContains(@"Type: Error, Message: Error downloading email from mail server - Code: InvalidAuthenticationToken
Message: CompactToken parsing failed with error code: 80049217", log.ToString());
				}
			}
		}

		[DeveloperOnlyTest]
		public void TestDownloadRequestsLog()
		{
			var authResult = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult();
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(authResult));
			using (Env.Registry.RawRegistry.UseOAuth2ForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365))
			using (Env.Registry.RawRegistry.UseGraphApiForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.Registry.RawRegistry.Ms365OAuth2AppTokenForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new byte[] { 1, 2 }))
			using (ObjectFactory.Substitute(helper.Object))
			using (Globals.TemporaryOverrideForIsTest(true))
			{
				AssertEquals("Pre-Condition", false, ((Ms365OAuth2Configuration)MailServerConfiguration.Default.GetOAuth2Configuration()).IsUserToken);

				using (RawDataRegistry.Instance.MailboxEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test2@email.com"))
				{
					var log = new StringBuilder();
					using var mailDownloader = new GraphMailDownloader();
					mailDownloader.LogMessage += (type, message) => log.AppendLine($"Type: {type}, Message: {message}");
					mailDownloader.DownloadFromServer();

					AssertContains("Type: Verbose, Message: Sending Graph request", log.ToString());
					AssertContains("Type: Verbose, Message: Received Graph response", log.ToString());
					AssertContains(@"Type: Error, Message: Error downloading email from mail server - Code: InvalidAuthenticationToken
Message: CompactToken parsing failed with error code: 80049217", log.ToString());
				}
			}
		}

		public void TestDeleteMessage()
		{
			var log = new StringBuilder();
			var authResult = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult("Id", EmailType.Incoming);
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(
				Task.FromResult(authResult));

			using (Env.Registry.RawRegistry.UseOAuth2ForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365))
			using (Env.Registry.RawRegistry.UseGraphApiForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.Registry.RawRegistry.Ms365OAuth2TokenForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Ms365OAuth2Token { Identifier = "Id", Token = new byte[] { 1, 2 }, User = "email@test.com" }))
			using (ObjectFactory.Substitute(helper.Object))
			using (Globals.TemporaryOverrideForIsTest(true))
			{
				AssertEquals("Pre-Condition", true, ((Ms365OAuth2Configuration)MailServerConfiguration.Default.GetOAuth2Configuration()).IsUserToken);

				using var mailDownloader = new GraphMailDownloader();
				mailDownloader.UserRequestBuilderForTest = new GraphServiceClientForTest(authResult.GetAuthenticationProvider()).GetUserRequestBuilder();
				mailDownloader.LogMessage += (type, message) => log.Append($"Type: {type}, Message: {message}");
				mailDownloader.DeleteMessage("cc36d7be-fe3d-4041-a836-d595f357da2b");
				AssertEquals("Type: Information, Message: Mail(cc36d7be-fe3d-4041-a836-d595f357da2b) deleted", log.ToString());
			}

			var authResult2 = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult();
			var helper2 = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper2.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(authResult2));

			using (Env.Registry.RawRegistry.UseOAuth2ForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365))
			using (Env.Registry.RawRegistry.UseGraphApiForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.Registry.RawRegistry.Ms365OAuth2AppTokenForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new byte[] { 1, 2 }))
			using (ObjectFactory.Substitute(helper2.Object))
			using (Globals.TemporaryOverrideForIsTest(true))
			{
				AssertEquals("Pre-Condition", false, ((Ms365OAuth2Configuration)MailServerConfiguration.Default.GetOAuth2Configuration()).IsUserToken);

				using (RawDataRegistry.Instance.MailboxEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@email.com"))
				{
					log = new StringBuilder();
					using var mailDownloader = new GraphMailDownloader();
					mailDownloader.UserRequestBuilderForTest = new GraphServiceClientForTest(authResult2.GetAuthenticationProvider()).GetUserRequestBuilder();
					mailDownloader.LogMessage += (type, message) => log.Append($"Type: {type}, Message: {message}");
					mailDownloader.DeleteMessage("cc36d7be-fe3d-4041-a836-d595f357da2b");
					AssertEquals("Type: Information, Message: Mail(cc36d7be-fe3d-4041-a836-d595f357da2b) deleted", log.ToString());
				}

				using (RawDataRegistry.Instance.MailboxEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test2@email.com"))
				{
					log = new StringBuilder();
					using var mailDownloader = new GraphMailDownloader();
					mailDownloader.UserRequestBuilderForTest = new GraphServiceClientForTest(authResult2.GetAuthenticationProvider()).GetUserRequestBuilder();
					mailDownloader.LogMessage += (type, message) => log.Append($"Type: {type}, Message: {message}");
					mailDownloader.DeleteMessage("cc36d7be-fe3d-4041-a836-d595f357da2b");
					AssertEquals("Type: Error, Message: 'MailboxEmailAddress' doesn't exist in AAD, please check it in registry.", log.ToString());
				}
			}
		}
	}
}
