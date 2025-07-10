using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[HttpContextEnabledTest]
	sealed class ZSearchControlTest : TestCaseWithFactory
	{
		public void TestBeforeSaveLayout()
		{
			var cachedIsWeb = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				using (var searchControl = new ZSearchControlForTest())
				{
					var layout = Factory.NewWithValidTestData<StmModuleFilter>();
					layout.S9_FilterName = "Layout One";
					layout.S9_IsPublished = false;

					searchControl.OnBeforeSaveLayoutForTest(layout);
					AssertEquals("Layout One", searchControl.SearchResultsDataGridForTest.SaveAsLayoutNameForTest);
					AssertEquals(false, searchControl.SearchResultsDataGridForTest.SaveAsIsPublishedForOrganisationForTest);
					AssertEquals(false, searchControl.SearchResultsDataGridForTest.SaveAsIsPublishedForCompanyForTest);
				}
			}
			finally
			{
				Globals.IsWeb = cachedIsWeb;
			}
		}

		public void TestEDocsColumn()
		{
			var docTypeACV = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "ACV"));
			var docTypeMSC = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "MSC"));

			var refDocTypeEntryCollection = new RefDocTypeEntryCollection();
			var entry1 = refDocTypeEntryCollection.AddNew();
			var entry2 = refDocTypeEntryCollection.AddNew();

			entry1.RefDocTypePK = docTypeACV.PK;
			entry2.RefDocTypePK = docTypeMSC.PK;

			var dictionary = new WebEDocsDownloadEntryDictionary();
			dictionary.Add(WebModuleIDs.TrackingShipments.Name, new WebEDocsDownloadEntry(true, refDocTypeEntryCollection));

			using (WebDataRegistry.Instance.WebEDocsBulkDownload.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dictionary))
			{
				using (var page = new PageForTestWithPopulatedFilterStrip())
				using (var searchControl = new ZSearchControlForTest())
				{
					page.OnLoad();
					searchControl.ModuleID = WebModuleIDs.TrackingShipments;
					searchControl.Page = page;
					searchControl.OnLoadForTest();

					var columnProvider = ((ZFilterStripGridModule)searchControl.Module).ColumnProvider;
					Assert(columnProvider.AllColumns.Any(c => c.HeaderText == "ACV"));
					Assert(columnProvider.AllColumns.Any(c => c.HeaderText == "MSC"));
					Assert(columnProvider.AllColumns.Any(c => c.HeaderText == "All eDocs"));
					Assert(!columnProvider.AllColumns.Any(c => c.HeaderText == "BOE"));
				}

				//TrackingDeclaration extends TrackingShipments, but it shouldn't support eDocs bulk download
				using (var page = new PageForTestWithPopulatedFilterStrip())
				using (var searchControl = new ZSearchControlForTest())
				{
					page.OnLoad();
					searchControl.ModuleID = WebModuleIDs.TrackingDeclarations;
					searchControl.Page = page;
					searchControl.OnLoadForTest();

					var columnProvider = ((ZFilterStripGridModule)searchControl.Module).ColumnProvider;
					Assert(!columnProvider.AllColumns.Any(c => c.HeaderText == "ACV"));
					Assert(!columnProvider.AllColumns.Any(c => c.HeaderText == "MSC"));
					Assert(!columnProvider.AllColumns.Any(c => c.HeaderText == "All eDocs"));
					Assert(!columnProvider.AllColumns.Any(c => c.HeaderText == "BOE"));
				}

				//TrackingContainers doesn't support eDocs bulk download
				using (var page = new PageForTestWithPopulatedFilterStrip())
				using (var searchControl = new ZSearchControlForTest())
				{
					page.OnLoad();
					searchControl.ModuleID = WebModuleIDs.TrackingContainers;
					searchControl.Page = page;
					searchControl.OnLoadForTest();

					var columnProvider = ((ZFilterStripGridModule)searchControl.Module).ColumnProvider;
					Assert(!columnProvider.AllColumns.Any(c => c.HeaderText == "ACV"));
					Assert(!columnProvider.AllColumns.Any(c => c.HeaderText == "MSC"));
					Assert(!columnProvider.AllColumns.Any(c => c.HeaderText == "All eDocs"));
					Assert(!columnProvider.AllColumns.Any(c => c.HeaderText == "BOE"));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestLoadCollectionFromCachedPKs_ActiveBusinessObjectCollection()
		{
			using (var page = new PageForTestWithPopulatedFilterStrip())
			using (var searchControl = new ZSearchControlForTest())
			{
				page.OnLoad();
				searchControl.ModuleID = WebModuleIDs.TrackingCartage; // Uses ActiveBusinessObjectCollection
				searchControl.Page = page;

				searchControl.SearchResultsDataGrid.Collapsed = false;
				searchControl.ViewState["ModuleGridCollectionCachedPKs"] = new[] { ZGuid.NewZGuid() };
				searchControl.OnLoadForTest();
			}
		}

		[HttpContextEnabledTest]
		public void TestSortCommandLoadsOnce()
		{
			using (var page = new PageForTestWithPopulatedFilterStrip())
			using (var searchControl = new ZSearchControlForTest())
			{
				page.OnLoad();
				searchControl.ModuleID = WebModuleIDs.TrackingInventory;
				searchControl.Page = page;

				searchControl.SearchResultsDataGrid.Collapsed = false;
				searchControl.IsResultsRelatedOperation = true;
				searchControl.OnLoadForTest();

				AssertEquals(true, searchControl.ViewState["SearchControlHasLoadedResults"]);

				searchControl.ViewState["SearchControlHasLoadedResults"] = false;
				searchControl.SortCommandForTest();

				AssertEquals(false, searchControl.ViewState["SearchControlHasLoadedResults"]);
			}
		}

		[HttpContextEnabledTest]
		public void TestPageIndexChangedLoadsOnce()
		{
			using (var page = new PageForTestWithPopulatedFilterStrip())
			using (var searchControl = new ZSearchControlForTest())
			{
				page.OnLoad();
				searchControl.ModuleID = WebModuleIDs.TrackingInventory;
				searchControl.Page = page;

				searchControl.SearchResultsDataGrid.Collapsed = false;
				searchControl.IsResultsRelatedOperation = true;
				searchControl.OnLoadForTest();

				AssertEquals(true, searchControl.ViewState["SearchControlHasLoadedResults"]);

				searchControl.ViewState["SearchControlHasLoadedResults"] = false;
				searchControl.PageIndexChangedForTest();

				AssertEquals(false, searchControl.ViewState["SearchControlHasLoadedResults"]);
			}
		}

		[HttpContextEnabledTest]
		public void TestCollectionToExport()
		{
			var maxFilteredRecordsForExportToExcel = 5;
			using (WebDataRegistry.Instance.MaxFilteredRecords.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (WebDataRegistry.Instance.MaxFilteredRecordsForExportToExcel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, maxFilteredRecordsForExportToExcel))
			using (var page = new PageForTestWithPopulatedFilterStrip())
			using (var searchControl = new ZSearchControlForTest())
			{
				page.OnLoad();
				searchControl.ModuleID = WebModuleIDs.OrgAddress;
				searchControl.Page = page;

				searchControl.SearchResultsDataGrid.Collapsed = false;
				searchControl.OnLoadForTest();
				searchControl.OnInitForTest();

				var expectedCount = Factory.GetDatabaseCount(searchControl.Module.GridCollection.TypeOfElements, searchControl.FilterBusinessObject.Filter);
				expectedCount = (expectedCount > maxFilteredRecordsForExportToExcel) ? maxFilteredRecordsForExportToExcel : expectedCount;
				var exportedCount = searchControl.SearchResultsDataGrid.GetCollectionToExportOverride().Count;

				AssertEquals("GetCollectionToExport should return " + expectedCount + " rows", expectedCount, exportedCount);
			}
		}

		public void TestLoadViewStateForModule()
		{
			using (var searchControl = new ZSearchControlForTest())
			using (var module = new ZFilterGridModuleForTest(Factory, null))
			{
				var viewState = Array.Empty<object>();
				searchControl.SetModuleForTest(module);
				searchControl.LoadViewStateForTest(new object[] { null, string.Empty, ListSortDirection.Ascending, "module data", null });

				AssertEquals("module data", module.SessionCollectionIndexer);
			}
		}

		public void TestSaveViewStateForModule()
		{
			using (var searchControl = new ZSearchControlForTest())
			using (var module = new ZFilterGridModuleForTest(Factory, null))
			{
				var viewState = Array.Empty<object>();
				module.SessionCollectionIndexer = "module data";
				searchControl.SetModuleForTest(module);
				var data = (object[])searchControl.SaveViewStateForTest();

				AssertEquals("module data", data[3]);
			}
		}

		class ZGridForTest : ZGrid
		{
			public override void SaveAsLayoutColumns(string layoutName, bool isPublishedForOrganisation, bool isPublishedForCompany)
			{
				SaveAsLayoutNameForTest = layoutName;
				SaveAsIsPublishedForOrganisationForTest = isPublishedForOrganisation;
				SaveAsIsPublishedForCompanyForTest = isPublishedForCompany;
			}

			public string SaveAsLayoutNameForTest;
			public bool SaveAsIsPublishedForOrganisationForTest;
			public bool SaveAsIsPublishedForCompanyForTest;
		}

		class ZSearchControlForTest : ZSearchControl
		{
			public void OnBeforeSaveLayoutForTest(StmModuleFilter layout)
			{
				base.OnBeforeSaveLayout(this, new LayoutEventArgs(layout));
			}

			protected override ZGrid GetNewDataGrid()
			{
				return new ZGridForTest();
			}

			public ZGridForTest SearchResultsDataGridForTest => (ZGridForTest)SearchResultsDataGrid;

			public void OnLoadForTest()
			{
				base.OnLoad(EventArgs.Empty);
			}

			public void SortCommandForTest()
			{
				SearchResultsDataGrid_SortCommand(SearchResultsDataGrid, new DataGridSortCommandEventArgs(null, new DataGridCommandEventArgs(new DataGridItem(0, 0, ListItemType.Header), null, new CommandEventArgs("Sort", "Ascending"))));
			}

			public void PageIndexChangedForTest()
			{
				SearchResultsDataGrid_PageIndexChanged(SearchResultsDataGrid, new DataGridPageChangedEventArgs(null, 2));
			}

			public void OnInitForTest()
			{
				base.OnInit(EventArgs.Empty);
			}

			protected override void LoadFilterControl(ZFilterGridModule module)
			{
			}

			public void LoadViewStateForTest(object[] savedState) => base.LoadViewState(savedState);

			public object SaveViewStateForTest() => base.SaveViewState();

			public void SetModuleForTest(ZFilterGridModule module) => base.module = module;

			public new StateBag ViewState => base.ViewState;
		}

		class ZFilterGridModuleForTest : WebDummyModule
		{
			public ZFilterGridModuleForTest(BusinessObjectFactory factory, ZPage page) : base(factory, page)
			{
			}

			protected override bool CacheCollection => true;
		}
	}
}
