using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	public class UCC6TemporaryStoragePackedItemControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new UCC6TemporaryStoragePackedItemControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("PackedItemTabControl");
				AssertNotNull(tabControl);

				AssertEquals("Tab Pages count", 6, tabControl.TabCount);

				AssertContainsExactElementsInExactOrder("Tab Pages names", new[]
				{
					"PackedItemDetailsTabPage",
					"PackPackedItemPivotTabPage",
					"SupportingDocumentsTabPage",
					"PreviousDocumentsTabPage",
					"AdditionalInfoTabPage",
					"SupplyChainActorTabPage"
				}, tabControl.TabPages.ToList<ZTabPage>().Select(x => x.Name));
			}
		}

		public void TestSplitterSize()
		{
			using (var control = new UCC6TemporaryStoragePackedItemControl())
			{
				var splitter = control.FindSingle<KSplitter>("Splitter");
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(1289, 5, true), splitter.Size);
			}
		}

		public void TestPackedItemTabControlSize()
		{
			using (var control = new UCC6TemporaryStoragePackedItemControl())
			{
				var packedItemTabControl = control.FindSingle<ZTemplateTabControl>("PackedItemTabControl");
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(1289, 280, true), packedItemTabControl.Size);
			}
		}

		public void TestSupplyChainActorTabPage()
		{
			using (var control = new UCC6TemporaryStoragePackedItemControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("PackedItemTabControl");
				var supplyChainActorTabPage = tabControl.GetTabPage("SupplyChainActorTabPage");
				var supplyChainActorTabUserControl = supplyChainActorTabPage.FindSingle<SupplyChainActorTabUserControl>("SupplyChainActorTabUserControl");
				AssertNotNull(supplyChainActorTabUserControl);
			}
		}

		public void TestItemPackPackedItemPivotTabPage()
		{
			using (var control = new UCC6TemporaryStoragePackedItemControl())
			{
				var packPackedItemPivotTabPage = control.FindSingle<ZTabPage>("PackPackedItemPivotTabPage");
				AssertNotNull(packPackedItemPivotTabPage);

				var ucc6BillsPackedItemPackingPivotControl = control.FindSingle<ZUserControl>("UCC6BillsPackedItemPackingPivotControl");
				AssertNotNull(ucc6BillsPackedItemPackingPivotControl);
			}
		}

		public void TestPreviousDocumentsTabPage()
		{
			using (var control = new UCC6TemporaryStoragePackedItemControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("PackedItemTabControl");
				var previousDocumentsTabPage = tabControl.GetTabPage("PreviousDocumentsTabPage");
				var previousDocumentsLayoutPanel = previousDocumentsTabPage.FindSingle<DynamicLayoutPanel>("PreviousDocumentsLayoutPanel");
				AssertNotNull(previousDocumentsLayoutPanel);
			}
		}
	}
}
