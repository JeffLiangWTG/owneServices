using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class IncidentLocationWrapperTest : Customs.Business.Testing.DataProviderTestCase<IncidentLocationWrapper>
	{
		public void TestQualifierofIdentification()
		{
			AssertEquals("QualifierofIdentification should be equal to incident.GoodsLocation.CGL_Qualifier.", "I", Provider.QualifierofIdentification);
		}

		public void TestUNLoCode()
		{
			AssertEquals("UNLoCode should be equal to incident.GoodsLocation?.Unlocode.", "Unloco", Provider.UNLoCode);
		}

		public void TestCountry()
		{
			AssertEquals("Country should be equal to goodsLocation.Address.E2_RN_NKCountryCode.", "FR", Provider.Country);
		}

		public void TestGNSS()
		{
			AssertEquals("Latitude should be equal to Incident.GoodsLocation.Address.Latitude.",  "10", Provider.GNSS.Latitude);
			AssertEquals("Longitude should be equal to Incident.GoodsLocation.Address.Longitude.", "20", Provider.GNSS.Longitude);
		}

		public void TestAddress()
		{
			AssertEquals("Address city should be equal to goodsLocation address.", "Paris", Provider.Address.City);
			AssertEquals("Address city should be equal to goodsLocation address.", "95310", Provider.Address.PostCode);
			AssertEquals("Address city should be equal to goodsLocation address.", "15 ABC", Provider.Address.StreetAndNumber);
			AssertEquals("Address city should be equal to goodsLocation address.", "FR", Provider.Address.Country);
		}

		protected override IncidentLocationWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var incident = nctsHeader.EnRouteIncidents.AddNew();
			var goodsLocation = incident.GoodsLocation;
			goodsLocation.CGL_Qualifier = "I";
			goodsLocation.Unlocode = "Unloco";
			var address = goodsLocation.Address;
			address.E2_Latitude = 10;
			address.E2_Longitude = 20;

			var goodsLocationAddress = goodsLocation.Address;
			goodsLocationAddress.E2_Address1AndE2_Address2 = "15 ABC";
			goodsLocationAddress.E2_City = "Paris";
			goodsLocationAddress.E2_Postcode = "95310";
			goodsLocationAddress.E2_RN_NKCountryCode = "FR";

			return IncidentLocationWrapper.New(incident);
		}
	}
}
