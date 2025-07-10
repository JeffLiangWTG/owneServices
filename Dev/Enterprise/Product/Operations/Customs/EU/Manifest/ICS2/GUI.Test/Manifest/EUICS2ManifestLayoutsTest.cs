using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using AsycudaManifestHeader = Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	[TestedType(typeof(EUICS2ManifestLayouts))]
	sealed class EUICS2ManifestLayoutsTest : LayoutsAbstractTest
	{
		public void TestVisibility()
		{
			var manifestBag = EUICS2ManifestControlBag.Instance;

			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			AssertControlsVisibility(
				"Check controls visibility for Carrier Manifest form",
				manifest,
				new[]
				{
					manifestBag.SpecificCircumstanceIndicatorDropEdit.Name,
					manifestBag.LocalReferenceNumberTextBox.Name,
					manifestBag.MOTIdentifierTextBox.Name,
					manifestBag.MOTIdentifierTypeDropEdit.Name,
				},
				new[]
				{
					manifestBag.BranchGuidFindBox.Name
				});

			manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;

			AssertControlsVisibility(
				"Check controls visibility for Forwarder Manifest form",
				manifest,
				new[]
				{
					manifestBag.SpecificCircumstanceIndicatorDropEdit.Name,
					manifestBag.LocalReferenceNumberTextBox.Name,
					manifestBag.BranchGuidFindBox.Name
				},
				Enumerable.Empty<string>());
		}

		public void TestVesselCodeFindBoxVisibility()
		{
			var manifestBag = CommonManifestControlBag.Instance;
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			var layout = ((IPanelLayoutProvider)new EUICS2ManifestLayouts()).Layout;

			CombineAssertions("VesselCodeFindBox should have same visibilities under ApplicationCode NVC & VOC.", () =>
			{
				manifest.AMA_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("Visible for transport mode: SEA", true, layout.IsVisible(manifestBag.VesselCodeFindBox, manifest));

				manifest.AMA_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("NOT visible for other transport modes", false, layout.IsVisible(manifestBag.VesselCodeFindBox, manifest));

				manifest.AMA_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				AssertEquals("Visible for transport mode: IWT", true, layout.IsVisible(manifestBag.VesselCodeFindBox, manifest));

				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				AssertEquals("VOC & IWT, should be visible.", true, layout.IsVisible(manifestBag.VesselCodeFindBox, manifest));
			});
		}

		public void TestLloydsNumberTextBoxVisibility()
		{
			var manifestBag = CommonManifestControlBag.Instance;
			var manifest = Factory.New<AsycudaManifestHeader>();

			CombineAssertions(() =>
			{
				var layout = ((IPanelLayoutProvider)new EUICS2ManifestLayouts()).Layout;
				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				manifest.AMA_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("Visible for transport mode: SEA", true, layout.IsVisible(manifestBag.LloydsNumberTextBox, manifest));

				manifest.AMA_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("NOT visible for other transport modes", false, layout.IsVisible(manifestBag.LloydsNumberTextBox, manifest));

				manifest.AMA_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				AssertEquals("Visible for transport mode: IWT", true, layout.IsVisible(manifestBag.LloydsNumberTextBox, manifest));

				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				AssertEquals("VOC & IWT, should be visible.", true, layout.IsVisible(manifestBag.LloydsNumberTextBox, manifest));
			});
		}

		public void TestConveyanceCountryCodeFindBoxVisibility()
		{
			var manifestBag = CommonManifestControlBag.Instance;
			var manifest = Factory.New<AsycudaManifestHeader>();

			CombineAssertions(() =>
			{
				var layout = ((IPanelLayoutProvider)new EUICS2ManifestLayouts()).Layout;
				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				manifest.AMA_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("Visible for transport mode: SEA", true, layout.IsVisible(manifestBag.ConveyanceCountryCodeFindBox, manifest));

				manifest.AMA_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("NOT visible for other transport modes", false, layout.IsVisible(manifestBag.ConveyanceCountryCodeFindBox, manifest));

				manifest.AMA_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				AssertEquals("Visible for transport mode: IWT", true, layout.IsVisible(manifestBag.ConveyanceCountryCodeFindBox, manifest));

				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				AssertEquals("VOC & IWT, should be visible.", true, layout.IsVisible(manifestBag.ConveyanceCountryCodeFindBox, manifest));
			});
		}

		public void TestMastersNameTextBoxVisibility()
		{
			var manifestBag = CommonManifestControlBag.Instance;
			var manifest = Factory.New<AsycudaManifestHeader>();

			CombineAssertions(() =>
			{
				var layout = ((IPanelLayoutProvider)new EUICS2ManifestLayouts()).Layout;
				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				manifest.AMA_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("Visible for transport mode: SEA", true, layout.IsVisible(manifestBag.MastersNameTextBox, manifest));

				manifest.AMA_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("NOT visible for other transport modes", false, layout.IsVisible(manifestBag.MastersNameTextBox, manifest));

				manifest.AMA_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				AssertEquals("Visible for transport mode: IWT", true, layout.IsVisible(manifestBag.MastersNameTextBox, manifest));
			});
		}

		public void TestMeansOfTransportTypeDropEditVisibility_Forwarder()
		{
			var manifestBag = EUICS2ManifestControlBag.Instance;
			var manifest = Factory.New<AsycudaManifestHeader>();

			CombineAssertions(() =>
			{
				var layout = ((IPanelLayoutProvider)new EUICS2ManifestLayouts()).Layout;
				manifest.AMA_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("Visible for transport mode: ROA", true, layout.IsVisible(manifestBag.MeansOfTransportTypeDropEdit, manifest));

				manifest.AMA_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("NOT visible for other transport modes", false, layout.IsVisible(manifestBag.MeansOfTransportTypeDropEdit, manifest));

				manifest.AMA_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("Visible for transport mode: SEA", true, layout.IsVisible(manifestBag.MeansOfTransportTypeDropEdit, manifest));

				manifest.AMA_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				AssertEquals("Visible for transport mode: IWT", true, layout.IsVisible(manifestBag.MeansOfTransportTypeDropEdit, manifest));
			});
		}

		public void TestMeansOfTransportTypeDropEditVisibility_Carrier()
		{
			var manifestBag = EUICS2ManifestControlBag.Instance;
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;

			CombineAssertions(() =>
			{
				var layout = ((IPanelLayoutProvider)new EUICS2ManifestLayouts()).Layout;
				manifest.AMA_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("Visible for transport mode: ROA", true, layout.IsVisible(manifestBag.MeansOfTransportTypeDropEdit, manifest));

				manifest.AMA_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("Visible for transport mode: RAI", true, layout.IsVisible(manifestBag.MeansOfTransportTypeDropEdit, manifest));
			});
		}

		public void TestEstimatedDepartureEdit_DateTimeFormat()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;

			using var form = new ManifestForm(manifest);
			form.Show();

			var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
			var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
			var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
			mainTabControl.SelectedTab = mainTabPage;

			var estDepartureDateEditName = nameof(CommonManifestControlBag.Instance.EstDepartureDateEdit);
			var estDepartureDateEdit = mainTabPage.FindSingle<ZDateEdit>(c => c.Name == estDepartureDateEditName, 2);

			AssertEquals($"{estDepartureDateEditName} must allow date+time input for Forwarder manifest and Road transport", ZArchitecture.Core.ZDateTimePickerFormat.Long, estDepartureDateEdit.DateTimeFormat);
		}

		public void TestVehicleRegistrationAndCountryUserControlVisibility()
		{
			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_TransportMode = TransportModes.Road;
				AssertEquals("AMA_TransportMode ='ROA': visible", expected: true, Layout.IsVisible(EUICS2ManifestControlBag.Instance.VehicleRegistrationAndNationalityUserControl, header));

				header.AMA_TransportMode = TransportModes.Air;
				AssertEquals("AMA_TransportMode ='AIR': not visible", expected: false, Layout.IsVisible(EUICS2ManifestControlBag.Instance.VehicleRegistrationAndNationalityUserControl, header));
			});
		}

		public void TestRadioCallSignTextBoxVisibility()
		{
			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_TransportMode = TransportModes.Sea;
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				AssertEquals("AMA_TransportMode ='SEA' and IsCarrierManifest: should be visible", expected: true, Layout.IsVisible(CommonManifestControlBag.Instance.RadioCallSignTextBox, header));

				header.AMA_TransportMode = TransportModes.Air;
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				AssertEquals("AMA_TransportMode !='SEA' and IsCarrierManifest: should be NOT visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.RadioCallSignTextBox, header));

				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				AssertEquals("IsForwarderManifest: should be not visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.RadioCallSignTextBox, header));
			});
		}

		public void TestVehicleRegistrationVisibility()
		{
			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

				header.AMA_TransportMode = TransportModes.Sea;
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				AssertEquals("AMA_TransportMode ='Sea' and isCarrierManifest: visible", expected: true, Layout.IsVisible(CommonManifestControlBag.Instance.VehicleRegistrationTextBox, header));

				header.AMA_TransportMode = TransportModes.Road;
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				AssertEquals("AMA_TransportMode ='ROA' and isCarrierManifest: not visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.VehicleRegistrationTextBox, header));

				header.AMA_TransportMode = TransportModes.Air;
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				AssertEquals("AMA_TransportMode ='AIR' and IsForwarderManifest: not visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.VehicleRegistrationTextBox, header));

				header.AMA_TransportMode = TransportModes.Road;
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				AssertEquals("AMA_TransportMode ='ROA' and IsForwarderManifest: not visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.VehicleRegistrationTextBox, header));
			});
		}

		public void TestTrailersBoxesVisibility()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_TransportMode = TransportModes.Road;
			CombineAssertions(() =>
			{
				AssertEquals("AMA_TransportMode ='ROA' and isCarrierManifest: Trailer1RegNoTextBox not visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer1RegNoTextBox, header));
				AssertEquals("AMA_TransportMode ='ROA' and isCarrierManifest: Trailer1RegCountryCodeFindBox not visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer1RegCountryCodeFindBox, header));
				AssertEquals("AMA_TransportMode ='ROA' and isCarrierManifest: Trailer2RegNoTextBox not visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer2RegNoTextBox, header));
				AssertEquals("AMA_TransportMode ='ROA' and isCarrierManifest: Trailer2RegCountryCodeFindBox not visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer2RegCountryCodeFindBox, header));
			});

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_TransportMode = TransportModes.Air;
			CombineAssertions(() =>
			{
				AssertEquals("AMA_TransportMode !='ROA' and isCarrierManifest: Trailer1RegNoTextBox visible", expected: true, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer1RegNoTextBox, header));
				AssertEquals("AMA_TransportMode !='ROA' and isCarrierManifest: Trailer1RegCountryCodeFindBox visible", expected: true, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer1RegCountryCodeFindBox, header));
				AssertEquals("AMA_TransportMode !='ROA' and isCarrierManifest: Trailer2RegNoTextBox visible", expected: true, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer2RegNoTextBox, header));
				AssertEquals("AMA_TransportMode !='ROA' and isCarrierManifest: Trailer2RegCountryCodeFindBox visible", expected: true, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer2RegCountryCodeFindBox, header));
			});

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header.AMA_TransportMode = TransportModes.Road;
			CombineAssertions(() =>
			{
				AssertEquals("AMA_TransportMode ='ROA' and IsForwarderManifest: Trailer1RegNoTextBox NOT visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer1RegNoTextBox, header));
				AssertEquals("AMA_TransportMode ='ROA' and IsForwarderManifest: Trailer1RegCountryCodeFindBox NOT visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer1RegCountryCodeFindBox, header));
				AssertEquals("AMA_TransportMode ='ROA' and IsForwarderManifest: Trailer2RegNoTextBox NOT visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer2RegNoTextBox, header));
				AssertEquals("AMA_TransportMode ='ROA' and IsForwarderManifest: Trailer2RegCountryCodeFindBox NOT visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer2RegCountryCodeFindBox, header));
			});

			header.AMA_TransportMode = TransportModes.Air;
			CombineAssertions(() =>
			{
				AssertEquals("AMA_TransportMode !='ROA' and IsForwarderManifest: Trailer1RegNoTextBox NOT visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer1RegNoTextBox, header));
				AssertEquals("AMA_TransportMode !='ROA' and IsForwarderManifest: Trailer1RegCountryCodeFindBox NOT visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer1RegCountryCodeFindBox, header));
				AssertEquals("AMA_TransportMode !='ROA' and IsForwarderManifest: Trailer2RegNoTextBox NOT visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer2RegNoTextBox, header));
				AssertEquals("AMA_TransportMode !='ROA' and IsForwarderManifest: Trailer2RegCountryCodeFindBox NOT visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.Trailer2RegCountryCodeFindBox, header));
			});
		}

		public void TestReceptacleUserControlVisibility()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;

			AssertEquals("IsForwarderManifest: ReceptacleUserControl NOT visible", expected: false, Layout.IsVisible(EUICS2ManifestControlBag.Instance.ReceptacleUserControl, header));

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;

			AssertEquals("IsCarrierManifest: ReceptacleUserControl visible", expected: true, Layout.IsVisible(EUICS2ManifestControlBag.Instance.ReceptacleUserControl, header));
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EUICS2ManifestControlBag.Instance.AddressedMemberStateDropEdit, ControlWidthClass.Medium);
				yield return (EUICS2ManifestControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.RegistrationDateEdit, ControlWidthClass.Medium);
				yield return (EUICS2ManifestControlBag.Instance.RegistrationNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestTypeDropEdit, ControlWidthClass.Long);
				yield return (EUICS2ManifestControlBag.Instance.SpecificCircumstanceIndicatorDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.NatureDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.AgentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.BuyersConsolidationCheckBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (EUICS2ManifestControlBag.Instance.MOTIdentifierTypeDropEdit, ControlWidthClass.Long);
				yield return (EUICS2ManifestControlBag.Instance.MOTIdentifierTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.LloydsNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.RadioCallSignTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MastersNameTextBox, ControlWidthClass.Long);
				yield return (EUICS2ManifestControlBag.Instance.MeansOfTransportTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VehicleRegistrationTextBox, ControlWidthClass.Medium);
				yield return (EUICS2ManifestControlBag.Instance.VehicleRegistrationAndNationalityUserControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.Trailer1RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.Trailer2RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstDepartureDateEdit, ControlWidthClass.Auto);
				yield return (EUICS2ManifestControlBag.Instance.ActualDepartureDateEdit, ControlWidthClass.Auto);
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
				yield return (EUICS2ManifestControlBag.Instance.LocalReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MasterBOLTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.IssueDateDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.PaymentMethodDropEdit, ControlWidthClass.Long);
				yield return (EUICS2ManifestControlBag.Instance.TransportDocumentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.DeclarantAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierCodeTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (EUICS2ManifestControlBag.Instance.ReEntryIndicatorCheckBox, ControlWidthClass.Long);
				yield return (EUICS2ManifestControlBag.Instance.SplitConsignmentIndicatorCheckBox, ControlWidthClass.Long);
				yield return (EUICS2ManifestControlBag.Instance.PreviousMRNTextBox, ControlWidthClass.Long);
				yield return (EUICS2ManifestControlBag.Instance.OriginCodeFindBox, ControlWidthClass.Long);
				yield return (EUICS2ManifestControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (EUICS2ManifestControlBag.Instance.ReceptacleUserControl, ControlWidthClass.Long);
			}
		}

		void AssertControlsVisibility(string message, AsycudaManifestHeader manifest, IEnumerable<string> controlsExpectedToBeVisible, IEnumerable<string> controlsExpectedToBeHidden)
		{
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>("mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var manifestGroupBox = mainTabPage.FindSingle<ZGroupBox>("ManifestGroupBox");

				CombineAssertions(message, () =>
				{
					foreach (var controlName in controlsExpectedToBeVisible)
					{
						var control = manifestGroupBox.FindSingleOrDefault<Control>(controlName);
						AssertNotNull(string.Format("We expect to have control {0}, but not found", controlName), control);
						if (control != null)
						{
							AssertEquals(string.Format("We expect {0} is visible, but it's hidden", controlName), true, control.Visible);
						}
					}

					foreach (var controlName in controlsExpectedToBeHidden)
					{
						var control = manifestGroupBox.FindSingleOrDefault<Control>(controlName);
						AssertNotNull(string.Format("We expect to have control {0}, but not found", controlName), control);
						if (control != null)
						{
							AssertEquals(string.Format("We expect {0} is hidden, but it's visible", controlName), false, control.Visible);
						}
					}
				});
			}
		}

		public void TestActualDepartureDateVisibility()
		{
			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				header.AMA_TransportMode = TransportModes.Road;
				AssertEquals("AMA_TransportMode ='ROA' , SpecificCircumstanceIndicator = 'F50': visible", expected: true, Layout.IsVisible(EUICS2ManifestControlBag.Instance.ActualDepartureDateEdit, header));

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				header.AMA_TransportMode = TransportModes.Air;
				AssertEquals("AMA_TransportMode NOT 'ROA' , SpecificCircumstanceIndicator = 'F50': not visible", expected: false, Layout.IsVisible(EUICS2ManifestControlBag.Instance.ActualDepartureDateEdit, header));

				header.AMA_TransportMode = TransportModes.Road;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F10;
				AssertEquals("AMA_TransportMode = 'ROA' , SpecificCircumstanceIndicator NOT 'F50': not visible", expected: false, Layout.IsVisible(EUICS2ManifestControlBag.Instance.ActualDepartureDateEdit, header));

				header.AMA_TransportMode = TransportModes.Air;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F10;
				AssertEquals("AMA_TransportMode NOT 'ROA' , SpecificCircumstanceIndicator NOT 'F50': not visible", expected: false, Layout.IsVisible(EUICS2ManifestControlBag.Instance.ActualDepartureDateEdit, header));

				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header.AMA_TransportMode = TransportModes.Rail;
				AssertEquals("AMA_TransportMode not 'ROA' , is Carrier Manifest: not visible", expected: false, Layout.IsVisible(EUICS2ManifestControlBag.Instance.ActualDepartureDateEdit, header));

				header.AMA_TransportMode = TransportModes.Road;
				AssertEquals("AMA_TransportMode is 'ROA' , is Carrier Manifest: visible", expected: true, Layout.IsVisible(EUICS2ManifestControlBag.Instance.ActualDepartureDateEdit, header));
			});
		}

		public void TestPaymentMethodDropVisibility()
		{
			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_TransportMode = TransportModes.Road;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				AssertEquals("isCarrierManifest , AMA_TransportMode is 'ROA' , SpecificCircumstanceIndicator = 'F50': visible", expected: true, Layout.IsVisible(CommonManifestControlBag.Instance.PaymentMethodDropEdit, header));

				header.AMA_TransportMode = TransportModes.Rail;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F51;
				AssertEquals("isCarrierManifest , AMA_TransportMode is 'RAI' , SpecificCircumstanceIndicator = 'F51': visible", expected: true, Layout.IsVisible(CommonManifestControlBag.Instance.PaymentMethodDropEdit, header));

				header.AMA_TransportMode = TransportModes.Air;
				AssertEquals("isCarrierManifest , AMA_TransportMode neither 'ROA' nor 'RAI' , SpecificCircumstanceIndicator = 'F51': not visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.PaymentMethodDropEdit, header));

				header.AMA_TransportMode = TransportModes.Road;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F10;
				AssertEquals("isCarrierManifest , AMA_TransportMode is 'ROA' , SpecificCircumstanceIndicator neither 'F50' nor 'F51': not visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.PaymentMethodDropEdit, header));

				header.AMA_TransportMode = TransportModes.Road;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				AssertEquals("isForwarderManifest , AMA_TransportMode is 'ROA' , SpecificCircumstanceIndicator = 'F50': not visible", expected: false, Layout.IsVisible(CommonManifestControlBag.Instance.PaymentMethodDropEdit, header));
			});
		}

		public void TestSplitConsignmentIndicatorCheckBoxVisibility() {
			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;

				header.AMA_TransportMode = TransportModes.InlandWaterwayTransport;
				AssertEquals("Not a CarrierManifest, AMA_TransportMode ='IWT': not visible", expected: false, Layout.IsVisible(EUICS2ManifestControlBag.Instance.SplitConsignmentIndicatorCheckBox, header));

				header.AMA_TransportMode = TransportModes.Sea;
				AssertEquals("Not a CarrierManifest, AMA_TransportMode ='SEA': not visible", expected: false, Layout.IsVisible(EUICS2ManifestControlBag.Instance.SplitConsignmentIndicatorCheckBox, header));

				header.AMA_TransportMode = TransportModes.Air;
				AssertEquals("Not a CarrierManifest, AMA_TransportMode ='AIR': not visible", expected: false, Layout.IsVisible(EUICS2ManifestControlBag.Instance.SplitConsignmentIndicatorCheckBox, header));

				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;

				header.AMA_TransportMode = TransportModes.InlandWaterwayTransport;
				AssertEquals("CarrierManifest, AMA_TransportMode ='IWT': visible", expected: true, Layout.IsVisible(EUICS2ManifestControlBag.Instance.SplitConsignmentIndicatorCheckBox, header));

				header.AMA_TransportMode = TransportModes.Sea;
				AssertEquals("CarrierManifest, AMA_TransportMode ='SEA': visible", expected: true, Layout.IsVisible(EUICS2ManifestControlBag.Instance.SplitConsignmentIndicatorCheckBox, header));

				header.AMA_TransportMode = TransportModes.Air;
				AssertEquals("CarrierManifest, AMA_TransportMode ='AIR': not visible", expected: false, Layout.IsVisible(EUICS2ManifestControlBag.Instance.SplitConsignmentIndicatorCheckBox, header));
			});
		}

		public void TestOriginCodeFindBox()
		{
			CombineAssertions("OriginCodeFindBox: visible when SEA/IWT and F10/F11.", () =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header.AMA_TransportMode = TransportModes.Road;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				AssertEquals(
					message: "ROA&F50, hide.",
					expected: false,
					actual: Layout.IsVisible(EUICS2ManifestControlBag.Instance.OriginCodeFindBox, header)
				);

				header.AMA_TransportMode = TransportModes.Sea;
				AssertEquals(
					message: "SEA&F50, hide.",
					expected: false,
					actual: Layout.IsVisible(EUICS2ManifestControlBag.Instance.OriginCodeFindBox, header)
				);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
				AssertEquals(
					message: "SEA&F11, show.",
					expected: true,
					actual: Layout.IsVisible(EUICS2ManifestControlBag.Instance.OriginCodeFindBox, header)
				);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F12;
				AssertEquals(
					message: "SEA&F12, show.",
					expected: true,
					actual: Layout.IsVisible(EUICS2ManifestControlBag.Instance.OriginCodeFindBox, header)
				);

				header.AMA_TransportMode = TransportModes.InlandWaterwayTransport;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F12;
				AssertEquals(
					message: "IWT&F12, show.",
					expected: true,
					actual: Layout.IsVisible(EUICS2ManifestControlBag.Instance.OriginCodeFindBox, header)
				);
			});
		}

		public void TestFinalDestinationCodeFindBox()
		{
			CombineAssertions("FinalDestinationCodeFindBox: visible when SEA/IWT and F10/F11.", () =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header.AMA_TransportMode = TransportModes.Road;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				AssertEquals(
					message: "ROA&F50, hide.",
					expected: false,
					actual: Layout.IsVisible(EUICS2ManifestControlBag.Instance.FinalDestinationCodeFindBox, header)
				);

				header.AMA_TransportMode = TransportModes.Sea;
				AssertEquals(
					message: "SEA&F50, hide.",
					expected: false,
					actual: Layout.IsVisible(EUICS2ManifestControlBag.Instance.FinalDestinationCodeFindBox, header)
				);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
				AssertEquals(
					message: "SEA&F11, show.",
					expected: true,
					actual: Layout.IsVisible(EUICS2ManifestControlBag.Instance.FinalDestinationCodeFindBox, header)
				);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F12;
				AssertEquals(
					message: "SEA&F12, show.",
					expected: true,
					actual: Layout.IsVisible(EUICS2ManifestControlBag.Instance.FinalDestinationCodeFindBox, header)
				);

				header.AMA_TransportMode = TransportModes.InlandWaterwayTransport;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F12;
				AssertEquals(
					message: "IWT&F12, show.",
					expected: true,
					actual: Layout.IsVisible(EUICS2ManifestControlBag.Instance.FinalDestinationCodeFindBox, header)
				);
			});
		}

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new EUICS2ManifestLayouts()).Layout);
		PanelLayout layout;
	}
}
