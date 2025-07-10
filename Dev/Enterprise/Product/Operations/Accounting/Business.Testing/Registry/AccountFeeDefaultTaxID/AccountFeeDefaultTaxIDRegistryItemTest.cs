using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(AccountFeeDefaultTaxIDRegistryItem))]
	class AccountFeeDefaultTaxIDRegistryItemTest : StronglyTypedRegistryItemTestCase<Guid>
	{
		protected override StronglyTypedRegistryItem<Guid, Guid> GetNewRegistryItem()
		{
			return new AccountFeeDefaultTaxIDRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
