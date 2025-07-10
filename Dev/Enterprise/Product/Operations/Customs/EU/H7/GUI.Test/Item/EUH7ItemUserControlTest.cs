using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7ItemUserControl))]
	sealed class EUH7ItemUserControlTest : TestCaseWithFactory
	{
		public void TestItemGridSupplementUQColumn_CharacterCasingShouldBeUpper()
		{
			using (var control = new EUH7ItemUserControl())
			{
				var grid = control.FindSingle<ZGrid>(c => c.Name == "ItemsGrid");
				var columnInfo = grid.GetColumnStyle("API_CustomsUQ2");
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			}
		}

		[RequiresSTA]
		public void TestSetVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			using (var form = new ZForm(manifest))
			using (var control = new EUH7ItemUserControl())
			{
				control.SetDataBinding(manifest, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("ItemsGrid");

				CombineAssertions(() =>
				{
					EUH7GUITestHelper.AssertGridLayout(grid,
						[
							AsycudaPackedItem.Schema.API_FormattedTariff,
							AsycudaPackedItem.Schema.API_GoodsValue,
							AsycudaPackedItem.Schema.API_RX_NKGoodsValueCurrency,
							AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin,
							AsycudaPackedItem.Schema.API_GoodsDescription,
							AsycudaPackedItem.Schema.API_CustomsQty2,
							AsycudaPackedItem.Schema.API_CustomsUQ2,
							AsycudaPackedItem.Schema.API_GrossWeight,
							AsycudaPackedItem.Schema.API_GrossWeightUQ
						]);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_FormattedTariff);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_GoodsValue);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_RX_NKGoodsValueCurrency);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_GoodsDescription);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_CustomsQty2);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_CustomsUQ2);
				});
			}

			using (var form = new ZForm(bill))
			using (var control = new EUH7ItemUserControl())
			{
				control.SetDataBinding(bill, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("ItemsGrid");

				CombineAssertions(() =>
				{
					EUH7GUITestHelper.AssertGridLayout(grid,
						[
							AsycudaPackedItem.Schema.API_FormattedTariff,
							AsycudaPackedItem.Schema.API_GoodsValue,
							AsycudaPackedItem.Schema.API_RX_NKGoodsValueCurrency,
							AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin,
							AsycudaPackedItem.Schema.API_GoodsDescription,
							AsycudaPackedItem.Schema.API_CustomsQty2,
							AsycudaPackedItem.Schema.API_CustomsUQ2,
							AsycudaPackedItem.Schema.API_GrossWeight,
							AsycudaPackedItem.Schema.API_GrossWeightUQ
						]);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_FormattedTariff);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_GoodsValue);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_RX_NKGoodsValueCurrency);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_GoodsDescription);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_CustomsQty2);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, AsycudaPackedItem.Schema.API_CustomsUQ2);
				});
			}
		}

		public void TestAdditionalTabPages()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;

				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var itemTabpage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_EUH7ItemUserControl");
				billsAndPacksTabControl.SelectedTab = itemTabpage;

				var itemDetailsUserControl = itemTabpage.FindSingle<EUH7ItemTabControl>();

				CombineAssertions("4 additional tabs should be added", () =>
				{
					AssertNotNull(itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_EUH7ItemPacksUserControl", true).FirstOrDefault());
					AssertNotNull(itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_SupportingDocumentsUserControl", true).FirstOrDefault());
					AssertNotNull(itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_AdditionalDocumentsUserControl", true).FirstOrDefault());
					AssertNotNull(itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_PreviousDocumentsUserControl", true).FirstOrDefault());
				});
			}

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();
				var billsAndPacksTabControl = form.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var itemTabpage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_EUH7ItemUserControl");
				billsAndPacksTabControl.SelectedTab = itemTabpage;

				var itemDetailsUserControl = itemTabpage.FindSingle<EUH7ItemTabControl>();

				CombineAssertions("4 additional tabs should be added", () =>
				{
					AssertNotNull(itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_EUH7ItemPacksUserControl", true).FirstOrDefault());
					AssertNotNull(itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_SupportingDocumentsUserControl", true).FirstOrDefault());
					AssertNotNull(itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_AdditionalDocumentsUserControl", true).FirstOrDefault());
					AssertNotNull(itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_PreviousDocumentsUserControl", true).FirstOrDefault());
				});
			}
		}

		public void TestItemGridGoodsValueColumn()
		{
			using (var control = new EUH7ItemUserControl())
			{
				var grid = control.FindSingle<ZGrid>(c => c.Name == "ItemsGrid");
				var columnInfo = grid.GetColumnStyle("API_GoodsValue") as ZCalcEditColumnStyleInfo;

				CombineAssertions("", () =>
				{
					AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
					AssertEquals("Group key", "90a80f5e-aea4-44eb-8e9d-dc7259f51e32", columnInfo.GroupName.Key);
					AssertEquals("Decimals", 2, columnInfo.Decimals);
				});
			}
		}

		public void TestItemGridGrossWeightColumns()
		{
			using (var control = new EUH7ItemUserControl())
			{
				var grid = control.FindSingle<ZGrid>(c => c.Name == "ItemsGrid");
				var grossWeightColumnInfo = grid.GetColumnStyle("API_GrossWeight") as ZCalcEditColumnStyleInfo;
				var grossWeightUQColumnInfo = grid.GetColumnStyle("API_GrossWeightUQ") as ZDropEditColumnStyleInfo;

				CombineAssertions("Gross value column", () =>
				{
					AssertEquals(3, grossWeightColumnInfo.Decimals);
					Assert(grossWeightColumnInfo.IsVisible);
					AssertEquals(CharacterCasing.Upper, grossWeightUQColumnInfo.CharacterCasing);
					Assert(grossWeightUQColumnInfo.IsVisible);
				});
			}
		}

		public void TestItemGridNetWeightColumns()
		{
			using (var control = new EUH7ItemUserControl())
			{
				var grid = control.FindSingle<ZGrid>(c => c.Name == "ItemsGrid");
				var netWeightColumnInfo = grid.GetColumnStyle("API_NetWeight") as ZCalcEditColumnStyleInfo;
				var netWeightUQColumnInfo = grid.GetColumnStyle("API_NetWeightUQ") as ZDropEditColumnStyleInfo;

				CombineAssertions("Net value column", () =>
				{
					AssertEquals(3, netWeightColumnInfo.Decimals);
					Assert(!netWeightColumnInfo.IsVisible);
					AssertEquals(CharacterCasing.Upper, netWeightUQColumnInfo.CharacterCasing);
					Assert(!netWeightUQColumnInfo.IsVisible);
				});
			}
		}

		public void TestItemGridGoodsValueCurrencyColumn()
		{
			using (var control = new EUH7ItemUserControl())
			{
				var grid = control.FindSingle<ZGrid>(c => c.Name == "ItemsGrid");
				var columnInfo = grid.GetColumnStyle("API_RX_NKGoodsValueCurrency");

				CombineAssertions("", () =>
				{
					AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
					AssertEquals("Group key", "90a80f5e-aea4-44eb-8e9d-dc7259f51e32", columnInfo.GroupName.Key);
				});
			}
		}

		public void TestItemGridCustomsMessageStatusColumns()
		{
			using (var control = new EUH7ItemUserControl())
			{
				var grid = control.FindSingle<ZGrid>(c => c.Name == "ItemsGrid");
				var packStatusColumnInfo = grid.GetColumnStyle("API_PackStatus") as ZTextBoxColumnStyleInfo;
				var messageStatusColumnInfo = grid.GetColumnStyle("API_MessageStatus") as ZTextBoxColumnStyleInfo;

				CombineAssertions("pack status and message status column", () =>
				{
					Assert(!packStatusColumnInfo.IsVisible);
					Assert(!messageStatusColumnInfo.IsVisible);
				});
			}
		}

		public void TestAPI_CustomsValue()
		{
			using (var control = new EUH7ItemUserControl())
			{
				var grid = control.FindSingle<ZGrid>(c => c.Name == "ItemsGrid");
				var customsValueColumnInfo = grid.GetColumnStyle("API_CustomsValue") as ZCalcEditColumnStyleInfo;

				CombineAssertions("Customs value column", () =>
				{
					AssertEquals(3, customsValueColumnInfo.Decimals);
					Assert(!customsValueColumnInfo.IsVisible);
				});
			}
		}

		public void TestAPI_CustomsQty()
		{
			using (var control = new EUH7ItemUserControl())
			{
				var grid = control.FindSingle<ZGrid>(c => c.Name == "ItemsGrid");
				var customsQtyColumnInfo = grid.GetColumnStyle("API_CustomsQty") as ZCalcEditColumnStyleInfo;

				CombineAssertions("Customs Qty column", () =>
				{
					AssertEquals(3, customsQtyColumnInfo.Decimals);
					Assert(!customsQtyColumnInfo.IsVisible);
				});
			}
		}

		public void TestAPI_CustomsUQ()
		{
			using (var control = new EUH7ItemUserControl())
			{
				var grid = control.FindSingle<ZGrid>(c => c.Name == "ItemsGrid");
				var customsUQColumnInfo = grid.GetColumnStyle("API_CustomsUQ") as ZDropEditColumnStyleInfo;

				CombineAssertions("Customs UQ column", () =>
				{
					AssertEquals(CharacterCasing.Upper, customsUQColumnInfo.CharacterCasing);
					Assert(!customsUQColumnInfo.IsVisible);
				});
			}
		}
	}
}
