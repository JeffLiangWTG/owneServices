using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class LocationProviderTest : Customs.Business.Testing.DataProviderTestCase<LocationProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationProvider(null));
		}

		public void TestQualifierOfIdentification()
		{
			incident.GoodsLocation.CGL_Qualifier = "A";
			AssertEquals("A", Provider.QualifierOfIdentification);
		}

		public void TestUNLocode_QualifierU()
		{
			incident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			incident.GoodsLocation.Unlocode = "XYZ";
			AssertEquals("XYZ", Provider.UNLocode);
		}

		public void TestUNLocode_QualifierNotU()
		{
			incident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			incident.GoodsLocation.Unlocode = "XYZ";
			AssertNull(Provider.UNLocode);
		}

		public void TestGNSSLatitude_QualifierW()
		{
			incident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			incident.GoodsLocation.Address.E2_Latitude = 12;
			AssertEquals("12", Provider.GNSSLatitude);
		}

		public void TestGNSSLatitude_QualifierNotW()
		{
			incident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			incident.GoodsLocation.Address.E2_Latitude = 12;
			AssertNull(Provider.GNSSLatitude);
		}

		public void TestGNSSLongitude_QualifierW()
		{
			incident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			incident.GoodsLocation.Address.E2_Longitude = 123;
			AssertEquals("123", Provider.GNSSLongitude);
		}

		public void TestGNSSLongitude_QualifierNotW()
		{
			incident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			incident.GoodsLocation.Address.E2_Longitude = 123;
			AssertNull(Provider.GNSSLongitude);
		}

		public void TestCountry()
		{
			incident.BN_EventCountryCode = Core.Constants.CountryCodes.Belgium;
			AssertEquals("BE", Provider.Country);
		}

		public void TestAddress_QualifierZ()
		{
			incident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			AssertNotNull(Provider.Address);
		}

		public void TestAddress_QualifierNotZ()
		{
			incident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			AssertNull(Provider.Address);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			incident = header.EnRouteIncidents.AddNew();

			provider = new LocationProvider(incident);
		}

		protected override LocationProvider GetProvider() => provider;

		LocationProvider provider;
		EnRouteIncident incident;
	}
}
