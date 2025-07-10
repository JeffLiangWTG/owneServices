using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaArrivalsUserControlTest : TestCaseWithFactory
	{
		public void TestArrivalsUserControl()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = manifest.ArrivalHeaders.AddNew();
			var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(manifest);

			using (var form = new ZForm(manifest))
			using (var arrivalsUserControl = new AsycudaArrivalsUserControl())
			{
				form.Controls.Add(arrivalsUserControl);
				form.Show();

				var arrivalsGroupbox = arrivalsUserControl.FindSingle<ZGroupBox>("groupboxForArrivals");
				AssertEquals("Arrivals", arrivalsGroupbox.Text);

				var arrivalHeadersGrid = arrivalsGroupbox.FindSingle<ZArchitecture.ZGrid>("arrivalHeadersGrid");
				var flightNoColumn = arrivalHeadersGrid.GetColumnStyle(AsycudaArrivalHeader.Schema.ATH_VoyageFlightNo);
				AssertEquals(System.Windows.Forms.CharacterCasing.Upper, flightNoColumn.CharacterCasing);
				var referenceColumn = arrivalHeadersGrid.GetColumnStyle(AsycudaArrivalHeader.Schema.ATH_Reference) as ZArchitecture.ZTextBoxColumnStyleInfo;
				AssertEquals(System.Windows.Forms.CharacterCasing.Upper, referenceColumn.CharacterCasing);
				var etaAtDischargePortColumn = arrivalHeadersGrid.GetColumnStyle(AsycudaArrivalHeader.Schema.ATH_ETAAtDischargePort);
				AssertEquals("EtaAtDischargePort column is available", false, etaAtDischargePortColumn.IsUnavailable);
				var etaColumn = arrivalHeadersGrid.GetColumnStyle("ETAAtDischargePortForShortFormat") as ZArchitecture.ZDateEditColumnStyleInfo;
				AssertEquals(ZDateTimePickerFormat.Short, etaColumn.DateTimeFormat);
				var referenceDateColumn = arrivalHeadersGrid.GetColumnStyle(AsycudaArrivalHeader.Schema.ATH_ReferenceIssueDate) as ZArchitecture.ZDateEditColumnStyleInfo;
				AssertEquals(ZDateTimePickerFormat.Short, referenceDateColumn.DateTimeFormat);
			}
		}

		public void TestArrivalsUserControl_ArrivalHeadersGridColumnAvailability()
		{
			var gridColumnAvailability = new Dictionary<bool, string[]>();
			gridColumnAvailability.Add(false, new[] { AsycudaArrivalHeader.Schema.ATH_ETAAtDischargePort });

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = manifest.ArrivalHeaders.AddNew();
			var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(manifest);
			applicationGUIProvider.GetArrivalHeadersGridColumnAvailabilityForTesting = () => gridColumnAvailability;

			using (var form = new ZForm(manifest))
			using (var arrivalsUserControl = new AsycudaArrivalsUserControl())
			{
				form.Controls.Add(arrivalsUserControl);
				form.Show();

				var arrivalHeadersGrid = arrivalsUserControl.FindSingle<ZArchitecture.ZGrid>("arrivalHeadersGrid");
				var etaAtDischargePortColumn = arrivalHeadersGrid.GetColumnStyle(AsycudaArrivalHeader.Schema.ATH_ETAAtDischargePort);
				AssertEquals("EtaAtDischargePort column is unavailable", true, etaAtDischargePortColumn.IsUnavailable);
			}
		}

		public void TestArrivalDetails()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = manifest.ArrivalHeaders.AddNew();
			var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(manifest);

			using (var form = new ZForm(manifest))
			using (var arrivalsUserControl = new AsycudaArrivalsUserControl())
			{
				form.Controls.Add(arrivalsUserControl);
				form.Show();

				var arrivalDetailsTabControl = arrivalsUserControl.FindSingle<ZTabControl>("arrivalDetailsTabControl");
				var arrivalDetailsTab = arrivalDetailsTabControl.FindSingle<ZTabPage>("arrivalDetailsTabPage");
				Assert("arrivalDetailsTab TabVisible", arrivalDetailsTab.TabVisible);
				arrivalDetailsTabControl.SelectedTab = arrivalDetailsTab;

				var arrivalDetailsGrid = arrivalDetailsTab.FindSingle<ZArchitecture.ZGrid>("arrivalDetailsGrid");
				var lineBillNoColumn = arrivalDetailsGrid.GetColumnStyle("ATL_BillNumber");
				Assert(lineBillNoColumn.IsVisible);
				var lineQuantityColumn = arrivalDetailsGrid.GetColumnStyle(AsycudaArrivalLine.Schema.ATL_Quantity);
				Assert(lineQuantityColumn.IsVisible);
				var lineCargoStatusColumn = arrivalDetailsGrid.GetColumnStyle(AsycudaArrivalLine.Schema.ATL_CargoStatus);
				Assert(lineCargoStatusColumn.IsVisible);
				var lineReferenceColumn = arrivalDetailsGrid.GetColumnStyle(AsycudaArrivalLine.Schema.ATL_Reference);
				Assert(lineReferenceColumn.IsVisible);
				var weightColumn = arrivalDetailsGrid.GetColumnStyle(AsycudaArrivalLine.Schema.ATL_Weight);
				Assert(weightColumn.IsVisible);
				var weightUQColumn = arrivalDetailsGrid.GetColumnStyle(AsycudaArrivalLine.Schema.ATL_WeightUQ);
				Assert(weightUQColumn.IsVisible);
			}
		}
	}
}
