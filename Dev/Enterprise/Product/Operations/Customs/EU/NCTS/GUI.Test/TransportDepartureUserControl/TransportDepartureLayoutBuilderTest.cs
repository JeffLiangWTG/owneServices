using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(TransportDepartureLayoutBuilder<NctsDepartureMovementHeader>))]
	sealed class TransportDepartureLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransportDepartureLayoutBuilder<NctsDepartureMovementHeader>, NctsDepartureMovementHeader, TransportDepartureControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override PanelLayoutTabSequence ExpectedTabSequence => PanelLayoutTabSequence.RowWise;

		protected override TransportDepartureLayoutBuilder<NctsDepartureMovementHeader> GetColumnLayoutBuilderForTesting() => new TransportDepartureLayoutBuilder<NctsDepartureMovementHeader>();

		public void TestTestTransportAtDepartureTextBoxVisibility()
		{
			AssertControlVisibility(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox,
				ModeOfTransportList.Codes._2_RailTransport,
				ModeOfTransportList.Codes._3_RoadTransport,
				ModeOfTransportList.Codes._4_AirTransport,
				ModeOfTransportList.Codes._7_FixedTransportInstallations,
				ModeOfTransportList.Codes._8_InlandWaterwayTransport,
				ModeOfTransportList.Codes._9_OwnPropulsion);
		}

		public void TestTransportAtDepartureCountryCodeFindBoxVisibility()
		{
			AssertControlVisibility(TransportDepartureControlBag.Instance.TransportAtDepartureCountryCodeFindBox,
				ModeOfTransportList.Codes._2_RailTransport,
				ModeOfTransportList.Codes._3_RoadTransport,
				ModeOfTransportList.Codes._4_AirTransport,
				ModeOfTransportList.Codes._7_FixedTransportInstallations,
				ModeOfTransportList.Codes._8_InlandWaterwayTransport,
				ModeOfTransportList.Codes._9_OwnPropulsion);
		}

		public void TestTransportAtDepartureTrailer1RegNoTextBoxVisibility()
		{
			AssertControlVisibility(TransportDepartureControlBag.Instance.TransportAtDepartureTrailer1RegNoTextBox, ModeOfTransportList.Codes._3_RoadTransport);
		}

		public void TestTransportAtDepartureTrailer1NationalityCodeFindBoxVisibility()
		{
			AssertControlVisibility(TransportDepartureControlBag.Instance.TransportAtDepartureTrailer1NationalityCodeFindBox, ModeOfTransportList.Codes._3_RoadTransport);
		}

		public void TestTransportAtDepartureTrailer2RegNoTextBoxVisibility()
		{
			AssertControlVisibility(TransportDepartureControlBag.Instance.TransportAtDepartureTrailer2RegNoTextBox, ModeOfTransportList.Codes._3_RoadTransport);
		}

		public void TestTransportAtDepartureTrailer2NationalityCodeFindBoxVisibility()
		{
			AssertControlVisibility(TransportDepartureControlBag.Instance.TransportAtDepartureTrailer2NationalityCodeFindBox, ModeOfTransportList.Codes._3_RoadTransport);
		}

		public void TestTransportAtDepartureTypeDropEditVisibility()
		{
			AssertControlVisibility(TransportDepartureControlBag.Instance.TransportAtDepartureTypeDropEdit,
				ModeOfTransportList.Codes._1_SeaTransport,
				ModeOfTransportList.Codes._2_RailTransport,
				ModeOfTransportList.Codes._3_RoadTransport,
				ModeOfTransportList.Codes._4_AirTransport,
				ModeOfTransportList.Codes._7_FixedTransportInstallations,
				ModeOfTransportList.Codes._8_InlandWaterwayTransport,
				ModeOfTransportList.Codes._9_OwnPropulsion);
		}

		public void TestVesselCodeFindBoxVisibility()
		{
			AssertControlVisibility(TransportDepartureControlBag.Instance.VesselCodeFindBox, ModeOfTransportList.Codes._1_SeaTransport);
		}

		public void TestVesselCountryCodeFindBoxVisibility()
		{
			AssertControlVisibility(TransportDepartureControlBag.Instance.VesselCountryCodeFindBox, ModeOfTransportList.Codes._1_SeaTransport);
		}

		public void TestAdditionalWagonNumbersButtonVisibilityDuringTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertControlVisibility(TransportDepartureControlBag.Instance.AdditionalWagonNumbersButton);
			}
		}

		public void TestAdditionalWagonNumbersButtonVisibilityOutsideTransitionPeriod()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			var layout = ((IPanelLayoutProvider)new TransportDepartureLayout()).Layout;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertEquals("TransportAtDeparture not set", false, layout.IsVisible(TransportDepartureControlBag.Instance.AdditionalWagonNumbersButton, movementHeader));

				movementHeader.BM_TransportAtDeparture = "TESTREG";
				AssertEquals("TransportAtDeparture set", true, layout.IsVisible(TransportDepartureControlBag.Instance.AdditionalWagonNumbersButton, movementHeader));
			}
		}

		public void TestTransportAtDepartureTextBoxCaption()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var movementHeader = nctsHeader.MovementHeader;
			var layout = ((IPanelLayoutProvider)new TransportDepartureLayout()).Layout;

			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
				nctsHeader.MovementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._20;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, movementHeader, out var resourceStringData);
				AssertEquals("BM_InlandTransportMode = '2', BM_TransportAtDepartureType = '20'", "Wagon Number", resourceStringData.Caption);

				nctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
				nctsHeader.MovementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._21;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, movementHeader, out resourceStringData);
				AssertEquals("BM_InlandTransportMode = '2', BM_TransportAtDepartureType = '21'", "Train Number", resourceStringData.Caption);

				nctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, movementHeader, out resourceStringData);
				AssertEquals("BM_InlandTransportMode = '3'", "Transport ID", resourceStringData.Caption);

				nctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				nctsHeader.MovementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._40;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, movementHeader, out resourceStringData);
				AssertEquals("BM_InlandTransportMode = '4', BM_TransportAtDepartureType = '40'", "Flight Number", resourceStringData.Caption);

				nctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				nctsHeader.MovementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._41;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, movementHeader, out resourceStringData);
				AssertEquals("BM_InlandTransportMode = '4', BM_TransportAtDepartureType = '41'", "Registration Number", resourceStringData.Caption);

				nctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, movementHeader, out resourceStringData);
				AssertEquals("BM_InlandTransportMode = '7'", "Transport ID", resourceStringData.Caption);

				nctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
				nctsHeader.MovementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._80;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, movementHeader, out resourceStringData);
				AssertEquals("BM_InlandTransportMode = '8', BM_TransportAtDepartureType = '80'", "ENI Code", resourceStringData.Caption);

				nctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
				nctsHeader.MovementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._81;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, movementHeader, out resourceStringData);
				AssertEquals("BM_InlandTransportMode = '8', BM_TransportAtDepartureType = '81'", "Vessel", resourceStringData.Caption);

				nctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, movementHeader, out resourceStringData);
				AssertEquals("BM_InlandTransportMode = '9'", "Transport ID", resourceStringData.Caption);
			});
		}

		public void TestVesselCodeFindBoxTextBoxCaption()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var movementHeader = nctsHeader.MovementHeader;
			var layout = ((IPanelLayoutProvider)new TransportDepartureLayout()).Layout;

			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._10;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.VesselCodeFindBox, movementHeader, out var resourceStringData);
				AssertEquals("TransportTypeAtDeparture = '_10'", "Lloyds Number", resourceStringData.Caption);

				nctsHeader.MovementHeader.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._11;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.VesselCodeFindBox, movementHeader, out resourceStringData);
				AssertEquals("TransportTypeAtDeparture = '_11'", "Vessel", resourceStringData.Caption);
			});
		}

		void AssertControlVisibility(ControlReference control, params string[] visibleForInlandTransportModes)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_TransportAtDeparture = "A";
			var layout = ((IPanelLayoutProvider)new TransportDepartureLayout()).Layout;

			CombineAssertions(() =>
			{
				foreach (var transportMode in new ModeOfTransportList().GetAllCodes())
				{
					movementHeader.BM_InlandTransportMode = transportMode;
					AssertEquals($"BM_InlandTransportMode = '{transportMode}'", transportMode.In(visibleForInlandTransportModes), layout.IsVisible(control, movementHeader));
				}
			});
		}
	}
}
