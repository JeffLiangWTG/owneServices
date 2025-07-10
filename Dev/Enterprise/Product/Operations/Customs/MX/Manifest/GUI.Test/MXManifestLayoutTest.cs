using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.GUI.Testing
{
	[TestedType(typeof(MXManifestlLayouts))]
	sealed class MXManifestLayoutTest : LayoutsAbstractTest
	{
		public void TestMXCountrySpecificFieldsVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.FillWithValidTestData();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var lloydsNumber = asycudaManifestUserControl.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.LloydsNumberTextBox));
				var lastForeignPortCodeFindBox = asycudaManifestUserControl.Controls.Find("LastForeignPortCodeFindBox", true).FirstOrDefault();
				var agentType = asycudaManifestUserControl.Controls.Find("AgentTypeDropEdit", true).FirstOrDefault();
				var customsOffice = asycudaManifestUserControl.FindSingle<ZDropEdit>(nameof(CommonManifestControlBag.CustomsOfficeDropEdit));
				var registrationDateEdit = asycudaManifestUserControl.FindSingle<ZDateEdit>(nameof(CommonManifestControlBag.RegistrationDateEdit));
				var registrationNumberTextBox = asycudaManifestUserControl.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.RegistrationNumberTextBox));
				var customsStatusDropEdit = asycudaManifestUserControl.FindSingle<ZDropEdit>(nameof(CommonManifestControlBag.CustomsStatusDropEdit));

				CombineAssertions(() =>
				{
					manifest.AMA_TransportMode = "SEA";
					AssertEquals(true, lloydsNumber.Visible);
					AssertEquals(true, agentType.Visible);
					AssertEquals(false, lastForeignPortCodeFindBox.Visible);
					AssertEquals(true, customsOffice.Visible);
					AssertEquals(true, registrationDateEdit.Visible);
					AssertEquals(true, registrationNumberTextBox.Visible);
					AssertEquals(true, customsStatusDropEdit.Visible);

					manifest.AMA_Nature = "IMP";
					AssertEquals(true, lastForeignPortCodeFindBox.Visible);

					manifest.AMA_TransportMode = "AIR";
					AssertEquals(false, lloydsNumber.Visible);
					AssertEquals(false, lastForeignPortCodeFindBox.Visible);
					AssertEquals(false, agentType.Visible);
					AssertEquals(false, customsOffice.Visible);
					AssertEquals(false, registrationDateEdit.Visible);
					AssertEquals(false, registrationNumberTextBox.Visible);
					AssertEquals(false, customsStatusDropEdit.Visible);
				});
			}
		}

		public void TestCustomsPortVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.FillWithValidTestData();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var dischargePort = asycudaManifestUserControl.FindSingle<ZCodeFindBox>(nameof(CommonManifestControlBag.CustomsDischargePortCodeFindBox));
				var loadPort = asycudaManifestUserControl.FindSingle<ZCodeFindBox>(nameof(CommonManifestControlBag.CustomsLoadPortCodeFindBox));
				CombineAssertions(() =>
				{
					manifest.AMA_TransportMode = "SEA";
					AssertEquals(true, dischargePort.Visible);
					AssertEquals(true, loadPort.Visible);

					manifest.AMA_TransportMode = "AIR";
					AssertEquals(false, dischargePort.Visible);
					AssertEquals(false, loadPort.Visible);
				});
			}
		}

		public void TestRegistrationDetailsVisible()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.FillWithValidTestData();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var registrationNumber = asycudaManifestUserControl.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.RegistrationNumberTextBox));
				var registrationDate = asycudaManifestUserControl.FindSingle<ZDateEdit>(nameof(CommonManifestControlBag.RegistrationDateEdit));

				CombineAssertions(() =>
				{
					manifest.AMA_TransportMode = "SEA";
					AssertEquals(true, registrationNumber.Visible);
					AssertEquals(true, registrationDate.Visible);

					manifest.AMA_TransportMode = "AIR";
					AssertEquals(false, registrationNumber.Visible);
					AssertEquals(false, registrationDate.Visible);
				});
			}
		}

		public void TestCustomsStatusVisible()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.FillWithValidTestData();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var customsStatusDropEdit = asycudaManifestUserControl.FindSingle<ZDropEdit>(nameof(CommonManifestControlBag.CustomsStatusDropEdit));

				CombineAssertions(() =>
				{
					manifest.AMA_TransportMode = "SEA";
					AssertEquals(true, customsStatusDropEdit.Visible);

					manifest.AMA_TransportMode = "AIR";
					AssertEquals(false, customsStatusDropEdit.Visible);
				});
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
				yield return (CommonManifestControlBag.Instance.LloydsNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.RadioCallSignTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MastersNameTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VehicleRegistrationTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.Trailer2RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsLoadPortCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstDepartureDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsDischargePortCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstArrivalDateEdit, ControlWidthClass.Auto);
				yield return (MXManifestControlBag.Instance.LastForeignPortCodeFindBox, ControlWidthClass.Auto);
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
				yield return (CommonManifestControlBag.Instance.CarrierCodeTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<Business.AsycudaManifestHeader>();
	}
}
