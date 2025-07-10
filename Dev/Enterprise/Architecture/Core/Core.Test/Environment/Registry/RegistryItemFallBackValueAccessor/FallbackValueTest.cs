using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class FallbackValueTest : TestCase
	{
		public void TestConstructor()
		{
			FallbackValue fallbackValue = new FallbackValue(RegistryStorageFlags.System, "Unmerged");
			AssertEquals("Level", RegistryStorageFlags.System, fallbackValue.Level);
			AssertEquals("IsMergedWithDefaultValue", false, fallbackValue.IsMergedWithDefaultValue);
			AssertEquals("Value", "Unmerged", fallbackValue.Value);

			fallbackValue = new FallbackValue(RegistryStorageFlags.System | RegistryStorageFlags.Company, true, "Merged");
			AssertEquals("Level", RegistryStorageFlags.System | RegistryStorageFlags.Company, fallbackValue.Level);
			AssertEquals("IsMergedWithDefaultValue", true, fallbackValue.IsMergedWithDefaultValue);
			AssertEquals("Value", "Merged", fallbackValue.Value);
		}
	}
}
