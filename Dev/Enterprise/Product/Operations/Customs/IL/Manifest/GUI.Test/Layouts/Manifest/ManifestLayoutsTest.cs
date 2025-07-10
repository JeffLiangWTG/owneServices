using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.IL.Manifest.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	[TestedType(typeof(ManifestLayouts))]
	sealed class ManifestLayoutsTest : LayoutsAbstractTest
	{
		public void TestCountrySpecificFieldsVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var manifestNumberFromMasterBillTextBox = asycudaManifestUserControl.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.ManifestNumberFromMasterBillTextBox));

				CombineAssertions("CountrySpecificFieldsVisibility", () =>
				{
					manifest.AMA_TransportMode = "ROA";
					AssertEquals("ManifestNumberFromMasterBillTextBox visible, when is not SEA or AIR", false, manifestNumberFromMasterBillTextBox.Visible);
					manifest.AMA_TransportMode = "SEA";
					AssertEquals("ManifestNumberFromMasterBillTextBox visible, when SEA", true, manifestNumberFromMasterBillTextBox.Visible);
					manifest.AMA_TransportMode = "AIR";
					AssertEquals("ManifestNumberFromMasterBillTextBox visible, when AIR", true, manifestNumberFromMasterBillTextBox.Visible);
				});
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

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.CountryTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.NatureDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.DeclarantAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ActArrivalDateEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.JobReferenceTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.RegistrationNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.RegistrationDateEdit, ControlWidthClass.Medium);
			}
		}
	}
}
