using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NotificationDataRegistry))]
	sealed class NotificationDataRegistryTest : RegistryItemSetTestCaseWithFactory<NotificationDataRegistry>
	{
		public void TestBankStatementImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Bank Statement Import Notification Group", ItemSet.BankStatementImportNotificationGroup, AllUsersGroupPK, Guid.NewGuid());
		}

		public void TestUpdateAPARAccountBalancesProcessNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Update Account Balances Import Notification Group", ItemSet.UpdateAPARAccountBalancesProcessNotificationGroupItem, AllUsersGroupPK, Guid.NewGuid());
		}

		public void TestOrganisationImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Organisation Import Notification Group", ItemSet.OrganisationImportNotificationGroup, AllUsersGroupPK, Guid.NewGuid());
		}

		public void TestFaxDeliveryFailuresFallback()
		{
			SetAndAssertItemValue("FaxDeliveryFailuresFallback", ItemSet.FaxDeliveryFailuresFallback, Guid.NewGuid());
		}

		public void TestPODImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("POD Import Notification Group", ItemSet.PODImportNotificationGroup, AllUsersGroupPK, Guid.NewGuid());
		}

		public void TestProductImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Product Import Notification Group", ItemSet.ProductImportNotificationGroup, AllUsersGroupPK, Guid.NewGuid());
		}

		public void TestScheduleImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Schedules Import Notification Group", ItemSet.SchedulesImportNotificationGroup, AllUsersGroupPK, Guid.NewGuid());
		}

		public void TestImportedOrderChangesNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Imported Order Changes Notification Group", ItemSet.ImportedOrderChangesNotificationGroup, Guid.Empty, Guid.NewGuid());
		}

		public void TestEDIMessageDeliveryFailNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("EDIMessageDelivery Fail Notification Group", ItemSet.EDIMessageDeliveryFailNotificationGroup, AllUsersGroupPK, Guid.NewGuid());
		}

		public void TestCommercialInvoiceImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Commercial Invoice Import Notification Group", ItemSet.CommercialInvoiceImportNotificationGroup, AllUsersGroupPK, Guid.NewGuid());
		}

		public void TestContainerEventsImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("ContainerEvents Import Notification Group", ItemSet.ContainerEventsImportNotificationGroup, AllUsersGroupPK, Guid.NewGuid());
		}

		public void TestContainerMovementImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Container Movement Import Notification Group", ItemSet.ContainerMovementImportNotificationGroup, AllUsersGroupPK, Guid.NewGuid());
			SetAndAssertItemValue("Container Movement Import Notification Group", ItemSet.ContainerMovementImportNotificationGroup, Guid.Empty);
		}

		public void TestCustomsDeclarationImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Customs Declaration Import Notification Group", ItemSet.CustomsDeclarationImportNotificationGroup, AllUsersGroupPK, Guid.NewGuid());
		}

		public void TestConsolShipmentImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Consol Shipment Import Notification Group", ItemSet.ConsolShipmentImportNotificationGroup, Core.Constants.Groups.AllPK, Guid.NewGuid());
		}

		public void TestBookingImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Booking Import Notification Group", ItemSet.BookingImportNotificationGroup, Core.Constants.Groups.AllPK, Guid.NewGuid());
		}

		public void TestWarehouseImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Warehouse Import Notification Group", ItemSet.WarehouseImportNotificationGroup, Core.Constants.Groups.AllPK, Guid.NewGuid());
		}

		public void TestOrderImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Order Import Notification Group", ItemSet.OrderImportNotificationGroup, Core.Constants.Groups.AllPK, Guid.NewGuid());
		}

		public void TestAgencyBillOfLadingImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("AgencyBillOfLading Import Notification Group", ItemSet.AgencyBillOfLadingImportNotificationGroup, Core.Constants.Groups.AllPK, Guid.NewGuid());
		}

		public void TestNettingClearingJournalImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Netting Clearing Journal Import Notification Group", ItemSet.NettingClearingJournalImportNotificationGroup, Core.Constants.Groups.AllPK, Guid.NewGuid());
		}

		public void TestLocalCartageImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Local Cartage Notification Group", ItemSet.LocalCartageNotificationGroup, Core.Constants.Groups.AllPK, Guid.NewGuid());
		}

		public void TestEventImportNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Event Import Notification Group", ItemSet.EventImportNotificationGroup, Core.Constants.Groups.AllPK, Guid.NewGuid());
		}

		public void TestDatabaseBackupAndMaintenanceNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Database Backup and Maintenance Notification Group", ItemSet.DatabaseBackupAndMaintenanceNotificationGroup, Core.Constants.Groups.PostMastersGroupPK, Guid.NewGuid());
		}

		public void TestSystemServiceTasksNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("System Service Tasks Notification Group", ItemSet.SystemServiceTasksNotificationGroup, Core.Constants.Groups.PostMastersGroupPK, Guid.NewGuid());
		}

		public void TestDatabaseHealthCheckNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Database Health Check Notification Group", ItemSet.DatabaseHealthCheckNotificationGroup, Core.Constants.Groups.PostMastersGroupPK, Guid.NewGuid());
		}

		public void TestBiAdminNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("BI Admin Notification Group", ItemSet.BiAdminNotificationGroup, Core.Constants.Groups.PostMastersGroupPK, Guid.NewGuid());
		}

		public void TestXMLPostingFailureNotificationGroup()
		{
			AssertItemDefaultValueAndNewValue("Transactions Pending Allocation XML Posting Failure Notification Group", ItemSet.TransactionsPendingAllocationXMLPostingFailureNotificationGroup, Core.Constants.Groups.AllPK, Guid.NewGuid());
		}

		#region ProductivityWise

		public void TestIsProperlyPWVisible_UpdateAPARAccountBalancesProcessNotificationGroupItem()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			Assert("We expect this registry item to not be hidden, but instead...", ItemSet.XMSFailureFallBackNotificationGroup.Options != RegistryOptions.IsHidden);

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			Assert("We expect this registry item to not be hidden, but instead...", ItemSet.XMSFailureFallBackNotificationGroup.Options != RegistryOptions.IsHidden);
		}

		public void TestIsProperlyPWVisible_FinancialTransactionImportNotificationGroup()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			Assert("We expect this registry item to not be hidden, but instead...", ItemSet.XMSFailureFallBackNotificationGroup.Options != RegistryOptions.IsHidden);

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			Assert("We expect this registry item to not be hidden, but instead...", ItemSet.XMSFailureFallBackNotificationGroup.Options != RegistryOptions.IsHidden);
		}

		public void TestIsProperlyPWVisible_NettingClearingJournalImportNotificationGroup()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			Assert("We expect this registry item to not be hidden, but instead...", ItemSet.XMSFailureFallBackNotificationGroup.Options != RegistryOptions.IsHidden);

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			Assert("We expect this registry item to not be hidden, but instead...", ItemSet.XMSFailureFallBackNotificationGroup.Options != RegistryOptions.IsHidden);
		}

		public void TestIsProperlyPWVisible_BankStatementImportNotificationGroup()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			Assert("We expect this registry item to not be hidden, but instead...", ItemSet.XMSFailureFallBackNotificationGroup.Options != RegistryOptions.IsHidden);

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			Assert("We expect this registry item to not be hidden, but instead...", ItemSet.XMSFailureFallBackNotificationGroup.Options != RegistryOptions.IsHidden);
		}

		public void TestIsProperlyPWVisible_EventImportNotificationGroup()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			Assert("We expect this registry item to not be hidden, but instead...", ItemSet.XMSFailureFallBackNotificationGroup.Options != RegistryOptions.IsHidden);

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			Assert("We expect this registry item to not be hidden, but instead...", ItemSet.XMSFailureFallBackNotificationGroup.Options != RegistryOptions.IsHidden);
		}

		public void TestIsProperlyPWVisible_XMSFailureFallBackNotificationGroup()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			Assert("We expect this registry item to not be hidden, but instead...", ItemSet.XMSFailureFallBackNotificationGroup.Options != RegistryOptions.IsHidden);

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			Assert("We expect this registry item to not be hidden, but instead...", ItemSet.XMSFailureFallBackNotificationGroup.Options != RegistryOptions.IsHidden);
		}

		#endregion

		Guid AllUsersGroupPK;
		protected override void SetUp()
		{
			base.SetUp();
			AllUsersGroupPK = RegistryFactory.Instance.GetGroupPK("ALL");
		}
	}
}
