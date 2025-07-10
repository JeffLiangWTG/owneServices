using CargoWise.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	abstract class UnloadingDifferencesLayoutAbstractTest : LayoutsAbstractTest
	{
		public void AssertControlVisibility(ControlReference control, params string[] visibleForInlandTransportModes)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			var layout = GetNewPanelLayoutProvider().Layout;

			CombineAssertions(() =>
			{
				foreach (var transportMode in new ModeOfTransportList().GetAllCodes())
				{
					movementHeader.BM_InlandTransportMode = transportMode;
					AssertEquals($"BM_InlandTransportMode = '{transportMode}'", transportMode.In(visibleForInlandTransportModes), layout.IsVisible(control, movementHeader));
				}
			});
		}

		public void AssertTransportIDTextBoxCaption(ControlReference control)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var layout = GetNewPanelLayoutProvider().Layout;
			var movementHeader = nctsHeader.ArrivalMovementHeader;

			CombineAssertions(() =>
			{
				movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
				layout.TryGetCaption(control, movementHeader, out var resourceStringData);
				AssertEquals("BM_InlandTransportMode = '2'", "Train Number", resourceStringData.Caption);

				movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
				layout.TryGetCaption(control, movementHeader, out resourceStringData);
				AssertEquals("BM_InlandTransportMode = '3'", "Transport ID", resourceStringData.Caption);

				movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				layout.TryGetCaption(control, movementHeader, out resourceStringData);
				AssertEquals("BM_InlandTransportMode = '4'", "Flight Number", resourceStringData.Caption);

				movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
				layout.TryGetCaption(control, movementHeader, out resourceStringData);
				AssertEquals("BM_InlandTransportMode = '9'", "Transport ID", resourceStringData.Caption);
			});
		}

		public void AssertTrailer1RegNoTextBoxCaption(ControlReference control)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var layout = GetNewPanelLayoutProvider().Layout;
			var movementHeader = nctsHeader.ArrivalMovementHeader;

			CombineAssertions(() =>
			{
				movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
				layout.TryGetCaption(control, movementHeader, out var resourceStringData);
				AssertEquals("BM_InlandTransportMode = '2'", "Wagon Number", resourceStringData.Caption);

				movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
				layout.TryGetCaption(control, movementHeader, out resourceStringData);
				AssertEquals("BM_InlandTransportMode = '3'", "Trailer 1 ID", resourceStringData.Caption);
			});
		}
	}
}
