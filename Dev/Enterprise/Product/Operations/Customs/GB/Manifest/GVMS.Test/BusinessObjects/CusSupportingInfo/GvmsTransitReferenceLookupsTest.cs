using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	internal class GvmsTransitReferenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransitReferenceList()
		{
			var header = Factory.New<GvmsTransitReference>();
			AssertContainsExactElementsInAnyOrder(GvmsItemReferencePartitions.TransitReferenceCodes, ((ReadOnlyCodeDescriptionPairList)header.Lookups.CodeList).GetAllCodes());
		}
	}
}
