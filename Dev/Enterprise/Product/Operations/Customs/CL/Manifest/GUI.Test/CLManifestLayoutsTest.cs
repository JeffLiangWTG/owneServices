using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.GUI.Testing
{
	[TestedType(typeof(CLManifestLayouts))]
	sealed class CLManifestLayoutsTest : LayoutsAbstractTest
	{
		public void TestCLManifestLayoutVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;

				AssertNull("CarrierCode", asycudaManifestUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "CarrierCode"));
				AssertNull("AgentTypeDropEdit", asycudaManifestUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "AgentTypeDropEdit"));
				AssertNull("RadioCallSignTextBox", asycudaManifestUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "RadioCallSignTextBox"));
				AssertNull("ConveyanceCountryCodeFindBox", asycudaManifestUserControl.FindSingleOrDefault<ZCodeFindBox>(c => c.Name == "ConveyanceCountryCodeFindBox"));
				AssertNull("MastersNameTextBox", asycudaManifestUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "MastersNameTextBox"));
				AssertNull("PortOfFirstArrival", asycudaManifestUserControl.FindSingleOrDefault<ZCodeFindBox>(c => c.Name == "PortOfFirstArrival"));
				AssertNull("CustomsOffice", asycudaManifestUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "CustomsOffice"));

				var is_tramp = asycudaManifestUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "IsTrampCheckBox");
				var transhipmentTypeDropEdit = asycudaManifestUserControl.FindSingle<ZDropEdit>(nameof(CLManifestControlBag.TranshipmentTypeDropEdit));

				manifest.AMA_TransportMode = "SEA";
				AssertEquals(true, is_tramp.Visible);
				AssertEquals(false, transhipmentTypeDropEdit.Visible);

				manifest.AMA_TransportMode = "AIR";
				AssertEquals(false, is_tramp.Visible);
				AssertEquals(true, transhipmentTypeDropEdit.Visible);

				var dynamicPanel = mainTabControl.FindSingle<DynamicLayoutPanel>("dynamicManifestDetailsPanel");
				var customsStatusDropEdit = dynamicPanel.FindSingleOrDefault<ZDropEdit>(nameof(CommonManifestControlBag.CustomsStatusDropEdit));
				var messageStatusDropEdit = dynamicPanel.FindSingleOrDefault<ZDropEdit>(nameof(CommonManifestControlBag.MessageStatusDropEdit));
				var messageStatusTextBox = dynamicPanel.FindSingleOrDefault<ZTextBox>(nameof(CommonManifestControlBag.MessageStatusTextBox));

				AssertNull(customsStatusDropEdit);
				AssertNull(messageStatusDropEdit);
				AssertNull(messageStatusTextBox);
			}
		}

		protected override int ControlBagCount => 2;

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
				yield return (CLManifestControlBag.Instance.TranshipmentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.BuyersConsolidationCheckBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (CLManifestControlBag.Instance.IsTrampCheckBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VehicleRegistrationTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.Trailer2RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstDepartureDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstArrivalDateEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.JobReferenceTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MasterBOLTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.IssueDateDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeader>();
	}
}
