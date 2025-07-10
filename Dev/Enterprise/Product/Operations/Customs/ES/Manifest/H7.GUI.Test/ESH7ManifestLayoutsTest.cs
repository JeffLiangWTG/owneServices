using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(ESH7ManifestLayouts))]
	sealed class ESH7ManifestLayoutsTest : LayoutsAbstractTest
	{
		public void TestMastersNameTextBoxVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();

			manifest.AMA_TransportMode = "AIR";
			AssertEquals("Master name text box is visible", true, LayoutForTesting.IsVisible(CommonManifestControlBag.Instance.MastersNameTextBox, manifest));

			manifest.AMA_TransportMode = "SEA";
			AssertEquals("Master name text box is visible", true, LayoutForTesting.IsVisible(CommonManifestControlBag.Instance.MastersNameTextBox, manifest));
		}

		protected override int ControlBagCount => 3;

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.CountryTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.LloydsNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.RadioCallSignTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
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
				yield return (CommonManifestControlBag.Instance.ActArrivalDateEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.JobReferenceTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MasterBillTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.DeclarantAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.RepresentativeAddressControl, ControlWidthClass.Long);
				yield return (EUH7ManifestControlBag.Instance.PresenterAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.AgentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MastersNameTextBox, ControlWidthClass.Long);
				yield return (ESH7ManifestControlBag.Instance.EntryLineNumberTextBox, ControlWidthClass.Long);
				yield return (ESH7ManifestControlBag.Instance.TransportDocumentReferenceTextBox, ControlWidthClass.Long);
				yield return (ESH7ManifestControlBag.Instance.TransportDocumentTypeDropEdit, ControlWidthClass.Long);
				yield return (ESH7ManifestControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
				yield return (ESH7ManifestControlBag.Instance.G3MRNToRevokeDropEdit, ControlWidthClass.Long);
				yield return (EUH7ManifestControlBag.Instance.CustomsOfficeLabel, ControlWidthClass.LongNoCaption);
				yield return (EUH7ManifestControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (EUH7ManifestControlBag.Instance.PresentationOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (ESH7ManifestControlBag.Instance.CusAgentCodeFindBox, ControlWidthClass.Long);
				yield return (ESH7ManifestControlBag.Instance.CertificateDropEdit, ControlWidthClass.Long);
				yield return (ESH7ManifestControlBag.Instance.TrainingCheckBox, ControlWidthClass.Long);
				yield return (EUH7ManifestControlBag.Instance.ConsolidatedStatusSeparatorUserControl, ControlWidthClass.Long);
				yield return (EUH7ManifestControlBag.Instance.ConsolidatedCustomsStatusDropEdit, ControlWidthClass.Long);
			}
		}
	}
}
