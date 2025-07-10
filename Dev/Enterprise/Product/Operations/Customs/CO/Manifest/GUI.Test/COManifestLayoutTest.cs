using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.GUI.Testing
{
	[TestedType(typeof(COManifestLayouts))]
	sealed class COManifestLayoutTest : LayoutsAbstractTest
	{
		public void TestCOManifestLayoutVisibilty()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;

				var cargoDisposition = asycudaManifestUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "CargoDispositionDropEdit");
				var travelDocumentType = asycudaManifestUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "TravelDocumentTypeDropEdit");
				var multimodal = asycudaManifestUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "MultimodalCheckBox");
				var precursors = asycudaManifestUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "PrecursorsCheckBox");
				var carriersLiability = asycudaManifestUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "CarriersLiabilityCheckBox");
				var deliveryMode = asycudaManifestUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "DeliveryModeDropEdit");

				AssertNull("LloydsNumberTextBox", asycudaManifestUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "LloydsNumberTextBox"));
				AssertNull("RadioCallSignTextBox", asycudaManifestUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "RadioCallSignTextBox"));
				AssertNull("ConveyanceCountryCodeFindBox", asycudaManifestUserControl.FindSingleOrDefault<ZCodeFindBox>(c => c.Name == "ConveyanceCountryCodeFindBox"));
				AssertNull("MastersNameTextBox", asycudaManifestUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "MastersNameTextBox"));
				AssertNull("CarrierCodeTextBox", asycudaManifestUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "CarrierCodeTextBox"));

				manifest.AMA_TransportMode = "SEA";
				AssertEquals(true, cargoDisposition.Visible);
				AssertEquals(true, travelDocumentType.Visible);
				AssertEquals(true, multimodal.Visible);
				AssertEquals(true, precursors.Visible);
				AssertEquals(true, carriersLiability.Visible);
				AssertEquals(true, deliveryMode.Visible);

				manifest.AMA_TransportMode = "AIR";
				AssertEquals(false, cargoDisposition.Visible);
				AssertEquals(false, travelDocumentType.Visible);
				AssertEquals(false, multimodal.Visible);
				AssertEquals(false, precursors.Visible);
				AssertEquals(false, carriersLiability.Visible);
				AssertEquals(false, deliveryMode.Visible);
			}
		}

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
				yield return (CommonManifestControlBag.Instance.CountryTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.RegistrationDateEdit, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.RegistrationNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.NatureDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.AgentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.BuyersConsolidationCheckBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.VehicleRegistrationTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.Trailer2RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstDepartureDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstArrivalDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.CustomsOfficeDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.JobReferenceTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MasterBOLTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.IssueDateDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.Long);
				yield return (COManifestControlBag.Instance.TravelDocumentTypeDropEdit, ControlWidthClass.Long);
				yield return (COManifestControlBag.Instance.CargoDispositionDropEdit, ControlWidthClass.Long);
				yield return (COManifestControlBag.Instance.DeliveryModeDropEdit, ControlWidthClass.Long);
				yield return (COManifestControlBag.Instance.MultimodalCheckBox, ControlWidthClass.Long);
				yield return (COManifestControlBag.Instance.PrecursorsCheckBox, ControlWidthClass.Long);
				yield return (COManifestControlBag.Instance.CarriersLiabilityCheckBox, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeader>();
	}
}
