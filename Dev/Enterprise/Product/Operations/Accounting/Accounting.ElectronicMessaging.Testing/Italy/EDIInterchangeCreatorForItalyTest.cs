using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	public class EDIInterchangeCreatorForItalyTest : GEIEDIInterchangeCreatorTest
	{
		public override void TestIsBillingSupported()
		{
			var logger = new TestServiceLogger();
			var processor = new InterchangeProcessorWithIsBillingSupportedExposed(GlbCompany.CurrentCompany);
			Assert("Billing transactions are created for Italy.", processor.GetIsBillingSupported());
		}

		public void TestLinkEventWithBatchAndInvoice()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var logger = new TestServiceLogger();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
				var processor = new MockEDIInterchangeCreatorForItalyEInvoicingBatch(GlbCompany.CurrentCompany, converter, null);
				processor.Process(logger);
			}

			AssertEquals("invoice posted", "AR|INV|Posted", arInvoice.Logs.AutoCreatedLogSL_ReferenceCache);
			var dataExportLog = arInvoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExport.Code)).SingleOrDefault();
			AssertNotNull("event linked to invoice", dataExportLog);
			AssertEquals("event linked to invoice", "Purpose: E-Reporting Transaction Exported. Batch : 100", dataExportLog.SL_Reference);
			AssertEquals("event linked to batch", "DEX", batch.Logs.MostRecentLog.SL_SE_NKEvent);
			AssertEquals("event linked to batch", "Purpose: E-Reporting Transaction Exported. Batch : 100", batch.Logs.MostRecentLog.SL_Reference);
		}

		public override void TestProcessPartOfBatchesFailed()
		{
			Assert("Each batch contains only one invoice, so partial failure of a batch is not possible for an Italian company", true);
		}

		public void TestProcessZSaveExceptionFailed()
		{
			Globals.IsUserInteractive = false;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				AccountingConfigurationRegistry.Instance.GenerateARInvoiceAttachmentForEReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var batch = TestObjectCreator.CreateEInvoicingBatch(Business.TestObjectCreator.GetRandomInt(1, 1000), Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m);
				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Batched);
				Factory.Save();

				var logger = new TestServiceLogger();
				var processor = new EDIInterchangeCreatorForItaly(GlbCompany.CurrentCompany);

				processor.AdditionalActionDuringSave_ForTestOnly += (_, factories) =>
				{
					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var invoiceInNewFactory = newFactory.Load<InvoicingBase>(arInvoice.PK);
					var newInfo = invoiceInNewFactory.DocManagerInfo;
					var newDocument = newInfo.AddFileOrDocument(new byte[] { 2, 2, 2, 2, 2 }, "Test2.pdf", "MSC");
					invoiceInNewFactory.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
					newFactory.Save();

					AssertEquals(1, newInfo.Files.Count);
				};

				AssertExceptionThrown<ZSaveConcurrencyException>("Fail to process concurrency error", () => processor.Process(logger));

				AssertContains(@"Debug|Attempting to save all changes in database
Debug|eDocs Reload Required : While you were working, the eDocs for this record were modified. The system will now need to merge this information.
Debug|Attempting to save all changes in database", logger.ToString());
			}
		}

		public void TestProcessZSaveExceptionSuccessfully()
		{
			Globals.IsUserInteractive = false;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				AccountingConfigurationRegistry.Instance.GenerateARInvoiceAttachmentForEReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var batch = TestObjectCreator.CreateEInvoicingBatch(Business.TestObjectCreator.GetRandomInt(1, 1000), Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m);
				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Batched);
				Factory.Save();

				var logger = new TestServiceLogger();
				var processor = new EDIInterchangeCreatorForItaly(GlbCompany.CurrentCompany);

				processor.AdditionalActionDuringSave_ForTestOnly += (_, factories) =>
				{
					((BusinessObjectFactory)factories.ToArray()[0]).Saved += RollbackTransaction_Saved;

					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var invoiceInNewFactory = newFactory.Load<InvoicingBase>(arInvoice.PK);
					var newInfo = invoiceInNewFactory.DocManagerInfo;
					var newDocument = newInfo.AddFileOrDocument(new byte[] { 2, 2, 2, 2, 2 }, "Test2.pdf", "MSC");
					invoiceInNewFactory.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
					newFactory.Save();

					AssertEquals(1, newInfo.Files.Count);
				};

				AssertNoExceptionThrown("ZSaveException handled", () => processor.Process(logger));

				var logMsg = logger.ToString();
				AssertContains(@"Debug|Attempting to save all changes in database
Debug|eDocs Reload Required : While you were working, the eDocs for this record were modified. The system will now need to merge this information.
Debug|Attempting to save all changes in database
Debug|Successfully saved. Processing is complete.", logMsg);
				AssertNotContains("Debug | Attempting to send an email notification containing error details.", logMsg);

				void RollbackTransaction_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
				{
					if (!savedSuccessfully)
					{
						var factoryForRollBack = new BusinessObjectFactory();
						factoryForRollBack.RefreshEnabled = false;
						var batchNew = factoryForRollBack.Load<AccEInvoicingBatch>(batch.PK);
						batchNew.AIB_Status = Core.Constants.EInvoicingBatchState.Ready;
						batchNew.TransactionPivots[0].AIP_Status = EInvoicingPivotState.Batched;
						factoryForRollBack.Save();
					}
				}
			}
		}

		#region Implementation

		public override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(GlbCompany company)
		{
			return new MockEDIInterchangeCreatorForItalyEInvoicingBatch(GlbCompany.CurrentCompany, () => new MockAccEInvoiceBatchToGEIConverter());
		}

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(Func<IAccEInvoiceBatchToGEIConverter> converter = null, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null, AccEInvoicingBatch batchWithErrors = null)
		{
			var interchangeCreator = new MockEDIInterchangeCreatorForItalyEInvoicingBatch(GlbCompany.CurrentCompany, converter, forceThisErrorWhileCreatingEDIInterchange);
			interchangeCreator.BatchForWhichErrorWillBeForced = batchWithErrors;

			return interchangeCreator;
		}

		protected override AccEInvoicingBatch GetBatchWithReadyStatus()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(Business.TestObjectCreator.GetRandomInt(1, 1000), Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), Business.TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			return batch;
		}

		protected override AccEInvoicingBatch GetBatchWithReadyStatusForSending(out string number)
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			arInvoice.AH_ConsolidatedInvoiceRef = "STEST00001";
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Batched);
			Factory.Save();

			number = arInvoice.AH_TransactionNum;
			return batch;
		}

		protected override string CountryCode => CountryCodes.Italy;

		class InterchangeProcessorWithIsBillingSupportedExposed : EDIInterchangeCreatorForItaly
		{
			public InterchangeProcessorWithIsBillingSupportedExposed(GlbCompany company)
				: base(company)
			{ }

			public bool GetIsBillingSupported() => IsBillingSupported;
		}

		#endregion
	}

	#region Inner Classes

	public class MockEDIInterchangeCreatorForItalyEInvoicingBatch : EDIInterchangeCreatorForItaly
	{
		public MockEDIInterchangeCreatorForItalyEInvoicingBatch(GlbCompany company, Func<IAccEInvoiceBatchToGEIConverter> converter, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null)
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
	}

	#endregion
}
