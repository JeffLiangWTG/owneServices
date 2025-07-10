using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	sealed class ZDocumentMenuItemTest : ZDocumentMenuTest<ZDocumentMenuItem, MenuItem>
	{
		protected override MenuItem FindByText(ZDocumentMenuItem documentMenuItem, string text)
		{
			return documentMenuItem.MenuItems.FindByText(text);
		}

		protected override void PerformClick(MenuItem documentMenuItem)
		{
			documentMenuItem.PerformClick();
		}

		protected override void AddToMainMenu(ZForm form, ZDocumentMenuItem docMenuItem)
		{
			form.Menu.MenuItems.Add(docMenuItem);
		}

		protected override string GetText(MenuItem menuItem)
		{
			return menuItem.Text;
		}

		protected override bool GetEnabled(MenuItem menuItem)
		{
			return menuItem.Enabled;
		}

		protected override string GetShortCut(MenuItem menuItem)
		{
			return menuItem.Shortcut.ToString();
		}

		protected override IList GetItems(MenuItem menuItem)
		{
			return menuItem.MenuItems;
		}

		protected override ZDocumentsMenuItemHelper<MenuItem> HelperForTesting(ZDocumentMenuItem menuItem)
		{
			return menuItem.HelperForTesting;
		}

		protected override void ResetIsRebuildMenuItemsCalled(ZDocumentMenuItem menuItem)
		{
			menuItem.HelperForTesting.IsRebuildMenuItemsCalled = false;
		}

		protected override IList GetMainMenuCollection(ZForm form)
		{
			return form.Menu.MenuItems;
		}

		public void TestModuleIDForDocumentSecurityIsUsed()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
			using (var form = (ZForm)controller.ShowNewForm())
			using (var documentMenuItem = new ZDocumentMenuItem())
			{
				documentMenuItem.Setup(DocBusinessObject, null, null, null);
				documentMenuItem.LoadMenus(form);
				AssertNull("Pre-condition: documentMenuItem.MenuItems.FindByText(\"My Document\")", FindByText(documentMenuItem, "My Document"));

				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.SU_MenuName = "My Document";
				documentCommand.SU_IsSystemDefined = ZBool.True;
				documentCommand.SU_IsPublished = ZBool.True;
				documentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
				Factory.Save();

				var helper = HelperForTesting(documentMenuItem);

				form.Menu.MenuItems.Add(documentMenuItem);

				var runner = helper.GetNewDocumentRunner(form);

				AssertEquals(controller.ModuleIDForDocumentSecurity, runner.moduleIDForSecurity);
				AssertNotEquals(controller.ModuleID, runner.moduleIDForSecurity);
			}
		}

		public void TestParentFormCursor()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
			using (var form = (ZForm)controller.ShowNewForm())
			using (var documentMenuItem = new ZDocumentMenuItem())
			{
				var previousCursor = form.Cursor;
				documentMenuItem.Setup(DocBusinessObject, null, null, null);
				documentMenuItem.LoadMenus(form);

				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.SU_MenuName = "My Document";
				documentCommand.SU_IsSystemDefined = ZBool.True;
				documentCommand.SU_IsPublished = ZBool.True;
				documentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
				Factory.Save();

				var helper = HelperForTesting(documentMenuItem);
				form.Menu.MenuItems.Add(documentMenuItem);
				var runner = helper.GetNewDocumentRunner(form);
				runner.Run(documentCommand);

				AssertEquals("Should not change form Cursor", previousCursor, form.Cursor);
			}
		}

		public void TestLoadingTemplateCacheHaveNoExceptionThrown()
		{
			TemplateCache.UseCacheInUnitTests();

			var dummyDocSupportable = Factory.New<DummyBODocSupportable>();
			dummyDocSupportable.Z0_Description = "Test Save Documents";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "My Document";
			documentCommand.SU_IsSystemDefined = ZBool.True;
			documentCommand.SU_IsPublished = ZBool.True;
			documentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			documentCommand.Parent = dummyDocSupportable;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "System Document Elements",
				@"{A}-[#Config]
{A}-[#EndOfReport]");

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var config = pivot.DocConfigs.AddNew();

			Factory.Save();

			var runner = new DocumentRunner();

			AssertNoExceptionThrown(() => runner.Run(documentCommand));
		}

		public void TestGetControllerIDFromFilterGridModule()
		{
			using (var form = new ZForm())
			using (var documentMenuItem = new ZDocumentMenuItem())
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration))
			using (var filterGrid = new ZFilterGrid())
			{
				documentMenuItem.Setup(DocBusinessObject, null, null, null);
				form.ContextMenu = new ContextMenu();
				form.ContextMenu.MenuItems.Add(documentMenuItem);
				form.Visible = true; 

				form.Controls.Add(filterGrid);
				filterGrid.SetParentFilterGridModule(module);
				filterGrid.Visible = true;

				ControllerID controllerID;
				form.ContextMenu.Popup += delegate
				{
					var helper = HelperForTesting(documentMenuItem);
					controllerID = helper.TryGetControllerID(documentMenuItem, form);

					AssertEquals(ControllerIDs.Customs.JobDeclaration, controllerID);

					form.ContextMenu.Dispose();
				};
				form.ContextMenu.Show(filterGrid, new Point(10, 10));
			}
		}
	}
}
