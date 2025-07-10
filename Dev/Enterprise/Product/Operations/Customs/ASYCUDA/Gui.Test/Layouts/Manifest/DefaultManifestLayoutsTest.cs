using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(DefaultManifestLayouts))]
	sealed class DefaultManifestLayoutsTest : LayoutsAbstractTest
	{
		public void TestConveyanceCountryCodeVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			var er = helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Eritrea, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(er.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");
			var wcoDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, parent: wcoDataGrouping);
			Factory.Save();

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var conveyanceCountryCodeFindBox = asycudaManifestUserControl.Controls.Find("ConveyanceCountryCodeFindBox", true).FirstOrDefault();

				CombineAssertions("ConveyanceCountryCodeVisibility", () =>
				{
					manifest.AMA_TransportMode = "ROA";
					AssertEquals("ConveyanceCountryCodeFindBox visible the ROA", false, conveyanceCountryCodeFindBox.Visible);

					manifest.AMA_TransportMode = "SEA";
					AssertEquals("ConveyanceCountryCodeFindBox visible the SEA", true, conveyanceCountryCodeFindBox.Visible);

					manifest.AMA_TransportMode = "AIR";
					AssertEquals("ConveyanceCountryCodeFindBox visible the AIR", true, conveyanceCountryCodeFindBox.Visible);
				});
			}
		}

		public void TestCustomsStatusDropEditMustHaveExplicitBindTo()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var layout1 = (IPanelLayoutProvider)new ManifestLayoutsWithCustomsStatusDropEdit();
			var layout2 = (IPanelLayoutProvider)new ManifestLayoutsWithoutCustomsStatusDropEdit();
			var bag = CommonManifestControlBag.Instance;

			using (var form = new ZForm(header))
			using (var panel = new DynamicLayoutPanel())
			{
				form.Controls.Add(panel);
				panel.Width = 300;
				panel.UpdateLayout(layout1);
				form.Show();

				var dropEdit = panel.FindSingle<ZDropEdit>(c => c.Name == nameof(bag.CustomsStatusDropEdit), 1);

				CombineAssertions("BindTo need be set explicitly due to the missing BindTo while Add/Remove control during update Layout will cause Binding ZDropEdit Description error", () =>
				{
					AssertEquals("layout1 include CustomsStatusDropEdit and be on panel", nameof(AsycudaManifestHeader.RegistrationStatus), dropEdit.BindTo);

					panel.UpdateLayout(layout2);
					AssertEquals("layout2: will remove CustomsStatusDropEdit from panel and BinTo should still exist", nameof(AsycudaManifestHeader.RegistrationStatus), dropEdit.BindTo);

					panel.UpdateLayout(layout1);
					AssertEquals("layout1 include CustomsStatusDropEdit and add it to panel again", nameof(AsycudaManifestHeader.RegistrationStatus), dropEdit.BindTo);
				});
			}
		}

		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
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
				yield return (CommonManifestControlBag.Instance.MastersNameTextBox, ControlWidthClass.Long);
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
				yield return (CommonManifestControlBag.Instance.CarrierCodeTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.Long);
			}
		}

		sealed class ManifestLayoutsWithCustomsStatusDropEdit : IPanelLayoutProvider
		{
			PanelLayout ManifestDetails { get; }

			PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

			public ManifestLayoutsWithCustomsStatusDropEdit()
			{
				ManifestDetails = CreateManifestDetailsLayout();
			}

			PanelLayout CreateManifestDetailsLayout()
			{
				var builder = new ManifestLayoutBuilder<AsycudaManifestHeader>();
				var common = builder.CommonBag;
				builder.AddColumn();
				builder.Add(common.CustomsStatusDropEdit, ControlWidthClass.Auto);
				return builder.Build();
			}
		}

		sealed class ManifestLayoutsWithoutCustomsStatusDropEdit : IPanelLayoutProvider
		{
			PanelLayout ManifestDetails { get; }

			PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

			public ManifestLayoutsWithoutCustomsStatusDropEdit()
			{
				ManifestDetails = CreateManifestDetailsLayout();
			}

			PanelLayout CreateManifestDetailsLayout()
			{
				var builder = new ManifestLayoutBuilder<AsycudaManifestHeader>();
				var common = builder.CommonBag;
				builder.AddColumn();
				builder.Add(common.CountryTextBox, ControlWidthClass.Auto);
				return builder.Build();
			}
		}
	}
}
