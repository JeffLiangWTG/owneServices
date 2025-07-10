using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

#if WINZOR
	using WinzorFramework;
#endif
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.StripControl;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZFilterStripBaseControlForTest : ZFilterStripBaseControl
	{
		public ZFilterStripBaseControlForTest(ZGrid grid, FilterStripBusinessObject filterBusinessObject)
			: base(grid, filterBusinessObject)
		{
			finishedConstruction = true;
		}

		public ZFilterStripBaseControlForTest(ZGrid grid, FilterStripBusinessObject filterBusinessObject, string saveLayoutName, bool saveLayoutIsUserDefinedFilter)
				: this(grid, filterBusinessObject)
		{
			this.saveLayoutName = saveLayoutName;
			this.saveLayoutIsUserDefinedFilter = saveLayoutIsUserDefinedFilter;
		}

		public ZFilterStripBaseControlForTest(FilterStripBusinessObject filterBusinessObject)
			: base(filterBusinessObject) { }

		readonly string saveLayoutName;
		readonly bool saveLayoutIsUserDefinedFilter;

		public override FilterStripBusinessObject FilterBusinessObject
		{
			get
			{
				FilterStripBusinessObject result;

				if (!finishedConstruction)
				{
					AccessingFilterBusinessObjectDuringConstruction = true;
					result = base.FilterBusinessObject;
					AccessingFilterBusinessObjectDuringConstruction = false;
				}
				else
				{
					result = base.FilterBusinessObject;
				}

				return result;
			}
		}

		public new List<ZFilterStrip> Strips
		{
			get { return (List<ZFilterStrip>)typeof(ZFilterStripControl).GetField("Strips", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this); }
		}

		public override ZGrid RelatedGrid
		{
			get
			{
				if (!finishedConstruction && AccessingFilterBusinessObjectDuringConstruction)
				{
					TryToSetQueryObjectTypeDuringConstruction = true;
				}

				return base.RelatedGrid;
			}
		}

		bool finishedConstruction { get; }
		bool AccessingFilterBusinessObjectDuringConstruction { get; set; }

		public bool TryToSetQueryObjectTypeDuringConstruction { get; set; }

		public void SaveLayout_Exposed()
		{
			SaveLayout();
		}

		protected override void OnSearchPerformed(bool showError, bool didSearch, bool isManualSearch, Form form)
		{
			SearchPerformedCount++;
			base.OnSearchPerformed(showError, didSearch, isManualSearch, form);
		}

		public int SearchPerformedCount { get; set; }

		public void ManageLayouts_Exposed(bool shouldShowUserDefinedFilter)
		{
			ManageLayouts(shouldShowUserDefinedFilter);
		}

		public void AddOrUpdateExistingFindDropListItemExposed(StmModuleFilter filter)
		{
			this.AddOrUpdateExistingFindDropListItem(filter);
		}

		protected override SaveLayoutUserQueryHandler GetQueryHander()
		{
			return new SaveLayoutUserQueryHandler_ForTest(saveLayoutName, saveLayoutIsUserDefinedFilter);
		}

		protected override ManageLayoutsForm GetManageLayoutsForm(FilterStripBusinessObject filterBusinessObject, bool canSaveColumnLayouts, bool canSaveGridColours, bool shouldShowUserDefinedFilter, GridColourScheme colourScheme = null)
		{
			return new ManageFiltersFormTest.TestManageLayoutsForm(filterBusinessObject, canSaveColumnLayouts, canSaveGridColours, shouldShowUserDefinedFilter, colourScheme) { NewLayoutNameForRenaming = NewLayoutNameForRenaming };
		}

		public string NewLayoutNameForRenaming { get; set; }

		public List<ZFilterStrip> Strips_Exposed { get { return base.Strips; } }
	}

	class SaveLayoutUserQueryHandler_ForTest : SaveLayoutUserQueryHandler
	{
		public SaveLayoutUserQueryHandler_ForTest(string layoutName, bool isUserDefinedFilter)
		{
			this.layoutName = layoutName;
			this.isUserDefinedFilter = isUserDefinedFilter;
		}

		readonly string layoutName;
		readonly bool isUserDefinedFilter;

		protected override SaveLayoutBizO GetSaveLayoutBizObj(IModifyModuleAndGridLayout layoutManageable)
		{
			var result = base.GetSaveLayoutBizObj(layoutManageable);
			result.LayoutName = layoutName;
			result.IsUserDefinedFilter = isUserDefinedFilter;

			return result;
		}

		protected override SaveLayoutForm GetSaveLayoutForm(SaveLayoutBizO saveLayoutBizObj, bool shouldShowSaveColumnCheckBox, bool shouldShowSaveAsUserDefinedFilter)
		{
			var form = base.GetSaveLayoutForm(saveLayoutBizObj, shouldShowSaveColumnCheckBox, shouldShowSaveAsUserDefinedFilter);
			form.IsOkToSave = true;

			return form;
		}
	}

	sealed class ZFilterStripBaseControlTest : TestCaseWithFactory
	{
		public void TestControlShowModuleAccessErrorIfModuleAccessIsNotGrantedAndNoCodeOrDescriptionProperty()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.MailItem))
			{
				var security = Env.Security.FindOrCreateAccessModuleCheckPoint(module.SecurityCheckpoint);
				security.IsAllowed = false;
				using (var control = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
				{
					control.FilterStripLoaded();
					AssertEquals(security.ErrorMessageForNotAllowed.ToString(), UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(control.IsFilterReadonly);

					var count = control.SearchPerformedCount;
					control.FirePerformSearch();
					AssertEquals(count, control.SearchPerformedCount);
				}
			}
		}

		#region TestDoNotTryToSetQueryObjectTypeDuringConstruction

		public void TestDoNotTryToSetQueryObjectTypeDuringConstruction()
		{
			using (var form = GetNewFilterForm())
			using (var control = new ZFilterStripBaseControlForTest(form.Grid, new DummyFilterStripBusinessObject()))
			{
				Assert(!control.TryToSetQueryObjectTypeDuringConstruction);
			}
		}

		#endregion

		#region Test favorites

		[GuiTest]
		public void TestClickOnFavoriteImageTogglesFavorites()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			var layout = dummyFilterStripBizO.Layouts.AddNew();
			layout.S9_FilterName = "This one";

			using (var form = new Form())
			using (var filterControl = GetControlForFilterStripBizo(dummyFilterStripBizO))
			{
				form.Controls.Add(filterControl);
				form.Show();

				Application.DoEvents();

				Assert("PRE: layout is not favorite", !IsFavorite(dummyFilterStripBizO, layout));
				AssertEquals("There should be 2 elements in total", 2, filterControl.ToolStripFindDropButton_ForTest.DropDownItems.Count);

				ClickOnFavoriteImage(filterControl, layout);
				Assert("After clicking the image of a non-favorite layout it should become a favorite", IsFavorite(dummyFilterStripBizO, layout));

				ClickOnFavoriteImage(filterControl, layout);
				Assert("After clicking the image of a favorite layout it should no longer be a favorite", !IsFavorite(dummyFilterStripBizO, layout));
			}
		}

		[GuiTest]
		public void TestMultipleFavoritesStillTogglesFavorites()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();
			var favoriteLayout1 = dummyFilterStripBizO.Layouts.AddNew();
			favoriteLayout1.S9_FilterName = "Bart";

			var favoriteLayout2 = dummyFilterStripBizO.Layouts.AddNew();
			favoriteLayout2.S9_FilterName = "Homer";

			var layout1 = dummyFilterStripBizO.Layouts.AddNew();
			layout1.S9_FilterName = "Moe";

			var layout2 = dummyFilterStripBizO.Layouts.AddNew();
			layout2.S9_FilterName = "Apu";

			dummyFilterStripBizO.AddOrRemoveFavoriteFilter(favoriteLayout1);
			dummyFilterStripBizO.AddOrRemoveFavoriteFilter(favoriteLayout2);

			using (var form = new Form())
			using (var filterControl = GetControlForFilterStripBizo(dummyFilterStripBizO))
			{
				form.Controls.Add(filterControl);
				form.Show();

				Application.DoEvents();
				CombineAssertions(() =>
				{
					AssertEquals("There should be 6 elements in total", 6, filterControl.ToolStripFindDropButton_ForTest.DropDownItems.Count);
					AssertEquals("Very first element should be the category title for Favorites", "Favorites", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[0].Text.Trim());
					AssertEquals("1st filter should be 'Bart'", "Bart", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[1].Text.Trim());
					AssertEquals("2nd filter should be 'Homer'", "Homer", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[2].Text.Trim());
					AssertEquals("Very first element should be the category title for private layouts", "My Filter Layouts", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[3].Text.Trim());
					AssertEquals("3rd filter should be 'Apu'", "Apu", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[4].Text.Trim());
					AssertEquals("4th filter should be 'Moe'", "Moe", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[5].Text.Trim());
				});

				ClickOnFavoriteImage(filterControl, layout1);
				CombineAssertions(() =>
				{
					AssertEquals("There should be 6 elements in total", 6, filterControl.ToolStripFindDropButton_ForTest.DropDownItems.Count);
					AssertEquals("Very first element should be the category title for Favorites", "Favorites", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[0].Text.Trim());
					AssertEquals("1st filter should be 'Bart'", "Bart", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[1].Text.Trim());
					AssertEquals("2nd filter should be 'Homer'", "Homer", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[2].Text.Trim());
					AssertEquals("3rd filter should be 'Moe'", "Moe", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[3].Text.Trim());
					AssertEquals("Very first element should be the category title for private layouts", "My Filter Layouts", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[4].Text.Trim());
					AssertEquals("4th filter should be 'Apu'", "Apu", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[5].Text.Trim());
				});

				ClickOnFavoriteImage(filterControl, layout2);
				CombineAssertions(() =>
				{
					AssertEquals("There should be 5 elements in total", 5, filterControl.ToolStripFindDropButton_ForTest.DropDownItems.Count);
					AssertEquals("Very first element should be the category title for Favorites", "Favorites", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[0].Text.Trim());
					AssertEquals("1st filter should be 'Apu'", "Apu", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[1].Text.Trim());
					AssertEquals("2nd filter should be 'Bart'", "Bart", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[2].Text.Trim());
					AssertEquals("3rd filter should be 'Homer'", "Homer", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[3].Text.Trim());
					AssertEquals("4th filter should be 'Moe'", "Moe", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[4].Text.Trim());
				});
			}
		}

		[GuiTest]
		public void TestFavoritesFullShowsWarning()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();
			StmModuleFilter layout = null;
			for (var i = 1; i < 11; i++)
			{
				layout = dummyFilterStripBizO.Layouts.AddNew();
				layout.S9_FilterName = "layout" + i;
				dummyFilterStripBizO.AddOrRemoveFavoriteFilter(layout);
			}

			var layoutExtra = dummyFilterStripBizO.Layouts.AddNew();
			layoutExtra.S9_FilterName = "layoutExtra";

			using (var form = new Form())
			using (var filterControl = GetControlForFilterStripBizo(dummyFilterStripBizO))
			{
				form.Controls.Add(filterControl);
				form.Show();

				Application.DoEvents();

				AssertEquals("13 layouts expected", 13, filterControl.ToolStripFindDropButton_ForTest.DropDownItems.Count);
				AssertEquals("There should be the category title for Favorites", "Favorites", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[0].Text.Trim());
				AssertEquals("'My Filter Layouts' is second from last layout", "My Filter Layouts", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[11].Text.Trim());
				AssertEquals("Last layout should be 'layoutExtra'", layoutExtra.S9_FilterName, filterControl.ToolStripFindDropButton_ForTest.DropDownItems[12].Text.Trim());

				ClickOnFavoriteImage(filterControl, layoutExtra);
				AssertEquals("Warning message expected", string.Format("Warning Limit of 10 favorites reached.{0}You first need to remove a favorite if you want to add '{1}'", System.Environment.NewLine, layoutExtra.S9_FilterName), ((UnitTestUserNotification)Globals.Message).LastMessage.ToString());

				ClickOnFavoriteImage(filterControl, layout);
				AssertEquals("'My Filter Layouts' has moved up one index", "My Filter Layouts", filterControl.ToolStripFindDropButton_ForTest.DropDownItems[10].Text.Trim());
				AssertEquals("'layout10' should have moved under the private filter layouts", layout.S9_FilterName, filterControl.ToolStripFindDropButton_ForTest.DropDownItems[11].Text.Trim());
				AssertEquals("'layoutExtra' is the last layout still", layoutExtra.S9_FilterName, filterControl.ToolStripFindDropButton_ForTest.DropDownItems[12].Text.Trim());

				ClickOnFavoriteImage(filterControl, layoutExtra);
				AssertEquals("'layoutExtra' should have now moved under the Favorites", layoutExtra.S9_FilterName, filterControl.ToolStripFindDropButton_ForTest.DropDownItems[10].Text.Trim());

				ClickOnFavoriteImage(filterControl, layout);
				AssertEquals("Warning message expected - we cant put 'layout10' back in the Favorites", string.Format("Warning Limit of 10 favorites reached.{0}You first need to remove a favorite if you want to add '{1}'", System.Environment.NewLine, layout.S9_FilterName), ((UnitTestUserNotification)Globals.Message).LastMessage.ToString());
			}
		}

		FilterStripControlTest.DummyZFilterStripControl GetControlForFilterStripBizo(FilterStripBusinessObject bizo)
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			return new FilterStripControlTest.DummyZFilterStripControl(collection, bizo);
		}

		class ZFilterToolStripMenuItemForTest : ZFilterToolStripMenuItem
		{
			public ZFilterToolStripMenuItemForTest(string text) : base(text)
			{
			}

#if !WINZOR
			protected override bool IsPointOnImage(Point p) => true;
#endif 
		}

		void ClickOnFavoriteImage(FilterStripControlTest.DummyZFilterStripControl filterControl, StmModuleFilter layout)
		{
			var layoutMenuItemForTest = new ZFilterToolStripMenuItemForTest(layout.S9_FilterName);
			layoutMenuItemForTest.Tag = layout;
			layoutMenuItemForTest.Owner = new ToolStrip();
#if WINZOR
			(layoutMenuItemForTest as IWinzorMenuItem).OnImageMouseEnter();
#endif
			filterControl.ItemClicked_Exposed(layoutMenuItemForTest, null);
			filterControl.ToolStripFindDropButtonExposed.HideDropDown();
#if WINZOR
			(layoutMenuItemForTest as IWinzorMenuItem).OnImageMouseLeave();
#endif
		}

		bool IsFavorite(FilterStripBusinessObject bizo, StmModuleFilter layout)
			=> bizo.FavoriteLayouts.Contains(layout);

