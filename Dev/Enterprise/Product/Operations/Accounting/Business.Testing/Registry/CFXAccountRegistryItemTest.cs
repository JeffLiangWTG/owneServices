using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CFXAccountRegistryItem))]
	class CFXAccountRegistryItemTest : StronglyTypedRegistryItemTestCase<Guid>
	{
		protected override StronglyTypedRegistryItem<Guid, Guid> GetNewRegistryItem()
		{
			return new CFXAccountRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, Guid.NewGuid());
		}
	}
}
