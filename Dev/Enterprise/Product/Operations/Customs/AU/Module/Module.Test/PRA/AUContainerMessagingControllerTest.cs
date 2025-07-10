using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.PRA.Module.Testing
{
	[TestedType(typeof(AUContainerMessagingController))]
	sealed class AUContainerMessagingControllerTest : ZControllerBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.AUContainerMessaging;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var container = Factory.New<CommonContainer>();
			Factory.Save();
			return container;
		}
	}
}
