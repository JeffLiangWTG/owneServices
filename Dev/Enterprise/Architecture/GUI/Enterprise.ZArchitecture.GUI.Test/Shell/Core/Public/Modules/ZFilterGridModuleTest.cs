using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Async.Test;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Billing.Business.Testing;
using Enterprise.Core.GUI.Testing;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.AutoRefresh;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	class ZFilterGridModuleTest : ZFilterModuleTest
	{
		public void TestAutoRefreshCanHandleInvalidCustomSqlFilter()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterControl = (DummyFilterControl)module.EmbeddedControl;

				module.AutoRefreshTimerExposed.Interval = int.MaxValue;
				module.AutoRefreshTimerExposed.Enabled = true;

				var sqlFilter = (ModuleSQLFilter)module.FilterBusinessObject["Custom SQL Filter"];
				sqlFilter.IsActive = true;
				sqlFilter.Property1 = "1=1";

				AssertEquals("Timer should enable", true, module.AutoRefreshTimerExposed.Enabled);
				module.fAutoRefreshTimer_TickExposed(module.AutoRefreshTimerExposed, null);
				AssertEquals("Timer should enable", true, module.AutoRefreshTimerExposed.Enabled);
				AssertEquals("No Auto refresh warning", false, filterControl.AutoRefreshWarningLabelExposed.Visible);

				sqlFilter.Property1 = "rubbish";
				module.fAutoRefreshTimer_TickExposed(module.AutoRefreshTimerExposed, null);
				AssertEquals("Should disable", false, module.AutoRefreshTimerExposed.Enabled);
				AssertEquals("Should show auto refresh warning", true, filterControl.AutoRefreshWarningLabelExposed.Visible);
				AssertEquals(filterControl.AutoRefreshQueryQueryErrorMessage, filterControl.AutoRefreshWarningLabelExposed.Text);
			}
		}

		[UseSnapshotProtection]
		public void TestAutoRefreshCanHandleDatabaseUpgradeException()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.AutoRefreshTimerExposed.Interval = int.MaxValue;
				module.AutoRefreshTimerExposed.Enabled = true;

				try
				{
					using (new DisposableAction(Db.Connection.CloseConnection, Db.Connection.BeginTransaction))
					using (Db.DisposableUpgrade_ForTest(acquireLockOut: false))
					{
						AssertNoExceptionThrown(() => module.fAutoRefreshTimer_TickExposed(module.AutoRefreshTimerExposed, null));
						AssertEquals("Should disable", false, module.AutoRefreshTimerExposed.Enabled);
					}
				}
				finally
				{
					Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
				}
			}
		}

		public void TestStartAutoRefresh()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterControl = (DummyFilterControl)module.EmbeddedControl;

				AutoRefreshManager.Instance.SetAutoRefreshTimeOut(DummyModuleIDs.Dummy, false, byte.MaxValue - 1);
				module.AutoRefreshTimerExposed.Enabled = false;

				module.StartAutoRefreshExposed(byte.MaxValue);

				AssertEquals("Timer should on", true, module.AutoRefreshTimerExposed.Enabled);
				AssertEquals("No Auto refresh warning", false, filterControl.AutoRefreshWarningLabelExposed.Visible);
				AssertEquals("Auto refresh registry should be on", true, AutoRefreshManager.Instance.IsAutoRefreshEnabled(DummyModuleIDs.Dummy));
				AssertEquals("Auto refresh timeout", byte.MaxValue, AutoRefreshManager.Instance.GetAutoRefreshTimeOut(DummyModuleIDs.Dummy));
				AssertEquals("Menu item", true, module.AutoRefreshMenuItemExposed.Checked);
			}
		}

		public void TestStopAutoRefresh()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterControl = (DummyFilterControl)module.EmbeddedControl;

				AutoRefreshManager.Instance.SetAutoRefreshTimeOut(DummyModuleIDs.Dummy, true, byte.MaxValue);
				module.AutoRefreshTimerExposed.Enabled = true;
				module.AutoRefreshTimerExposed.Interval = byte.MaxValue;

				module.StopAutoRefreshExposed(AutoRefreshWarningType.None);

				AssertEquals("Timer", false, module.AutoRefreshTimerExposed.Enabled);
				AssertEquals("No Auto refresh warning", false, filterControl.AutoRefreshWarningLabelExposed.Visible);
				AssertEquals("Auto refresh registry should be off", false, AutoRefreshManager.Instance.IsAutoRefreshEnabled(DummyModuleIDs.Dummy));
				AssertEquals("Menu item", false, module.AutoRefreshMenuItemExposed.Checked);
			}
		}

		public void TestStopAutoRefreshTemporary()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterControl = (DummyFilterControl)module.EmbeddedControl;

				AutoRefreshManager.Instance.SetAutoRefreshTimeOut(DummyModuleIDs.Dummy, true, byte.MaxValue);
				module.AutoRefreshTimerExposed.Enabled = true;
				module.AutoRefreshTimerExposed.Interval = byte.MaxValue;

				module.StopAutoRefreshExposed(AutoRefreshWarningType.SlowQuery);

				AssertEquals("Timer", false, module.AutoRefreshTimerExposed.Enabled);
				AssertEquals("Auto refresh warning", true, filterControl.AutoRefreshWarningLabelExposed.Visible);
				AssertEquals("Auto refresh registry should be on", true, AutoRefreshManager.Instance.IsAutoRefreshEnabled(DummyModuleIDs.Dummy));
				AssertEquals("Menu item", false, module.AutoRefreshMenuItemExposed.Checked);
			}
		}

		public void TestGetSelectedBusinessObject_ReturnsEmptyWhenNonInteractive()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				Globals.IsUserInteractive = false;
				AssertArrayEqualsByElements("When there is no user to SelectBusinessObjects, no objects are selected and an empty array is returned.", Array.Empty<object>(), module.GetSelectedBusinessObjects());
			}

			// We're still not supposed to access the selected items in a non interactive environment, but we dont need to blow up for it. A report will suffice
			if (ErrorReporter.LastKeyReported == "CantCreateWinformsFromNonInteractive" && ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}

		public void TestGetSelectedBusinessObject_WhenUserInteractive()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new ZForm())
			{
				Factory.Save();

				var filterControl = (DummyFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				AssertArrayEqualsByElements("Before elements are selected, there are no SelectedElements to return.", Array.Empty<object>(), module.GetSelectedBusinessObjects());

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();
				AssertEquals("We should see the Dummy from TestCaseWithDummy.", 1, module.GetSelectedBusinessObjects().Length);
			}
		}

		public void TestAdditionalActionsMenuItems()
		{
			var provider = new Mock<IFilterGridMenuItemProvider>();

			provider
				.Setup(p => p.GetMenuItems(It.IsAny<ZFilterGridModule>()))
				.Returns(new[] { new ZMenuItem("I can't even"), new ZMenuItem("I just. I didn't.") });

			using (ObjectFactory.Substitute("FilterGridActionsMenuItemProviders", new ArrayList { provider.Object }))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var notUsedButCausesActionsMenuToLoad = module.EmbeddedControl)
			{
				AssertNotNull(module.ActionsMenuItem.MenuItems.FindByText("I can't even"));
				AssertNotNull(module.ActionsMenuItem.MenuItems.FindByText("I just. I didn't."));
			}
		}

		public void TestMenuItem_CloneMenu()
		{
			var cache = new MockResourceStringCache("ENG");
			cache.Put("New", new ResourceStringData("MockModuleGrid.New", "&New"));

			var menuItem = new ZMenuItem(cache.Get(0, "New"), (o, e) => { }, IconTypes.NewButtonActive, IconTypes.NewButtonRest);

			AssertNull("MenuItem's Caption should be null", menuItem.Caption);
			AssertEquals("&New", menuItem.CaptionResourceString.Caption);
			AssertEquals("&New", menuItem.Text);
			AssertEquals(IconTypes.NewButtonActive, menuItem.ActiveIcon);
			AssertEquals(IconTypes.NewButtonRest, menuItem.RestIcon);

			var cloneMenuItem = (ZMenuItem)menuItem.CloneMenu();

			AssertEquals("Clone MenuItem's Text should equal with original MenuItem", menuItem.Text, cloneMenuItem.Text);
			AssertEquals("Clone MenuItem's ActiveIcon should equal with original MenuItem", menuItem.ActiveIcon, cloneMenuItem.ActiveIcon);
			AssertEquals("Clone MenuItem's RestIcon should equal with original MenuItem", menuItem.RestIcon, cloneMenuItem.RestIcon);
		}

		public void TestMenuItem_CloneMenu_TextShouldNotBeNull()
		{
			var menuItem = new ZMenuItem((NoResString)"&New");

			AssertNull("MenuItem's CaptionResourceString should be null", menuItem.CaptionResourceString);
			AssertEquals("&New", menuItem.Text);

			var cloneMenuItem = (ZMenuItem)menuItem.CloneMenu();

			AssertEquals("Clone MenuItem's Text should equal with original MenuItem", menuItem.Text, cloneMenuItem.Text);

			var cache = new MockResourceStringCache("ENG");
			cache.Put("NewMenu", new ResourceStringData("MockModuleGrid.New", "&NewMenu"));
			var menuItem2 = new ZMenuItem(cache.Get(0, "NewMenu"));

			AssertNull("MenuItem's Caption should be null", menuItem2.Caption);
			AssertEquals("&NewMenu", menuItem2.Text);

			var cloneMenuItem2 = (ZMenuItem)menuItem2.CloneMenu();

			AssertEquals("Clone MenuItem's Text should equal with original MenuItem", menuItem2.Text, cloneMenuItem2.Text);
		}

		public void TestMenuItem_CaptionAndCaptionResourceString()
		{
			//if Caption and CaptionResource set a different value, should reset another
			var cache = new MockResourceStringCache("ENG");
			cache.Put("New", new ResourceStringData("MockModuleGrid.New", "&New"));

			var menuItem = new ZMenuItem(cache.Get(0, "New"), (o, e) => { });

			AssertEquals("Create by ResourceStringData, CaptionResourceString.Caption should have value", "&New", menuItem.CaptionResourceString.Caption);
			AssertNull("Create by ResourceStringData, Caption should be null", menuItem.Caption);

			menuItem.Caption = (NoResString)"&New";

			AssertEquals("Caption should have value", "&New", menuItem.Caption);
			AssertEquals("CaptionResourceString should have value", "&New", menuItem.CaptionResourceString.Caption);

			menuItem.Caption = (NoResString)"&NewNew";
			AssertEquals("Caption should have value", "&NewNew", menuItem.Caption);
			AssertNull("CaptionResourceString should be null", menuItem.CaptionResourceString);

			menuItem.CaptionResourceString = cache.Get(0, "New");

			AssertEquals("CaptionResourceString should have value", "&New", menuItem.CaptionResourceString.Caption);
			AssertNull("Caption should be null", menuItem.Caption);
		}

		public void TestAdvancedDataAutomationWizardMenuItem_ShouldNotBeShownWhenRegistryIsNotEnabled()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (Env.SetTemporaryUserContext(EnsureTempStaff().PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (module.EmbeddedControl)
			{
				AssertNotNull(module.DataTransferMenuItem);
				module.DataTransferMenuItem.OnPopup(EventArgs.Empty);
				AssertNull(module.DataTransferMenuItem.MenuItems.FindByText("Advanced Data Automation Wizard"));
			}
		}

		public void TestAdvancedDataAutomationWizardMenuItem_ShouldNotBeShownWhenAllowNewFalse()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			SetAllowNew(false);
			using (Env.SetTemporaryUserContext(EnsureTempStaff().PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (module.EmbeddedControl)
			{
				AssertNotNull(module.DataTransferMenuItem);
				module.DataTransferMenuItem.OnPopup(EventArgs.Empty);
				AssertNull(module.DataTransferMenuItem.MenuItems.FindByText("Advanced Data Automation Wizard"));
			}
		}

		public void TestAdvancedDataAutomationWizardMenuItem_AllowAdvancedDataAutomationWizard_DoNotAllowNew_ShouldShow()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (Env.SetTemporaryUserContext(EnsureTempStaff().PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var form = new ZChildForm())
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				SetAllowNew(false);
				module.SetAllowAdvancedDataAutomationWizard(true);
				form.Controls.Add(module.EmbeddedControl);
				form.Text = "Data Automation Wizard Test";
				form.Show();

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var dataTransferMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("D&ata Transfer");
				dataTransferMenuItem.OnPopup(EventArgs.Empty);
				var dataTransferMenuItems = dataTransferMenuItem.MenuItems;

				AssertNotNull("Precondition: Data transfer menu should exist.", dataTransferMenuItems);
				module.AddNativeDataTransferMenuItem(dataTransferMenuItems);

				AssertNotNull("Expected to show Data Automation Wizard even when new items are not allowed.", dataTransferMenuItems.FindByText("Advanced Data Automation Wizard"));
			}
		}

		public void TestAdvancedDataAutomationWizardMenuItem_AllowAdvancedDataAutomationWizard_AllowNew_ShouldShow()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (Env.SetTemporaryUserContext(EnsureTempStaff().PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var form = new ZChildForm())
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				SetAllowNew(true);
				module.SetAllowAdvancedDataAutomationWizard(true);
				form.Controls.Add(module.EmbeddedControl);
				form.Text = "Data Automation Wizard Test";
				form.Show();

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var dataTransferMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("D&ata Transfer");
				dataTransferMenuItem.OnPopup(EventArgs.Empty);
				var dataTransferMenuItems = dataTransferMenuItem.MenuItems;

				AssertNotNull("Precondition: Data transfer menu should exist.", dataTransferMenuItems);
				module.AddNativeDataTransferMenuItem(dataTransferMenuItems);

				var count = dataTransferMenuItems.Count;
				AssertEquals("-", dataTransferMenuItems[count - 2].Text);
				AssertEquals("Advanced Data Automation Wizard", dataTransferMenuItems[count - 1].Text);

				var adawMenuItem = dataTransferMenuItems[count - 1];
				AssertEquals(1, adawMenuItem.MenuItems.Count);
				AssertEquals("Loading...", adawMenuItem.MenuItems[0].Text);
			}
		}

		public void TestAdvancedDataAutomationWizardMenuItem_DoNotAllowAdvancedDataAutomationWizard_AllowNew_ShouldNotShow()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (Env.SetTemporaryUserContext(EnsureTempStaff().PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var form = new ZChildForm())
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				SetAllowNew(true);
				module.SetAllowAdvancedDataAutomationWizard(false);
				form.Controls.Add(module.EmbeddedControl);
				form.Text = "Data Automation Wizard Test";
				form.Show();

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var dataTransferMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("D&ata Transfer");
				dataTransferMenuItem.OnPopup(EventArgs.Empty);
				var dataTransferMenuItems = dataTransferMenuItem.MenuItems;

				AssertNotNull("Precondition: Data transfer menu should exist.", dataTransferMenuItems);
				module.AddNativeDataTransferMenuItem(dataTransferMenuItems);

				AssertNull("Expected Data Automation Wizard to be disabled.", dataTransferMenuItems.FindByText("Advanced Data Automation Wizard"));
			}
		}

		public void TestAdvancedDataAutomationWizardMenuItem_ShouldBeShown()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (Env.SetTemporaryUserContext(EnsureTempStaff().PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (module.EmbeddedControl)
			{
				AssertNotNull(module.DataTransferMenuItem);
				module.DataTransferMenuItem.OnPopup(EventArgs.Empty);
				AssertNotNull(module.DataTransferMenuItem.MenuItems.FindByText("Advanced Data Automation Wizard"));
			}
		}

		public void TestAdvancedDataAutomationWizardMenuItem_CallsGlowIntegrationWhenSelected()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var mapping1 = Factory.New<StmModuleFilter>();
			mapping1.S9_ModuleID = "GLOWDataImportV2_IDummyBizo";
			mapping1.S9_FilterName = "test mapping 1";
			mapping1.S9_RelatedEntityID = Guid.Empty;

			var mapping2 = Factory.New<StmModuleFilter>();
			mapping2.S9_ModuleID = "GLOWDataImportV2_IDummyBizo";
			mapping2.S9_FilterName = "test mapping 2";
			mapping2.S9_RelatedEntityID = Guid.Empty;

			Factory.Save();
			try
			{
				var parserMock = new Mock<IDataTransferMappingParser>();
				parserMock.Setup(p => p.FromModuleFilterInfo(Moq.It.IsAny<IModuleFilterInfo>())).Returns((IModuleFilterInfo filterInfo) =>
				{
					var mappingMock = new Mock<IDataTransferMapping>();
					mappingMock.Setup(m => m.Name).Returns(filterInfo.FilterName);
					mappingMock.Setup(m => m.PK).Returns(filterInfo.PK);
					return mappingMock.Object;
				});
				ObjectFactory.Substitute(parserMock.Object);

				using (Env.SetTemporaryUserContext(EnsureTempStaff().PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				using (var module = new DummyFilterGridModuleWithDummyImportWizard(string.Empty))
				using (module.EmbeddedControl)
				{
					module.SetAllowAdvancedDataAutomationWizard(true);
					AssertNotNull(module.DataTransferMenuItem);

					var importWizardMenuItem = module.DataTransferMenuItem.MenuItems.FindByText("Advanced Data Automation Wizard");
					AssertNotNull(importWizardMenuItem);
					importWizardMenuItem.OnPopup(EventArgs.Empty);

					AssertEquals(importWizardMenuItem.MenuItems.Count, 2);
					var mainImportWizardMenuItem = importWizardMenuItem.MenuItems[1];
					AssertEquals(mainImportWizardMenuItem.Text, "Manage Mappings");

					var manageMappingMenuItems = mainImportWizardMenuItem.MenuItems;
					AssertEquals(manageMappingMenuItems.Count, 3);
					AssertEquals(manageMappingMenuItems[0].Text, "New Mapping");
					AssertEquals(manageMappingMenuItems[1].Text, "test mapping 1");
					AssertEquals(manageMappingMenuItems[2].Text, "test mapping 2");

					manageMappingMenuItems[0].PerformClick();
					AssertEquals(module.TypeOfElements, typeof(DummyBusinessObject));
					AssertEquals(module.Mapping.Name, "New Mapping");
					AssertEquals(module.Mapping.PK, Guid.Empty);

					manageMappingMenuItems[1].PerformClick();
					AssertEquals(module.TypeOfElements, typeof(DummyBusinessObject));
					AssertEquals(module.Mapping.Name, "test mapping 1");
					AssertEquals(module.Mapping.PK, mapping1.PK);

					mapping1.Delete();
					Factory.Save();

					importWizardMenuItem.OnPopup(EventArgs.Empty);
					mainImportWizardMenuItem = importWizardMenuItem.MenuItems[1];

					manageMappingMenuItems = mainImportWizardMenuItem.MenuItems;
					AssertEquals(manageMappingMenuItems.Count, 2);
					AssertEquals(manageMappingMenuItems[0].Text, "New Mapping");
					AssertEquals(manageMappingMenuItems[1].Text, "test mapping 2");
				}
			}
			finally
			{
				ObjectFactory.DisposeSubstitutions();
			}
		}

		public void TestAdvancedDataAutomationWizardMenuItem_ShouldShowEmbeddedMenu()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var mapping1 = Factory.New<StmModuleFilter>();
			mapping1.S9_ModuleID = "GLOWDataImportV2_IDummyBizo";
			mapping1.S9_FilterName = "test mapping 1";
			mapping1.S9_RelatedEntityID = Guid.Empty;

			var mapping2 = Factory.New<StmModuleFilter>();
			mapping2.S9_ModuleID = "GLOWDataImportV2_IDummyBizo";
			mapping2.S9_FilterName = "test mapping 2";
			mapping2.S9_RelatedEntityID = Guid.Empty;

			Factory.Save();
			try
			{
				var parserMock = new Mock<IDataTransferMappingParser>();
				parserMock.Setup(p => p.FromModuleFilterInfo(Moq.It.IsAny<IModuleFilterInfo>())).Returns((IModuleFilterInfo filterInfo) =>
				{
					var mappingMock = new Mock<IDataTransferMapping>();
					mappingMock.Setup(m => m.Name).Returns(filterInfo.FilterName);
					mappingMock.Setup(m => m.PK).Returns(filterInfo.PK);
					return mappingMock.Object;
				});
				ObjectFactory.Substitute(parserMock.Object);

				using (Env.SetTemporaryUserContext(EnsureTempStaff().PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				using (var module = new DummyFilterGridModuleWithDummyImportWizard(string.Empty))
				using (module.EmbeddedControl)
				{
					AssertNotNull(module.DataTransferMenuItem);

					var importWizardMenuItem = module.DataTransferMenuItem.MenuItems.FindByName("GlowIntegrationMenuItem");
					AssertNotNull(importWizardMenuItem);
					importWizardMenuItem.OnPopup(EventArgs.Empty);

					AssertEquals(importWizardMenuItem.MenuItems.Count, 2);
					AssertEquals(importWizardMenuItem.MenuItems[1].Text, "Manage Mappings");

					var importEmbeddedWizardMenuItem = importWizardMenuItem.MenuItems[0];
					AssertEquals(importEmbeddedWizardMenuItem.Text, "Import File Using");

					AssertEquals(importEmbeddedWizardMenuItem.MenuItems.Count, 2);
					AssertNotNull(importEmbeddedWizardMenuItem.MenuItems.FindByText("test mapping 1"));
					AssertNotNull(importEmbeddedWizardMenuItem.MenuItems.FindByText("test mapping 2"));

					mapping1.Delete();
					Factory.Save();

					importWizardMenuItem.OnPopup(EventArgs.Empty);
					importEmbeddedWizardMenuItem = importWizardMenuItem.MenuItems[0];

					AssertEquals(importEmbeddedWizardMenuItem.MenuItems.Count, 1);
					AssertNotNull(importEmbeddedWizardMenuItem.MenuItems.FindByText("test mapping 2"));
				}
			}
			finally
			{
				ObjectFactory.DisposeSubstitutions();
			}
		}

		public void TestAdvancedDataAutomationWizardMenuItem_ShouldNotMakeNetworkRequestBeforeADAWMenuIsExpanded()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();

			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (Env.SetTemporaryUserContext(EnsureTempStaff().PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var response = new HttpResponseMessage(HttpStatusCode.OK))
			using (response.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool> { { "IDummyBizo", false } })))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ReturnsAsync(response);

				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);

				using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				using (module.EmbeddedControl)
				{
					AssertNotNull(module.DataTransferMenuItem);
					module.DataTransferMenuItem.OnPopup(EventArgs.Empty);

					var importWizardMenuItem = module.DataTransferMenuItem.MenuItems.FindByName("GlowIntegrationMenuItem");
					AssertNotNull(importWizardMenuItem);
					clientFactoryMock.Verify(c => c.Create(It.IsAny<Uri>()), Times.Never());

					importWizardMenuItem.OnPopup(EventArgs.Empty);
					clientFactoryMock.Verify(c => c.Create(It.IsAny<Uri>()), Times.Once());
				}
			}
		}

		public void TestAdvancedDataAutomationWizardMenuItem_ShouldNotMakeNetworkRequestWhenPopupIsTriggeredByShortcut()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();

			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (Env.SetTemporaryUserContext(EnsureTempStaff().PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var response = new HttpResponseMessage(HttpStatusCode.OK))
			using (response.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool> { { "IDummyBizo", false } })))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ReturnsAsync(response);

				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);

				using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				using (module.EmbeddedControl)
				{
					AssertNotNull(module.DataTransferMenuItem);
					module.DataTransferMenuItem.OnPopup(EventArgs.Empty);

					var importWizardMenuItem = module.DataTransferMenuItem.MenuItems.FindByName("GlowIntegrationMenuItem");
					AssertNotNull(importWizardMenuItem);
					clientFactoryMock.Verify(c => c.Create(It.IsAny<Uri>()), Times.Never());

					KeySender.SendKeyDownToProcessCmdKey(module.Grid, (int)(Keys.Control | Keys.Shift));
					Application.DoEvents();
					clientFactoryMock.Verify(c => c.Create(It.IsAny<Uri>()), Times.Never());
				}
			}
		}

		public void TestToolStripResourceString()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var popup = new EmbeddedModulePopup(module))
			{
				var tmp = popup.FindToolBarButtonByText("Edit") as ZToolStripButton;
				AssertNotNull("Edit button does not exist", tmp);
				AssertNotNull("ResourceString does not exist", tmp.CaptionResourceString);
				AssertNotNullOrEmpty("ResourceString has no Full Description", tmp.CaptionResourceString.FullDescription);
			}
		}

		public void TestActivateDeactivate()
		{
			var dummy1 = Factory.New<DummyCancellable>();
			var dummy2 = Factory.New<DummyCancellable>();
			var dummy3 = Factory.New<DummyCancellable>();
			var dummy4 = Factory.New<DummyCancellable>();
			Factory.Save();

			dummy1.CanCancelMessage = null;
			dummy2.CanCancelMessage = null;
			dummy3.CanCancelMessage = null;
			dummy4.CanCancelMessage = null;
			using (var form = new ZChildForm())
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var collection = new DummyBusinessObjectCollection(Factory);
					form.Controls.Add(module.EmbeddedControl);

					module.GridCollection.Add(dummy1);
					module.GridCollection.Add(dummy2);
					module.GridCollection.Add(dummy3);
					module.GridCollection.Add(dummy4);

					form.Show();

					dummy1.IsCancelled = true;
					dummy2.IsCancelled = true;

					Assert(dummy1.IsCancelled);
					Assert(dummy2.IsCancelled);
					Assert(!dummy3.IsCancelled);
					Assert(!dummy4.IsCancelled);

					module.Grid.Select(1);
					module.Grid.Select(2);
					module.Activate_Exposed();

					Assert(dummy1.IsCancelled);
					Assert(!dummy2.IsCancelled);
					Assert(!dummy3.IsCancelled);
					Assert(!dummy4.IsCancelled);

					module.Grid.SelectAllElements();
					module.Activate_Exposed();

					Assert(!dummy1.IsCancelled);
					Assert(!dummy2.IsCancelled);
					Assert(!dummy3.IsCancelled);
					Assert(!dummy4.IsCancelled);

					module.Grid.UnSelect(0);
					module.Grid.UnSelect(1);
					module.DeActivate_Exposed();

					Assert(!dummy1.IsCancelled);
					Assert(!dummy2.IsCancelled);
					Assert(dummy3.IsCancelled);
					Assert(dummy4.IsCancelled);
				}
			}
		}

		public void TestActivateDeactivate_ClickingRowHeader()
		{
			var bizOCancelled = Factory.New<DummyCancellable>();
			bizOCancelled.IsCancelled = true;
			var bizOActive = Factory.New<DummyCancellable>();
			bizOActive.IsCancelled = false;
			Factory.Save();

			using (var form = new ZChildForm())
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var collection = new DummyBusinessObjectCollection(Factory);
					form.Controls.Add(module.EmbeddedControl);

					module.GridCollection.Add(bizOCancelled);
					module.GridCollection.Add(bizOActive);

					form.Show();

					var grid = module.DisplayGrid;
					var row1Rectangle = grid.GetRowNotificationRectangle(1);
					typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, row1Rectangle.X, row1Rectangle.Y, 0) });
					typeof(Control).InvokeMember("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, row1Rectangle.X, row1Rectangle.Y, 0) });
					Application.DoEvents();
					AssertEquals("Clicking RowHeader on non-cancelled bizO must show 'deactivate' deleteButtonText", "&Deactivate", module.DeleteButtonText);

					row1Rectangle = grid.GetRowNotificationRectangle(0);
					typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, row1Rectangle.X, row1Rectangle.Y, 0) });
					typeof(Control).InvokeMember("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, row1Rectangle.X, row1Rectangle.Y, 0) });
					Application.DoEvents();
					AssertEquals("Clicking RowHeader on active bizO must show 'activate' deleteButtonText", "&Activate", module.DeleteButtonText);
				}
			}
		}

		delegate void ActivateDeactivate(DummyFilterGridModule module);

		public void TestCannotActivate()
		{
			ActivateDeactivate activate = module => { module.Activate_Exposed(); };
			ActivateDeactivate deactivate = module => { module.DeActivate_Exposed(); };
			var dummy = Factory.New<DummyCancellable>();
			dummy.CanCancelMessage = "zzz";
			AssertActivateDeactivate(dummy, false, "zzz", deactivate);

			dummy.CanCancelMessage = null;
			AssertActivateDeactivate(dummy, false, "", deactivate);

			dummy.CanReactivateMessage = "123";
			AssertActivateDeactivate(dummy, true, "123", activate);

			dummy.CanReactivateMessage = null;
			AssertActivateDeactivate(dummy, true, "", activate);
		}

		void AssertActivateDeactivate(DummyCancellable dummy, bool isCancelled, string expectedMessage, ActivateDeactivate deleg)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			using (var form = new ZChildForm())
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var collection = new DummyBusinessObjectCollection(Factory);
					form.Controls.Add(module.EmbeddedControl);
					module.GridCollection.Add(dummy);
					form.Show();

					dummy.IsCancelled = isCancelled;
					module.Grid.Select(0);
					deleg.Invoke(module);
					if (!string.IsNullOrEmpty(expectedMessage))
					{
						AssertEquals("Should not change state", isCancelled, dummy.IsCancelled);
						AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertEquals("Should change state", !isCancelled, dummy.IsCancelled);
					}
				}
			}
		}

		public void TestInvalidColumnExceptionIsReported()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var query = new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.GreaterThan, ZDate.Today);
				query.AddToFilter(DummyDependentBizoSchema.ZD1_Number, ZInt.Zero);

				var collection = new DummyBusinessObjectCollection(Factory);

				try
				{
					module.LoadCollectionExposed(Factory, collection.TypeOfElements, query);
					Assert("LoadCollectionExposed should throw exception", false);
				}
				catch (Exception ex)
				{
					AssertEquals("SqlException", ex.GetType().Name);
					AssertEquals("Invalid column name 'ZD1_Number'.", ex.Message);
				}

				ErrorReporter.Clear();
			}
		}

		#region TestAllowSendEmailAction

		public void TestAllowSendEmailAction()
		{
			using (var module = new DummyFilterGridModuleWithTypeOfTopLevelBusinessTest(typeof(DummyBusinessObjectForAllowSendEmailActionTest)))
			{
				AssertEquals("AllowSendEmailAction should be true if TopLevelBusinessObject is ISendEmailSource", true, module.AllowSendEmailAction);
			}
		}

		class DummyBusinessObjectForAllowSendEmailActionTest : DummyBusinessObject, ISendEmailActionSource
		{
			public DummyBusinessObjectForAllowSendEmailActionTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		#endregion

		class TestIImportService : IImportService
		{
			public void GenerateAndSaveXSD(string tableName)
			{
				GenerateAndSaveXSDCallCount.Add(tableName);
			}

			public void Import()
			{
				ImportCallCount++;
			}

			internal List<string> GenerateAndSaveXSDCallCount = new List<string>();
			internal int ImportCallCount;

			public bool CanBeImported(string tableName)
			{
				return CanBeImportedDelegate == null || CanBeImportedDelegate(tableName);
			}

			internal CanBeImportedChecker CanBeImportedDelegate;

			internal delegate bool CanBeImportedChecker(string tableName);

			public int GenerateAndSaveAllXSDs(string directoryName)
			{
				throw new NotImplementedException("Not called by this code.");
			}
		}

		class TestIExportService : IExportService
		{
			public bool CanBeExported(Type type)
			{
				return CanBeExportedDelegate == null || CanBeExportedDelegate(type);
			}

			internal CanBeExportedChecker CanBeExportedDelegate;

			internal delegate bool CanBeExportedChecker(Type tableName);

			internal int ExportCallCount;

			public void Export(IEnumerable<IBusiness> businessObject)
			{
				ExportCallCount++;
			}

			public void Export(IEnumerable<IBusiness> businessObjects, Stream targetStream)
			{
				ExportCallCount++;
			}

			public void Export(IEnumerable<IBusiness> businessObjects, Stream targetStream, BusinessObjectFactory factory)
			{
				ExportCallCount++;
			}

			public void ExportWithSave(IEnumerable<IBusiness> businessObject)
			{
				ExportCallCount++;
			}

			public void ExportWithSave(IEnumerable<IBusiness> businessObject, Func<DataTable, DataRow> rowFilterOnMultiRowResult)
			{
				ExportCallCount++;
			}
		}

		public void TestDataTransferMenu()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new ZChildForm())
			{
				using (var testModule = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var importService = new TestIImportService();
					importService.CanBeImportedDelegate = (tableName) => { return tableName == "DummyBizo"; };
					testModule.SetNativeXmlImportServiceForTesting(importService);

					Assert(!importService.CanBeImported("Fail"));
					Assert(importService.CanBeImported("DummyBizo"));

					form.Controls.Add(testModule.EmbeddedControl);
					form.Text = "Data Transfer Test";
					form.Show();

					var menuItems = testModule.GetNewAdditionalMenuItemsExposed();
					var dataTransferMenuItem = menuItems.FindByText("Actions").MenuItems.FindByText("Data Transfer");
					AssertNotNull("Precondition: Data Transfer menu item should exist", dataTransferMenuItem);
					dataTransferMenuItem.OnPopup(new EventArgs());

					var dataTransferMenuItems = dataTransferMenuItem.MenuItems;
					var importXMLMenuItem = dataTransferMenuItems.FindByText("Import Native XML");
					var nativeXmlSchemasMenu = dataTransferMenuItems.FindByText("Native XML Schemas");

					AssertNotNull("Could not find Native XML Schemas Menu", nativeXmlSchemasMenu);

					var generateDummyBizoMenuItem = nativeXmlSchemasMenu.MenuItems.FindByText("Generate DummyBizo XSD");
					var generateAllMenuItem = nativeXmlSchemasMenu.MenuItems.FindByText("Generate All Native XSDs");

					AssertNotNull("Could not find Import Menu Item", importXMLMenuItem);
					AssertNotNull("Could not find Generate Table XSD Item", generateDummyBizoMenuItem);
					AssertNotNull("Could not find Generate All Native XSDs Item", generateAllMenuItem);

					importXMLMenuItem.PerformClick();
					AssertEquals("Calls to the import method", 1, importService.ImportCallCount);

					generateDummyBizoMenuItem.PerformClick();
					AssertEquals("Should have called GenerateAndSave once", 1, importService.GenerateAndSaveXSDCallCount.Count);
					AssertEquals("Should have called GenerateAndSave with the table name", "DummyBizo", importService.GenerateAndSaveXSDCallCount[0]);
					importService.GenerateAndSaveXSDCallCount.Clear();

					generateAllMenuItem.PerformClick();
					AssertEquals("Should have called GenerateAndSave once", 1, importService.GenerateAndSaveXSDCallCount.Count);
					AssertEquals("Should have called GenerateAndSave with a null", null, importService.GenerateAndSaveXSDCallCount[0]);
				}
			}
		}

		public void TestDataTransferMenu_ExportRows()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new ZChildForm())
			{
				using (var testModule = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var businessObject = testModule.GridCollection.AddNew();
					var exportService = new TestIExportService();
					exportService.CanBeExportedDelegate = (type) => { return type == testModule.GridCollection.TypeOfElements; };
					testModule.SetNativeXmlExportServiceForTesting(exportService);

					testModule.SelectedBusinessObjectsOverride = new BusinessObject[] { businessObject };

					form.Controls.Add(testModule.EmbeddedControl);
					form.Text = "Data Transfer Test";
					form.Show();

					var menuItems = testModule.GetNewAdditionalMenuItemsExposed();
					var dataTransferMenuItem = menuItems.FindByText("Actions").MenuItems.FindByText("Data Transfer");
					AssertNotNull("Precondition: Data Transfer menu item should exist", dataTransferMenuItem);
					dataTransferMenuItem.OnPopup(new EventArgs());

					var dataTransferMenuItems = dataTransferMenuItem.MenuItems;
					var exportXMLMenuItem = dataTransferMenuItems.FindByText("Export Native XML");

					exportXMLMenuItem.PerformClick();
					AssertEquals("Calls to the export method", 1, exportService.ExportCallCount);
				}
			}
		}

		public void TestAddNativeDataTransferMenuItem()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new ZChildForm())
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var importService = new TestIImportService();
				importService.CanBeImportedDelegate = (tableName) => { return tableName == "DummyBizo"; };
				module.SetNativeXmlImportServiceForTesting(importService);

				var exportService = new TestIExportService();
				exportService.CanBeExportedDelegate = (type) => { return type == module.GridCollection.TypeOfElements; };
				module.SetNativeXmlExportServiceForTesting(exportService);

				Assert(!importService.CanBeImported("Fail"));
				Assert(importService.CanBeImported("DummyBizo"));

				form.Controls.Add(module.EmbeddedControl);
				form.Text = "Data Transfer Test";
				form.Show();

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var dataTransferMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("D&ata Transfer");
				var dataTransferMenuItems = dataTransferMenuItem.MenuItems;
				AssertAddNativeDataTransferMenuItems(module, dataTransferMenuItems);

				var emptyMenuItems = new Menu.MenuItemCollection(dataTransferMenuItem);
				AssertAddNativeDataTransferMenuItems(module, emptyMenuItems);
			}

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();

			using (var form = new ZChildForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Text = "Data Transfer Test";
				form.Show();

				var menuItems = module.FormActionMenu;
				var dataTransferMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("D&ata Transfer", true);
				var dataTransferMenuItems = dataTransferMenuItem.MenuItems;
				module.AddNativeDataTransferMenuItem(dataTransferMenuItems);
				var importNativeXmlMenuItem = dataTransferMenuItems.FindByText("Import Native XML");
				var exportNativeXmlMenuItem = dataTransferMenuItems.FindByText("Export Native XML");
				var nativeXmlSchemasMenuItem = dataTransferMenuItems.FindByText("Native XML Schemas");
				AssertNotNull("Import Native XML menu item should be added", importNativeXmlMenuItem);
				AssertNotNull("Export Native XML menu item should be added", exportNativeXmlMenuItem);
				AssertNotNull("Native XML Schemas menu item should be added", nativeXmlSchemasMenuItem);
				AssertEquals(typeof(ImportSecurityChecker), module.NativeXmlMenuItems["Export Native XML"].Target.GetType());
				AssertEquals(typeof(ImportSecurityChecker), module.NativeXmlMenuItems["Import Native XML"].Target.GetType());

				DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(-1).ToDateTime();

				var emptyMenuItems = new Menu.MenuItemCollection(new MainMenu());
				module.AddNativeDataTransferMenuItem(emptyMenuItems);
				AssertEquals(0, emptyMenuItems.Count);
			}
		}

		public void TestAddPrintAllCovertSheetsMenuItem()
		{
			using var form = new ZChildForm();
			using var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.RefDocType);
			form.Controls.Add(module.EmbeddedControl);
			form.Text = "Print All Cover Sheets Test";
			form.Show();

			var menuItems = module.FormActionMenu;
			var printAllCoverSheetsMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("Print All Cover Sheets", true);
			module.Grid.ListManager.AddNew();
			AssertNoExceptionThrown(printAllCoverSheetsMenuItem.PerformClick);
		}

		static void AssertAddNativeDataTransferMenuItems(DummyFilterGridModule module, Menu.MenuItemCollection dataTransferMenuItems)
		{
			module.AddNativeDataTransferMenuItem(dataTransferMenuItems);
			var importNativeXmlMenuItem = dataTransferMenuItems.FindByText("Import Native XML");
			var exportNativeXmlMenuItem = dataTransferMenuItems.FindByText("Export Native XML");
			var nativeXmlSchemasMenuItem = dataTransferMenuItems.FindByText("Native XML Schemas");
			AssertNotNull("Import Native XML menu item should be added", importNativeXmlMenuItem);
			AssertNotNull("Export Native XML menu item should be added", exportNativeXmlMenuItem);
			AssertNotNull("Native XML Schemas menu item should be added", nativeXmlSchemasMenuItem);
			AssertEquals(typeof(ImportSecurityChecker), module.NativeXmlMenuItems["Export Native XML"].Target.GetType());
			AssertEquals(typeof(ImportSecurityChecker), module.NativeXmlMenuItems["Import Native XML"].Target.GetType());

			var separator = dataTransferMenuItems.FindByText("-");
			if (separator != null)
			{
				var separatorIndex = dataTransferMenuItems.IndexOf(separator);
				AssertNotNull("Should have a menu item after the separator", dataTransferMenuItems[separatorIndex + 1]);
				var importNativeIndex = dataTransferMenuItems.IndexOf(importNativeXmlMenuItem);
				var exportNativeIndex = dataTransferMenuItems.IndexOf(exportNativeXmlMenuItem);
				var nativeSchemasIndex = dataTransferMenuItems.IndexOf(nativeXmlSchemasMenuItem);
				Assert("Native Data Transfer menu items should come before the separator", importNativeIndex < separatorIndex && nativeSchemasIndex < separatorIndex && exportNativeIndex < separatorIndex);
			}
			else
			{
				AssertEquals("Should only have the Import Native XML & Native XML Schemas menu items", 2, dataTransferMenuItems.Count);
			}
		}

		public void TestSeparatorWhenNoExportOrImport()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var form = new ZChildForm())
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Text = "Data Transfer Test";
				form.Show();

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var dataTransferMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("D&ata Transfer");
				var dataTransferMenuItems = dataTransferMenuItem.MenuItems;
				module.AddNativeDataTransferMenuItem(dataTransferMenuItems);

				var separator = dataTransferMenuItems.FindByText("-");

				AssertNull("Separator should not be required", separator);
				var importNativeXmlMenuItem = dataTransferMenuItems.FindByText("Import Native XML");
				var exportNativeXmlMenuItem = dataTransferMenuItems.FindByText("Export Native XML");
				var nativeXmlSchemasMenuItem = dataTransferMenuItems.FindByText("Native XML Schemas");
				AssertNull("Import Native XML menu item shouldn't be added", importNativeXmlMenuItem);
				AssertNull("Export Native XML menu item shouldn't be added", exportNativeXmlMenuItem);
				AssertNull("Native XML Schemas menu item shouldn't be added", nativeXmlSchemasMenuItem);
			}
		}

		public void TestAddNativeDataTransferMenuItem_GenerateRefExchangeRateShouldPresentInCurrenciesModule()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new ZChildForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.RefCurrency))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Text = "Data Transfer Test";
				form.Show();

				var menuItems = module.FormActionMenu;
				var dataTransferMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("D&ata Transfer", true);
				var dataTransferMenuItems = dataTransferMenuItem.MenuItems;
				module.AddNativeDataTransferMenuItem(dataTransferMenuItems);
				var importNativeXmlMenuItem = dataTransferMenuItems.FindByText("Import Native XML");
				AssertNotNull("Import Native XML menu item should be added", importNativeXmlMenuItem);
				var nativeXmlSchemasMenuItem = dataTransferMenuItems.FindByText("Native XML Schemas");
				AssertNotNull("Native XML Schemas menu item should be added", nativeXmlSchemasMenuItem);

				var nativeXmlSchemasMenuItems = nativeXmlSchemasMenuItem.MenuItems;
				var generateRefExchangeRateXSD = nativeXmlSchemasMenuItems.FindByText("Generate RefExchangeRate XSD");
				AssertNotNull("Generate RefExchangeRate XSD menu item should be added", generateRefExchangeRateXSD);
				var generateAllNativeXSDs = nativeXmlSchemasMenuItems.FindByText("Generate All Native XSDs");
				AssertNotNull("Generate All Native XSDs menu item should be added", generateAllNativeXSDs);

				using (var tempFileDirectory = new TempDirectory())
				{
					ZFormModaliser.PathToSelectInShowCommonDialog = tempFileDirectory.DirectoryName;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					generateRefExchangeRateXSD.PerformClick();
					var zipPath = Path.Combine(tempFileDirectory.DirectoryName, "NativeCurrencyExchangeRate.zip");
					AssertEquals("File NativeDummy.zip Exists", true, File.Exists(zipPath));
					ZipCompression.Unzip(zipPath, tempFileDirectory.DirectoryName);
					Assert("NativeCurrencyExchangeRate XSD should be created", File.Exists(tempFileDirectory.DirectoryName + "\\NativeCurrencyExchangeRate.xsd"));
				}
			}
		}

		public void TestUniversalDataTransferMenuItems()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var menuItem1 = new ZMenuItem("test additional menu item 1");
				var menuItem2 = new ZMenuItem("test additional menu item 2");

				module.UniversalDataTransferMenuItemsForTest = new[] { menuItem1, menuItem2 };

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var dataTransferMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("D&ata Transfer");
				AssertNotNull("precondition - Data Transfer menu item should exist", dataTransferMenuItem);

				var dataTransferMenuItems = dataTransferMenuItem.MenuItems;

				AssertNotNull("additional menu item 1 added", dataTransferMenuItems.FindByText("test additional menu item 1"));
				AssertNotNull("additional menu item 2 added", dataTransferMenuItems.FindByText("test additional menu item 2"));
			}
		}

		public void TestCheckCopySelectedRowsAllowed()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.ExportSecurityCheckpoint_Exposed.IsAllowed = true;
				Assert(module.CheckCopySelectedRowsAllowed_Exposed());

				module.ExportSecurityCheckpoint_Exposed.IsAllowed = false;
				Assert(!module.CheckCopySelectedRowsAllowed_Exposed());
			}
		}

		public void TestNotificationTypeWhenAdditionalFilterNotMet()
		{
			var dummy11 = Factory.New<DummyBusinessObject>();
			var dummy21 = Factory.New<DummyBusinessObject>();
			Factory.Save();

			using (var form = new ZChildForm())
			{
				using (var testModule = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					ZString message = "DUMMY NOT MATCHED";
					var collection = new DummyBusinessObjectCollection(Factory);
					collection.SetOverrideNotificationTypeWhenAdditionalFilterNotMet(NotificationType.Warning);
					collection.SetOverrideNotificationWhenAdditionalFilterNotMet(message);
					((ILegacyBusinessObjectCollectionInternals)collection).SetOverriddenAdditionalFilter(new ZQuery(DummyBizoSchema.Z0_FK_Code, "Z!Z"));

					var moduleDecisionProvider = new Mock<IModuleDecisionProvider>();
					moduleDecisionProvider.Setup(x => x.ShouldIgnoreAdditionalFilter).Returns(true);
					moduleDecisionProvider.Setup(x => x.List).Returns(collection);

					IDummyFilterModule dummyFilterModule = testModule;
					dummyFilterModule.ModuleDecisionProvider = moduleDecisionProvider.Object;
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					AssertNoRowWarningContaining(dummy11, message);
					AssertNoRowWarningContaining(dummy21, message);
					((IFilterModuleInternalsForTesting)testModule).PerformSearch();

					var dummy1 = testModule.GridCollection.FindByPK(dummy11.PK);
					var dummy2 = testModule.GridCollection.FindByPK(dummy21.PK);
					AssertHasRowWarningContaining(dummy1, message);
					AssertHasRowWarningContaining(dummy2, message);
					AssertNoRowMessageErrorContaining(dummy1, message);
					AssertNoRowMessageErrorContaining(dummy2, message);
					AssertNoRowErrorContaining(dummy1, message);
					AssertNoRowErrorContaining(dummy2, message);

					collection.SetOverrideNotificationTypeWhenAdditionalFilterNotMet(NotificationType.MessageError);
					((IFilterModuleInternalsForTesting)testModule).PerformSearch();
					dummy1 = testModule.GridCollection.FindByPK(dummy11.PK);
					dummy2 = testModule.GridCollection.FindByPK(dummy21.PK);
					AssertNoRowWarningContaining(dummy1, message);
					AssertNoRowWarningContaining(dummy2, message);
					AssertHasRowMessageErrorContaining(dummy1, message);
					AssertHasRowMessageErrorContaining(dummy2, message);
					AssertNoRowErrorContaining(dummy1, message);
					AssertNoRowErrorContaining(dummy2, message);

					collection.SetOverrideNotificationTypeWhenAdditionalFilterNotMet(NotificationType.Error);
					((IFilterModuleInternalsForTesting)testModule).PerformSearch();
					dummy1 = testModule.GridCollection.FindByPK(dummy11.PK);
					dummy2 = testModule.GridCollection.FindByPK(dummy21.PK);
					AssertNoRowWarningContaining(dummy1, message);
					AssertNoRowWarningContaining(dummy2, message);
					AssertNoRowMessageErrorContaining(dummy1, message);
					AssertNoRowMessageErrorContaining(dummy2, message);
					AssertHasRowErrorContaining(dummy1, message);
					AssertHasRowErrorContaining(dummy2, message);
				}
			}
		}

		public void TestModuleSqlSearch_WhenEnableGlowIndexSearchUsageCollectorRegistry_ShouldGenerateEDI()
		{
			AssertModulePerformResult(true, true);
		}

		public void TestModuleIndexSearch_WhenEnableGlowIndexSearchUsageCollectorRegistry_ShouldGenerateEDI()
		{
			AssertModulePerformResult(false, true);
		}

		public void TestModuleSearch_WhenEnableGlowIndexSearchUsageCollectorRegistry_ShouldNotGenerateEDI()
		{
			AssertModulePerformResult(true, false);
		}

		void AssertModulePerformResult(bool isSqlSearch, bool enableRegistry)
		{
			var helper = new UsageCollectorTestHelper(new BusinessObjectFactory());

			using (new GlowIndexQueryEngineMock(registryMockSetupAction: (mock) =>
			{
				mock.Setup(e => e.IsGlowIndexSearchAllowedForModule(It.IsAny<string>())).Returns(true);
				mock.Setup(e => e.MaximumNumberOfModuleFiltersSearchResults).Returns(50);
				mock.Setup(r => r.IndexSearchUsageCollector).Returns(enableRegistry);
			}))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				module.FilterBusinessObject.SearchType = isSqlSearch ? SearchType.Sql : SearchType.Index;

				var filterName = isSqlSearch ? "Code" : "CODE";
				var sqlFilter = (ModuleTextFilter)module.FilterBusinessObject[filterName];
				sqlFilter.IsActive = true;

				module.PerformSearch();

				if (enableRegistry)
				{
					var properties = new List<(string name, object value)>()
					{
						("SearchType", isSqlSearch ? "ModuleSql" : "ModuleIndexSearch"),
						("ModuleID", "GlbStaff(Staff and Resources)"),
						("Filters", isSqlSearch ? "Code" : "CODE")
					};
					AssertEquals("Contains message.", true, helper.AssertUsageMessagesContains("SPF", properties));
				}
				else
				{
					AssertEquals("Not contain message.", true, helper.AssertUsageMessagesCount("SPF", 0));
				}
			}
		}

		public void TestGetFirstBizOInList()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				Factory.Save();

				AssertNull(module.GetFirstBizOInList());

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals(Dummy.PK, module.GetFirstBizOInList().PK);
			}
		}

		public void TestDataTransferToolbarButtonsAreCorrectlyShownForOtherLanguages()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new ZChildForm())
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Text = "Module Test";
					form.Show();

					var actionsToolBarButton = module.ToolBarButtons.FindByText("Actions");
					var menuItem = actionsToolBarButton.FindByText("D&ata Transfer");

					Assert("Import/Export Tooblar visible", menuItem.Visible);
				}
			}

			using (var cache = Res.UseMockData())
			{
				cache.SetResourceGetter((s) => new ResourceStringData("", "WHATEVER"));

				cache.Put("FilterStrip|ToolStrip|ButtonDataTransfer", new ResourceStringData("", "&CHSDATATRANSFER"));
				cache.Put("ModuleGrid.DataTransfer", new ResourceStringData("", "&CHSDATATRANSFER"));
				cache.Put("ModuleGrid.Actions", new ResourceStringData("", "&Actions"));

				using (var form = new ZChildForm())
				{
					using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
					{
						form.Controls.Add(module.EmbeddedControl);
						form.Text = "Module Test";
						form.Show();

						var dataTransferMenuItem = module.ToolBarButtons.FindByText("&Actions").FindByText("&CHSDATATRANSFER");
						Assert("Import/Export Tooblar visible as it is matched against resource text", dataTransferMenuItem.Visible);
					}
				}
			}
		}

		public void TestRowHeaderClickAndActivateLabel()
		{
			var dummy1 = Factory.New<ZFormTest.DummyCancellableWhichCanBeCancelled>();
			var dummy2 = Factory.New<ZFormTest.DummyCancellableWhichCanBeCancelled>();
			var dummy3 = Factory.New<ZFormTest.DummyCancellableWhichCanBeCancelled>();
			var dummy4 = Factory.New<ZFormTest.DummyCancellableWhichCanBeCancelled>();

			Factory.Save();

			using (var form = new ZChildForm())
			{
				using (var testModule = new DummyFilterGridModuleWithCancellableBizO())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Text = "An exciting module test!";
					form.Size = new System.Drawing.Size(300, 400);
					form.Show();

					AssertNotNull(testModule.Grid);
					((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();

					((ZFormTest.DummyCancellableWhichCanBeCancelled)testModule.Grid.List[0]).IsCancelled = true;
					((ZFormTest.DummyCancellableWhichCanBeCancelled)testModule.Grid.List[1]).IsCancelled = false;
					((ZFormTest.DummyCancellableWhichCanBeCancelled)testModule.Grid.List[2]).IsCancelled = false;
					((ZFormTest.DummyCancellableWhichCanBeCancelled)testModule.Grid.List[3]).IsCancelled = false;

					var x = 4;
					var y = 4;
					var isRowHeaderFound = false;

					while (y < testModule.Grid.Height)
					{
						if (testModule.Grid.HitTest(x, y).Type == DataGrid.HitTestType.RowHeader)
						{
							isRowHeaderFound = true;
							break;
						}

						y++;
					}

					AssertEquals("Should have found RowHeader", true, isRowHeaderFound);

					typeof(Control).GetMethod("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(testModule.Grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, x, y, 0) });
					AssertEquals("&Activate", testModule.DeleteButtonText);

					isRowHeaderFound = false;
					var isRowResizeFound = false;
					var oldY = y;

					while (y < testModule.Grid.Height)
					{
						if (testModule.Grid.HitTest(x, y).Type == DataGrid.HitTestType.RowResize)
						{
							isRowResizeFound = true;
						}
						else if (testModule.Grid.HitTest(x, y).Type == DataGrid.HitTestType.RowHeader && isRowResizeFound)
						{
							isRowHeaderFound = true;
							break;
						}

						y++;
					}

					AssertEquals("Should have found RowHeader", true, isRowHeaderFound);

					typeof(Control).GetMethod("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(testModule.Grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, x, y, 0) });
					AssertEquals("&Deactivate", testModule.DeleteButtonText);
					typeof(Control).GetMethod("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(testModule.Grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, x, oldY, 0) });
					AssertEquals("&Activate", testModule.DeleteButtonText);

					//Would like to test ctrl+row header clicks too, but not certain how to.
				}
			}
		}

		public void TestFilteredGridLoader_NoExtraDbHitsOnDispose()
		{
			for (var i = 0; i < 5; i++)
			{
				Factory.New<ZFormTest.DummyCancellableWhichCanBeCancelled>();
			}
			Factory.Save();

			IBusinessObjectCollection gridCollection;
			using (var form = new ZChildForm())
			using (var testModule = new DummyFilterGridModuleWithCancellableBizO())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				testModule.PerformSearch();

				gridCollection = testModule.GridCollection;
			}
			AssertEquals("FilteredGridLoader.Dispose (FilterGridLoader.SwapFactory to original) should not cause extra Db hits.", 0, gridCollection.Factory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDoubleClick()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy3 = Factory.New<DummyBusinessObject>();
			var dummy4 = Factory.New<DummyBusinessObject>();
			Factory.Save();

			using (var form = new ZChildForm())
			{
				using (var testModule = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Text = "An exciting module test!";
					form.Size = new System.Drawing.Size(300, 400);
					form.Show();

					AssertNotNull(testModule.Grid);
					((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();

					var x = 20;
					var y = 10;
					var isColumnResizeFound = false;

					while (x < testModule.Grid.Width)
					{
						if (testModule.Grid.HitTest(x, y).Type == DataGrid.HitTestType.ColumnResize)
						{
							isColumnResizeFound = true;
							break;
						}

						x++;
					}

					AssertEquals("Should have found ColumnResize", true, isColumnResizeFound);

					typeof(Control).GetMethod("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(testModule.Grid, new object[] { new MouseEventArgs(MouseButtons.Left, 2, x, y, 0) });
					// Actually, controller was accessed by ShowRecentItemsCore
					//AssertNull("Double click on ColumnHeader not counted as DefaultAction, Controller should be null as no action has taken place.", TestModule.LastController);
				}
			}
		}

		public void TestNoMenuItemInContextMenuIfNoSecurityRight()
		{
			using (var testModule = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				testModule.LimitedColumns = new ZLimitedColumnsProvider(DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);
				AssertEquals(0, testModule.ContextMenuExposed.Length);
				AssertEquals(0, testModule.FormActionMenu.Length);
			}
		}

		public void TestModuleHasActionsTrue()
		{
			AssertEquals("Default has actions exposed", true, DummyFilterGridModule.HasActionsExposed);
			using (var testModule = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				try
				{
					Assert("Has actions", testModule.FormActionMenu.Length > 0);
					Assert("Has actions", testModule.ToolBarButtons.Length > 0);
					Assert("Has actions", testModule.ContextMenuExposed.Length > 0);
				}
				finally
				{
					foreach (ZToolBarButton button in testModule.ToolBarButtons)
					{
						button.Dispose();
					}
				}
			}
		}

		public void TestModuleHasActionsFalse()
		{
			using (var form = new ZChildForm())
			{
				try
				{
					DummyFilterGridModule.HasActionsExposed = false;
					using (var testModule = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
					{
						AssertNull("No actions", testModule.FormActionMenu);
						AssertNull("No actions", testModule.ToolBarButtons);
						AssertNull("No actions", testModule.ContextMenuExposed);
					}
				}
				finally
				{
					DummyFilterGridModule.HasActionsExposed = true;
				}
			}
		}

		public void TestLoadWithBlobs()
		{
			using (var form = new ZChildForm())
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Text = "An exciting module test!";
				form.Size = new System.Drawing.Size(300, 400);
				form.Show();

				var oldColumnStyles = new ArrayList(module.Grid.ColumnStyles);
				try
				{
					module.Grid.ColumnStyles.Clear();
					module.Grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo { ColumnName = "Z0_Number" });
					AssertEquals("Filter should not load blobs for grid without blob columns.", 0,
						((IFilterGridModuleInternalsForTesting)module).GetDisplayResultsQuery().LoadWithBlobs.Count);

					((IFilterGridModuleInternalsForTesting)module).ClearGridBlobFields();

					module.Grid.ColumnStyles.Clear();
					module.Grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_VarCharMax" });
					AssertEquals("Filter should load blobs for grid with blob columns.", 1,
						((IFilterGridModuleInternalsForTesting)module).GetDisplayResultsQuery().LoadWithBlobs.Count);
				}
				finally
				{
					module.Grid.ColumnStyles.Clear();
					module.Grid.ColumnStyles.AddRange(oldColumnStyles);
				}
			}
		}

		public void TestGetActionsTopMenuItem_NullReferenceException()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var menuItemList = new List<MenuItem> { null };
				AssertNoExceptionThrown(() => module.GetActionsTopMenuItem(menuItemList.ToArray()));
			}
		}

		IGlbStaff EnsureTempStaff()
		{
			var tempStaff = Factory.New<IGlbStaff>();
			Factory.Save();
			return tempStaff;
		}

		#region Toolbar Buttons

		public void TestToolBarButtons()
		{
			SetAllowNew(true);
			using (var module = new ToolbarButtonsDisabledDummyFilterGridModule())
			{
				foreach (var toolBarButton in module.ToolBarButtons)
				{
					if (toolBarButton.Text == "Delete")
					{
						AssertEquals(true, toolBarButton.Enabled);
					}

					if (toolBarButton.Text == "New")
					{
						AssertEquals(false, toolBarButton.Enabled);
					}

					if (toolBarButton.Text == ZFilterGridModule.ActionsMenuItemText)
					{
						AssertNotNull(toolBarButton.DropDownMenu.MenuItems);
					}

					toolBarButton.Dispose();
				}
			}
		}

		public void TestToolBarButtonsWithAmpersands()
		{
			using (var module = new ToolbarButtonsWithAmpersandsFilterGridModule())
			{
				try
				{
					Assert(module.ToolBarButtons.Any(btn => btn.Text == "Test1"));
					Assert(module.ToolBarButtons.Any(btn => btn.Text == "You && Me"));
					Assert(module.ToolBarButtons.Any(btn => btn.Text == "Cat && Dog"));
					Assert(module.ToolBarButtons.Any(btn => btn.Text == "Test321(T)"));
				}
				finally
				{
					foreach (var toolBarButton in module.ToolBarButtons)
					{
						toolBarButton.Dispose();
					}
				}
			}
		}

		public void TestToolBarButtonsActions()
		{
			using (var module = new ToolbarButtonsWithoutActionsFilterGridModule())
			{
				AssertEquals(false, module.ToolBarButtons.Any((b) => b.Text == ZFilterGridModule.ActionsMenuItemText));
			}
		}

		protected class ToolbarButtonsDisabledDummyFilterGridModule : DummyFilterGridModule
		{
			public ToolbarButtonsDisabledDummyFilterGridModule()
				: base()
			{
			}

			protected override MenuItem[] GetNewStandardMenuItems()
			{
				var menuItemCollection = base.GetNewStandardMenuItems();
				menuItemCollection.FindByText("&Delete").Enabled = true;
				menuItemCollection.FindByText("&New").Enabled = false;

				return menuItemCollection;
			}
		}

		protected class ToolbarButtonsWithoutActionsFilterGridModule : DummyFilterGridModule
		{
			public ToolbarButtonsWithoutActionsFilterGridModule()
				: base()
			{
			}

			protected override MenuItem[] GetNewActionMenuItems()
			{
				return Array.Empty<MenuItem>();
			}
		}

		protected class ToolbarButtonsWithAmpersandsFilterGridModule : DummyFilterGridModule
		{
			public ToolbarButtonsWithAmpersandsFilterGridModule()
				: base()
			{
			}
			protected override MenuItem[] GetNewStandardMenuItems()
			{
				var menuItemCollection = new MenuItem[]
					{
						new ZMenuItem("&Test1"),
						new ZMenuItem("You && Me"),
						new ZMenuItem("&Cat && Dog"),
						new ZMenuItem("Test321(&T)"),
					};

				return menuItemCollection;
			}
		}

		#endregion

		#region Data Transfer

		public void TestToolBarImportExportMenuDefaults()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var form = new ZChildForm())
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Text = "Module Test";
					form.Show();
					var dataTransferMenuItem = module.ToolBarButtons.FindByText("Actions").FindByText("D&ata Transfer");
					Assert("Import/Export Tooblar visible", dataTransferMenuItem.Visible);
					AssertNotNull("Excel printing available by default", module.ExportMenuItemsForTest["Export All Columns To Excel"]);
					AssertEquals(2, dataTransferMenuItem.MenuItems.Count);

					var importExportFound = false;
					foreach (MenuItem item in module.ContextMenuExposed.First(s => s.Text == "&Actions").MenuItems)
					{
						var exportToExcelFound = false;
						if (item.Text == "D&ata Transfer")
						{
							importExportFound = true;
							foreach (MenuItem subItem in item.MenuItems)
							{
								if (subItem.Text == "Export All Columns To Excel")
								{
									exportToExcelFound = true;
									break;
								}
							}
							AssertEquals("Export to Excel action menu exists", true, exportToExcelFound);
							break;
						}
					}
					AssertEquals("Import / Export action menu exists", true, importExportFound);
				}
			}
		}

		public void TestToolBarImportExportMenuCustomisation()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var form = new ZChildForm())
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.AddImportDataItemForTest("Test Import 1", delegate
				{ });
				module.AddImportDataItemForTest("Test Import 2", delegate
				{ });
				module.AddExportDataItemForTest("Test Export 1", delegate
				{ });

				form.Controls.Add(module.EmbeddedControl);
				form.Text = "Module Test";
				form.Show();
				var actionsMenuItem = module.ToolBarButtons.FindByText("Actions");

				var dataTransferMenuItem = actionsMenuItem.FindByText("D&ata Transfer");
				AssertNotNull("Data Transfer Menu", dataTransferMenuItem);
				AssertEquals("Menu should have 6 items (5 items plus 1 separator)", 6, dataTransferMenuItem.MenuItems.Count);
				AssertEquals("Menu 1 is Test Import 1", "Import Test Import 1", dataTransferMenuItem.MenuItems[0].Text);
				AssertEquals("Menu 2 is Test Import 2", "Import Test Import 2", dataTransferMenuItem.MenuItems[1].Text);
				AssertEquals("Menu 3 is a separator", "-", dataTransferMenuItem.MenuItems[2].Text);
				AssertEquals("Menu 4 is Export All Columns To Excel", "Export All Columns To Excel", dataTransferMenuItem.MenuItems[3].Text);
				AssertEquals("Menu 5 is Export Visible Columns To Excel", "Export Visible Columns To Excel", dataTransferMenuItem.MenuItems[4].Text);
				AssertEquals("Menu 5 is Test Export 1", "Export Test Export 1", dataTransferMenuItem.MenuItems[5].Text);

				var importExportMenuItem = module.ContextMenuExposed
					.SelectMany(outerm => outerm.SelectRecursive(m => m.MenuItems.Cast<MenuItem>()))
					.FirstOrDefault(m => m.Text == "D&ata Transfer");

				AssertNotNull("Import / Export action menu exists", importExportMenuItem);
				AssertEquals("Menu 1 is Test Import 1", "Import Test Import 1", importExportMenuItem.MenuItems[0].Text);
				AssertEquals("Menu 2 is Test Import 2", "Import Test Import 2", importExportMenuItem.MenuItems[1].Text);
				AssertEquals("Menu 3 is separator", "-", importExportMenuItem.MenuItems[2].Text);
				AssertEquals("Menu 4 is Export All Columns To Excel", "Export All Columns To Excel", importExportMenuItem.MenuItems[3].Text);
				AssertEquals("Menu 5 is Export Visible Columns To Excel", "Export Visible Columns To Excel", importExportMenuItem.MenuItems[4].Text);
				AssertEquals("Menu 5 is Test Export 1", "Export Test Export 1", importExportMenuItem.MenuItems[5].Text);
			}
		}

		public void TestToolBarImportExportMenuNoExcelExport()
		{
			using (var form = new ZChildForm())
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					using (var findBox = new ZGuidFindBox())
					{
						var provider = new DummyModuleDecisionProvider(findBox);
						provider.ShouldAllowExcelImport = false;
						module.OverrideModuleDecisionProvider(provider);

						form.Controls.Add(module.EmbeddedControl);
						form.Text = "Module Test";
						form.Show();

						AssertNull("Import/Export Toolbar Button Not Visible", module.ToolBarButtons.FindByText("Import/Export"));

						MenuItem importExportMenuItem = null;
						foreach (var item in module.ContextMenuExposed)
						{
							if (item.Text == "&Import/Export")
							{
								importExportMenuItem = item;
								break;
							}
						}
						AssertNull("Import/Export Menu Not Visibile", importExportMenuItem);
					}
				}
			}
		}

		public void TestImportingChecksSuccessful()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var form = new ZChildForm())
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var eventRaised = false;
					EventHandler handler = delegate
					{ eventRaised = true; };

					var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
					eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

					module.AddImportDataItemForTest("Test Import", handler);
					form.Controls.Add(module.EmbeddedControl);
					form.Text = "Hey you!!!";
					form.Show();
					var dataTransferMenuItem = module.ToolBarButtons.FindByText("Actions").FindByText("D&ata Transfer");
					AssertNotNull("Data Transfer Menu", dataTransferMenuItem);
					var importExportMenu = dataTransferMenuItem.MenuItems;
					AssertEquals("PRE: Import/Export menu should have 4 items (3 items plus 1 separator)", 4, importExportMenu.Count);
					AssertEquals("PRE: Menu Item 1 is Test Import", "Import Test Import", importExportMenu[0].Text);
					Assert("PRE: Handler not called", !eventRaised);

					importExportMenu[0].PerformClick();
					Assert("Handler called", eventRaised);
					AssertEquals(true, string.IsNullOrEmpty(EnvProxy.Instance.Licence.InterfaceConnector.LastReasonForNotAllowing));
				}
			}
		}

		public void TestToolbarIconsInAllLanguages()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var expectedIconTypes = new IconTypes[] { IconTypes.ViewButtonRest, IconTypes.NewButtonRest, IconTypes.EditButtonRest, IconTypes.CopyButtonRest, IconTypes.DeleteButtonRest, IconTypes.ActionsButtonRest, IconTypes.CollapseButtonRest };
			CombineAssertions(delegate
			{
				foreach (var language in DataFile.GetAvailableLanguages())
				{
					SetAllowNew(true);
					using (Res.TemporarilySwitchLanguage(language))
					using (var form = new ZChildForm())
					using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
					{
						module.SetCanBeCopied(true);
						form.Controls.Add(module.EmbeddedControl);
						form.Text = "Module Test";
						form.Show();
						AssertContainsExactElementsInAnyOrder("Toolbar Icons are not correct in " + language, Array.ConvertAll(expectedIconTypes, item => Icons.GetImageIndex(item)), Array.ConvertAll(module.ToolBarButtons.ToArray(), item => item.ImageIndex));
					}
				}
			});
		}

		#endregion

		#region Menu Items

		public void TestGetNewStandardMenuItems()
		{
			SetAllowNew(true);
			var module = CreateDummyFilterModule() as DummyFilterGridModule;
			if (module != null)
			{
				using (module)
				{
					var menuItems = module.GetNewStandardMenuItemsExposed();

					AssertNotNull(menuItems.FindByText("&New"));

					module.SetCanBeCopied(true);
					module.SetCanBeReversed(true);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNotNull(menuItems.FindByText("&Copy"));
					AssertNotNull(menuItems.FindByText("&Copy").MenuItems.FindByText("&Reverse"));

					module.SetCanBeReversed(false);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNotNull(menuItems.FindByText("&Copy"));
					AssertNull(menuItems.FindByText("&Copy").MenuItems.FindByText("&Reverse"));

					module.SetCanBeCopied(false);
					module.SetCanBeReversed(true);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNull(menuItems.FindByText("&Copy"));

					module.SetCanBeReversed(false);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNull(menuItems.FindByText("&Copy"));

					module.SetAllowEdit(true);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNotNull(menuItems.FindByText("&Edit"));

					module.SetAllowEdit(false);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNull(menuItems.FindByText("&Edit"));

					module.SetAllowView(true);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNotNull(menuItems.FindByText("&View"));

					module.SetAllowView(false);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNull(menuItems.FindByText("&View"));

					module.SetAllowDelete(true);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNotNull(menuItems.FindByText("&Delete"));

					module.SetAllowDelete(false);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNull(menuItems.FindByText("&Delete"));

					module.SetAllowToggleFilterVisibilityMenuItem(true);
					menuItems = module.GetNewAdditionalMenuItemsExposed();
					AssertNotNull(menuItems.FindByText("Hide/Show Filters"));

					module.SetAllowToggleFilterVisibilityMenuItem(false);
					menuItems = module.GetNewAdditionalMenuItemsExposed();
					AssertNull(menuItems.FindByText("Hide/Show Filters"));
				}
			}
		}

		public void TestAllowAddCopyMenuItem()
		{
			var module = CreateDummyFilterModule() as DummyFilterGridModule;
			if (module != null)
			{
				using (module)
				{
					SetAllowNew(true);
					module.SetCanBeCopied(true);
					Assert(module.AllowAddCopyMenuItem);

					module.SetCanBeCopied(false);
					Assert(!module.AllowAddCopyMenuItem);

					SetAllowNew(false);
					module.SetCanBeCopied(true);
					Assert(!module.AllowAddCopyMenuItem);

					module.SetCanBeCopied(false);
					Assert(!module.AllowAddCopyMenuItem);
				}
			}
		}

		public void TestAddUniversalCopyMenuItems()
		{
			SetAllowNew(true);
			var module = CreateDummyFilterModule() as DummyFilterGridModule;
			if (module != null)
			{
				using (module)
				{
					var newMenuItems = module.AddUniversalCopyMenuItemsExposed();
					var newButton = newMenuItems.FindByText("&New");
					AssertEquals(2, newButton.MenuItems.Count);
				}
			}
		}

		public void TestGetNewAdditionalMenuItems_WithNoAdditionalActions()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var module = CreateDummyFilterModule() as DummyFilterGridModule;
			if (module != null)
			{
				using (module)
				{
					var menuItems = module.GetNewAdditionalMenuItemsExposed();
					AssertEquals(2, menuItems.Length);
					AssertEquals("&Actions", menuItems[0].Text);
					AssertEquals("Hide/Show Filters", menuItems[1].Text);
				}
			}
		}

		public void TestGetNewAdditionalMenuItems_WithAdditionalActions()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var module = CreateDummyFilterModule() as DummyFilterGridModule;
			if (module != null)
			{
				using (module)
				{
					module.ActionMenuItems.Add(new ZMenuItem("An Action"));
					var menuItems = module.GetNewAdditionalMenuItemsExposed();
					AssertEquals(2, menuItems.Length);
					AssertEquals("&Actions", menuItems[0].Text);
					AssertEquals("Hide/Show Filters", menuItems[1].Text);

					AssertEquals("Action menu item count", 3, menuItems[0].MenuItems.Count);
					AssertEquals("D&ata Transfer", menuItems[0].MenuItems[0].Text);
					AssertEquals("An Action", menuItems[0].MenuItems[2].Text);

					module.SetAllowDefaultActivateDeactivate(true);
					menuItems = module.GetNewAdditionalMenuItemsExposed();
					AssertNotNull(menuItems[0].MenuItems.FindByText("Activate"));
					AssertNotNull(menuItems[0].MenuItems.FindByText("Deactivate"));

					module.SetAllowDefaultActivateDeactivate(false);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNull(menuItems[0].MenuItems.FindByText("Activate"));
					AssertNull(menuItems[0].MenuItems.FindByText("Deactivate"));
				}
			}
		}

		public void TestPluginAddsMenuItems()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var module = (ZFilterGridModule)CreateDummyFilterModule())
			{
				module.Plugins.Add(DummyControllerIDs.Dummy1);
				module.Plugins.Add(DummyControllerIDs.Dummy2);
				var dummy = module.Grid; //so it's initialized

				var actionsMenu = module.FormActionMenu.FindByText("&Actions");

				AssertNotNull("Should have an Actions menu", actionsMenu);
				AssertEquals("Three menu items in Actions", 3, actionsMenu.MenuItems.Count);
				AssertMenuPath("Actions", actionsMenu, "D&ata Transfer");
				MenuAssertion.AssertHasMenu("Actions", module.FormActionMenu, "&Actions", "D&ata Transfer");
				AssertNotNull("PlaceholderForPluginsActionMenuItems was added", actionsMenu.MenuItems.FindByName("PlaceholderForPluginsActionMenuItems"));

				actionsMenu.OnPopup(EventArgs.Empty);
				AssertEquals("Three menu items in Actions", 3, actionsMenu.MenuItems.Count);
				AssertNull("PlaceholderForPluginsActionMenuItems was deleted", actionsMenu.MenuItems.FindByName("PlaceholderForPluginsActionMenuItems"));
				AssertMenuPath("Actions", actionsMenu, "Dummy");
				MenuAssertion.AssertHasMenu("Actions", module.FormActionMenu, "&Actions", "Dummy");
				var actionMenu = module.ActionsMenuItem;
				var dummyMenu1 = actionMenu.MenuItems[2];
				AssertEquals("Dummy menu item should be the third one", "Dummy", dummyMenu1.Text);

				actionsMenu.OnPopup(EventArgs.Empty);
				var dummyMenu2 = actionMenu.MenuItems[2];
				AssertEquals("Still three menu items in Actions", 3, actionsMenu.MenuItems.Count);
				AssertEquals("Dummy menu item should be the third one", "Dummy", dummyMenu2.Text);
				AssertNotEquals("Dummy menu item should be recreated", dummyMenu1, dummyMenu2);
			}
		}

		public void TestPluginAddsMenuItems_OnlyOneMenuItemExceptPluginsMenuItems()
		{
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var module = (ZFilterGridModule)CreateDummyFilterModule())
			{
				(module as DummyFilterGridModule).SetAllowExcelExport(false);
				module.Plugins.Add(DummyControllerIDs.Dummy1);
				module.Plugins.Add(DummyControllerIDs.Dummy2);
				var dummy = module.Grid; //so it's initialized

				var actionsMenu = module.FormActionMenu.FindByText("&Actions");

				AssertNotNull("Should have an Actions menu", actionsMenu);
				AssertEquals("Two menu items in Actions", 2, actionsMenu.MenuItems.Count);
				AssertNotNull("PlaceholderForPluginsActionMenuItems was added", actionsMenu.MenuItems.FindByName("PlaceholderForPluginsActionMenuItems"));

				actionsMenu.OnPopup(EventArgs.Empty);
				AssertEquals("Two menu items in Actions", 2, actionsMenu.MenuItems.Count);
				AssertNull("PlaceholderForPluginsActionMenuItems was deleted", actionsMenu.MenuItems.FindByName("PlaceholderForPluginsActionMenuItems"));
				AssertMenuPath("Actions", actionsMenu, "Dummy");
				MenuAssertion.AssertHasMenu("Actions", module.FormActionMenu, "&Actions", "Dummy");
				var actionMenu = module.ActionsMenuItem;
				var dummyMenu1 = actionMenu.MenuItems[1];
				AssertEquals("Dummy menu item should be the second one", "Dummy", dummyMenu1.Text);

				actionsMenu.OnPopup(EventArgs.Empty);
				var dummyMenu2 = actionMenu.MenuItems[1];
				AssertEquals("Still two menu items in Actions", 2, actionsMenu.MenuItems.Count);
				AssertEquals("Dummy menu item should be the second one", "Dummy", dummyMenu2.Text);
				AssertNotEquals("Dummy menu item should be recreated", dummyMenu1, dummyMenu2);
			}
		}

		public void TestGetNewStandardMenuItems_Copy()
		{
			SetAllowNew(true);
			var module = CreateDummyFilterModule() as DummyFilterGridModule;
			if (module != null)
			{
				using (module)
				{
					var menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNotNull(menuItems.FindByText("&New"));

					module.SetCanBeCopied(true);
					module.SetCanBeReversed(true);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNotNull(menuItems.FindByText("&Copy"));
					AssertNotNull(menuItems.FindByText("&Copy").MenuItems.FindByText("&Reverse"));

					module.SetCanBeReversed(false);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNotNull(menuItems.FindByText("&Copy"));
					AssertNull(menuItems.FindByText("&Copy").MenuItems.FindByText("&Reverse"));

					module.SetCanBeCopied(false);
					module.SetCanBeReversed(true);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNull(menuItems.FindByText("&Copy"));

					module.SetCanBeReversed(false);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNull(menuItems.FindByText("&Copy"));

					SetAllowNew(false);
					module.SetCanBeCopied(true);
					menuItems = module.GetNewStandardMenuItemsExposed();
					AssertNull("Copy item only appears if AllowNew is true", menuItems.FindByText("&Copy"));
				}
			}
		}

		public void TestDeleteButtonShowsDeactivateTextWhenTypeHasPreventDeleteTrue()
		{
			using (var form = new ZChildForm())
			{
				using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.JobShipment))
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Text = "Module Test";
					form.Show();

					AssertEquals("&Deactivate", module.DeleteButtonText);
				}
			}
		}

		#endregion

		#region Recent Items

		public void TestModuleWithRecentItems()
		{
			using (var form = new Form())
			using (var module = new DummyFilterGridModule())
			{
				form.Controls.Add(module.EmbeddedControl);

				var recentItemsPanel = form.Controls.Find("RecentItemsPanel", false);
				AssertEquals("Recent Panel added to parent form", 1, recentItemsPanel.Length);
			}
		}

		public void TestModuleWithNoRecentItems()
		{
			using (var form = new Form())
			using (var module = new DummyFilterGridModuleWithNoRecentItems())
			{
				form.Controls.Add(module.EmbeddedControl);

				var recentItemsPanel = form.Controls.Find("RecentItemsPanel", false);
				AssertEquals("Recent Panel not added to parent form", 0, recentItemsPanel.Length);
			}
		}

		public void TestModuleWithRecentModuleID()
		{
			using (var form = new Form())
			using (var module = new DummyFilterGridModuleWithRecentModuleID())
			{
				AssertNotEquals("Module ID not the same as Recent Items Module ID", module.ID.Name, module.RecentItemsModuleID.Name);
			}
		}

		#endregion

		#region Data Import Wizard

		public void TestRunImportDataWizard()
		{
			using (var module = new DummyFilterGridModuleWithDataImportWizard())
			{
				var menuItem = module.ImportMenuItemsForTest["&Import By Data Wizard"];
				AssertNotNull("Import from Data Import Wizard available", menuItem);

				module.RunImportDataWizard_Exposed();
				AssertNotNull(module.lastShownDataImportWizardFormForTesting);

				var info = ((ImportWizard)module.lastShownDataImportWizardFormForTesting.BusinessEntity).CollectionInfo;
				AssertEquals(0, info.Collection.Count);
				module.lastShownDataImportWizardFormForTesting.Close();
				module.lastShownDataImportWizardFormForTesting.Dispose();
			}
		}

		#endregion

		#region Template Records

		public void TestAddNewTemplateRecordMenuItem()
		{
			SetAllowNew(true);

			using (var module = new DummyFilterGridModuleWithTemplates())
			{
				var menuItems = module.FormActionMenu;
				var newMenuItem = menuItems.FindByText("&New");
				AssertNull(newMenuItem.MenuItems.FindByText("New Template Record"));
			}

			using (var module = new DummyFilterGridModuleWithTemplates())
			{
				module.SupportTemplateRecordsForTest = true;
				module.AllowLoadTemplateRecords = true;

				var menuItems = module.FormActionMenu;
				var newMenuItem = menuItems.FindByText("&New");
				AssertNotNull(newMenuItem.MenuItems.FindByText("New Template Record"));
			}
		}

		public void TestLoadFromTemplateRecordPkReturnsSameInstance()
		{
			using (var module = new DummyFilterGridModuleWithTemplates())
			{
				module.SupportTemplateRecordsForTest = true;
				module.AllowLoadTemplateRecords = true;

				var pk1 = ZGuid.NewZGuid();
				var pk2 = ZGuid.NewZGuid();

				var record1 = module.LoadFromTemplateRecordPk(Factory, pk1);
				var record2 = module.LoadFromTemplateRecordPk(Factory, pk1);

				AssertSame("Should return same bizo for same pk", record1, record2);

				var record3 = module.LoadFromTemplateRecordPk(Factory, pk2);

				AssertNotEquals("Should create nem bizo for different pk", record1.PK, record3.PK);

				var record4 = module.LoadFromTemplateRecordPk(new BusinessObjectFactory(), pk1);

				AssertNotEquals("Should create nem bizo for different factory", record1.PK, record4.PK);
			}
		}

		public void TestIsTemplateRecord()
		{
			AssertEquals("IsTemplateRecord should return false when bizo is null.", false, ZFilterGridModule.IsTemplateRecord(null));

			var templateProviderBizo = Factory.New<DummyTemplateRecordProvider>();
			templateProviderBizo.IsTemplateRecord = true;
			Assert("IsTemplateRecord should return true when bizo is template record provider.", ZFilterGridModule.IsTemplateRecord(templateProviderBizo));

			templateProviderBizo.IsTemplateRecord = false;
			AssertEquals(@"IsTemplateRecord should return false when ""template record provider"".IsTemplateRecord is false.", false, ZFilterGridModule.IsTemplateRecord(null));

			var templateBizo = Factory.New<DummyTemplateRecord>();
			Assert("IsTemplateRecord should return true when bizo is itself a template record.", ZFilterGridModule.IsTemplateRecord(templateBizo));
		}

		public void TestDeleteButtonInfoWhenSelectTemplateRecord()
		{
			var templateRecordDeactive = Factory.New<DummyTemplateRecord>();
			templateRecordDeactive.IsCancelled = true;
			var templateRecordActive = Factory.New<DummyTemplateRecord>();
			templateRecordActive.IsCancelled = false;
			Factory.Save();

			using (var form = new ZChildForm())
			using (var module = new DummyFilterGridModuleWithTemplates())
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Add(templateRecordDeactive);
				module.GridCollection.Add(templateRecordActive);
				form.Show();

				var grid = module.DisplayGrid;
				var rowRectangle = grid.GetRowNotificationRectangle(1);

				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, rowRectangle.X, rowRectangle.Y, 0) });
				typeof(Control).InvokeMember("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, rowRectangle.X, rowRectangle.Y, 0) });
				Application.DoEvents();
				AssertEquals("&Deactivate", module.DeleteButtonText);
				AssertEquals(IconTypes.ClearButtonActive, module.DeleteButtonImage);
				AssertEquals(IconTypes.ClearButtonActive, module.DeleteButtonImageActive);
				AssertEquals("Deactivates the selected item after viewing its details read-only (shortcut Del)", module.DeleteButtonToolTipText);

				rowRectangle = grid.GetRowNotificationRectangle(0);
				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, rowRectangle.X, rowRectangle.Y, 0) });
				typeof(Control).InvokeMember("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, rowRectangle.X, rowRectangle.Y, 0) });
				Application.DoEvents();
				AssertEquals("&Activate", module.DeleteButtonText);
				AssertEquals(IconTypes.BlackWhite_Tick, module.DeleteButtonImage);
				AssertEquals(IconTypes.BlackWhite_Tick, module.DeleteButtonImageActive);
				AssertEquals("Activates the selected item", module.DeleteButtonToolTipText);
			}
		}

		#endregion

		#region Indexing Search

		public void TestIndexingSearch()
		{
			IndexSearchFilterHelper.ResetIndexQueryEngine();
			var staff = (BusinessObject)Factory.New<IGlbStaff>();
			staff.FillWithValidTestData();

			var staffNoInclude = (BusinessObject)Factory.New<IGlbStaff>();
			staffNoInclude.FillWithValidTestData();

			Factory.Save();

			using (var mocker = new GlowIndexQueryEngineMock(GetQueryResult(staff as IGlbStaff)))
			using (var form = new ZChildForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var filter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["CODE"];
				AssertNotNull(filter);

				filter.IsActive = true;
				filter.Property = "HAY";
				filter.ComparisonOperator = "starts with";

				var query = module.FilterBusinessObject.DoGlowQuery();
				AssertContains(staff.PK.ToSqlGuid(), query.LiteralTextADO, ignoreCase: true);
				AssertNotContains(staffNoInclude.PK.ToSqlGuid(), query.LiteralTextADO, ignoreCase: true);

				module.PerformSearch();

				AssertEquals(1, module.GridCollection.Count);

				var result = module.GridCollection[0] as BusinessObject;
				AssertEquals(staff.PK, result.PK);
				Assert(!module.GridCollection.Contains(staffNoInclude));
			}
		}

		public void TestIndexingSearch_RespectsReturnNoResultsQueryFlag()
		{
			IndexSearchFilterHelper.ResetIndexQueryEngine();
			var staff = (BusinessObject)Factory.New<IGlbStaff>();
			staff.FillWithValidTestData();

			var staffNoInclude = (BusinessObject)Factory.New<IGlbStaff>();
			staffNoInclude.FillWithValidTestData();

			Factory.Save();

			using (var mocker = new GlowIndexQueryEngineMock())
			using (var form = new ZChildForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				mocker.Results.Results.Add(new GlowIndexQueryResult(staff.PK.ToString(), "IGlbStaff"));
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var filter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["CODE"];
				AssertNotNull(filter);

				filter.IsActive = true;
				filter.Property = "HAY";
				filter.ComparisonOperator = "starts with";

				var query = module.FilterBusinessObject.DoGlowQuery();
				AssertContains(staff.PK.ToSqlGuid(), query.LiteralTextADO, ignoreCase: true);
				AssertNotContains(staffNoInclude.PK.ToSqlGuid(), query.LiteralTextADO, ignoreCase: true);

				module.PerformSearch();

				AssertEquals(1, module.GridCollection.Count);

				var result = module.GridCollection[0] as BusinessObject;
				AssertEquals(staff.PK, result.PK);
				Assert(!module.GridCollection.Contains(staffNoInclude));

				module.FilterBusinessObject.ReturnNoResultsQuery = true;
				module.PerformSearch();
				AssertEquals(0, module.GridCollection.Count);
			}
		}

		public void TestIndexingSearchWithError()
		{
			IndexSearchFilterHelper.ResetIndexQueryEngine();

			var staff = Factory.New<IGlbStaff>();
			var result = GetQueryResult(staff);
			result.Status = GlowIndexQueryStatus.BadRequest;
			result.ErrorMessage = "ErrorMessage";

			using (new GlowIndexQueryEngineMock(result))
			using (var form = new ZChildForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var filter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["CODE"];
				AssertNotNull(filter);

				filter.IsActive = true;
				filter.Property = "HAY";
				filter.ComparisonOperator = "starts with";

				module.PerformSearch();

				AssertEquals("ErrorMessage", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestIndexSearchWithNoQuery()
		{
			var fields = GetTestSearchFields();
			var field = SearchField.Create(fieldName: "DateTime", description: "DateTime", dataType: typeof(DateTime));
			fields = new SearchFieldCollection(fields.EntityType, new SearchField[] { field }.Concat(fields.Value).ToArray());

			using (new GlowIndexQueryEngineMock(mock =>
			{
				_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(fields);
				_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject", "IGlbStaff" });
			}))
			using (var form = new ZChildForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var filter = (IndexSearchModuleDateFilter)module.FilterBusinessObject["DateTime"];
				AssertNotNull(filter);

				filter.IsActive = true;
				filter.PropertySearch = "Today";

				AssertNoExceptionThrown(() => module.PerformSearch());
			}
		}

		SearchFieldCollection GetTestSearchFields()
		{
			var field1 = SearchField.Create("CODE", "Code");
			var field2 = SearchField.Create("NAME", "Full Name");
			var ret = new SearchFieldCollection("IGlbStaff", new SearchField[] { field1, field2 });
			return ret;
		}
		GlowIndexQueryResultCollection GetQueryResult(IGlbStaff staff)
		{
			var ret = new GlowIndexQueryResultCollection();
			ret.Status = GlowIndexQueryStatus.Success;
			ret.Results = new GlowIndexQueryResult[]
			{
				new GlowIndexQueryResult(staff.PK.ToString(),"IGlbStaff")
			};
			return ret;
		}

		public void TestToggleIndexSearchButtonIsNullByDefault()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new ZForm())
			using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
			{
				AssertNull(module.ToggleIndexSearchFilterMenuItem);
			}
		}

		public void TestToggleIndexSearchButtonIsNullWhenNotHasFields()
		{
			using (new GlowIndexQueryEngineMock(mock => mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject", "IGlbStaff" })))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new ZForm())
			using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
			{
				AssertNull(module.ToggleIndexSearchFilterMenuItem);
			}
		}

		public void TestToggleIndexSearchButtonVisible_WhenHasGlowSearchFields()
		{
			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
			{
				AssertEquals(true, module.ToggleIndexSearchFilterMenuItem.Visible);
			}
		}

		public void TestIndexSearchToggleButtonText()
		{
			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
			{
				AssertEquals("Index Search", module.ToggleIndexSearchFilterMenuItem.Text);
				AssertEquals("Index Search", module.ToggleIndexSearchFilterMenuItem.CaptionResourceString.Caption);
				AssertEquals(@"Switch Between Index Search Filter and SQL Filter.", module.ToggleIndexSearchFilterMenuItem.CaptionResourceString.FullDescription);
			}
		}

		public void TestIndexSearchToggleButtonTextUnverifiedModule()
		{
			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
			{
				AssertEquals("Index Search (Beta)", module.ToggleIndexSearchFilterMenuItem.Text);
				AssertEquals("Index Search (Beta)", module.ToggleIndexSearchFilterMenuItem.CaptionResourceString.Caption);
				AssertEquals(@"Switch Between Index Search Filter and SQL Filter.
Beta:
When enabling a module that is still in Beta, module searches may have limited functionality.
Please report any issues that are encountered.", module.ToggleIndexSearchFilterMenuItem.CaptionResourceString.FullDescription);
			}
		}

		public void TestToggleIndexSearchButton()
		{
			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			using (var form = new ZForm())
			using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertEquals(true, filterControl.FilterBusinessObject.HasIndexSearchFields);

				AssertEquals(true, filterControl.FilterBusinessObject.All(f => f is IIndexSearchModuleFilter));
				AssertNotNull(filterControl.FilterBusinessObject["NAME"]);

				var menuItem = module.ToggleIndexSearchFilterMenuItem;
				menuItem.PerformClick();

				AssertEquals(true, filterControl.FilterBusinessObject.All(f => !(f is IIndexSearchModuleFilter)));

				menuItem.PerformClick();
				AssertEquals(true, filterControl.FilterBusinessObject.All(f => f is IIndexSearchModuleFilter));
			}
		}

		public void TestToggleIndexSearchButton_ShouldKeepInitalCode()
		{
			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new ZForm())
			using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
			{
				form.Controls.Add(filterControl);
				form.Show();

				module.SetInitialCodeForSearch("YEA");

				var menuItem = module.ToggleIndexSearchFilterMenuItem;
				menuItem.PerformClick();

				var filterBO = filterControl.FilterBusinessObject;
				AssertEquals("Code set", "YEA", ((ModuleTextFilter)filterBO["Z0_Code"]).Property);

				menuItem.PerformClick();
				var commonFilter = (IndexSearchModuleTextFilter)filterBO["Common"];
				AssertEquals("Common set", "YEA", commonFilter.Property);
				AssertEquals(FilterVisibility.AlwaysVisible, commonFilter.Visibility);
			}
		}

		public void TestToggleIndexSearchButton_ShouldKeepExternalDefaults()
		{
			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var findBox = new ZGuidFindBox())
			{
				var provider = new DummyModuleDecisionProvider(findBox);
				provider.SetShouldLoadFilterBizObj(false);
				module.OverrideModuleDecisionProvider(provider);

				using (var form = new ZForm())
				using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
				{
					form.Controls.Add(filterControl);
					form.Show();

					var defaultsProvider = module.GridCollection as IFilterBusinessObjectDefaultsProvider;
					defaultsProvider.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Z0_Description", "Property", new ZString("TestSQL"), SearchType.Sql));
					defaultsProvider.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("NAME", "Property", new ZString("TestIndex"), SearchType.Index));

					var menuItem = module.ToggleIndexSearchFilterMenuItem;
					menuItem.PerformClick();

					var filterBO = filterControl.FilterBusinessObject;
					AssertEquals("External Defaults set", "TestSQL", ((ModuleTextFilter)filterBO["Z0_Description"]).Property);

					menuItem.PerformClick();
					AssertEquals("External Defaults set", "TestIndex", ((IndexSearchModuleTextFilter)filterBO["NAME"]).Property);
				}
			}
		}

		public void TestSetDefaultSearchTypeByExternalDefaultsInsteadOfLastUsedLayoutInSomeCases()
		{
			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var findBox = new ZGuidFindBox())
			{
				var provider = new DummyModuleDecisionProvider(findBox);
				provider.SetShouldLoadFilterBizObj(true);
				using (var form = new ZForm())
				using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
				{
					form.Controls.Add(filterControl);
					form.Show();

					var filterBO = filterControl.FilterBusinessObject;
					var result = new FilterBusinessObjectDefaults();
					result.Add(new FilterBusinessObjectDefault("Z0_Description", "Property", new ZString("TestSQL"), SearchType.Sql));

					AssertEquals(SearchType.Index, filterBO.SearchType);
					module.FilterBusinessObject.SetExternalDefaults(result);
					var lastUsedLayout = Factory.New<StmModuleFilter>();
					lastUsedLayout.S9_IsIndexSearch = true;
					filterBO.LastUsedLayout = lastUsedLayout;
					filterControl.FilterStripLoaded();

					AssertEquals(SearchType.Sql, filterBO.SearchType);
				}
			}

			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var findBox = new ZGuidFindBox())
			{
				var provider = new DummyModuleDecisionProvider(findBox);
				provider.SetShouldLoadFilterBizObj(true);
				using (var form = new ZForm())
				using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
				{
					form.Controls.Add(filterControl);
					form.Show();
					module.FilterBusinessObject.SearchType = SearchType.Sql;

					var filterBO = filterControl.FilterBusinessObject;
					var result = new FilterBusinessObjectDefaults();
					result.Add(new FilterBusinessObjectDefault("NAME", "Property", new ZString("TestIndex"), SearchType.Index));

					AssertEquals(SearchType.Sql, filterBO.SearchType);
					module.FilterBusinessObject.SetExternalDefaults(result);
					var lastUsedLayout = Factory.New<StmModuleFilter>();
					lastUsedLayout.S9_IsIndexSearch = false;
					filterBO.LastUsedLayout = lastUsedLayout;
					filterControl.FilterStripLoaded();

					AssertEquals(SearchType.Index, filterBO.SearchType);
				}
			}
		}

		public void TestToggleIndexSearchButton_ShouldKeepExternalDefaults_WhenUseSetExternalDefaultsMethod()
		{
			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var findBox = new ZGuidFindBox())
			{
				var provider = new DummyModuleDecisionProvider(findBox);
				provider.SetShouldLoadFilterBizObj(true);
				using (var form = new ZForm())
				using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
				{
					form.Controls.Add(filterControl);
					form.Show();

					var filterBO = filterControl.FilterBusinessObject;
					var result = new FilterBusinessObjectDefaults();
					result.Add(new FilterBusinessObjectDefault("Z0_Description", "Property", new ZString("TestSQL"), SearchType.Sql));

					AssertEquals(SearchType.Index, filterBO.SearchType);
					module.FilterBusinessObject.SetExternalDefaults(result);
					module.FilterBusinessObject.LoadLayout(null, true);

					AssertEquals(SearchType.Sql, filterBO.SearchType);
					AssertEquals("External Defaults set", "TestSQL", ((ModuleTextFilter)filterBO["Z0_Description"]).Property);

					var menuItem = module.ToggleIndexSearchFilterMenuItem;
					menuItem.PerformClick();
					AssertEquals(SearchType.Index, filterBO.SearchType);

					menuItem.PerformClick();
					AssertEquals(SearchType.Sql, filterBO.SearchType);
					AssertEquals("External Defaults set", "TestSQL", ((ModuleTextFilter)filterBO["Z0_Description"]).Property);
				}
			}

			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var findBox = new ZGuidFindBox())
			{
				var provider = new DummyModuleDecisionProvider(findBox);
				provider.SetShouldLoadFilterBizObj(true);
				using (var form = new ZForm())
				using (var filterControl = new ZFilterStripBaseControlForTest(module.Grid, module.FilterBusinessObject))
				{
					form.Controls.Add(filterControl);
					form.Show();
					module.FilterBusinessObject.SearchType = SearchType.Sql;

					var filterBO = filterControl.FilterBusinessObject;
					var result = new FilterBusinessObjectDefaults();
					result.Add(new FilterBusinessObjectDefault("NAME", "Property", new ZString("TestIndex"), SearchType.Index));

					AssertEquals(SearchType.Sql, filterBO.SearchType);
					module.FilterBusinessObject.SetExternalDefaults(result);
					module.FilterBusinessObject.LoadLayout(null, true);

					AssertEquals(SearchType.Index, filterBO.SearchType);
					AssertEquals("External Defaults set", "TestIndex", ((IndexSearchModuleTextFilter)filterBO["NAME"]).Property);

					var menuItem = module.ToggleIndexSearchFilterMenuItem;
					menuItem.PerformClick();
					AssertEquals(SearchType.Sql, filterBO.SearchType);

					menuItem.PerformClick();
					AssertEquals(SearchType.Index, filterBO.SearchType);
					AssertEquals("External Defaults set", "TestIndex", ((IndexSearchModuleTextFilter)filterBO["NAME"]).Property);
				}
			}
		}

		#endregion

		#region Secondary Server

		public void TestPerformSearchOnSecondaryServer()
		{
			using (var testModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				testModule.PerformSearch();
				AssertEquals("Module : GlbStaff", testModule.GridCollection.Factory.NameForDebugging);
				SecondaryServerConnectionForModuleSearchDetailsProvider.ClearCache();
			}

			using (SystemDataRegistry.Instance.EnableModuleQueryFromSecondaryDbReplica.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ModuleQueryDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { Db.ServerName }))
			using (var testModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				testModule.PerformSearch();
				AssertEquals("[SecondarySQLServer]Module : GlbStaff", testModule.GridCollection.Factory.NameForDebugging);
				SecondaryServerConnectionForModuleSearchDetailsProvider.ClearCache();
			}

			IndexSearchFilterHelper.ResetIndexQueryEngine();
			var staff = (BusinessObject)Factory.New<IGlbStaff>();
			staff.FillWithValidTestData();
			var staffNoInclude = (BusinessObject)Factory.New<IGlbStaff>();
			staffNoInclude.FillWithValidTestData();
			Factory.Save();
			using (var mocker = new GlowIndexQueryEngineMock(GetQueryResult(staff as IGlbStaff)))
			using (SystemDataRegistry.Instance.EnableModuleQueryFromSecondaryDbReplica.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ModuleQueryDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { Db.ServerName }))
			using (var testModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				AssertEquals(SearchType.Index, testModule.FilterBusinessObject.SearchType);
				testModule.PerformSearch();
				AssertEquals("Module : GlbStaff", testModule.GridCollection.Factory.NameForDebugging);
				SecondaryServerConnectionForModuleSearchDetailsProvider.ClearCache();
			}

			using (SystemDataRegistry.Instance.EnableModuleQueryFromSecondaryDbReplica.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ModuleQueryDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "invalidServerName" }))
			using (var testModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				var mockDbEnvirment = new Mock<IDbEnvironment>();
				mockDbEnvirment.SetupGet(x => x.ConnectionTimeout).Returns(1);
				using (DbEnv.SetTemporaryDbEnvironment(mockDbEnvirment.Object))
				{
					AssertEquals(true, SecondaryServerConnectionForModuleSearchDetailsProvider.IsSecondaryDbEnabled);
					testModule.PerformSearch();
					Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Secondary server \"invalidServerName\" unavailable, will perform search on primary server. Exception Message:"));
					AssertEquals("Module : GlbStaff", testModule.GridCollection.Factory.NameForDebugging);
					UnitTestUserNotification.Instance.ClearMessages();

					AssertEquals(false, SecondaryServerConnectionForModuleSearchDetailsProvider.IsSecondaryDbEnabled);
					testModule.PerformSearch();
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Module : GlbStaff", testModule.GridCollection.Factory.NameForDebugging);
					UnitTestUserNotification.Instance.ClearMessages();
					SecondaryServerConnectionForModuleSearchDetailsProvider.ClearCache();
				}
			}
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			DummyFilterGridModule.Filter = new ZQuery();
			ResetAllowNewToDefault();
		}

		protected override ModuleIdentifier ModuleID
		{
			get { return DummyModuleIDs.Dummy; }
		}

		protected override void SetActiveFilter(ZQuery query)
		{
			DummyFilterGridModule.Filter = query;
		}

		protected override void SetAllowNew(bool value)
		{
			DummyFilterGridModule.SetAllowNew(value);
		}

		protected override void ResetAllowNewToDefault()
		{
			DummyFilterGridModule.ResetAllowNewToDefault();
		}

		void AssertMenuPath(string message, MenuItem root, params string[] path)
		{
			AssertMenuPath(message, root, 0, path);
		}

		void AssertMenuPath(string message, MenuItem root, int index, params string[] path)
		{
			if (index == path.Length)
			{
				Assert(true);
			}
			else
			{
				var child = FindMenu(root, path[index]);

				if (child == null)
				{
					var builder = new System.Text.StringBuilder(Html(message));
					builder.Append("<br/><br/>Cant Find: ");
					builder.Append(Html(string.Join(" -> ", path, 0, index)));
					if (index > 0)
					{
						builder.Append(" -> ");
					}

					builder.Append(" <b>");
					builder.Append(Html(path[index]));
					builder.Append("</b> ");
					if (index + 1 < path.Length)
					{
						builder.Append(" -> ");
					}

					builder.Append(Html(string.Join(" -> ", path, index + 1, path.Length - index - 1)));
					HtmlFail(builder.ToString());
				}
				else
				{
					AssertMenuPath(message, child, index + 1, path);
				}
			}
		}

		MenuItem FindMenu(MenuItem parent, string childName)
		{
			foreach (MenuItem item in parent.MenuItems)
			{
				if (item.Text == childName)
				{
					return item;
				}
			}
			return null;
		}

		class DummyFilterGridModuleWithDataImportWizard : DummyFilterGridModule, IImportCollectionInfoProvider
		{
			protected override DataTransferProcessor GetDataImportWizardProcessor(IImportCollectionInfo collectionInfo)
			{
				return new DummyDataTransferProcessor();
			}

			string IImportCollectionInfoProvider.ContextKey
			{
				get { return "Dummy"; }
			}

			IImportCollectionInfo IImportCollectionInfoProvider.ImportCollectionInfo
			{
				get { return new DummyIImportCollectionInfo(Factory); }
			}

			public void RunImportDataWizard_Exposed()
			{
				base.RunImportDataWizard();
			}
		}

		class DummyDataTransferProcessor : DataTransferProcessor
		{
			public DummyDataTransferProcessor()
			{
			}

			public override void Import()
			{
			}

			public override void Rollback()
			{
			}
		}

		class DummyIImportCollectionInfo : IImportCollectionInfo
		{
			public DummyIImportCollectionInfo(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			readonly BusinessObjectFactory factory;

			public IBusinessObjectCollection Collection
			{
				get { return collection ?? (collection = new DummyBusinessObjectCollection(factory)); }
			}

			IBusinessObjectCollection collection;

			public void OnImportCompleted(bool success)
			{
			}

			public void OnImportStarted()
			{
			}

			public IEnumerable<IImportPropertyInfo> Properties
			{
				get { return new List<IImportPropertyInfo>(); }
			}

			public bool ValidateAndSave
			{
				get { return false; }
			}
		}
		#endregion
	}

	[UseSnapshotProtection]
	class ZFilterGridModuleNonTransactionedTest : TestCase
	{
		#region Threading

		class NotAsyncStrategy : IAsyncStrategy
		{
			public Action OnGetAsync { get; set; }

			public Task DoAsync(Action action, IThreadSentry threadSentry = null, [CallerMemberName] string threadName = "")
			{
				OnGetAsync?.Invoke();
				action();
				return Task.CompletedTask;
			}

			public Thread DoAsyncAsThread(Action action, ApartmentState apartmentState = ApartmentState.MTA, IThreadSentry threadSentry = null, [CallerMemberName] string threadName = "")
			{
				OnGetAsync?.Invoke();
				action();
				return Thread.CurrentThread;
			}

			public Task<T> GetAsync<T>(Func<T> func, IThreadSentry threadSentry = null, [CallerMemberName] string threadName = "", CancellationTokenSource cancellationTokenSource = null)
			{
				OnGetAsync?.Invoke();
				var source = new TaskCompletionSource<T>();
				source.SetResult(func());
				return source.Task;
			}

			public IAutoRefresher GetAutoRefresher(AutoRefreshAction updateAction, TimeSpan delay, Func<bool> shouldRefresh = null)
			{
				throw new NotImplementedException();
			}

			public void ParallelForEach<T>(IEnumerable<T> source, Action<T> body)
			{
				foreach (var item in source)
				{
					body(item);
				}
			}
		}

		public void TestLoadAsync_Cancel()
		{
			var newFactory = new BusinessObjectFactory();
			var dummy = newFactory.New<DummyBusinessObject>();
			newFactory.Save();

			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.ShouldPerformSearchAsync = true;
				using (var form = new ZForm())
				{
					var filterControl = (DummyFilterControl)module.EmbeddedControl;
					form.Controls.Add(filterControl);
					form.Show();

					var strat = new NotAsyncStrategy();
					module.AsyncStrategy = strat;
					strat.OnGetAsync = () =>
					{
						module.AsyncTaskCancellationTokenSource.Cancel();
					};

					module.PerformSearchAsync(new PerformSearchAsyncEventArgs(null, null, null));
					Application.DoEvents();

					AssertNull("We cancelled query so it never ran.", module.GetFirstBizOInList());
					AssertNull(module.SearchResultBoundToGrid);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		public void TestLoadAsync_ShouldPerformThreadSentryHandoverForSearchResultFactory()
		{
			var newFactory = new BusinessObjectFactory();
			var dummy = newFactory.New<DummyBusinessObject>();
			newFactory.Save();

			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.ShouldPerformSearchAsync = true;
				using (var form = new ZForm())
				{
					var filterControl = (DummyFilterControl)module.EmbeddedControl;
					form.Controls.Add(filterControl);
					form.Show();

					module.PerformSearchAsync(new PerformSearchAsyncEventArgs(null, null, null));
					module.AsyncTask.Wait();
					Application.DoEvents();

					AssertNull("Should complete the search", module.AsyncTask);
					AssertEquals(dummy.PK, module.GetFirstBizOInList().PK);

					AssertNotNull(module.SearchResultBoundToGrid);
					Assert("The result search factory should be handed over to the grid's thread", module.SearchResultBoundToGrid.Factory.IsOwnedByCurrentThread);
				}
			}
		}

		public void TestLoadAsync_EmptyResult()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.ShouldPerformSearchAsync = true;
				using (var form = new ZForm())
				{
					var filterControl = (DummyFilterControl)module.EmbeddedControl;
					form.Controls.Add(filterControl);
					form.Show();

					module.PerformSearchAsync(new PerformSearchAsyncEventArgs(null, null, null));
					module.AsyncTask.Wait();
					Application.DoEvents();

					AssertNull("Should complete the search", module.AsyncTask);
					AssertNull("Should be no dummy bizos found", module.GetFirstBizOInList());

					AssertNotNull(module.SearchResultBoundToGrid);
					AssertNotNull(module.SearchResultBoundToGrid.Factory);
					Assert("The result search factory should be handed over to the grid's thread", module.SearchResultBoundToGrid.Factory.IsOwnedByCurrentThread);
				}
			}
		}

		public void TestLoadAsync_ShouldPerformThreadSentryHandoverForNewlyCreatedSearchResultFactory_WhenQueryHasHighEstimateCostAndStillProceeds()
		{
			var newFactory = new BusinessObjectFactory();
			var dummy = newFactory.New<DummyBusinessObject>();
			newFactory.Save();

			int callNumber = 0;
			BusinessObjectFactory firstSearchAttemptFactory = null;
			BusinessObjectFactory secondSearchAttemptFactory = null;
			Action<BusinessObjectFactory> loadCollectionCoreCallback = (factory) =>
			{
				if (callNumber++ == 0)
				{
					firstSearchAttemptFactory = factory;
					throw SqlExceptionBuilder.CreateSqlException(8649, "The query has been canceled because the estimated cost of this query (...) exceeds the configured threshold of ... Contact the system administrator");
				}
				secondSearchAttemptFactory = factory;
			};
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // still proceed with the query

			using (var module = new DummyZModuleWithLoadCollectionCoreCallback(loadCollectionCoreCallback))
			{
				module.ShouldPerformSearchAsync = true;
				using (var form = new ZForm())
				{
					var filterControl = (DummyFilterControl)module.EmbeddedControl;
					form.Controls.Add(filterControl);
					form.Show();

					module.PerformSearchAsync(new PerformSearchAsyncEventArgs(null, null, null));
					module.AsyncTask.Wait();
					Application.DoEvents();

					AssertNull("Should complete the search", module.AsyncTask);
					AssertEquals(dummy.PK, module.GetFirstBizOInList().PK);

					AssertNotNull("Should generate a factory for the first search attempt when it realises that the query is too expensive", firstSearchAttemptFactory);
					AssertNotNull("Should generate another factory after the user commits to proceed anyway", secondSearchAttemptFactory);
					AssertNotEquals(firstSearchAttemptFactory, secondSearchAttemptFactory);

					AssertNotNull(module.SearchResultBoundToGrid);
					AssertEquals("The result bound to the grid should be loaded by the second factory", secondSearchAttemptFactory, module.SearchResultBoundToGrid.Factory);
					Assert("The result search factory should be handed over to the grid's thread", module.SearchResultBoundToGrid.Factory.IsOwnedByCurrentThread);
				}
			}
		}

		class DummyZModuleWithLoadCollectionCoreCallback : DummyFilterGridModule
		{
			public DummyZModuleWithLoadCollectionCoreCallback(Action<BusinessObjectFactory> loadCollectionCoreCallback)
			{
				this.loadCollectionCoreCallback = loadCollectionCoreCallback;
			}

			readonly Action<BusinessObjectFactory> loadCollectionCoreCallback;

			protected override FilteredGridLoader CreateSearchManager()
				=> new FilteredGridLoaderWithLoadCollectionCoreCallback(loadCollectionCoreCallback, FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);
		}

		class FilteredGridLoaderWithLoadCollectionCoreCallback : FilteredGridLoader
		{
			public FilteredGridLoaderWithLoadCollectionCoreCallback(Action<BusinessObjectFactory> loadCollectionCoreCallback, FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
				: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
			{
				this.loadCollectionCoreCallback = loadCollectionCoreCallback;
			}

			readonly Action<BusinessObjectFactory> loadCollectionCoreCallback;

			protected override BusinessObject[] LoadCollectionCore(BusinessObjectFactory factory, Type type, ZQuery query)
			{
				loadCollectionCoreCallback.Invoke(factory);
				return base.LoadCollectionCore(factory, type, query);
			}
		}

		public void TestLoadAsync_ShouldNotDoThreadSentryHandover_ForFactoriesUnrelatedToSearchResult()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.ShouldPerformSearchAsync = true;
				using (var form = new ZForm())
				{
					var filterControl = (DummyFilterControl)module.EmbeddedControl;
					form.Controls.Add(filterControl);
					form.Show();

					BusinessObjectFactory createdFactory = null;
					module.LoadingCollection += (e) => createdFactory = new BusinessObjectFactory();
					module.PerformSearchAsync(new PerformSearchAsyncEventArgs(null, null, null));
					module.AsyncTask.Wait();
					Application.DoEvents();
					AssertNull(module.AsyncTask);
					Assert("Should not handover the factory as it's unrelated to the search result", !createdFactory.IsOwnedByCurrentThread);
				}
			}
		}

		public void TestOpeningEntitiesOnBackgroundThread_ShouldBeThreadSafe()
		{
			MainThreadRunner.InvocationStrategy.Value = new NonBlockingMainThreadInvocationStrategy();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = "DJT";

			factory.Save();

			DoModuleActionOnBackgroundThread((module, loadedDummy) => module.ShowViewForm(loadedDummy), dummy);
			DoModuleActionOnBackgroundThread((module, loadedDummy) => module.ShowEditForm(loadedDummy), dummy);
			DoModuleActionOnBackgroundThread((module, loadedDummy) => module.ShowDeleteForm(loadedDummy), dummy);

			AssertNull(ErrorReporter.LastExceptionReported);
		}

		static void DoModuleActionOnBackgroundThread(Action<ZFilterGridModule, BusinessObject> moduleAction, BusinessObject dummy)
		{
			DBConnectionDisposalAsyncStrategy.Get().DoAsync(() =>
			{
				var newFactory = new BusinessObjectFactory();
				var loadedDummy = newFactory.Load<DummyBusinessObject>(dummy.PK);

				using (var module = new NaughtyZModule())
				{
					moduleAction(module, loadedDummy);
				}
			}).GetAwaiter().GetResult();

			Application.DoEvents();
		}

		class NaughtyZModule : ZFilterGridModule
		{
			public override ModuleIdentifier ID => DummyModuleIDs.Dummy;

			public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

			protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

			protected override FilterBusinessObject GetNewFilterBusinessObject()
			{
				return new DummyFilterBusinessObject();
			}

			protected override IFilterControl GetNewFilterControl()
			{
				throw new NotImplementedException();
			}

			protected override IBusinessObjectCollection GetNewGridCollection()
			{
				return new DummyBusinessObjectCollection(Factory);
			}

			protected internal override ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				HitTheDatabase(selectedBusinessObject);

				return new NaughtyZController();
			}
		}

		class NaughtyZController : ZController
		{
			public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

			public override ControllerID ID => DummyControllerIDs.Dummy;

			public override ModuleIdentifier ModuleID => DummyModuleIDs.Dummy;

			public override Type TypeOfTopLevelBusinessObject => typeof(DummyBusinessObject);

			protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

			protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

			protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

			protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

			protected override IZForm GetForm(IBusiness businessEntity)
			{
				HitTheDatabase(businessEntity);

				var closingForm = new ZForm(businessEntity);
				closingForm.Shown += delegate
				{ closingForm.Dispose(); };

				return closingForm;
			}

			public override IZForm ShowEditForm(BusinessObject sourceEntity)
			{
				HitTheDatabase(sourceEntity);

				return base.ShowEditForm(sourceEntity);
			}

			public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
			{
				HitTheDatabase(sourceEntity);

				return base.ShowDeleteForm(sourceEntity);
			}

			public override IZForm ShowViewForm(BusinessObject sourceEntity)
			{
				HitTheDatabase(sourceEntity);

				return base.ShowViewForm(sourceEntity);
			}
		}

		static void HitTheDatabase(IBusiness bizo) => bizo.Factory.Load<DummyBusinessObject>(ZGuid.NewZGuid()); // Let's all go the database.

		#endregion
	}
}
