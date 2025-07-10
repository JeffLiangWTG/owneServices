using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	internal sealed class ProductAreaAssignmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProductAreaList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("AAA", "SCW");
			list.AddPair("BBB", "TST");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			ProductAreaAssignment assignment = new ProductAreaAssignment();
			AssertEquals(3, assignment.Lookups.ProductAreaList.Count);
			AssertEquals(true, assignment.Lookups.ProductAreaList.ContainsCode(ProductAreaAssignmentLookups.BlankProductAreaCode));
		}
	}
}