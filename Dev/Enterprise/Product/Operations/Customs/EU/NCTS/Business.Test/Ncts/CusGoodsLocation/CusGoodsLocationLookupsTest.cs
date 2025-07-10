using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQualifierList()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var incident = header.EnRouteIncidents.AddNew();
			var goodsLocation = (CusGoodsLocation)incident.GoodsLocation;
			var lookups = new CusGoodsLocationLookups(goodsLocation);
			var list = lookups.QualifierList;
			CombineAssertions(() =>
			{
				AssertEquals("List", "U, W, Z", list.CodesAsString);
				AssertSame("Cached", list, lookups.QualifierList);
			});
		}
	}
}
