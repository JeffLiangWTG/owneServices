using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(UnapprovedIntercompanyTransactionModule))]
	public class UnapprovedIntercompanyTransactionModuleTest : UnapprovedTransactionModuleTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.UnapprovedIntercompanyTransaction;
		}

		#region TestGetActionMenu

		protected override void AssertNewCopyMenuItem(UnapprovedTransactionModule module)
		{
			AssertNull("There should not be a 'new' menu item", module.NewMenuItem);
			AssertNull("There should not be a 'copy' menu item", module.CopyMenuItem);
		}

		#endregion

		public override void TestAllowNew()
		{
			using (var module = ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertEquals("AllowNew", false, module.AllowNew);
			}
		}

		public override void TestToolbarButtons()
		{
			using (var moduleToTest = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertEquals("ToolBarButtons count", 7, moduleToTest.ToolBarButtons.Length);
			}
		}

		public override void TestGetNewControllerFromCreator()
		{
			using (UnapprovedIntercompanyTransactionModule module = (UnapprovedIntercompanyTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var invoice = Factory.New<ARInvoice>();
				var controller = module.GetNewControllerFromCreator_ForTestOnly(invoice);
				AssertEquals("Should be controller for AR Invoice For InterComapny Transaction", ControllerIDs.ARInvoiceForInterCompanyTransaction, controller.ID);

				var creditNote = Factory.New<ARCreditNote>();
				controller = module.GetNewControllerFromCreator_ForTestOnly(creditNote);
				AssertEquals("Should be controller for AR CreditNote For InterComapny Transaction", ControllerIDs.ARCreditNoteForInterCompanyTransaction, controller.ID);
			}
		}

		public void TestDocumentMenuIsRemovedFromGridContextMenu()
		{
			using (var testModule = (UnapprovedIntercompanyTransactionModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				testModule.DisplayGrid.ContextMenu.MenuItems.Add("Documents");
				testModule.ContextMenu_Popup_ForTestOnly(this, null);
				var doesDocumentsMenuExist = false;
				for (int i = 0; i < testModule.DisplayGrid.ContextMenu.MenuItems.Count; i++)
				{
					if (testModule.DisplayGrid.ContextMenu.MenuItems[i].Text == "Documents")
					{
						doesDocumentsMenuExist = true;
					}
				}
				Assert("Menu Item 'Documents' must be deleted", !doesDocumentsMenuExist);
			}
		}
	}
}
