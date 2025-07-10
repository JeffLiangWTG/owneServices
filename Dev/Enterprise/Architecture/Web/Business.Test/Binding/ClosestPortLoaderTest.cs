using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class ClosestPortLoaderTest : TestCaseWithFactory
	{
		#region Test Cases

		#region TestGetPortCodeFromAddressOverride

		public void TestGetPortCodeFromAddressOverride()
		{
			AssertNotNull(TestDocAddress);

			SetupDomesticCartageZone(ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZString.Empty);

			SetupDomesticCartageZone("123456", ZString.Empty, ZString.Empty, 0.5m, "A1");
			SetupDomesticCartageZone("1234", ZString.Empty, ZString.Empty, 5m, "A2");
			SetupDomesticCartageZone("123456", ZString.Empty, "US111", 7m, "A3");
			SetupDomesticCartageZone("123456", ZString.Empty, "AU111", 2m, "A4");
			SetupDomesticCartageZone("123456", "A", ZString.Empty, 10m, "A5");
			SetupDomesticCartageZone("123456", "A", "US112", 10m, "A6");
			SetupDomesticCartageZone("123456", "A", "AU112", 2m, "A7");
			SetupDomesticCartageZone("1234", "A", "US113", 5m, "A8");
			SetupDomesticCartageZone("1234", "B", "US117", 3m, "A8");
			SetupDomesticCartageZone("1234", "C", "US118", 2m, "A8");
			SetupDomesticCartageZone("1234", "A", "AU113", 1m, "A9");

			SetupDomesticCartageZone("0123456", ZString.Empty, ZString.Empty, 0.3m, "A10");
			SetupDomesticCartageZone("01234", ZString.Empty, ZString.Empty, 5m, "A11");
			SetupDomesticCartageZone("0123456", ZString.Empty, "US114", 7m, "A12");
			SetupDomesticCartageZone("0123456", ZString.Empty, "AU114", 2m, "A13");
			SetupDomesticCartageZone("0123456", "A", ZString.Empty, 10m, "A14");
			SetupDomesticCartageZone("0123456", "A", "US115", 10m, "A15");
			SetupDomesticCartageZone("0123456", "A", "AU115", 2m, "A16");
			SetupDomesticCartageZone("01234", "A", "US116", 5m, "A17");
			SetupDomesticCartageZone("01234", "A", "AU116", 1m, "A18");

			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_Code = "US113";
			unloco.RL_PortName = "A8";

			Factory.Save();

			TestDocAddress.E2_AddressOverride = true;
			TestDocAddress.E2_RN_NKCountryCode = ZString.Empty;
			Assert(TestDocAddress.E2_AddressOverride);

			AssertEquals("", TestDocAddress.E2_Postcode);
			AssertEquals("", TestDocAddress.E2_RN_NKCountryCode);

			TestPortLoader.PortCodeLoaderCalledForPortCode = false;
			AssertEquals("", TestPortLoader.GetClosestPortCode(TestDocAddress));
			AssertEquals(true, TestPortLoader.PortCodeLoaderCalledForPortCode);

			TestPortLoader.PortCodeLoaderCalledForPortCode = false;
			TestDocAddress.E2_RN_NKCountryCode = "US";
			AssertEquals("", TestPortLoader.GetClosestPortCode(TestDocAddress));
			AssertEquals(true, TestPortLoader.PortCodeLoaderCalledForPortCode);

			TestPortLoader.PortCodeLoaderCalledForPortCode = false;
			TestDocAddress.E2_RN_NKCountryCode = "US";
			TestDocAddress.E2_Postcode = "1234";
			AssertEquals("US113", TestPortLoader.GetClosestPortCode(TestDocAddress));
			AssertEquals(false, TestPortLoader.PortCodeLoaderCalledForPortCode);

			TestPortLoader.PortCodeLoaderCalledForPortCode = false;
			TestDocAddress.E2_RN_NKCountryCode = "";
			TestDocAddress.E2_Postcode = "1234";
			AssertEquals("AU113", TestPortLoader.GetClosestPortCode(TestDocAddress));   // find 1st match 1234
			AssertEquals(false, TestPortLoader.PortCodeLoaderCalledForPortCode);

			TestPortLoader.PortCodeLoaderCalledForPortCode = false;
			TestDocAddress.E2_RN_NKCountryCode = "AU";
			TestDocAddress.E2_Postcode = "123456";
			AssertEquals("AU112", TestPortLoader.GetClosestPortCode(TestDocAddress));
			AssertEquals(false, TestPortLoader.PortCodeLoaderCalledForPortCode);

			TestPortLoader.PortCodeLoaderCalledForPortCode = false;
			TestDocAddress.E2_RN_NKCountryCode = "US";
			TestDocAddress.E2_Postcode = "123456";
			AssertEquals("US112", TestPortLoader.GetClosestPortCode(TestDocAddress));
			AssertEquals(false, TestPortLoader.PortCodeLoaderCalledForPortCode);

			TestPortLoader.PortCodeLoaderCalledForPortCode = false;
			TestDocAddress.E2_RN_NKCountryCode = "";
			TestDocAddress.E2_Postcode = "123456";
			AssertEquals("AU112", TestPortLoader.GetClosestPortCode(TestDocAddress));  // find 1st match 123456
			AssertEquals(false, TestPortLoader.PortCodeLoaderCalledForPortCode);

			TestPortLoader.PortCodeLoaderCalledForPortCode = false;
			TestDocAddress.E2_RN_NKCountryCode = "ZZ";
			TestDocAddress.E2_Postcode = "123456";
			AssertEquals("AU112", TestPortLoader.GetClosestPortCode(TestDocAddress));
			AssertEquals(false, TestPortLoader.PortCodeLoaderCalledForPortCode);

			TestPortLoader.PortCodeLoaderCalledForPortCode = false;
			TestDocAddress.E2_RN_NKCountryCode = "US";
			TestDocAddress.E2_Postcode = string.Empty;
			TestDocAddress.E2_City = "A8";
			AssertEquals("when there is no postal code, use city/country to determine port code", "US113", TestPortLoader.GetClosestPortCode(TestDocAddress));
			Assert(TestPortLoader.PortCodeLoaderCalledForPortCode);

			TestPortLoader.PortCodeLoaderCalledForPortCode = false;
			TestDocAddress.E2_RN_NKCountryCode = string.Empty;
			TestDocAddress.E2_Postcode = string.Empty;
			TestDocAddress.E2_City = "A8";
			AssertEquals("when country and postal code are empty, should use country name to determine port code", "US113", TestPortLoader.GetClosestPortCode(TestDocAddress.E2_Postcode, TestDocAddress.E2_City, TestDocAddress.E2_State, TestDocAddress.E2_RN_NKCountryCode, "United States", TestDocAddress.Factory));
			Assert(TestPortLoader.PortCodeLoaderCalledForPortCode);
		}

		void SetupDomesticCartageZone(ZString postCode, ZString zoneCode, ZString countryCode, ZDecimal distance, ZString city)
		{
			RefDomesticCartageZone zone = Factory.New<RefDomesticCartageZone>();
			zone.F1_CityTownPostCode = postCode;
			zone.F1_Zone = zoneCode;
			zone.F1_RL_NKLoco = countryCode;
			zone.F1_Distance = distance;
			zone.F1_CityTown = city;
		}

		#endregion

		#region TestGetPortCodeFromAddressNoOverride

		public void TestGetPortCodeFromAddressNoOverride()
		{
			AssertNotNull(TestDocAddress);
			AssertNotNull(TestDocAddress.Organisation);
			AssertNotNull(TestDocAddress.Address);
			AssertEquals(false, TestDocAddress.E2_AddressOverride);

			TestDocAddress.Address.OA_RL_NKRelatedPortCode = ZString.Empty;

			TestDocAddress.Organisation.OH_RL_NKClosestPort = ZString.Empty;
			AssertEquals("", TestPortLoader.GetClosestPortCode(TestDocAddress));

			TestDocAddress.Organisation.OH_RL_NKClosestPort = "AUBNE";
			AssertEquals("AUBNE", TestPortLoader.GetClosestPortCode(TestDocAddress));

			TestDocAddress.Address.OA_RL_NKRelatedPortCode = "USORD";
			AssertEquals("USORD", TestPortLoader.GetClosestPortCode(TestDocAddress));

			TestDocAddress.Address.OA_RL_NKRelatedPortCode = "AUSYD";
			AssertEquals("AUSYD", TestPortLoader.GetClosestPortCode(TestDocAddress));
		}

		#endregion

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			TestDocAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			TestDocAddress.Organisation.OH_FullName = "Cargowise Importers";
			TestDocAddress.Organisation.OH_Code = "TESTORG";
			TestDocAddress.E2_OA_Address = TestDocAddress.Organisation.Addresses[0].PK;

			TestPortLoader = new PortLoader();
		}

		JobDocAddress TestDocAddress;
		PortLoader TestPortLoader;

		#endregion
	}
}
