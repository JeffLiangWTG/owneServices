using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARCashAdvanceModule))]
	public class ARCashAdvanceModuleTest : CashAdvanceModuleTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ARCashAdvance;
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new ARCashAdvanceModuleForTest())
			{
				AssertEquals(Env.Security.ARCashAdvance, module.SecurityCheckpoint);
			}
		}

		public void TestGetNewFilterControl()
		{
			using (var module = new ARCashAdvanceModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is CashAdvanceFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new ARCashAdvanceModuleForTest())
			{
				IBusinessObjectCollection cashAdvanceRequestHeaderCollection = module.NewGridCollection;
				Assert("Invalid type", cashAdvanceRequestHeaderCollection is AccCashAdvanceRequestHeaderCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new ARCashAdvanceModuleForTest())
			{
				var filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is ARCashAdvanceFilterBusinessObject);
			}
		}

		public void TestActionMenuItemsWithSecurityRights()
		{
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var module = new ARCashAdvanceModuleForTest())
				{
					var actionMenuItems = module.NewActionMenuItems;
					AssertNotNull("There should be 'Cancel' menu item", actionMenuItems.FindByText("Cancel"));
					AssertNotNull("There should be 'Mark as Paid' menu item", actionMenuItems.FindByText("Mark as Paid"));
					AssertNotNull("There should be 'Mark as Unpaid' menu item", actionMenuItems.FindByText("Mark as Unpaid"));
					AssertNotNull("There should be 'Print' menu item", actionMenuItems.FindByText("Print"));
				}
			}
		}

		public void TestActionMenuItemsWithOutSecurityRights()
		{
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var module = new ARCashAdvanceModuleForTest())
				{
					var actionMenuItems = module.NewActionMenuItems;
					AssertNotNull("There should be 'Cancel' menu item", actionMenuItems.FindByText("Cancel"));
					AssertNull("There should not be 'Mark as Paid' menu item", actionMenuItems.FindByText("Mark as Paid"));
					AssertNull("There should not be 'Mark as Unpaid' menu item", actionMenuItems.FindByText("Mark as Unpaid"));
					AssertNotNull("There should be 'Print' menu item", actionMenuItems.FindByText("Print"));
				}
			}
		}

		AccCashAdvanceRequestHeader CreateCashAdvanceRequest(ZString jobNumber, ZString cashAdvanceNumber, OrgHeader org)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var header = Factory.NewJobForTesting<Job>();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_JobNum = jobNumber;
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var request = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			request.CAH_RequestReferenceNumber = cashAdvanceNumber;
			request.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			request.CAH_Ledger = LedgerTypes.AccountsReceivable;
			request.CAH_OH_Organization = org.PK;
			request.CAH_JH_Job = header.PK;
			request.CAH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			request.CAH_OSAmount = 100m;
			request.CAH_LocalAmount = 100m;
			request.CAH_OSPaidAmount = 0m;
			request.CAH_LocalPaidAmount = 0m;
			request.CAH_GC_Company = GlbCompany.CurrentCompany.PK;
			return request;
		}

		public void TestInactiveOrgDoesNotAllowPrinting()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_IsActive = false;
			newOrg.OH_IsDebtor = true;
			newOrg.OH_IsCreditor = true;

			CreateCashAdvanceRequest("00001000", "00001001", newOrg);
			Factory.Save();
			Assert("Precondition: no messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);

			using (var module = new ARCashAdvanceModuleForTest())
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					module.HandlePrint_ForTestOnly(this, EventArgs.Empty);
					AssertEquals(@"The following Advance Payment request(s) cannot be printed
Advance Payment request 00001001 cannot be printed because it is for an inactive organization
", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCancelledReqeustNotAllowPrinting()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_IsDebtor = true;
			newOrg.OH_IsCreditor = true;

			var request = CreateCashAdvanceRequest("00001000", "00001001", newOrg);
			request.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Cancelled;
			Factory.Save();
			Assert("Precondition: no messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);

			using (var module = new ARCashAdvanceModuleForTest())
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					module.HandlePrint_ForTestOnly(this, EventArgs.Empty);
					AssertEquals(@"The following Advance Payment request(s) cannot be printed
Advance Payment request 00001001 cannot be printed because it is canceled
", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPrintingMultipleRequests()
		{
			var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			mockIPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(
				It.IsAny<PrintTask>(),
				It.IsAny<DeliveryInstructions>(),
				It.IsAny<ISecurityCheckpoint>()))
				.Returns(false);
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
			using (var module = new ARCashAdvanceModuleForTest())
			{
				using (ZForm form = new ZForm())
				{
					var newOrg = Factory.NewWithValidTestData<OrgHeader>();
					newOrg.OH_IsDebtor = true;
					newOrg.OH_IsCreditor = true;

					CreateCashAdvanceRequest("00001000", "00001001", newOrg);
					CreateCashAdvanceRequest("00002000", "00001002", newOrg);
					Factory.Save();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					module.PerformSearch_ForTest();

					module.DisplayGrid.SelectAllElements();
					AssertEquals(2, module.DisplayGrid.SelectedElements.Length);

					module.HandlePrint_ForTestOnly(this, EventArgs.Empty);
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			mockIPrintTaskUIProvider.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestPrintingDoesNotThrowExceptionsWithNoTransactionResult()
		{
			using (var module = new ARCashAdvanceModuleForTest())
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					module.PerformSearch_ForTest();
					AssertEquals(0, module.DisplayGrid.SelectedElements.Length);
					module.HandlePrint_ForTestOnly(this, EventArgs.Empty);
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var bizo = factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			bizo.CAH_GC_Company = Env.CurrentCompany.PK;
			return bizo;
		}

		class ARCashAdvanceModuleForTest : ARCashAdvanceModule
		{
			public ARCashAdvanceModuleForTest()
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
