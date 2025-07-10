using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	class JobDeclarationTransportSupporterTest : TestCaseWithFactory
	{
		public void TestDefaultItineraryCountriesFromRouting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var itineraryCountry = declaration.ItineraryCountries.AddNew();
			itineraryCountry.CY_Code = Core.Constants.CountryCodes.Netherlands;

			var transport1 = declaration.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "IEDUB";
			transport1.JW_RL_NKDiscPort = "GBLON";
			var transport2 = declaration.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "NLAMS";
			transport2.JW_RL_NKDiscPort = "DEBRE";
			var transport3 = declaration.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "DEHAM";
			transport3.JW_RL_NKDiscPort = "DKCPH";

			AssertEquals(3, declaration.ItineraryCountries.Count);
			AssertEquals("Already exists in ItineraryCountries, do not add", "NL", declaration.ItineraryCountries[0].CY_Code);
			AssertEquals("Add only one for two DE ports", "DE", declaration.ItineraryCountries[1].CY_Code);
			AssertEquals("Normal", "DK", declaration.ItineraryCountries[2].CY_Code);
		}
	}
}
