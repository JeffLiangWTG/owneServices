using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.DeviceManagement.GUI;
using Enterprise.Client.EDI.Modules;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Module.Testing
{
	[TestedType(typeof(ClientDeviceHeaderModule))]
	public class ClientDeviceHeaderModuleTest : ZModuleBasherTest
	{
		public void TestNewMenuItems()
		{
			TestCaseHelper.ClearTable(DmgDeviceComponentIdentificationSchema.Constants.TableName);
			TestCaseHelper.ClearTable(DmgDeviceComponentSchema.Constants.TableName);
			TestCaseHelper.ClearTable(DmgDeviceHeaderSchema.Constants.TableName);
			var device1 = Factory.New<ClientDeviceHeader>();
			device1.CDH_IsTemplate = true;
			device1.CDH_ModelID = "MD1";
			device1.CDH_Description = "Model 1";
			var device2 = Factory.New<ClientDeviceHeader>();
			device2.CDH_IsTemplate = true;
			device2.CDH_ModelID = "ABC";
			device2.CDH_Description = "Model 2";
			var device3 = Factory.New<ClientDeviceHeader>();
			device3.CDH_ModelID = "XYZ";
			device3.CDH_Description = "Not a model";
			Factory.Save();
			using (var module = new ClientDeviceHeaderModule())
			{
				var newMenuItem = module.FormActionMenu.FindByText("New");
				AssertEquals("New", newMenuItem.Text);
				AssertEquals(2, newMenuItem.MenuItems.Count);
				AssertEquals("ABC - Model 2", newMenuItem.MenuItems[0].Text);
				AssertEquals("MD1 - Model 1", newMenuItem.MenuItems[1].Text);
				newMenuItem.PerformClick();
				AssertNull(ClientDeviceMenuItemGenerator.LastControllerForTesting);
				newMenuItem.MenuItems[0].PerformClick();
				var lastShownForm = (ZForm)ClientDeviceMenuItemGenerator.LastControllerForTesting.LastShownForm;
				try
				{
					AssertNotNull(lastShownForm);
					AssertEquals("ABC", ((ClientDeviceHeader)lastShownForm.BusinessEntity).CDH_ModelID);
					AssertEquals("Model 2", ((ClientDeviceHeader)lastShownForm.BusinessEntity).CDH_Description);
					AssertNotEquals(device2.PK, ((ClientDeviceHeader)lastShownForm.BusinessEntity).PK);
				}
				finally
				{
					ClientDeviceMenuItemGenerator.LastControllerForTesting = null;
					lastShownForm.Dispose();
					newMenuItem.Dispose();
				}
			}
		}

		public void TestOperationalActions()
		{
			using (var module = new ClientDeviceHeaderModule())
			{
				Assert("Module should implement IOperationalActionSupportable to support Operational Actions.", module is IOperationalActionSupportable);
				var oas = (IOperationalActionSupportable)module;
				AssertType<ClientDeviceHeaderOperationalActionSupporter>(oas.OperationalActionSupporter);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ClientModuleRegistration.ClientDevice;
	}
}
