using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.ZStmALogModuleTest;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZStmALogFilterControlTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestZStmALogFilterControl_Suffix()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var child = dummy.Collection.AddNew();
			child.HumanReadableNameForTest = "Child";
			child.Logs.AddNew();

			child = dummy.Collection.AddNew();
			child.HumanReadableNameForTest = "Child";
			child.Logs.AddNew();

			dummy.BusinessObjectsWithRelatedEventsForTest = delegate
			{
				var result = new List<BusinessObject>();
				result.AddRange(dummy.Collection);
				return result.ToArray();
			};

			using (var module = GetALogModuleTest(dummy))
			using (var form = new ZForm())
			{
				var control = module.EmbeddedControl as ZStmALogFilterControlForTest;
				form.Controls.Add(control);
				form.Show();

				(control.FilterBusinessObject as ZStmALogFilterBusinessObject).LogsToShow = LogsToShow.All;
				AssertEquals("Precondition", 0, control.GridCollection.Count);
				control.FindForTest();

				AssertEquals("should find 2 records", 2, control.GridCollection.Count);
				var log1 = control.GridCollection.ToArray<StmALog>()[0];
				var log2 = control.GridCollection.ToArray<StmALog>()[1];
				if (string.IsNullOrEmpty(log1.SuffixForTableFriendlyName))
				{
					AssertEquals("[2]", log2.SuffixForTableFriendlyName);
				}
				else
				{
					Assert(string.IsNullOrEmpty(log2.SuffixForTableFriendlyName));
					AssertEquals("[2]", log1.SuffixForTableFriendlyName);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestZStmALogFilterControl_PerformSearch()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			dummy.Logs.AddNew();
			dummy.Logs.AddNew();

			using (var module = GetALogModuleTest(dummy))
			using (var form = new ZForm())
			{
				var control = module.EmbeddedControl as ZStmALogFilterControlForTest;
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Precondition", 0, control.GridCollection.Count);
				control.FindForTest();
				AssertEquals("should find 2 records", 2, control.GridCollection.Count);
			}
		}

		public void TestContextMenu()
		{
			DummyEnterpriseBusinessObject dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();

			using (var module = GetALogModuleTest(dummy))
			{
				var control = module.EmbeddedControl as ZStmALogFilterControlForTest;
				(control.FilterBusinessObject as ZStmALogFilterBusinessObject).LogsToShow = LogsToShow.All;
				InvokeContextMenuPopupEvent(control.FilteredGrid.ContextMenu);
				AssertEquals("Menu item visible when Operations logs available", true, GetAddNewEventMenuItem(control.FilteredGrid.ContextMenu).Visible);
				AssertEquals("Menu item visible when Operations logs available", true, GetCancelMenuItem(control.FilteredGrid.ContextMenu).Visible);

				(control.FilterBusinessObject as ZStmALogFilterBusinessObject).LogsToShow = LogsToShow.ChangeLogs;
				InvokeContextMenuPopupEvent(control.FilteredGrid.ContextMenu);
				AssertEquals("Menu item not visible when only ChangeLogs available", false, GetAddNewEventMenuItem(control.FilteredGrid.ContextMenu).Visible);
				AssertEquals("Menu item not visible when only ChangeLogs available", false, GetCancelMenuItem(control.FilteredGrid.ContextMenu).Visible);
			}
		}

		public void TestAddNewEventMenuItemShouldBeHiddenIfLogParentPKIsInvalid()
		{
			var dummy = new StmALogParentWithLogParentPK();

			using (var module = GetALogModuleTest(dummy))
			{
				var control = module.EmbeddedControl as ZStmALogFilterControlForTest;
				dummy.LogsParentPK = ZGuid.Invalid;
				InvokeContextMenuPopupEvent(control.FilteredGrid.ContextMenu);
				AssertEquals("Menu item not visible when logs parent is not valid", false, GetAddNewEventMenuItem(control.FilteredGrid.ContextMenu).Visible);

				dummy.LogsParentPK = Guid.NewGuid();
				(control.FilterBusinessObject as ZStmALogFilterBusinessObject).LogsToShow = LogsToShow.All;
				InvokeContextMenuPopupEvent(control.FilteredGrid.ContextMenu);
				AssertEquals("Menu item visible when Operations logs available", true, GetAddNewEventMenuItem(control.FilteredGrid.ContextMenu).Visible);
			}
		}

		class StmALogParentWithLogParentPK : NonPersistentBusinessObject, IStmALogParent
		{
			Logs IStmALogProvider.Logs => throw new NotImplementedException();

			BusinessObjectFactory IStmALogProvider.LogsFactory => throw new NotImplementedException();

			void IStmALogParent.ProcessLog(IStmALog log)
			{
				throw new NotImplementedException();
			}

			public ZGuid LogsParentPK { get; set; }

			string IStmALogParent.LogsParentTableName => throw new NotImplementedException();

			BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents => Array.Empty<BusinessObject>();

			bool IStmALogParent.DeferFiringWorkflow => throw new NotImplementedException();
		}

		[UseSnapshotProtection]
		public void TestCancelMenuItemCaption()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			using (var module = GetALogModuleTest(dummy))
			using (var form = new ZForm())
			{
				var control = module.EmbeddedControl as ZStmALogFilterControlForTest;
				form.Controls.Add(control);
				form.Show();

				((ModuleFlagsFilter)(control.FilterBusinessObject as ZStmALogFilterBusinessObject)[ZStmALogFilterBusinessObject.Schema.ShowCancelled]).Property0 = ZBool.True;

				var log = dummy.Logs.AddNew();

				control.FindForTest();
				InvokeContextMenuPopupEvent(control.FilteredGrid.ContextMenu);
				var cancelMenuItem = GetCancelMenuItem(control.FilteredGrid.ContextMenu);
				AssertEquals("Cancel Selected Event", cancelMenuItem.Text);
				AssertEquals(true, cancelMenuItem.Enabled);

				control.FilteredGrid.Select(0);
				cancelMenuItem.PerformClick();

				InvokeContextMenuPopupEvent(control.FilteredGrid.ContextMenu);
				AssertEquals("Event already canceled", cancelMenuItem.Text);
				AssertEquals(false, cancelMenuItem.Enabled);
				AssertEquals(true, log.SL_IsCancelled);
			}
		}

		[UseSnapshotProtection]
		public void TestCancelMenuItemCaption_NotCancellableEvent()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			using (var module = GetALogModuleTest(dummy))
			using (var form = new ZForm())
			{
				var control = module.EmbeddedControl as ZStmALogFilterControlForTest;
				form.Controls.Add(control);
				form.Show();

				((ModuleFlagsFilter)(control.FilterBusinessObject as ZStmALogFilterBusinessObject)[ZStmALogFilterBusinessObject.Schema.ShowCancelled]).Property0 = ZBool.True;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				var log = dummy.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

				control.FindForTest();
				control.FilteredGrid.Select(0);
				InvokeContextMenuPopupEvent(control.FilteredGrid.ContextMenu);
				var cancelMenuItem = GetCancelMenuItem(control.FilteredGrid.ContextMenu);
				AssertEquals("Cannot cancel " + log.SL_SE_NKEvent + " event", cancelMenuItem.Text);
				AssertEquals(false, cancelMenuItem.Enabled);
			}
		}

		[ExpectNoExceptions()]
		public void TestAddNewEvent_CancelButtonClicked()
		{
			DummyEnterpriseBusinessObject dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			using (var module = GetALogModuleTest(dummy))
			{
				var control = module.EmbeddedControl as ZStmALogFilterControlForTest;
				InvokeContextMenuPopupEvent(control.FilteredGrid.ContextMenu);
				MenuItem addNewEventMenuItem = GetAddNewEventMenuItem(control.FilteredGrid.ContextMenu);
				using (ZFormModaliser.SuspendDispose())
				{
					addNewEventMenuItem.PerformClick();
					ZStmALogAddForm addForm = (ZStmALogAddForm)ZFormModaliser.LastFormShownDialogForTest;
					addForm.Show();
					ZButton cancelButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["CancelAddButton"];
					cancelButton.PerformClick();
					AssertEquals("Should not add event (StmALog) to collection if adding was cancelled", 0, dummy.Logs.GetAllLogs().Count);
				}
			}
		}

		public void TestZBindingContextUsed()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			using (var form = new ZForm(dummy))
			using (var module = GetALogModuleTest(dummy))
			{
				var control = module.EmbeddedControl as ZStmALogFilterControlForTest;
				form.Controls.Add(control);
				AssertEquals("We use the parent's ZBindingContext if we're attached to a parent ZForm", typeof(ZBindingContext), control.Grid.BindingContext.GetType());
			}

			using (var module = GetALogModuleTest(dummy))
			{
				var control = module.EmbeddedControl as ZStmALogFilterControlForTest;
				AssertEquals("BindingContext is used if we are not attached to a parent ZForm", typeof(BindingContext), control.Grid.BindingContext.GetType());
			}
		}

		[ExpectNoExceptions()]
		public void TestAddNewEvent_AddButtonClicked()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			using (var module = GetALogModuleTest(dummy))
			{
				var control = module.EmbeddedControl as ZStmALogFilterControlForTest;

				InvokeContextMenuPopupEvent(control.FilteredGrid.ContextMenu);
				var addNewEventMenuItem = GetAddNewEventMenuItem(control.FilteredGrid.ContextMenu);
				using (ZFormModaliser.SuspendDispose())
				{
					addNewEventMenuItem.PerformClick();
					var addForm = (ZStmALogAddForm)ZFormModaliser.LastFormShownDialogForTest;
					addForm.Show();
					var addFormBusinessEntity = (BaseStmALog)addForm.BusinessEntity;
					addFormBusinessEntity.SL_SE_NKEvent = Events.ArrivalCode;
					var addButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["AddButton"];

					var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ArrivalCode);
					var allLogs = dummy.Logs.GetAllLogs();
					var arvLogsCount_Before = allLogs.Find(query).Length;

					addButton.PerformClick();

					var arvLogsCount_After = allLogs.Find(query).Length;
					AssertEquals("One more ARV Event should be added", arvLogsCount_Before + 1, arvLogsCount_After);
				}
			}
		}

		[RequiresSTA]
		[UseSnapshotProtection]
		public void TestAddNewEvent_VisibleImmediately()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			dummy.Logs.AddNew();
			Factory.Save();

			using (var module = GetALogModuleTest(dummy))
			using (var form = new ZForm())
			{
				var control = module.EmbeddedControl as ZStmALogFilterControlForTest;
				form.Controls.Add(control);
				form.Show();

				InvokeLogAddForm(control, Events.AcknowledgedCode);
				AssertEquals("Grid should add a row immediately.", 1, control.FilteredGrid.VisibleRowCount);
				AssertEquals("A new row should be added at the head.", Events.AcknowledgedCode, control.GridCollection.ToArray<StmALog>()[0].SL_SE_NKEvent);

				InvokeLogAddForm(control, Events.ArrivalCode);
				AssertEquals("Grid should add another row immediately.", 2, control.FilteredGrid.VisibleRowCount);
				AssertEquals("Another new row should be added at the head.", Events.ArrivalCode, control.GridCollection.ToArray<StmALog>()[0].SL_SE_NKEvent);

				InvokeLogAddForm(control, Events.ArrivalCode, add: false);
				AssertEquals("Grid should not be changed when cancelled.", 2, control.FilteredGrid.VisibleRowCount);
			}
		}

		void InvokeLogAddForm(ZStmALogFilterControlForTest control, ZString eventCode, bool add = true)
		{
			InvokeContextMenuPopupEvent(control.FilteredGrid.ContextMenu);
			var addNewEventMenuItem = GetAddNewEventMenuItem(control.FilteredGrid.ContextMenu);
			using (ZFormModaliser.SuspendDispose())
			{
				addNewEventMenuItem.PerformClick();
				var addForm = (ZStmALogAddForm)ZFormModaliser.LastFormShownDialogForTest;
				addForm.Show();
				var addFormBusinessEntity = (BaseStmALog)addForm.BusinessEntity;
				addFormBusinessEntity.SL_SE_NKEvent = eventCode;
				if (add)
				{
					var addButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["AddButton"];
					addButton.PerformClick();
				}
				else
				{
					var cancelButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["CancelAddButton"];
					cancelButton.PerformClick();
				}
			}
		}

		void InvokeContextMenuPopupEvent(ContextMenu menu)
		{
			typeof(ContextMenu).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menu, new object[] { EventArgs.Empty });
		}

		MenuItem GetAddNewEventMenuItem(ContextMenu menu)
		{
			foreach (MenuItem menuItem in menu.MenuItems)
			{
				if (menuItem.Text == "&Add New Event")
				{
					return menuItem;
				}
			}
			return null;
		}

		MenuItem GetCancelMenuItem(ContextMenu menu)
		{
			foreach (MenuItem menuItem in menu.MenuItems)
			{
				if (menuItem.Text.ToLower().IndexOf("cancel") != -1)
				{
					return menuItem;
				}
			}
			return null;
		}

		[UseSnapshotProtection]
		public void TestSaveLastUsedLayoutInDisposing()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			var filterA = dummyFilterStripBizO.Layouts.AddNew();
			filterA.S9_FilterName = "AP";
			filterA.S9_IsPublished = true;
			var filterB = dummyFilterStripBizO.Layouts.AddNew();
			filterB.S9_FilterName = "EU";
			filterB.S9_IsPublished = false;

			var collection = new DummyBusinessObjectCollection(Factory);
			using (var form = new ZForm())
			{
				using (var module = GetALogModuleTest(dummy, (m) => dummyFilterStripBizO))
				{
					var filterControl = module.EmbeddedControl as ZStmALogFilterControl;
					form.Controls.Add(filterControl);
					form.Show();

					AssertNull(dummyFilterStripBizO.GetLastUsedLayout());
					var filterLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;
					filterLayouts[3].PerformClick();
				}
			}

			using (var form = new ZForm())
			{
				using (var module = GetALogModuleTest(dummy, (m) => dummyFilterStripBizO))
				{
					var filterControl = module.EmbeddedControl as ZStmALogFilterControl;
					form.Controls.Add(filterControl);
					form.Show();

					AssertNotNull(dummyFilterStripBizO.GetLastUsedLayout());
					AssertEquals("AP", dummyFilterStripBizO.GetLastUsedLayout().DisplayName);
					AssertEquals("Find (AP)", filterControl.ToolStripFindDropButton_ForTest.Text);

					var filterLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;
					filterLayouts[1].PerformClick();
				}
			}

			using (var form = new ZForm())
			{
				using (var module = GetALogModuleTest(dummy, (m) => dummyFilterStripBizO))
				{
					var filterControl = module.EmbeddedControl as ZStmALogFilterControl;
					form.Controls.Add(filterControl);
					form.Show();

					AssertNotNull(dummyFilterStripBizO.GetLastUsedLayout());
					AssertEquals("EU", dummyFilterStripBizO.GetLastUsedLayout().DisplayName);
					AssertEquals("Find (EU)", filterControl.ToolStripFindDropButton_ForTest.Text);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestGridCollectionOnLoad_DisableFetchHintOnGenPivot()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var log1 = dummy.Logs.AddNew();
			var log2 = dummy.Logs.AddNew();
			var log3 = dummy.Logs.AddNew();
			log1.SL_GS_NKUser = "A";
			log2.SL_GS_NKUser = "B";
			log3.SL_GS_NKUser = "C";
			Factory.Save();

			using (var form = new ZForm(dummy))
			using (var eventSourceInfoUserControl = new EventSourceInfoUserControl())
			using (var module = GetALogModuleTest(dummy))
			using (var grid = new ZGrid())
			{
				var filterControl = module.EmbeddedControl as ZStmALogFilterControlForTest;

				form.DisplayMode = Core.ODisplayMode.ReadOnly;
				var info = new ZTextBoxColumnStyleInfo();
				info.ColumnName = StmALogSchema.Constants.SL_Reference;
				grid.ColumnStyles.Add(info);

				form.Controls.Add(filterControl);
				form.Controls.Add(eventSourceInfoUserControl);
				form.Controls.Add(grid);
				eventSourceInfoUserControl.SetDataBinding(filterControl.GridCollection, "");
				grid.SetDataBinding(filterControl.GridCollection, "");

				form.Show();
				filterControl.FindForTest();
				grid.ListManager.Position = 0;
				grid.ListManager.Position = 1;
				grid.ListManager.Position = 2;
				AssertEquals("Should have 3 hit to GenPivot", 3, module.FactoryExposed.GetTableHitCount(GenPivotSchema.Constants.TableName));
			}
		}

		ZStmALogModuleForTest GetALogModuleTest(IStmALogParent master, GetStmALogFilterStripBusinessObject getStmALogFilterStripBusinessObject = null)
		{
			var module = new ZStmALogModuleForTest();
			module.InitData(master, getStmALogFilterStripBusinessObject);
			return module;
		}
		BusinessObjectFactory Factory => factory ??= new();
		BusinessObjectFactory factory;
	}

	public class ZStmALogFilterControlForTest : ZStmALogFilterControl
	{
		public ZStmALogFilterControlForTest(IStmALogParent master, IBusinessObjectCollection collection, FilterStripBusinessObject filterStripBusinessObject, ZStmALogModule module)
			: base(master, collection, filterStripBusinessObject, module)
		{
			this.module = module as ZStmALogModuleForTest;
		}

		readonly ZStmALogModuleForTest module;

		public void FindForTest()
		{
			Find();
			module?.AsyncTask_Exposed?.Wait();
			Application.DoEvents();
		}
	}
}
