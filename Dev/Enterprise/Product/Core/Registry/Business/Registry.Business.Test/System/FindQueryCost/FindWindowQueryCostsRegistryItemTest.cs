using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FindWindowQueryCostsRegistryItem))]
	sealed class FindWindowQueryCostsRegistryItemTest : StronglyTypedRegistryItemTestCase<FindWindowQueryCosts>
	{
		protected override StronglyTypedRegistryItem<FindWindowQueryCosts, FindWindowQueryCosts> GetNewRegistryItem()
		{
			return new FindWindowQueryCostsRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.Company, new FindWindowQueryCosts());
		}
	}
}
