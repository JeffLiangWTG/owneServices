using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.SystemToSystemTrust;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using WTG.IdentitySecurity;

namespace Enterprise.Registry.Business.Testing;

sealed class SystemToSystemTrustInfoServiceTest : TestCaseWithFactory
{
	public void TestGetEnterpriseCode_WithNullOverride()
	{
		CheckGetEnterpriseCode(null);
	}

	public void TestGetEnterpriseCode_WithEmptyOverride()
	{
		CheckGetEnterpriseCode(string.Empty);
	}

	public void TestGetEnterpriseCode_WithValidOverride()
	{
		CheckGetEnterpriseCode("ABC");
	}

	void CheckGetEnterpriseCode(string enterpriseCode)
	{
		var productRegistrationMock = Mock.Of<IProductRegistration>(x => x.Key.EnterpriseCode == enterpriseCode);
		using var substitute = ObjectFactory.Substitute(productRegistrationMock);
		var service = new SystemToSystemTrustInfoService();
		var result = service.GetEnterpriseCode();
		AssertEquals(enterpriseCode, result);
	}

	public void TestResetSystemToSystemTrustInfo_NoRegistry_SetsEmptyTrustInfo()
	{
		var service = new SystemToSystemTrustInfoService();
		service.ResetSystemToSystemTrustInfo();

		var trustInfoItem = SystemDataRegistry.Instance.SystemToSystemCertificate;
		CombineAssertions(() =>
		{
			AssertRegistryHasAnyValue(true, trustInfoItem);
			AssertRegistryItemHasCorrectValue(new SystemToSystemTrustInfo(), trustInfoItem.Value, withDeserialization: true);
		});
	}

	public void TestResetSystemToSystemTrustInfoAsync_WithRegistrySetup_SetsEmptyTrustInfo()
	{
		CheckTrustInfoReset(CreateValidTrustInfo());
	}

	public void TestResetSystemToSystemTrustInfo_WithRegistry_SetsEmptyTrustInfo()
	{
		var initialTrustInfo = new SystemToSystemTrustInfo
		{
			ClientId = Guid.NewGuid().ToString(),
			TenantId = Guid.NewGuid().ToString(),
			CertificateSigningRequest = "csr",
			OperationId = Guid.NewGuid().ToString(),
			LegacyCertificate = Encoding.ASCII.GetBytes("legacy certificate"),
			Certificate = Encoding.ASCII.GetBytes("certificate"),
#pragma warning disable CS0618 // For unit test purposes only
			LegacyPrivateKey = "legacy private key",
			PrivateKey = "private key",
			RolloverPrivateKey = "rollover private key",
#pragma warning restore CS0618 // For unit test purposes only
		};
		CheckTrustInfoReset(initialTrustInfo);
	}

	void CheckTrustInfoReset(SystemToSystemTrustInfo initialTrustInfo)
	{
		var service = new SystemToSystemTrustInfoService();
		using var overrideRegistry = SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, initialTrustInfo);

		service.ResetSystemToSystemTrustInfo();

