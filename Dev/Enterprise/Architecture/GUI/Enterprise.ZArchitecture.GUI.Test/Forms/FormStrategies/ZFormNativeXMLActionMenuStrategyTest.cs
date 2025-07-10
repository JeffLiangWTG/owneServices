using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.FormStrategies
{
	public class ZFormNativeXMLActionMenuStrategyTest : TestCaseWithFactory
	{
		public void TestAddAdornments_Normal()
		{
			using (form = new ZForm(businessObject.Object))
			{
				exportValidator.Setup(o => o.CanBeExported(It.IsAny<Type>())).Returns(true);
				strategy.ExportValidator = exportValidator.Object;
				strategy.Exporter = exportService.Object;

				var menuItemsProvider = form as IFileMenuItemsProvider;
				var beforeCount = menuItemsProvider.ActionsMenuItem.MenuItems.Count;

				strategy.AddAdornments(form);

				var afterCount = menuItemsProvider.ActionsMenuItem.MenuItems.Count;
				AssertEquals("It should add MenuItem to ActionMenu", beforeCount + 1, afterCount);
			}
		}

		public void TestNativeXMLMenuIsDisabled()
		{
			exportValidator.Setup(o => o.CanBeExported(It.IsAny<Type>())).Returns(true);
			strategy.ExportValidator = exportValidator.Object;
			strategy.Exporter = exportService.Object;

			var menuItem = strategy.CreateExportMenuItem(businessObject.Object, null);
			AssertEquals(ExportXmlMenuItemHelper.NativeMenuItemText, menuItem.Text);
			menuItem.Dispose();

			exportValidator.Setup(o => o.CanBeExported(It.IsAny<Type>())).Returns(false);
			menuItem = strategy.CreateExportMenuItem(businessObject.Object, null);
			AssertNull(menuItem);
		}

		public void TestNativeXMLMenuHandler()
		{
			using (var module = new DummyModule())
			{
				ZCurrentModules.Instance.SetCurrentModule(module);
				exportValidator.Setup(o => o.CanBeExported(It.IsAny<Type>())).Returns(true);
				strategy.ExportValidator = exportValidator.Object;
				strategy.Exporter = exportService.Object;
				var controller = new DummyController();

				var menuItem = strategy.CreateExportMenuItem(Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>(), controller.ID);
				AssertEquals(typeof(ImportSecurityChecker), strategy.NativeXmlMenuItems[ExportXmlMenuItemHelper.NativeMenuItemText].Target.GetType());
			}
		}

		public void TestExportMenuSecurityCheck_FormWithoutCurrentModule_SecurityAllowed()
		{
			var strategy = new ZFormNativeXMLActionMenuStrategyForTest();
			strategy.CheckpointOverride = true;

			exportValidator.Setup(o => o.CanBeExported(It.IsAny<Type>())).Returns(true);
			strategy.ExportValidator = exportValidator.Object;
			strategy.Exporter = exportService.Object;
			var controller = new DummyController();

			var menuItem = strategy.CreateExportMenuItem(Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>(), controller.ID);
			menuItem.PerformClick();

			Assert("Export Native XML Security Checked", strategy.GetSecurityCheckerCoreCalled);
			Assert("Export Native XML Core Executed", strategy.ExportCoreCalled);
		}

		public void TestExportMenuSecurityCheck_FormWithoutCurrentModule_SecurityDenied()
		{
			var strategy = new ZFormNativeXMLActionMenuStrategyForTest();
			strategy.CheckpointOverride = false;

			exportValidator.Setup(o => o.CanBeExported(It.IsAny<Type>())).Returns(true);
			strategy.ExportValidator = exportValidator.Object;
			strategy.Exporter = exportService.Object;
			var controller = new DummyController();

			var menuItem = strategy.CreateExportMenuItem(Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>(), controller.ID);
			menuItem.PerformClick();

			Assert("Export Native XML Security Checked", strategy.GetSecurityCheckerCoreCalled);
			Assert("Export Native XML Core Not Executed", !strategy.ExportCoreCalled);
		}

		public void TestExportMenuSecurityCheck_FormWithCurrentModule_SecurityAllowed()
		{
			using (var module = new DummyModule())
			{
				var strategy = new ZFormNativeXMLActionMenuStrategyForTest();
				strategy.CheckpointOverride = true;

				exportValidator.Setup(o => o.CanBeExported(It.IsAny<Type>())).Returns(true);
				strategy.ExportValidator = exportValidator.Object;
				strategy.Exporter = exportService.Object;
				var controller = new DummyController();

				var menuItem = strategy.CreateExportMenuItem(Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>(), controller.ID);
				menuItem.PerformClick();

				Assert("Export Native XML Security Checked", strategy.GetSecurityCheckerCoreCalled);
				Assert("Export Native XML Core Executed", strategy.ExportCoreCalled);
			}
		}

		public void TestExportMenuSecurityCheck_FormWithCurrentModule_SecurityDenied()
		{
			using (var module = new DummyModule())
			{
				var strategy = new ZFormNativeXMLActionMenuStrategyForTest();
				strategy.CheckpointOverride = false;

				exportValidator.Setup(o => o.CanBeExported(It.IsAny<Type>())).Returns(true);
				strategy.ExportValidator = exportValidator.Object;
				strategy.Exporter = exportService.Object;
				var controller = new DummyController();

				var menuItem = strategy.CreateExportMenuItem(Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>(), controller.ID);
				menuItem.PerformClick();

				Assert("Export Native XML Security Checked", strategy.GetSecurityCheckerCoreCalled);
				Assert("Export Native XML Core Not Executed", !strategy.ExportCoreCalled);
			}
		}

		public void TestAddAdornments_WhenThereIsNoBusinessObject()
		{
			using (form = new ZForm(null))
			{
				var menuItemsProvider = form as IFileMenuItemsProvider;

				strategy.Exporter = exportService.Object;

				var beforeCount = menuItemsProvider.ActionsMenuItem.MenuItems.Count;

				strategy.AddAdornments(form);

				var afterCount = menuItemsProvider.ActionsMenuItem.MenuItems.Count;
				AssertEquals("It should not add adornments", beforeCount, afterCount);
			}
		}

		public void TestAddAdornments_WhenBusinessObjectCanNotBeExported()
		{
			using (form = new ZForm(businessObject.Object))
			{
				exportValidator.Setup(o => o.CanBeExported(It.IsAny<Type>())).Returns(false);
				strategy.ExportValidator = exportValidator.Object;
				strategy.Exporter = exportService.Object;

				var menuItemsProvider = form as IFileMenuItemsProvider;
				var beforeCount = menuItemsProvider.ActionsMenuItem.MenuItems.Count;

				strategy.AddAdornments(form);

				var afterCount = menuItemsProvider.ActionsMenuItem.MenuItems.Count;
				AssertEquals("It Should Not Add Adornments", beforeCount, afterCount);
			}
		}

		public void TestAddAdornments_WhenMenuItemAlreadyExisted()
		{
			using (form = new ZForm(businessObject.Object))
			{
				exportValidator.Setup(o => o.CanBeExported(It.IsAny<Type>())).Returns(true);
				strategy.ExportValidator = exportValidator.Object;
				strategy.Exporter = exportService.Object;

				var menuItemsProvider = form as IFileMenuItemsProvider;
				menuItemsProvider.ActionsMenuItem.MenuItems.Add(ExportXmlMenuItemHelper.NativeMenuItemText, (o, e) => { }).Name = ExportXmlMenuItemHelper.NativeExportMenuItemName;

				var beforeCount = menuItemsProvider.ActionsMenuItem.MenuItems.Count;

				strategy.AddAdornments(form);

				var afterCount = menuItemsProvider.ActionsMenuItem.MenuItems.Count;
				AssertEquals("It Should Not Add Adornments", beforeCount, afterCount);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			businessObject = new Mock<IBusiness>();
			exportService = new Mock<IExportService>();
			exportValidator = new Mock<IExportValidator>();
			strategy = new ZFormNativeXMLActionMenuStrategy();
		}

		Mock<IBusiness> businessObject;
		Mock<IExportService> exportService;
		Mock<IExportValidator> exportValidator;
		ZFormNativeXMLActionMenuStrategy strategy;
		ZForm form;

		class ZFormNativeXMLActionMenuStrategyForTest : ZFormNativeXMLActionMenuStrategy
		{
			internal bool ExportCoreCalled { get; set; }

			internal bool GetSecurityCheckerCoreCalled { get; set; }

			internal bool? CheckpointOverride { get; set; }

			protected override void ExportCore(IEnumerable<IBusiness> bizos)
			{
				ExportCoreCalled = true;
			}

			protected override ImportSecurityChecker GetSecurityCheckerCore(IEnumerable<IBusiness> bizos, SecurityCheckpoint checkpoint)
			{
				GetSecurityCheckerCoreCalled = true;
				if (CheckpointOverride != null)
				{
					checkpoint = new SecurityCheckpoint(checkpoint.Code, checkpoint.DisplayText, checkpoint.Parent, EnvProxy.Instance.Security, true, string.Empty, Guid.NewGuid());
					checkpoint.IsAllowed = CheckpointOverride.Value;
				}
				return base.GetSecurityCheckerCore(bizos, checkpoint);
			}
		}

		#endregion
	}
}
