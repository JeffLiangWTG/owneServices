using System.Collections;
using System.Windows.Forms;
using Enterprise.DbUpgrader.Data.Testing;
using Enterprise.DocumentEngine.Build;
using Enterprise.DocumentEngine.Build.Testing;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	class DocumentCustomisationMenusMakerTest : DocumentCustomisationMenusMakerTest<MenuItem, DocumentCustomisationMenuItemMenusMaker>
	{
		protected override IList GetMenuItems(Form form)
		{
			return form.Menu.MenuItems;
		}

		protected override DocumentCustomisationMenuItemMenusMaker GetNewDocumentCustomisationMenusMaker(Form form, IDocumentSupportable documentSupportable)
		{
			return new DocumentCustomisationMenuItemMenusMakerForTest(form, documentSupportable, null, new ZDocumentsMenuItemMenuHelper());
		}

		protected override void PerformClick(MenuItem menuItem)
		{
			menuItem.PerformClick();
		}

		protected override string GetText(MenuItem menuItem)
		{
			return menuItem.Text;
		}

		protected override string[] GetMenuAsString(MenuItem menuItem)
		{
			return new string[] { GetText(menuItem), menuItem.Checked.ToString() };
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMarkAllEditableInReportForm()
		{
			DocumentTablesCleaner.Clean();
			var businessEntity = new ReportMenuCustomisation(Factory, "RepRefFilesReports");

			using (EnableDebugOnlyMenuItemsForTesting())
			using (var form = new ReportCustomisationForm(businessEntity))
			{
				form.BusinessEntity.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;

				var maker = GetNewDocumentCustomisationMenusMaker(form, businessEntity);
				maker.Make(GetMenuItems(form));
				PerformClick(maker.MarkAllEditableMenu);
				AssertEquals("The reports should be editable after clicking Mark All Reports Editable", MenuEditingMode.AllowEditingOfSystemDefinedOnly, form.BusinessEntity.EditingMode);
			}
		}

		class DocumentCustomisationMenuItemMenusMakerForTest : DocumentCustomisationMenuItemMenusMaker
		{
			internal DocumentCustomisationMenuItemMenusMakerForTest(Form parentForm, IDocumentSupportable documentSupportable, UserControlProviderList userFieldList, ZDocumentsMenuItemMenuHelper helper) : base(parentForm, documentSupportable, userFieldList, helper)
			{
			}

			protected override DocumentsSetupController GetNewDocumentsSetupController() => new DocumentsSetupControllerTest.DocumentsTestSetupController();
		}
	}
}
