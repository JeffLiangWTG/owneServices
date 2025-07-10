using System;
using System.Security.Authentication;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Business.Services;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;
namespace Enterprise.Dash.Business.Tests.Services
{
	public class DocumentParsingTokenManagerTest : TestCaseWithFactory
	{
		#region System Trust to Trust Authentication Token

		[TestDate]
		public void TestCachedTokenIsUpdatedWhenExpired()
		{
			var tokenManager = new DocumentParsingTokenManagerForTest();

			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tokenManager.GetClientCertificateInfo()))
			{
				tokenManager.ClearCachedToken();
				var token = tokenManager.GetSystemToSystemTrustToken();

				AssertNotNullOrEmpty("Token should not be null or empty", token);

				var sameToken = tokenManager.GetSystemToSystemTrustToken();
				AssertEquals("These two tokens should be same as the token is not expired", token, sameToken);

				TestDateAttribute.Date = ZDateTime.UtcNow.ToDateTime().AddMinutes(25);
				var anotherToken = tokenManager.GetSystemToSystemTrustToken();

				AssertNotEquals("The expiration time of Token is 10 minutes and it has expired.", token, anotherToken);
			}
		}

		public void TestCachedTokenIsUpdatedWhenCertificateRegistryIsUpdated()
		{
			var tokenManager = new DocumentParsingTokenManagerForTest();

			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tokenManager.GetClientCertificateInfo()))
			{
				tokenManager.ClearCachedToken();
				var token = tokenManager.GetSystemToSystemTrustToken();
				AssertNotNullOrEmpty("Token should not be null or empty", token);

				var sameToken = tokenManager.GetSystemToSystemTrustToken();
				AssertEquals("These two tokens should be same as the token is not expired", token, sameToken);

				using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo()))
				{
					var anotherToken = tokenManager.GetSystemToSystemTrustToken();
					AssertNotEquals("The expiration time of Token is 10 minutes and it has expired.", token, anotherToken);
				}
			}
		}

		public void TestGetSystemToSystemToken_ExceptionIsThrown_WhenCertificateIsEmpty()
		{
			var tokenManager = new DocumentParsingTokenManagerForTest();
			var systemToSystemTrustInfo = tokenManager.GetClientCertificateInfo();
			systemToSystemTrustInfo.Certificate = ZBlob.Empty;

			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemToSystemTrustInfo))
			{
				AssertExceptionThrown<AuthenticationException>(() => tokenManager.GetSystemToSystemTrustToken());
			}
		}

		public void TestGetSystemToSystemToken_ExceptionIsThrown_WhenPrivateKeyIsEmpty()
		{
			var tokenManager = new DocumentParsingTokenManagerForTest();
			var systemToSystemTrustInfo = tokenManager.GetClientCertificateInfo();
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			systemToSystemTrustInfo.PrivateKey = string.Empty;
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented

			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemToSystemTrustInfo))
			{
				AssertExceptionThrown<AuthenticationException>(() => tokenManager.GetSystemToSystemTrustToken());
			}
		}

		public void TestGetSystemToSystemToken_ExceptionIsThrown_WhenClientIdIsEmpty()
		{
			var tokenManager = new DocumentParsingTokenManagerForTest();
			var systemToSystemTrustInfo = tokenManager.GetClientCertificateInfo();
			systemToSystemTrustInfo.ClientId = string.Empty;

			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemToSystemTrustInfo))
			{
				AssertExceptionThrown<AuthenticationException>(() => tokenManager.GetSystemToSystemTrustToken());
			}
		}

		public void TestGetSystemToSystemToken_GetClientAccessTokenFailed()
		{
			var tokenManager = new DocumentParsingTokenManagerForTest { ShouldThrowException = true };

			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tokenManager.GetClientCertificateInfo()))
			{
				AssertExceptionThrown<InvalidOperationException>(() => tokenManager.GetSystemToSystemTrustToken());
			}
		}

		#endregion

		#region EDocs Authentication Token

		public void TestTryEncryptEDocsAuthToken()
		{
			var encryptedToken = "k_Si0Ufa9oGNIyzS2gns9xihym1mUgmrJt1b53osB953FNjcAvWDoWJH1OCyxNb6jWbwkUBY5L62yMotwIjbw5kopEiUduvOypQ9Vtsh3-erANrLj1jCcyZPh6fZF_Wwk2k0VEqJ8WHf5Via5J49yWPsm99zUOoV00AqrjX4GuzWUc13gcq8cTQKhtgjsnn_I6efcKUZ05djWaTJQw";
			var tokenManager = new DocumentParsingTokenManager();

			using (DocManagerRegistry.Instance.EDocsAuthTokenEncryptionKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EncryptionKey))
			{
				AssertEquals("Encrypted token should be encrypted successfully", expected: true, tokenManager.TryDecryptEDocsAuthToken(encryptedToken, out var result));
				AssertEquals("EDIMessage PK should be same", new Guid("804AD29F-5077-415A-8F34-58A6D4F24C8A"), result.EDIMessagePK);
				AssertEquals("Doc type should be same", "CIV", result.DocType);
				AssertEquals("Data type should be same", "PDF", result.DataType);
				AssertEquals("Last datetime editing eDoc binary data should be same", new DateTime(2023, 11, 27), result.EDocLastEditTime);
			}
		}

		public void TestTryEncryptEDocsAuthToken_WithInvalidToken()
		{
			var invalidEncryptedToken = "h98H1bmxDuGgAkraqyNo4r2S2Gt9xyWvcKl7zS/kxYeWhw9v6qW/jJsii4mpqkINuTguP0TRP8Kmed/WpBGMbFP8/JM5GKUKp1nKf1RRlcwaXArgLUQ08XAiojZYOgu4auWfm+ydy3NxWXjmc/tWyjGvWqo";
			var tokenManager = new DocumentParsingTokenManager();
			var result = true;

			AssertNoExceptionThrown("No exception should be thrown out when decrypting an invalid token", () =>
			{
				result = tokenManager.TryDecryptEDocsAuthToken(invalidEncryptedToken, out _);
			});

			AssertEquals("Token should not be encrypted", expected: false, result);
		}

		#endregion

		#region Implementation

		const string EncryptionKey = "EoQNIn4lnxcKPmSH2RPACA==";

		#endregion
	}
}
