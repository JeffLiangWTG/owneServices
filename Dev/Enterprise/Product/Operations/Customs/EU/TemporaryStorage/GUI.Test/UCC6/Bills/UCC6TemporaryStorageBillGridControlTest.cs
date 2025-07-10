using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing;

sealed class UCC6TemporaryStorageBillGridControlTest : TestCaseWithFactory
{
	public void TestGridColumns()
	{
		using (var control = new UCC6TemporaryStorageBillGridControl())
		{
			var billsGrid = (ZGrid)control.Controls.Find("BillsGrid", true).First();
			var columns = billsGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable);
			AssertContainsExactElementsInExactOrder("BillsGrid Column Names",
			new[] { "ABL_Calc_IsMaster",
					"TypeOfBillDocument",
					"ABL_BillNumber",
					"ABL_UCRNumber",
					"ABL_GrossWeight",
					"ABL_GrossWeightUQ",
					"ConsignorOrgPK",
					"ABL_OA_Shipper",
					"ConsigneeOrgPK",
					"ABL_OA_Consignee",
			}, columns.Select(x => x.ColumnName));
		}
	}

	public void TestGridColumns_Transfer()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
		using (var form = new ZForm(header))
		using (var control = new UCC6TemporaryStorageBillGridControl())
		{
			form.Controls.Add(control);
			form.Show();

			var billsGrid = (ZGrid)control.Controls.Find("BillsGrid", true).First();
			var columns = billsGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable);
			AssertContainsExactElementsInExactOrder("BillsGrid Column Names when AMA_MessageType = 'TF'",
				new[] { "ABL_Calc_IsMaster",
					"TypeOfBillDocument",
					"ABL_BillNumber",
					"ABL_GrossWeight",
					"ABL_GrossWeightUQ",
			}, columns.Select(x => x.ColumnName));
		}
	}

	public void TestABL_Calc_IsMaster_IsReadOnly()
	{
		using (var control = new UCC6TemporaryStorageBillGridControl())
		{
			var billsGrid = (ZGrid)control.Controls.Find("BillsGrid", true).First();
			var isMasterColumnStyle = billsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == nameof(TemporaryStorageBill.ABL_Calc_IsMaster));
			Assert("ABL_Calc_IsMaster should be read only.", isMasterColumnStyle.IsReadOnly);
		}
	}

	public void TestDelete_RowsDeleting()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		Factory.Save();
		var billBOL = header.Bills[0];
		var bill1 = header.Bills.AddNew();
		bill1.ABL_BolType = "MWB";
		var bill2 = header.Bills.AddNew();
		bill2.ABL_BolType = "HWB";
		var bill3 = header.Bills.AddNew();
		bill3.ABL_BolType = "HWB";
		Factory.Save();

		using (var control = new UCC6TemporaryStorageBillGridControl())
		{
			control.SetDataBinding(header, string.Empty);
			var billsGrid = control.TemporaryStorageBillGrid;
			billsGrid.DataSource = header.Bills;

			AssertEquals("Prerequisite: ListManager has 4 rows at the beginnning", 4, billsGrid.ListManager.Count);

			billsGrid.Select(GetIndex(billBOL));
			billsGrid.OnDeleteKeyPressed();
			AssertEquals("Should not delete, one line selected is master", 4, billsGrid.ListManager.Count);
			billsGrid.UnSelectAll();

			billsGrid.Select(GetIndex(billBOL));
			billsGrid.OnDeleteKeyPressed();
			AssertEquals("Should not delete, one line selected is master", 4, billsGrid.ListManager.Count);
			billsGrid.UnSelectAll();

			billsGrid.Select(GetIndex(bill2));
			billsGrid.OnDeleteKeyPressed();
			AssertEquals("Should delete bill2", 3, billsGrid.ListManager.Count);
			billsGrid.UnSelectAll();

			var message = "Should delete bill1 and bill3";
			billsGrid.Select(GetIndex(bill1));
			billsGrid.Select(GetIndex(bill3));
			billsGrid.OnDeleteKeyPressed();
			AssertEquals(message, 1, billsGrid.ListManager.Count);
			billsGrid.UnSelectAll();

			int GetIndex(TemporaryStorageBill bobj)
			{
				return billsGrid.ListManager.List.IndexOf(bobj);
			}
		}
	}

	public void TestContextMenuDelete_DeleteMenuItem_Enabled()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		Factory.Save();
		var billBOL = header.Bills[0];
		var bill1 = header.Bills.AddNew();
		bill1.ABL_BolType = "MWB";
		var bill2 = header.Bills.AddNew();
		bill2.ABL_BolType = "HWB";
		Factory.Save();

		using (var control = new UCC6TemporaryStorageBillGridControl())
		{
			control.SetDataBinding(header, string.Empty);
			var billsGrid = control.TemporaryStorageBillGrid;
			billsGrid.DataSource = header.Bills;

			AssertEquals("Prerequisite: ListManager has 3 rows at the beginnning", 3, billsGrid.ListManager.Count);

			billsGrid.CurrentRowIndex = GetIndex(billBOL);
			billsGrid.ContextMenu.ShowPopupMenu();
			AssertEquals("Delete menu should not be enabled, the current row is master", false, billsGrid.DeleteMenuItem.Enabled);

			billsGrid.CurrentRowIndex = GetIndex(bill1);
			billsGrid.ContextMenu.ShowPopupMenu();
			AssertEquals("Delete menu should be enabled, the current row is not master", true, billsGrid.DeleteMenuItem.Enabled);

			billsGrid.Select(GetIndex(billBOL));
			billsGrid.ContextMenu.ShowPopupMenu();
			AssertEquals("Delete menu should not be enabled, one line selected is Master", false, billsGrid.DeleteMenuItem.Enabled);
			billsGrid.UnSelectAll();

			billsGrid.Select(GetIndex(billBOL));
			billsGrid.Select(GetIndex(bill1));
			billsGrid.ContextMenu.ShowPopupMenu();
			AssertEquals("Delete menu should not be enabled, at leaset one line selected is Master", false, billsGrid.DeleteMenuItem.Enabled);
			billsGrid.UnSelectAll();

			billsGrid.Select(GetIndex(bill1));
			billsGrid.Select(GetIndex(bill2));
			billsGrid.ContextMenu.ShowPopupMenu();
			AssertEquals("Delete menu should be enabled, no line selected is Master", true, billsGrid.DeleteMenuItem.Enabled);
			billsGrid.UnSelectAll();

			int GetIndex(TemporaryStorageBill bobj)
			{
				return billsGrid.ListManager.List.IndexOf(bobj);
			}
		}
	}
}
