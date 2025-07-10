using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Registry.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class RefreshTokenGeneratorTest : TestCaseWithFactory
	{
		#region Access Token
		public void TestGenerateToken()
		{
			SetupTrustedResourceSystem();
			var claims = new List<KeyValuePair<string, string>>()
			{ new KeyValuePair<string, string>("Shipment ID", "S03243280") };
			var (accessToken, refreshToken) = EdiAccessTokenGenerator.CreateTokens(Guid.NewGuid(), "DUM", ResourceProduct, ResourceSystemId, claims, DateTime.Now.AddMinutes(15), createRefreshToken: false, isEncryptionRequired: true);
			AssertNotEquals(string.Empty, accessToken);
			AssertEquals(string.Empty, refreshToken);
		}

		public void TestGenerateToken_NoEncryption()
		{
			SetupTrustedResourceSystem();
			var (accessToken, refreshToken) = EdiAccessTokenGenerator.CreateTokens(Guid.NewGuid(), "DUM", ResourceProduct, ResourceSystemId, null, DateTime.Now.AddMinutes(15), createRefreshToken: false, isEncryptionRequired: false);
			AssertNotEquals(string.Empty, accessToken);
			AssertEquals(string.Empty, refreshToken);
		}

		public void TestGenerateToken_CustomisedExpiry()
		{
			SetupTrustedResourceSystem();
			var (accessToken, refreshToken) = EdiAccessTokenGenerator.CreateTokens(Guid.NewGuid(), "DUM", ResourceProduct, ResourceSystemId, null, DateTime.Now.AddHours(1), createRefreshToken: false, isEncryptionRequired: false);
			AssertNotEquals(string.Empty, accessToken);
			AssertEquals(string.Empty, refreshToken);
		}

		public void TestGenerateToken_WithRefreshToken()
		{
			SetupTrustedResourceSystem();
			var (accessToken, refreshToken) = EdiAccessTokenGenerator.CreateTokens(Guid.NewGuid(), "DUM", ResourceProduct, ResourceSystemId, null, DateTime.Now.AddMinutes(15), createRefreshToken: true, isEncryptionRequired: false);
			AssertNotEquals(string.Empty, accessToken);
			AssertNotEquals(string.Empty, refreshToken);
		}

		void SetupTrustedResourceSystem()
		{
			var regCollection = new CodeDescriptionBoolCollection { ResourceProduct };
			EDIDataRegistry.Instance.MyAccountTrustedServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regCollection);
			var clientSystemConfig = Factory.New<EdiTrustedMessagingConfig>();
			clientSystemConfig.ETM_Product = ResourceProduct;
			clientSystemConfig.ETM_CertificateType = "PDC";
			clientSystemConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Client.cer");
			var centralSystemConfig = Factory.New<EdiTrustedMessagingConfig>();
			centralSystemConfig.ETM_Product = ResourceProduct;
			centralSystemConfig.ETM_CertificateType = "CSC";
			centralSystemConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Server.pfx");
			Factory.Save();
		}

		const string ResourceProduct = "SMF";
		const string ResourceSystemId = "";
		#endregion
		#region Refresh Token
		public void TestCreateRefreshToken()
		{
			var token = EdiAccessTokenGenerator.CreateRefreshToken(Guid.NewGuid(), "DUM", "DDD", "DDD-PRD", "{'is_admin':'true'}", DateTime.Now.AddMinutes(5));
			AssertNotEquals(string.Empty, token);
		}

		public void TestConsumeRefreshToken()
		{
			var ownerId = Guid.NewGuid();
			var token = EdiAccessTokenGenerator.CreateRefreshToken(ownerId, "DUM", "DDD", "DDD-PRD", "{'is_admin':'true'}", DateTime.Now.AddMinutes(5));
			var result = EdiAccessTokenGenerator.ConsumeRefreshToken(token, out var tokenInfo);
			AssertEquals(true, result);
			AssertEquals(ownerId, tokenInfo.OwnerId);
			AssertEquals("DUM", tokenInfo.OwnerTableCode);
			AssertEquals("DDD", tokenInfo.ResourceProduct);
			AssertEquals("DDD-PRD", tokenInfo.ResourceSystemId);
			AssertEquals("{'is_admin':'true'}", tokenInfo.Scope);
		}
		#endregion
	}
}