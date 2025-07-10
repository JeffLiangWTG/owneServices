using System;
using Enterprise.MailManager.Integration;
using Microsoft.Identity.Client;
using Moq;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class Ms365OAuth2AuthenticationHelperTest : TransactionedTestCase
	{
		public void TestTenant()
		{
			var helper = GetAuthenticationHelperForTest(
				tenantId: TenantIdForTest,
				applicationId: ApplicationIdForTest,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: Array.Empty<byte>(),
				identifier: "Id");

			AssertEquals("Tenant should be the TenantId set in the configuration", TenantIdForTest, helper.Tenant);
		}

		public void TestTenant_Common()
		{
			var helper = GetAuthenticationHelperForTest(
				tenantId: string.Empty,
				applicationId: ApplicationIdForTest,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: Array.Empty<byte>(),
				identifier: "Id");

			AssertEquals("Tenant should be common", "common", helper.Tenant);
		}

		public void TestAppSecret()
		{
			var helper = GetAuthenticationHelperForTest(
				tenantId: TenantIdForTest,
				applicationId: ApplicationIdForTest,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.ApplicationPermission_GraphAPI,
				cachedToken: Array.Empty<byte>(),
				identifier: "Id",
				clientSecret: null);

			AssertAcquireTokenExceptionMessage(helper, "Value cannot be null.");
		}

		public void TestApplicationBehavesDifferentlyIfApplicationIdRegistryValueChanged()
		{
			var applicationIdInvalidMessage = "No account or login hint was passed to the AcquireTokenSilent call. ";
			var applicationIdNullMessage = "No ClientId was specified. ";

			var helper = GetAuthenticationHelperForTest(
				tenantId: TenantIdForTest,
				applicationId: ApplicationIdForTest,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: Array.Empty<byte>(),
				identifier: "Id");

			AssertAcquireTokenExceptionMessage(helper, applicationIdInvalidMessage);

			helper = GetAuthenticationHelperForTest(
				tenantId: TenantIdForTest,
				applicationId: ApplicationIdForTest,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: Array.Empty<byte>(),
				identifier: "Id");

			AssertAcquireTokenExceptionMessage(helper, applicationIdInvalidMessage);

			helper = GetAuthenticationHelperForTest(
				tenantId: TenantIdForTest,
				applicationId: null,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: Array.Empty<byte>(),
				identifier: "Id");

			AssertAcquireTokenExceptionMessage(helper, applicationIdNullMessage);

			helper = GetAuthenticationHelperForTest(
				tenantId: TenantIdForTest,
				applicationId: null,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: Array.Empty<byte>(),
				identifier: "Id");

			AssertAcquireTokenExceptionMessage(helper, applicationIdNullMessage);
		}

		void AssertAcquireTokenExceptionMessage(Ms365OAuth2AuthenticationHelper helper, string expectedMessage)
		{
			var helperException2 = AssertExceptionThrown<AggregateException>(() => _ = ((IMs365OAuth2AuthenticationHelper)helper).AcquireTokenAsync().Result);
			AssertContains(expectedMessage, helperException2.InnerException.Message);
		}

		public void TestCachedTokenIsUpdatedAfterAccessNotification()
		{
			byte[] tokenBytes1 = { 1 };
			byte[] tokenBytes2 = { 2 };

			var helper = GetAuthenticationHelperForTest(
				tenantId: TenantIdForTest,
				applicationId: ApplicationIdForTest,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: tokenBytes1,
				identifier: "Id",
				clientSecret: null);

			var privateFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

			var cachedTokenField = helper.GetType().GetField("cachedToken", privateFlags);
			AssertEquals("CachedToken should be set to the passed in byte array.", tokenBytes1, cachedTokenField.GetValue(helper));

			var mockTokenCacheSerializer = new Mock<ITokenCacheSerializer>();
			mockTokenCacheSerializer.Setup(s => s.SerializeMsalV3()).Returns(tokenBytes2);

			var tokenCacheNotificationArgs = new TokenCacheNotificationArgs(tokenCache: mockTokenCacheSerializer.Object,
																   clientId: default,
																   account: default,
																   hasStateChanged: true,
																   isApplicationCache: default,
																   suggestedCacheKey: default,
																   hasTokens: default,
																   suggestedCacheExpiry: default,
																   cancellationToken: default);

			var afterAccessNotificationMethod = helper.GetType().GetMethod("AfterAccessNotification", privateFlags);
			afterAccessNotificationMethod.Invoke(helper, new object[] { tokenCacheNotificationArgs });
			AssertEquals("CachedToken should be updated after access notification.", tokenBytes2, cachedTokenField.GetValue(helper));
		}

		Ms365OAuth2AuthenticationHelper GetAuthenticationHelperForTest(string tenantId,
							 string applicationId,
							 Ms365OAuth2Configuration.Ms365OAuth2PermissionType permissionType,
							 byte[] cachedToken,
							 Action<byte[]> tokenSaveAction = null,
							 string identifier = null,
							 string clientSecret = null,
							 bool shouldAcquireTokenInteractive = false)
		{
			return new Ms365OAuth2AuthenticationHelper(new Ms365OAuth2Configuration(tenantId, applicationId, permissionType, cachedToken, tokenSaveAction, identifier, clientSecret, shouldAcquireTokenInteractive));
		}

		const string TenantIdForTest = "E573B2C9-0B1C-4F1B-ADCD-B3DC3301E300";
		const string ApplicationIdForTest = "7A2C345D-B952-41B0-B7F2-6666A510639D";
	}
}
