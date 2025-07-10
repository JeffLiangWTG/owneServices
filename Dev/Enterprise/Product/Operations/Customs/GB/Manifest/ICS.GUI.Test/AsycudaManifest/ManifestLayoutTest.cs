using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.GUI;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.GUI.Testing
{
	[TestedType(typeof(ManifestLayouts))]
	sealed class ManifestLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeaderBase>();

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
				yield return (CommonManifestControlBag.Instance.EstDepartureDateEdit, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstArrivalDateEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.JobReferenceTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MasterBOLTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.IssueDateDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierCodeTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsOfficeDropEdit, ControlWidthClass.Long);
				yield return (EUManifestControlBag.Instance.SpecificCircumstanceIndicatorDropEdit, ControlWidthClass.Long);
				yield return (EUManifestControlBag.Instance.MethodOfPaymentDropEdit, ControlWidthClass.Long);
				yield return (EUManifestControlBag.Instance.ETAatFirstCustomsOfficeDateEdit, ControlWidthClass.Auto);
				yield return (EUManifestControlBag.Instance.ATAatFirstCustomsOfficeDateEdit, ControlWidthClass.Auto);
				yield return (EUManifestControlBag.Instance.EUCustomsOfficesUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		public void TestFieldsVisibility()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EUImportControlSystem, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				using (var form = new ManifestForm(manifest))
				{
					form.Show();

					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
					mainTabControl.SelectedTab = mainTabPage;

					CombineAssertions(() =>
					{
						AssertNull("SpecialMentionsDropEdit excluded", asycudaManifestUserControl.Controls.Find("SpecialMentionsDropEdit", true).FirstOrDefault());

						AssertEquals("SpecificCircumstanceIndicatorDropEdit visible", true, asycudaManifestUserControl.FindSingle<ZDropEdit>("SpecificCircumstanceIndicatorDropEdit").Visible);
						AssertEquals("MethodOfPaymentDropEdit visible", true, asycudaManifestUserControl.FindSingle<ZDropEdit>("MethodOfPaymentDropEdit").Visible);
						AssertEquals("ATAatFirstCustomsOfficeDateEdit visible", true, asycudaManifestUserControl.FindSingle<ZDateEdit>("ATAatFirstCustomsOfficeDateEdit").Visible);
						AssertEquals("ETAatFirstCustomsOfficeDateEdit visible", true, asycudaManifestUserControl.FindSingle<ZDateEdit>("ETAatFirstCustomsOfficeDateEdit").Visible);

						var customsOfficesUserControl = asycudaManifestUserControl.FindSingle<EUCountryCustomsOfficesUserControl>(nameof(EUManifestControlBag.EUCustomsOfficesUserControl));
						AssertEquals("CustomsOfficesGrid visible", true, customsOfficesUserControl.FindSingle<ZArchitecture.ZGrid>("CustomsOfficesGrid").Visible);
					});
				}
			}
		}

		public void TestHeaderAdditionalTabPages()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EUImportControlSystem, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var manifest = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
				using (var form = new ManifestForm(manifest))
				{
					form.Show();

					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
					var itineraryTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name.Contains("ItineraryForManifestHeaderUserControl"));
					mainTabControl.SelectedTab = itineraryTabPage;

					var itineraryForManifestHeaderUserControl = form.FindSingle<ItineraryForManifestHeaderUserControl>("ItineraryForManifestHeaderUserControl");
					AssertEquals("itineraryGrid visible", true, itineraryForManifestHeaderUserControl.FindSingle<ZArchitecture.ZGrid>("itineraryGrid").Visible);
				}
			}
		}
	}
}
