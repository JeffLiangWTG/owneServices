using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.MailManager.ExternalMailInterface.IMAP;
using Enterprise.MailManager.ExternalMailInterface.POP3;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Identity.Client;
using Moq;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class MailProtocolFactoryTest : TransactionedTestCase
	{
		public void TestGetMailProtocol_MailKitPop3FieldsSet()
		{
			var ms365OAuth2Token = new Ms365OAuth2Token() { Identifier = "id", Token = Guid.NewGuid().ToByteArray(), User = "user1" };
			Action<byte[]> action = DummyAction;

			var mailServerConfiguration = new MailServerConnectionConfiguration("dummyServer", 110, SecureConnectionTypes.None);

			var oAuth2Configuration = new Ms365OAuth2Configuration(
				tenantId: TenantIdForTest,
				applicationId: ApplicationIdForTest,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: ms365OAuth2Token.Token,
				tokenSaveAction: DummyAction,
				identifier: ms365OAuth2Token.Identifier);

			var pop3 = (MailKitPop3)new MailProtocolFactory().GetMailProtocol(MailRetrievalProtocols.POP3,
				mailServerConfiguration, oAuth2Configuration, reportErrorAction: DummyReportErrorAction);
			var reportErrorAction = new Action<string, Exception, string>(DummyReportErrorAction);

			AssertEquals(oAuth2Configuration, GetValue(pop3.MailServiceHelper, "oAuth2Configuration"));
			AssertEquals(reportErrorAction, GetValue(pop3, "reportErrorAction"));

			void DummyAction(byte[] message)
			{
				return;
			}

			void DummyReportErrorAction(string uniqueId, Exception exception, string emailAction)
			{
				return;
			}
		}
		public void TestGetMailProtocol_MailKitImapFieldsSet()
		{
			var ms365OAuth2Token = new Ms365OAuth2Token() { Identifier = "id", Token = Guid.NewGuid().ToByteArray(), User = "user1" };
			Action<byte[]> action = DummyAction;

			var mailServerConfiguration = new MailServerConnectionConfiguration("dummyServer", 110, SecureConnectionTypes.None);

			var oAuth2Configuration = new Ms365OAuth2Configuration(
				tenantId: TenantIdForTest,
				applicationId: ApplicationIdForTest,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: ms365OAuth2Token.Token,
				tokenSaveAction: DummyAction,
				identifier: ms365OAuth2Token.Identifier);

			var imap = (MailKitImap)new MailProtocolFactory().GetMailProtocol(MailRetrievalProtocols.IMAP,
				mailServerConfiguration, oAuth2Configuration, reportErrorAction: DummyReportErrorAction);
			var reportErrorAction = new Action<string, Exception, string>(DummyReportErrorAction);

			AssertEquals(oAuth2Configuration, GetValue(imap.MailServiceHelper, "oAuth2Configuration"));
			AssertEquals(reportErrorAction, GetValue(imap, "reportErrorAction"));

			void DummyAction(byte[] message)
			{
				return;
			}

			void DummyReportErrorAction(string uniqueId, Exception exception, string emailAction)
			{
				return;
			}
		}

		static object GetValue(object o, string fieldName)
		{
			var field = o.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
			var value = field.GetValue(o);

			return value;
		}

		public void TestGetSaslMechanismOAuth2()
		{
			var oAuth2Configuration = new Ms365OAuth2Configuration(
				tenantId: TenantIdForTest,
				applicationId: ApplicationIdForTest,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: Array.Empty<byte>(),
				identifier: "Id");

			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(new Func<CancellationToken, Task<AuthenticationResult>>(c => Task.FromResult(TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult(oAuth2Configuration.Identifier, oAuth2Configuration.ApplicationId))));

			using (ObjectFactory.Substitute(helper.Object))
			{
				var result = oAuth2Configuration.GetSaslMechanism();
				AssertEquals("Id" + ApplicationIdForTest, result.Credentials.UserName);
			}
		}

		public void TestGetOAuth2AuthenticationResult()
		{
			var oAuth2Configuration = new Ms365OAuth2Configuration(
				tenantId: TenantIdForTest,
				applicationId: ApplicationIdForTest,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: Array.Empty<byte>(),
				identifier: "Id");

			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(new Func<CancellationToken, Task<AuthenticationResult>>(c => Task.FromResult(TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult(oAuth2Configuration.Identifier, oAuth2Configuration.ApplicationId))));

			using (ObjectFactory.Substitute(helper.Object))
			{
				var result = Ms365OAuth2AuthenticationHelper.GetOAuth2AuthenticationResult(oAuth2Configuration);
				AssertEquals("Id" + ApplicationIdForTest, result.Account.Username);
				AssertEquals("accessToken", result.AccessToken);
			}
		}

		const string TenantIdForTest = "8D4C9F2A-10DB-41DA-A7BA-9698B8539C24";
		const string ApplicationIdForTest = "E0A69CDC-391E-4D03-8CD7-375E40A30D83";
	}
}
