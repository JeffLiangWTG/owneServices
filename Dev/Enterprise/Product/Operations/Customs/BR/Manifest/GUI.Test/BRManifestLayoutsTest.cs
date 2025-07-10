using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.BR.Manifest.Business;
using Enterprise.Customs.BR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.BR.Manifest.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.BR.Manifest.GUI.Testing
{
	[TestedType(typeof(BRManifestLayouts))]
	sealed class BRManifestLayoutTest : LayoutsAbstractTest
	{
		public void TestMasterUCRTextBoxVisible()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var masterUCRTextBox = asycudaManifestUserControl.Controls.Find("CustomsOwnNumberTextBox", true).FirstOrDefault();
				AssertEquals(true, masterUCRTextBox.Visible);
			}
		}

		public void TestMercanteTextBoxCaption()
		{
			using (BRCustomsDataRegistry.Instance.EnableMercanteSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.FillWithValidTestData();
				header.AMA_ManifestType = "MER";

				using (var form = new ManifestForm(header))
				{
					form.Show();

					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
					mainTabControl.SelectedTab = mainTabPage;

					var masterUCRTextBox = asycudaManifestUserControl.Controls.Find("CustomsOwnNumberTextBox", true).FirstOrDefault();
					AssertEquals("CE Merchant", masterUCRTextBox.GetExtension<LabelCaptionRenderer>().Caption);
				}
			}
		}

		public void TestDischargeTerminalAddressControlVisibleMURC()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.FillWithValidTestData();
			manifest.AMA_ManifestType = BRManifestTypes.Codes.MUCR;

			using (var form = new ManifestForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;

				var dischargeTerminalAddressControl = asycudaManifestUserControl.Controls.Find("DischargeTerminalAddressControl", true).FirstOrDefault();
				AssertEquals(true, dischargeTerminalAddressControl.Visible);
			}
		}

		public void TestDischargeTerminalAddressControlNotVisibleMER()
		{
			using (BRCustomsDataRegistry.Instance.EnableMercanteSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.FillWithValidTestData();
				header.AMA_ManifestType = "MER";

				using (var form = new ManifestForm(header))
				{
					form.Show();

					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
					mainTabControl.SelectedTab = mainTabPage;

					var dischargeTerminalAddressControl = asycudaManifestUserControl.Controls.Find("DischargeTerminalAddressControl", true).FirstOrDefault();
					AssertEquals(false, dischargeTerminalAddressControl.Visible);
				}
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
				yield return (CommonManifestControlBag.Instance.AgentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.BuyersConsolidationCheckBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.LloydsNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.RadioCallSignTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VehicleRegistrationTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer2RegNoTextBox, ControlWidthClass.Medium);
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
				yield return (BRManifestControlBag.Instance.CustomsOwnNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.IssueDateDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierCodeTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.DeconsolidateAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.DischargeTerminalAddressControl, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeader>();
	}
}
