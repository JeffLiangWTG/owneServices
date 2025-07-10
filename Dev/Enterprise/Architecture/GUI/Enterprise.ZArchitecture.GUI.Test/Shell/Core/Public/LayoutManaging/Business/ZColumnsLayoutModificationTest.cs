using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZColumnsLayoutModificationTest : TestCaseWithDummy
	{
		public void TestValidateAndGetErrorsForSavingLayout()
		{
			var layoutManageable = new ZColumnsLayoutModification(new List<ICustomizableColumn>(), Factory, null, "xxx-yyy");
			var reason = layoutManageable.ValidateAndGetErrorsForSavingLayout();
			AssertEquals("No columns to save", ZColumnsLayoutModification.NoColumnsToSave, reason);

			layoutManageable = new ZColumnsLayoutModification(new List<ICustomizableColumn> { new CustomizableColumn() }, Factory, null, "xxx-yyy");
			reason = layoutManageable.ValidateAndGetErrorsForSavingLayout();
			Assert(string.IsNullOrEmpty(reason));
		}

		public void TestGetReasonLayoutNameNotAllowed()
		{
			var layoutManageable = new ZColumnsLayoutModification(new List<ICustomizableColumn>(), Factory, null, "xxx-yyy");

			var reason = layoutManageable.GetReasonLayoutNameNotAllowed("Default", false);
			AssertEquals("Cannot name a new layout to 'Default'", ZColumnsLayoutModification.DefaultAsFilterNameIsNotAcceptable, reason);

			reason = layoutManageable.GetReasonLayoutNameNotAllowed("1", false);
			Assert("Any other name is ok", string.IsNullOrEmpty(reason));
		}

		public void TestAddNewLayoutStorage()
		{
			const string gridID = "xxx-yyy";

			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true },
				};

			var layoutManageable = new ZColumnsLayoutModification(columns, Factory, null, gridID);

			var layoutStorage = layoutManageable.AddNewLayoutStorage();
			AssertEquals("RelatedEntityID should have been set", EnvProxy.Instance.CurrentUser.PK, layoutStorage.S9_RelatedEntityID);
			AssertEquals("Current company should have been set", EnvProxy.Instance.CurrentCompany.PK, layoutStorage.S9_GC);
			AssertEquals("Save columns", true, layoutStorage.S9_SaveColumnLayout);
			AssertEquals("Not published", false, layoutStorage.S9_IsPublished);
		}

		public void TestFindLayoutByPK()
		{
			const string gridID = "xxx-yyy";

			var columns = new List<ICustomizableColumn>
			{
				new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
				new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true },
			};

			var layoutManageable = new ZColumnsLayoutModification(columns, Factory, null, gridID);
			var layoutStorage = layoutManageable.AddNewLayoutStorage();

			var foundLayout = layoutManageable.FindLayout(layoutStorage.PK);
			AssertNotNull(foundLayout);
		}

		public void TestGetLayoutDetailTree()
		{
			const string gridID = "xxx-yyy";

			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true },
				};

			var serialiser = new DataGridLayoutDataSetSerialiser();
			var layout1 = Factory.New<StmModuleFilter>();
			using (var layoutStream = serialiser.GetLayoutStream(columns))
			{
				layout1.S9_ColumnLayoutData = layoutStream.ToArray();
				layout1.S9_FilterName = "Layout1";
				layout1.S9_ModuleID = gridID;
				layout1.S9_SaveColumnLayout = true;
			}

			columns[0].IsVisible = true;
			columns[1].IsVisible = false;

			var layout2 = Factory.New<StmModuleFilter>();
			using (var layoutStream = serialiser.GetLayoutStream(columns))
			{
				layout2.S9_ColumnLayoutData = layoutStream.ToArray();
				layout2.S9_FilterName = "Layout2";
				layout2.S9_ModuleID = gridID;
				layout2.S9_SaveColumnLayout = true;
			}

			Factory.Save();

			columns[0].IsVisible = true;
			columns[1].IsVisible = true;

			var layoutManageable = new ZColumnsLayoutModification(columns, Factory, null, gridID);

			var layoutTreeViews = new List<ILayoutDetailTreeNode>(layoutManageable.GetLayoutDetailTree(layout1));
			AssertEquals("One column", 1, layoutTreeViews.Count);
			AssertEquals("X2", layoutTreeViews[0].UniqueID);

			layoutTreeViews = new List<ILayoutDetailTreeNode>(layoutManageable.GetLayoutDetailTree(layout2));
			AssertEquals("One column", 1, layoutTreeViews.Count);
			AssertEquals("X1", layoutTreeViews[0].UniqueID);
		}

		public void TestGetLayoutPublished_NotPublished()
		{
			const string gridID = "xxx-yyy";

			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true },
				};

			var layoutA = Factory.New<StmModuleFilter>();
			layoutA.S9_FilterName = "A";
			layoutA.S9_ModuleID = gridID;
			layoutA.S9_IsPublished = true;

			var layoutB = Factory.New<StmModuleFilter>();
			layoutB.S9_FilterName = "B";
			layoutB.S9_ModuleID = gridID;
			layoutB.S9_IsPublished = false;

			var layoutC = Factory.New<StmModuleFilter>();
			layoutC.S9_FilterName = "C";
			layoutC.S9_ModuleID = gridID;
			layoutC.S9_IsPublished = false;

			Factory.Save();

			var layoutManageable = new ZColumnsLayoutModification(columns, Factory, null, gridID);

			var publishedFilters = new List<StmModuleFilter>(layoutManageable.GetLayouts(true));
			AssertEquals(1, publishedFilters.Count);
			AssertEquals(layoutA.PK, publishedFilters[0].PK);

			var unpublishedFilters = new List<StmModuleFilter>(layoutManageable.GetLayouts(false));
			AssertEquals(2, unpublishedFilters.Count);
			Assert(layoutB.PK == unpublishedFilters[0].PK || layoutC.PK == unpublishedFilters[0].PK);
			Assert(layoutB.PK == unpublishedFilters[1].PK || layoutC.PK == unpublishedFilters[1].PK);
		}

		public void TestUseDifferentFactoryToGridDataSource()
		{
			const string gridID = "xxx-yyy";

			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", IsVisible = false },
					new CustomizableColumn { Caption = "C2", IsVisible = true },
				};

			AssertEquals("PreCondition:Dummy is not in database", false, Dummy.IsInDatabase);

			var customiseBizO = new ZGridCustomiseBizObj(gridID, null);

			AssertNotEquals("Different factory", Dummy.Factory, customiseBizO.Factory);

			using (var customiseForm = new ZColumnsCustomiseTester(columns, null, true, customiseBizO))
			{
				var gridLayoutManageable = customiseForm.GetGridLayoutManageableExposed();
				AssertNotEquals("Different factory", Dummy.Factory, gridLayoutManageable.Factory);

				new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "HUFDYUI", true, false, SaveColumnLayout.Yes);
				AssertEquals("Dummy is not in database as SaveLayout is done in a different factory", false, Dummy.IsInDatabase);
			}
		}
	}
}
