using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class GvmsCustomsReferenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsReferenceList()
		{
			var header = Factory.New<GvmsCustomsReference>();
			AssertContainsExactElementsInAnyOrder(GvmsItemReferencePartitions.CustomsReferenceCodes, ((ReadOnlyCodeDescriptionPairList)header.Lookups.CodeList).GetAllCodes());
		}
	}
}
