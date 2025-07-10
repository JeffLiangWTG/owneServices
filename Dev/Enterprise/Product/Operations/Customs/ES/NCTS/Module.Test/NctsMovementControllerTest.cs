using System;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.ES.NCTS.GUI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.Module.Testing
{
	[TestedType(typeof(NctsMovementController))]
	class NctsMovementControllerTest : ZControllerBasherTest
	{
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
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			AssertNctsMovementFormType<NctsMovementForm>(nctsHeader);
		}

		[RequiresSTA]
		public void TestNctsMovementFormPhase5Departure()
		{
			var nctsHeaderDeparture = Factory.New<NctsHeader>();
			nctsHeaderDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = "mrnCode";

			var nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderArrival.MovementReferenceEntryNumber.CE_EntryNum = "mrnCode";

			CombineAssertions(() =>
			{
				AssertNctsMovementFormType<Phase5DepartureMovementForm>(nctsHeaderDeparture, message: "Phase5DepartureMovementForm, BM_Phase is not TNN");

				nctsHeaderDeparture.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				nctsHeaderArrival.ESNctsHeader.CEN_TNNArrival = true;
				AssertNctsMovementFormType<Phase5ArrivalMovementForm>(nctsHeaderArrival, message: "Phase5ArrivalMovementForm, BM_Phase is TNN");

				nctsHeaderDeparture.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Annexes;
				AssertNctsMovementFormType<Phase5DepartureMovementForm>(nctsHeaderDeparture, message: "Phase5DepartureMovementForm, BM_Phase is DOT");
			});
		}

		public void TestNctsMovementFormPhase5DepartureWhenCEN_TNNArrivalIsFalse()
		{
			var nctsHeaderDeparture = Factory.New<NctsHeader>();
			nctsHeaderDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = "mrnCode";

			var nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderArrival.MovementReferenceEntryNumber.CE_EntryNum = "mrnCode";

			CombineAssertions(() =>
			{
				AssertNctsMovementFormType<Phase5DepartureMovementForm>(nctsHeaderDeparture, message: "Phase5DepartureMovementForm, BM_Phase is not TNN");

				nctsHeaderDeparture.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				nctsHeaderArrival.ESNctsHeader.CEN_TNNArrival = false;
				AssertNctsMovementFormType<Phase5DepartureMovementForm>(nctsHeaderDeparture, message: "Phase5DepartureMovementForm, CEN_TNNArrival is false");
			});
		}

		public void TestNctsMovementFormPhase5Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = "NC5";
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			AssertNctsMovementFormType<Phase5ArrivalMovementForm>(nctsHeader);
		}

		public override Type ControllerToBashType => typeof(NctsMovementController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.NctsMovementController;

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();
			return nctsHeader;
		}

		void AssertNctsMovementFormType<TForm>(NctsHeader nctsHeader, string message = "NCTS Form Type") where TForm : ZForm
		{
			var nctsMovementController = (ZControllerInternals)new NctsMovementController();
			using (var form = nctsMovementController.GetForm(nctsHeader))
			{
				AssertType<TForm>(message, form);
			}
		}
	}
}
