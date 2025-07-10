using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7ManifestLayouts))]
	sealed class EUH7ManifestLayoutsTest : LayoutsAbstractTest
	{
		public void TestMessagesTabOnBillLevel()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;

				var billsAndPacksTabControl = billsAndPacksTabPage.FindSingle<ZTabControl>("billsAndPacksTabControl");
				var messagesTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabControl_TabPage_EUH7MessagesUserControl");
				billsAndPacksTabControl.SelectedTab = messagesTabPage;

				var messagesUserControl = messagesTabPage.FindSingle<EUH7MessagesUserControl>();
				AssertNotNull(messagesUserControl);
			}
		}

		public void TestMasterBillTextBoxCaptions()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var masterBillTextBoxControl = mainTabControl.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.Instance.MasterBillTextBox));
				var captions = masterBillTextBoxControl.CaptionResourceString;
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Bill Number", captions.Caption);
					AssertEquals("MediumCaption", "Bill No.", captions.MediumCaption);
					AssertEquals("ShortCaption", "Bill No.", captions.ShortCaption);
					AssertEquals("FullDescription", "The house bill number (e.g. BOL, MAWB) used to identify the shipment associated with the declaration.", captions.FullDescription);
				});
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeader>();

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
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
				yield return (CommonManifestControlBag.Instance.CustomsOfficeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.DeclarantAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.RepresentativeAddressControl, ControlWidthClass.Long);
				yield return (EUH7ManifestControlBag.Instance.ConsolidatedStatusSeparatorUserControl, ControlWidthClass.Long);
				yield return (EUH7ManifestControlBag.Instance.ConsolidatedCustomsStatusDropEdit, ControlWidthClass.Long);
			}
		}
	}
}
