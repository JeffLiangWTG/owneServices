using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZDataGridTest : WebControlTest
	{
		#region Test Post Data Changes Events

		public void TestOnPostDataChanged()
		{
			TestGrid.PostDataChanged += new EventHandler(OnPostDataChangedTest);
			Assert(!OnPostDataChangedHasBeenCalled);

			TestGrid.OnPostDataChanged(this, new EventArgs());
			Assert(OnPostDataChangedHasBeenCalled);
		}

		public void TestRaisePostDataChangedEvent()
		{
			TestGrid.PostDataChanged += new EventHandler(OnPostDataChangedTest);
			Assert(!OnPostDataChangedHasBeenCalled);

			TestGrid.RaisePostDataChangedEvent();
			Assert(OnPostDataChangedHasBeenCalled);
		}

		bool OnPostDataChangedHasBeenCalled;

		void OnPostDataChangedTest(object sender, EventArgs e)
		{
			OnPostDataChangedHasBeenCalled = true;
		}

		#endregion

		#region Test Properties

		public void TestIncludeItemDataRefKey()
		{
			Assert(!TestGrid.IncludeItemDataRefKey);

			TestGrid.IncludeItemDataRefKey = true;
			Assert(TestGrid.IncludeItemDataRefKey);
		}

		public void TestCaption()
		{
			AssertEquals(ZString.Empty, TestGrid.Caption);

			TestGrid.Caption = "Test1";
			AssertEquals("Test1", TestGrid.Caption);

			TestGrid.Caption = "Test2";
			AssertEquals("Test2", TestGrid.Caption);
		}

		public void TestCssClass()
		{
			AssertEquals(ZString.Empty, TestGrid.CssClass);

			TestGrid.CssClass = "Test1";
			AssertEquals("Test1", TestGrid.CssClass);

			TestGrid.CssClass = "Test2";
			AssertEquals("Test2", TestGrid.CssClass);
		}

		public void TestInitialRowsToDisplay()
		{
			AssertEquals(0, TestGrid.InitialRowsToDisplay);

			TestGrid.InitialRowsToDisplay = 2;
			AssertEquals(2, TestGrid.InitialRowsToDisplay);

			TestGrid.InitialRowsToDisplay = 5;
			AssertEquals(5, TestGrid.InitialRowsToDisplay);
		}

		#endregion

		#region TestMaxRowsLimitation Status Labelling

		public void TestMaxRowsLimitationInNONVirtualMode()
		{
			TestGrid.AllowPaging = true;

			DummyBusinessObject otherBizO = Factory.New<DummyBusinessObject>();

			otherBizO.Collection.Load();
			otherBizO.Collection.RemoveAndDeleteAll();
			TestGrid.BindTo = "Collection";
			TestGrid.Columns.Add(new ZTextEditColumn("Test", DummyChildBusinessObject.Schema.Z0_Code));
			TestGrid.Bind(otherBizO);
			AssertEquals("StatusLabel should show expected row count", Res.GetString("8f76304f-2687-4ecd-943f-d046a5eac49b", "No records found."), TestGrid.StatusLabel.Text);

			otherBizO.Collection.AddNew();
			otherBizO.Collection.AddNew();
			otherBizO.Collection.AddNew();
			TestGrid.Bind(otherBizO);

			string expStatusMessage = Res.GetString("8eff11fc-e2f4-455f-9076-7990ca0dc24d", "Found {0} record(s).", 3);
			AssertEquals("StatusLabel should show expected row count", expStatusMessage, TestGrid.StatusLabel.Text);
		}

		#endregion TestMaxRowsLimitation Status Labelling

		#region TestPrepareControlHierarchy

		public void TestPrepareControlHierarchy()
		{
			AssertEquals("PreCondition: Controls should be empty", 0, TestGrid.Controls.Count);
			TestGrid.Bind(TestBizO.Collection);
			TestGrid.PrepareControlHierarchyInternal();
			AssertEquals("No Data Found Control should have been added", 1, TestGrid.Controls.Count);
			AssertSame("StatusPanel", TestGrid.StatusPanel, TestGrid.Controls[0]);
			AssertEquals("StatusLabel", Res.GetString("54a8dfcb-0122-4e1a-ba64-b24e3c047c63", "No Data Found"), TestGrid.StatusLabel.Text);
		}

		#endregion TestPrepareControlHierarchy

		#region TestInitializeItem

		public virtual void TestInitializeItem()
		{
			DataGridItem testItem = new DataGridItem(0, 0, ListItemType.Footer);
			BoundColumn[] testColumns = new BoundColumn[4];
			testColumns[0] = new BoundColumn();
			testColumns[1] = new BoundColumn();
			testColumns[2] = new BoundColumn();
			testColumns[3] = new BoundColumn();

			AssertEquals("PreCondition: AllowAdd", false, TestGrid.AllowAdd);
			AssertEquals("PreCondition: AllowPaging", false, TestGrid.AllowPaging);
			AssertEquals("PreCondition: Collection", null, TestGrid.Collection);
			TestGrid.InitializeItemInternal(testItem, testColumns);
			AssertEquals("Footer", 1, testItem.Cells.Count);
			AssertEquals("Footer Colspan", 4, testItem.Cells[0].ColumnSpan);
			AssertEquals("Cell should contain single control", 1, testItem.Cells[0].Controls.Count);
			AssertCollectionContains("Cells Controls should contain StatusPanel", TestGrid.StatusPanel, testItem.Cells[0].Controls);
		}

		#endregion TestInitializeItem

		#region TestDisplaysFirstPageOnBind

		public void TestDisplaysFirstPageOnBind()
		{
			TestGrid.AllowPaging = true;
			TestGrid.PageSize = 10;

			CreateLotsOfChildren(TestBizO, 30);
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, TestBizO.PK);
			TestBizO.Collection.Load(filter);

			TestGrid.BindTo = "Collection";
			TestGrid.Bind(TestBizO);

			AssertEquals("PageCount", 3, TestGrid.PageCount);
			TestGrid.SetCurrentPageIndex(2);

			TestBizO.Collection.RemoveAll();
			filter = new ZQuery(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, TestBizO.PK);
			filter.MaximumRows = 10;
			TestBizO.Collection.Load(filter);
			TestGrid.MaxRows = 10;

			AssertEquals("Collection Count", 10, TestBizO.Collection.Count);
			AssertEquals("CurrentPageIndex should be on last page", 2, TestGrid.CurrentPageIndex);
			TestGrid.Bind(TestBizO);
			AssertEquals("CurrentPageIndex should have been reset", 0, TestGrid.CurrentPageIndex);
			AssertEquals("StatusMessage should indicate limitation", Res.GetString("4da6042b-d40b-42d5-a93d-6c4b2ffa3346", "Search returned {0} or more records. Please review your search criteria.", 10), TestGrid.StatusLabel.Text);
		}

		#endregion TestDisplaysFirstPageOnBind

		#region TestBinding

		public void TestIsBindable()
		{
			AssertEquals("PreCondition: Empty BindTo", "", TestGrid.BindTo);
			AssertEquals("IsBindable (Empty BindTo and null datasource)", false, TestGrid.IsBindable(null));
			AssertEquals("IsBindable (Empty BindTo and Not IBusinessObjectCollection)", false, TestGrid.IsBindable(TestBizO));
			AssertEquals("IsBindable (Empty BindTo and IBusinessObjectCollection)", true, TestGrid.IsBindable(TestBizO.Collection));
			TestGrid.BindTo = "Collection";
			AssertEquals("IsBindable (BindTo set and non IBusinessObjectCollection)", true, TestGrid.IsBindable(TestBizO));
		}

		public void TestUnBind()
		{
			AssertNull("PreCondition: null datasource", TestGrid.DataSource);
			TestGrid.DataSource = TestBizO.Collection;
			TestGrid.UnBind();
			AssertNull("Datasource should be nulled", TestGrid.DataSource);
		}

		public void TestBinding()
		{
			CreateLotsOfChildren(TestBizO, 10);
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, TestBizO.PK);
			TestBizO.Collection.Load(filter);
			TestGrid.AllowPaging = true;

			AssertEquals("PreCondition: IsBindable should be false", false, TestGrid.IsBindable(TestBizO));
			TestGrid.Bind(TestBizO);
			AssertNull("Grid Collection", TestGrid.Collection);
			AssertEquals("CurrentPageIndex", 0, TestGrid.CurrentPageIndex);
			AssertEquals("PageCount", 0, TestGrid.PageCount);
			AssertEquals("IsBound should be true", false, TestGrid.IsBound);
			AssertEquals("StatusLabel text should not have been set", "", TestGrid.StatusLabel.Text);

			TestGrid.BindTo = "Collection";
			TestGrid.Bind(TestBizO);
			AssertEquals("Paging", true, TestGrid.AllowPaging);
			AssertSame("Grid Collection", TestBizO.Collection, TestGrid.Collection);
			AssertEquals("IsBound should be true", true, TestGrid.IsBound);
			AssertEquals("CurrentPageIndex", 0, TestGrid.CurrentPageIndex);
			AssertEquals("PageCount", 1, TestGrid.PageCount);
			AssertEquals("StatusLabel", Res.GetString("8eff11fc-e2f4-455f-9076-7990ca0dc24d", "Found {0} record(s).", 10), TestGrid.StatusLabel.Text);
		}

		#endregion TestBindable

		#region TestGetCollectionIndex

		public void TestGetCollectionIndex()
		{
			MasterFiles.Business.RefCurrencyCollection collection = new MasterFiles.Business.RefCurrencyCollection(Factory);
			TestGrid.DataSource = collection;
			Assert("at least two items in the colleciton", collection.Count >= 2);
			TestGrid.PageSize = collection.Count - 1;
			DataGridItem item = new DataGridItem(0, 0, ListItemType.Item);
			DataGridCommandEventArgs e = new DataGridCommandEventArgs(item, collection, new CommandEventArgs("test", null));
			TestGrid.SetCurrentPageIndex(1);
			AssertEquals(collection.Count - 1, TestGrid.GetCollectionIndex(e));
		}
		#endregion

		#region TestGetBusinessObjectForItemCommand
		public void TestGetBusinessObjectForItemCommand()
		{
			MasterFiles.Business.RefCurrencyCollection collection = new MasterFiles.Business.RefCurrencyCollection(Factory);
			TestGrid.DataSource = collection;
			Assert("at least two items in the colleciton", collection.Count >= 2);
			TestGrid.PageSize = collection.Count - 1;
			DataGridItem item = new DataGridItem(0, 0, ListItemType.Item);
			DataGridCommandEventArgs e = new DataGridCommandEventArgs(item, collection, new CommandEventArgs("test", null));
			TestGrid.SetCurrentPageIndex(1);
			AssertEquals(collection[collection.Count - 1], TestGrid.GetBusinessObjectForItemCommand(e));
		}
		#endregion

		#region Test Exceptions

		protected Exception ThrownException;
		protected string ThrownCommand = "";

		void TestGrid_CommandError(string operation, Exception ex)
		{
			ThrownCommand = operation;
			ThrownException = ex;
		}

		public void TestCommandErrorOnDelete()
		{
			AssertNull("PreCondition: null datasource", TestGrid.DataSource);
			TestGrid.Bind(TestBizO.Collection);
			Exception expectedException = null;
			try
			{
				TestGrid.OnDeleteCommandInternal(new DataGridCommandEventArgs(new DataGridItem(0, 0, ListItemType.EditItem), null, new CommandEventArgs("", null)));
				Assert("Shouldn't reach here", false);
			}
			catch (Exception ex)
			{
				expectedException = ex;
				Assert("exception needs to b thrown", true);
			}
			TestGrid.CommandError += new CommandErrorHandler(TestGrid_CommandError);
			ThrownException = null;
			TestGrid.OnDeleteCommandInternal(new DataGridCommandEventArgs(new DataGridItem(0, 0, ListItemType.EditItem), null, new CommandEventArgs("", null)));
			AssertEquals("Message should contain the same exception", expectedException.Message, ThrownException.Message);
			AssertEquals("Message should contain the same exception", "Delete", ThrownCommand);
		}

		public void TestCommandErrorOnCancel()
		{
			AssertNull("PreCondition: null datasource", TestGrid.DataSource);
			TestGrid.DataSource = null;
			Exception expectedException = null;
			try
			{
				TestGrid.RowAdded = true;
				TestGrid.OnCancelCommandInternal(new DataGridCommandEventArgs(new DataGridItem(0, 0, ListItemType.EditItem), null, new CommandEventArgs("", null)));
				Assert("Shouldn't reach here", false);
			}
			catch (Exception ex)
			{
				expectedException = ex;
				Assert("exception needs to b thrown", true);
			}
			TestGrid.CommandError += new CommandErrorHandler(TestGrid_CommandError);
			ThrownException = null;
			TestGrid.RowAdded = true;
			TestGrid.OnCancelCommandInternal(new DataGridCommandEventArgs(new DataGridItem(0, 0, ListItemType.EditItem), null, new CommandEventArgs("", null)));
			AssertEquals("Message should contain the same exception", expectedException.Message, ThrownException.Message);
			AssertEquals("Message should contain the same exception", "Cancel", ThrownCommand);
		}

		#endregion

		#region TestCanDelete

		public void TestCanDelete()
		{
			TestGrid.CanDeleteCommand += new DataGridDeleteHandler(TestGrid_CanDeleteCommand);
			DummyChildBusinessObject child1 = TestBizO.Collection.AddNew();
			DummyChildBusinessObject child2 = TestBizO.Collection.AddNew();
			AssertEquals("Collection Count", 2, TestBizO.Collection.Count);
			TestGrid.Bind(TestBizO.Collection);

			CanDataGridDelete = false;
			TestGrid.OnDeleteCommandInternal(new DataGridCommandEventArgs(new DataGridItem(0, 0, ListItemType.EditItem), null, new CommandEventArgs("", null)));
			AssertEquals("Collection should still contain 2 elements", 2, TestBizO.Collection.Count);

			CanDataGridDelete = true;
			TestGrid.OnDeleteCommandInternal(new DataGridCommandEventArgs(new DataGridItem(0, 0, ListItemType.EditItem), null, new CommandEventArgs("", null)));
			AssertEquals("Collection should now contain only 1 element", 1, TestBizO.Collection.Count);
			AssertCollectionNotContains("Child 1 should have been removed", child1, TestBizO.Collection);
			AssertCollectionContains("Child 2 should still be in the collection", child2, TestBizO.Collection);
		}

		void TestGrid_CanDeleteCommand(DataGridDeleteEventArgs e)
		{
			e.CanDelete = CanDataGridDelete;
		}
		bool CanDataGridDelete;

		#endregion

		#region TestAllowPaging

		public void TestAllowPaging()
		{
			ZDataGrid testGrid = new ZDataGrid();
			testGrid.PageSize = 25;
			testGrid.AllowPaging = true;

			AssertEquals("The AllowPaging property must not change the page size.", 25, testGrid.PageSize);
		}

		#endregion

		#region Test MultilineSelection

		public void TestAllowMultilineSelection()
		{
			ZDataGrid testGrid = new ZDataGrid();
			AssertEquals("Disabled by default", false, testGrid.AllowMultiLineSelection);

			testGrid.AllowMultiLineSelection = true;
			AssertEquals("Value as assigned", true, testGrid.AllowMultiLineSelection);
		}

		public void TestSelectRow()
		{
			AssertEquals("Nothing selected", "", TestGrid.SelectedKeys);
			TestGrid.AllowMultiLineSelection = true;
			AssertEquals("Value as assigned", true, TestGrid.AllowMultiLineSelection);
			AssertNull("No DataSource specified", TestGrid.DataSource);

			ZGuid pk = ZGuid.NewZGuid();

			TestGrid.SelectRow(pk, true);
			AssertEquals("Should be selected even when there is no DataSource", pk.ToString(), TestGrid.SelectedKeys);

			TestGrid.BindTo = "Collection";
			TestBizO.Collection.AddNew();
			TestBizO.Collection.AddNew();
			TestGrid.Bind(TestBizO);
			AssertNotNull("DataSource assigneded", TestGrid.DataSource);

			TestGrid.SelectRow(pk, false);
			AssertEquals("Should be successfully unselected", "", TestGrid.SelectedKeys);

			TestGrid.SelectRow(null, true);
			AssertEquals("Nothing selected because key is null", "", TestGrid.SelectedKeys);

			TestGrid.SelectRow(pk, true);
			AssertEquals("One item should be selected", pk.ToString(), TestGrid.SelectedKeys);

			ZGuid pk2 = ZGuid.NewZGuid();

			TestGrid.SelectRow(pk2, true);
			AssertEquals("Two items should be selected", string.Format("{0},{1}", pk, pk2), TestGrid.SelectedKeys);

			TestGrid.SelectRow(pk, false);
			AssertEquals("One item should be selected", pk2.ToString(), TestGrid.SelectedKeys);

			TestGrid.SelectRow(pk2, false);
			AssertEquals("Nothing should be selected", "", TestGrid.SelectedKeys);
		}

		public void TestIsRowSelected()
		{
			AssertEquals("Nothing selected", "", TestGrid.SelectedKeys);
			TestGrid.AllowMultiLineSelection = true;
			AssertEquals("Value as assigned", true, TestGrid.AllowMultiLineSelection);
			AssertNull("No DataSource specified", TestGrid.DataSource);

			ZGuid pk = ZGuid.NewZGuid();

			Assert("IsRowSelected should be False", !TestGrid.IsRowSelected(pk));

			TestGrid.SelectRow(pk, true);
			AssertEquals("Should be selected even when there is no DataSource", pk.ToString(), TestGrid.SelectedKeys);
			Assert("IsRowSelected should be True for selected key", TestGrid.IsRowSelected(pk));
			Assert("IsRowSelected should be False for an unselected key", !TestGrid.IsRowSelected(ZGuid.NewZGuid()));
		}

		public void TestClearMultiLineSelection()
		{
			AssertEquals("Nothing selected", "", TestGrid.SelectedKeys);
			TestGrid.AllowMultiLineSelection = true;

			ZGuid pk = ZGuid.NewZGuid();

			TestGrid.SelectRow(pk, true);
			AssertEquals("PK should be selected", pk.ToString(), TestGrid.SelectedKeys);

			TestGrid.ClearMultiLineSelection();
			AssertEquals("Nothing should be selected", "", TestGrid.SelectedKeys);
		}

		public void TestDataBindDoesNotClearMultilineSelection()
		{
			AssertEquals("Nothing selected", "", TestGrid.SelectedKeys);
			TestGrid.AllowMultiLineSelection = true;

			TestGrid.BindTo = "Collection";
			TestBizO.Collection.AddNew();
			TestBizO.Collection.AddNew();
			TestGrid.Bind(TestBizO);

			ZGuid pk = ZGuid.NewZGuid();

			TestGrid.SelectRow(pk, true);
			AssertEquals("Key should be selected", pk.ToString(), TestGrid.SelectedKeys);

			TestGrid.DataBind();
			AssertEquals("Key should be selected after binding", pk.ToString(), TestGrid.SelectedKeys);
		}

		public void TestChangingPageDoesNotClearMultilineSelection()
		{
			AssertEquals("Nothing selected", "", TestGrid.SelectedKeys);
			TestGrid.AllowMultiLineSelection = true;
			TestGrid.AllowPaging = true;
			TestGrid.PageSize = 2;

			TestGrid.BindTo = "Collection";
			TestBizO.Collection.AddNew();
			TestBizO.Collection.AddNew();
			TestBizO.Collection.AddNew();
			TestGrid.Bind(TestBizO);

			ZGuid pk = ZGuid.NewZGuid();

			TestGrid.SelectRow(pk, true);
			AssertEquals("Key should be selected", pk.ToString(), TestGrid.SelectedKeys);

			TestGrid.SetCurrentPageIndex(1);
			AssertEquals("Key should remain selected", pk.ToString(), TestGrid.SelectedKeys);
			TestGrid.SetCurrentPageIndex(0);
			AssertEquals("Key should remain selected", pk.ToString(), TestGrid.SelectedKeys);
		}

		public void TestMultilineSelectionAddsSelectionColumn()
		{
			TestGrid.AllowMultiLineSelection = true;
			AssertEquals("Precondition - no columns", 0, TestGrid.Columns.Count);
			TestGrid.Columns.Add(new BoundColumn());
			AssertEquals("One column", 1, TestGrid.Columns.Count);

			TestGrid.BindTo = "Collection";
			TestBizO.Collection.AddNew();
			TestGrid.Bind(TestBizO);
			TestGrid.PrepareControlHierarchyInternal();
			Assert("Should be some Items", TestGrid.Items.Count > 0);
			AssertEquals("Selection should be added", 2, TestGrid.Items[0].Cells.Count);
			AssertNotNull("SelectionCheckBox", TestGrid.Items[0].Cells[0].Controls[0] as ZSelectionCheckBox);
		}

		public void TestGetSelectedKeys()
		{
			ZDataGrid testGrid = new ZDataGrid();
			AssertNotNull("Should not return null", testGrid.GetSelectedPKs());

			testGrid.AllowMultiLineSelection = true;
			testGrid.BindTo = "Collection";
			TestBizO.Collection.AddNew();
			TestBizO.Collection.AddNew();
			testGrid.Bind(TestBizO);
			AssertNotNull("DataSource assigneded", testGrid.DataSource);
			Assert("Should be at least two elements in the Collection", testGrid.Collection.Count >= 2);

			testGrid.SelectRow(TestBizO.Collection[1].PK, true);
			testGrid.SelectRow(TestBizO.Collection[0].PK, true);
			AssertEquals("Two keys should be selected", string.Format("{0},{1}", TestBizO.Collection[1].PK, TestBizO.Collection[0].PK), testGrid.SelectedKeys);

			ZGuid[] selectedKeys = testGrid.GetSelectedPKs();
			AssertNotNull("Should not return null", selectedKeys);
			AssertEquals("Should return two PKs", 2, selectedKeys.Length);
			foreach (BusinessObject bizO in testGrid.Collection)
			{
				Assert("Should contain BizO with this key", ((IList)selectedKeys).Contains(bizO.PK));
			}
		}

		#endregion

		#region Selectable Columns

		public void TestSelectCellValue()
		{
			AssertEquals("Nothing selected", 0, TestGrid.SelectedCellsValues.Count);
			AssertNull("No DataSource specified", TestGrid.DataSource);

			var pk = ZGuid.NewZGuid();

			TestGrid.SelectCellValue(pk, "ACV", true);
			AssertEquals(1, TestGrid.SelectedCellsValues.Count);
			var entry = TestGrid.SelectedCellsValues[pk.ToString()].Values;
			AssertNotNull("Should be selected even when there is no DataSource", entry);
			AssertEquals(1, entry.Count);
			AssertEquals("ACV", entry[0]);

			TestGrid.BindTo = "Collection";
			TestBizO.Collection.AddNew();
			TestBizO.Collection.AddNew();
			TestGrid.Bind(TestBizO);
			AssertNotNull("DataSource assigneded", TestGrid.DataSource);

			TestGrid.SelectCellValue(pk, "ACV", false);
			AssertEquals(0, TestGrid.SelectedCellsValues.Count);

			TestGrid.SelectCellValue(null, "MSC", true);
			AssertEquals(0, TestGrid.SelectedCellsValues.Count);

			TestGrid.SelectCellValue(pk, "ACV", true);
			entry = TestGrid.SelectedCellsValues[pk.ToString()].Values;
			AssertNotNull(entry);
			AssertEquals(1, entry.Count);
			AssertEquals("ACV", entry[0]);

			TestGrid.SelectCellValue(pk, "ACV", true);
			entry = TestGrid.SelectedCellsValues[pk.ToString()].Values;
			AssertNotNull(entry);
			AssertEquals("Selecting the same value again doesn't increase the count", 1, entry.Count);
			AssertEquals("ACV", entry[0]);

			var pk2 = ZGuid.NewZGuid();

			TestGrid.SelectCellValue(pk, "BOE", true);
			TestGrid.SelectCellValue(pk2, "ACV", true);
			entry = TestGrid.SelectedCellsValues[pk.ToString()].Values.OrderBy(k => k).ToList();
			AssertNotNull(entry);
			AssertEquals("There should be 2 entries for pk", 2, entry.Count);
			AssertEquals("ACV", entry[0]);
			AssertEquals("BOE", entry[1]);
			entry = TestGrid.SelectedCellsValues[pk2.ToString()].Values;
			AssertNotNull(entry);
			AssertEquals("There should be 1 entry for pk2", 1, entry.Count);
			AssertEquals("ACV", entry[0]);

			TestGrid.SelectCellValue(pk, "ACV", false);
			TestGrid.SelectCellValue(pk, "BOE", false);
			Assert(!TestGrid.SelectedCellsValues.ContainsKey(pk.ToString()));
			entry = TestGrid.SelectedCellsValues[pk2.ToString()].Values;
			AssertNotNull(entry);
			AssertEquals("There should be 1 entry for pk2", 1, entry.Count);
			AssertEquals("ACV", entry[0]);

			TestGrid.SelectCellValue(pk2, "ACV", false);
			AssertEquals("Nothing should be selected", 0, TestGrid.SelectedCellsValues.Count);
		}

		#endregion

		#region Test Classes

		class DummyChildBusinessObjectWithHumanReadableName : DummyChildBusinessObject
		{
			public DummyChildBusinessObjectWithHumanReadableName(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZString HumanReadableNameCore
			{
				get { return "Dummy Bizo"; }
			}
		}

		#endregion

		#region Implementation

		void CreateLotsOfChildren(DummyBusinessObject parent, int numToCreate)
		{
			for (int i = 0; i < numToCreate; i++)
			{
				DummyChildBusinessObjectWithHumanReadableName child = Factory.NewWithValidTestData<DummyChildBusinessObjectWithHumanReadableName>();
				child.Z0_Number = i;
				child.Z0_Guid = parent.PK;
			}
			Factory.Save();
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		protected BusinessObjectFactory WebFactory
		{
			get { return Factory; }
		}

		protected ZDataGrid TestGrid
		{
			get { return Control as ZDataGrid; }
		}

		protected override Control GetNewControl()
		{
			return new ZDataGrid();
		}

		#endregion
	}
}
