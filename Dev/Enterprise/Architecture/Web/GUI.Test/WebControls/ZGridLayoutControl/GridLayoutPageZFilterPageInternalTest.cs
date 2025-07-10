using System;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class GridLayoutPageZFilterPageInternalTest : ZIFramePageTest
	{
		public override void TestGetNewDataSource()
		{
			TestGridLayoutPage.RequestQueryStringInternal[ZGridLayoutControl.GridCurrentLayoutKey] = "0";
			TestGridLayoutPage.RequestQueryStringInternal[ZGridLayoutControl.GridModuleIDKey] = WebModuleIDs.Dummy.ToString();

			BusinessObject dataSource = TestGridLayoutPage.GetNewDataSourceInternal();
			AssertNotNull("GridLayoutPage DataSource should not be null", dataSource);

			GridLayoutContainer container = dataSource as GridLayoutContainer;
			AssertNotNull("Should be GridLayoutContainer", container);

			AssertEquals("Five Columns", 5, container.AllColumns.Count);
			AssertEquals("Two Columns in Default", 2, container.DefaultLayout.Count);
			AssertEquals("CurrentLayout should contain one column", 1, container.CurrentLayout.Count);
			AssertEquals("Should be first column in the CurrentLayoutString", "0", container.CurrentLayoutString);
			AssertEquals("Should be two columns available", 2, container.AvailableColumns.Count);
		}

		public void TestGetNewDataSource_SetCurrentLayoutToDefaultWhenNoLayoutProvided()
		{
			TestGridLayoutPage.RequestQueryStringInternal[ZGridLayoutControl.GridCurrentLayoutKey] = "";
			TestGridLayoutPage.RequestQueryStringInternal[ZGridLayoutControl.GridModuleIDKey] = WebModuleIDs.Dummy.ToString();

			BusinessObject dataSource = TestGridLayoutPage.GetNewDataSourceInternal();
			AssertNotNull("GridLayoutPage DataSource should not be null", dataSource);

			GridLayoutContainer container = dataSource as GridLayoutContainer;
			AssertNotNull("Should be GridLayoutContainer", container);

			AssertEquals("Five Columns", 5, container.AllColumns.Count);
			AssertEquals("Two Columns in Default", 2, container.DefaultLayout.Count);
			AssertEquals("CurrentLayout should contain two columns", 2, container.CurrentLayout.Count);
			AssertEquals("Should be both columns in the CurrentLayoutString", "0,4", container.CurrentLayoutString);
			AssertEquals("Should be one column available", 1, container.AvailableColumns.Count);
		}

		[ExpectExceptionMessage(typeof(ZException), "Module ID : SomeCrap resolves to null. Web GridLayoutControl Module must be ZGenericFilterGridModule.")]
		public void TestGetNewDataSource_ExceptionThrownWhenZModuleIsInvalid()
		{
			TestGridLayoutPage.RequestQueryStringInternal[ZGridLayoutControl.GridModuleIDKey] = "SomeCrap";
			BusinessObject dataSource = TestGridLayoutPage.GetNewDataSourceInternal();
		}

		public void TestGetNewDataSource_ExcludesColumnsInAGroupFromLayout()
		{
			TestGridLayoutPage.RequestQueryStringInternal[ZGridLayoutControl.GridCurrentLayoutKey] = "0,1,2,3,4";
			TestGridLayoutPage.RequestQueryStringInternal[ZGridLayoutControl.GridModuleIDKey] = WebModuleIDs.Dummy.ToString();

			BusinessObject dataSource = TestGridLayoutPage.GetNewDataSourceInternal();
			AssertNotNull("GridLayoutPage DataSource should not be null", dataSource);

			GridLayoutContainer container = dataSource as GridLayoutContainer;
			AssertNotNull("Should be GridLayoutContainer", container);

			AssertEquals("Should exclude group member columns from the layout", "0,3,4", container.CurrentLayoutString);
		}

		public void TestSelectOneButton_Click()
		{
			LoadAndAssertDataSource();
			GridLayoutContainer container = TestGridLayoutPage.DataSource as GridLayoutContainer;

			container.SelectedAvailableColumn = "5";
			PerformClick(TestGridLayoutPage.SelectOneButton);

			AssertEquals("CurrentLayout should contain two columns", 2, container.CurrentLayout.Count);
			AssertEquals("Should be both columns in the CurrentLayoutString", "0,4", container.CurrentLayoutString);
			AssertEquals("Should be one column available", 1, container.AvailableColumns.Count);
		}

		public void TestRemoveOneButton_Click()
		{
			TestSelectOneButton_Click();
			GridLayoutContainer container = TestGridLayoutPage.DataSource as GridLayoutContainer;

			container.SelectedLayoutColumn = "5";
			PerformClick(TestGridLayoutPage.RemoveOneButton);

			AssertEquals("CurrentLayout should contain one column", 1, container.CurrentLayout.Count);
			AssertEquals("Should be second column in the CurrentLayoutString", "0", container.CurrentLayoutString);
			AssertEquals("Should be two columns available", 2, container.AvailableColumns.Count);
		}

		public void TestDefaultButton_Click()
		{
			LoadAndAssertDataSource();
			GridLayoutContainer container = TestGridLayoutPage.DataSource as GridLayoutContainer;

			PerformClick(TestGridLayoutPage.DefaultButton);

			AssertEquals("CurrentLayout should contain two columns", 2, container.CurrentLayout.Count);
			AssertEquals("Should be both columns in the CurrentLayoutString", "0,4", container.CurrentLayoutString);
			AssertEquals("Should be one column available", 1, container.AvailableColumns.Count);
		}

		public void TestCancelButton_ClickRollbackChanges()
		{
			TestSelectOneButton_Click();
			GridLayoutContainer container = TestGridLayoutPage.DataSource as GridLayoutContainer;

			PerformClick(TestGridLayoutPage.CancelButton);

			AssertEquals("CurrentLayout should contain one column", 1, container.CurrentLayout.Count);
			AssertEquals("Should be second column in the CurrentLayoutString", "0", container.CurrentLayoutString);
			AssertEquals("Should be two columns available", 2, container.AvailableColumns.Count);
		}

		public void TestMoveUpButton_Click()
		{
			TestDefaultButton_Click();
			GridLayoutContainer container = TestGridLayoutPage.DataSource as GridLayoutContainer;

			container.SelectedLayoutColumn = "5";
			PerformClick(TestGridLayoutPage.MoveUpButton);

			AssertEquals("Should be moved in the CurrentLayoutString", "4,0", container.CurrentLayoutString);
		}

		public void TestMoveDownButton_Click()
		{
			TestDefaultButton_Click();
			GridLayoutContainer container = TestGridLayoutPage.DataSource as GridLayoutContainer;

			container.SelectedLayoutColumn = "-1";
			PerformClick(TestGridLayoutPage.MoveDownButton);

			AssertEquals("Should be moved in the CurrentLayoutString", "4,0", container.CurrentLayoutString);
		}

		public override void TestOKFunctionArguments()
		{
			AssertNotNull("OKFunctionArguments should be not null", TestGridLayoutPage.OKFunctionArgumentsInternal);
			AssertEquals("Should be one argument", 1, TestGridLayoutPage.OKFunctionArgumentsInternal.Length);
			AssertEquals("Should be Default Layout", "'0'", TestGridLayoutPage.OKFunctionArgumentsInternal[0]);

			GridLayoutContainer container = TestGridLayoutPage.DataSource as GridLayoutContainer;
			AssertNotNull("GridLayoutContainer should not be null", container);
			container.SelectedAvailableColumn = "5";
			container.AddToLayout();

			AssertNotNull("OKFunctionArguments should be not null", TestGridLayoutPage.OKFunctionArgumentsInternal);
			AssertEquals("Should be one argument", 1, TestGridLayoutPage.OKFunctionArgumentsInternal.Length);
			AssertEquals("Should be customised Layout", "'0,4'", TestGridLayoutPage.OKFunctionArgumentsInternal[0]);
		}

		public override void TestCancelFunctionArguments()
		{
			AssertNull(TestGridLayoutPage.CancelFunctionArgumentsInternal);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestGridLayoutPage.SetServerMappedPathForTest(TestRuntimeDirectory.DirectoryName);
			TestGridLayoutPage.ID = "TestGridLayoutPage";
			TestGridLayoutPage.OnInit(EventArgs.Empty);
			TestGridLayoutPage.OnLoad(EventArgs.Empty);
		}

		TestGridLayoutPage TestGridLayoutPage
		{
			get { return (TestGridLayoutPage)Control; }
		}

		public override Type ExpectedBusinessObjectType
		{
			get { return typeof(GridLayoutContainer); }
		}

		protected override Control GetNewControl()
		{
			return new TestGridLayoutPage();
		}

		protected override bool OKButtonIsVisible
		{
			get { return true; }
		}

		void LoadAndAssertDataSource()
		{
			TestGridLayoutPage.OnLoad(EventArgs.Empty);

			GridLayoutContainer container = TestGridLayoutPage.DataSource as GridLayoutContainer;
			AssertNotNull("Should be GridLayoutContainer", container);

			AssertEquals("Five Columns", 5, container.AllColumns.Count);
			AssertEquals("Two Columns in Default", 2, container.DefaultLayout.Count);
			AssertEquals("CurrentLayout should contain one column", 1, container.CurrentLayout.Count);
			AssertEquals("Should be first column in the CurrentLayoutString", "0", container.CurrentLayoutString);
			AssertEquals("Should be two columns available", 2, container.AvailableColumns.Count);
		}

		void PerformClick(Button button)
		{
			typeof(Button).InvokeMember("OnClick", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, button, new object[] { EventArgs.Empty });
		}

		#endregion
	}
}
