using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.ICS.GUI.Testing.AsycudaManifest
{
	class ManifestLayoutBuilderTest : TestCaseWithFactory
	{
		public void TestRoadAndSeaControlVisibilities()
		{
			var commonBag = CommonManifestControlBag.Instance;
			var header = Factory.New<AsycudaManifestHeaderSS>();
			var layout = ((IPanelLayoutProvider)new ManifestLayouts()).Layout;
			header.AMA_ManifestType = ICSManifestTypes.Codes.SAS;

			header.AMA_TransportMode = GBSSTransportTypeList.Codes.SeaFreight;
			CombineAssertions("Sea S&S", () =>
			{
				AssertVisibility("MessageStatusTextBox", true, commonBag.MessageStatusTextBox);
				AssertVisibility("MessageStatusDropEdit", false, commonBag.MessageStatusDropEdit);
				AssertVisibility("JobReferenceTextBox", false, commonBag.JobReferenceTextBox);
				AssertVisibility("ContainerModeDropEdit", true, commonBag.ContainerModeDropEdit);
				AssertVisibility("VesselCodeFindBox", true, commonBag.VesselCodeFindBox);
				AssertVisibility("MastersNameTextBox", true, commonBag.MastersNameTextBox);
				AssertVisibility("ConveyanceCountryCodeFindBox", true, commonBag.ConveyanceCountryCodeFindBox);
				AssertVisibility("VehicleRegistrationTextBox", false, commonBag.VehicleRegistrationTextBox);
				AssertVisibility("VoyageFlightTextBox", true, commonBag.VoyageFlightTextBox);
				AssertVisibility("RadioCallSignTextBox", true, commonBag.RadioCallSignTextBox);
				AssertVisibility("LloydsNumberTextBox", true, commonBag.LloydsNumberTextBox);
				AssertVisibility("Trailer1RegNoTextBox", false, commonBag.Trailer1RegNoTextBox);
				AssertVisibility("Trailer2RegNoTextBox", false, commonBag.Trailer2RegNoTextBox);
				AssertVisibility("Trailer1RegCountryCodeFindBox", false, commonBag.Trailer1RegCountryCodeFindBox);
				AssertVisibility("Trailer2RegCountryCodeFindBox", false, commonBag.Trailer2RegCountryCodeFindBox);
				AssertVisibility("MasterBOLTextBox", false, commonBag.MasterBOLTextBox);
				AssertVisibility("BuyersConsolidationCheckBox", false, commonBag.BuyersConsolidationCheckBox);
			});

			header.AMA_TransportMode = GBSSTransportTypeList.Codes.InlandWaterTransport;
			CombineAssertions("IWT S&S", () =>
			{
				AssertVisibility("MessageStatusTextBox", true, commonBag.MessageStatusTextBox);
				AssertVisibility("MessageStatusDropEdit", false, commonBag.MessageStatusDropEdit);
				AssertVisibility("JobReferenceTextBox", false, commonBag.JobReferenceTextBox);
				AssertVisibility("ContainerModeDropEdit", true, commonBag.ContainerModeDropEdit);
				AssertVisibility("VesselCodeFindBox", true, commonBag.VesselCodeFindBox);
				AssertVisibility("MastersNameTextBox", true, commonBag.MastersNameTextBox);
				AssertVisibility("ConveyanceCountryCodeFindBox", true, commonBag.ConveyanceCountryCodeFindBox);
				AssertVisibility("VehicleRegistrationTextBox", false, commonBag.VehicleRegistrationTextBox);
				AssertVisibility("VoyageFlightTextBox", true, commonBag.VoyageFlightTextBox);
				AssertVisibility("RadioCallSignTextBox", true, commonBag.RadioCallSignTextBox);
				AssertVisibility("LloydsNumberTextBox", true, commonBag.LloydsNumberTextBox);
				AssertVisibility("Trailer1RegNoTextBox", false, commonBag.Trailer1RegNoTextBox);
				AssertVisibility("Trailer2RegNoTextBox", false, commonBag.Trailer2RegNoTextBox);
				AssertVisibility("Trailer1RegCountryCodeFindBox", false, commonBag.Trailer1RegCountryCodeFindBox);
				AssertVisibility("Trailer2RegCountryCodeFindBox", false, commonBag.Trailer2RegCountryCodeFindBox);
				AssertVisibility("MasterBOLTextBox", false, commonBag.MasterBOLTextBox);
				AssertVisibility("BuyersConsolidationCheckBox", false, commonBag.BuyersConsolidationCheckBox);
			});

			header.AMA_TransportMode = GBSSTransportTypeList.Codes.RoadFreight;
			CombineAssertions("Road S&S", () =>
			{
				AssertVisibility("MessageStatusTextBox", true, commonBag.MessageStatusTextBox);
				AssertVisibility("MessageStatusDropEdit", false, commonBag.MessageStatusDropEdit);
				AssertVisibility("JobReferenceTextBox", false, commonBag.JobReferenceTextBox);
				AssertVisibility("ContainerModeDropEdit", true, commonBag.ContainerModeDropEdit);
				AssertVisibility("VesselCodeFindBox", false, commonBag.VesselCodeFindBox);
				AssertVisibility("MastersNameTextBox", false, commonBag.MastersNameTextBox);
				AssertVisibility("ConveyanceCountryCodeFindBox", false, commonBag.ConveyanceCountryCodeFindBox);
				AssertVisibility("VehicleRegistrationTextBox", true, commonBag.VehicleRegistrationTextBox);
				AssertVisibility("VoyageFlightTextBox", false, commonBag.VoyageFlightTextBox);
				AssertVisibility("RadioCallSignTextBox", false, commonBag.RadioCallSignTextBox);
				AssertVisibility("LloydsNumberTextBox", false, commonBag.LloydsNumberTextBox);
				AssertVisibility("Trailer1RegNoTextBox", true, commonBag.Trailer1RegNoTextBox);
				AssertVisibility("Trailer2RegNoTextBox", true, commonBag.Trailer2RegNoTextBox);
				AssertVisibility("Trailer1RegCountryCodeFindBox", true, commonBag.Trailer1RegCountryCodeFindBox);
				AssertVisibility("Trailer2RegCountryCodeFindBox", true, commonBag.Trailer2RegCountryCodeFindBox);
				AssertVisibility("MasterBOLTextBox", false, commonBag.MasterBOLTextBox);
				AssertVisibility("BuyersConsolidationCheckBox", false, commonBag.BuyersConsolidationCheckBox);
			});

			header.AMA_TransportMode = GBSSTransportTypeList.Codes.RoroAccompanied;
			CombineAssertions("RoRo Accompanied S&S", () =>
			{
				AssertVisibility("MessageStatusTextBox", true, commonBag.MessageStatusTextBox);
				AssertVisibility("MessageStatusDropEdit", false, commonBag.MessageStatusDropEdit);
				AssertVisibility("JobReferenceTextBox", false, commonBag.JobReferenceTextBox);
				AssertVisibility("ContainerModeDropEdit", true, commonBag.ContainerModeDropEdit);
				AssertVisibility("VesselCodeFindBox", false, commonBag.VesselCodeFindBox);
				AssertVisibility("MastersNameTextBox", false, commonBag.MastersNameTextBox);
				AssertVisibility("ConveyanceCountryCodeFindBox", false, commonBag.ConveyanceCountryCodeFindBox);
				AssertVisibility("VehicleRegistrationTextBox", true, commonBag.VehicleRegistrationTextBox);
				AssertVisibility("VoyageFlightTextBox", false, commonBag.VoyageFlightTextBox);
				AssertVisibility("RadioCallSignTextBox", false, commonBag.RadioCallSignTextBox);
				AssertVisibility("LloydsNumberTextBox", false, commonBag.LloydsNumberTextBox);
				AssertVisibility("Trailer1RegNoTextBox", true, commonBag.Trailer1RegNoTextBox);
				AssertVisibility("Trailer2RegNoTextBox", true, commonBag.Trailer2RegNoTextBox);
				AssertVisibility("Trailer1RegCountryCodeFindBox", true, commonBag.Trailer1RegCountryCodeFindBox);
				AssertVisibility("Trailer2RegCountryCodeFindBox", true, commonBag.Trailer2RegCountryCodeFindBox);
				AssertVisibility("MasterBOLTextBox", false, commonBag.MasterBOLTextBox);
				AssertVisibility("BuyersConsolidationCheckBox", false, commonBag.BuyersConsolidationCheckBox);
			});

			header.AMA_TransportMode = GBSSTransportTypeList.Codes.RoroUnaccompanied;
			CombineAssertions("RoRo Unaccompanied S&S", () =>
			{
				AssertVisibility("MessageStatusTextBox", true, commonBag.MessageStatusTextBox);
				AssertVisibility("MessageStatusDropEdit", false, commonBag.MessageStatusDropEdit);
				AssertVisibility("JobReferenceTextBox", false, commonBag.JobReferenceTextBox);
				AssertVisibility("ContainerModeDropEdit", true, commonBag.ContainerModeDropEdit);
				AssertVisibility("VesselCodeFindBox", false, commonBag.VesselCodeFindBox);
				AssertVisibility("MastersNameTextBox", false, commonBag.MastersNameTextBox);
				AssertVisibility("ConveyanceCountryCodeFindBox", false, commonBag.ConveyanceCountryCodeFindBox);
				AssertVisibility("VehicleRegistrationTextBox", true, commonBag.VehicleRegistrationTextBox);
				AssertVisibility("VoyageFlightTextBox", false, commonBag.VoyageFlightTextBox);
				AssertVisibility("RadioCallSignTextBox", false, commonBag.RadioCallSignTextBox);
				AssertVisibility("LloydsNumberTextBox", false, commonBag.LloydsNumberTextBox);
				AssertVisibility("Trailer1RegNoTextBox", true, commonBag.Trailer1RegNoTextBox);
				AssertVisibility("Trailer2RegNoTextBox", true, commonBag.Trailer2RegNoTextBox);
				AssertVisibility("Trailer1RegCountryCodeFindBox", true, commonBag.Trailer1RegCountryCodeFindBox);
				AssertVisibility("Trailer2RegCountryCodeFindBox", true, commonBag.Trailer2RegCountryCodeFindBox);
				AssertVisibility("MasterBOLTextBox", false, commonBag.MasterBOLTextBox);
				AssertVisibility("BuyersConsolidationCheckBox", false, commonBag.BuyersConsolidationCheckBox);
			});

			void AssertVisibility(string message, bool visibility, ControlReference controlToTest)
			{
				AssertEquals(message, visibility, layout.IsVisible(controlToTest, header));
			}
		}
	}
}
