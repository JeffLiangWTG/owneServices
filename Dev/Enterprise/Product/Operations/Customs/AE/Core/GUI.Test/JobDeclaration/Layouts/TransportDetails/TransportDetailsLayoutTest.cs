using System.Collections.Generic;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(TransportDetailsLayout))]
sealed class TransportDetailsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportDetailsLayoutBuilder<JobDeclaration>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, ControlWidthClass.Long);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, ControlWidthClass.Medium);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.VehicleRegistrationNumberTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.FlightUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.VoyageNumberTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, ControlWidthClass.Auto);
			yield return (TransportDetailsControlBag.Instance.PlaceOfDischargeDropEdit, ControlWidthClass.Auto);
		}
	}
}
