using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(GuidArrayRegistryItem))]
	sealed class GuidArrayRegistryItemTest : StronglyTypedRegistryItemTestCase<Guid[]>
	{
		public void TestConstructor()
		{
			Guid newGuid = Guid.NewGuid();
			Guid[] guidList = new Guid[] { newGuid };
			GuidArrayRegistryItem registry = new GuidArrayRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company, guidList);

			AssertNotNull("Registry should not be null", registry);
			Guid[] defaultValue = registry.DefaultValue;
			AssertEquals("DefaultValue.Length", 1, defaultValue.Length);
			AssertEquals("DefaultValue[0]", newGuid, defaultValue[0]);
			AssertEquals("Value.Length", 1, registry.Value.Length);

			registry = new GuidArrayRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company);
			AssertNotNull("Registry should not be null", registry);
			AssertEquals("Value.Length", 0, registry.Value.Length);
		}

		public void TestValue()
		{
			Guid newGuid = Guid.NewGuid();
			Guid[] guidList = new Guid[] { newGuid };
			GuidArrayRegistryItem registry = new GuidArrayRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company, guidList);
			AssertEquals("Registry.Value.Length", guidList.Length, registry.Value.Length);
			AssertCollectionContains("Registry.Value contains NewGuid", newGuid, registry.Value);
		}

		protected override StronglyTypedRegistryItem<Guid[], Guid[]> GetNewRegistryItem()
		{
			return new GuidArrayRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
