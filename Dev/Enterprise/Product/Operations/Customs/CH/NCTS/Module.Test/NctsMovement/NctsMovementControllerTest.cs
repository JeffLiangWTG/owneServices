using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Module.Testing;

[TestedType(typeof(NctsMovementController))]
sealed class NctsMovementControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
{
	public void TestPlugin()
	{
		var consol = Factory.New<ForwardingConsol>();
		var getPlugInMethod = Controller.GetType().GetMethod("GetPlugIn", BindingFlags.NonPublic | BindingFlags.Instance);
		using (var plugIn = (ZPlugIn)getPlugInMethod.Invoke(Controller, new object[] { consol }))
		{
			AssertType<GUI.NctsPlugin>(plugIn);
		}
	}

	protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.NctsMovementController;

	protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		Factory.Save();
		return nctsHeader;
	}

	protected override string CountryCode => Core.Constants.CountryCodes.Switzerland;
}
