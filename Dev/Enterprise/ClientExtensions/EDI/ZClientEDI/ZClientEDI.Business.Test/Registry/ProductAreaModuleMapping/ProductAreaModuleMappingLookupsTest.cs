using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	internal sealed class ProductAreaModuleMappingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProductAreaList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("AAA", "Product Area A");
			list.AddPair("BBB", "Product Area B");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			ProductAreaModuleMapping mapping = new ProductAreaModuleMapping();
			AssertEquals(2, mapping.Lookups.ProductAreaList.Count);
		}
	}
}