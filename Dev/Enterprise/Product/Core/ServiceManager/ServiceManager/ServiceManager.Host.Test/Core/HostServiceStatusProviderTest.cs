using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using Range = Moq.Range;

namespace Enterprise.ServiceManager.Host.Testing.Core;

sealed class HostServiceStatusProviderTest : TestCaseWithFactory
{
	Mock<IServiceHostsCache> mockServiceHostsCache;
	Mock<IHostLogger> mockLogger;
	HostServiceStatusProvider hostServiceStatusProvider;

	protected override void SetUp()
	{
		base.SetUp();
		mockServiceHostsCache = new Mock<IServiceHostsCache>(MockBehavior.Strict);
		mockLogger = new Mock<IHostLogger>();
		hostServiceStatusProvider = new HostServiceStatusProvider(mockLogger.Object, mockServiceHostsCache.Object);
	}

	protected override void TearDown()
	{
		// Verify that all expected calls were made on IServiceHostsCache
		mockServiceHostsCache.VerifyNoOtherCalls();
		// Verify that all expected calls were made on ILogger, ignoring Debug calls
		mockLogger.Verify(
			m => m.Log(LogLevel.Debug, It.IsAny<string>()),
			Times.Between(0, int.MaxValue, Range.Inclusive));
		mockLogger.VerifyNoOtherCalls();
		// call base TearDown
		base.TearDown();
	}

	public void TestWrongConstructorParams()
	{
		NUnit.Framework.Assert.Multiple(() =>
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => new HostServiceStatusProvider(null, mockServiceHostsCache.Object));
			NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("logger"));

