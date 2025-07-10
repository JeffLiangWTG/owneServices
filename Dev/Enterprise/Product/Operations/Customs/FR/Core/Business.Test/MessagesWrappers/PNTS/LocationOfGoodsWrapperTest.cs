using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class LocationOfGoodsWrapperTest : Customs.Business.Testing.DataProviderTestCase<LocationOfGoodsWrapper>
	{
		public void TestAdditionalIdentifier()
		{
			AssertEquals("AdditionalIdentifier should be equal to AdditionalIdentifier. - AdditionalIdentifier has value.", "FRBER", Provider.AdditionalIdentifier);

			var locationOfGoods = Factory.NewWithValidTestData<CusGoodsLocation>();
			locationOfGoods.CGL_Qualifier = "U";
			locationOfGoods.CGL_AdditionalIdentifier = ZString.Empty;
			locationOfGoods.CGL_Type = "A";

			var wrapper = LocationOfGoodsWrapper.New(locationOfGoods);

			AssertEquals("AdditionalIdentifier should be equal to AdditionalIdentifier - AdditionalIdentifier is empty", ZString.Empty, wrapper.AdditionalIdentifier);
		}

		public void TestAddress()
		{
			CombineAssertions("Address should use AddressWrapper", () =>
			{
				AssertEquals("City should equal E2_City.", "MAURENS", Provider.Address.City);
				AssertEquals("Country should equal E2_RN_NKCountryCode.", "FR", Provider.Address.Country);
				AssertEquals("PostCode should equal E2_Postcode.", "24140", Provider.Address.PostCode);
				AssertEquals("StreetAndNumber should equal E2_Address1.", "177 Impasse Jane Poupelet", Provider.Address.StreetAndNumber);
			});
		}

		public void TestAuthorisationNumber()
		{
			var locationOfGoods = Factory.NewWithValidTestData<CusGoodsLocation>();
			var locationOfGoodsAddress = locationOfGoods.Address;
			locationOfGoodsAddress.E2_GovRegNum = "GRN001";

			var wrapper = LocationOfGoodsWrapper.New(locationOfGoods);

			AssertEquals("Prerequisite: goodsLocation.Address.AuthorizationNumber should be equal to E2_GovRenNum.", "GRN001", locationOfGoodsAddress.AuthorisationNumber);
			AssertEquals("AuthorisationNumber should be equal to locationOfGoodsAddress.AuthorisationNumber.", "GRN001", wrapper.AuthorisationNumber);
		}

		public void TestCustomsOffice()
		{
			AssertEquals("CustomsOffice should use CustomsOfficeWrapper.", "FR230023", Provider.CustomsOffice.ReferenceNumber);
		}

		public void TestEconomicOperator()
		{
			AssertEquals("EconomicOperator should use EconomicOperatorWrapper.", "GRN001", Provider.EconomicOperator.IdentificationNumber);
		}

		public void TestGnss()
		{
			CombineAssertions("Gnss should use GnnWrapper", () =>
			{
				AssertEquals("Latitude should equal E2_Latitude.", "32", Provider.Gnss.Latitude);
				AssertEquals("Longitude should equal E2_Longitude.", "21", Provider.Gnss.Longitude);
			});
		}

		public void TestQualifierOfIdentification()
		{
			AssertEquals("QualifierOfIdentification should equal CGL_Qualifier.", "U", Provider.QualifierOfIdentification);
		}

		public void TestTypeOfLocation()
		{
			AssertEquals("TypeOfLocation should equal CGL_Type.", "A", Provider.TypeOfLocation);
		}

		public void TestUnLoCode()
		{
			AssertEquals("UnLoCode should equal CGL_AdditionalIdentifier.", "FRBER", Provider.UnLoCode);
		}

		protected override LocationOfGoodsWrapper GetProvider()
		{
			var locationOfGoods = Factory.NewWithValidTestData<CusGoodsLocation>();
			locationOfGoods.CGL_Qualifier = "U";
			locationOfGoods.CGL_AdditionalIdentifier = "FRBER";
			locationOfGoods.CGL_CustomsOffice = "FR230023";
			locationOfGoods.CGL_Type = "A";

			var locationOfGoodsAddress = locationOfGoods.Address;
			locationOfGoodsAddress.E2_City = "MAURENS";
			locationOfGoodsAddress.E2_RN_NKCountryCode = "FR";
			locationOfGoodsAddress.E2_Postcode = "24140";
			locationOfGoodsAddress.E2_Address1 = "177 Impasse Jane Poupelet";
			locationOfGoodsAddress.E2_GovRegNum = "GRN001";
			locationOfGoodsAddress.E2_Latitude = 32m;
			locationOfGoodsAddress.E2_Longitude = 21m;

			return LocationOfGoodsWrapper.New(locationOfGoods);
		}
	}
}
