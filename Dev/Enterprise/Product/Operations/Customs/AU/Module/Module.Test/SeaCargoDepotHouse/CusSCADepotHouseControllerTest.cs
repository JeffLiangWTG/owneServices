using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.SeaCargo.Testing
{
	[TestedType(typeof(CusSCADepotHouseController))]
	sealed class CusSCADepotHouseControllerTest : ZControllerBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.CusSCADepotHouse;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			CusSCADepotHouse result = Factory.New<CusSCADepotHouse>();
			result.CX_HouseBill = "TEST HOUSEBILL";
			result.CX_ClientID = "C012345678";
			result.CX_Status = SeaCargoMessageTypes.ImpendingArrival;
			Factory.Save();
			return result;
		}
	}
}
