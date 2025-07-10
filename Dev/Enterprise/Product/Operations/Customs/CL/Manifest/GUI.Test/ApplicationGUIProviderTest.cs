using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
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

			AssertSelectionDialogType<CLBillsSelectionDialog>(MessageSubTypeCodes.Codes.Original);
			AssertSelectionDialogType<AsycudaItemSelectionDialog>(string.Empty);
			AssertSelectionDialogType<CLBillsSelectionDialog>("Invalid Sub MessageType");
		}

		public override void TestArrivalLinesReadOnlyIsTrueByDefault()
		{
			var header = CreateNewManifest();
			AssertEquals("ArrivalLinesReadOnly should be not Read Only by default", false, ApplicationGUIProvider.GetApplicationGuiProvider(header).ArrivalLinesReadOnly());
		}

		protected override void AssertGetArrivalHeadersGridColumnAvailability(IReadOnlyDictionary<bool, string[]> columnAvailability)
		{
			AssertEquals(1, columnAvailability.Count);
		}

		protected override void AssertGetArrivalLinesGridColumnAvailability(IReadOnlyDictionary<bool, string[]> columnAvailability)
		{
			AssertEquals(1, columnAvailability.Count);
		}

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Dictionary<string, ControlReference[]> GetManifestControlGroups()
		{
			var common = CommonManifestControlBag.Instance;
			var cl = CLManifestControlBag.Instance;
			var groups = new Dictionary<string, ControlReference[]>();
			groups.Add("Sea Vessel", new[]
			{
				common.VesselCodeFindBox,
				cl.IsTrampCheckBox,
				common.LloydsNumberTextBox,
				common.VoyageFlightTextBox,
			});
			return groups;
		}

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl) };

		protected override Type ExpectedBillLayoutType => typeof(CLBillLayouts);

		protected override Type ExpectedBillPartiesLayoutType => typeof(CLBillPartiesLayouts);

		protected override void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
		{
			AssertEquals(12, columnsOrder.Length);
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
	}
}
