using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	class TransportDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsVisibleForAIR()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
				AssertEquals("MasterBillTextBox", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
				AssertEquals("OceanBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
				AssertEquals("VesselUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.VesselUserControl, declaration));
				AssertEquals("TransportIDAndNationalityUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
				AssertEquals("TransportIDAndNationalityRailUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityRailUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayENIUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));
				AssertEquals("FlightAndNationalityUserControl", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
				AssertEquals("AircraftRegistrationNumberTextBox", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));
				AssertEquals("VoyageAndNationalityUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
				AssertEquals("PortOfLoadingUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));
				AssertEquals("PortOfFirstArrivalUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, declaration));
				AssertEquals("PortOfDischargeUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, declaration));
				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._41;
				AssertEquals("AircraftRegistrationNumberTextBox", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));
			});
		}

		public void TestControlsVisibleForSEA()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				AssertEquals("MasterBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
				AssertEquals("OceanBillTextBox", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
				AssertEquals("VesselUserControl", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.VesselUserControl, declaration));
				AssertEquals("TransportIDAndNationalityUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
				AssertEquals("TransportIDAndNationalityRailUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityRailUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayENIUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));
				AssertEquals("FlightAndNationalityUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
				AssertEquals("AircraftRegistrationNumberTextBox", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));
				AssertEquals("VoyageAndNationalityUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
				AssertEquals("PortOfLoadingUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));
				AssertEquals("PortOfFirstArrivalUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, declaration));
				AssertEquals("PortOfDischargeUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, declaration));
			});
		}

		public void TestControlsVisibleForROA()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
				AssertEquals("MasterBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
				AssertEquals("OceanBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
				AssertEquals("VesselUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.VesselUserControl, declaration));
				AssertEquals("TransportIDAndNationalityUserControl", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
				AssertEquals("TransportIDAndNationalityRailUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityRailUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayENIUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));
				AssertEquals("FlightAndNationalityUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
				AssertEquals("AircraftRegistrationNumberTextBox", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));
				AssertEquals("VoyageAndNationalityUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
				AssertEquals("PortOfLoadingUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));
				AssertEquals("PortOfFirstArrivalUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, declaration));
				AssertEquals("PortOfDischargeUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, declaration));
			});
		}

		public void TestControlsVisibleForFIX()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = declaration.TransportModeFixedCodeForTesting;
				AssertEquals("MasterBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
				AssertEquals("OceanBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
				AssertEquals("VesselUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.VesselUserControl, declaration));
				AssertEquals("TransportIDAndNationalityUserControl", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
				AssertEquals("TransportIDAndNationalityRailUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityRailUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayENIUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));
				AssertEquals("FlightAndNationalityUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
				AssertEquals("AircraftRegistrationNumberTextBox", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));
				AssertEquals("VoyageAndNationalityUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
				AssertEquals("PortOfLoadingUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));
				AssertEquals("PortOfFirstArrivalUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, declaration));
				AssertEquals("PortOfDischargeUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, declaration));
			});
		}

		public void TestControlsVisibleForMAI()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = declaration.TransportModeFixedCodeForTesting;
				AssertEquals("MasterBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
				AssertEquals("OceanBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
				AssertEquals("VesselUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.VesselUserControl, declaration));
				AssertEquals("TransportIDAndNationalityUserControl", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
				AssertEquals("TransportIDAndNationalityRailUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityRailUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayENIUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));
				AssertEquals("FlightAndNationalityUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
				AssertEquals("AircraftRegistrationNumberTextBox", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));
				AssertEquals("VoyageAndNationalityUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
				AssertEquals("PortOfLoadingUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));
				AssertEquals("PortOfFirstArrivalUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, declaration));
				AssertEquals("PortOfDischargeUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, declaration));
			});
		}

		public void TestControlsVisibleForRAI()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
				AssertEquals("MasterBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
				AssertEquals("OceanBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
				AssertEquals("VesselUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.VesselUserControl, declaration));
				AssertEquals("TransportIDAndNationalityUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
				AssertEquals("TransportIDAndNationalityRailUserControl", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityRailUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayENIUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));
				AssertEquals("FlightAndNationalityUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
				AssertEquals("AircraftRegistrationNumberTextBox", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));
				AssertEquals("VoyageAndNationalityUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
				AssertEquals("PortOfLoadingUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));
				AssertEquals("PortOfFirstArrivalUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, declaration));
				AssertEquals("PortOfDischargeUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, declaration));
			});
		}

		public void TestControlsVisibleForIWT()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = "IWT";
				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._80;
				AssertEquals("MasterBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
				AssertEquals("OceanBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
				AssertEquals("VesselUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.VesselUserControl, declaration));
				AssertEquals("TransportIDAndNationalityUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
				AssertEquals("TransportIDAndNationalityRailUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityRailUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayENIUserControl", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));
				AssertEquals("FlightAndNationalityUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
				AssertEquals("AircraftRegistrationNumberTextBox", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));
				AssertEquals("VoyageAndNationalityUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
				AssertEquals("PortOfLoadingUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));
				AssertEquals("PortOfFirstArrivalUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, declaration));
				AssertEquals("PortOfDischargeUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, declaration));
				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._81;
				AssertEquals("TransportIDAndNationalityInlandWaterwayUserControl", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayENIUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));
			});
		}

		public void TestControlsVisibleForOWN()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = "OWN";
				AssertEquals("MasterBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
				AssertEquals("OceanBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
				AssertEquals("VesselUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.VesselUserControl, declaration));
				AssertEquals("TransportIDAndNationalityUserControl", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
				AssertEquals("TransportIDAndNationalityRailUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityRailUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayENIUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));
				AssertEquals("FlightAndNationalityUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
				AssertEquals("AircraftRegistrationNumberTextBox", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));
				AssertEquals("VoyageAndNationalityUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
				AssertEquals("PortOfLoadingUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));
				AssertEquals("PortOfFirstArrivalUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, declaration));
				AssertEquals("PortOfDischargeUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, declaration));
				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._40;
				AssertEquals("FlightAndNationalityUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._81;
				AssertEquals("TransportIDAndNationalityInlandWaterwayUserControl", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));
				AssertEquals("TransportIDAndNationalityInlandWaterwayENIUserControl", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));
			});
		}

		public void TestControlsVisibleForInlandMOTAIR()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = declaration.TransportModeAirCodeForTesting;

			CombineAssertions(() =>
			{
				AssertEquals("TransportInlandSeparatorUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
				AssertEquals("TransportInlandModeAndTypeOfIdUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, declaration));
				AssertEquals("TransportInlandIDAndNationalityUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, declaration));
				AssertEquals("TransportInlandRoadUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, declaration));
				AssertEquals("TransportInlandSeaUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
			});
		}

		public void TestControlsVisibleForInlandMOTFIX()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = declaration.TransportModeFixedCodeForTesting;

			CombineAssertions(() =>
			{
				AssertEquals("TransportInlandSeparatorUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
				AssertEquals("TransportInlandModeAndTypeOfIdUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, declaration));
				AssertEquals("TransportInlandIDAndNationalityUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, declaration));
				AssertEquals("TransportInlandRoadUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, declaration));
				AssertEquals("TransportInlandSeaUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
			});
		}

		public void TestControlsVisibleForInlandMOTIWT()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = "IWT";

			CombineAssertions(() =>
			{
				AssertEquals("TransportInlandSeparatorUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
				AssertEquals("TransportInlandModeAndTypeOfIdUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, declaration));
				AssertEquals("TransportInlandIDAndNationalityUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, declaration));
				AssertEquals("TransportInlandRoadUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, declaration));
				AssertEquals("TransportInlandSeaUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
			});
		}

		public void TestControlsVisibleForInlandMOTOWN()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = "OWN";

			CombineAssertions(() =>
			{
				AssertEquals("TransportInlandSeparatorUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
				AssertEquals("TransportInlandModeAndTypeOfIdUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, declaration));
				AssertEquals("TransportInlandIDAndNationalityUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, declaration));
				AssertEquals("TransportInlandRoadUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, declaration));
				AssertEquals("TransportInlandSeaUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
			});
		}

		public void TestControlsVisibleForInlandMOTMAI()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = declaration.TransportModeMailCodeForTesting;

			CombineAssertions(() =>
			{
				AssertEquals("TransportInlandSeparatorUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
				AssertEquals("TransportInlandModeAndTypeOfIdUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, declaration));
				AssertEquals("TransportInlandIDAndNationalityUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, declaration));
				AssertEquals("TransportInlandRoadUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, declaration));
				AssertEquals("TransportInlandSeaUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
			});
		}

		public void TestControlsVisibleForInlandMOTRAI()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = declaration.TransportModeRailCodeForTesting;

			CombineAssertions(() =>
			{
				AssertEquals("TransportInlandSeparatorUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
				AssertEquals("TransportInlandModeAndTypeOfIdUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, declaration));
				AssertEquals("TransportInlandIDAndNationalityUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, declaration));
				AssertEquals("TransportInlandRoadUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, declaration));
				AssertEquals("TransportInlandSeaUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
			});
		}

		public void TestControlsVisibleForInlandMOTROA()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = declaration.TransportModeRoadCodeForTesting;

			CombineAssertions(() =>
			{
				AssertEquals("TransportInlandSeparatorUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
				AssertEquals("TransportInlandModeAndTypeOfIdUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, declaration));
				AssertEquals("TransportInlandRoadUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, declaration));
				AssertEquals("TransportInlandSeaUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
			});
		}

		public void TestControlsVisibleForInlandMOTSEA()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = declaration.TransportModeSeaCodeForTesting;

			CombineAssertions(() =>
			{
				AssertEquals("TransportInlandSeparatorUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
				AssertEquals("TransportInlandModeAndTypeOfIdUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, declaration));
				AssertEquals("TransportInlandIDAndNationalityUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, declaration));
				AssertEquals("TransportInlandRoadUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, declaration));
				AssertEquals("TransportInlandSeaUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
			});
		}

		PanelLayout Layout => layout ?? (layout = new TransportDetailsLayout().Layout);
		PanelLayout layout;
	}
}
