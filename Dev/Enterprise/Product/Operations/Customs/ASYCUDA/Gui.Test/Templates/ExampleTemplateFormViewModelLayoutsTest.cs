using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(ExampleTemplateFormViewModel))]
	sealed class ExampleTemplateFormViewModelLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ExampleTemplateFormViewModelBuilderForTest();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (ExampleControlBag.Instance.ManifestTypeDropEdit, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.NatureDropEdit, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.TransportModeDropEdit, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.BuyersConsolidationCheckBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.AgentTypeDropEdit, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.MasterBOLTextBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.VehicleRegistrationTextBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.VesselCodeFindBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.RadioCallSignTextBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.ConveyanceCountryCodeFindBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.MastersNameTextBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.CustomsLoadPortCodeFindBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.EstDepartureDateEdit, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.PortOfFirstArrivalCodeFindBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.CustomsDischargePortCodeFindBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.EstArrivalDateEdit, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.CarrierAddressControl, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.CarrierCodeTextBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.ManifestNumberFromMasterBillTextBox, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.IssueDateDateEdit, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.CustomsOfficeDropEdit, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.DeconsolidateAddressControl, ControlWidthClass.CustomWidth);
				yield return (ExampleControlBag.Instance.DischargeTerminalAddressControl, ControlWidthClass.CustomWidth);
			}
		}

		class ExampleTemplateFormViewModelBuilderForTest : ColumnLayoutBuilder<AsycudaManifestHeader, ControlBag>
		{
			public override ControlBag CommonBag => ExampleControlBag.Instance;

			protected override int MaxColumns => 1;
		}
	}
}
