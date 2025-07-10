using System;
using System.Collections;
using System.IO;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Modules;
using static Enterprise.ZArchitecture.Web.GUI.WebControls.SearchControl;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[HttpContextEnabledTest]
	public class FilterCriteriaTest : TestCaseWithFactory
	{
		#region TestIsSearchingIndexer

		public void TestIsSearchingIndexer()
		{
			using (var testPage = new TopLevelFilterPage())
			using (var testControl = new SearchControlForTest())
			{
				testControl.SearchResultsDataGrid = new ZDataGrid();
				testControl.Page = testPage;
				testPage.OnLoad();

				AssertNull(testPage.Session[SearchControlIsSearchingIndexer]);
				testControl.OnSearch += testControl_OnSearch;
				testControl.FindForTesting();
				AssertNull("Indexer must be removed from session", testPage.Session[SearchControlIsSearchingIndexer]);
			}
		}

		void testControl_OnSearch(object sender, EventArgs e)
		{
			Assert("Session should say 'true' while searchControl is searching", (bool)((SearchControlForTest)sender).Page.Session[SearchControlIsSearchingIndexer]);
		}

		#endregion

		#region TestShouldStoreFilterCriteria

		public void TestShouldStoreFilterCriteria()
		{
			using (var page = new TopLevelFilterPage())
			using (var control = new SearchControlForTest())
			{
				control.SearchResultsDataGrid = new ZDataGrid();
				control.Page = page;
				page.OnLoad();

				AssertEquals("Pre-condition", "", Env.Registry.GetFilterCriteria(WebEnv.CurrentUser.PK.ToGuid(), typeof(OrganisationFilterBusinessObject).FullName));
				control.FindButton_Click(control, EventArgs.Empty);
				AssertNotEquals("Should be stored if ITopLevelFilterPage", "", Env.Registry.GetFilterCriteria(WebEnv.CurrentUser.PK.ToGuid(), typeof(OrganisationFilterBusinessObject).FullName));
			}
		}
		#endregion

		#region TestShouldNotStoreFilterCriteriaIfPageIsNotITopLevelFilterPage

		public void TestShouldNotStoreFilterCriteriaIfPageIsNotITopLevelFilterPage()
		{
			using (var page = new NonTopLevelFilterPage())
			using (var control = new SearchControlForTest())
			{
				control.SearchResultsDataGrid = new ZDataGrid();
				control.Page = page;
				page.OnLoad();

				AssertEquals("Pre-condition", "", Env.Registry.GetFilterCriteria(WebEnv.CurrentUser.PK.ToGuid(), typeof(OrganisationFilterBusinessObject).FullName));
				control.FindButton_Click(control, EventArgs.Empty);
				AssertEquals("Should not be stored if not ITopLevelFilterPage", "", Env.Registry.GetFilterCriteria(WebEnv.CurrentUser.PK.ToGuid(), typeof(OrganisationFilterBusinessObject).FullName));
			}
		}
		#endregion

		#region TestCssClasses

		public void TestCssClasses()
		{
			using (var page = new TopLevelFilterPage())
			using (var control = new SearchControlForTest())
			{
				control.ModuleID = WebModuleIDs.Dummy;
				control.Page = page;

				page.OnLoad();

				AssertEquals("AlternatingStyle", "DetailsAlternatingCell", control.AlternatingCss);
				AssertEquals("ItemStyle", "DetailsCell", control.ItemCss);
				AssertEquals("TableStyle", "DetailsTable", control.TableCss);
				AssertEquals("HeaderStyle", "DetailsHeader", control.HeaderCss);
				AssertEquals("PagerStyle", "ResultsTablePager", control.PagerCss);
			}
		}
		#endregion

		#region TestStoreGridLayoutThroughGridLayoutControl

		public void TestStoreGridLayoutThroughGridLayoutControl()
		{
			using (var page = new TopLevelFilterPage())
			using (var control = new SearchControlForTest())
			{
				control.SearchResultsDataGrid = new ZDataGrid();
				control.ModuleID = WebModuleIDs.Dummy;
				control.Page = page;
				page.OnLoad();

				AssertEquals("Pre-condition", null, control.GridLayoutRegistry.GetGridLayout(control.ModuleID.ToString() + ".GridLayout", WebEnv.CurrentUser.PK.ToGuid()));
				control.GridLayoutControl.Text = "0";
				control.OnGridLayoutChanged(control, EventArgs.Empty);
				using (var layout = control.GridLayoutRegistry.GetGridLayout(control.ModuleID.ToString() + ".GridLayout", WebEnv.CurrentUser.PK.ToGuid()))
				{
					AssertNotNull("Layout should be stored", layout);
				}
			}
		}
		#endregion

		#region TestDefaultModuleLayoutAppliedForNoLayoutStored

		public void TestDefaultModuleLayoutAppliedForNoLayoutStored()
		{
			using (var page = new TopLevelFilterPage())
			using (var control = new SearchControlForTest())
			{
				control.SearchResultsDataGrid = new ZDataGrid();
				control.ModuleID = WebModuleIDs.Dummy;
				control.Page = page;
				page.OnLoad();

				AssertEquals("GridColumnsLayoutKey", WebModuleIDs.Dummy.ToString() + ".GridLayout", control.GridColumnsLayoutKey);
				var layoutInSession = page.Session[control.GridColumnsLayoutKey] as string;
				AssertNull("Pre-condition - No layout should be stored in Session", layoutInSession);

				AssertNull("Pre-condition - No layout in the Registry", control.GridLayoutRegistry.GetGridLayout(control.ModuleID.ToString(), WebEnv.CurrentUser.PK.ToGuid()));

				control.SetupGridColumns();
				AssertEquals("Should be 3 columns in the grid", 3, control.SearchResultsDataGrid.Columns.Count);
				AssertEquals("First Column", control.Module.DefaultGridColumnFields[0], control.SearchResultsDataGrid.Columns[0]);
				AssertEquals("Second Column", ((ZGroupColumn)control.Module.DefaultGridColumnFields[1]).GroupMembers[0], control.SearchResultsDataGrid.Columns[1]);
				AssertEquals("Third Column", ((ZGroupColumn)control.Module.DefaultGridColumnFields[1]).GroupMembers[1], control.SearchResultsDataGrid.Columns[2]);
			}
		}

		#endregion

		#region TestLayoutLoadDoesNotRePopulateEmptyGrid

		public void TestLayoutLoadDoesNotRePopulateEmptyGrid()
		{
			using (var page = new TopLevelFilterPageWithFilterStripBO())
			using (var control = new SearchControlForTest())
			{
				control.SearchResultsDataGrid = new ZDataGrid();
				control.ModuleID = WebModuleIDs.Dummy;
				control.Page = page;
				page.OnLoad();
				control.OnLoad();
				control.SetupGrid();
				var collection = new DummyBusinessObjectCollection(Factory);
				control.SearchResultsDataGrid.Bind(collection);
				var filterStripBizO = control.FilterStripBizO;
				AssertEquals(0, collection.Count);
				AssertEquals(0, control.SearchResultsDataGrid.Items.Count);
				AssertEquals(0, control.RePopulateGridCounter);
				filterStripBizO.LoadLayout(null);
				AssertEquals(0, control.RePopulateGridCounter);
				for (var i = 0; i < 5; i++)
				{
					collection.Add(Factory.New<DummyBusinessObject>());
				}
				control.Module.GridCollection.AddRange(collection);
				control.SearchResultsDataGrid.Bind(collection);
				AssertEquals(5, collection.Count);
				AssertEquals(5, control.SearchResultsDataGrid.Items.Count);
				AssertEquals(0, control.RePopulateGridCounter);
				filterStripBizO.LoadLayout(null);
				AssertEquals(1, control.RePopulateGridCounter);
			}
		}

		#endregion

		#region TestGetSelectionPK

		public void TestGetSelectionPK()
		{
			using (var page = new TopLevelFilterPage())
			using (var control = new SearchControlForTest())
			{
				control.SearchResultsDataGrid = new ZDataGrid();
				control.ModuleID = WebModuleIDs.Dummy;
				control.Page = page;
				page.OnLoad();
				control.SetupGrid();
				AssertEquals("GetSelectionPK", ZGuid.Empty, control.GetSelectionPK());

				var collection = new DummyBusinessObjectCollection(Factory);
				for (var i = 0; i < 5; i++)
				{
					collection.Add(Factory.New<DummyBusinessObject>());
				}

				control.LoadResults(control.FilterBusinessObject);
				Assert("IsLoaded", control.Module.GridCollection.IsLoaded);
				AssertEquals("Count", 0, control.Module.GridCollection.Count);
				control.Module.GridCollection.AddRange(collection);
				control.SearchResultsDataGrid.Bind(collection);
				control.SearchResultsDataGrid.SelectedIndex = 0;

				AssertEquals("GetSelectionPK", ((control.SearchResultsDataGrid.DataSource as IList)[0] as BusinessObject).PK, control.GetSelectionPK());
			}
		}
		#endregion

		#region TestSelectionColumnIndex

		public void TestSelectionColumnIndex()
		{
			using (var page = new TopLevelFilterPage())
			using (var control = new SearchControlForTest())
			{
				control.SearchResultsDataGrid = new ZDataGrid();
				control.ModuleID = WebModuleIDs.Dummy;
				control.Page = page;
				page.OnLoad();

				AssertEquals("Should be no columns in the grid", 0, control.SearchResultsDataGrid.Columns.Count);
				AssertEquals("SelectionColumnIndex should be 0 for legacy support", 0, control.SelectionColumnIndex);

				AssertEquals("Should be five columns defined by module", 5, control.Module.GridColumnFields.Length);
				control.SearchResultsDataGrid.Columns.Add(control.Module.GridColumnFields[1]);
				AssertEquals("Should be one columns in the grid", 1, control.SearchResultsDataGrid.Columns.Count);
				AssertEquals("SelectionColumnIndex should be -1 because there is no SelectionColumn", -1, control.SelectionColumnIndex);

				control.SetupGridColumns();
				AssertEquals("Should be 3 columns in the grid", 3, control.SearchResultsDataGrid.Columns.Count);
				AssertEquals("FirstColumn should be SelectionColumn", control.Module.SelectionColumn, control.SearchResultsDataGrid.Columns[0]);
				AssertEquals("SelectionColumnIndex should BeginEventHandler 0", 0, control.SelectionColumnIndex);
			}
		}
		#endregion

		#region TestFindButtonClickClearsMultiLineSelection

		public void TestFindButtonClickClearsMultiLineSelection()
		{
			using (var page = new FilterPageForTest())
			using (var control = new SearchControlForTest())
			{
				control.Page = page;
				control.SearchResultsDataGrid = new ZDataGrid();
				control.SearchResultsDataGrid.AllowMultiLineSelection = true;
				page.OnLoad();

				var pk = ZGuid.NewZGuid();

				control.SearchResultsDataGrid.SelectRow(pk, true);
				Assert("Should be Selected", control.SearchResultsDataGrid.IsRowSelected(pk));

				control.FindButton_Click(control, EventArgs.Empty);
				Assert("Should be not Selected", !control.SearchResultsDataGrid.IsRowSelected(pk));
			}
		}

		#endregion

		#region TestClearButtonClickClearsMultiLineSelection

		public void TestClearButtonClickClearsMultiLineSelection()
		{
			using (var page = new FilterPageForTest())
			using (var control = new SearchControlForTest())
			{
				control.ModuleID = WebModuleIDs.Dummy;
				control.Page = page;
				control.SearchResultsDataGrid = new ZDataGrid();
				control.SearchResultsDataGrid.AllowMultiLineSelection = true;
				page.OnLoad();

				var pk = ZGuid.NewZGuid();

				control.SearchResultsDataGrid.SelectRow(pk, true);
				Assert("Should be Selected", control.SearchResultsDataGrid.IsRowSelected(pk));

				control.ClearButton_Click(control, EventArgs.Empty);
				Assert("Should be not Selected", !control.SearchResultsDataGrid.IsRowSelected(pk));
			}
		}

		#endregion

		#region TestCustomNewButtonClick

		public void TestCustomNewButtonClick()
		{
			using (var page = new FilterPageForTest())
			using (var control = new SearchControlForTest())
			{
				control.ModuleID = WebModuleIDs.Dummy;
				control.Page = page;
				control.CustomNewButtonClick += new EventHandler(control.CustomNewButtonClickForTest);
				control.SearchResultsDataGrid = new ZDataGrid();
				control.SearchResultsDataGrid.AllowMultiLineSelection = true;
				page.OnLoad();

				AssertEquals("Precondition: CustomNewButtonClick should not be Invoked", false, control.CustomNewButtonClickInvoked);
				control.NewButtonUrl = "Some Url";
				control.NewButton_Click(control, EventArgs.Empty);
				Assert("CustomNewButtonClick should be Invoked", control.CustomNewButtonClickInvoked);
				AssertNull("NewButtonUrl should be reset to null", control.NewButtonUrl);
			}
		}

		#endregion

		//#region TestOverrideModuleFilterBizO

		//public void TestOverrideModuleFilterBizO()
		//{
		//    using (FilterPageForTest Page = new FilterPageForTest())
		//    using (SearchControlForTest Control = new SearchControlForTest())
		//    {
		//        Control.ModuleID = WebModuleIDs.TrackingShipments;
		//        Control.Page = Page;
		//        Control.SearchResultsDataGrid = new ZDataGrid();
		//        Control.SearchResultsDataGrid.AllowMultiLineSelection = true;
		//        DummyFilterStripBusinessObject dummyFilterStripBizO = new DummyFilterStripBusinessObject();
		//        Control.FilterStripBizO = dummyFilterStripBizO;
		//        Page.OnLoad();

		//        DummyZFilterStripGridModule dummyStripModule = Control.Module as DummyZFilterStripGridModule;

		//        AssertEquals(true, dummyStripModule is ZFilterStripGridModule);
		//        AssertEquals(dummyFilterStripBizO, dummyStripModule.FilterStripBizOForTest);
		//    }
		//}

		//#endregion

		//#region TestInitializeFilterStripControl

		//public void TestInitializeFilterStripControl()
		//{ 
		//    using (FilterPageForTest Page = new FilterPageForTest())
		//    using (SearchControlForTest Control = new SearchControlForTest())
		//    {
		//        Control.ModuleID = WebModuleIDs.TrackingShipments;
		//        Control.Page = Page;
		//        Control.SearchResultsDataGrid = new ZDataGrid();
		//        Control.SearchResultsDataGrid.AllowMultiLineSelection = true;
		//        DummyFilterStripBusinessObject dummyFilterStripBizO = new DummyFilterStripBusinessObject();
		//        Control.FilterStripBizO = dummyFilterStripBizO;
		//        Control.InitialiseFilterStripControlForTest();
		//        Page.OnLoad();

		//        AssertEquals(dummyFilterStripBizO, Control.FilterStripControlForTest.FilterStripBizO);
		//    }
		//}

		//#endregion

		#region TestCustomViewButtonClick

		public void TestCustomViewButtonClick()
		{
			using (FilterPageForTest page = new FilterPageForTest())
			using (SearchControlForTest control = new SearchControlForTest())
			{
				control.ModuleID = WebModuleIDs.Dummy;
				control.Page = page;
				control.CustomViewButtonClick += new EventHandler(control.CustomViewButtonClickForTest);
				control.SearchResultsDataGrid = new ZDataGrid();
				control.SearchResultsDataGrid.AllowMultiLineSelection = true;
				page.OnLoad();

				AssertEquals("Precondition: CustomViewButtonClick should not be Invoked", false, control.CustomViewButtonClickInvoked);
				control.ViewButtonUrl = "Some Url";
				control.ViewButton_Click(control, EventArgs.Empty);
				Assert("CustomViewButtonClick should be Invoked", control.CustomViewButtonClickInvoked);
				AssertNull("ViewButtonUrl should be reset to null", control.ViewButtonUrl);
			}
		}

		#endregion

		#region TestExportToExcelButtonClick

		public void TestExportToExcelButtonClick()
		{
			DummyBusinessObjectCollection testCollection = new DummyBusinessObjectCollection(Factory);
			for (int i = 0; i < 2; i++)
			{
				testCollection.AddNew();
			}
			Factory.Save();
			RunExcelTest(testCollection, false);
		}

		public void TestExportToExcelButtonClickExportsFromNewCollection()
		{
			DummyBusinessObjectCollection testCollection = new DummyBusinessObjectCollection(Factory);
			for (int i = 0; i < 2; i++)
			{
				testCollection.AddNew();
			}
			RunExcelTest(testCollection, true);
		}

		void RunExcelTest(IBusinessObjectCollection testCollection, bool shouldBeEmpty)
		{
			using (var page = new TopLevelFilterPageWithFilterStripBO())
			using (var control = new SearchControlForTest())
			{
				WebDataRegistry.Instance.MaxFilteredRecords.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

				control.ModuleID = WebModuleIDs.Dummy;
				control.Page = page;
				var testGrid = new ZDataGrid();
				testGrid.Columns.Add(new ZTextEditColumn("Text", DummyBusinessObject.Schema.Z0_VarCharMax));
				testGrid.Columns.Add(new ZDateTimeColumn("Date", DummyBusinessObject.Schema.Z0_Date));
				control.SearchResultsDataGrid = testGrid;

				page.OnLoad();
				control.SearchResultsDataGrid.DataSource = testCollection;

				var ms = new MemoryStream();
				var testResponseFilter = new TestResponseFilter(HttpContext.Current.Response.Filter, ms);
				HttpContext.Current.Response.Filter = testResponseFilter;

				int beforeExport = Factory.DatabaseLoadCount;

				control.ExportToExcelButton_Click(null, EventArgs.Empty);
				AssertEquals("No new db hits as another factory was used", beforeExport, Factory.DatabaseLoadCount);
				HttpContext.Current.Response.Flush();
				Assert(string.Format("The response should {0} be empty", shouldBeEmpty ? "" : "not"), shouldBeEmpty ? ms.Length == 0 : ms.Length > 0);
			}
		}

		#endregion

		#region PageForTest

		class ZPageForTest : ZPage
		{
			protected override BusinessObject GetNewDataSource()
			{
				return new WebFilterBusinessObjectFactory(Factory).New<OrganisationFilterBusinessObject>();
			}

			public void OnLoad()
			{
				base.OnLoad(EventArgs.Empty);
			}
		}

		class TopLevelFilterPage : ZPageForTest, IRememberFilterCriteriaPage
		{
		}

		class TopLevelFilterPageWithFilterStripBO : TopLevelFilterPage
		{
			protected override BusinessObject GetNewDataSource()
			{
				return new DummyFilterStripBusinessObject();
			}
		}

		class NonTopLevelFilterPage : ZPageForTest
		{
		}

		class FilterPageForTest : ZPageForTest
		{
			protected override BusinessObject GetNewDataSource()
			{
				return new WebFilterBusinessObjectFactory(Factory).New<DummyFilterBusinessObject>();
			}
		}

		#endregion
	}
}
