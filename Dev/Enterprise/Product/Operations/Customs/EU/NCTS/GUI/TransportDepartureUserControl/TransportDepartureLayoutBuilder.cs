using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class TransportDepartureLayoutBuilder<T> : ColumnLayoutBuilder<T, TransportDepartureControlBag> where T : BusinessObject, IDepartureTransportMeansProvider
	{
		public override TransportDepartureControlBag CommonBag => TransportDepartureControlBag.Instance;

		protected override PanelLayoutTabSequence TabSequence => PanelLayoutTabSequence.RowWise;

		protected override int MaxColumns => 2;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetControlVisibility(CommonBag.TransportAtDepartureTypeDropEdit,
				ModeOfTransportList.Codes._1_SeaTransport,
				ModeOfTransportList.Codes._2_RailTransport,
				ModeOfTransportList.Codes._3_RoadTransport,
				ModeOfTransportList.Codes._4_AirTransport,
				ModeOfTransportList.Codes._7_FixedTransportInstallations,
				ModeOfTransportList.Codes._8_InlandWaterwayTransport,
				ModeOfTransportList.Codes._9_OwnPropulsion);
			SetControlVisibility(CommonBag.TransportAtDepartureTextBox,
				ModeOfTransportList.Codes._2_RailTransport,
				ModeOfTransportList.Codes._3_RoadTransport,
				ModeOfTransportList.Codes._4_AirTransport,
				ModeOfTransportList.Codes._7_FixedTransportInstallations,
				ModeOfTransportList.Codes._8_InlandWaterwayTransport,
				ModeOfTransportList.Codes._9_OwnPropulsion);
			SetControlVisibility(CommonBag.TransportAtDepartureCountryCodeFindBox,
				ModeOfTransportList.Codes._2_RailTransport,
				ModeOfTransportList.Codes._3_RoadTransport,
				ModeOfTransportList.Codes._4_AirTransport,
				ModeOfTransportList.Codes._7_FixedTransportInstallations,
				ModeOfTransportList.Codes._8_InlandWaterwayTransport,
				ModeOfTransportList.Codes._9_OwnPropulsion);
			SetControlVisibility(CommonBag.TransportAtDepartureTrailer1RegNoTextBox, ModeOfTransportList.Codes._3_RoadTransport);
			SetControlVisibility(CommonBag.TransportAtDepartureTrailer1NationalityCodeFindBox, ModeOfTransportList.Codes._3_RoadTransport);
			SetControlVisibility(CommonBag.TransportAtDepartureTrailer2RegNoTextBox, ModeOfTransportList.Codes._3_RoadTransport);
			SetControlVisibility(CommonBag.TransportAtDepartureTrailer2NationalityCodeFindBox, ModeOfTransportList.Codes._3_RoadTransport);
			SetControlVisibility(CommonBag.VesselCodeFindBox, ModeOfTransportList.Codes._1_SeaTransport);
			SetControlVisibility(CommonBag.VesselCountryCodeFindBox, ModeOfTransportList.Codes._1_SeaTransport);
			SetAdditionalWagonNumbersButtonVisibility(CommonBag.AdditionalWagonNumbersButton);
		}

		protected override void SetDefaultCaptions()
		{
			base.SetDefaultCaptions();

			SetCaption(CommonBag.TransportAtDepartureTextBox, GetTransportAtDepartureTextBoxCaption, b => b.InlandTransportModeAtDepartureInfo, h => h.TransportTypeAtDepartureInfo);
			SetCaption(CommonBag.VesselCodeFindBox, GetVesselCodeFindBoxCaption, h => h.InlandTransportModeAtDepartureInfo);
		}

		void SetControlVisibility(ControlReference control, params ZString[] visibleForInlandTransportModes)
		{
			SetVisibility(control, b => b.InlandTransportModeAtDeparture.In(visibleForInlandTransportModes), b => b.InlandTransportModeAtDepartureInfo);
		}

		void SetAdditionalWagonNumbersButtonVisibility(ControlReference control)
		{
			SetVisibility(control,
				b => b.InlandTransportModeAtDeparture == ModeOfTransportList.Codes._2_RailTransport && !b.TransportAtDeparture.IsEmpty && !IsInPhase5TransitionPeriod(b),
				b => b.InlandTransportModeAtDepartureInfo, b => b.TransportAtDepartureInfo);

			static bool IsInPhase5TransitionPeriod(IDepartureTransportMeansProvider provider) => provider.IsInPhase5TransitionPeriod;
		}

		ResourceStringData GetTransportAtDepartureTextBoxCaption(IDepartureTransportMeansProvider header)
		{
			switch (header.InlandTransportModeAtDeparture)
			{
				case ModeOfTransportList.Codes._2_RailTransport:
					return header.TransportTypeAtDeparture == NctsTransportTypeOfIdList.Codes._20
						? Res.GetData("ED216D85-1791-4E20-B4CE-2A796A06B51F", "Wagon No.", "Wagon Number", "")
						: Res.GetData("8370A2F7-9818-45B0-9FEE-CD3CF4D20703", "Train No.", "Train Number", "");
				case ModeOfTransportList.Codes._3_RoadTransport:
				case ModeOfTransportList.Codes._7_FixedTransportInstallations:
				case ModeOfTransportList.Codes._9_OwnPropulsion:
					return Res.GetData("30C8977A-206B-489A-B227-2BAAF88D0FE7", "Transport ID");
				case ModeOfTransportList.Codes._4_AirTransport:
					return header.TransportTypeAtDeparture == NctsTransportTypeOfIdList.Codes._40
						? Res.GetData("7696DB11-206C-474E-92D0-2DE5D66A1D20", "Flight No.", "Flight Number", "")
						: Res.GetData("97AE09D1-B010-4E58-826D-354AB0EE6BE6", "Registration No.", "Registration Number", "");
				case ModeOfTransportList.Codes._8_InlandWaterwayTransport:
					return header.TransportTypeAtDeparture == NctsTransportTypeOfIdList.Codes._80
						? Res.GetData("544CB77F-1966-46A3-B3CC-1F0AEE8564F2", "ENI Code")
						: Res.GetData("8938DC45-3429-4503-867C-E42964E711D5", "Vessel");
				default:
					return new ResourceStringData();
			}
		}

		ResourceStringData GetVesselCodeFindBoxCaption(IDepartureTransportMeansProvider departureMovementHeader) =>
			departureMovementHeader.TransportTypeAtDeparture == NctsTransportTypeOfIdList.Codes._10
				? Res.GetData("6B5B56D4-FA68-4BB9-84B6-90FB488041BB", "Lloyds No.", "Lloyds Number", "")
				: Res.GetData("AC238251-85FD-4B16-9E57-B67464B8451B", "Vessel");
	}
}
