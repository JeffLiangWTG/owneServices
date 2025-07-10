using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class WebSecuritySearchControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			using (var page = new BasePage())
			using (var searchControl = new WebSecuritySearchControlForTest())
			{
				searchControl.ModuleID = WebModuleIDs.CargoWiseEDIWebSecurityContacts;
				searchControl.Page = page;
				searchControl.FilterStripBizO = new WebSecurityDataSource(Factory, org.PK);
				searchControl.OnLoadForTest();
				var columnProvider = ((ZFilterStripGridModule)searchControl.Module).ColumnProvider;
				var cols = string.Join("\r\n", columnProvider.AllColumns.Select(x => x.HeaderText));
				AssertEquals(@"Contact Name
Email
Company Name
UNLOCO", cols);
			}
		}

		[HttpContextEnabledTest]
		public void TestContactSecurityGridColumns()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			using (var page = new BasePage())
			using (var searchControl = new ZSearchControlForTest())
			{
				searchControl.ModuleID = WebModuleIDs.CargoWiseEDIWebSecurityContacts;
				searchControl.Page = page;
				searchControl.FilterStripBizO = new WebSecurityDataSource(Factory, org.PK);
				searchControl.OnInitForTest();
				searchControl.OnLoadForTest();
				var columnProvider = searchControl.ContactSecurityGrid.ColumnProvider;
				var cols = string.Join("\r\n", columnProvider.AllColumns.Select(x => x.HeaderText));
				AssertEquals(@"Security Item
Granted", cols);
			}
		}

		class WebSecuritySearchControlForTest : WebSecuritySearchControl
		{
			public void OnLoadForTest()
			{
				base.OnLoad(EventArgs.Empty);
			}
		}

		[ExpectNoExceptions]
		[HttpContextEnabledTest]
		public void TestOnInitShouldNotThrowNREIfEnvironmentVariablesAreNull()
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
				Env.ClearUserContext();
				AssertNull(Env.CurrentUser);
				searchControl.OnInitForTest();
			}
		}

		class PageForTest : ZPage
		{
			protected override BusinessObject GetNewDataSource()
			{
				return new DummyFilterStripBusinessObject();
			}

			public void OnLoad()
			{
				base.OnLoad(EventArgs.Empty);
			}
		}

		class PageForTestWithPopulatedFilterStrip : PageForTest
		{
			protected override BusinessObject GetNewDataSource()
			{
				return new FilterStripBusinessObjectForTest();
			}
		}

		public class FilterStripBusinessObjectForTest : FilterStripBusinessObject
		{
			public static class Schema
			{
				public static ZString TestAuditFilterName = "TestAuditFilter";
				public static ZString TestDateFilterName = "TestDateFilter";
				public static ZString TestGuidFilterName = "TestGuidFilter";
				public static ZString TestGuidsFilterName = "TestGuidsFilter";
				public static ZString TestLocationFilterName = "TestLocationFilter";
				public static ZString TestNkFilterName = "TestNkFilter";
				public static ZString TestNumberFilterName = "TestNumberFilter";
				public static ZString TestNumberRangeFilterName = "TestNumberRangeFilter";
				public static ZString TestSingleDateFilterName = "TestSingleDateFilter";
				public static ZString TestTextAndNkFilterName = "TestTextAndNkFilter";
				public static ZString TestTextFilterName = "TestTextFilter";
				public static ZString TestTextRangeFilterName = "TestTextRangeFilter";
				public static ZString UnpublishedFilterName = "UnpublishedFilter";
			}

			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				ModuleFilterCollection result = new ModuleFilterCollection();
				result.AddDateFilter(Schema.TestDateFilterName, TestQuery);
				result.AddGuidFilter(Schema.TestGuidFilterName, DummyModuleIDs.Dummy, TestQuery, OrgList);
				result.AddCustomFilter(new ModuleGuidsFilter(Schema.TestGuidsFilterName, DummyModuleIDs.Dummy, TestQuery, OrgList, OrgList));
				result.AddLocationFilter(Schema.TestLocationFilterName, TestQuery, LocationList, LocationList);
				result.AddNkFilter(Schema.TestNkFilterName, TestQueryNk, DummyModuleIDs.Dummy, TestList);
				result.AddNumberFilter(Schema.TestNumberFilterName, TestQuery);
				result.AddNumberRangeFilter(Schema.TestNumberRangeFilterName, TestQuery);
				result.AddSingleDateFilter(Schema.TestSingleDateFilterName, TestQuery);
				result.AddTextAndNkFilter(Schema.TestTextAndNkFilterName, TestQuery, DummyModuleIDs.Dummy, TestList);
				result.AddTextFilter(Schema.TestTextFilterName, TestQuery);
				result.AddTextRangeFilter(Schema.TestTextRangeFilterName, TestQuery);
				ModuleTextFilter unpublishedFilter = result.AddTextFilter(Schema.UnpublishedFilterName, TestQuery);
				unpublishedFilter.IsPublishedOnWeb = false;
				return result;
			}

			ZQuery TestQuery(SQLComparisonOperator comparisonOperator, ZString value)
			{
				return new ZQuery();
			}

			ZQuery TestQuery(SQLComparisonOperator comparisonOperator, ZString value1, ZString value2)
			{
				return new ZQuery();
			}

			ZQuery TestQuery(ZGuid value)
			{
				return new ZQuery();
			}

			ZQuery TestQuery(ZGuid value1, ZGuid value2)
			{
				return new ZQuery();
			}

			ZQuery TestQueryNk(ZString value)
			{
				return new ZQuery();
			}

			ZQuery TestQuery(ZDateTime value)
			{
				return new ZQuery();
			}

			ZQuery TestQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
			{
				return new ZQuery();
			}

			ZQuery TestQuery(ZString value1, ZString value2)
			{
				return new ZQuery();
			}

			ZQuery TestQuery(INumericZType value1, INumericZType value2)
			{
				return new ZQuery();
			}

			IBusinessObjectCollection TestList
			{
				get
				{
					return new DummyBusinessObjectCollection(Factory);
				}
			}

			LocationCollection LocationList
			{
				get
				{
					return new LocationCollection(Factory);
				}
			}

			OrgHeaderCollection OrgList
			{
				get
				{
					return new OrgHeaderCollection(Factory);
				}
			}
		}

		class ZSearchControlForTest : WebSecuritySearchControl
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

			public new StateBag ViewState => base.ViewState;
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
	}
}
