using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	internal class GvmsEidrReferenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEidrReferenceList()
		{
			var header = Factory.New<GvmsEidrReference>();
			AssertContainsExactElementsInAnyOrder(GvmsItemReferencePartitions.EidrReferenceCodes, ((ReadOnlyCodeDescriptionPairList)header.Lookups.CodeList).GetAllCodes());
		}
	}
}
