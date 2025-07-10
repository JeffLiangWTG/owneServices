using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Taiwan.Testing
{
	public class EDIInterchangeCreatorForTaiwanTest : GEIEDIInterchangeCreatorTest
	{
		public override void TestIsBillingSupported()
		{
			var mockEDIInterchangeCreator = GetInterchangeProcessor() as MockEDIInterchangeCreatorForTaiwanEInvoicingBatch;
			Assert("Billing transactions are created in Taiwan.", mockEDIInterchangeCreator.GetIsBillingSupported());
		}

		public void TestLinkEventWithBatchAndComplianceDocument()
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var complianceDocument = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arInvoice.Lines[0], TestObjectCreator.Debtor);
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var logger = new TestServiceLogger();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
				var processor = new MockEDIInterchangeCreatorForTaiwanEInvoicingBatch(GlbCompany.CurrentCompany, converter, null);
				processor.Process(logger);
			}

			var dataExportLog = complianceDocument.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExport.Code)).SingleOrDefault();
			AssertNotNull("event linked to compliance document", dataExportLog);
			AssertEquals("event linked to invoice", "Purpose: E-Reporting Transaction Exported. Batch : 100", dataExportLog.SL_Reference);
			AssertEquals("event linked to batch", "DEX", batch.Logs.MostRecentLog.SL_SE_NKEvent);
			AssertEquals("event linked to batch", "Purpose: E-Reporting Transaction Exported. Batch : 100", batch.Logs.MostRecentLog.SL_Reference);
		}

		public override void TestProcessPartOfBatchesFailed()
		{
			var notificationGroupPK = Helper.CreateNotificationGroup("Test User 2", "company2user@abc.com");
			Factory.Save();

			var batch1 = GetBatchWithReadyStatus();
			var batch2 = GetBatchWithReadyStatus();

			var logger = new TestServiceLogger();
			AssertEquals(EInvoicingBatchState.Ready, batch1.AIB_Status);
			AssertEquals(EInvoicingBatchState.Ready, batch2.AIB_Status);

			var ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals(0, ediInterchanges.Length);
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
				var processor = GetInterchangeProcessor(converter, ExceptionTypes.NotifcationErrorDuringEDIMessageCreation, batch2);
				processor.Process(logger);

				//Batch 1
				AssertEquals(EInvoicingBatchState.Sent, batch1.AIB_Status);
				AssertEquals(EInvoicingPivotState.Sent, batch1.TransactionPivots[0].AIP_Status);

				//Batch 2
				AssertEquals(EInvoicingBatchState.Ready, batch2.AIB_Status);
				AssertEquals(EInvoicingPivotState.Batched, batch2.TransactionPivots[0].AIP_Status);

				AssertEquals("1 error notification email should be sent for batch 2", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals(1, ediInterchanges.Length);
				ediMessages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals(1, ediMessages.Length);
			}
		}

		#region Implementation

		public override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(GlbCompany company)
		{
			return new MockEDIInterchangeCreatorForTaiwanEInvoicingBatch(GlbCompany.CurrentCompany, () => new MockAccEInvoiceBatchToGEIConverter());
		}

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(Func<IAccEInvoiceBatchToGEIConverter> converter = null, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null, AccEInvoicingBatch batchWithErrors = null)
		{
			var interchangeCreator = new MockEDIInterchangeCreatorForTaiwanEInvoicingBatch(GlbCompany.CurrentCompany, converter, forceThisErrorWhileCreatingEDIInterchange);
			interchangeCreator.BatchForWhichErrorWillBeForced = batchWithErrors;

			return interchangeCreator;
		}

		protected override AccEInvoicingBatch GetBatchWithReadyStatus()
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var complianceDocument = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", TestObjectCreator.GetRandomString(8), "TXE", "desc", arInvoice.Lines[0], TestObjectCreator.Debtor);
			var batch = TestObjectCreator.CreateEInvoicingBatch(TestObjectCreator.GetRandomInt(1, 1000), Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			return batch;
		}

		protected override AccEInvoicingBatch GetBatchWithReadyStatusForSending(out string number)
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var complianceDocument = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arInvoice.Lines[0], TestObjectCreator.Debtor);
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			number = complianceDocument.ADH_DocumentNumber;
			return batch;
		}

		protected override string CountryCode => CountryCodes.Taiwan;

		#endregion
	}

	#region Inner Classes

	public class MockEDIInterchangeCreatorForTaiwanEInvoicingBatch : EDIInterchangeCreatorForTaiwan
	{
		public MockEDIInterchangeCreatorForTaiwanEInvoicingBatch(GlbCompany company, Func<IAccEInvoiceBatchToGEIConverter> converter, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null)
			: base(company)
		{
			Converter = converter;
			ForceThisError = forceThisErrorWhileCreatingEDIInterchange;
		}

		Func<IAccEInvoiceBatchToGEIConverter> Converter { get; }

		ExceptionTypes? ForceThisError { get; }

		public AccEInvoicingBatch BatchForWhichErrorWillBeForced { get; set; }

		protected override void CreateEDIMessageAndInterchangeCore(TransactionBatchProcessContext batchProcessContext, INotifications notifications)
		{
			base.CreateEDIMessageAndInterchangeCore(batchProcessContext, notifications);
			EInvoicingTestHelper.ForceErrorWhileCreatingEDIInterchange(batchProcessContext, notifications, ForceThisError, BatchForWhichErrorWillBeForced);
		}

		protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter => Converter();

		public bool GetIsBillingSupported() => IsBillingSupported;
	}

	#endregion
}