		var trustInfoItem = SystemDataRegistry.Instance.SystemToSystemCertificate;
		CombineAssertions(() =>
		{
			AssertRegistryHasAnyValue(true, trustInfoItem);
			AssertRegistryItemHasCorrectValue(new SystemToSystemTrustInfo(), trustInfoItem.Value, withDeserialization: true);
		});
	}

	public void TestRetrieveSystemToSystemTrustInfoNoCache_NoRegistry_ReturnsEmptyTrustInfo()
	{
		var trustInfoItem = SystemDataRegistry.Instance.SystemToSystemCertificate;
		AssertRegistryHasAnyValue(false, trustInfoItem);
		var service = new SystemToSystemTrustInfoService();
		var retrieved = service.RetrieveSystemToSystemTrustInfoNoCache();
		CombineAssertions(() =>
		{
			AssertRegistryHasAnyValue(false, trustInfoItem);
			AssertRegistryItemHasCorrectValue(new SystemToSystemTrustInfo(), retrieved, withDeserialization: false);
		});
	}

	public void TestRetrieveSystemToSystemTrustInfoNoCache_DoNotUseCachedValue()
	{
		var registryTrustInfo = CreateValidTrustInfo();
		var cachedTrustInfo = CreateValidTrustInfo();
		using var overrideRegistry = OverrideCachedValue(registryTrustInfo, cachedTrustInfo);

		var service = new SystemToSystemTrustInfoService();
		var retrieved = service.RetrieveSystemToSystemTrustInfoNoCache();
		CombineAssertions(() =>
		{
			AssertRegistryItemHasCorrectValue(registryTrustInfo, retrieved, withDeserialization: true);
		});
	}

	public void TestRetrieveTokenConfigAsync_NoRegistry_ReturnsEmptyTrustInfo()
	{
		var trustInfoItem = SystemDataRegistry.Instance.SystemToSystemCertificate;
		AssertRegistryHasAnyValue(false, trustInfoItem);
		var service = new SystemToSystemTrustInfoService();
		var retrieved = service.RetrieveTokenConfigAsync(CancellationToken.None).GetAwaiter().GetResult();
		CombineAssertions(() =>
		{
			AssertRegistryHasAnyValue(false, trustInfoItem);
			AssertRegistryItemHasCorrectValue(new SystemToSystemTrustInfo(), trustInfoItem.Value, withDeserialization: false);
		});
	}

	public void TestRetrieveTokenConfigAsync_UseCachedValue()
	{
		var registryTrustInfo = CreateValidTrustInfo();
		var cachedTrustInfo = CreateValidTrustInfo();
		using var overrideRegistry = OverrideCachedValue(registryTrustInfo, cachedTrustInfo);

		var service = new SystemToSystemTrustInfoService();
		var retrieved = service.RetrieveTokenConfigAsync(CancellationToken.None).GetAwaiter().GetResult();
		CombineAssertions(() =>
		{
			AssertType<SystemToSystemTrustInfo>(retrieved);
			AssertSame("Expected cached item", cachedTrustInfo, retrieved);
		});
	}

	public void TestSaveSystemToSystemTrustInfo_WithNullTrustInfo_Throws()
	{
		var service = new SystemToSystemTrustInfoService();
		var e = AssertExceptionThrown<NullReferenceException>(() => service.SaveSystemToSystemTrustInfo(null));

		var trustInfoItem = SystemDataRegistry.Instance.SystemToSystemCertificate;
		CombineAssertions(() =>
		{
			AssertRegistryHasAnyValue(false, trustInfoItem);
			AssertEquals("Object reference not set to an instance of an object.", e?.Message);
		});
	}

	public void TestSaveSystemToSystemTrustInfo_WithEmptyTrustInfo_SetsEmptyTrustInfo()
	{
		var service = new SystemToSystemTrustInfoService();
		service.SaveSystemToSystemTrustInfo(new SystemToSystemTrustInfo());

		var trustInfoItem = SystemDataRegistry.Instance.SystemToSystemCertificate;
		CombineAssertions(() =>
		{
			AssertRegistryHasAnyValue(true, trustInfoItem);
			AssertRegistryItemHasCorrectValue(new SystemToSystemTrustInfo(), trustInfoItem.Value, withDeserialization: true);
		});
	}

	public void TestSaveSystemToSystemTrustInfo_WithValidTrustInfo_SetsTrustInfo()
	{
		var trustInfo = CreateValidTrustInfo();
		var service = new SystemToSystemTrustInfoService();
		service.SaveSystemToSystemTrustInfo(trustInfo);

		var trustInfoItem = SystemDataRegistry.Instance.SystemToSystemCertificate;
		CombineAssertions(() =>
		{
			AssertRegistryHasAnyValue(true, trustInfoItem);
			AssertRegistryItemHasCorrectValue(trustInfo, trustInfoItem.Value, withDeserialization: true);
		});
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
#pragma warning disable CS0618 // For unit test purposes only
			PrivateKey = rsa.ExportPrivateKey(),
#pragma warning restore CS0618 // For unit test purposes only
			Certificate = cert.Export(X509ContentType.Cert),
		};
	}

	static void AssertRegistryItemHasCorrectValue(ISystemToSystemTrustInfo expected, ISystemToSystemTrustInfo actual, bool withDeserialization)
	{
		var defaultStringValue = withDeserialization ? string.Empty : null;
		AssertNotSame(expected, actual);
		AssertEquals("LegacyCertificateBytes", expected.LegacyCertificateBytes, actual.LegacyCertificateBytes);
		AssertEquals("CertificateBytes", expected.CertificateBytes, actual.CertificateBytes);
#pragma warning disable CS0618 // For unit test purposes only
		AssertEquals("LegacyPrivateKey", expected.LegacyPrivateKey ?? defaultStringValue, actual.LegacyPrivateKey);
		AssertEquals("PrivateKey", expected.PrivateKey ?? defaultStringValue, actual.PrivateKey);
		AssertEquals("RolloverPrivateKey", expected.RolloverPrivateKey ?? defaultStringValue, actual.RolloverPrivateKey);
#pragma warning restore CS0618 // For unit test purposes only
		AssertEquals("CertificateSigningRequest", expected.CertificateSigningRequest ?? defaultStringValue, actual.CertificateSigningRequest);
		AssertEquals("ClientId", expected.ClientId ?? defaultStringValue, actual.ClientId);
		AssertEquals("TenantId", expected.TenantId ?? defaultStringValue, actual.TenantId);
		AssertEquals("OperationId", expected.OperationId ?? defaultStringValue, actual.OperationId);
	}

	static void AssertRegistryHasAnyValue(bool expected, IRegistryItemInternals item)
	{
		AssertEquals("Set in registry", expected, item.HasValueForAnyLevel());
	}

	IDisposable OverrideCachedValue(ISystemToSystemTrustInfo registryValue, ISystemToSystemTrustInfo valueToCache)
	{
		var systemToSystemTrustRegistryItem = SystemDataRegistry.Instance.SystemToSystemCertificate;
		var inner = systemToSystemTrustRegistryItem.Inner;

		// get the UpdateCache method using reflection
		var updateCache = typeof(RegistryItemImpl).GetMethod("UpdateCache", BindingFlags.NonPublic | BindingFlags.Instance);
		AssertNotNull(nameof(updateCache), updateCache);

		// set the registry value
		var overrideRegistry = systemToSystemTrustRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

		// set the cached value
		var key = new RegistryCacheKey(systemToSystemTrustRegistryItem.Name, null, null, null);
		updateCache!.Invoke(inner, new object[] { key, valueToCache });

		// ensure that the cached value is properly used
		var newValue = systemToSystemTrustRegistryItem.Value;
		CombineAssertions(() =>
		{
			AssertSame("Cached value", valueToCache, newValue);
			AssertNotSame("Value without cache", registryValue, newValue);
		});
		return overrideRegistry;
	}
}
