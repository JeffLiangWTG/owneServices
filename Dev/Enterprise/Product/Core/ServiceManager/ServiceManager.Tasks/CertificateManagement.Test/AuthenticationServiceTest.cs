using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using CargoWise.Application;
using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.DataContracts;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;
using WTG.IdentitySecurity;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement.Test;

class AuthenticationServiceTest : TransactionedTestCase
{
	public void TestConstructor()
	{
		var authenticationService = CreateAuthenticationService();

		AssertNotNull(authenticationService);
	}

	public void TestGetAccessToken_NoRegistry_ThrowsException()
	{
		var authenticationService = CreateAuthenticationService();

		var e = AssertExceptionThrown<InvalidOperationException>(() => authenticationService.GetAccessToken());

		AssertEquals("SystemToSystemCertificate is not valid. Please ensure System to System Trust configuration has been setup and the TCM task has run successfully", e.Message);
	}

	public void TestGetAccessToken_WithRegistryNotSetup_ThrowsException()
	{
		var authenticationService = CreateAuthenticationService();
		var initialTrustInfo = new SystemToSystemTrustInfo
		{
			ClientId = Guid.NewGuid().ToString(),
			TenantId = Guid.NewGuid().ToString(),
			CertificateSigningRequest = "csr",
			OperationId = Guid.NewGuid().ToString(),
		};
		using var overrideRegistry = SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, initialTrustInfo);

		var e = AssertExceptionThrown<InvalidOperationException>(() => authenticationService.GetAccessToken());

		AssertEquals("SystemToSystemCertificate is not valid. Please ensure System to System Trust configuration has been setup and the TCM task has run successfully", e.Message);
	}

	public void TestGetAccessToken_WithRegistrySetup_CallsOAuthClientAssertion()
	{
		var authenticationService = CreateAuthenticationService();
		var initialTrustInfo = CreateValidTrustInfo();
		using var overrideRegistry = SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, initialTrustInfo);

		var e = AssertExceptionThrown<InvalidOperationException>(() => authenticationService.GetAccessToken());

		AssertContains("Failed to get access token. invalid_request.", e.Message);
		AssertContains($"Tenant '{initialTrustInfo.TenantId}' not found.", e.Message);
	}

	public void TestGetAccessTokenAsync_CallsTokenGeneratorService()
	{
		using var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
		var authenticationService = CreateAuthenticationService();
		var tokenGeneratorService = new Mock<ITokenGeneratorService>(MockBehavior.Strict);
		tokenGeneratorService
			.Setup(m => m.SignCwTokenAsync(It.IsAny<SignCwTokenRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new SignCwTokenResponse("dummy token"));
		var tokenServicesFactoryMock = new Mock<ITokenServicesFactory>(MockBehavior.Strict);
		tokenServicesFactoryMock.Setup(m => m.GetTokenGeneratorService()).Returns(() => tokenGeneratorService.Object);

		var tempClientId = Guid.NewGuid().ToString();
		using var substitute = ObjectFactory.Substitute(tokenServicesFactoryMock.Object);
		using var overrideRegistry = SystemDataRegistry.Instance.EDIClientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempClientId);

		var token = authenticationService.GetAccessTokenAsync(cancellationTokenSource.Token).Result;

		AssertEquals("dummy token", token);
		tokenServicesFactoryMock.Verify(m => m.GetTokenGeneratorService(), Times.Once);
		tokenGeneratorService.Verify(m => m.SignCwTokenAsync(new SignCwTokenRequest(tempClientId), cancellationTokenSource.Token), Times.Once);
		tokenServicesFactoryMock.VerifyNoOtherCalls();
		tokenGeneratorService.VerifyNoOtherCalls();
	}

	AuthenticationService CreateAuthenticationService()
	{
		return new AuthenticationService();
	}

	static SystemToSystemTrustInfo CreateValidTrustInfo()
	{
		using var rsa = new RSACryptoServiceProvider(2048);
		var subjectName = "CN=Test";
		var req = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));
		return new SystemToSystemTrustInfo
		{
			ClientId = Guid.NewGuid().ToString(),
			TenantId = Guid.NewGuid().ToString(),
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			PrivateKey = rsa.ExportPrivateKey(),
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			Certificate = cert.Export(X509ContentType.Cert),
		};
	}
}