#endregion

		#region TestFilterStripsPersistWhenFilterIsDeleted

		public void TestFilterStripsPersistWhenFilterIsDeleted()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.Grid.Visible = false;

				using (var form = new ZForm())
				using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject, "Filter Control", saveLayoutIsUserDefinedFilter: false))
				{
					form.Controls.Add(filterControl);
					form.Show();

					var strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Description");
					filterControl.SaveLayout_Exposed();
					filterControl.AddFilterStrip(strip);

					strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Code");
					filterControl.AddFilterStrip(strip);

					strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Number");
					filterControl.AddFilterStrip(strip);

					strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Bool");
					filterControl.AddFilterStrip(strip);

					strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Guid");
					filterControl.AddFilterStrip(strip);

					filterControl.SaveLayout_Exposed();

					// 5 from above + 1 thats always there ("<select something to filter by>")
					AssertEquals("All Filter strips should have been added", 6, module.FilterBusinessObject.FilterStrips.Count);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
					{
						var manageForm = (ManageFiltersFormTest.TestManageLayoutsForm)dialog;
						manageForm.Shown += (_, x_) =>
						{
							manageForm.DeleteSelectedLayoutExposed();
							manageForm.SaveButtonExposed.PerformClick();
						};
					});

					filterControl.ManageLayouts_Exposed(false);
					Application.DoEvents();

					// 5 from above + 1 thats always there ("<select something to filter by>")
					AssertEquals("Filter strips should not be removed after the filter has been deleted", 6, module.FilterBusinessObject.FilterStrips.Count);
				}
			}
		}

		#endregion

		#region TestCustomiseColumnsAfterFilterDeleted

		public void TestCustomiseColumnsAfterFilterDeleted()
		{
			// Need a DummyBizO for the TestGrid, can't use TestCaseWithDummy because tests that extend this test use TestCaseWithFactory
			var dummyPk = new ZGuid(Guid.NewGuid());
			DummyTableCreator.AddDummyBusinessObjectsToDB(dummyPk.ToGuid(), Guid.NewGuid());
			var dummyRow = new RowFactory().LoadFromPK(DummyBusinessObject.Schema.TableName, dummyPk);
			var dummy = new DummyBusinessObject(Factory, dummyRow);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.Grid.Visible = false;

				using (var form = new ZTestGridForm(dummy))
				using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject, "Filter Control", saveLayoutIsUserDefinedFilter: false))
				{
					form.Controls.Add(filterControl);
					form.Show();

					var grid = form.TabGrid;
					grid.LayoutCategoryPK = Guid.NewGuid();

					form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

					var serialiser = new DataGridLayoutDataSetSerialiser();

					var firstFilter = Factory.NewWithValidTestData<StmModuleFilter>();
					firstFilter.S9_ModuleID = new DataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter;
					firstFilter.S9_ColumnLayoutData = serialiser.GetLayoutStream(grid.Columns).ToArray();
					firstFilter.S9_FilterName = "filtery boi";

					var strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Description");
					filterControl.SaveLayout_Exposed();
					filterControl.AddFilterStrip(strip);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
					{
						var manageForm = (ManageFiltersFormTest.TestManageLayoutsForm)dialog;
						manageForm.Shown += (_, x_) =>
						{
							manageForm.DeleteSelectedLayoutExposed();
							manageForm.SaveButtonExposed.PerformClick();
						};
					});

					filterControl.ManageLayouts_Exposed(false);
					Application.DoEvents();

					Factory.Save();

					var customiseBizO = new ZGridCustomiseBizObj(new string[] { firstFilter.S9_ModuleID }, new string[] { firstFilter.S9_FilterName }, null, ZGuid.Empty);

					using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, customiseBizO))
					{
						customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 1;
						customiseForm.MoveColumnUpExposed();

						AssertEquals("Columns should be re-ordered", "Number", customiseForm.CurrentColumnsListBoxExposed.Items[0].ToString());
					}
				}
			}
		}

		#endregion

		#region TestSaveLayout

		public void TestSaveLayout_ZSaveExceptionShouldBeHandledProperly()
		{
			const string LayoutName = "This is a test layout";

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Z0_Description");
				var filter = (ModuleTextFilter)strip.CurrentModuleFilter;
				filter.Property = "filter";
				module.Grid.Visible = false;

				using (var control = new ZFilterStripBaseControlForTest(module.Grid, filterBizo, LayoutName, saveLayoutIsUserDefinedFilter: false))
				{
					control.SaveLayout_Exposed();
				}

				var layout = Factory.Load<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_FilterName, LayoutName)).Single();
				var userData = filterBizo.GetLayoutUserDataForColorScheme(layout);
				userData.S0_S9 = ZGuid.NewZGuid();

				using (var control = new ZFilterStripBaseControlForTest(module.Grid, filterBizo, "This will be a User Defined Filter Strip", saveLayoutIsUserDefinedFilter: true))
				{
					AssertExceptionThrown<ZSaveException>("ZSaveException is expected not ArgumentException", () => control.SaveLayout_Exposed());
				}
			}
		}

		public void TestSaveLayout_ShouldShowIsUserDefinedFilterCheckBox()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Z0_Description");
				var filter = (ModuleTextFilter)strip.CurrentModuleFilter;
				filter.Property = "filter";
				module.Grid.Visible = false;

				using (var control = new ZFilterStripBaseControlForTest(module.Grid, filterBizo, "This will be a normal layout", saveLayoutIsUserDefinedFilter: true))
				{
					bool? isUserDefinedFilterCheckBoxVisibility = null;

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
					{
						var saveLayoutForm = f as SaveLayoutForm;

						saveLayoutForm.Shown += (_, x_) =>
						{
							isUserDefinedFilterCheckBoxVisibility = saveLayoutForm.FindSingleOrDefault<ZCheckBox>("IsUserDefinedFilterCheckBox").Visible;
						};
					});

					control.SaveLayout_Exposed();

					AssertEquals("IsUserDefinedFilterCheckBox should be visible", true, isUserDefinedFilterCheckBoxVisibility);
				}
			}
		}

		public void TestSaveLayout_SaveAsUserDefinedFilterVisibility()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (new GlowIndexQueryEngineMock((mock) => { }))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.IsGlowIndexSearchAllowed = true;
				var strip = filterBizo.FilterStrips.AddNew("Z0_Description");
				var filter = (ModuleTextFilter)strip.CurrentModuleFilter;
				filter.Property = "filter";
				module.Grid.Visible = false;

				filterBizo.SearchType = SearchType.Sql;
				using (var control = new ZFilterStripBaseControlForTest(module.Grid, filterBizo, "This will be a normal layout", saveLayoutIsUserDefinedFilter: true))
				{
					bool? isUserDefinedFilterCheckBoxVisibility = null;

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
					{
						var saveLayoutForm = f as SaveLayoutForm;

						saveLayoutForm.Shown += (_, x_) =>
						{
							isUserDefinedFilterCheckBoxVisibility = saveLayoutForm.FindSingleOrDefault<ZCheckBox>("IsUserDefinedFilterCheckBox").Visible;
						};
					});

					control.SaveLayout_Exposed();

					AssertEquals("IsUserDefinedFilterCheckBox should be visible", true, isUserDefinedFilterCheckBoxVisibility);
				}

				filterBizo.SearchType = SearchType.Index;
				using (var control = new ZFilterStripBaseControlForTest(module.Grid, filterBizo, "This will be a normal layout", saveLayoutIsUserDefinedFilter: true))
				{
					bool? isUserDefinedFilterCheckBoxVisibility = null;

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
					{
						var saveLayoutForm = f as SaveLayoutForm;

						saveLayoutForm.Shown += (_, x_) =>
						{
							isUserDefinedFilterCheckBoxVisibility = saveLayoutForm.FindSingleOrDefault<ZCheckBox>("IsUserDefinedFilterCheckBox").Visible;
						};
					});

					control.SaveLayout_Exposed();

					AssertEquals("IsUserDefinedFilterCheckBox should be invisible", null, isUserDefinedFilterCheckBoxVisibility);
				}
			}
		}

		public void TestSaveLayout_DuplicateUniqueIndexKey()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Z0_Description");
				var filter = (ModuleTextFilter)strip.CurrentModuleFilter;
				filter.Property = "filter";
				module.Grid.Visible = false;

				using (var control = new ZFilterStripBaseControlForTest(module.Grid, filterBizo, "layout1", saveLayoutIsUserDefinedFilter: true))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
					{
						var saveLayoutForm = form as SaveLayoutForm;

						saveLayoutForm.FormClosed += (_, x_) =>
						{
							var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
							var layout = newFactory.New<StmModuleFilter>();
							layout.S9_ModuleID = DummyModuleIDs.Dummy.Name;
							layout.S9_FilterName = "layout1";
							layout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
							layout.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
							newFactory.Save();
						};
					});

					control.SaveLayout_Exposed();
					AssertEquals("A layout or scheme with this name 'layout1' already exists in database. Please use a different name.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestSaveLayout_ShouldSaveCurrentUsedColourLayout()
		{
			var dummy = new DummyBusinessObjectCollection(Factory);

			for (var i = 0; i < 10; ++i)
			{
				dummy.AddNew();
			}
			Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var filterStripControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
			{
				var strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Description");

				filterStripControl.RelatedGrid.SetDataBinding(dummy, "Collection");

				AssertNull(filterStripControl.RelatedGrid.GetLastUsedColourSchemeForCurrentUser);

				var userScheme = Factory.New<GridColourScheme>();
				userScheme.S9_FilterName = "user scheme";
				userScheme.S9_ModuleID = "Dummy_CS";
				userScheme.S9_IsPublished = false;
				userScheme.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				userScheme.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				var colorStrip = new GridColourStripBusinessObject(filterStripControl.FilterBusinessObject, userScheme, null);
				userScheme.ColourStrips.Add(colorStrip);
				Factory.Save();

				filterStripControl.RelatedGrid.GridColourSchemeManagerForTest.GridColourFactory.SetLastUsedSchemeForCurrentUser(filterStripControl.FilterBusinessObject, userScheme);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
				{
					var saveLayoutForm = f as SaveLayoutForm;

					saveLayoutForm.Shown += (_, x_) =>
					{
						var saveColourLayout = saveLayoutForm.FindSingleOrDefault<ZCheckBox>("SaveGridColoursCheckBox");
						saveColourLayout.Checked = true;
						var filterNameTextBox = saveLayoutForm.FindSingleOrDefault<ZTranslatableTextControl>("FilterNameTextBox");
						filterNameTextBox.Text = "TestLayout";
					};
				});

				filterStripControl.SaveLayout_Exposed();

				var currentUsed = filterStripControl.RelatedGrid.GetLastUsedColourSchemeForCurrentUser;

				AssertNotNull(currentUsed);
				AssertEquals(userScheme.PK, currentUsed.PK);
			}
		}

		public void TestSaveLayout_ConcurrencyExceptionChangingDeletedLayout()
		{
			const string layoutName = "This is a test layout";

			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var module2 = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (TestCaseWithFactory.GetFactoryIsolater(((IDummyFilterModule)module).Factory))
			using (TestCaseWithFactory.GetFactoryIsolater(((IDummyFilterModule)module2).Factory))
			using (TestCaseWithFactory.GetFactoryIsolater(((IModifyModuleAndGridLayout)module.FilterBusinessObject).Factory))
			using (TestCaseWithFactory.GetFactoryIsolater(((IModifyModuleAndGridLayout)module2.FilterBusinessObject).Factory))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Z0_Description");
				AssertEquals("Strip Name", "Z0_Description", strip.FilterDescription);
				var filter = (ModuleTextFilter)strip.CurrentModuleFilter;
				filter.Property = "filter";
				module.Grid.Visible = false;

				ZFilterStripBaseControlForTest filterControl2;

				using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, filterBizo, layoutName, saveLayoutIsUserDefinedFilter: true))
				using (var form = new Form())
				using (var form2 = new Form())
				{
					form.Controls.Add(filterControl);
					form.Show();

					filterControl.AddFilterStrip(strip);

					var filters = filterControl.Strips_Exposed;
					AssertEquals("First Filter", "Z0_Description", filters[1].FilterDescriptionDropEdit.CodeBox.Text);
					AssertEquals("First Filter Property", "filter", ((ModuleTextFilter)filters[1].CurrentDataItem.CurrentModuleFilter).Property);

					filterControl.SaveLayout_Exposed();

					var loader = new StmModuleFilter.Loader(((IDummyFilterModule)module2).Factory);
					var layout = loader.FindTop1ByIDAndName(DummyModuleIDs.Dummy.Name, layoutName);
					AssertEquals("Load Layout", layoutName, layout.S9_FilterName);
					module2.FilterBusinessObject.LoadLayout(layout);

					var strip2 = module2.FilterBusinessObject.FilterStrips[0];
					AssertEquals("Number of Filter Strips", 1, module2.FilterBusinessObject.FilterStrips.Count);
					AssertEquals("Strip Name", "Z0_Description", strip2.FilterDescription);
					var filter2 = (ModuleTextFilter)strip2.ModuleFilters["Z0_Description"];
					AssertEquals("Read saved filter", "filter", filter2.Property);

					filterControl2 = new ZFilterStripBaseControlForTest(module2.Grid, module2.FilterBusinessObject, layoutName, saveLayoutIsUserDefinedFilter: true);
					form2.Controls.Add(filterControl2);
					form2.Show();

					filterControl2.AddFilterStrip(strip2);

					AssertEquals("Read saved filter after second form showed", "filter", filter2.Property);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
					{
						var manageForm = (ManageFiltersFormTest.TestManageLayoutsForm)dialog;
						manageForm.Shown += (_, x_) =>
						{
							manageForm.DeleteSelectedLayoutExposed();
							manageForm.SaveButtonExposed.PerformClick();
						};
					});

					filterControl.ManageLayouts_Exposed(true);
					Application.DoEvents();

					var loaderTest = new StmModuleFilter.Loader(((IDummyFilterModule)module).Factory);
					var layoutTest = loaderTest.FindTop1ByIDAndName(DummyModuleIDs.Dummy.Name, layoutName);
					AssertNull("Layout should be deleted", layoutTest);

					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog => { });

					filters = filterControl2.Strips_Exposed;
					AssertEquals("First Filter", "Z0_Description", filters[1].FilterDescriptionDropEdit.CodeBox.Text);
					((ModuleTextFilter)filters[1].CurrentDataItem.CurrentModuleFilter).Property = "filter2";

					filterControl2.FilterBusinessObject.ModuleFilters["Z0_Description"].IsActive = true;

					AssertEquals("Second Filter Control Active Module Filters Count", 1, filterControl2.FilterBusinessObject.ActiveModuleFilters.Count);

					filterControl2.SaveLayout_Exposed();
				}

				AssertStartsWith("Concurrency Error Message", "While you were editing your data, another user ", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveLayout_ForUserDefinedFilter_ShouldSetFilterTypeToUSR()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Z0_Description");
				var filter = (ModuleTextFilter)strip.CurrentModuleFilter;
				filter.Property = "Shalala";
				module.Grid.Visible = false;

				using (var control = new ZFilterStripBaseControlForTest(module.Grid, filterBizo, "This will be a normal layout", saveLayoutIsUserDefinedFilter: false))
				{
					control.SaveLayout_Exposed();
				}

				var layout = Factory.Load<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_FilterName, "This will be a normal layout")).Single();
				AssertEquals("The filters were saved as a normal layout, so filter type should be empty, and yet...", string.Empty, layout.S9_FilterType);

				using (var control = new ZFilterStripBaseControlForTest(module.Grid, filterBizo, "This will be a User Defined Filter Strip", saveLayoutIsUserDefinedFilter: true))
				{
					control.SaveLayout_Exposed();
				}

				layout = Factory.Load<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_FilterName, "This will be a User Defined Filter Strip")).Single();
				AssertEquals("The filters were saved as a normal layout, so filter type should be empty, and yet...", StmModuleFilterTypes.Codes.UserDefined, layout.S9_FilterType);
			}
		}

		public void TestSaveUserDefinedFilter_ShouldUpdateFilterDropDownsInParentModuleWindow()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.Grid.Visible = false;
				var strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Description");
				var filter = (ModuleTextFilter)strip.CurrentModuleFilter;
				filter.Property = "I shall remain";

				using (var form = new Form())
				using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject, "A new filter", saveLayoutIsUserDefinedFilter: true))
				{
					form.Controls.Add(filterControl);
					form.Show();
					filterControl.AddFilterStrip(strip);
					filterControl.SaveLayout_Exposed();

					var filterStrip = filterControl.FindAll<ZFilterStrip>().Single(x => x.CurrentDataItem?.CurrentModuleFilter != null);
					var items = filterStrip.FilterDescriptionDropEdit.List.Cast<CodeDescriptionPair>();

					AssertEquals("The new user-defined filter should appear in the filter list immediately, and yet...", true, items.Any(x => x.Code == "A new filter [+]"));
					AssertEquals("Saving a user-defined filter should not remove filter strips from the module, and yet...", "I shall remain", ((ModuleTextFilter)filterStrip.CurrentDataItem.CurrentModuleFilter).Property);
				}
			}
		}

		public void TestRenameUserDefinedFilter_ShouldUpdateFilterDropDownsInParentModuleWindow()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.Grid.Visible = false;

				using (var form = new ZForm())
				using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject, "This shall be renamed", saveLayoutIsUserDefinedFilter: true) { NewLayoutNameForRenaming = "I've been renamed!" })
				{
					form.Controls.Add(filterControl);
					form.Show();

					var strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Description");
					((ModuleTextFilter)strip.CurrentModuleFilter).Property = "Shalala";
					filterControl.SaveLayout_Exposed(); // Saves the user-defined filter

					strip = module.FilterBusinessObject.FilterStrips.AddNew("[USR]This shall be renamed");
					filterControl.AddFilterStrip(strip);
					var filterStrip = filterControl.FindSingleOrDefault<ZFilterStrip>(x => x.CurrentDataItem != null && x.CurrentDataItem.CurrentModuleFilterDescription == "[USR]This shall be renamed");
					AssertNotNull(filterStrip);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
					{
						var manageForm = (ManageFiltersFormTest.TestManageLayoutsForm)dialog;
						manageForm.Shown += (_, x_) =>
						{
							manageForm.RenameSelectedLayoutExposed();
							manageForm.SaveButtonExposed.PerformClick();
						};
					});

					filterControl.ManageLayouts_Exposed(true);
					Application.DoEvents();

					AssertEquals("[USR]This shall be renamed", filterStrip.FilterDescriptionDropEdit.CodeBox.Text);
					AssertHasError(filterStrip.CurrentDataItem.FilterDescriptionInfo, "Enter a valid selection.");

					var items = filterStrip.FilterDescriptionDropEdit.List.Cast<CodeDescriptionPair>().ToArray();
					AssertEquals("The original name for the user-defined filter should no longer be in the list, and yet...", false, items.Any(x => x.Code == "This shall be renamed [+]"));
					AssertEquals("The new name for the user-defined filter should be in the list, and yet...", true, items.Any(x => x.Code == "I've been renamed! [+]"));
				}
			}
		}

		public void TestDeleteUserDefinedFilter_ShouldUpdateFilterDropDownsInParentModuleWindow()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.Grid.Visible = false;

				using (var form = new ZForm())
				using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject, "This shall be deleted", saveLayoutIsUserDefinedFilter: true))
				{
					form.Controls.Add(filterControl);
					form.Show();

					var strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Description");
					((ModuleTextFilter)strip.CurrentModuleFilter).Property = "Shalala";
					filterControl.SaveLayout_Exposed(); // Saves the user-defined filter

					strip = module.FilterBusinessObject.FilterStrips.AddNew("[USR]This shall be deleted");
					filterControl.AddFilterStrip(strip);
					var filterStrip = filterControl.FindSingleOrDefault<ZFilterStrip>(x => x.CurrentDataItem != null && x.CurrentDataItem.CurrentModuleFilterDescription == "[USR]This shall be deleted");
					AssertNotNull(filterStrip);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
					{
						var manageForm = (ManageFiltersFormTest.TestManageLayoutsForm)dialog;
						manageForm.Shown += (_, x_) =>
						{
							manageForm.DeleteSelectedLayoutExposed();
							manageForm.SaveButtonExposed.PerformClick();
						};
					});

					filterControl.ManageLayouts_Exposed(true);
					Application.DoEvents();

					AssertEquals("[USR]This shall be deleted", filterStrip.FilterDescriptionDropEdit.CodeBox.Text);
					AssertHasError(filterStrip.CurrentDataItem.FilterDescriptionInfo, "Enter a valid selection.");

					var items = filterStrip.FilterDescriptionDropEdit.List.Cast<CodeDescriptionPair>().ToArray();
					AssertEquals("The original name for the user-defined filter should no longer be in the list, and yet...", false, items.Any(x => x.Code == "This shall be deleted [+]"));
					AssertEquals("Deleting UserDefined FilterStrip while it's in current grid layout should remove the FilterCollectionFindBox from the ZFilterStrip", 0, filterStrip.Controls.OfType<ZFilterCollectionFindBox>().Count());
				}
			}
		}

		public void TestShowsMessageWhenNoFilterAdded_AndAfterLastItemRemoved()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, dummyFilterStripBizO, "This shall be deleted", saveLayoutIsUserDefinedFilter: true))
			using (var form = new Form())
			{
				form.Controls.Add(filterControl);
				form.Show();

				var filterLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;
				AssertEquals("The tool strip should have 1 layout", 1, filterLayouts.Count);
				AssertEquals("The first layout should be <no filters have been added>", "<no filters have been added>", filterLayouts[0].Text);

				var filterA = dummyFilterStripBizO.Layouts.AddNew();
				filterA.S9_FilterName = "A";
				filterA.S9_IsPublished = false;

				filterControl.AddItemToFindDropList(filterA);

				AssertEquals("The tool strip should have 2 layouts", 2, filterLayouts.Count);
				AssertEquals("The first layout should be 'My Filter Layouts'", "My Filter Layouts", filterLayouts[0].Text.Trim());
				AssertEquals("The second layout should be A", "A", filterLayouts[1].Text.Trim());

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var manageForm = (ManageFiltersFormTest.TestManageLayoutsForm)dialog;
					manageForm.Shown += (_, x_) =>
					{
						manageForm.DeleteSelectedLayoutExposed();
						manageForm.SaveButtonExposed.PerformClick();
					};
				});

				filterControl.ManageLayouts_Exposed(false);
				Application.DoEvents();

				AssertEquals("The tool strip should have 1 layout", 1, filterLayouts.Count);
				AssertEquals("The first layout should be <no filters have been added>", "<no filters have been added>", filterLayouts[0].Text);
			}
		}

		public void TestDeletingMenuItemThatHasChildrenShouldOnlyRemoveItsAction()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, dummyFilterStripBizO, "A/B/C", saveLayoutIsUserDefinedFilter: true))
			using (var form = new Form())
			{
				form.Controls.Add(filterControl);
				form.Show();

				var filterLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;
				AssertEquals("The tool strip should have 1 layout", 1, filterLayouts.Count);
				AssertEquals("The first layout should be <no filters have been added>", "<no filters have been added>", filterLayouts[0].Text);

				var filterA = dummyFilterStripBizO.Layouts.AddNew();
				filterA.S9_IsPublished = false;
				filterA.S9_FilterName = "A";
				var filterC = dummyFilterStripBizO.Layouts.AddNew();
				filterC.S9_IsPublished = false;
				filterC.S9_FilterName = "A/B/C";
				var filterD = dummyFilterStripBizO.Layouts.AddNew();
				filterD.S9_IsPublished = false;
				filterD.S9_FilterName = "D";

				filterControl.AddItemToFindDropList(filterA);
				filterControl.AddItemToFindDropList(filterC);
				filterControl.AddItemToFindDropList(filterD);

				AssertEquals("The tool strip should have 3 layouts: 'My Filter Layouts', 'A' and 'D'", 3, filterLayouts.Count);
				AssertNotNull("The first menu item should have a Tag", ((ZToolStripMenuItem)filterLayouts[1]).Tag);
				AssertEquals("The first layout should have a sub menu", 1, ((ZToolStripMenuItem)filterLayouts[1]).DropDownItems.Count);
				AssertEquals("The sub menu Text should be B", "B", ((ZToolStripMenuItem)filterLayouts[1]).DropDownItems[0].Text);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var manageForm = (ManageFiltersFormTest.TestManageLayoutsForm)dialog;

					manageForm.Shown += (_, x_) =>
					{
						manageForm.DeleteSelectedLayoutExposed();
						manageForm.SaveButtonExposed.PerformClick();
					};
				});

				filterControl.ManageLayouts_Exposed(false);
				Application.DoEvents();

				AssertEquals("The tool strip should still have 3 layouts", 3, filterLayouts.Count);
				AssertEquals("The first layout should still exist and have a sub menu", 1, ((ZToolStripMenuItem)filterLayouts[1]).DropDownItems.Count);
				AssertNull("The first menu item should no longer have a Tag", ((ZToolStripMenuItem)filterLayouts[1]).Tag);
			}
		}

		public void TestUpdateGroupFilterLayouts()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, dummyFilterStripBizO, "A/B/C", saveLayoutIsUserDefinedFilter: true))
			using (var form = new Form())
			{
				form.Controls.Add(filterControl);
				form.Show();

				var filterLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;
				AssertEquals("The tool strip should have 1 layout", 1, filterLayouts.Count);
				AssertEquals("The first layout should be <no filters have been added>", "<no filters have been added>", filterLayouts[0].Text);

				var filterA = dummyFilterStripBizO.Layouts.AddNew();
				filterA.S9_IsPublished = false;
				filterA.S9_FilterName = "A";
				var filterC = dummyFilterStripBizO.Layouts.AddNew();
				filterC.S9_IsPublished = false;
				filterC.S9_FilterName = "A/B/C";
				var filterD = dummyFilterStripBizO.Layouts.AddNew();
				filterD.S9_IsPublished = false;
				filterD.S9_FilterName = "D";

				filterControl.AddItemToFindDropList(filterA);
				filterControl.AddItemToFindDropList(filterC);
				filterControl.AddItemToFindDropList(filterD);
				AssertLayout();

				filterControl.AddOrUpdateExistingFindDropListItemExposed(filterC);
				AssertLayout();

				void AssertLayout()
				{
					AssertEquals("The tool strip should have 3 layouts: 'My Filter Layouts', 'A' and 'D'", 3, filterLayouts.Count);
					AssertNotNull("The first menu item should have a Tag", ((ZToolStripMenuItem)filterLayouts[1]).Tag);
					AssertEquals("The first layout should have a sub menu", 1, ((ZToolStripMenuItem)filterLayouts[1]).DropDownItems.Count);
					AssertEquals("The sub menu Text should be B", "B", ((ZToolStripMenuItem)filterLayouts[1]).DropDownItems[0].Text);
					AssertEquals("The last sub menu Text should be C", "C", ((ZToolStripMenuItem)((ZToolStripMenuItem)filterLayouts[1]).DropDownItems[0]).DropDownItems[0].Text);
				}
			}
		}

		#endregion

		#region TestFirePerformSearch

		public void TestValidationFailurePreventsPerfomingSearch()
		{
			var searched = false;

			using (var form = GetNewFilterForm())
			{
				form.FilterControl.FilterBusinessObject.AddRowError("Bad filter");
				form.Show();
				form.FilterControl.PerformSearch += delegate
				{ searched = true; };

				form.FilterControl.FirePerformSearch();
				Assert("Should not have searched", !searched);
			}
		}

		public void TestSqlExceptionWithoutCustomSqlOrUserDefinedFiltersIsNotCaught()
		{
			using (var form = GetNewFilterForm(new DummyFilterBusinessObject(withCustomSqlFilter: false)))
			{
				form.Show();
				var filterControl = form.FilterControl;
				filterControl.PerformSearch += delegate
				{ ThrowSqlException(); };

				Assert("The form under test should have no custom SQL filters", !HasSqlFilter(filterControl.FilterBusinessObject));
				AssertExceptionThrown<SqlException>(
					"A SqlException should be propagated if there are no Custom SQL Filters",
					() => filterControl.FirePerformSearch());
			}
		}

		public void TestSqlExceptionWithInactiveCustomSqlFilterIsNotCaught()
		{
			using (var form = GetNewFilterForm(new DummyFilterBusinessObject(withCustomSqlFilter: false)))
			{
				form.Show();
				var filterControl = form.FilterControl;
				var filterBizO = filterControl.FilterBusinessObject;
				AddSqlFilter(filterBizO, "Foo1 = 'Bar1'", isActive: false);
				form.FilterControl.PerformSearch += delegate
				{ ThrowSqlException(); };

				//Preconditions
				Assert("The form under test should have at least one Custom SQL filter", HasSqlFilter(filterBizO));
				Assert("The form under test should have no active Custom SQL filter", !HasActiveSqlFilter(filterBizO));

				AssertExceptionThrown<SqlException>(
					"A SqlException should be propagated if there are no active Custom SQL Filters",
					() => filterControl.FirePerformSearch());
			}
		}
		public void TestSqlExceptionWithInactiveUserDefinedFilterIsNotCaught()
		{
			using (var form = GetNewFilterForm(new DummyFilterBusinessObject(withCustomSqlFilter: false)))
			{
				form.Show();
				var filterControl = form.FilterControl;
				var filterBizO = filterControl.FilterBusinessObject;
				AddUserDefinedFilter(filterBizO, Factory, "test filter", isActive: false);
				form.FilterControl.PerformSearch += delegate
				{ ThrowSqlException(); };

				//Preconditions
				Assert("The form under test should have at least one user-defined filter", HasUserDefinedFilter(filterBizO));
				Assert("The form under test should have no active user-defined filters", !HasActiveUserDefinedFilter(filterBizO));

				AssertExceptionThrown<SqlException>(
					"A SqlException should be propagated if there are no active user-defined Filters",
					() => filterControl.FirePerformSearch());
			}
		}

		public void TestSqlExceptionWithActiveCustomSqlFilterIsCaughtAndExpectedErrorDisplayed()
		{
			var filter1 = "Foo1 = 'Bar1'";
			var filter2 = "Foo2 = 'Bar2'";
			var expectedMessage = $@"The search had an error that may be caused by a Custom SQL Filter. Custom SQL Filters may also be part of a User-Defined Filter.
Please remove Custom SQL Filters from the search or from any User-Defined Filters and try again before reporting an incident.

Error message: Could not find stored procedure 'Invalid'.

Custom SQL Filters should be of the form [Column 1] = 'Value 1' AND [Column 2] = 'Value 2'.

Your Custom SQL Filter(s):
   {filter1}
   {filter2}
Your User-Defined Filter(s):";

			using (var form = GetNewFilterForm(new DummyFilterBusinessObject(withCustomSqlFilter: false)))
			{
				form.Show();
				var filterControl = form.FilterControl;
				var filterBizO = filterControl.FilterBusinessObject;
				AddSqlFilter(filterBizO, filter1, isActive: true);
				AddSqlFilter(filterBizO, filter2, isActive: true);
				form.FilterControl.PerformSearch += delegate
				{ ThrowSqlException(); };

				//Precondition
				Assert("The form under test should have at least one active Custom SQL filter", HasActiveSqlFilter(filterBizO));

				AssertNoExceptionThrown(
					"A SqlException should be handled if there are Custom SQL Filters",
					() => filterControl.FirePerformSearch());
				var notifications = Globals.Message as UnitTestUserNotification;
				AssertEquals("An appropriate message should be displayed", expectedMessage, notifications?.LastMessage.Text);
			}
		}

		public void TestSqlExceptionWithActiveUserDefinedFilterIsCaughtAndExpectedErrorDisplayed()
		{
			var filter1 = "test filter 1";
			var filter2 = "test filter 2";
			var expectedMessage = $@"The search had an error that may be caused by a Custom SQL Filter. Custom SQL Filters may also be part of a User-Defined Filter.
Please remove Custom SQL Filters from the search or from any User-Defined Filters and try again before reporting an incident.

Error message: Could not find stored procedure 'Invalid'.

Custom SQL Filters should be of the form [Column 1] = 'Value 1' AND [Column 2] = 'Value 2'.

Your Custom SQL Filter(s):
Your User-Defined Filter(s):
   {filter1}
   {filter2}";

			using (var form = GetNewFilterForm(new DummyFilterBusinessObject(withCustomSqlFilter: false)))
			{
				form.Show();
				var filterControl = form.FilterControl;
				var filterBizO = filterControl.FilterBusinessObject;
				AddUserDefinedFilter(filterBizO, Factory, filter1, isActive: true);
				AddUserDefinedFilter(filterBizO, Factory, filter2, isActive: true);
				form.FilterControl.PerformSearch += delegate
				{ ThrowSqlException(); };

				//Precondition
				Assert("The form under test should have at least one active user-defined filter", HasActiveUserDefinedFilter(filterBizO));

				AssertNoExceptionThrown(
					"A SqlException should be handled if there are user-defined Filters",
					() => filterControl.FirePerformSearch());
				var notifications = Globals.Message as UnitTestUserNotification;
				AssertEquals("An appropriate message should be displayed", expectedMessage, notifications?.LastMessage.Text);
			}
		}

		public void TestSqlExceptionWithActiveUserDefinedFilterAndActiveSQLFilterIsCaughtAndExpectedErrorDisplayed()
		{
			var userFilter1 = "test filter 1";
			var userFilter2 = "test filter 2";
			var sqlFilter1 = "Foo1 = 'Bar1'";
			var sqlFilter2 = "Foo2 = 'Bar2'";
			var expectedMessage = $@"The search had an error that may be caused by a Custom SQL Filter. Custom SQL Filters may also be part of a User-Defined Filter.
Please remove Custom SQL Filters from the search or from any User-Defined Filters and try again before reporting an incident.

Error message: Could not find stored procedure 'Invalid'.

Custom SQL Filters should be of the form [Column 1] = 'Value 1' AND [Column 2] = 'Value 2'.

Your Custom SQL Filter(s):
   {sqlFilter1}
   {sqlFilter2}
Your User-Defined Filter(s):
   {userFilter1}
   {userFilter2}";

			using (var form = GetNewFilterForm(new DummyFilterBusinessObject(withCustomSqlFilter: false)))
			{
				form.Show();
				var filterControl = form.FilterControl;
				var filterBizO = filterControl.FilterBusinessObject;
				AddUserDefinedFilter(filterBizO, Factory, userFilter1, isActive: true);
				AddUserDefinedFilter(filterBizO, Factory, userFilter2, isActive: true);
				AddSqlFilter(filterBizO, sqlFilter1, isActive: true);
				AddSqlFilter(filterBizO, sqlFilter2, isActive: true);
				form.FilterControl.PerformSearch += delegate
				{ ThrowSqlException(); };

				//Precondition
				Assert("The form under test should have at least one active user-defined filter", HasActiveUserDefinedFilter(filterBizO));

				AssertNoExceptionThrown(
					"A SqlException should be handled if there are user-defined Filters",
					() => filterControl.FirePerformSearch());
				var notifications = Globals.Message as UnitTestUserNotification;
				AssertEquals("An appropriate message should be displayed", expectedMessage, notifications?.LastMessage.Text);
			}
		}

		public void TestSqlExceptionAboutConversionWithCustomFilterIsRedacted()
		{
			var userFilter1 = "test filter 1";
			var userFilter2 = "test filter 2";
			var sqlFilter1 = "Foo1 = 'Bar1'";
			var sqlFilter2 = "Foo2 = 'Bar2'";
			var expectedMessage = $@"The search had an error that may be caused by a Custom SQL Filter. Custom SQL Filters may also be part of a User-Defined Filter.
Please remove Custom SQL Filters from the search or from any User-Defined Filters and try again before reporting an incident.

Error message: Conversion failed.

Custom SQL Filters should be of the form [Column 1] = 'Value 1' AND [Column 2] = 'Value 2'.

Your Custom SQL Filter(s):
   {sqlFilter1}
   {sqlFilter2}
Your User-Defined Filter(s):
   {userFilter1}
   {userFilter2}";

			using (var form = GetNewFilterForm(new DummyFilterBusinessObject(withCustomSqlFilter: false)))
			{
				form.Show();
				var filterControl = form.FilterControl;
				var filterBizO = filterControl.FilterBusinessObject;
				AddUserDefinedFilter(filterBizO, Factory, userFilter1, isActive: true);
				AddUserDefinedFilter(filterBizO, Factory, userFilter2, isActive: true);
				AddSqlFilter(filterBizO, sqlFilter1, isActive: true);
				AddSqlFilter(filterBizO, sqlFilter2, isActive: true);
				form.FilterControl.PerformSearch += delegate
				{ ThrowSqlException_ConversionFailed(); };

				//Precondition
				Assert("The form under test should have at least one active user-defined filter", HasActiveUserDefinedFilter(filterBizO));

				AssertNoExceptionThrown(
					"A SqlException should be handled if there are user-defined Filters",
					() => filterControl.FirePerformSearch());
				var notifications = Globals.Message as UnitTestUserNotification;
				AssertEquals("An appropriate message should be displayed", expectedMessage, notifications?.LastMessage.Text);
			}
		}

		static void AddSqlFilter(FilterStripBusinessObject filterBizO, string query, bool isActive)
		{
			var filter = new ModuleSQLFilter($"Custom SQL ({query})", filterBizO.QueryObjectType)
			{
				Property1 = query,
				Category = FilterCategories.Other,
				IsActive = isActive
			};
			filterBizO.ModuleFilters.AddFilter(filter);
		}

		static void AddUserDefinedFilter(FilterStripBusinessObject filterBizO, BusinessObjectFactory factory, string filterName, bool isActive)
		{
			var layout = factory.NewWithValidTestData<StmModuleFilter>();
			layout.S9_FilterName = filterName;
			var filter = new ModuleUserDefinedFilter(layout, DummyModuleIDs.Dummy, DummyBizoSchema.PK)
			{
				IsActive = isActive
			};
			filter.SetFilterBusinessObject(filter, filterBizO);
			filterBizO.ModuleFilters.AddFilter(filter);
		}

		static bool HasSqlFilter(FilterStripBusinessObject filterBizO)
		{
			return filterBizO.ModuleFilters.OfType<ModuleSQLFilter>().Any();
		}

		static bool HasUserDefinedFilter(FilterStripBusinessObject filterBizO)
		{
			return filterBizO.ModuleFilters.OfType<ModuleUserDefinedFilter>().Any();
		}

		static bool HasActiveSqlFilter(FilterStripBusinessObject filterBizO)
		{
			return filterBizO.ActiveModuleFilters.OfType<ModuleSQLFilter>().Any();
		}

		static bool HasActiveUserDefinedFilter(FilterStripBusinessObject filterBizO)
		{
			return filterBizO.ActiveModuleFilters.OfType<ModuleUserDefinedFilter>().Any();
		}

		static void ThrowSqlException()
		{
			// We can't just create a SqlException because it has no public constructors
			Db.Connection.ExecuteNonQuery("Invalid SQL");
		}

		static void ThrowSqlException_ConversionFailed()
		{
			Db.Connection.ExecuteNonQuery("select * from dbo.glbstaff where GS_UserAddress1='xxx' and 1=(select top 5 concat_ws(0x3a,GS_LoginName,convert(varchar(max), GS_PasswordHash, 2))a from dbo.GlbStaff for xml auto)");
		}

		#endregion

		#region IsFilterReadonly

		public void TestIsFilterReadOnly_WhenTrue_ShouldDisableGroupStripControlButtons()
		{
			using (var module = (ZFilterModule)ZModule.GetZModule(ModuleIDs.ProcessTasks))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Description");
				strip.GroupName = "First Group";
				strip = filterBizo.FilterStrips.AddNew("Description");
				strip.GroupName = "First Group";
				strip = filterBizo.FilterStrips.AddNew("Description");
				strip.GroupName = "Second Group";
				strip = filterBizo.FilterStrips.AddNew("Description");
				strip.GroupName = "Second Group";

				using (var wrapperControl = new FilterRuleFilterStripControlTest.WrapperControl_ForTest(filterBizo) { Dock = DockStyle.Fill, IsPreviewAllowed = true })
				using (var form = new KForm() { Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
				{
					form.Controls.Add(wrapperControl);
					form.Show();
					Application.DoEvents();

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
					{
						var popup = (EmbeddedModulePopup)dialog;

						popup.Shown += (_, x_) =>
						{
							try
							{
								var buttons = popup.FilterControlPanel_ForTest.FindAll<ZButton>();

								CombineAssertions(() =>
								{
									foreach (var button in buttons)
									{
										AssertEquals(string.Format(CultureInfo.InvariantCulture, "Button [{0}] should be read only, and yet...", button.Name), false, button.Enabled);
									}
								});
							}
							finally
							{
								popup.Close();
							}
						};
					});

					wrapperControl.StripControl.SetIsPreviewAllowed(ModuleIDs.ProcessTasks.Name);
					var previewButton = wrapperControl.StripControl.ToolStripPreviewDropButton_Exposed;
					previewButton.PerformClick();
				}
			}
		}

		#endregion

		#region TestAddingAndDeletingFilterStrips

		public void TestAddAlwaysVisibleFilterStrips()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter1 = new ModuleTextFilter("Some Filter", DummyBizoSchema.Z0_Description);
			var filter2 = new ModuleTextFilter("Some Other Filter", DummyBizoSchema.Z0_Description);

			filterStripBizO.AddModuleFilterForTest(filter1);
			filterStripBizO.AddModuleFilterForTest(filter2);

			var defaults = new FilterBusinessObjectDefaults();
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property", new ZString("Value1")));
			defaults.Add(new FilterBusinessObjectDefault("Some Other Filter", "Property", new ZString("Value2")));
			filterStripBizO.SetExternalDefaults(defaults);

			using (var form = GetNewFilterForm(filterStripBizO))
			using (var control = new ZFilterStripBaseControlForTest(filterStripBizO))
			{
				form.Show();

				AssertEquals("There should be three filter strips added", 3, control.FilterBusinessObject.FilterStrips.Count);
				AssertEquals("First Filter name", "Some Filter", control.FilterBusinessObject.FilterStrips[0].FilterDescriptionLocalized);
				AssertEquals("Second Filter name", "Some Other Filter", control.FilterBusinessObject.FilterStrips[1].FilterDescriptionLocalized);
				AssertNull("Blank Filter shouldn't have associated CurrentModuleFilter", control.FilterBusinessObject.FilterStrips[2].CurrentModuleFilter);
			}
		}

		public void TestAddAlvaysVisibleFilterStrips_WithDuplicateDefaults()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter1 = new ModuleTextFilter("Some Filter", DummyBizoSchema.Z0_Description);

			filterStripBizO.AddModuleFilterForTest(filter1);

			var defaults = new FilterBusinessObjectDefaults();
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property", new ZString("Value1"), FilterOrCategory.Blue, 1));
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property", new ZString("Value2"), FilterOrCategory.Blue, 2));
			filterStripBizO.SetExternalDefaults(defaults);

			using (var form = GetNewFilterForm(filterStripBizO))
			using (var control = new ZFilterStripBaseControlForTest(filterStripBizO))
			{
				form.Show();

				AssertEquals("There should be three filter strips added", 3, control.FilterBusinessObject.FilterStrips.Count);

				AssertEquals("First Filter name", "Some Filter", control.FilterBusinessObject.FilterStrips[0].FilterDescriptionLocalized);
				AssertEquals("First Filter color", FilterOrCategory.Blue, control.FilterBusinessObject.FilterStrips[0].OrCategory);

				AssertEquals("Second Filter name", "Some Filter", control.FilterBusinessObject.FilterStrips[1].FilterDescriptionLocalized);
				AssertEquals("First Filter color", FilterOrCategory.Blue, control.FilterBusinessObject.FilterStrips[1].OrCategory);

				AssertNull("Blank Filter shouldn't have associated CurrentModuleFilter", control.FilterBusinessObject.FilterStrips[2].CurrentModuleFilter);
			}
		}

		#endregion

		#region ToggleIndexSearchButton

		public void TestSetGlowFilters()
		{
			using (new GlowIndexQueryEngineMock(GetGlowIndexQueryEngineMock))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
			{
				AssertEquals(SearchType.Index, filterControl.FilterBusinessObject.SearchType);
				AssertEquals(true, filterControl.FilterBusinessObject.HasIndexSearchFields);
				AssertEquals(3, filterControl.FilterBusinessObject.IndexSearchFields.Value.Length);

				AssertEquals(true, filterControl.FilterBusinessObject.All(f => f is IIndexSearchModuleFilter));
				AssertNotNull(filterControl.FilterBusinessObject["CODE"]);
				AssertNotNull(filterControl.FilterBusinessObject["NAME"]);
			}
		}

		public void TestSetGlowFiltersInChildFiltering()
		{
			using (var mocker = new GlowIndexQueryEngineMock(GetGlowIndexQueryEngineMock))
			using (var form = new ZForm())
			using (var findbox = new TestPopupFindBox())
			{
				form.Controls.Add(findbox);
				findbox.ParentModuleID = DummyModuleIDs.Dummy;

				using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
				{
					module.FilterBusinessObject.ParentType = typeof(DummyBizo);
					module.FilterBusinessObject.SearchType = SearchType.Index;

					using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
					{
						findbox.SetFilterModuleForTesting(module);
						findbox.SelectFromPopupForm(false);

						AssertEquals(SearchType.Index, filterControl.FilterBusinessObject.SearchType);
						AssertEquals(true, filterControl.FilterBusinessObject.HasIndexSearchFields);
						AssertEquals(true, filterControl.FilterBusinessObject.All(f => f is IIndexSearchModuleFilter));

						var searchFields = filterControl.FilterBusinessObject.IndexSearchFields.Value;
						AssertEquals(3, searchFields.Length);
						AssertEquals("CODE", searchFields[0].FieldName);
						AssertEquals("NAME", searchFields[1].FieldName);
						AssertEquals("IGlbStaff", searchFields[2].FieldName);
					}
				}
			}
		}

		SearchFieldCollection GetTestSearchFields(string entityType)
		{
			var field1 = SearchField.Create("CODE", "Code");
			var field2 = SearchField.Create("NAME", "Full Name");
			var field3 = SearchField.Create(entityType, entityType);
			var ret = new SearchFieldCollection(null, new SearchField[] { field1, field2, field3 });
			return ret;
		}

		void GetGlowIndexQueryEngineMock(Mock<IGlowIndexQueryEngine> mock)
		{
			_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns((string entityType) => GetTestSearchFields(entityType));
			_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject", "IGlbStaff" });
		}

		class TestPopupFindBox : ZPopupFindBox
		{
			public void SetFilterModuleForTesting(ZFilterModule module)
			{
				filterModuleForTesting = module;
				ModuleID = module.ID;
			}
			ZFilterModule filterModuleForTesting;

			protected internal override ZFilterModule NewModuleFromModuleID()
			{
				var result = filterModuleForTesting ?? base.NewModuleFromModuleID();
				return result;
			}
		}

		#endregion

		public void TestGridColourSchemeManagerDoesntCrash()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new ZForm())
			using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertNoExceptionThrown(() => { filterControl.ManageLayouts_Exposed(false); });
			}
		}

		public void TestAutoRefreshSlowQueryWarningMessage()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			using (var control = new ZFilterStripBaseControlForTest(filterStripBizO))
			{
				AssertEquals(control.AutoRefreshSlowQueryWarningMessage, @"Auto-Refresh has been temporarily disabled because the search was too slow on 2 successive retrieves.
Try optimizing the filter, reducing the record set and removing any 'contains' filters or any of the more complex filters.");
			}
		}

		#region Implementation

		ZDummyFilterStripBaseControlForm GetNewFilterForm()
		{
			return new ZDummyFilterStripBaseControlForm(Factory.New<DummyBusinessObject>());
		}

		ZDummyFilterStripBaseControlForm GetNewFilterForm(FilterStripBusinessObject filterBizo)
		{
			return new ZDummyFilterStripBaseControlForm(Factory.New<DummyBusinessObject>(), filterBizo);
		}

		#endregion
	}

	public class ZFilterStripBaseControlSharedTest : FilterStripControlSharedTest
	{
		protected override IZDummyFilterStripForm GetNewFilterForm()
		{
			return new ZDummyFilterStripBaseControlForm(Factory.New<DummyBusinessObject>());
		}

		protected override IZDummyFilterStripForm GetNewFilterForm(FilterStripBusinessObject filterBizo)
		{
			return new ZDummyFilterStripBaseControlForm(Factory.New<DummyBusinessObject>(), filterBizo);
		}

		public override void AssertOnS9ColumnLayoutData(StmModuleFilter layout)
		{
			AssertNotEquals("layout.S9_ColumnLayoutData should not be cleared out", ZBlob.Empty, layout.S9_ColumnLayoutData);
		}
	}
}
