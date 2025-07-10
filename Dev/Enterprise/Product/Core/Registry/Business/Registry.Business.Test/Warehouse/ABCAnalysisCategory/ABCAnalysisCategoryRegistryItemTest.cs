using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ABCAnalysisCategoryRegistryItem))]
	sealed class ABCAnalysisCategoryRegistryItemTest : StronglyTypedRegistryItemTestCase<ABCAnalysisCategoryCollection>
	{
		protected override StronglyTypedRegistryItem<ABCAnalysisCategoryCollection, ABCAnalysisCategoryCollection> GetNewRegistryItem()
		{
			return new ABCAnalysisCategoryRegistryItem("", null, null, null, RegistryStorageFlags.System, new ABCAnalysisCategoryCollection());
		}
	}
}
