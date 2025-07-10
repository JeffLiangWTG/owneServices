using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.GUI.NCTS;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.NCTS.Testing
{
	[TestedType(typeof(NctsMovementController))]
	public class NctsMovementControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.EU.NctsMovementController;
		}
		protected override string CountryCode => Core.Constants.CountryCodes.France;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = base.Factory.NewWithValidTestData<NctsHeader>();
			result.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			Factory.Save();
			return result;
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

		public void TestNctsMovementFormPhase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = "NCT";

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

		void AssertNctsMovementFormType<TForm>(NctsHeader nctsHeader) where TForm : ZForm
		{
			var nctsMovementController = (ZControllerInternals)Controller;
			using (var form = nctsMovementController.GetForm(nctsHeader))
			{
				AssertType<TForm>("NCTS Form Type", form);
			}
		}
	}
}
