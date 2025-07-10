using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl) };

		protected override Type ExpectedBillLayoutType => typeof(MXBillLayouts);

		protected override Dictionary<string, ControlReference[]> GetManifestControlGroups()
		{
			var common = CommonManifestControlBag.Instance;
			var groups = new Dictionary<string, ControlReference[]>();
			groups.Add("Sea Vessel", new[]
			{
				common.VesselCodeFindBox,
				common.LloydsNumberTextBox,
				common.VoyageFlightTextBox,
				common.RadioCallSignTextBox,
				common.ConveyanceCountryCodeFindBox
			});
			return groups;
		}

		public void TestGetNewAsycudaItemSelectionDialogCore()
		{
			var header = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);

			void AssertSelectionDialogType<T>(string subMessageType)
			{
				var messageChooser = header.GetNewMessageChooser(items, subMessageType, false);
				using (var dialog = provider.GetNewAsycudaItemSelectionDialog(messageChooser, string.Empty, subMessageType))
				{
					AssertType<T>($"Should be {typeof(T).Name} when the sub message type is {subMessageType}.", dialog);
				}
			}

			AssertSelectionDialogType<BillsSelectionDialog>(MessageSubTypeCodes.Codes.Original);
			AssertSelectionDialogType<AsycudaItemSelectionDialog>(string.Empty);
			AssertSelectionDialogType<BillsSelectionDialog>("Invalid Sub MessageType");
		}

		public void TestBillsGridColumnAvailability()
		{
			var manifest = CreateNewManifest();
			var bill = manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				AssertEquals("ABL_BolType IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BolType).IsUnavailable);
			}
		}

		public void TestPacksGridColumnAvailability()
		{
			var manifest = CreateNewManifest();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var packsTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
				billsAndPacksTabControl.SelectedTab = packsTabPage;
				var asycudaPackUserControl = packsTabPage.FindSingleOrDefault<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
				var packsGrid = asycudaPackUserControl.FindSingle<ZGrid>(c => c.Name == "PacksGrid");

				AssertEquals("UNDGSubstanceManagerValue IsUnavailable", true, packsGrid.GetColumnStyle(UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue).IsUnavailable);
				AssertEquals("UNDGClassManagerValue IsUnavailable", true, packsGrid.GetColumnStyle(UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue).IsUnavailable);
			}

			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var packsTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
				billsAndPacksTabControl.SelectedTab = packsTabPage;
				var asycudaPackUserControl = packsTabPage.FindSingleOrDefault<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
				var packsGrid = asycudaPackUserControl.FindSingle<ZGrid>(c => c.Name == "PacksGrid");

				AssertEquals("UNDGSubstanceManagerValue IsAvailable", false, packsGrid.GetColumnStyle(UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue).IsUnavailable);
				AssertEquals("UNDGClassManagerValue IsAvailable", false, packsGrid.GetColumnStyle(UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue).IsUnavailable);
			}
		}

		public void TestCheckPacksGridColumnAvailability()
		{
			var manifest = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(manifest);
			AssertEquals("ShouldCheckPacksGridColumnAvailability is true for Mexico", true, provider.ShouldCheckPacksGridColumnAvailability());
		}

		protected override void AssertGetPacksGridColumnAvailability(IReadOnlyDictionary<bool, string[]> result, AsycudaManifestHeader header)
		{
			AssertEquals(1, result.Count);
			AssertEquals("UNDGSubstanceManagerValue", "UNDGs+UNDGSubstanceManager+Value", result.Values.First()[0]);
			AssertEquals("UNDGClassManagerValue", "UNDGs+UNDGClassManager+Value", result.Values.First()[1]);
		}
	}
}
