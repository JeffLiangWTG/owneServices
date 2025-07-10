using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.GUI;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class OperationalActionsMenuItemGeneratorTest : TestCaseWithFactory
	{
		public void TestMenuItemsSavesToDB()
		{
			var menuAction = Factory.New<StmMenuItem>();
			menuAction.SU_MenuName = "你好"; // hello // checking the nvarchar change
			menuAction.SU_MenuType = "ACT";
			menuAction.SU_BusinessContext = Environment.Env.CurrentUser.Initials;
			menuAction.SU_GS_NKStaffCode = "";
			Factory.Save();

			var retrivedMenuActionFromDB = Factory.Load<StmMenuItem>(menuAction.PK);
			AssertEquals(menuAction.SU_MenuName, retrivedMenuActionFromDB.SU_MenuName);
		}

		public void TestOnlyUseStandardMenuItem()
		{
			// non-standard menu items tend not to survive the "translation" process when building the tool strip.

			var manager = new OperationalActionManager(Factory, Context);
			manager.Actions.RemoveAndDeleteAll();

			OperationalAction action1 = manager.Actions.AddNew();
			action1.SU_MenuName = "Action1";
			action1.SU_MenuPath = "PathA";

			OperationalAction action2 = manager.Actions.AddNew();
			action2.SU_MenuName = "Action2";
			action2.SU_MenuPath = "PathA/PathB";

			var result = new StringBuilder();

			using (MenuItem dummy = new ZMenuItem("", Generator.Generate(false)))
			{
				var menuItems = new Queue<MenuItem>();

				menuItems.Enqueue(dummy);

				while (menuItems.Count > 0)
				{
					MenuItem item = menuItems.Dequeue();

					if (item.GetType() != typeof(ZMenuItem))
					{
						result.AppendFormat("{0} ({1})\r\n", item.Name, GetType());
					}

					foreach (MenuItem subItem in item.MenuItems)
					{
						menuItems.Enqueue(subItem);
					}
				}
			}

			AssertEquals("the following menu items are non-standard.", "", result.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Testing")]
		public void TestMultilingualMenuWhenLanguageChanges()
		{
			using (var mockFrn = Res.GetLanguageInstance(Core.SharedConstants.Languages.French).UseMockData())
			{
				const string frenchMenuName = "Nombre de Menu 1";
				const string frenchMenuPathA = "French Path A";
				const string frenchMenuPathB = "French Path B";
				const string frenchMenuPath = frenchMenuPathA + "/" + frenchMenuPathB;
				const string action1MenuName = "Action1";
				const string action1MenuPathA = "PathA";
				const string action1MenuPathB = "PathB";
				const string action1MenuPath = action1MenuPathA + "/" + action1MenuPathB;

				var keyMenuName = CustomizableDataResourceStrings.GetCustomizableDataKey("SU_MenuName_ACT", action1MenuName);
				mockFrn.Put(keyMenuName, new ResourceStringData(keyMenuName, frenchMenuName));

				var keyMenuPath = CustomizableDataResourceStrings.GetCustomizableDataKey("SU_MenuPath_ACT", action1MenuPath);
				mockFrn.Put(keyMenuPath, new ResourceStringData(keyMenuPath, frenchMenuPath));

				var manager = new OperationalActionManager(Factory, Context);
				manager.Actions.RemoveAndDeleteAll();

				var action1 = manager.Actions.AddNew();

				action1.SU_MenuName = action1MenuName;
				action1.SU_MenuPath = action1MenuPath;

				Factory.Save();

				using (var dummy = new ZMenuItem("", Generator.Generate(false)))
				{
					AssertEquals(action1MenuName, Res.GetString(keyMenuName, action1MenuName));
					AssertEquals(action1MenuPath, Res.GetString(keyMenuPath, action1MenuPath));
					MenuAssertion.AssertHasMenu(dummy, action1MenuPathA, action1MenuPathB, action1MenuName);
				}

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.French))
				{
					using (var dummy = new ZMenuItem("", Generator.Generate(false)))
					{
						AssertEquals(frenchMenuName, Res.GetString(keyMenuName, action1MenuName));
						AssertEquals(frenchMenuPath, Res.GetString(keyMenuPath, action1MenuPath));
						MenuAssertion.AssertHasMenu(dummy, frenchMenuPathA, frenchMenuPathB, frenchMenuName);
					}
				}
			}
		}

		public void TestGeneratePath()
		{
			var manager = new OperationalActionManager(Factory, Context);
			manager.Actions.RemoveAndDeleteAll();

			var action1 = manager.Actions.AddNew();
			var action2 = manager.Actions.AddNew();
			var action3 = manager.Actions.AddNew();
			var action4 = manager.Actions.AddNew();
			var action5 = manager.Actions.AddNew();
			var action6 = manager.Actions.AddNew();
			var action7 = manager.Actions.AddNew();

			action1.SU_MenuName = "Action1";
			action1.SU_MenuPath = "PathA";

			action2.SU_MenuName = "Action2";
			action2.SU_MenuPath = "PathA/PathB";

			action3.SU_MenuName = "Action3";
			action3.SU_MenuPath = "PathB//";

			action4.SU_MenuName = "Action4";
			action4.SU_MenuPath = "";

			action5.SU_MenuName = "Action5";
			action5.SU_MenuPath = "/";

			action6.SU_MenuName = "Action6";
			action6.SU_MenuPath = "/PathC";

			action7.SU_MenuName = "Action7";
			action7.SU_MenuPath = "\\PathC\\PathD";

			Factory.Save();

			using (var dummy = new ZMenuItem("", Generator.Generate(false)))
			{
				MenuAssertion.AssertHasMenu(dummy, "PathA", "Action1");
				MenuAssertion.AssertHasMenu(dummy, "PathA", "PathB", "Action2");
				MenuAssertion.AssertHasMenu(dummy, "PathB", "Action3");
				MenuAssertion.AssertHasMenu(dummy, "Action4");
			}

			using (var dummy = new ZMenuItem("", Generator.Generate(true)))
			{
				MenuAssertion.AssertHasMenu(dummy, "Action5");
				MenuAssertion.AssertHasMenu(dummy, "PathC", "Action6");
				MenuAssertion.AssertHasMenu(dummy, "PathC", "PathD", "Action7");
			}
		}

		public void TestDoAction()
		{
			ActionSupportable.runCheckpoint = Env.Security.FindOrCreateOperationalActionsRunCheckpoint(Env.Security.Organisation);
			ActionSupportable.runCheckpoint.IsAllowed = true;

			var manager = new OperationalActionManager(Factory, Context);
			manager.Actions.RemoveAndDeleteAll();

			OperationalAction action = manager.Actions.AddNew();
			action.SU_MenuName = "Action1";

			Factory.Save();

			Module.GridCollection.AddNew();

			MenuItem[] menuItems = Generator.Generate(false);
			menuItems[0].PerformClick();

			AssertNotNull("should have shown a form", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("should have shown the correct form", typeof(OperationalActionRunnerForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			OperationalActionRunner runner = (OperationalActionRunner)((ZForm)ZFormModaliser.LastFormShownDialogForTest).LastDataSourceForTest;
			AssertEquals("should have used the correct action", action.PK, runner.Action.PK);
			AssertNotEquals("runner form should not be using the menu factory", Factory, runner.Action.Factory);

			Dispose(menuItems);
		}

		public void TestDoActionRunWithoutUI()
		{
			ActionSupportable.runCheckpoint = Env.Security.FindOrCreateOperationalActionsRunCheckpoint(Env.Security.Organisation);
			ActionSupportable.runCheckpoint.IsAllowed = true;

			var manager = new OperationalActionManager(Factory, Context);
			manager.Actions.RemoveAndDeleteAll();
			manager.ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);

			var action = manager.Actions.AddNew();
			action.SU_MenuName = "Action1";

			var methodDescriptor = action.MethodDescriptors.AddNew();
			methodDescriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			methodDescriptor.MethodID = TestingConstants.DummyActionMethodRunWithoutUI;

			Factory.Save();

			Module.GridCollection.AddNew();

			MenuItem[] menuItems = Generator.Generate(false);
			menuItems[0].PerformClick();

			AssertNull("should have not shown a form for non-ui only actions", ZFormModaliser.LastFormShownDialogForTest);

			methodDescriptor = action.MethodDescriptors.AddNew();
			methodDescriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			methodDescriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;

			Factory.Save();

			menuItems[0].PerformClick();

			AssertNotNull("should have shown a form for UI action", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("should have shown the correct form", typeof(OperationalActionRunnerForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			var runner = (OperationalActionRunner)((ZForm)ZFormModaliser.LastFormShownDialogForTest).LastDataSourceForTest;
			AssertEquals("should have used the correct action", action.PK, runner.Action.PK);
			AssertNotEquals("runner form should not be using the menu factory", Factory, runner.Action.Factory);

			Dispose(menuItems);
		}

		public void TestDoAction_RunSecurityCheckpoint_IsNotAllowed()
		{
			ActionSupportable.runCheckpoint = Env.Security.FindOrCreateOperationalActionsRunCheckpoint(Env.Security.Organisation);
			ActionSupportable.runCheckpoint.IsAllowed = true;
			Env.Security.FindOrCreateOperationalActionsRunSpecificCheckpoint((NoResString)"Action1", ActionSupportable.runCheckpoint).IsAllowed = false;

			var manager = new OperationalActionManager(Factory, Context);
			manager.Actions.RemoveAndDeleteAll();

			OperationalAction action = manager.Actions.AddNew();
			action.SU_MenuName = "Action1";

			Factory.Save();

			MenuItem[] menuItems = Generator.Generate(false);
			menuItems[0].PerformClick();

			AssertNull("should NOT have shown a form", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> Operational Actions -> Run Actions -> Action1", UnitTestUserNotification.Instance.LastMessage.Text);
			Dispose(menuItems);
		}

		public void TestDoAction_ActionSpecificSecurity_Allowed()
		{
			ActionSupportable.runCheckpoint = Env.Security.FindOrCreateOperationalActionsRunCheckpoint(Env.Security.Organisation);
			ActionSupportable.runCheckpoint.IsAllowed = false;
			Env.Security.FindOrCreateOperationalActionsRunSpecificCheckpoint((NoResString)"Action1", ActionSupportable.runCheckpoint).IsAllowed = true;

			var manager = new OperationalActionManager(Factory, Context);
			manager.Actions.RemoveAndDeleteAll();
			manager.ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);

			var action = manager.Actions.AddNew();
			action.SU_MenuName = "Action1";
			action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithGUI);
			action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithoutGUI);

			Factory.Save();

			Module.GridCollection.AddNew();

			Env.Security.Schedules.IsAllowed = true;
			Env.Security.Forwarding.IsAllowed = true;
			Env.Security.AgencyBillOfLading.IsAllowed = true;

			MenuItem[] menuItems = Generator.Generate(false);
			menuItems[0].PerformClick();

			AssertEquals("Should not have shown an error.", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals("Should have shown the runner form", typeof(OperationalActionRunnerForm), ZFormModaliser.LastFormShownDialogForTest == null ? null : ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestDoAction_ActionSpecificSecurity_NotAllowed()
		{
			var manager = new OperationalActionManager(Factory, Context);
			manager.Actions.RemoveAndDeleteAll();
			manager.ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);

			var action = manager.Actions.AddNew();
			action.SU_MenuName = "Action1";
			action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithGUI);
			action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithoutGUI);

			Factory.Save();

			Module.GridCollection.AddNew();

			Env.Security.Schedules.IsAllowed = false;
			Env.Security.Forwarding.IsAllowed = false;
			Env.Security.AgencyBillOfLading.IsAllowed = false;

			MenuItem[] menuItems = Generator.Generate(false);
			menuItems[0].PerformClick();

			const string expectedMessage =
					"You are missing 1 or more security rights required to run this action.\r\n" +
					"If you believe you should be able to run this action then you should ask your administrator to grant you the following security rights.\r\n" +
					"\r\n" +
					"\tOperate -> Forwarding\r\n" +
					"\tOperate -> Liner & Agency -> Bills of Lading\r\n" +
					"\tOperate -> Schedules\r\n" +
					"";

			AssertMultilineASCIIEquals("Should have shown the error.", "Error " + expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals("Should not have shown the runner form", null, ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestDoAction_NoEditPermission_NotAllowedActionWithField()
		{
			var manager = new OperationalActionManager(Factory, Context);
			manager.Actions.RemoveAndDeleteAll();
			manager.ActionSupporter.Methods.Add(ActionMethodProviderIDs.JobDeclaration);

			var action1 = manager.Actions.AddNew();
			action1.SU_MenuName = "Action1";
			action1.FieldDescriptors.AddNew().FieldName = "Field1";

			action1.MethodDescriptors.AddNew(ActionMethodProviderIDs.JobDeclaration, TestingConstants.DummyActionMethodWithGUI);
			action1.MethodDescriptors.AddNew(ActionMethodProviderIDs.JobDeclaration, TestingConstants.DummyActionMethodWithoutGUI);

			var action2 = manager.Actions.AddNew();
			action2.SU_MenuName = "Action2";
			action2.MethodDescriptors.AddNew(ActionMethodProviderIDs.JobDeclaration, TestingConstants.DummyActionMethodWithGUI);
			action2.MethodDescriptors.AddNew(ActionMethodProviderIDs.JobDeclaration, TestingConstants.DummyActionMethodWithoutGUI);

			Factory.Save();

			Module_Shipment.GridCollection.AddNew();
			Module_Shipment.GridCollection.Factory.Save();

			MenuItem[] menuItems = Generator_ShipmentModule.Generate(false);

			Env.Security.MaintainShipmentEdit.IsAllowed = false;
			menuItems.FirstOrDefault(x => x.Text == "Action1").PerformClick();

			const string expectedMessage =
					"You are missing 1 or more security rights required to run this action.\r\n" +
					"If you believe you should be able to run this action then you should ask your administrator to grant you the following security rights.\r\n" +
					"\r\n" +
					"\tOperate -> Forwarding -> Shipments -> Edit\r\n" +
					"";

			AssertMultilineASCIIEquals("Should have shown the error.", "Error " + expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals("Should not have shown the runner form", null, ZFormModaliser.LastFormShownDialogForTest);

			UnitTestUserNotification.Instance.ClearMessages();
			menuItems.FirstOrDefault(x => x.Text == "Action2").PerformClick();

			AssertEquals("Should not have shown an error.", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals("Should have shown the runner form", typeof(OperationalActionRunnerForm), ZFormModaliser.LastFormShownDialogForTest == null ? null : ZFormModaliser.LastFormShownDialogForTest.GetType());

			Module.GridCollection.AddNew();
			Module.GridCollection.Factory.Save();

			menuItems = Generator.Generate(false);

			Env.Security.MaintainShipmentEdit.IsAllowed = false;
			menuItems.FirstOrDefault(x => x.Text == "Action1").PerformClick();

			AssertEquals("Should not have shown an error.", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals("Should have shown the runner form", typeof(OperationalActionRunnerForm), ZFormModaliser.LastFormShownDialogForTest == null ? null : ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestDoAction_WithEditPermission_Allowed()
		{
			var manager = new OperationalActionManager(Factory, Context);
			manager.Actions.RemoveAndDeleteAll();
			manager.ActionSupporter.Methods.Add(ActionMethodProviderIDs.JobDeclaration);

			var action1 = manager.Actions.AddNew();
			action1.SU_MenuName = "Action1";
			action1.FieldDescriptors.AddNew().FieldName = "Moo";
			action1.MethodDescriptors.AddNew(ActionMethodProviderIDs.JobDeclaration, TestingConstants.DummyActionMethodWithGUI);
			action1.MethodDescriptors.AddNew(ActionMethodProviderIDs.JobDeclaration, TestingConstants.DummyActionMethodWithoutGUI);

			Factory.Save();

			Module.GridCollection.AddNew();

			MenuItem[] menuItems = Generator.Generate(false);

			Env.Security.MaintainShipmentEdit.IsAllowed = true;
			menuItems[0].PerformClick();

			AssertEquals("Should not have shown an error.", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals("Should have shown the runner form", typeof(OperationalActionRunnerForm), ZFormModaliser.LastFormShownDialogForTest == null ? null : ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestGetControllerRecursivelyFromType_NoInfiniteLoop()
		{
			Assert(!OperationalActionsMenuItemGenerator.GetControllersRecursivelyFromType(typeof(BusinessObject)).Any());
		}

		public void TestDoAction_NoObjectsSelected()
		{
			ActionSupportable.runCheckpoint = Env.Security.FindOrCreateOperationalActionsRunCheckpoint(Env.Security.Organisation);
			ActionSupportable.runCheckpoint.IsAllowed = true;

			var manager = new OperationalActionManager(Factory, Context);
			manager.Actions.RemoveAndDeleteAll();

			var action = manager.Actions.AddNew();
			action.SU_MenuName = "Action1";

			Factory.Save();

			MenuItem[] menuItems = Generator.Generate(false);
			menuItems[0].PerformClick();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals(typeof(OperationalActionRunnerForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			Module.GridCollection.AddNew();

			menuItems = Generator.Generate(false);
			menuItems[0].PerformClick();

			AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals(typeof(OperationalActionRunnerForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			var runner = (OperationalActionRunner)((ZForm)ZFormModaliser.LastFormShownDialogForTest).LastDataSourceForTest;
			AssertEquals(action.PK, runner.Action.PK);
			AssertNotEquals("Runner form should not be using the menu factory", Factory, runner.Action.Factory);

			Dispose(menuItems);
		}

		public void TestSubMenuItems()
		{
			var manager = new OperationalActionManager(Factory, Context);

			AssertMenuItems("empty", manager, "No Operational Actions Found.", "-", "Customize");

			var action1 = manager.Actions.AddNew();
			action1.SU_MenuName = "Action1";

			var action2 = manager.Actions.AddNew();
			action2.SU_MenuName = "Action2";
			action2.SU_FilterList = "Country == \"AU\"";

			var action3 = manager.Actions.AddNew();
			action3.SU_MenuName = "Action3";
			action3.SU_FilterList = "Country == \"NZ\"";

			Factory.Save();

			AssertMenuItems("2 actions match their filters", manager, "Action1", "Action2", "-", "Customize");

			action1.Delete();
			action2.Delete();

			Factory.Save();

			AssertMenuItems("no actions match their filters", manager, "No Operational Actions Found.", "-", "Customize");
		}

		public void TestDoAction_DeletedRecordMessage()
		{
			ActionSupportable.runCheckpoint = Env.Security.FindOrCreateOperationalActionsRunCheckpoint(Env.Security.Organisation);
			ActionSupportable.runCheckpoint.IsAllowed = true;

			var manager = new OperationalActionManager(Factory, Context);
			manager.Actions.RemoveAndDeleteAll();

			var action = manager.Actions.AddNew();
			action.SU_MenuName = "Action1";

			Factory.Save();

			var menuItems = Generator.Generate(false);

			var actionInRunnerFactory = Factory.Load<OperationalAction>(action.PK);
			actionInRunnerFactory.Delete();
			Factory.Save();

			AssertNoExceptionThrown(() => menuItems[0].PerformClick());
			AssertEquals("The Operational Action 'Action1' could not be found, it has been deleted by another user.", UnitTestUserNotification.Instance.LastMessage.Text);

			Dispose(menuItems);
		}

		#region Implementation

		void AssertMenuItems(string message, OperationalActionManager manager, params string[] items)
		{
			MenuItem[] menuItems = Generator.Generate(false);
			using (new ZMenuItem("", menuItems))
			{
				AssertArrayEqualsByElements(message, items, Array.ConvertAll(menuItems, (m) => m.Text));
			}
		}

		OperationalActionsMenuItemGenerator Generator
		{
			get { return generator ?? (generator = new OperationalActionsMenuItemGenerator(Factory, new ModuleSelection(Module), Context)); }
		}
		OperationalActionsMenuItemGenerator generator;

		OperationalActionsMenuItemGenerator Generator_ShipmentModule
		{
			get { return generator_ShipmentModule ?? (generator_ShipmentModule = new OperationalActionsMenuItemGenerator(Factory, new ModuleSelection(Module_Shipment), Context)); }
		}
		OperationalActionsMenuItemGenerator generator_ShipmentModule;

		DummyFilterGridModule Module
		{
			get { return module ?? (module = new DummyModuleWithActionsSupport<DummyBusinessObjectWithDocumentSupport>()); }
		}
		DummyFilterGridModule module;

		DummyFilterGridModule Module_Shipment
		{
			get
			{
				if (module_Shipment == null)
				{
					module_Shipment = new DummyModuleWithActionsSupport<DummyBusinessObjectWithDocumentSupport>();
					module_Shipment.fControllerID = ControllerIDs.JobShipment;
				}
				return module_Shipment ?? (module_Shipment = new DummyModuleWithActionsSupport<DummyBusinessObjectWithDocumentSupport>());
			}
		}
		DummyModuleWithActionsSupport<DummyBusinessObjectWithDocumentSupport> module_Shipment;

		OperationalActionContext Context
		{
			get { return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name")); }
		}
		OperationalActionContext context;

		OperationalActionSupporter ActionSupporter
		{
			get { return actionSupporter ?? (actionSupporter = ActionSupportable.OperationalActionSupporter); }
		}
		OperationalActionSupporter actionSupporter;

		MockOperationalActionSupportable ActionSupportable
		{
			get
			{
				if (actionSupportable == null)
				{
					actionSupportable = new MockOperationalActionSupportable();
					actionSupportable.businessContext = (BusinessContext)(-1);
				}
				return actionSupportable;
			}
		}
		MockOperationalActionSupportable actionSupportable;

		void Dispose(MenuItem[] menuItemsToDispose)
		{
			foreach (MenuItem item in menuItemsToDispose)
			{
				item.Dispose();
			}
		}

		protected override void TearDown()
		{
			using (module)
			using (module_Shipment)
			{
				base.TearDown();
			}
		}

		#endregion
	}
}
