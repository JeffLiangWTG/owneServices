using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class PortFinderWebServiceMethodTest : WebServiceMethodTest<PortFinderWebServiceMethod>
	{
		#region Implementation

		protected override void SetMethodParametersAndExpectedResponseTokens(Dictionary<string, WebServiceResponse> setting)
		{
			PortFinderParameters testParameters = new PortFinderParameters();
			testParameters.City = "";
			testParameters.Country = "";
			testParameters.PortControlID = "";
			testParameters.PostalCode = "";
			testParameters.State = "";
			WebServiceResponse expectedResponse = new WebServiceResponse();
			expectedResponse.Add(new ShowErrorResponseToken("Value cannot be null.\r\nParameter name: PortControlID"));
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.PortControlID = "TestID";
			expectedResponse = new WebServiceResponse();
			expectedResponse.Add(new UpdateValueResponseToken("TestID", ""));
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.PostalCode = "123456";
			expectedResponse = new WebServiceResponse();
			expectedResponse.Add(new UpdateValueResponseToken("TestID", "AU112"));
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.City = "A3";
			expectedResponse = new WebServiceResponse();
			expectedResponse.Add(new UpdateValueResponseToken("TestID", "AU112"));
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.PostalCode = "1234";
			testParameters.City = "A8";
			expectedResponse = new WebServiceResponse();
			expectedResponse.Add(new UpdateValueResponseToken("TestID", "AU113"));
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.PostalCode = "01234";
			testParameters.City = "A8";
			expectedResponse = new WebServiceResponse();
			expectedResponse.Add(new UpdateValueResponseToken("TestID", "AU116"));
			setting.Add(testParameters.ToString(), expectedResponse);
		}

		protected override string GetExpectedMethodName()
		{
			return "FindPort";
		}

		protected override PortFinderWebServiceMethod GetNewWebServiceMethod()
		{
			return new PortFinderWebServiceMethod();
		}

		protected override void TestExecuteSetup()
		{
			base.TestExecuteSetup();
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

			Factory.Save();
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
	}
}
