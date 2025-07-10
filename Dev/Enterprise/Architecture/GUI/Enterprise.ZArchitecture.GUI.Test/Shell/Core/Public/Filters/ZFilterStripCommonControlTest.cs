using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.GUI.TileBar;
using CargoWise.Main.Navigation;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ZArchitecture.Modules.ZFilterModule;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	#region ZDummyFilterForm

	public interface IZDummyFilterStripForm : IDisposable
	{
		FilterStripBusinessObject GetFilterBizO();
		ZFilterStripBaseControl GetFilterControl();
		IFilterStripBaseControlForTest GetFilterControlExposed();
		ZTextBox GetAnotherTextBox();
		ZGrid GetGrid();
	}

	public class ZDummyFilterStripCommonForm : ZForm, IZDummyFilterStripForm
	{
		protected ZDummyFilterStripCommonForm() : this(new DummyFilterStripBusinessObject()) { }

		protected ZDummyFilterStripCommonForm(FilterStripBusinessObject filterBizO)
		{
			SetupFilter(filterBizO);
		}

		public ZDummyFilterStripCommonForm(DummyBusinessObject dummy)
			: this(dummy, new DummyFilterStripBusinessObject())
		{ }

		public ZDummyFilterStripCommonForm(DummyBusinessObject dummy, FilterStripBusinessObject filterBizO)
			: base(dummy)
		{
			SetupFilter(filterBizO);
		}

		void SetupFilter(FilterStripBusinessObject filterBizO)
		{
			FilterBizO = filterBizO;
			FilterControl = GetNewDummyZFilterStripControl(FilterBizO);
			Controls.Add(FilterControl);
			AnotherTextBox = new ZTextBox();
			Controls.Add(AnotherTextBox);
		}

		public FilterStripBusinessObject FilterBizO { get; private set; }
		public ZFilterStripCommonControl FilterControl { get; private set; }
		public IFilterStripControlForTest FilterControlExposed { get { return (IFilterStripControlForTest)FilterControl; } }
		public ZTextBox AnotherTextBox { get; private set; }

		protected virtual ZFilterStripCommonControl GetNewDummyZFilterStripControl(FilterStripBusinessObject filterBizO)
		{
			return new DummyZFilterStripControlCommon(filterBizO);
		}

		public FilterStripBusinessObject GetFilterBizO()
		{
			return FilterBizO;
		}

		public ZFilterStripBaseControl GetFilterControl()
		{
			return FilterControl;
		}

		IFilterStripBaseControlForTest IZDummyFilterStripForm.GetFilterControlExposed()
		{
			return (IFilterStripControlForTest)FilterControl;
		}

		public ZTextBox GetAnotherTextBox()
		{
			return AnotherTextBox;
		}

		public ZGrid GetGrid()
		{
			return null;
		}
	}

	#endregion

	#region DummyZFilterStripControlCommon

	public interface IFilterStripControlForTest : IFilterStripBaseControlForTest
	{
		void SetMaximumAllowableQueriesPerSqlStatement(int maxRecords);
		int MaximumAllowableQueriesPerSqlStatementExposed { get; }
	}

	public class DummyZFilterStripControlCommon : ZFilterStripCommonControl, IFilterStripControlForTest
	{
		public DummyZFilterStripControlCommon(FilterStripBusinessObject filterBusinessObject)
			: base(filterBusinessObject)
		{
			Grid.Columns.AddTextColumn("Z0_Code", 80, true, false);
			Grid.Columns.AddTextColumn("Z0_Description", 80, true, false);
			Grid.Columns.AddCalcEditColumn("Z0_Number", 80, true, false, 0);

			Grid.SetModuleId(DummyModuleIDs.Dummy);

			BindingSource.DataSourceType = typeof(DummyBusinessObject);
			BindingSource.SetBindingMember(Grid, "FilteredCollection");
		}

		#region Expose

		public RecentItemsControl RecentItemsControlExposed
		{
			get { return RecentItemsControl; }
		}

		public ZLabel AutoRefreshWarningLabelExposed
		{
			get { return AutoRefreshWarningLabel; }
		}

		public void SetShouldPerformSearch(bool shouldPerformSearch)
		{
			this.shouldPerformSearch = shouldPerformSearch;
		}
		bool shouldPerformSearch = true;

		protected override ZBool ShouldPerformSearch()
		{
			return shouldPerformSearch;
		}

		public void SetMaximumAllowableQueriesPerSqlStatement(int maxRecords)
		{
			this.maxRecords = maxRecords;
		}
		int maxRecords;

		public int MaximumAllowableQueriesPerSqlStatementExposed
		{
			get { return MaximumAllowableQueriesPerSqlStatement; }
		}

		protected override int MaximumAllowableQueriesPerSqlStatement
		{
			get { return maxRecords == 0 ? base.MaximumAllowableQueriesPerSqlStatement : maxRecords; }
		}

		public void AddOrUpdateExistingFindDropListItemExposed(StmModuleFilter filter)
		{
			this.AddOrUpdateExistingFindDropListItem(filter);
		}

		public void HandleFindButtonDropDownItemClickExposed(ToolStripItem item)
		{
			HandleFindButtonDropDownItemClick(item);
		}

		public ToolStripSplitButton ToolStripFindDropButtonExposed
		{
			get { return ToolStripFindDropButton; }
		}

		public ToolStripSplitButton FindButtonExposed
		{
			get { return base.ToolStripFindDropButton; }
		}

		public ToolStripButton ToolStripSaveLayoutButtonExposed
		{
			get { return base.ToolStripSaveLayoutButton; }
		}

		public ToolStripMenuItem ToolStripManageLayoutsButtonExposed
		{
			get { return base.ToolStripManageLayoutsMenuItem; }
		}

		public bool CanSaveColumnLayoutsExposed
		{
			get { return CanSaveColumnLayouts; }
		}

		public bool CanSaveGridColoursExposed
		{
			get { return CanSaveGridColours; }
		}

		public KPanel FilterStripsPanelExposed
		{
			get { return FilterStripsPanel; }
		}

		public ZToolStrip ToolStripHelpExposed
		{
			get { return ToolStripHelp; }
		}

		public ToolStripButton ToolStripClearButtonExposed
		{
			get { return ToolStripClearButton; }
		}

		public bool ProcessDialogKeyExposed(Keys keyData)
		{
			return ProcessDialogKey(keyData);
		}

		protected override bool CanSaveColumnLayouts
		{
			get { return canSaveColumnLayouts; }
		}

		bool canSaveColumnLayouts = true;

		public void SetCanSaveColumnLayouts(bool canSaveColumnLayouts)
		{
			this.canSaveColumnLayouts = canSaveColumnLayouts;
		}

		protected override bool CanSaveGridColours
		{
			get { return canSaveGridColours; }
		}

		bool canSaveGridColours = true;

		public void SetCanSaveGridColours(bool canSaveGridColours)
		{
			this.canSaveGridColours = canSaveGridColours;
		}

		#endregion
	}

	#endregion

	class ZFilterStripCommonControlForTest : ZFilterStripCommonControl
	{
		public ZFilterStripCommonControlForTest(FilterStripBusinessObject filterBusinessObject)
			: base(filterBusinessObject)
		{
			finishedConstruction = true;
		}

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

		public override ZFilterGrid Grid
		{
			get
			{
				if (!finishedConstruction && AccessingFilterBusinessObjectDuringConstruction)
				{
					TryToSetQueryObjectTypeDuringConstruction = true;
				}

				return base.Grid;
			}
		}

		bool finishedConstruction { get; set; }
		bool AccessingFilterBusinessObjectDuringConstruction { get; set; }

		public bool TryToSetQueryObjectTypeDuringConstruction { get; set; }
	}

	sealed class ZFilterStripControlCommonTest : TestCaseWithFactory
	{
		#region TestDoNotTryToSetQueryObjectTypeDuringConstruction

		public void TestDoNotTryToSetQueryObjectTypeDuringConstruction()
		{
			using (var control = new ZFilterStripCommonControlForTest(new DummyFilterStripBusinessObject()))
			{
				Assert(!control.TryToSetQueryObjectTypeDuringConstruction);
			}
		}

		#endregion

		#region TestRespectsRunSearchOnEnteringAModuleRegistryItem

		public void TestRespectsRunSearchOnEnteringAModuleRegistryItem()
		{
			var searchRun = false;

			EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = false;
			using (var form = GetNewFilterForm())
			{
				form.FilterControl.PerformSearch += delegate
				{ searchRun = true; };
				form.Show();

				AssertEquals(false, searchRun);
			}

			EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
			using (var form = GetNewFilterForm())
			{
				form.FilterControl.PerformSearch += delegate
				{ searchRun = true; };
				form.Show();

				AssertEquals(true, searchRun);
			}
		}

		public void TestFirePerformSearchWithSettingToShowTheMessage()
		{
			var searchRun = false;

			using (var form = GetNewFilterForm())
			{
				var noRecMsg = "There are no records that match your search.";
				form.FilterControl.PerformSearch += (obj, args) =>
				{
					searchRun = true;
				};
				var filterControl = form.FilterControl;
				((IFilterControl)filterControl).UpdateNumberLoadedMessage(noRecMsg, 0, true);
				filterControl.FirePerformSearch(showError: false);

				AssertEquals(true, searchRun);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				searchRun = false;
				filterControl.FirePerformSearch(showError: true);

				AssertEquals(true, searchRun);
				AssertEquals(noRecMsg, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRunOnEnteringModule_Validation()
		{
			var filterBizO = new DummyFilterStripBusinessObjectWithErrors();
			var searchRun = false;

			EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
			using (var form = GetNewFilterForm(filterBizO))
			{
				form.FilterControl.PerformSearch += delegate
				{ searchRun = true; };
				form.Show();

				AssertEquals(false, searchRun);
				AssertEquals("There are errors. Please correct these before searching.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class DummyFilterStripBusinessObjectWithErrors : DummyFilterStripBusinessObject
		{
			protected override void RunPreSaveValidationCore()
			{
				base.RunPreSaveValidationCore();
				AddRowError("Some Error");
			}
		}

		#endregion

		#region TestMaximumAllowableQueriesPerSqlStatement

		public void TestMaximumAllowableQueriesPerSqlStatement()
		{
			using (var form = GetNewFilterForm())
			{
				form.Show();

				AssertEquals("Default max records is 1000", 1000, form.FilterControlExposed.MaximumAllowableQueriesPerSqlStatementExposed);

				SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)((IntRegistryDataType)SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.DataType).LowerBound);
				AssertEquals("Max records is 1", 1, form.FilterControlExposed.MaximumAllowableQueriesPerSqlStatementExposed);

				SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)((IntRegistryDataType)SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.DataType).UpperBound);
				AssertEquals("Max records is 15000", 15000, form.FilterControlExposed.MaximumAllowableQueriesPerSqlStatementExposed);

				form.FilterControlExposed.SetMaximumAllowableQueriesPerSqlStatement(200);
				AssertEquals("Max records should now be 200", 200, form.FilterControlExposed.MaximumAllowableQueriesPerSqlStatementExposed);
			}
		}

		#endregion

		#region TestRecentItems

		public void TestRecentItems()
		{
			using (var form = GetNewFilterForm())
			using (var module = new DummyFilterGridModule())
			{
				form.Show();
				form.FilterControl.Grid.SetParentFilterGridModule(module);
				form.FilterControl.LoadRecentItems();

				var viewModel = form.FilterControlExposed.RecentItemsControlExposed.DataContext as MenuSection;
				AssertNotNull(viewModel);

				var recentItem = new LinkWrapper(module.ID.Name, Guid.NewGuid(), "http://", "description");
				RecentItemManager.Reset(); //should no longer cause problems
				RecentItemManager.Instance.AddOrUpdateRecentItems(module.ID.Name, recentItem);
				viewModel = form.FilterControlExposed.RecentItemsControlExposed.DataContext as MenuSection;
				Assert("Recent item added to list", viewModel.Items[0].Key == recentItem.UniqueKey);
			}
		}

		public void TestRecentItems_CollectionWithBusinessObjectLoader()
		{
			var pk = Guid.NewGuid();
			using (var form = GetNewFilterForm())
			using (var module = new DummyFilterGridModuleWithIBusinessObjectLoaderCollection(ModuleIDs.ExchangeRate))
			{
				form.Show();
				form.FilterControl.Grid.SetParentFilterGridModule(module);
				form.FilterControl.LoadRecentItems();

				var recentItemLinkWrapper = new LinkWrapper(ModuleIDs.ExchangeRate.Name, pk, "http://", "description");
				RecentItemManager.Instance.AddOrUpdateRecentItems("ExchangeRate", recentItemLinkWrapper);

				var items = ((MenuSection)form.FilterControlExposed.RecentItemsControlExposed.DataContext).Items;
				items[0].LinkAction.Execute(null);
				AssertEquals("IBusinessObjectLoader.Load has been invoked", true, ((DummyBusinessObjectCollectionWithBusinessObjectLoader)module.GridCollection).HasBeenInvoked);
			}
		}

		public void TestNoRecentItemsIfNoSecurityRight()
		{
			using (var form = GetNewFilterForm())
			using (var module = new DummyFilterGridModuleWithRecentModuleID())
			{
				module.LimitedColumns = new ZLimitedColumnsProvider(DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);

				form.Show();
				form.FilterControl.Grid.SetParentFilterGridModule(module);
				form.FilterControl.LoadRecentItems();

				var controll = form.FilterControlExposed.RecentItemsControlExposed;
				AssertNull(controll);
			}
		}

		public void TestRecentItemsWithDifferentModuleID()
		{
			using (var form = GetNewFilterForm())
			using (var module = new DummyFilterGridModuleWithRecentModuleID())
			{
				form.Show();
				form.FilterControl.Grid.SetParentFilterGridModule(module);
				form.FilterControl.LoadRecentItems();

				var viewModel = form.FilterControlExposed.RecentItemsControlExposed.DataContext as MenuSection;
				AssertNotNull(viewModel);

				var recentItem = new LinkWrapper(DummyModuleIDs.Dummy2.Name, Guid.NewGuid(), "http://", "description");
				RecentItemManager.Instance.AddOrUpdateRecentItems(DummyModuleIDs.Dummy2.Name, recentItem);
				viewModel = form.FilterControlExposed.RecentItemsControlExposed.DataContext as MenuSection;
				Assert("Recent item added to list", viewModel.Items[0].Key == recentItem.UniqueKey);
			}
		}

		public void TestRecentItemsAddedToFavorites()
		{
			using (var form = GetNewFilterForm())
			using (var module = new DummyFilterGridModule())
			{
				form.Show();
				form.FilterControl.Grid.SetParentFilterGridModule(module);
				form.FilterControl.LoadRecentItems();

				var viewModel = form.FilterControlExposed.RecentItemsControlExposed.DataContext as MenuSection;
				AssertNotNull(viewModel);

				var recentItem = new LinkWrapper(module.ID.Name, Guid.NewGuid(), "http://", "description");
				RecentItemManager.Instance.AddOrUpdateRecentItems(module.ID.Name, recentItem);
				viewModel = form.FilterControlExposed.RecentItemsControlExposed.DataContext as MenuSection;
				Assert("Recent item not in favorites", !RecentItemManager.Instance.IsInFavoriteModules(recentItem));

				viewModel.Items[0].FavoriteAction.Execute(null);
				Assert("Recent item added to favorites", RecentItemManager.Instance.IsInFavoriteModules(recentItem));

				viewModel.Items[0].FavoriteAction.Execute(null);
				Assert("Recent item removed from favorites", !RecentItemManager.Instance.IsInFavoriteModules(recentItem));
			}
		}

		public void TestRecentItemsAreLoadedInSeparateFactory()
		{
			using (var form = GetNewFilterForm())
			using (var module = new DummyFilterGridModule())
			{
				var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				dummy.Z0_Code = "DR0";
				Factory.Save();

				var recentItem = new LinkWrapper(module.ID.Name, dummy.PK.ToGuid(), "http://", "description");
				RecentItemManager.Instance.AddOrUpdateRecentItems(module.ID.Name, recentItem);

				form.FilterControl.Grid.SetParentFilterGridModule(module);
				form.Show();
				form.FilterControl.LoadRecentItems();

				var viewModel = form.FilterControlExposed.RecentItemsControlExposed.DataContext as MenuSection;
				AssertNotNull(viewModel);
				AssertEquals("Recent item should be added to list", recentItem.UniqueKey, viewModel.Items[0].Key);

				var query = new ZQuery(DummyBizoSchema.PK, dummy.PK) { FetchOnlyFromLocalCache = true };
				AssertNull("Bizo should not be loaded in filter's factory", module.FilterBusinessObject.Factory.LoadTop1<DummyBusinessObject>(query));

				viewModel.Items[0].LinkAction.Execute(null);
				AssertNull("Bizo should not be loaded in filter's factory", module.FilterBusinessObject.Factory.LoadTop1<DummyBusinessObject>(query));
			}
		}

		public void TestRecentItemsLinkActionShouldTriggerDefaultAction()
		{
			using (var form = GetNewFilterForm())
			using (var module = new DummyFilterGridModuleWithOverriddenDefaultAction())
			{
				var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				dummy.Z0_Code = "DR0";
				Factory.Save();

				var recentItem = new LinkWrapper(module.ID.Name, dummy.PK.ToGuid(), "http://", "description");
				RecentItemManager.Instance.AddOrUpdateRecentItems(module.ID.Name, recentItem);

				form.FilterControl.Grid.SetParentFilterGridModule(module);
				form.Show();
				form.FilterControl.LoadRecentItems();

				var viewModel = form.FilterControlExposed.RecentItemsControlExposed.DataContext as MenuSection;
				AssertNotNull(viewModel);
				var item = viewModel.Items.SingleOrDefault();
				AssertNotNull(item);

				var decisionProvider = (TestDecisionProvider)module.ModuleDecisionProvider;
				Assert(!decisionProvider.IsDefaultActionHandled);

				item.LinkAction.Execute(null);

				Assert(decisionProvider.IsDefaultActionHandled);

				var dummyForm = ZApplication.GetOpenForms().OfType<ZDummyForm>().SingleOrDefault();
				AssertNotNull(dummyForm);
				dummyForm.Close();
			}
		}

		public void TestRecentItemsLinkActionShouldPrepareSpecialDefaultActionBeforeTriggering()
		{
			using (var form = GetNewFilterForm())
			using (var module = new DummyFilterGridModuleWithSpecialOverriddenDefaultAction())
			{
				var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				dummy.Z0_Code = "DR0";
				Factory.Save();

				var recentItem = new LinkWrapper(module.ID.Name, dummy.PK.ToGuid(), "http://", "description");
				RecentItemManager.Instance.AddOrUpdateRecentItems(module.ID.Name, recentItem);

				form.FilterControl.Grid.SetParentFilterGridModule(module);
				form.Show();
				form.FilterControl.LoadRecentItems();

				var viewModel = form.FilterControlExposed.RecentItemsControlExposed.DataContext as MenuSection;
				AssertNotNull(viewModel);
				var item = viewModel.Items.SingleOrDefault();
				AssertNotNull(item);

				var decisionProvider = (TestDecisionProviderWithSpecialDefaultAction)module.ModuleDecisionProvider;
				Assert(!decisionProvider.DefaultActionDefinitionStarted);
				Assert(!decisionProvider.DefaultActionDefinitionFinished);

				item.LinkAction.Execute(null);

				Assert(decisionProvider.DefaultActionDefinitionStarted);
				Assert(decisionProvider.DefaultActionDefinitionFinished);

				var dummyForm = ZApplication.GetOpenForms().OfType<ZDummyForm>().SingleOrDefault();
				AssertNotNull(dummyForm);
				dummyForm.Close();
			}
		}

		#region Test Classes for Testing Recent Items

		class DummyFilterGridModuleWithOverriddenDefaultAction : DummyFilterGridModule
		{
			protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider()
			{
				return new TestDecisionProvider(this);
			}
		}

		class TestDecisionProvider : DefaultModuleDecisionProvider
		{
			public TestDecisionProvider(ZFilterModule module)
				: base(module)
			{
			}

			public bool IsDefaultActionHandled;

			public override void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
			{
				base.HandleDefaultAction(selectedBusinessObjects);
				IsDefaultActionHandled = true;
			}
		}

		class DummyFilterGridModuleWithSpecialOverriddenDefaultAction : DummyFilterGridModule
		{
			protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider()
			{
				return new TestDecisionProviderWithSpecialDefaultAction(this);
			}
		}

		class TestDecisionProviderWithSpecialDefaultAction : TestDecisionProvider, IHaveSpecialDefaultAction
		{
			public TestDecisionProviderWithSpecialDefaultAction(ZFilterModule module)
				: base(module)
			{
			}

			public bool DefaultActionDefinitionStarted;
			public bool DefaultActionDefinitionFinished;

			IDisposable IHaveSpecialDefaultAction.DefineDefaultActionTriggeredByRecentItems()
			{
				DefaultActionDefinitionStarted = true;
				return new DisposableAction(() => DefaultActionDefinitionFinished = true);
			}

			public override void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
			{
				Assert(DefaultActionDefinitionStarted);
				Assert(!DefaultActionDefinitionFinished);

				base.HandleDefaultAction(selectedBusinessObjects);
			}
		}

		#endregion

		#endregion

		#region TestRunSearchOnEnteringAModuleOverride

		public void TestRunSearchOnEnteringAModuleOverride()
		{
			var searchRun = false;

			EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = false;
			using (var form = GetNewFilterForm())
			{
				form.FilterControl.PerformSearch += delegate
				{ searchRun = true; };
				form.Show();

				AssertEquals(false, searchRun);
			}

			using (var form = GetNewFilterForm())
			{
				form.FilterControl.RunSearchOnEnteringAModuleOverride = true;
				form.FilterControl.PerformSearch += delegate
				{ searchRun = true; };
				form.Show();

				AssertEquals(true, searchRun);
			}

			EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
			using (var form = GetNewFilterForm())
			{
				form.FilterControl.PerformSearch += delegate
				{ searchRun = true; };
				form.Show();

				AssertEquals(true, searchRun);
			}

			using (var form = GetNewFilterForm())
			{
				form.FilterControl.RunSearchOnEnteringAModuleOverride = true;
				form.FilterControl.PerformSearch += delegate
				{ searchRun = true; };
				form.Show();

				AssertEquals(true, searchRun);
			}
		}

		#endregion

		#region Search Error Messages

		public void TestAutoSearchEmbeddedModulePopup_WithNoResults_ShouldShowMessageAfterFormShown()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var popup = new EmbeddedModulePopup(module))
			{
				var filterControl = (ZFilterStripCommonControl)module.EmbeddedControl;
				filterControl.RunSearchOnEnteringAModuleOverride = true;

				UnitTestUserNotification.Instance.ClearMessages();
				var isMessageShownAtFormLoad = false;
				popup.Load += (s, e) =>
				{
					var messageAtLoad = UnitTestUserNotification.Instance.LastMessage;
					isMessageShownAtFormLoad = messageAtLoad != null && messageAtLoad.Text == "There are no records that match your search.";
				};

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogAndDispose(popup);
				AssertEquals(false, isMessageShownAtFormLoad);

				var message = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("There are no records that match your search.", message.Text);
			}
		}

		[StressTest]
		public void TestAutoSearchEmbeddedModulePopup_WithTooManyResults_ShouldShowMessageAfterFormShown()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			using (var popup = new EmbeddedModulePopup(module))
			{
				var filterControl = (ZFilterStripCommonControl)module.EmbeddedControl;
				filterControl.RunSearchOnEnteringAModuleOverride = true;

				UnitTestUserNotification.Instance.ClearMessages();
				var isMessageShownAtFormLoad = false;
				popup.Load += (s, e) =>
				{
					var messageAtLoad = UnitTestUserNotification.Instance.LastMessage;
					isMessageShownAtFormLoad = messageAtLoad != null && messageAtLoad.Text == "This search returns more than the maximum number of records to display.\r\nThe number of search records to display can be defined in the registry up to a maximum value of 15,000 records.\r\n\r\nSee: Registry > Physical Server > Display Grid > Max No. of Records to Show\r\n\r\nThe current value is set to 1000.";
				};

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogAndDispose(popup);
				AssertEquals(false, isMessageShownAtFormLoad);

				var message = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("This search returns more than the maximum number of records to display.\r\nThe number of search records to display can be defined in the registry up to a maximum value of 15,000 records.\r\n\r\nSee: Registry > Physical Server > Display Grid > Max No. of Records to Show\r\n\r\nThe current value is set to 1000.", message.Text);
			}
		}

		public void TestAutoSearchEmbeddedModulePopup_WithNoResults_ShouldCloseWindowOnlyIfFiltersAreReadOnly()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var popup = new EmbeddedModulePopup(module))
			{
				var filterControl = (ZFilterStripCommonControl)module.EmbeddedControl;
				filterControl.RunSearchOnEnteringAModuleOverride = true;
				filterControl.IsFilterReadonly = true;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogAndDispose(popup);
				AssertEquals("The form should have been closed because the filters are read-only, and yet...", DialogResult.Abort, popup.DialogResult);
			}

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var popup = new EmbeddedModulePopup(module))
			{
				var filterControl = (ZFilterStripCommonControl)module.EmbeddedControl;
				filterControl.RunSearchOnEnteringAModuleOverride = true;
				filterControl.IsFilterReadonly = false;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogAndDispose(popup);
				AssertNotEquals("The form should not have been closed because the filters aren't read-only, and yet...", DialogResult.Abort, popup.DialogResult);
			}
		}

		public void TestManualSearchEmbeddedModulePopup_WithNoResults_ShouldShowError()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var popup = new EmbeddedModulePopup(module))
			{
				var filterControl = (ZFilterStripCommonControl)module.EmbeddedControl;

				ZFormModaliser.ShowDialogsInTest = true;
				popup.Show();
				filterControl.Find();
				AssertEquals("There are no records that match your search.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotEquals("The form should not have been closed because the search was performed manually, and yet...", DialogResult.Abort, popup.DialogResult);
			}
		}

		public void TestRegularModuleSearch_WithNoResults_ShouldShowMessageAndNotCloseForm()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new ZForm())
			{
				module.EmbeddedControl.Dock = DockStyle.Fill;

				form.Size = ControlDpiScalingHelper.NewScaledSize(1366, 768);
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var isFormClosed = false;
				form.Closed += (s, e) => isFormClosed = true;

				var filterControl = (ZFilterStripCommonControl)module.EmbeddedControl;
				filterControl.Find();

				var message = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("There are no records that match your search.", message.Text);
				AssertEquals(false, isFormClosed);
			}
		}

		[StressTest]
		public void TestRegularModuleSearch_WithTooManyResults_ShouldShowMessageAndNotCloseForm()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			using (var form = new ZForm())
			{
				module.EmbeddedControl.Dock = DockStyle.Fill;

				form.Size = ControlDpiScalingHelper.NewScaledSize(1366, 768);
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var isFormClosed = false;
				form.Closed += (s, e) => isFormClosed = true;

				var filterControl = (ZFilterStripCommonControl)module.EmbeddedControl;
				filterControl.Find();

				var message = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("This search returns more than the maximum number of records to display.\r\nThe number of search records to display can be defined in the registry up to a maximum value of 15,000 records.\r\n\r\nSee: Registry > Physical Server > Display Grid > Max No. of Records to Show\r\n\r\nThe current value is set to 1000.", message.Text);
				AssertEquals(false, isFormClosed);
			}
		}

		#endregion

		#region Implementation

		ZDummyFilterStripCommonForm GetNewFilterForm()
		{
			return new ZDummyFilterStripCommonForm(Factory.New<DummyBusinessObject>());
		}

		ZDummyFilterStripCommonForm GetNewFilterForm(FilterStripBusinessObject filterBizo)
		{
			return new ZDummyFilterStripCommonForm(Factory.New<DummyBusinessObject>(), filterBizo);
		}

		#endregion
	}

	public class ZFilterStripControlCommonSharedTest : FilterStripControlSharedTest
	{
		protected override IZDummyFilterStripForm GetNewFilterForm()
		{
			return new ZDummyFilterStripCommonForm(Factory.New<DummyBusinessObject>());
		}

		protected override IZDummyFilterStripForm GetNewFilterForm(FilterStripBusinessObject filterBizo)
		{
			return new ZDummyFilterStripCommonForm(Factory.New<DummyBusinessObject>(), filterBizo);
		}

		public override void AssertOnS9ColumnLayoutData(StmModuleFilter layout)
		{
			AssertEquals("layout.S9_ColumnLayoutData should be cleared out", ZBlob.Empty, layout.S9_ColumnLayoutData);
		}
	}
}
