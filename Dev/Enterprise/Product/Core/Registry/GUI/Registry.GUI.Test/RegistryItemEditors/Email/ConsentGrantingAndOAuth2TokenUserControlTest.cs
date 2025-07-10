using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Identity.Client;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class ConsentGrantingAndOAuth2TokenUserControlTest : TestCase
	{
		public void TestSetCachedToken()
		{
			using (var control = new ConsentGrantingAndOAuth2TokenUserControl(EmailType.Incoming, null, null, null))
			{
				control.Token.User = "test@email.com";
				control.Token.Identifier = "706BA020-F38A-4232-B7FF-20714B8ABEB1";
				control.SetCachedToken(null);

				AssertNullOrEmpty(control.Token.User);
				AssertNullOrEmpty(control.Token.Identifier);
				AssertNull(control.Token.Token);
			}
		}

		public void TestMsalClientException()
		{
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			using (var control = new ConsentGrantingAndOAuth2TokenUserControl(EmailType.Incoming, null, null, null))
			{
				control.Token.User = "test@email.com";
				control.Token.Identifier = "706BA020-F38A-4232-B7FF-20714B8ABEB1";
				helper
					.Setup(m => m.AcquireTokenAsync(It.IsAny<CancellationToken>()))
					.Throws(new MsalClientException("invalid_client_id", "Invalid Client Id."));

				using (ObjectFactory.Substitute(helper.Object))
				{
					control.btnGrant_Click(null, null);
					var error = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals(@"Error when granting permissions.
Error Code: invalid_client_id
Error Message: Invalid Client Id.", error);
					AssertNullOrEmpty(control.Token.User);
					AssertNullOrEmpty(control.Token.Identifier);
					AssertNull(control.Token.Token);
				}
			}
		}

		public void TestMsalServiceException()
		{
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			using (var control = new ConsentGrantingAndOAuth2TokenUserControl(EmailType.Incoming, null, null, null))
			{
				control.Token.User = "test@email.com";
				control.Token.Identifier = "706BA020-F38A-4232-B7FF-20714B8ABEB1";
				helper
					.Setup(m => m.AcquireTokenAsync(It.IsAny<CancellationToken>()))
					.Throws(new MsalServiceException("invalid_request", "Invalid Tenant Id."));

				using (ObjectFactory.Substitute(helper.Object))
				{
					control.btnGrant_Click(null, null);
					var error = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals(@"Error when granting permissions.
Error Code: invalid_request
Error Message: Invalid Tenant Id.", error);
					AssertNullOrEmpty(control.Token.User);
					AssertNullOrEmpty(control.Token.Identifier);
					AssertNull(control.Token.Token);
				}
			}
		}

		public void TestGranted()
		{
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			using (var control = new ConsentGrantingAndOAuth2TokenUserControl(EmailType.Incoming, null, null, null))
			{
				helper
					.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>()))
					.Callback(() =>
					{
						control.SetCachedToken(new byte[] { 11, 22 });
					})
					.Returns(Task.FromResult(AuthenticationResult));

				using (ObjectFactory.Substitute(helper.Object))
				{
					control.btnGrant_Click(null, null);
					AssertEquals("Granted, User: justin.chen@email.com", control.lblMessage.Text);
					AssertEquals(Identifier, control.Token.Identifier);
				}
			}
		}

		public void TestNotGranted()
		{
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			using (var control = new ConsentGrantingAndOAuth2TokenUserControl(EmailType.Incoming, null, null, null))
			{
				control.Token.Identifier = Identifier;
				helper
					.Setup(m => m.AcquireTokenAsync(It.IsAny<CancellationToken>()))
					.Returns(Task.FromResult((AuthenticationResult)null));

				using (ObjectFactory.Substitute(helper.Object))
				{
					control.btnGrant_Click(null, null);
					AssertEquals("Not Granted", control.lblMessage.Text);
				}
			}
		}

		public void TestClear()
		{
			using (var control = new ConsentGrantingAndOAuth2TokenUserControl(EmailType.Incoming, null, null, null))
			{
				control.SetCachedToken(new byte[] { 11, 22 });
				AssertEquals("Granted", control.lblMessage.Text);
				AssertNotNull(control.CachedTokenAsBinary);

				control.btnClear_Click(null, null);
				AssertEquals("Not Granted", control.lblMessage.Text);
				AssertNull(control.CachedTokenAsBinary);
			}
		}

		public void TestAuthenticationHelper()
		{
			var ms365OAuth2TenantId = new StringRegistryItem("ms365OAuth2TenantId", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, Guid.NewGuid().ToString());
			var ms365ApplicationIdForIncoming = new StringRegistryItem("ms365ApplicationIdForIncoming", null, null, null, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, Guid.NewGuid().ToString());
			var useGraphApiForIncoming = new BooleanRegistryItem("useGraphApiForIncoming", null, null, null, RegistryStorageFlags.All, true);

			using (var control = new ConsentGrantingAndOAuth2TokenUserControl(EmailType.Incoming, ms365OAuth2TenantId, ms365ApplicationIdForIncoming, useGraphApiForIncoming))
			{
				var oAuth2Configuration = GetValue(control.AuthenticationHelper, "oAuth2Configuration") as Ms365OAuth2Configuration;
				AssertEquals(ms365OAuth2TenantId.Value, oAuth2Configuration.TenantId);
				AssertEquals(ms365ApplicationIdForIncoming.Value, oAuth2Configuration.ApplicationId);
				AssertEquals(Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_GraphAPI, oAuth2Configuration.PermissionType);
			}
		}

		static object GetValue(IMs365OAuth2AuthenticationHelper authenticationHelper, string fieldName)
		{
			var field = typeof(Ms365OAuth2AuthenticationHelper).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
			var value = field.GetValue(authenticationHelper);

			return value;
		}

		const string Identifier = "248CC5AF-8DB8-4DC8-8B7D-0C776067C33D";

		AuthenticationResult AuthenticationResult
		{
			get
			{
				var account = new Mock<IAccount>();
				account.Setup(m => m.Username).Returns("justin.chen@email.com");
				account.Setup(m => m.HomeAccountId).Returns(new AccountId(Identifier, "830DC300-BE69-4769-88DC-F4A7BD0C7751", "D226563D-77C0-4082-8467-32FC7CBD5E46"));
				return new AuthenticationResult("accessToken", false, "uniqueId", DateTimeOffset.Now,
					DateTimeOffset.Now, "tenantId", account.Object, "idToken", new[] { "sdf" }, Guid.Empty, null, "asdf");
			}
		}
	}
}
