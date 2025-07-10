using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Module.Testing;

[TestedType(typeof(NctsMovementController))]
sealed class NctsMovementControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
{
	public void TestTypeOfTopLevelBusinessObject()
	{
		AssertEquals("For IT TypeOfTopLevelBusinessObject must be", typeof(NctsHeader), new NctsMovementController().TypeOfTopLevelBusinessObject);
	}

	public void TestNctsMovementFormPhase4()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = "NCT";
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		AssertNctsMovementFormType<NctsMovementForm>(nctsHeader);
	}

	public void TestNctsMovementFormPhase5Departure()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = "NC5";
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		AssertNctsMovementFormType<EU.NCTS.GUI.Phase5DepartureMovementForm>(nctsHeader);
	}

	public void TestNctsMovementFormPhase5Arrival()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = "NC5";
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

		AssertNctsMovementFormType<EU.NCTS.GUI.Phase5ArrivalMovementForm>(nctsHeader);
	}

	public void TestPlugin()
	{
		var consol = Factory.New<ForwardingConsol>();
		var getPlugInMethod = Controller.GetType().GetMethod("GetPlugIn", BindingFlags.NonPublic | BindingFlags.Instance);
		using (var plugIn = (ZPlugIn)getPlugInMethod.Invoke(Controller, new object[] { consol }))
		{
			AssertType<NctsPlugin>(plugIn);
		}
	}

	protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
		Factory.Save();
		return nctsHeader;
	}

	protected override ControllerID GetControllerID()
	{
		return ControllerIDs.Customs.EU.NctsMovementController;
	}

	protected override string CountryCode => Core.Constants.CountryCodes.Italy;

	void AssertNctsMovementFormType<TForm>(NctsHeader nctsHeader) where TForm : ZForm
	{
		var nctsMovementController = (ZControllerInternals)new NctsMovementController();
		using (var form = nctsMovementController.GetForm(nctsHeader))
		{
			AssertType<TForm>("NCTS Form Type", form);
		}
	}
}
