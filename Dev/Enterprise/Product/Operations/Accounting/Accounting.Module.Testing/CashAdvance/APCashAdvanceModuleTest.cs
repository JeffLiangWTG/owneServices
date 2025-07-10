using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APCashAdvanceModule))]
	public class APCashAdvanceModuleTest : CashAdvanceModuleTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.APCashAdvance;
		}

		public void TestAPCashAdvanceModule_SecurityCheckpoint_ShouldEqualAPCashAdvance()
		{
			using (var module = new APCashAdvanceModuleForTest())
			{
				AssertEquals(Env.Security.APCashAdvance, module.SecurityCheckpoint);
			}
		}

		public void TestAPCashAdvanceModule_NewFilterControl_ShouldBeCashAdvanceFilterControl()
		{
			using (var module = new APCashAdvanceModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is CashAdvanceFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestAPCashAdvanceModule_NewGridCollection_ShouldBeAccCashAdvanceRequestHeaderCollection()
		{
			using (var module = new APCashAdvanceModuleForTest())
			{
				IBusinessObjectCollection cashAdvanceRequestHeaderCollection = module.NewGridCollection;
				Assert("Invalid type", cashAdvanceRequestHeaderCollection is AccCashAdvanceRequestHeaderCollection);
			}
		}

		public void TestAPCashAdvanceModule_NewFilterBusinessObject_ShouldBeAPCashAdvanceFilterBusinessObject()
		{
			using (var module = new APCashAdvanceModuleForTest())
			{
				var filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is APCashAdvanceFilterBusinessObject);
			}
		}

		public void TestAPCashAdvanceModule_ActionMenuItems_WithSecurityRights()
		{
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var module = new APCashAdvanceModuleForTest())
				{
					var actionMenuItems = module.NewActionMenuItems;
					AssertNotNull("There should be 'Cancel' menu item", actionMenuItems.FindByText("Cancel"));
					AssertNotNull("There should be 'Mark as Paid' menu item", actionMenuItems.FindByText("Mark as Paid"));
					AssertNotNull("There should be 'Mark as Unpaid' menu item", actionMenuItems.FindByText("Mark as Unpaid"));
					AssertNull("There should not be 'Print' menu item", actionMenuItems.FindByText("Print"));
				}
			}
		}

		public void TestAPCashAdvanceModule_ActionMenuItems_WithoutSecurityRights()
		{
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var module = new APCashAdvanceModuleForTest())
				{
					var actionMenuItems = module.NewActionMenuItems;
					AssertNotNull("There should be 'Cancel' menu item", actionMenuItems.FindByText("Cancel"));
					AssertNull("There should not be 'Mark as Paid' menu item", actionMenuItems.FindByText("Mark as Paid"));
					AssertNull("There should not be 'Mark as Unpaid' menu item", actionMenuItems.FindByText("Mark as Unpaid"));
					AssertNull("There should not be 'Print' menu item", actionMenuItems.FindByText("Print"));
				}
			}
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var bizo = factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			bizo.CAH_GC_Company = Env.CurrentCompany.PK;
			return bizo;
		}

		class APCashAdvanceModuleForTest : APCashAdvanceModule
		{
			public APCashAdvanceModuleForTest()
			{ }

			public IFilterControl NewFilterControl => GetNewFilterControl();

			public IBusinessObjectCollection NewGridCollection => GetNewGridCollection();

			public FilterBusinessObject NewFilterBusinessObject => GetNewFilterBusinessObject();

			public MenuItem[] NewActionMenuItems => GetNewActionMenuItems();

			public void HandlePrint_ForTestOnly(object sender, EventArgs e)
			{
				HandlePrint(sender, e);
			}
		}
	}
}
