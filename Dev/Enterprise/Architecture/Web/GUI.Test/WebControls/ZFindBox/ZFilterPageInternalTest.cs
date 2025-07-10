using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[HttpContextEnabledTest]
	public class ZFilterPageInternalTest : ZIFramePageTest
	{
		#region Module Tests

		public void TestModuleID()
		{
			AssertEquals(WebModuleIDs.Dummy, TestFilterPage.ModuleID);
		}

		public void TestModuleType()
		{
			AssertEquals(TestFilterPage.SearchControl.Module.GetType(), typeof(WebDummyModule));
		}

		#endregion Module Test

		#region TestSetupFilterGrid

		public void TestSetupFilterGrid()
		{
			TestFilterPage.FilterDataGrid.Columns.Clear();
			AssertEquals(0, TestFilterPage.FilterDataGrid.Columns.Count);
			Assert(!TestFilterPage.FilterDataGrid.AutoGenerateColumns);

			SetupDummyGrid();

			AssertEquals("First Column", TestFilterPage.SearchControl.Module.GridColumnFields[0], TestFilterPage.FilterDataGrid.Columns[0]);
			AssertEquals("Second Column", TestFilterPage.SearchControl.Module.GridColumnFields[1], TestFilterPage.FilterDataGrid.Columns[1]);
			AssertEquals("Third Column", TestFilterPage.SearchControl.Module.GridColumnFields[2], TestFilterPage.FilterDataGrid.Columns[2]);
		}

		#endregion TestSetupFilterGrid

		#region TestOKFunctionArgs

		public override void TestOKFunctionArguments()
		{
			SetupDummyGrid();

			TestFilterPage.FilterDataGrid.Bind(GenerateDummyDataSource());
			TestFilterPage.FilterDataGrid.SelectedIndex = 0;

			AssertEquals(2, TestFilterPage.OKFunctionArgumentsInternal.Length);
			AssertEquals("'r1c1'", TestFilterPage.OKFunctionArgumentsInternal[0]);

			TestFilterPage.FilterDataGrid.SelectedIndex = 1;
			AssertEquals("'r2c1'", TestFilterPage.OKFunctionArgumentsInternal[0]);
		}

		public override void TestOKButtonClick()
		{
			SetupDummyGrid();
			TestFilterPage.FilterDataGrid.Bind(GenerateDummyDataSource());
			TestFilterPage.FilterDataGrid.SelectedIndex = 0;
			ExpectedScriptArgs = "'r1c1'";
			base.TestOKButtonClick();
		}

		public void TestOKButtonNotVisible()
		{
			TestFilterPage.OnLoad(EventArgs.Empty);
			Assert(!TestFilterPage.OKButton.Visible);
		}

		public void TestOKFunctionsArgsMultiPage()
		{
			AssertEquals("Module ID should be WebDummyModule", WebModuleIDs.Dummy, TestFilterPage.ModuleID);
			CreateDummyObjects(20);

			SetupGridForDummyBizOCollection(true, 5);

			TestFilterPage.OnLoad(EventArgs.Empty);

			NameValueCollection postData = new NameValueCollection();
			postData.Add("Details.Text", "Dummy BizO");
			TestFilterPage.Details.ProcessPostData("Details.Text", postData);
			TestFilterPage.SearchControl.IsPostBack = true;
			TestFilterPage.OnLoad(EventArgs.Empty);
			TestFilterPage.FindButton_Click(this, EventArgs.Empty);
			TestFilterPage.FilterDataGrid.SelectedIndex = 3;
			TestFilterPage.HandleOkButtonClickInternal();
			AssertEquals("OKFunction Args Page 1", "'004'", TestFilterPage.OKFunctionArgumentsInternal[0]);

			TestFilterPage.FilterDataGrid.SelectedIndex = 2;
			TestFilterPage.OnLoad(EventArgs.Empty);
			TestFilterPage.SearchControl.SearchResultsDataGrid_PageIndexChanged(1);
			TestFilterPage.HandleOkButtonClickInternal();
			AssertEquals("OKFunction Args Page 2", "'008'", TestFilterPage.OKFunctionArgumentsInternal[0]);
		}

		#endregion TestOKFunctionArgs

		#region TestCancelFunctionArgs

		public override void TestCancelFunctionArguments()
		{
			AssertNull(TestFilterPage.CancelFunctionArgumentsInternal);
		}

		#endregion TestCancelFunctionArgs

		#region TestNewSearchShowsFirstPageOfResults

		public void TestNewSearchShowsFirstPageOfResults()
		{
			AssertEquals("Module ID should be WebDummyModule", WebModuleIDs.Dummy, TestFilterPage.ModuleID);

			CreateDummyObjects(20);

			SetupGridForDummyBizOCollection(true, 5);

			NameValueCollection postData = new NameValueCollection();
			postData.Add("Details.Text", "Dummy BizO");
			TestFilterPage.Details.ProcessPostData("Details.Text", postData);
			TestFilterPage.SearchControl.IsPostBack = true;
			TestFilterPage.OnLoad(EventArgs.Empty);
			TestFilterPage.FindButton_Click(this, EventArgs.Empty);

			TestFilterPage.FilterDataGrid.SelectedIndex = 2;
			TestFilterPage.OnLoad(EventArgs.Empty);
			TestFilterPage.SearchControl.SearchResultsDataGrid_PageIndexChanged(0);
			TestFilterPage.HandleOkButtonClickInternal();
			AssertEquals("Data Grid should display first page", 0, TestFilterPage.FilterDataGrid.CurrentPageIndex);
			AssertEquals("OKFunctionArguments should return third row", "'003'", TestFilterPage.OKFunctionArgumentsInternal[0]);

			TestFilterPage.FilterDataGrid.SelectedIndex = 3;
			TestFilterPage.SearchControl.IsPostBack = true;
			TestFilterPage.OnLoad(EventArgs.Empty);
			TestFilterPage.SearchControl.SearchResultsDataGrid_PageIndexChanged(1);
			AssertEquals("Data Grid should display second page", 1, TestFilterPage.FilterDataGrid.CurrentPageIndex);
			AssertEquals("OK FunctionArguments should return ninth row", "'009'", TestFilterPage.OKFunctionArgumentsInternal[0]);

			TestFilterPage.FindButton_Click(this, EventArgs.Empty);
			AssertEquals("Data Grid should display first page after new search", 0, TestFilterPage.FilterDataGrid.CurrentPageIndex);
		}

		#endregion TestNewSearchShowsFirstPageOfResults

		#region TestResources

		public void TestScriptResource()
		{
			string version = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(TestFilterPage.GetType().Assembly, typeof(AssemblyFileVersionAttribute))).Version;
			string expRuntimeDirectory = String.Format(@"/Runtime/Enterprise.ZArchitecture.Web.GUI/{0}/ZPage/ZIFramePage/ZFilterPage/", version).Replace(".", "_");
			AssertEquals("GridResizeScript", String.Format("{0}{1}", expRuntimeDirectory, "ZFilterPage_GridResizeScript.js"), TestFilterPage.GridResizeScript.FileName);
		}

		#endregion TestResources

		#region TestSearchControlPageSizeMaxRecords

		public void TestSearchControlPageSizeMaxRecords()
		{
			AssertEquals("The page size of the search control must be 25 (the value stored in the registry).",
				25, TestFilterPage.SearchControl.PageSize);

			AssertEquals("The maximum number of rows of the search control must be 1000 (the value stored in the registry).",
				1000, TestFilterPage.SearchControl.MaxRows);
		}

		public void TestSearchControlSearchResultsDataGridPageSizeMaxRecords()
		{
			var searchControl = TestFilterPage.SearchControl;
			var searchResultsDataGrid = searchControl.SearchResultsDataGrid;
			searchControl.IsPostBack = true;
			TestFilterPage.OnLoad(EventArgs.Empty);
			TestFilterPage.FindButton_Click(this, EventArgs.Empty);

			AssertEquals("The page size of the search control data grid must be 25 (the value stored in the registry).",
				25, searchResultsDataGrid.PageSize);

			AssertEquals("The maximum number of rows of the search control data grid must be 1000 (the value stored in the registry).",
				1000, searchResultsDataGrid.MaxRows);
		}

		#endregion

		#region Test Scripts

		public void TestGridResizeScriptBlock()
		{
			string version = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(TestFilterPage.GetType().Assembly, typeof(AssemblyFileVersionAttribute))).Version;
			string expRuntimeDirectory = String.Format(@"/Runtime/Enterprise.ZArchitecture.Web.GUI/{0}/ZPage/ZIFramePage/ZFilterPage/", version).Replace(".", "_");
			string expScriptBlock = String.Format("<script type=\"text/javascript\" src=\"{0}{1}\"></script>", expRuntimeDirectory, "ZFilterPage_GridResizeScript.js");
			AssertEquals("GridResizeScriptBlock", expScriptBlock, TestFilterPage.GridResizeScriptBlock);
		}

		public void TestGridResizeScriptHandler()
		{
			AssertEquals("ResultGridDiv NamingContainer", TestFilterPage.SearchControl, TestFilterPage.ResultsGridDiv.NamingContainer);
			AssertEquals("OKButton NamingContainer", TestFilterPage.ID, TestFilterPage.OKButton.NamingContainer.ID);
			AssertEquals("CancelButton NamingContainer", TestFilterPage.ID, TestFilterPage.CancelButton.NamingContainer.ID);
			string expHandler =
				"<SCRIPT type=\"text/javascript\">window.onload = function() { " +
				String.Format("ZFilterPage_GridResize('{0}','{1}')", TestFilterPage.ResultsGridDiv.ClientID, TestFilterPage.CancelButton.ClientID) +
				"; };</SCRIPT>";
			AssertEquals("GridResizeScriptHandler", expHandler, TestFilterPage.GridResizeScriptHandler);
		}

		#endregion Test Scripts

		[HttpContextEnabledTest]
		public void TestInvalidQueryString()
		{
			var filterPage = new ZTestFilterPage();
			filterPage.SetServerMappedPathForTest(TestRuntimeDirectory.DirectoryName);
			HttpContext.Current.Request.QueryString[ZFindBox.ModuleIDQuery] = "NotAModuleID";

			filterPage.OnInit(EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals($"{TestFilterPage.AppInstance.ErrorPage}?invalidQuery=true", HttpContext.Current.Response.RedirectLocation);
		}

		#region Overrides

		public override void TestGetNewDataSource()
		{
			BusinessObject dataSource = TestFilterPage.GetNewDataSourceInternal();
			AssertNotNull(dataSource);
			AssertEquals("TestDataSource differs from expected type", ExpectedBusinessObjectType, dataSource.GetType());
		}

		#endregion Overrides

		#region TestFindAutomaticallyIfRequired

		public void TestFindAutomaticallyIfRequired()
		{
			TestFilterPage.OnLoad(EventArgs.Empty);
			Assert("Find Button is not clicked", !TestFilterPage.SearchControl.FindButtonClicked);

			HttpContext.Current.Request.QueryString[ZFilterPage.ParentPKQuery] = ZGuid.Empty.ToString();
			TestFilterPage.OnLoad(EventArgs.Empty);
			Assert("Find Button is clicked", TestFilterPage.SearchControl.FindButtonClicked);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ExpCancelButton.Attributes[nameof(HtmlTextWriterAttribute.Class)] = "Button";
			ExpCancelButton.Style.Add("height", ZFilterStripConstants.Controls.ButtonHeight.ToString());
			TestFilterPage.fModuleID = WebModuleIDs.Dummy;
			TestFilterPage.SetServerMappedPathForTest(TestRuntimeDirectory.DirectoryName);
			TestFilterPage.ID = "TestFilterPage";
			TestFilterPage.OnInit(EventArgs.Empty);
			TestFilterPage.OnLoad(EventArgs.Empty);
		}

		public override Type ExpectedBusinessObjectType
		{
			get { return typeof(DummyFilterBusinessObject); }
		}

		protected override Control GetNewControl()
		{
			return new ZTestFilterPage();
		}

		protected override Panel ExpButtonsContainer
		{
			get
			{
				Panel result = base.ExpButtonsContainer;
				result.Style[HtmlTextWriterStyle.Position] = "absolute";
				result.Style["right"] = "0px";
				result.Style["bottom"] = "0px";

				return result;
			}
		}

		protected ZTestFilterPage TestFilterPage
		{
			get { return (ZTestFilterPage)Control; }
		}

		protected DummyBusinessObjectCollection GenerateDummyDataSource()
		{
			DummyBusinessObjectCollection result = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject obj1 = result.AddNew();
			obj1.Z0_Code = "r1c1";
			obj1.Z0_Description = "r1c2";

			DummyBusinessObject obj2 = result.AddNew();
			obj2.Z0_Code = "r2c1";
			obj2.Z0_Description = "r2c2";

			DummyBusinessObject obj3 = result.AddNew();
			obj3.Z0_Code = "r3c1";
			obj3.Z0_Description = "r3c2";

			return result;
		}

		protected void InsertDummyBusinessObjectToDB()
		{
			InsertDummyBusinessObjectToDB(1);
		}

		protected void InsertDummyBusinessObjectToDB(int rows)
		{
			for (int i = 0; i < rows; i++)
			{
				DummyBusinessObject newItem = Factory.New<DummyBusinessObject>();
				newItem.Z0_Code = "C00" + i.ToString();
				newItem.Z0_NVarChar = "NVarChar Test" + i.ToString();
				newItem.Z0_Description = "WebDummy";
			}
			Factory.Save();
		}

		protected void SetupDummyGrid()
		{
			TestFilterPage.FilterDataGrid.Columns.Clear();
			foreach (DataGridColumn column in TestFilterPage.SearchControl.Module.DefaultGridColumnFields)
			{
				if (column is ZGroupColumn)
				{
					foreach (DataGridColumn groupMember in ((ZGroupColumn)column).GroupMembers)
					{
						TestFilterPage.FilterDataGrid.Columns.Add(groupMember);
					}
				}
				else
				{
					TestFilterPage.FilterDataGrid.Columns.Add(column);
				}
			}
		}

		protected WebDummyModule TestModule
		{
			get { return new WebDummyModule(WebFactory); }
		}

		protected BusinessObjectFactory WebFactory
		{
			get { return Factory; }
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		void CreateDummyObjects(int numberOfObjects)
		{
			for (int i = 1; i < numberOfObjects + 1; i++)
			{
				DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Code = String.Format("{0:D3}", i);
				dummy.Z0_Description = "WebDummy";
			}
			Factory.Save();
		}

		void SetupGridForDummyBizOCollection(bool allowPaging, int pageSize)
		{
			// Setup the Datagrid to bind to the correct fields
			ButtonColumn column1 = new ButtonColumn();
			column1.DataTextField = DummyBizoSchema.Z0_Code.Name;
			column1.ButtonType = ButtonColumnType.LinkButton;
			BoundColumn column2 = new BoundColumn();
			column2.DataField = DummyBizoSchema.Z0_Description.Name;
			TestFilterPage.FilterDataGrid.Columns.Add(column1);
			TestFilterPage.FilterDataGrid.Columns.Add(column2);
			TestFilterPage.FilterDataGrid.AllowPaging = allowPaging;
			TestFilterPage.FilterDataGrid.PageSize = pageSize;
		}

		#endregion
	}
}
