using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentTransportDepartureLayoutBuilder<NctsBill>))]
	sealed class HouseConsignmentTransportDepartureLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<HouseConsignmentTransportDepartureLayoutBuilder<NctsBill>, NctsBill, TransportDepartureControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override PanelLayoutTabSequence ExpectedTabSequence => PanelLayoutTabSequence.RowWise;

		public void TestTransportAtDepartureTextBoxVisibility()
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
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
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
			var bill = nctsHeader.Bills.AddNew();
			var layout = PanelLayoutForTest;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertEquals("TransportAtDeparture not set", false, layout.IsVisible(TransportDepartureControlBag.Instance.AdditionalWagonNumbersButton, bill));

				bill.TransportAtDeparture = "TESTREG";
				AssertEquals("TransportAtDeparture set", true, layout.IsVisible(TransportDepartureControlBag.Instance.AdditionalWagonNumbersButton, bill));
			}
		}

		public void TestTransportAtDepartureTextBoxCaption()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var bill = nctsHeader.Bills.AddNew();
			var layout = PanelLayoutForTest;

			CombineAssertions(() =>
			{
				bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._20;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, bill, out var resourceStringData);
				AssertEquals("Type ='20'", "Wagon No.", resourceStringData.Caption);

				bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._21;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, bill, out resourceStringData);
				AssertEquals("Type ='21'", "Train No.", resourceStringData.Caption);

				bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._40;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, bill, out resourceStringData);
				AssertEquals("Type ='40'", "Flight No.", resourceStringData.Caption);

				bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._41;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, bill, out resourceStringData);
				AssertEquals("Type ='41'", "Registration No.", resourceStringData.Caption);

				bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._80;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, bill, out resourceStringData);
				AssertEquals("Type ='80'", "ENI Code", resourceStringData.Caption);

				bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._81;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, bill, out resourceStringData);
				AssertEquals("Type ='81'", "Vessel Name", resourceStringData.Caption);
			});
		}

		void AssertControlVisibility(ControlReference control, params string[] visibleForInlandTransportModes)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var bill = nctsHeader.Bills.AddNew();
			bill.Trailer1IDAtDeparture = "A";
			var movementHeader = nctsHeader.MovementHeader;
			var layout = PanelLayoutForTest;

			CombineAssertions(() =>
			{
				foreach (var transportMode in new ModeOfTransportList().GetAllCodes())
				{
					movementHeader.BM_InlandTransportMode = transportMode;
					AssertEquals($"BM_InlandTransportMode = '{transportMode}'", transportMode.In(visibleForInlandTransportModes), layout.IsVisible(control, bill));
				}
			});
		}

		public void TestVesselCodeFindBoxCaption()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var bill = nctsHeader.Bills.AddNew();
			var layout = PanelLayoutForTest;

			CombineAssertions(() =>
			{
				bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._10;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.VesselCodeFindBox, bill, out var resourceStringData);
				AssertEquals("TransportTypeAtDeparture = '_10'", "Lloyds No.", resourceStringData.Caption);

				bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._11;
				layout.TryGetCaption(TransportDepartureControlBag.Instance.VesselCodeFindBox, bill, out resourceStringData);
				AssertEquals("TransportTypeAtDeparture = '_11'", "Vessel", resourceStringData.Caption);
			});
		}

		protected override HouseConsignmentTransportDepartureLayoutBuilder<NctsBill> GetColumnLayoutBuilderForTesting() => new HouseConsignmentTransportDepartureLayoutBuilder<NctsBill>();

		PanelLayout PanelLayoutForTest => ((IPanelLayoutProvider)new HouseConsignmentTransportDepartureLayout()).Layout;
	}
}
