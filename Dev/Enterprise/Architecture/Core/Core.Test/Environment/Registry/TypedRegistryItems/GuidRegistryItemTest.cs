using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(GuidRegistryItem))]
	sealed class GuidRegistryItemTest : StronglyTypedRegistryItemTestCase<Guid>
	{
		public void TestIsValueMandatory_ByDefault()
		{
			AssertEquals("Guid registry item should be mandatory by default", true, ((IRegistryItem)new GuidRegistryItem("", (MultilingualString)null, null, null, RegistryStorageFlags.All)).IsValueMandatory);
		}

		public void TestIsValueOptional()
		{
			AssertEquals("Guid registry item when configured to be optional", false, ((IRegistryItem)new GuidRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.IsValueOptional)).IsValueMandatory);
		}

		protected override StronglyTypedRegistryItem<Guid, Guid> GetNewRegistryItem()
		{
			return new GuidRegistryItem("", (MultilingualString)null, null, null, RegistryStorageFlags.All);
		}
	}
}
