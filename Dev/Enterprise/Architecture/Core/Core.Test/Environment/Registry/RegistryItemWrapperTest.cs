using System;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RegistryItemWrapperTest : TestCase
	{
		public void TestOnOverrideDefaultChangedIsCalledBySetCurrentValueToUse()
		{
			DummyRegistryItemWrapper wrapper = new DummyRegistryItemWrapper(new RegistryItemImpl("", null, null, null, new DecimalRegistryDataType(), RegistryStorageFlags.System));
			((IRegistryItemInternals)wrapper).SetCurrentValueToUse(Guid.NewGuid(), Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			AssertEquals(1, wrapper.CountOnOverrideDefaultChangedCalled);
		}
	}
}
