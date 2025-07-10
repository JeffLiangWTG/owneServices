using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using ModeOfTransportList = Enterprise.Customs.EU.Business.ModeOfTransportList;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class HouseConsignmentTransportDepartureLayoutBuilder<T> : ColumnLayoutBuilder<T, TransportDepartureControlBag> where T : NctsBill
	{
		protected override int MaxColumns => 2;

		public override TransportDepartureControlBag CommonBag => TransportDepartureControlBag.Instance;

		protected override PanelLayoutTabSequence TabSequence => PanelLayoutTabSequence.RowWise;

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
			SetControlVisibility(CommonBag.PlaceHolder2Label,
				ModeOfTransportList.Codes._1_SeaTransport,
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
			SetAdditionalWagonNumbersButtonVisibility(CommonBag.AdditionalWagonNumbersButton, ModeOfTransportList.Codes._2_RailTransport);
		}

		protected override void SetDefaultCaptions()
		{
			base.SetDefaultCaptions();

			SetCaption(CommonBag.TransportAtDepartureTextBox, GetTransportAtDepartureTextBoxCaption, b => b.TransportTypeAtDepartureInfo);
			SetCaption(CommonBag.VesselCodeFindBox, GetVesselCodeFindBoxCaption, b => b.TransportTypeAtDepartureInfo);
		}

		void SetControlVisibility(ControlReference control, params ZString[] visibleForInlandTransportModes)
		{
			SetVisibility(control, b => b.InlandTransportModeAtDeparture.In(visibleForInlandTransportModes), b => b.InlandTransportModeAtDepartureInfo);
		}

		void SetAdditionalWagonNumbersButtonVisibility(ControlReference control, params ZString[] visibleForInlandTransportModes)
		{
			SetVisibility(control, b => b.InlandTransportModeAtDeparture.In(visibleForInlandTransportModes) && !b.TransportAtDeparture.IsEmpty && !IsInPhase5TransitionPeriod(b), b => b.InlandTransportModeAtDepartureInfo, b => b.TransportAtDepartureInfo);

			static bool IsInPhase5TransitionPeriod(NctsBill bill) => bill.Header?.IsInPhase5TransitionPeriod ?? false;
		}

		ResourceStringData GetTransportAtDepartureTextBoxCaption(NctsBill bill)
		{
			switch (bill.TransportTypeAtDeparture)
			{
				case NctsTransportTypeOfIdList.Codes._20:
					return Res.GetData("276CBC95-714C-4D79-B40E-28EFA70A1915", "Wagon No.");
				case NctsTransportTypeOfIdList.Codes._21:
					return Res.GetData("8BB58E38-8F4C-462D-8D22-DCD3F9FC9F9F", "Train No.");
				case NctsTransportTypeOfIdList.Codes._40:
					return Res.GetData("3E54D9A8-1AC6-4692-8A9E-E2C7B36D486C", "Flight No.");
				case NctsTransportTypeOfIdList.Codes._41:
					return Res.GetData("F4891019-5C69-46E8-BC63-0E999CDC8BAA", "Registration No.");
				case NctsTransportTypeOfIdList.Codes._80:
					return Res.GetData("BD63E7CA-FD7C-43B2-A0C2-D42FFEED950B", "ENI Code");
				case NctsTransportTypeOfIdList.Codes._81:
					return Res.GetData("0ECE3B73-9370-4591-83A4-CFC81D2ABD06", "Vessel Name");
				default:
					return new ResourceStringData();
			}
		}

		ResourceStringData GetVesselCodeFindBoxCaption(NctsBill bill)
		{
			switch (bill.TransportTypeAtDeparture)
			{
				case NctsTransportTypeOfIdList.Codes._10:
					return Res.GetData("0B13C916-6B67-4F9E-8499-F7979718537C", "Lloyds No.");
				case NctsTransportTypeOfIdList.Codes._11:
					return Res.GetData("9986835A-5492-4C27-969C-E2AD4E0F985F", "Vessel");
				default:
					return new ResourceStringData();
			}
		}
	}
}
