using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.UniversalCopy;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalCopy.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.UniversalCopy.GUI.Testing.GridUniversalCopyManagerTest;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	public sealed class GridUniversalCopyManagerTest : TestCaseWithFactory
	{
		public void TestAllowUniversalCopy()
		{
			using (var form = new ZForm())
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 50));
				form.Controls.Add(grid);
				grid.SetDataBinding(new DummyBusinessObjectCollection(Factory), "");

				grid.SetElementTypeForTest(typeof(UniversalCopyManagerTest.DummyA));
				using (var copyManager = new GridUniversalCopyManager(grid))
				{
					Assert("Do not allow as DummyA has no GLOW interface with description.", !copyManager.AllowsUniversalCopy);
				}

				grid.SetElementTypeForTest(typeof(UniversalCopyManagerTest.DummyB));
				using (var copyManager = new GridUniversalCopyManager(grid))
				{
					Assert("Allow as DummyB has GLOW interface with description.", copyManager.AllowsUniversalCopy);
				}

				grid.SetElementTypeForTest(typeof(UniversalCopyManagerTest.DummyC));
				using (var copyManager = new GridUniversalCopyManager(grid))
				{
					Assert("Allow as DummyC has UniversalCopyWithExtendedEntitiesAttribute applied.", copyManager.AllowsUniversalCopy);
				}
			}
		}

		public void TestAllowUniversalCopy_ZGridModule()
		{
			using (var form = new ZForm())
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 50));
				form.Controls.Add(grid);

				grid.SetDataBinding(new DummyCollectionWithModuleId(Factory), "");
				using (var copyManager = new GridUniversalCopyManagerForTest(grid))
				{
					var parentModule = copyManager.GetParentModuleExposed();
					using (var module = parentModule.Module)
					{
						AssertEquals("Should find the Staff module assign in the class", "GlbStaffModule", module.GetType().Name);
						Assert("Should dispose", parentModule.Dispose);
					}
				}

				grid.SetDataBinding(new DummyBusinessObjectCollection(Factory), "");
				using (var copyManager = new GridUniversalCopyManagerForTest(grid))
				{
					var parentModule = copyManager.GetParentModuleExposed();
					using (var module = parentModule.Module)
					{
						AssertNull("Class shouldn't have module", module);
						Assert("Shouldn't dispose", !parentModule.Dispose);
					}
				}
			}
		}

		public void TestAllowUniversalCopy_ZGridModule_NoController()
		{
			using var form = new ZForm();
			using var grid = new ZFilterGrid();
			using var parentModule = (ZEmbeddedModule)ZModuleFactory.Instance.Create(ModuleIDs.StmMenuItem);

			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 50));
			form.Controls.Add(grid);
			grid.SetDataBinding(new DummyBusinessObjectCollection(Factory), "");
			grid.SetElementTypeForTest(typeof(UniversalCopyManagerTest.DummyC));
			grid.SetParentFilterGridModule(parentModule);

			grid.ReadOnly = false;
			using (var copyManager = new GridUniversalCopyManager(grid))
			{
				Assert("Allow as grid is not readonly", copyManager.AllowsUniversalCopy);
			}

			grid.ReadOnly = true;
			using (var copyManager = new GridUniversalCopyManager(grid))
			{
				Assert("Do not allow as grid is readonly and module has no controller", !copyManager.AllowsUniversalCopy);
			}
		}

		public void TestCopySchedules_ShouldNotBeAvailable_ForWorkflowTemplates()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var bms = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IBMSystem)));
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_FS_BufferManagementSystem = bms.PK;
			Assert("Template should be associated with a buffer management system to allow new items in the process headers collection and thus allow universal copy for process headers", template.ProcessHeaders.AllowNew);

			using (var form = new ZForm())
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(ProcessHeaderSchema.Constants.FH_CompletionStatement, 50));
				form.Controls.Add(grid);
				grid.SetDataBinding(template, "ProcessHeaders");
				form.Show();

				grid.ListManager.AddNew();

				AssertSchedulesMenuIsNotAvailableForGrid(grid);
			}
		}

		public void TestCopySchedules_ShouldBeAvailable_ForOtherJobTypes()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var parent = Factory.NewWithValidTestData<DummyBusinessObject>();
			parent.Collection.Add(dummy1);
			parent.Collection.Add(dummy2);

			using (var form = new ZForm())
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(ProcessHeaderSchema.Constants.FH_CompletionStatement, 50));
				form.Controls.Add(grid);
				grid.SetDataBinding(parent, "Collection");
				form.Show();

				grid.ListManager.AddNew();

				AssertSchedulesMenuIsAvailableForGrid(grid);
			}
		}

		void AssertSchedulesMenuIsAvailableForGrid(ZGrid grid)
		{
			grid.ListManager.Position = 0;
			Application.DoEvents();
			AssertNotNull("Copy Schedules menu item should be available", GetSchedulesMenuItem(grid));
		}

		void AssertSchedulesMenuIsNotAvailableForGrid(ZGrid grid)
		{
			grid.ListManager.Position = 0;
			Application.DoEvents();
			AssertNull("Copy Schedules menu item should not be available", GetSchedulesMenuItem(grid));
		}

		MenuItem GetSchedulesMenuItem(ZGrid grid)
		{
			var ucMenuItem = grid.ContextMenu.MenuItems.FindByText("Universal Copy", true);
			AssertNotNull(ucMenuItem);
			ucMenuItem.PerformSelect();

			AssertNotEquals(0, ucMenuItem.MenuItems.Count);

			return ucMenuItem.MenuItems.FindByText("Copy Schedules");
		}

		public void TestPerformCopy_ForBusinessObjectWhichSpecifiesUniversalCopySelectively()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var dummy1 = Factory.New<UniversalCopyManagerTest.DummyWhichSelectivelySupportsUniversalCopy>();
			var dummy2 = Factory.New<UniversalCopyManagerTest.DummyWhichSelectivelySupportsUniversalCopy>();

			collection.Add(dummy1);
			collection.Add(dummy2);

			CreateDummyCopyTemplate(dummy1, dummy2);

			dummy1.SupportsUniversalCopy = false;
			dummy1.ReasonForNotSupportingUniversalCopy = "I just don't like you enough.";

			dummy2.SupportsUniversalCopy = false;
			dummy2.ReasonForNotSupportingUniversalCopy = "Nyahhhhhh.";

			Factory.Save();

			using (var form = CreateFormWithUniversalCopyStuff(collection))
			{
				form.Grid.Select(0);
				form.Grid.Select(1);

				var menuItem = form.Grid.ContextMenu.MenuItems.FindByText("Universal Copy", true);

				AssertNotNull(menuItem);

				menuItem.PerformSelect();

				var subMenuItems = menuItem.MenuItems.Cast<MenuItem>().ToArray();
				var copyMenuItem = subMenuItems[0];

				AssertNotNull(copyMenuItem);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				copyMenuItem.PerformClick();

				AssertEquals("Cannot perform Universal Copy", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertMultilineASCIIEquals("",
@"Universal Copy cannot be performed for the following reasons:
I just don't like you enough.
Nyahhhhhh.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();
			dummy1.SupportsUniversalCopy = true;

			using (var form = CreateFormWithUniversalCopyStuff(collection))
			{
				form.Grid.Select(0);
				form.Grid.Select(1);

				var menuItem = form.Grid.ContextMenu.MenuItems.FindByText("Universal Copy", true);

				AssertNotNull(menuItem);

				menuItem.PerformSelect();

				var subMenuItems = menuItem.MenuItems.Cast<MenuItem>().ToArray();
				var copyMenuItem = subMenuItems[0];

				AssertNotNull(copyMenuItem);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				copyMenuItem.PerformClick();

				AssertEquals("Cannot perform Universal Copy", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertMultilineASCIIEquals("",
@"Universal Copy cannot be performed for the following reason:
Nyahhhhhh.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();
			dummy2.SupportsUniversalCopy = true;

			using (var form = CreateFormWithUniversalCopyStuff(collection))
			{
				form.Grid.Select(0);
				form.Grid.Select(1);

				var menuItem = form.Grid.ContextMenu.MenuItems.FindByText("Universal Copy", true);

				AssertNotNull(menuItem);

				menuItem.PerformSelect();

				var subMenuItems = menuItem.MenuItems.Cast<MenuItem>().ToArray();
				var copyMenuItem = subMenuItems[0];

				AssertNotNull(copyMenuItem);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				copyMenuItem.PerformClick();

				AssertEquals("Selected Copy Template has nothing set to copy and cannot be used.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateCopySchedule_ForBusinessObjectWhichSpecifiesUniversalCopySelectively()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var dummy = Factory.New<UniversalCopyManagerTest.DummyWhichSelectivelySupportsUniversalCopy>();

			collection.Add(dummy);

			CreateDummyCopyTemplate(dummy);

			dummy.SupportsUniversalCopy = false;
			dummy.ReasonForNotSupportingUniversalCopy = "I just don't like you enough.";

			Factory.Save();

			using (var form = CreateFormWithUniversalCopyStuff(collection))
			{
				var menuItem = form.Grid.ContextMenu.MenuItems.FindByText("Universal Copy", true);

				AssertNotNull(menuItem);

				menuItem.PerformSelect();

				var subMenuItems = menuItem.MenuItems.Cast<MenuItem>().ToArray();
				var copySchedulesMenuItem = subMenuItems.FindByText("Copy Schedules");
				var createScheduleMenuItem = copySchedulesMenuItem.MenuItems.FindByText("Create Copy Schedule");

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				createScheduleMenuItem.PerformClick();

				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Cannot create a copy schedule for this object", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("I just don't like you enough.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();

			dummy.SupportsUniversalCopy = true;

			using (var form = CreateFormWithUniversalCopyStuff(collection))
			{
				var menuItem = form.Grid.ContextMenu.MenuItems.FindByText("Universal Copy", true);

				AssertNotNull(menuItem);

				menuItem.PerformSelect();

				var subMenuItems = menuItem.MenuItems.Cast<MenuItem>().ToArray();
				var copySchedulesMenuItem = subMenuItems.FindByText("Copy Schedules");
				var createScheduleMenuItem = copySchedulesMenuItem.MenuItems.FindByText("Create Copy Schedule");

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				createScheduleMenuItem.PerformClick();

				AssertType<UniversalCopyScheduleForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		void CreateDummyCopyTemplate(params DummyBusinessObject[] dummies)
		{
			var copyTemplate = Factory.New<UniversalCopyTemplate>();

			copyTemplate.S9_ModuleID = "CargoWise.EntityFramework.Testing.DummyBusinessObject_UC";
			copyTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			copyTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyBusinessObject), true)), copyTemplate);
			copyTemplate.PrepareForSave();

			copyTemplate.CopyTemplateTree.ConfigurationName = "Mister Doctor";

			foreach (var dummy in dummies)
			{
				var copyJob = Factory.New<StmUniversalCopy>();
				copyJob.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
				copyJob.SUC_CopyObjectId = dummy.PK;
				copyJob.SUC_S9_CopyTemplate = copyTemplate.PK;
			}
		}

		ZFormWithGrid CreateFormWithUniversalCopyStuff(DummyBusinessObjectCollection gridCollection)
		{
			var form = new ZFormWithGrid();
			var module = new DummyModuleWithUniversalCopy();
			var grid = new DummyZFilterGridForUniversalCopyTest(module);
			var manager = new GridUniversalCopyManager(grid);

			form.Grid = grid;

			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 50));
			form.Controls.Add(grid);
			grid.SetDataBinding(gridCollection, "");
			manager.AddMenuItems();
			grid.Select(0);

			var menuItem = grid.ContextMenu.MenuItems.FindByText("Universal Copy", true);

			AssertNotNull(menuItem);

			menuItem.PerformSelect();

			form.Disposed += (s, e) => module.Dispose();

			return form;
		}

		public void TestSetCopyMenuWhenAllowed()
		{
			using (var module = new DummyModuleNoUniversalCopy())
			using (var grid = new DummyZFilterGridForUniversalCopyTest(module))
			using (var manager = new GridUniversalCopyManager(grid))
			{
				manager.AddMenuItems();

				AssertNull("Manager does not allow copy, there's no menu item for it", grid.ContextMenu.MenuItems.FindByText("Universal Copy", true));
			}
			using (var module = new DummyModuleWithUniversalCopy())
			using (var grid = new DummyZFilterGridForUniversalCopyTest(module))
			using (var manager = new GridUniversalCopyManager(grid))
			{
				manager.AddMenuItems();

				AssertNotNull("Manager allows copy, there's a menu item for it", grid.ContextMenu.MenuItems.FindByText("Universal Copy", true));
			}
		}

		public void TestUniversalCopyMenuShortcut()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var module = new DummyModuleWithAllowUniversalCopyExposed())
			using (var grid = new DummyZFilterGridForUniversalCopyTest(module))
			using (var manager = new GridUniversalCopyManager(grid))
			{
				manager.AddMenuItems();

				module.allowUniversalCopy = true;
				AssertEquals(true, module.AllowUniversalCopy);

				KeySender.PostKeyDown(grid, Keys.Control | Keys.Shift | Keys.C);
				Application.DoEvents();
				AssertContains("There are no copy templates to use.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				module.allowUniversalCopy = false;
				AssertEquals(false, module.AllowUniversalCopy);

				KeySender.PostKeyDown(grid, Keys.Control | Keys.Shift | Keys.C);
				Application.DoEvents();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestNewElementIsAddedToBoundCollection()
		{
			using (var module = new DummyModuleNoUniversalCopy())
			using (var form = new ZForm())
			using (var grid = new DummyZFilterGridForUniversalCopyTest(module))
			using (var manager = new GridUniversalCopyManager(grid))
			{
				form.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 50));
				var dummies = new DummyCollectionWithOnAdded(Factory);
				grid.SetDataBinding(dummies, "");

				manager.AddMenuItems();

				AssertEquals(0, dummies.Count);
				dummies.AddNew();
				AssertEquals(1, dummies.Count);
				Assert(!dummies.WasInUC);

				form.Show();
				Application.DoEvents();

				var copyTree = new CopyTemplateTree(typeof(DummyBusinessObject));
				manager.CopyMenuClicked(copyTree, new[] { dummies[0] });

				AssertEquals("New element should be added to bound collection", 2, dummies.Count);
				Assert("Copied element should be added to collection with BusinessObjectUniversalCopyFactoryService enabled", dummies.WasInUC);
			}
		}

		public void TestNewFactoryIsFromController()
		{
			using (var module = new DummyModuleWithUniversalCopy())
			using (var form = new ZForm())
			using (var grid = new DummyZFilterGridForUniversalCopyTest(module))
			using (var manager = new GridUniversalCopyManagerForTest(grid))
			{
				form.Controls.Add(grid);
				grid.ReadOnly = true;
				var dummy = Factory.New<DummyBusinessObject>();
				var dummyInLocalFactory = manager.CopyMenuClicked_GetSourceElement_Exposed(dummy);

				AssertNotEquals(dummy.Factory, dummyInLocalFactory.Factory);
				AssertEquals("Local factory should be provided by controller", DummyController.FactoryNameForDebugging, dummyInLocalFactory.Factory.NameForDebugging);
			}
		}

		public void TestGetDefaultFactoryForCopyManager()
		{
			using (var module = new DummyModuleWithUniversalCopy())
			using (var form = new ZForm())
			using (var grid = new DummyZFilterGridForUniversalCopyTest(module))
			using (var manager = new GridUniversalCopyManagerForTest(grid))
			{
				form.Controls.Add(grid);
				grid.ReadOnly = true;
				var defaultFactory = manager.GetDefaultFactoryForCopyManagerExposed();

				AssertEquals("Defalult factory should be provided by controller", DummyController.FactoryNameForDebugging, defaultFactory.NameForDebugging);
			}
		}

		public void TestElementTypeIsOverridden()
		{
			using (var module = new DummyModuleWithInstance())
			using (var grid = new DummyZFilterGridForUniversalCopyTest(module))
			using (var manager = new GridUniversalCopyManagerForTest(grid))
			{
				AssertEquals(typeof(UniversalCopyDummy2), manager.ElementType);
			}
		}

		public void TestCopyMenuClicked_GetSourceElement()
		{
			using (var module = new DummyModuleWithUniversalCopy())
			using (var form = new ZForm())
			using (var grid = new DummyZFilterGridForUniversalCopyTest(module))
			using (var manager = new GridUniversalCopyManagerForTest(grid))
			{
				form.Controls.Add(grid);
				grid.ReadOnly = true;
				var dummy1 = Factory.New<UniversalCopyDummy1>();
				var dummy2 = manager.CopyMenuClicked_GetSourceElement_Exposed(dummy1);

				Assert(dummy2 is UniversalCopyDummy2);
			}
		}

		#region Test classes

		public class DummyZFilterGridForUniversalCopyTest : ZFilterGrid
		{
			public DummyZFilterGridForUniversalCopyTest(ZModule parentModule)
			{
				SetParentFilterGridModule(parentModule);
				SetElementTypeForTest(typeof(DummyBusinessObject));
			}
		}

		public class DummyModuleWithUniversalCopy : ZFilterGridModule
		{
			public override bool AllowUniversalCopy
			{
				get { return true; }
			}

			public override ModuleIdentifier ID
			{
				get { return ModuleIDs.NotAssigned; }
			}

			public override SecurityCheckpoint SecurityCheckpoint
			{
				get { return new DummyCheckPointWithSecuritySet(true); }
			}

			protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
			{
				get { return (Licensing.LicenceCheckpoint)EnvProxy.Instance.Licence.Core; }
			}

			protected internal override ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				return new DummyController();
			}

			protected override IBusinessObjectCollection GetNewGridCollection()
			{
				throw new NotImplementedException();
			}

			protected override IFilterControl GetNewFilterControl()
			{
				throw new NotImplementedException();
			}

			protected override FilterBusinessObject GetNewFilterBusinessObject()
			{
				throw new NotImplementedException();
			}

			protected override void ExportIntoAndOpenExcel()
			{
				throw new NotImplementedException();
			}

			protected override void SuspendGridLayout()
			{
				throw new NotImplementedException();
			}

			protected override void ResumeGridLayout()
			{
				throw new NotImplementedException();
			}

			protected override void ForceGridPreFetch()
			{
				throw new NotImplementedException();
			}

			protected override void ExportVisibleIntoAndOpenExcel()
			{
				throw new NotImplementedException();
			}

			protected override SortInfo DefaultSortOrder
			{
				get { throw new NotImplementedException(); }
			}

			protected override BusinessObject CurrentBusinessObjectInGrid
			{
				get { throw new NotImplementedException(); }
			}

			protected override BusinessObjectReader CollectionForExport
			{
				get { throw new NotImplementedException(); }
			}

			public override ZFilterGrid DisplayGrid
			{
				get { throw new NotImplementedException(); }
			}
		}

		[UniversalCopyInstanceType(InstanceType = typeof(UniversalCopyDummy2))]
		public class DummyModuleWithInstance : DummyModuleWithUniversalCopy
		{
		}

		[UniversalCopyInstanceType(InstanceType = typeof(UniversalCopyDummy))]
		public class DummyModuleWithInstance2 : DummyModuleWithUniversalCopy
		{
			protected override IBusinessObjectCollection GetNewGridCollection()
			{
				return new DummyBusinessObjectCollection(Factory);
			}
		}

		public class DummyController : ZController
		{
			public const string FactoryNameForDebugging = "DummyControllerFactory";

			public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

			protected override IZForm GetForm(IBusiness businessEntity)
			{
				throw new NotImplementedException();
			}

			protected override BusinessObjectFactory GetNewFactory()
			{
				var factory = base.GetNewFactory();
				factory.NameForDebugging = FactoryNameForDebugging;
				return factory;
			}

			public override ControllerID ID
			{
				get { throw new NotImplementedException(); }
			}

			public override ModuleIdentifier ModuleID
			{
				get { throw new NotImplementedException(); }
			}

			public override Type TypeOfTopLevelBusinessObject
			{
				get { return typeof(DummyBusinessObject); }
			}

			protected override SecurityCheckpoint CheckPointForView
			{
				get { throw new NotImplementedException(); }
			}

			protected override SecurityCheckpoint CheckPointForNew
			{
				get { throw new NotImplementedException(); }
			}

			protected override SecurityCheckpoint CheckPointForEdit
			{
				get { throw new NotImplementedException(); }
			}

			protected override SecurityCheckpoint CheckPointForDelete
			{
				get { throw new NotImplementedException(); }
			}
		}

		class DummyModuleNoUniversalCopy : DummyModuleWithUniversalCopy
		{
			public override bool AllowUniversalCopy
			{
				get { return false; }
			}
		}

		class DummyModuleWithAllowUniversalCopyExposed : DummyModuleWithUniversalCopy
		{
			public override bool AllowUniversalCopy
			{
				get { return allowUniversalCopy; }
			}
			public bool allowUniversalCopy = true;
		}

		public class GridUniversalCopyManagerForTest : GridUniversalCopyManager
		{
			public GridUniversalCopyManagerForTest(ZGrid grid) : base(grid) { }

			internal BusinessObject CopyMenuClicked_GetSourceElement_Exposed(BusinessObject selectedElement)
			{
				return CopyMenuClicked_GetSourceElement(selectedElement);
			}

			internal BusinessObjectFactory GetDefaultFactoryForCopyManagerExposed()
			{
				return GetDefaultFactoryForCopyManager();
			}

			public (ZModule Module, bool Dispose) GetParentModuleExposed()
			{
				return GetParentModule();
			}

			public ZMenuItem UniversalCopyMenuItemsExposed => gridUniversalMenuItem;
		}

		class DummyCollectionWithOnAdded : DummyBusinessObjectCollection
		{
			public DummyCollectionWithOnAdded(BusinessObjectFactory factory) : base(factory) { }

			protected override void OnAdded(BusinessObject bizOAdded)
			{
				base.OnAdded(bizOAdded);

				WasInUC = BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(Factory);
			}

			public bool WasInUC { get; set; }
		}

		class ZFormWithGrid : ZForm
		{
			internal ZGrid Grid { get; set; }
		}

		[ModuleID(ModuleId.GlbStaff)]
		class DummyCollectionWithModuleId : BusinessObjectCollection<DummyBusinessObject>
		{
			public DummyCollectionWithModuleId(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion
	}

	class GridUniversalCopyManagerTransactionedTest : NonTransactionedTestCase
	{
		#region Thread safety

#if !WINZOR

		public void TestUniversalCopy_WhenToldToExecuteOnMainThread_DoesIt()
		{
			var asyncer = DefaultAsyncStrategy.Get();
			Action otherThreadAction = () => AssertUniversalCopyHappensOnCorrectThread(true, Thread.CurrentThread.Name);

			AssertNoExceptionThrown("We should throw no threading exceptions while executing these on a different thread", () =>
			{
				var task = asyncer.RunInAnotherWinformsThreadAsync(otherThreadAction, null, "ExpectActionExecutedOnCurrentThread");

				task.Join();
				Application.DoEvents();
			});

			using (var openedForm = Application.OpenForms.OfType<UniversalCopyTemplateForm>().First())
			{
				openedForm.Show();
				Application.DoEvents();

				AssertNotNull("We opened our universalcopy form", openedForm);
				AssertEquals("The opened thread is owned by expected thread", Thread.CurrentThread.Name, ((BusinessObject)openedForm.DataSource).Factory.ThreadSentry.OwnerThread.ThreadName);
			}
		}

		public void TestUniversalCopy_WhenNotToldToExecuteOnMainThread_DoesItNot()
		{
			var asyncer = DefaultAsyncStrategy.Get();
			var myThreadName = "ExpectActionExecutedOnCurrentThread";
			Action otherThreadAction = () => AssertUniversalCopyHappensOnCorrectThread(false, myThreadName);

			AssertNoExceptionThrown("We should throw no threading exceptions while executing these on a different thread", () =>
			{
				var task = asyncer.RunInAnotherWinformsThreadAsync(otherThreadAction, null, myThreadName);
				task.Join();

				Application.DoEvents();
			});
		}

		void AssertUniversalCopyHappensOnCorrectThread(bool forceExecutionOnMainThread, string expectedThreadName)
		{
			SystemDataRegistry.Instance.ErrorReportingFormsRunningInBackground.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, forceExecutionOnMainThread);

			using (Db.DisposableActionForDbConnection())
			using (var module = new DummyModuleWithUniversalCopy())
			{
				module.ShowFormsFromMainThread = forceExecutionOnMainThread;

				using (var form = new ZForm())
				using (var grid = new DummyZFilterGridForUniversalCopyTest(module))
				using (var manager = new GridUniversalCopyManagerForTest(grid))
				{
					form.Controls.Add(grid);
					manager.AddMenuItems();

					form.Show();
					Application.DoEvents();

					var newCopyTemplateMenuItem = manager.UniversalCopyMenuItemsExposed.MenuItems.FindByText("New Copy Template", true);
					newCopyTemplateMenuItem.PerformClick();
					Application.DoEvents();

					if (forceExecutionOnMainThread)
					{
						// To have the following form exist on the main thread when forcing forms to be shown on the main thread,
						// we need to allow the message queue for the main thread to be processed
						MainThreadRunner.RunOnMainThread(Application.DoEvents);
						MainThreadRunner.RunOnMainThread(Application.DoEvents);
					}
					else
					{
						// we want the action to execute on THIS thread, meaning we have to deal with the form that's opened
						using (var openedForm = Application.OpenForms.OfType<UniversalCopyTemplateForm>().First())
						{
							openedForm.Show();
							Application.DoEvents();

							AssertNotNull("We opened our universalcopy form", openedForm);
							AssertEquals("The opened thread is owned by expected thread", expectedThreadName, ((BusinessObject)openedForm.DataSource).Factory.ThreadSentry.OwnerThread.ThreadName);
						}

						Application.DoEvents();
					}
				}
			}
		}
#endif

		#endregion
	}
}
