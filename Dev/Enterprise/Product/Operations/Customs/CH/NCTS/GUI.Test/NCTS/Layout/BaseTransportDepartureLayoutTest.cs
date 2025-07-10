using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

abstract class BaseTransportDepartureLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.InlandTransportModeDropEdit, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.AircraftIDAtDepartureTextBox, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.TransportAtDepartureTypeDropEdit, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.TransportAtDepartureCountryCodeFindBox, ControlWidthClass.LongNoCaption);
			yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.VesselCountryCodeFindBox, ControlWidthClass.LongNoCaption);
		}
	}

	public void TestTestTransportAtDepartureTextBoxVisibility()
	{
		AssertControlVisibility(EU.NCTS.GUI.TransportDepartureControlBag.Instance.TransportAtDepartureTextBox,
			ModeOfTransportList.Codes._2_RailTransport,
			ModeOfTransportList.Codes._3_RoadTransport,
			ModeOfTransportList.Codes._4_AirTransport,
			ModeOfTransportList.Codes._9_OwnPropulsion);
	}

	public void TestTransportAtDepartureCountryCodeFindBoxVisibility()
	{
		AssertControlVisibility(EU.NCTS.GUI.TransportDepartureControlBag.Instance.TransportAtDepartureCountryCodeFindBox,
			ModeOfTransportList.Codes._2_RailTransport,
			ModeOfTransportList.Codes._3_RoadTransport,
			ModeOfTransportList.Codes._4_AirTransport,
			ModeOfTransportList.Codes._9_OwnPropulsion);
	}

	public void TestTransportAtDepartureTypeDropEditVisibility()
	{
		AssertControlVisibility(EU.NCTS.GUI.TransportDepartureControlBag.Instance.TransportAtDepartureTypeDropEdit, ModeOfTransportList.Codes._9_OwnPropulsion);
	}

	public void TestAircraftIDAtDepartureTextBoxVisibility()
	{
		AssertControlVisibility(EU.NCTS.GUI.TransportDepartureControlBag.Instance.AircraftIDAtDepartureTextBox, ModeOfTransportList.Codes._4_AirTransport);
	}

	public void TestVesselCodeFindBoxVisibility()
	{
		AssertControlVisibility(EU.NCTS.GUI.TransportDepartureControlBag.Instance.VesselCodeFindBox, ModeOfTransportList.Codes._8_InlandWaterwayTransport);
	}

	public void TestVesselCountryCodeFindBoxVisibility()
	{
		AssertControlVisibility(EU.NCTS.GUI.TransportDepartureControlBag.Instance.VesselCountryCodeFindBox, ModeOfTransportList.Codes._8_InlandWaterwayTransport);
	}
	
	public void TestVesselCodeFindBoxCaption() => CombineAssertions(() =>
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
		var movementHeader = nctsHeader.MovementHeader;
		var layout = ((IPanelLayoutProvider)new TransportDepartureLayout()).Layout;

		layout.TryGetCaption(TransportDepartureControlBag.Instance.VesselCodeFindBox, movementHeader, out var resourceStringData);
		AssertEquals("BM_InlandTransportMode = '8' Caption should be:", "Vessel Name / ENI Number", resourceStringData.Caption);
		AssertEquals("BM_InlandTransportMode = '8' ShortCaption should be:", "Vessel / ENI Num", resourceStringData.ShortCaption);
	});

	void AssertControlVisibility(ControlReference control, params string[] visibleForInlandTransportModes) => CombineAssertions(() =>
	{
		var layout = CreateLayout().Layout;

		foreach (var transportMode in NctsHeader.Lookups.ModeOfTransportList.GetAllCodes())
		{
			SetTransportMode(transportMode);
			AssertEquals($"InlandTransportMode = '{transportMode}'", transportMode.In(visibleForInlandTransportModes), layout.IsVisible(control, GetBusinessObject()));
		}
	});

	protected abstract IPanelLayoutProvider CreateLayout();

	protected NctsHeader NctsHeader => nctsHeader ??= CreateNctsHeader();
	NctsHeader nctsHeader;

		NctsHeader CreateNctsHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
			return nctsHeader;
		}

	protected abstract BusinessObject GetBusinessObject();

	protected abstract void SetTransportMode(string transportMode);
}
