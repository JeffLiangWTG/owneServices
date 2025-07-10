using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.DataContracts;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement.Test;

public class WtgTokenServicesFactoryTest : TransactionedTestCase
{
	Mock<IServiceHostClient> serviceHostClientMock;
	Mock<IServiceHostsCache> serviceHostsCacheMock;
	Mock<IDbAccessTokenRetriever> dbAccessTokenRetrieverMock;
	IDisposable serviceHostsCacheOverride;
	IDisposable dbAccessTokenRetrieverOverride;
	CancellationTokenSource cancellationTokenSource;

	protected override void SetUp()
	{
		base.SetUp();
		serviceHostClientMock = new Mock<IServiceHostClient>(MockBehavior.Strict);
		serviceHostClientMock
			.Setup(m => m.PostNextRequestAsync<SignCwTokenRequest, SignCwTokenResponse>(It.IsAny<string>(), It.IsAny<SignCwTokenRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new SignCwTokenResponse("mock token"));
		serviceHostClientMock
			.Setup(m => m.PostNextRequestAsync<ResetAccessTokenRequest, ResetAccessTokenResponse>(It.IsAny<string>(), It.IsAny<ResetAccessTokenRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ResetAccessTokenResponse());
		serviceHostsCacheMock = new Mock<IServiceHostsCache>(MockBehavior.Strict);
		serviceHostsCacheMock
			.Setup(m => m.CallRandomConfiguredHostsUntilFirstSuccessAsync(It.IsAny<Func<IServiceHostClient, CancellationToken, Task<SignCwTokenResponse>>>(), It.IsAny<CancellationToken>()))
			.Returns((Func<IServiceHostClient, CancellationToken, Task<SignCwTokenResponse>> func, CancellationToken ct) => func(serviceHostClientMock.Object, ct));
		serviceHostsCacheMock
			.Setup(m => m.CallRandomConfiguredHostsUntilFirstSuccessAsync(It.IsAny<Func<IServiceHostClient, CancellationToken, Task<ResetAccessTokenResponse>>>(), It.IsAny<CancellationToken>()))
			.Returns((Func<IServiceHostClient, CancellationToken, Task<ResetAccessTokenResponse>> func, CancellationToken ct) => func(serviceHostClientMock.Object, ct));
		serviceHostsCacheOverride = ObjectFactory.Substitute(serviceHostsCacheMock.Object);
		dbAccessTokenRetrieverMock = new Mock<IDbAccessTokenRetriever>(MockBehavior.Strict);
		dbAccessTokenRetrieverMock
			.Setup(m => m.GetDbAccessTokenAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync("DbApiToken");
		dbAccessTokenRetrieverOverride = ObjectFactory.Substitute(dbAccessTokenRetrieverMock.Object);
		cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
	}

	protected override void TearDown()
	{
		cancellationTokenSource?.Dispose();
		serviceHostsCacheOverride?.Dispose();
		dbAccessTokenRetrieverOverride?.Dispose();
		base.TearDown();
		serviceHostClientMock?.VerifyNoOtherCalls();
		serviceHostsCacheMock?.VerifyNoOtherCalls();
		dbAccessTokenRetrieverMock?.VerifyNoOtherCalls();
	}

	public void TestConstructor()
	{
		var wtgTokenServicesFactory = new WtgTokenServicesFactory();
		AssertNotNull(wtgTokenServicesFactory);
	}

	public void TestObjectFactoryRegistration()
	{
		var wtgTokenServicesFactory = ObjectFactory.Get<ITokenServicesFactory>();
		AssertNotNull(wtgTokenServicesFactory);
		AssertType<WtgTokenServicesFactory>(wtgTokenServicesFactory);
	}

	public void TestGetTokenGeneratorService_NoProtection_UseRegistryItem()
	{
		var wtgTokenServicesFactory = new WtgTokenServicesFactory();
		using var overrideMechanism = OverrideProtectionMechanism(DataProtectionMechanisms.Codes.None);
		CheckTokenGeneratorServiceUseRegistryItem(wtgTokenServicesFactory);
	}

	public void TestGetTokenGeneratorService_WithProtection_UseTokenSigningService()
	{
		var wtgTokenServicesFactory = new WtgTokenServicesFactory();
		using var overrideMechanism = OverrideProtectionMechanism(DataProtectionMechanisms.Codes.ActiveDirectory);
		CheckTokenGeneratorServiceUseTokenSigningService(wtgTokenServicesFactory);
	}

	public void TestGetTokenGeneratorService_WhenProtectionChange_ChangeService()
	{
		var wtgTokenServicesFactory = new WtgTokenServicesFactory();
		using (OverrideProtectionMechanism(DataProtectionMechanisms.Codes.None))
		{
			CheckTokenGeneratorServiceUseRegistryItem(wtgTokenServicesFactory);
		}
		using (OverrideProtectionMechanism(DataProtectionMechanisms.Codes.ActiveDirectory))
		{
			CheckTokenGeneratorServiceUseTokenSigningService(wtgTokenServicesFactory);
		}
		using (OverrideProtectionMechanism(DataProtectionMechanisms.Codes.None))
		{
			CheckTokenGeneratorServiceUseRegistryItem(wtgTokenServicesFactory);
		}
	}

	void CheckTokenGeneratorServiceUseRegistryItem(ITokenServicesFactory tokenServicesFactory)
	{
		AssertNotNull(tokenServicesFactory);
		var systemToSystemTrustInfo = CreateTrustInfoWithCertificate();
		using var overrideTrustInfo = OverrideSystemToSystemTrustInfo(systemToSystemTrustInfo);

		var tokenGeneratorService = tokenServicesFactory.GetTokenGeneratorService();

		AssertNotNull(tokenGeneratorService);
		var audience = Guid.NewGuid().ToString();
		var request = new SignCwTokenRequest(audience);
		var e = AssertExceptionThrown<ArgumentNullException>(() => tokenGeneratorService.SignCwTokenAsync(request, cancellationTokenSource.Token).GetAwaiter().GetResult());
		CombineAssertions(() =>
		{
			AssertContains("Value cannot be null.", e?.Message);
			AssertEquals("privateKey", e?.ParamName);
		});
	}

	void CheckTokenGeneratorServiceUseTokenSigningService(ITokenServicesFactory tokenServicesFactory)
	{
		AssertNotNull(tokenServicesFactory);
		var tokenGeneratorService = tokenServicesFactory.GetTokenGeneratorService();

		AssertNotNull(tokenGeneratorService);

		var audience = Guid.NewGuid().ToString();
		var request = new SignCwTokenRequest(audience);
		var response = tokenGeneratorService.SignCwTokenAsync(request, cancellationTokenSource.Token).GetAwaiter().GetResult();

		AssertEquals(new SignCwTokenResponse("mock token"), response);
		dbAccessTokenRetrieverMock.Verify(m => m.GetDbAccessTokenAsync(cancellationTokenSource.Token), Times.Once);
		serviceHostClientMock.Verify(
			m => m.PostNextRequestAsync<SignCwTokenRequest, It.IsAnyType>(
				"signCW",
				request,
				"ApiKey DbApiToken",
				cancellationTokenSource.Token),
			Times.Once);
		serviceHostsCacheMock.Verify(
			m => m.CallRandomConfiguredHostsUntilFirstSuccessAsync(
				It.IsAny<Func<IServiceHostClient, CancellationToken, Task<It.IsAnyType>>>(),
				cancellationTokenSource.Token),
			Times.Once);
	}

	public void TestGetTokenConfigWriterService_NoProtection_UseRegistryItem()
	{
		var wtgTokenServicesFactory = new WtgTokenServicesFactory();
		using var overrideMechanism = OverrideProtectionMechanism(DataProtectionMechanisms.Codes.None);
		CheckTokenConfigWriterServiceUseRegistryItem(wtgTokenServicesFactory);
	}

	public void TestGetTokenConfigWriterService_WithProtection_UseTokenSigningService()
	{
		var wtgTokenServicesFactory = new WtgTokenServicesFactory();
		using var overrideMechanism = OverrideProtectionMechanism(DataProtectionMechanisms.Codes.ActiveDirectory);
		CheckTokenConfigWriterServiceUseTokenSigningService(wtgTokenServicesFactory);
	}

	public void TestGetTokenConfigWriterService_WhenProtectionChange_ChangeService()
	{
		var wtgTokenServicesFactory = new WtgTokenServicesFactory();
		using (OverrideProtectionMechanism(DataProtectionMechanisms.Codes.None))
		{
			CheckTokenConfigWriterServiceUseRegistryItem(wtgTokenServicesFactory);
		}
		using (OverrideProtectionMechanism(DataProtectionMechanisms.Codes.ActiveDirectory))
		{
			CheckTokenConfigWriterServiceUseTokenSigningService(wtgTokenServicesFactory);
		}
		using (OverrideProtectionMechanism(DataProtectionMechanisms.Codes.None))
		{
			CheckTokenConfigWriterServiceUseRegistryItem(wtgTokenServicesFactory);
		}
	}

	void CheckTokenConfigWriterServiceUseRegistryItem(ITokenServicesFactory tokenServicesFactory)
	{
		AssertNotNull(tokenServicesFactory);
		var systemToSystemTrustInfo = CreateTrustInfoWithCertificate();
		using var overrideTrustInfo = OverrideSystemToSystemTrustInfo(systemToSystemTrustInfo);

		var tokenConfigWriterService = tokenServicesFactory.GetTokenConfigWriterService();

		AssertNotNull(tokenConfigWriterService);
		var request = new ResetAccessTokenRequest();
		var response = tokenConfigWriterService.ResetAccessTokenAsync(request, cancellationTokenSource.Token).GetAwaiter().GetResult();

		var newTrustInfo = SystemDataRegistry.Instance.SystemToSystemCertificate.Value;
		CombineAssertions(() =>
		{
			AssertNotNull(response);
			AssertNotEquals(systemToSystemTrustInfo, newTrustInfo);
			AssertNullOrEmpty(newTrustInfo.ClientId);
			AssertNullOrEmpty(newTrustInfo.TenantId);
		});
	}

	void CheckTokenConfigWriterServiceUseTokenSigningService(ITokenServicesFactory tokenServicesFactory)
	{
		AssertNotNull(tokenServicesFactory);
		var tokenConfigWriterService = tokenServicesFactory.GetTokenConfigWriterService();

		AssertNotNull(tokenConfigWriterService);
		var request = new ResetAccessTokenRequest();
		var response = tokenConfigWriterService.ResetAccessTokenAsync(request, cancellationTokenSource.Token).GetAwaiter().GetResult();

		AssertNotNull(response);

		dbAccessTokenRetrieverMock.Verify(m => m.GetDbAccessTokenAsync(cancellationTokenSource.Token), Times.Once);
		serviceHostClientMock.Verify(
			m => m.PostNextRequestAsync<ResetAccessTokenRequest, ResetAccessTokenResponse>("resetAccessToken", request, "ApiKey DbApiToken", cancellationTokenSource.Token),
			Times.Once);
		serviceHostsCacheMock.Verify(
			m => m.CallRandomConfiguredHostsUntilFirstSuccessAsync(
				It.IsAny<Func<IServiceHostClient, CancellationToken, Task<It.IsAnyType>>>(),
				cancellationTokenSource.Token),
			Times.Once);
	}

	static SystemToSystemTrustInfo CreateTrustInfoWithCertificate()
	{
		using var rsa = new RSACryptoServiceProvider(2048);
		var subjectName = "CN=Test";
		var req = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));
		return new SystemToSystemTrustInfo
		{
			ClientId = Guid.NewGuid().ToString(),
			TenantId = Guid.NewGuid().ToString(),
			Certificate = cert.Export(X509ContentType.Cert),
		};
	}

	IDisposable OverrideProtectionMechanism(string mechanismCode)
	{
		return OverrideRegistryItem(SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism, mechanismCode);
	}

	IDisposable OverrideSystemToSystemTrustInfo(SystemToSystemTrustInfo systemToSystemTrustInfo)
	{
		return OverrideRegistryItem(SystemDataRegistry.Instance.SystemToSystemCertificate, systemToSystemTrustInfo);
	}

	IDisposable OverrideRegistryItem<T>(StronglyTypedRegistryItem<T> registryItem, T value)
	{
		return registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
	}
}
