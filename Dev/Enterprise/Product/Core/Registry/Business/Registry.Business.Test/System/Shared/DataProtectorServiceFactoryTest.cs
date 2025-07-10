using System;
using System.Threading;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing;

sealed class DataProtectorServiceFactoryTest : TransactionedTestCase
{
	public void TestCreate_NoDataProtection()
	{
		var systemDataRegistry = SystemDataRegistry.Instance;
		using var mechanismOverride = SetSystemTemporaryValue(systemDataRegistry.SystemToSystemTrustDataProtectionMechanism, DataProtectionMechanisms.Codes.None);

		var dataProtectorServiceFactory = new DataProtectorServiceFactory();
		var dataProtectorService = dataProtectorServiceFactory.Create();

		AssertType<NoProtectionDataProtectorService>(dataProtectorService);
	}

	public void TestCreate_ActiveDirectory_ValidAccountName()
	{
		var systemDataRegistry = SystemDataRegistry.Instance;
		using var mechanismOverride = SetSystemTemporaryValue(systemDataRegistry.SystemToSystemTrustDataProtectionMechanism, DataProtectionMechanisms.Codes.ActiveDirectory);
		using var groupOverride = SetSystemTemporaryValue(systemDataRegistry.SystemToSystemTrustDataProtectionGroup, "BUILTIN\\Administrators");

		var dataProtectorServiceFactory = new DataProtectorServiceFactory();
		var dataProtectorService = dataProtectorServiceFactory.Create();

		AssertType<ActiveDirectoryDataProtectorService>(dataProtectorService);
		AssertEquals("SID=S-1-5-32-544", (dataProtectorService as ActiveDirectoryDataProtectorService)?.Descriptor);
	}

	public void TestCreate_ActiveDirectory_InvalidAccountName()
	{
		var systemDataRegistry = SystemDataRegistry.Instance;
		using var validationOverride = systemDataRegistry.SystemToSystemTrustDataProtectionGroup.DataType.SuspendValidation();
		using var mechanismOverride = SetSystemTemporaryValue(systemDataRegistry.SystemToSystemTrustDataProtectionMechanism, DataProtectionMechanisms.Codes.ActiveDirectory);
		using var groupOverride = SetSystemTemporaryValue(systemDataRegistry.SystemToSystemTrustDataProtectionGroup, "Unknown");

		var dataProtectorServiceFactory = new DataProtectorServiceFactory();
		var e = AssertExceptionThrown<RegistryValidationException>(() => dataProtectorServiceFactory.Create());

		AssertEquals("AD name 'Unknown' not found.", e.Message);
	}

	public void TestCreate_UnknownMechanism()
	{
		var systemDataRegistry = SystemDataRegistry.Instance;
		using var validationOverride = systemDataRegistry.SystemToSystemTrustDataProtectionMechanism.DataType.SuspendValidation();
		using var mechanismOverride = SetSystemTemporaryValue(systemDataRegistry.SystemToSystemTrustDataProtectionMechanism, "Unknown");

		var dataProtectorServiceFactory = new DataProtectorServiceFactory();
		var e = AssertExceptionThrown<NotSupportedException>(() => dataProtectorServiceFactory.Create());

		AssertEquals("Data protection mechanism 'Unknown' is not supported.", e.Message);
	}

	public void TestCreate_CheckAllMechanismsAreCovered()
	{
		var mechanisms = new DataProtectionMechanisms();
		AssertContainsExactElementsInAnyOrder(
			"Add a test for each value.",
			new[] { DataProtectionMechanisms.Codes.ActiveDirectory, DataProtectionMechanisms.Codes.None },
			mechanisms.GetAllCodes());
	}

	public void TestIsTokenSigningServiceActiveAsync_NoDataProtection()
	{
		var systemDataRegistry = SystemDataRegistry.Instance;
		using var mechanismOverride = SetSystemTemporaryValue(systemDataRegistry.SystemToSystemTrustDataProtectionMechanism, DataProtectionMechanisms.Codes.None);

		var dataProtectorServiceFactory = new DataProtectorServiceFactory();
		var result = dataProtectorServiceFactory.IsTokenSigningServiceActiveAsync(CancellationToken.None).GetAwaiter().GetResult();

		AssertEquals(false, result);
	}

	public void TestIsTokenSigningServiceActiveAsync_ActiveDirectory()
	{
		var systemDataRegistry = SystemDataRegistry.Instance;
		using var mechanismOverride = SetSystemTemporaryValue(systemDataRegistry.SystemToSystemTrustDataProtectionMechanism, DataProtectionMechanisms.Codes.ActiveDirectory);

		var dataProtectorServiceFactory = new DataProtectorServiceFactory();
		var result = dataProtectorServiceFactory.IsTokenSigningServiceActiveAsync(CancellationToken.None).GetAwaiter().GetResult();

		AssertEquals(true, result);
	}

	public void TestIsTokenSigningServiceActiveAsync_UnknownMechanism()
	{
		var systemDataRegistry = SystemDataRegistry.Instance;
		using var validationOverride = systemDataRegistry.SystemToSystemTrustDataProtectionMechanism.DataType.SuspendValidation();
		using var mechanismOverride = SetSystemTemporaryValue(systemDataRegistry.SystemToSystemTrustDataProtectionMechanism, "Unknown");

		var dataProtectorServiceFactory = new DataProtectorServiceFactory();
		var e = AssertExceptionThrown<NotSupportedException>(() => dataProtectorServiceFactory.IsTokenSigningServiceActiveAsync(CancellationToken.None));

		AssertEquals("Data protection mechanism 'Unknown' is not supported.", e.Message);
	}

	public void TestIsTokenSigningServiceActiveAsync_CheckAllMechanismsAreCovered()
	{
		var mechanisms = new DataProtectionMechanisms();
		AssertContainsExactElementsInAnyOrder(
			"Add a test for each value.",
			new[] { DataProtectionMechanisms.Codes.ActiveDirectory, DataProtectionMechanisms.Codes.None },
			mechanisms.GetAllCodes());
	}

	IDisposable SetSystemTemporaryValue(RegistryItemWrapper wrapper, string value)
	{
		return wrapper.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
	}
}
