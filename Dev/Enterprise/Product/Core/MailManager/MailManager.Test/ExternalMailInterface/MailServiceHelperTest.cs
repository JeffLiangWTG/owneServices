using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.MailManager.Integration;
using MailKit;
using MailKit.Security;
using Microsoft.Identity.Client;
using Moq;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class MailServiceHelperTest : TransactionedTestCase
	{
		public void TestFailedToAuthenticateExceptionThrownWhenErrorAcquiringToken()
		{
			var mailService = GetMockMailService();
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Throws(new MsalUiRequiredException("test", "No account or login hint was passed to the AcquireTokenSilent call. "));

			using (ObjectFactory.Substitute(helper.Object))
			{
				var serviceHelper = GetMailServiceHelperForTest(mailService);
				AssertExceptionThrown<FailedToAuthenticateException>(() => serviceHelper.Open());
			}
		}

		public void TestUseOAuth2()
		{
			var acquireTokenCalled = false;
			var oAuthAuthenticateCalled = false;
			var mailService = GetMockMailService(() => oAuthAuthenticateCalled = true);
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(new Func<CancellationToken, Task<AuthenticationResult>>(c =>
			{
				acquireTokenCalled = true;
				return Task.FromResult(TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult("Id", "E573B2C9-0B1C-4F1B-ADCD-B3DC3301E3F1"));
			}));

			using (ObjectFactory.Substitute(helper.Object))
			{
				var serviceHelper = GetMailServiceHelperForTest(mailService, useOAuth2: false);
				serviceHelper.Open();
				Assert(!acquireTokenCalled);
				Assert(!oAuthAuthenticateCalled);

				serviceHelper = GetMailServiceHelperForTest(mailService);
				serviceHelper.Open();
				Assert(acquireTokenCalled);
				Assert(oAuthAuthenticateCalled);
			}
		}

		MailService GetMockMailService(Action authenticateAction = null)
		{
			var mailService = new Mock<MailService>();
			mailService.Setup(m => m.Timeout).Returns(3000);
			mailService.Setup(m => m.IsConnected).Returns(true);
			mailService.Setup(m => m.IsAuthenticated).Returns(false);
			mailService.Setup(m => m.Authenticate(It.Is<SaslMechanism>(s => s.Credentials.UserName == "Id" + "E573B2C9-0B1C-4F1B-ADCD-B3DC3301E3F1"), It.IsAny<CancellationToken>())).Callback<SaslMechanism, CancellationToken>(
				(x, y) =>
				{
					authenticateAction?.Invoke();
				});
			mailService.Object.Authenticate("user", "password", default);

			return mailService.Object;
		}

		MailServiceHelper GetMailServiceHelperForTest(MailService mailService, bool useOAuth2 = true)
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration("server", 1, SecureConnectionTypes.None);

			if (useOAuth2)
			{
				var tenantId = "8D4C9F2A-10DB-41DA-A7BA-9698B8539C24";
				var applicationId = "E0A69CDC-391E-4D03-8CD7-375E40A30D83";

				var oAuth2Configuration = new Ms365OAuth2Configuration(
					tenantId: tenantId,
					applicationId: applicationId,
					permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
					cachedToken: Array.Empty<byte>(),
					identifier: "Id");

				return new MailServiceHelper(mailService, mailServerConfiguration, oAuth2Configuration);
			}
			else
			{
				var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration("user", "password");
				return new MailServiceHelper(mailService, mailServerConfiguration, userPasswordAuthConfiguration);
			}
		}
	}
}
