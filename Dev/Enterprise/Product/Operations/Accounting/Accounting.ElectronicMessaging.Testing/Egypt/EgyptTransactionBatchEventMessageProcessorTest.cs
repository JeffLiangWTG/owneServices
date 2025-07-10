using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Egypt.Testing
{
	[TestedType(typeof(EgyptTransactionBatchEventMessageProcessor))]
	sealed class EgyptTransactionBatchEventMessageProcessorTest : ElectronicMessagingGlobalEInvoicingTest_AR
	{
		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithNewPivotStatusDelivered_WhenEnableReceivingEInvoiceStatusNotificationIsFalse()
		{
			DisableReceivingEInvoiceStatusNotification();
			AssertIAKMessage_WithNewPivotStatusDelivered(EInvoicingPivotState.Succeed);
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithNewPivotStatusSucceeded_WhenEnableReceivingEInvoiceStatusNotificationIsFalse()
		{
			DisableReceivingEInvoiceStatusNotification();
			TestIAKMessage_WithNewPivotStatusSucceeded();
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithNewPivotStatusDiscarded_WhenEnableReceivingEInvoiceStatusNotificationIsFalse()
		{
			DisableReceivingEInvoiceStatusNotification();
			TestIAKMessage_WithNewPivotStatusDiscarded();
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithNewPivotStatusFailure_WhenEnableReceivingEInvoiceStatusNotificationIsFalse()
		{
			DisableReceivingEInvoiceStatusNotification();
			TestIAKMessage_WithNewPivotStatusFailure();
		}

		protected override void SetUp()
		{
			AccountingElectronicMessagingRegistry.Instance.EnableReceivingEInvoiceStatusNotification
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), branchPk: default, departmentPk: default, temporaryValue: true);

			base.SetUp();
		}

		protected override TransactionBatchEventMessageProcessor CreateTransactionBatchEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			=> new EgyptTransactionBatchEventMessageProcessor(eventMessageProcessorData, countryEInvoicingObjectFactory);

		void DisableReceivingEInvoiceStatusNotification()
			=> AccountingElectronicMessagingRegistry.Instance.EnableReceivingEInvoiceStatusNotification
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), branchPk: default, departmentPk: default, temporaryValue: false);
	}
}
