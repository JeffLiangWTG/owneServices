using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(ExampleControlBag))]
	sealed class ExampleControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ExampleControlBag.ManifestTypeDropEdit);
				yield return nameof(ExampleControlBag.NatureDropEdit);
				yield return nameof(ExampleControlBag.TransportModeDropEdit);
				yield return nameof(ExampleControlBag.ContainerModeDropEdit);
				yield return nameof(ExampleControlBag.BuyersConsolidationCheckBox);
				yield return nameof(ExampleControlBag.AgentTypeDropEdit);
				yield return nameof(ExampleControlBag.MasterBOLTextBox);
				yield return nameof(ExampleControlBag.VehicleRegistrationTextBox);
				yield return nameof(ExampleControlBag.VesselCodeFindBox);
				yield return nameof(ExampleControlBag.RadioCallSignTextBox);
				yield return nameof(ExampleControlBag.VoyageFlightTextBox);
				yield return nameof(ExampleControlBag.ConveyanceCountryCodeFindBox);
				yield return nameof(ExampleControlBag.MastersNameTextBox);
				yield return nameof(ExampleControlBag.PortOfLoadingCodeFindBox);
				yield return nameof(ExampleControlBag.CustomsLoadPortCodeFindBox);
				yield return nameof(ExampleControlBag.EstDepartureDateEdit);
				yield return nameof(ExampleControlBag.PortOfFirstArrivalCodeFindBox);
				yield return nameof(ExampleControlBag.PortOfDischargeCodeFindBox);
				yield return nameof(ExampleControlBag.CustomsDischargePortCodeFindBox);
				yield return nameof(ExampleControlBag.EstArrivalDateEdit);
				yield return nameof(ExampleControlBag.CarrierAddressControl);
				yield return nameof(ExampleControlBag.CarrierCodeTextBox);
				yield return nameof(ExampleControlBag.ManifestNumberFromMasterBillTextBox);
				yield return nameof(ExampleControlBag.IssueDateDateEdit);
				yield return nameof(ExampleControlBag.CustomsOfficeDropEdit);
				yield return nameof(ExampleControlBag.ShippingAgentAddressControl);
				yield return nameof(ExampleControlBag.DeconsolidateAddressControl);
				yield return nameof(ExampleControlBag.DischargeTerminalAddressControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ExampleControlBag.Instance;
	}
}
