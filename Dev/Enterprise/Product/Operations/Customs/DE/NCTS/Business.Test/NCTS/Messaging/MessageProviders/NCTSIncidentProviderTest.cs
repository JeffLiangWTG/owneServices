using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSIncidentProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSIncidentProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull(NCTSIncidentProvider.NewOrNull(null));
		}

		public void TestQualifierOfIdentification()
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			AssertEquals(CusGoodsLocationQualifierList.Codes.UnLocode, Provider.QualifierOfIdentification);
		}

		public void TestUNLocode()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				goodsLocation.Unlocode = "PLACE";
				AssertEquals("BN_EventPlace isn't empty", "PLACE", Provider.UNLocode);

				goodsLocation.Unlocode = ZString.Empty;
				AssertNull("BN_EventPlace is empty", Provider.UNLocode);
			});
		}

		public void TestUNLocode_QualifierIsNotU()
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			goodsLocation.Unlocode = "PLAC";
			AssertNull(Provider.UNLocode);
		}

		public void TestCountry()
		{
			incident.BN_EventCountryCode = Core.Constants.CountryCodes.Germany;
			AssertEquals(Core.Constants.CountryCodes.Germany, Provider.Country);
		}

		public void TestLongitude()
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			incident.GoodsLocation.Address.E2_Latitude = 2.12;
			incident.GoodsLocation.Address.E2_Longitude = 3.123;
			AssertEquals("+003.1230000", Provider.Longitude);
		}

		public void TestLongitude_0()
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			incident.GoodsLocation.Address.E2_Latitude = 0;
			incident.GoodsLocation.Address.E2_Longitude = 0;
			AssertEquals("+000.0000000", Provider.Longitude);
		}

		public void TestLongitude_QualifierIsNotW()
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			incident.GoodsLocation.Address.E2_Latitude = 2.12;
			incident.GoodsLocation.Address.E2_Longitude = 3.123;
			AssertNull(Provider.Longitude);
		}

		public void TestLatitude()
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			incident.GoodsLocation.Address.E2_Latitude = -2.12;
			incident.GoodsLocation.Address.E2_Longitude = 3.123;
			AssertEquals("-02.1200000", Provider.Latitude);
		}

		public void TestLatitude_0()
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			incident.GoodsLocation.Address.E2_Latitude = 0;
			incident.GoodsLocation.Address.E2_Longitude = 0;
			AssertEquals("+00.0000000", Provider.Latitude);
		}

		public void TestLatitude_QualifierIsNotW()
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			incident.GoodsLocation.Address.E2_Latitude = 2.12;
			incident.GoodsLocation.Address.E2_Longitude = 3.123;
			AssertNull(Provider.Latitude);
		}

		public void TestStreetAndNumber()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				goodsLocation.Address.E2_Address1 = "Address-field-filled-up-to-50--------------------0";
				goodsLocation.Address.E2_Address2 = "Address2";
				AssertEquals("E2_Address1 isn't empty", "Address-field-filled-up-to-50--------------------0Address2", Provider.StreetAndNumber);

				goodsLocation.Address.E2_Address1 = ZString.Empty;
				goodsLocation.Address.E2_Address2 = ZString.Empty;
				AssertNull("E2_Address1 is empty", Provider.StreetAndNumber);
			});
		}

		public void TestStreetAndNumber_QualifierIsNotZ()
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			goodsLocation.Address.E2_Address1 = "Address1";
			AssertNull(Provider.StreetAndNumber);
		}

		public void TestPostcode()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				goodsLocation.Address.E2_Postcode = "123456";
				AssertEquals("E2_Postcode isn't empty", "123456", Provider.Postcode);

				goodsLocation.Address.E2_Postcode = ZString.Empty;
				AssertNull("E2_Postcode is empty", Provider.Postcode);
			});
		}

		public void TestPostcode_QualifierIsNotZ()
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			goodsLocation.Address.E2_Postcode = "123456";
			AssertNull(Provider.Postcode);
		}

		public void TestCity()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				goodsLocation.Address.E2_City = "Sydney";
				AssertEquals("E2_City isn't empty", "Sydney", Provider.City);

				goodsLocation.Address.E2_City = ZString.Empty;
				AssertNull("E2_City is empty", Provider.City);
			});
		}

		public void TestCity_QualifierIsNotZ()
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			goodsLocation.Address.E2_City = "Sydney";
			AssertNull(Provider.City);
		}

		public void TestTransportEquipments()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._2;
			var container1 = incident.IncidentContainers.AddNew();
			container1.BC_ContainerNum = "CN001";
			var container2 = incident.IncidentContainers.AddNew();
			container2.BC_ContainerNum = "CN002";
			AssertContainsExactElementsInAnyOrder(new[] { "CN001", "CN002" }, Provider.TransportEquipments.Select(x => x.IdentificationNumber));
		}

		public void TestTransportEquipments_IncidentCodeIs1Or5()
		{
			CombineAssertions(() =>
			{
				incident.BN_IncidentCode = IncidentCodeList.Codes._1;
				var container1 = incident.IncidentContainers.AddNew();
				container1.BC_ContainerNum = "CN001";
				AssertEquals(0, Provider.TransportEquipments.Count);

				incident.BN_IncidentCode = IncidentCodeList.Codes._5;
				AssertEquals(0, NCTSIncidentProvider.NewOrNull(incident).TransportEquipments.Count);
			});
		}

		public void TestTransportEquipments_IncidentCodeIs3Or6()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._3;
			var container1 = incident.IncidentContainers.AddNew();
			container1.BC_Mode = Core.Constants.ContainerModes.Containerised;
			var container2 = incident.IncidentContainers.AddNew();
			container2.BC_Mode = Core.Constants.ContainerModes.NonContainerised;

			CombineAssertions(() =>
			{
				AssertEquals("BN_IncidentCode is 3 and one 'CNT' container", 1, Provider.TransportEquipments.Count);

				incident.BN_IncidentCode = IncidentCodeList.Codes._6;
				AssertEquals("BN_IncidentCode is 6 and one 'CNT' container", 1, NCTSIncidentProvider.NewOrNull(incident).TransportEquipments.Count);
			});
		}

		public void TestContainerIndicators()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._3;
			var container1 = incident.IncidentContainers.AddNew();
			container1.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			var container2 = incident.IncidentContainers.AddNew();
			container2.BC_Mode = Core.Constants.ContainerModes.NonContainerised;

			CombineAssertions(() =>
			{
				AssertEquals("BN_IncidentCode is 3 and no 'CNT' container", false, Provider.ContainerIndicator);

				container1.BC_Mode = Core.Constants.ContainerModes.Containerised;
				AssertEquals("BN_IncidentCode is 3 and has 'CNT' container", true, Provider.ContainerIndicator);

				incident.BN_IncidentCode = IncidentCodeList.Codes._6;
				AssertEquals("BN_IncidentCode is 6 and has 'CNT' container", true, Provider.ContainerIndicator);
			});
		}

		public void TestContainerIndicator_IncidentCodeIsNot3Or6()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._1;
			var container = incident.IncidentContainers.AddNew();
			container.BC_Mode = Core.Constants.ContainerModes.Containerised;
			AssertEquals(false, Provider.ContainerIndicator);
		}

		public void TestTypeOfIdentification()
		{
			CombineAssertions(() =>
			{
				incident.BN_IncidentCode = IncidentCodeList.Codes._3;
				incident.BN_TransportAtDepartureType = "11";
				AssertEquals("BN_TransportAtDepartureType isn't empty", "11", Provider.TypeOfIdentification);

				incident.BN_TransportAtDepartureType = ZString.Empty;
				AssertNull("BN_TransportAtDepartureType is empty", Provider.TypeOfIdentification);
			});
		}

		public void TestTypeOfIdentification_IncidentCodeIsNot3Or6()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._1;
			incident.BN_TransportAtDepartureType = "11";
			AssertNull(Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			CombineAssertions(() =>
			{
				incident.BN_IncidentCode = IncidentCodeList.Codes._3;
				incident.BN_TransportAtDepartureID = "ID11";
				AssertEquals("BN_TransportAtDepartureID isn't empty", "ID11", Provider.IdentificationNumber);

				incident.BN_TransportAtDepartureID = ZString.Empty;
				AssertNull("BN_TransportAtDepartureID is empty", Provider.IdentificationNumber);
			});
		}

		public void TestIdentificationNumber_IncidentCodeIsNot3Or6()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._1;
			incident.BN_TransportAtDepartureID = "ID11";
			AssertNull(Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			CombineAssertions(() =>
			{
				incident.BN_IncidentCode = IncidentCodeList.Codes._3;
				incident.BN_RN_NKTransportAtDepartureIDNationality = Core.Constants.CountryCodes.Germany;
				AssertEquals("BN_RN_NKTransportAtDepartureIDNationality isn't empty", Core.Constants.CountryCodes.Germany, Provider.Nationality);

				incident.BN_RN_NKTransportAtDepartureIDNationality = ZString.Empty;
				AssertNull("BN_RN_NKTransportAtDepartureIDNationality is empty", Provider.Nationality);
			});
		}

		public void TestNationality_IncidentCodeIsNot3Or6()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._1;
			incident.BN_RN_NKTransportAtDepartureIDNationality = Core.Constants.CountryCodes.Germany;
			AssertNull(Provider.Nationality);
		}

		protected override NCTSIncidentProvider GetProvider() => NCTSIncidentProvider.NewOrNull(incident);

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			incident = nctsHeader.EnRouteIncidents.AddNew();
			goodsLocation = incident.GoodsLocation as CusGoodsLocation;
		}
		EnRouteIncident incident;
		CusGoodsLocation goodsLocation;
	}
}
