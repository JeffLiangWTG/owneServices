using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	public class IE007IncidentLocationProviderTest : Customs.Business.Testing.DataProviderTestCase<IE007IncidentLocationProvider>
	{
		public void TestLocationCodeType()
		{
			AssertNull("LocationCodeType", Provider.LocationCodeType);
		}

		public void TestUNLocode()
		{
			goodsLocation.Unlocode = "IE001";
			AssertEquals("UNLocode", "IE001", Provider.UNLocode);
		}

		public void TestCountry()
		{
			goodsLocation.Unlocode = "IE001";
			AssertEquals("Country", "IE", Provider.Country);
		}

		public void TestAddress()
		{
			AssertNull("Address", Provider.Address);
		}

		public void TestQualifierOfIdentification()
		{
			goodsLocation.CGL_Qualifier = "X";
			AssertEquals("QualifierOfIdentification", "X", Provider.QualifierOfIdentification);
		}

		public void TestGNSS()
		{
			AssertNull("GNSS", Provider.GNSS);
		}

		protected override IE007IncidentLocationProvider GetProvider() => new IE007IncidentLocationProvider((CusGoodsLocation)goodsLocation);

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			incident = nctsHeader.EnRouteIncidents.AddNew();
			goodsLocation = incident.GoodsLocation;
		}

		EnRouteIncident incident;
		EU.Business.CusGoodsLocation goodsLocation;
	}
}
