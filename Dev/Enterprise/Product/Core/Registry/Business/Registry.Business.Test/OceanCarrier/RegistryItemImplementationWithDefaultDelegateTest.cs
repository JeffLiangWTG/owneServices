using System;
using Enterprise.Integration;
using Enterprise.Registry.Business.OceanCarrier;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.OceanCarrier
{
	sealed class RegistryItemImplementationWithDefaultDelegateTest : TestCase
	{
		public void TestGetDelegate()
		{
			var value = 15;
			var impl = new RegistryItemImplementationWithDefaultDelegate<int>(
				"",
				null,
				null,
				null,
				new IntRegistryDataType(),
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				(c, b, d) => value);
			AssertEquals(15, impl.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty));
			value = 25;
			AssertEquals(25, impl.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}
