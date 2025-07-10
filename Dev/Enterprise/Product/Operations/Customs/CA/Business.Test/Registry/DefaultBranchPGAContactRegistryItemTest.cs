using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(DefaultBranchPGAContactRegistryItem))]
	sealed class DefaultBranchPGAContactRegistryItemTest : StronglyTypedRegistryItemTestCase<Guid>
	{
		protected override StronglyTypedRegistryItem<Guid, Guid> GetNewRegistryItem()
		{
			return new DefaultBranchPGAContactRegistryItem("", null, null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, Guid.NewGuid());
		}
	}
}
