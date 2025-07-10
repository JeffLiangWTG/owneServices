using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.SeaCargo.Testing
{
	[TestedType(typeof(CusSCADepotContainerController))]
	sealed class CusSCADepotContainerControllerTest : ZControllerBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.CusSCADepotContainer;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			CusSCADepotContainer result = Factory.New<CusSCADepotContainer>();
			result.CJ_ContainerNumber = "NLHU2936501";
			result.CJ_ClientID = "C012345678";
			result.CJ_Status = SeaCargoMessageTypes.ImpendingArrival;
			Factory.Save();
			return result;
		}
	}
}
