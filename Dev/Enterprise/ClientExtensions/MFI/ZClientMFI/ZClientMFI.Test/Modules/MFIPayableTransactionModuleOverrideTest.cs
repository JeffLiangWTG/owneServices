using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Module.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.MFI.Testing
{
	[TestedType(typeof(MFIPayableTransactionModuleOverride))]
	sealed class MFIPayableTransactionModuleOverrideTest : APTransactionModuleTest //ZModuleBasherTest
	{
		public void TestMenuItemHasBeenAdded()
		{
			using (var aPTranModule = new TestMFIPayableTransactionModuleOverride())
			{
				var formActionMenu = aPTranModule.FormActionMenu; // will load menu items
				var handler = aPTranModule.ImportMenuItems["Import &CSV EStatement"];
				AssertNotNull("Menu item should exist", handler);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportEStatementWhenErrors()
		{
			using (var aPTranModule = new TestMFIPayableTransactionModuleOverride())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				aPTranModule.ImportEStatement(BaseSourcePath + @"Enterprise\ClientExtensions\MFI\ZClientMFI\ZClientMFI.Test\DataImportExport\TestFiles\ValidData_Sample.csv");
				Assert("There should be errors shown to the user: ", UnitTestUserNotification.Instance.LastMessage.Text.IndexOf("Error:") > 0);
			}
		}

		public new void TestMenuItemAddedCorrectly()
		{
			var aPTranModule = new TestMFIPayableTransactionModuleOverride();
			using (aPTranModule)
			{
				using (var form = new ZForm())
				{
					var menu = aPTranModule.GetNewActionMenuItems();
					var menuitem = menu.FindByText("Create Compliance Document Records");
					AssertNull(menuitem);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var menu = aPTranModule.GetNewActionMenuItems();
				var menuitem = menu.FindByText("Create Compliance Document Records");
				AssertNotNull(menuitem);
				var subMenuItem1 = menuitem.MenuItems.FindByText("Roll-up by Charge Code");
				AssertNotNull(subMenuItem1);
				var subMenuItem2 = menuitem.MenuItems.FindByText("No Roll-up");
				AssertNotNull(subMenuItem2);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.APTransaction;

		sealed class TestMFIPayableTransactionModuleOverride : MFIPayableTransactionModuleOverride
		{
			internal new FilterModuleMenuItemDescriptorCollection ImportMenuItems => base.ImportMenuItems;

			internal new void ImportEStatement(ZString fileName) => base.ImportEStatement(fileName);

			internal new MenuItem[] GetNewActionMenuItems() => base.GetNewActionMenuItems();
		}
	}
}
