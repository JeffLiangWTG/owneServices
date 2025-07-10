using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.ZArchitecture.GUI;
using ModeOfTransportList = Enterprise.Customs.EU.Business.ModeOfTransportList;

namespace Enterprise.Customs.CH.NCTS.GUI;

public abstract class BaseTransportDepartureLayout<TDepartureTransportMeansProvider> : IPanelLayoutProvider where TDepartureTransportMeansProvider : BusinessObject, IDepartureTransportMeansProvider
{
	public BaseTransportDepartureLayout()
	{
		Layout = CreateTransportDepartureLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	PanelLayout CreateTransportDepartureLayout()
	{
		var builder = new EU.NCTS.GUI.TransportDepartureLayoutBuilder<TDepartureTransportMeansProvider>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.InlandTransportModeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.AircraftIDAtDepartureTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportAtDepartureTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TransportAtDepartureTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.VesselCodeFindBox, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(commonBag.TransportAtDepartureCountryCodeFindBox, ControlWidthClass.LongNoCaption, commonBag.TransportAtDepartureTextBox);
		builder.Add(commonBag.VesselCountryCodeFindBox, ControlWidthClass.LongNoCaption, commonBag.VesselCodeFindBox);

		SetControlVisibility(builder, commonBag.VesselCodeFindBox, ModeOfTransportList.Codes._8_InlandWaterwayTransport);
		SetControlVisibility(builder, commonBag.VesselCountryCodeFindBox, ModeOfTransportList.Codes._8_InlandWaterwayTransport);

		SetControlVisibility(builder, commonBag.AircraftIDAtDepartureTextBox, ModeOfTransportList.Codes._4_AirTransport);
		SetControlVisibility(builder, commonBag.TransportAtDepartureTextBox,
			ModeOfTransportList.Codes._2_RailTransport,
			ModeOfTransportList.Codes._3_RoadTransport,
			ModeOfTransportList.Codes._4_AirTransport,
			ModeOfTransportList.Codes._9_OwnPropulsion);
		SetControlVisibility(builder, commonBag.TransportAtDepartureCountryCodeFindBox,
			ModeOfTransportList.Codes._2_RailTransport,
			ModeOfTransportList.Codes._3_RoadTransport,
			ModeOfTransportList.Codes._4_AirTransport,
			ModeOfTransportList.Codes._9_OwnPropulsion);
		SetControlVisibility(builder, commonBag.TransportAtDepartureTypeDropEdit, ModeOfTransportList.Codes._9_OwnPropulsion);

		builder.SetCaption(commonBag.VesselCodeFindBox, _ => Res.GetData("E1EE4776-61C5-4018-843C-9371AECB20A6", "Vessel / ENI Num", "Vessel Name / ENI Number", ""));

		return builder.Build();
	}

	void SetControlVisibility(EU.NCTS.GUI.TransportDepartureLayoutBuilder<TDepartureTransportMeansProvider> builder, ControlReference control, params ZString[] visibleForInlandTransportModes)
	{
		builder.SetVisibility(control, h => h.InlandTransportModeAtDeparture.In(visibleForInlandTransportModes), h => h.InlandTransportModeAtDepartureInfo);
	}
}