			result = AssertExceptionThrown<ArgumentNullException>(() => new HostServiceStatusProvider(mockLogger.Object, null));
			NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceHostsCache"));
		});
	}

	[ExpectNoExceptions]
	public void TestIsReady_WithoutSecurityLauncher()
	{
		using var mechanismOverride = SetSystemTemporaryValue(SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism, DataProtectionMechanisms.Codes.None);
		NUnit.Framework.Assert.That(hostServiceStatusProvider.IsReady(), NUnit.Framework.Is.EqualTo(true));
		mockLogger.Verify(m => m.Log(LogLevel.Debug, "No extra services to check, returning IsReady = true."), Times.Once);
	}

	[ExpectNoExceptions]
	public void TestIsReady_WithLauncherSecurityAndServiceReady_ReturnTrue()
	{
		var hostName = GetHostName();
		using var mechanismOverride = SetSystemTemporaryValue(SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism, DataProtectionMechanisms.Codes.ActiveDirectory);
		var mockServiceHostClients = CreateMockServiceHostClients(hostName, Dns.GetHostName()).ToArray();
		mockServiceHostsCache
			.SetupGet(m => m.ConfiguredServiceHosts)
			.Returns(mockServiceHostClients.Select(m => m.Object));
		mockServiceHostClients[0]
			.Setup(m => m.CheckSecurityServiceReady())
			.Verifiable();
		NUnit.Framework.Assert.That(hostServiceStatusProvider.IsReady(), NUnit.Framework.Is.EqualTo(true));
		mockLogger.Verify(m => m.Log(LogLevel.Debug, "Successful response from all extra services [LauncherSecurity], returning IsReady = true."), Times.Once);
		mockServiceHostsCache.Verify(m => m.ConfiguredServiceHosts, Times.Once);
		mockServiceHostClients[0].Verify(m => m.CheckSecurityServiceReady(), Times.Once);
		foreach (var mockServiceHostClient in mockServiceHostClients)
		{
			mockServiceHostClient.Verify(m => m.HostName, Times.Once);
			mockServiceHostClient.VerifyNoOtherCalls();
		}
	}

	[ExpectNoExceptions]
	public void TestIsReady_WithLauncherSecurityAndServiceNotReady_ReturnFalse()
	{
		var hostName = GetHostName();
		var testException = new Exception("Test exception");
		using var mechanismOverride = SetSystemTemporaryValue(SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism, DataProtectionMechanisms.Codes.ActiveDirectory);
		var mockServiceHostClients = CreateMockServiceHostClients(Dns.GetHostName(), hostName).ToArray();
		mockServiceHostsCache
			.SetupGet(m => m.ConfiguredServiceHosts)
			.Returns(mockServiceHostClients.Select(m => m.Object));
		mockServiceHostClients[1]
			.Setup(m => m.CheckSecurityServiceReady())
			.Throws(testException)
			.Verifiable();
		NUnit.Framework.Assert.That(hostServiceStatusProvider.IsReady(), NUnit.Framework.Is.EqualTo(false));
		mockLogger.Verify(m => m.Log(LogLevel.Information, $"Unable to get response for LauncherSecurity on host {hostName}, returning IsReady = false", testException), Times.Once);
		mockServiceHostsCache.Verify(m => m.ConfiguredServiceHosts, Times.Once);
		mockServiceHostClients[1].Verify(m => m.CheckSecurityServiceReady(), Times.Once);
		foreach (var mockServiceHostClient in mockServiceHostClients)
		{
			mockServiceHostClient.Verify(m => m.HostName, Times.Once);
			mockServiceHostClient.VerifyNoOtherCalls();
		}
	}

	[ExpectNoExceptions]
	public void TestIsReady_WithExtraServiceAndNoServiceHostClient_ReturnFalse()
	{
		var hostName = GetHostName();
		using var mechanismOverride = SetSystemTemporaryValue(SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism, DataProtectionMechanisms.Codes.ActiveDirectory);
		mockServiceHostsCache
			.SetupGet(m => m.ConfiguredServiceHosts)
			.Returns(Enumerable.Empty<IServiceHostClient>());
		NUnit.Framework.Assert.That(hostServiceStatusProvider.IsReady(), NUnit.Framework.Is.EqualTo(false));
		mockLogger.Verify(
			m => m.Log(
				LogLevel.Error,
				$"Unable to get service host client for {hostName}",
				It.Is<Exception>(e => e.Message.Contains("Sequence contains no matching element"))),
			Times.Once);
		mockServiceHostsCache.Verify(m => m.ConfiguredServiceHosts, Times.Once);
	}

	[ExpectNoExceptions]
	public void TestIsReady_WithExtraServiceAndServiceHostClientNotFound_ReturnFalse()
	{
		var hostName = GetHostName();
		using var mechanismOverride = SetSystemTemporaryValue(SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism, DataProtectionMechanisms.Codes.ActiveDirectory);
		var mockServiceHostClients = CreateMockServiceHostClients("host2", "host3").ToArray();
		mockServiceHostsCache
			.SetupGet(m => m.ConfiguredServiceHosts)
			.Returns(mockServiceHostClients.Select(m => m.Object));
		NUnit.Framework.Assert.That(hostServiceStatusProvider.IsReady(), NUnit.Framework.Is.EqualTo(false));
		mockLogger.Verify(
			m => m.Log(
				LogLevel.Error,
				$"Unable to get service host client for {hostName}",
				It.Is<Exception>(e => e.Message.Contains("Sequence contains no matching element"))),
			Times.Once);
		mockServiceHostsCache.Verify(m => m.ConfiguredServiceHosts, Times.Once);
		foreach (var mockServiceHostClient in mockServiceHostClients)
		{
			mockServiceHostClient.Verify(m => m.HostName, Times.Once);
			mockServiceHostClient.VerifyNoOtherCalls();
		}
	}

	[ExpectNoExceptions]
	public void TestIsReady_WithExtraServiceAndSeveralServiceHostClient_ReturnFalse()
	{
		var hostName = GetHostName();
		using var mechanismOverride = SetSystemTemporaryValue(SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism, DataProtectionMechanisms.Codes.ActiveDirectory);
		var mockServiceHostClients = CreateMockServiceHostClients(hostName, "host2", "host3", hostName).ToArray();
		mockServiceHostsCache
			.SetupGet(m => m.ConfiguredServiceHosts)
			.Returns(mockServiceHostClients.Select(m => m.Object));
		NUnit.Framework.Assert.That(hostServiceStatusProvider.IsReady(), NUnit.Framework.Is.EqualTo(false));
		mockLogger.Verify(
			m => m.Log(
				LogLevel.Error,
				$"Unable to get service host client for {hostName}",
				It.Is<Exception>(e => e.Message.Contains("Sequence contains more than one matching element"))),
			Times.Once);
		mockServiceHostsCache.Verify(m => m.ConfiguredServiceHosts, Times.Once);
		foreach (var mockServiceHostClient in mockServiceHostClients)
		{
			mockServiceHostClient.Verify(m => m.HostName, Times.Once);
			mockServiceHostClient.VerifyNoOtherCalls();
		}
	}

	IDisposable SetSystemTemporaryValue(RegistryItemWrapper wrapper, string value)
	{
		return wrapper.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
	}

	IEnumerable<Mock<IServiceHostClient>> CreateMockServiceHostClients(params string[] hostNames)
	{
		return hostNames.Select(hostName =>
		{
			var mock = new Mock<IServiceHostClient>(MockBehavior.Strict);
			mock.SetupGet(m => m.HostName).Returns(hostName).Verifiable();
			return mock;
		});
	}

	ServiceHostName GetHostName()
	{
		return ServiceManagerHelper.GetHostName();
	}
}
