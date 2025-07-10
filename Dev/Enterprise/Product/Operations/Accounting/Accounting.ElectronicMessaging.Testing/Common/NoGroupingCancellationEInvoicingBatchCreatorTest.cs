using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	[TestedType(typeof(NoGroupingCancellationEInvoicingBatchCreator))]
	public class NoGroupingCancellationEInvoicingBatchCreatorTest : EInvoicingBatchCreatorBaseTest
	{
		[TestDate(2021, 2, 24)]
		public void TestCompanySpecificBatchNumbers_Brazil()
		{
			RunForCountry(CountryCodes.Brazil, 6);
		}

		[TestDate(2021, 02, 24)]
		public void TestBatchCreatedForPivotsWithCancelActionType_Brazil()
		{
			AssertTransactionPivotCreation(CountryCodes.Brazil, EInvoicingPivotState.Batched,          ZString.Empty, EInvoicingPivotState.Discarded, expectedSUBBatchStatus: EInvoicingBatchState.Discarded);
			AssertTransactionPivotCreation(CountryCodes.Brazil, EInvoicingPivotState.Queued,           ZString.Empty, EInvoicingPivotState.Discarded, expectedSUBBatchStatus: EInvoicingBatchState.Discarded);
			AssertTransactionPivotCreation(CountryCodes.Brazil, EInvoicingPivotState.Discarded,        ZString.Empty, EInvoicingPivotState.Discarded);
			AssertTransactionPivotCreation(CountryCodes.Brazil, EInvoicingPivotState.BatchedWithError, ZString.Empty, EInvoicingPivotState.Discarded, expectedSUBPivotStatus: EInvoicingPivotState.Discarded, expectedSUBBatchStatus: EInvoicingBatchState.Discarded);
			AssertTransactionPivotCreation(CountryCodes.Brazil, EInvoicingPivotState.Failed,           ZString.Empty, EInvoicingPivotState.Discarded, expectedSUBPivotStatus: EInvoicingPivotState.Failed);
			AssertTransactionPivotCreation(CountryCodes.Brazil, EInvoicingPivotState.Delivered,        "1234",        EInvoicingPivotState.Batched,   expectedSUBPivotStatus: EInvoicingPivotState.Delivered);
			AssertTransactionPivotCreation(CountryCodes.Brazil, EInvoicingPivotState.Succeed,          "1234",        EInvoicingPivotState.Batched,   expectedSUBPivotStatus: EInvoicingPivotState.Succeed);
			AssertTransactionPivotCreation(CountryCodes.Brazil, EInvoicingPivotState.Sent,             ZString.Empty, EInvoicingPivotState.Queued,    expectedSUBPivotStatus: EInvoicingPivotState.Sent);
		}

		[TestDate(2020, 1, 29, 23, 08, 00)]
		public void TestCancelInvoiceWhenPivotIsQueued_Brazil()
			=> AssertCancelInvoiceWhenPivotIsQueued(CountryCodes.Brazil);

		[TestDate(2020, 1, 29, 23, 08, 00)]
		public void TestGovernmentAllocatedNumberForReverseARCreditNoteWhenCancelledInvoicePivotIsSucceed_Brazil()
			=> AssertGovernmentAllocatedNumberForReverseARCreditNoteWhenCancelledInvoicePivotIsSucceed(CountryCodes.Brazil);

		#region Implementation

		void RunForCountry(string countryCode, int numberOfInvCrdAdjForThisCountry)
		{
			var company1 = Helper.CreateCompanyAndBranch(countryCode + "1", "BR1", countryCode, true);
			Helper.AddCustomsCodeForCountryIfMissing(company1.FirstActiveBranch.OrgProxy, countryCode, "VAT");
			Helper.AddCustomsCodeForCountryIfMissing(company1.FirstActiveBranch.OrgProxy, countryCode, "GST");
			var company2 = Helper.CreateCompanyAndBranch(countryCode + "2", "BR2", countryCode, true);
			Helper.AddCustomsCodeForCountryIfMissing(company2.FirstActiveBranch.OrgProxy, countryCode, "VAT");
			Helper.AddCustomsCodeForCountryIfMissing(company2.FirstActiveBranch.OrgProxy, countryCode, "GST");
			Helper.AddCustomsCodeForCountryIfMissing(ObjectCreator.AALSHI, countryCode, "VAT");
			Helper.AddCustomsCodeForCountryIfMissing(ObjectCreator.AALSHI, countryCode, "GST");

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, beforeSave: CreateQueuedPivotForAllTransactions);
			AssertCompanySpecificBatchNumbers(company1, numberOfInvCrdAdjForThisCountry * 1, numberOfInvCrdAdjForThisCountry);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, beforeSave: CreateQueuedPivotForAllTransactions);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, beforeSave: CreateQueuedPivotForAllTransactions);
			AssertCompanySpecificBatchNumbers(company1, numberOfInvCrdAdjForThisCountry * 2, numberOfInvCrdAdjForThisCountry);
			AssertCompanySpecificBatchNumbers(company2, numberOfInvCrdAdjForThisCountry * 1, numberOfInvCrdAdjForThisCountry);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, beforeSave: CreateQueuedPivotForAllTransactions);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, beforeSave: CreateQueuedPivotForAllTransactions);
			AssertCompanySpecificBatchNumbers(company1, numberOfInvCrdAdjForThisCountry * 3, numberOfInvCrdAdjForThisCountry);
			AssertCompanySpecificBatchNumbers(company2, numberOfInvCrdAdjForThisCountry * 2, numberOfInvCrdAdjForThisCountry);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, beforeSave: CreateQueuedPivotForAllTransactions);
			AssertCompanySpecificBatchNumbers(company1, numberOfInvCrdAdjForThisCountry * 4, numberOfInvCrdAdjForThisCountry);

			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, beforeSave: CreateQueuedPivotForAllTransactions);
			AssertCompanySpecificBatchNumbers(company2, numberOfInvCrdAdjForThisCountry * 3, numberOfInvCrdAdjForThisCountry);

			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, beforeSave: CreateQueuedPivotForAllTransactions);
			AssertCompanySpecificBatchNumbers(company2, numberOfInvCrdAdjForThisCountry * 4, numberOfInvCrdAdjForThisCountry);
		}

		void AssertCompanySpecificBatchNumbers(GlbCompany company, int batchNumber, int numberOfInvCrdAdjForThisCountry)
		{
			var transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, Core.Constants.EInvoicingPivotState.Queued, ZGuid.Empty);
			AssertEquals($"Number of transactions ready for batching (company '{company.GC_Code}').", numberOfInvCrdAdjForThisCountry, transactionPivots.Length);

			var processor = GetBatchProcessor(company);
			processor.PerformBatching(new DetailedLoggerForTest());

			var invoiceBatches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, Core.Constants.EInvoicingBatchState.Ready);
			AssertEquals($"Batch number starts as expected (company '{company.GC_Code}').", batchNumber, invoiceBatches[invoiceBatches.Length - 1].AIB_BatchNumber);
		}

		void AssertTransactionPivotCreation(string countryCode, ZString subPivotStatus, ZString governmentAllocatedNumber, ZString expectedCANPivotStatus, string expectedSUBPivotStatus = EInvoicingPivotState.Discarded, string expectedSUBBatchStatus = EInvoicingBatchState.Sent)
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				GlbCompany.CurrentCompany.Factory.Save();

				// Queue and batch a transaction.
				var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
				var pivot = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: EInvoicingPivotState.Queued);
				arInvoice.Factory.Save();

				var pivotAfterProcess = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				AssertNotNull(pivotAfterProcess);

				var batch = Helper.ObjectCreator.CreateEInvoicingBatchForPivot(pivotAfterProcess, GetNextBatchNumber, EInvoicingBatchState.Sent);
				batch.AIB_GovernmentAllocatedNumber = governmentAllocatedNumber;
				batch.Factory.Save();

				pivotAfterProcess.AIP_Status = subPivotStatus;
				pivotAfterProcess.Factory.Save();

				// Create a reversal and batch again.
				var arCreditNote = (ARCreditNote)Helper.ObjectCreator.ReverseTransaction(arInvoice, out _);
				var pivotCreditNote = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: EInvoicingPivotActionType.Cancel, status: EInvoicingPivotState.Queued);
				arCreditNote.Factory.Save();

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				processor.PerformBatching(new DetailedLoggerForTest());

				var pivotAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				AssertNotNull(pivotAfterCreditNoteProcessed);
				AssertEquals(expectedSUBPivotStatus, pivotAfterCreditNoteProcessed.AIP_Status);

				var creditPivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arCreditNote.PK));
				AssertNotNull(creditPivot);
				AssertEquals(expectedCANPivotStatus, creditPivot.AIP_Status);

				var batchAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, pivotAfterCreditNoteProcessed.AIP_AIB));
				AssertNotNull(batchAfterCreditNoteProcessed);
				AssertEquals(expectedSUBBatchStatus, batchAfterCreditNoteProcessed.AIB_Status);
			}
		}

		void AssertCancelInvoiceWhenPivotIsQueued(string countryCode)
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				GlbCompany.CurrentCompany.Factory.Save();

				var expectedOriginalTransactionPivotMessage = $"[{countryCode}] Sometimes, original transaction pivots have error messages.";

				var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
				var pivot = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: EInvoicingPivotState.Queued);
				pivot.AIP_ErrorDescription = expectedOriginalTransactionPivotMessage;
				arInvoice.Factory.Save();

				var arCreditNote = (ARCreditNote)Helper.ObjectCreator.ReverseTransaction(arInvoice, out _);
				var pivotCreditNote = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: EInvoicingPivotActionType.Cancel, status: EInvoicingPivotState.Queued);
				arCreditNote.Factory.Save();

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				var serviceLogger = new DetailedLoggerForTest();
				processor.PerformBatching(serviceLogger);

				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Processing cancellation pivots."));
				AssertEquals(0, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Processing all queued pivots."));
				var postFactory = new BusinessObjectFactory();

				var invPivot = LoadPivotForTransaction(postFactory, arInvoice.PK);
				AssertNotNull(invPivot);
				AssertEquals(pivot.PK, invPivot.PK);
				AssertEquals(EInvoicingPivotState.Discarded, invPivot.AIP_Status);
				AssertEquals(expectedOriginalTransactionPivotMessage, invPivot.AIP_ErrorDescription);

				var crdPivot = LoadPivotForTransaction(postFactory, arCreditNote.PK);
				AssertNotNull(crdPivot);
				AssertEquals(EInvoicingPivotState.Discarded, crdPivot.AIP_Status);
			}
		}

		void AssertGovernmentAllocatedNumberForReverseARCreditNoteWhenCancelledInvoicePivotIsSucceed(string countryCode)
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
				var pivot = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: EInvoicingPivotState.Queued);
				arInvoice.Factory.Save();

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				var serviceLogger = new DetailedLoggerForTest();
				processor.PerformBatching(serviceLogger);

				var postFactory = new BusinessObjectFactory();
				(var invPivot, var invBatch) = LoadPivotAndBatchForTransaction(postFactory, arInvoice.PK);
				AssertNotNull(invPivot);
				AssertNotNull(invBatch);

				invPivot.AIP_Status = EInvoicingPivotState.Succeed;
				invBatch.AIB_GovernmentAllocatedNumber = "Government Allocated Number";
				postFactory.Save();

				var arCreditNote = (ARCreditNote)Helper.ObjectCreator.ReverseTransaction(arInvoice, out _);
				var pivotCreditNote = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: EInvoicingPivotActionType.Cancel, status: EInvoicingPivotState.Queued);
				arCreditNote.Factory.Save();

				processor.PerformBatching(serviceLogger);

				var postARCreditNote = postFactory.Load<ARCreditNote>(arCreditNote.PK);
				(var crdPivot, var crdBatch) = LoadPivotAndBatchForTransaction(postFactory, arCreditNote.PK);
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Processing cancellation pivots."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Processing all queued pivots."));
				AssertNotNull(crdPivot);
				AssertNotNull(crdBatch);
				AssertEquals(EInvoicingPivotState.Batched, crdPivot.AIP_Status);
				AssertEquals(EInvoicingBatchState.Ready, crdBatch.AIB_Status);
				AssertEquals(arInvoice.EInvoicingGovernmentAllocatedNumber, postARCreditNote.EInvoicingGovernmentAllocatedNumber);
			}
		}

		void CreateQueuedPivotForAllTransactions(ARInvoice arInvoice, ARCreditNote arCreditNote, ARAdjustmentNote arAdjustmentNote, APInvoice apInvoice, APCreditNote apCreditNote, APAdjustmentNote apAdjustmentNote)
		{
			if (arInvoice != null)
			{
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			}
			if (arCreditNote != null)
			{
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote);
			}
			if (arAdjustmentNote != null)
			{
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arAdjustmentNote);
			}
			if (apInvoice != null)
			{
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(apInvoice);
			}
			if (apCreditNote != null)
			{
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(apCreditNote);
			}
			if (apAdjustmentNote != null)
			{
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(apAdjustmentNote);
			}
		}

		static AccEInvoicingTransactionPivot LoadPivotForTransaction(BusinessObjectFactory factory, ZGuid transactionPk)
			=> factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionPk));

		static (AccEInvoicingTransactionPivot, AccEInvoicingBatch) LoadPivotAndBatchForTransaction(BusinessObjectFactory factory, ZGuid transactionPk)
		{
			var pivot = LoadPivotForTransaction(factory, transactionPk);
			return (pivot, factory.Load<AccEInvoicingBatch>(pivot.AIP_AIB));
		}

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company)
		{
			return new NoGroupingCancellationEInvoicingBatchCreator(company);
		}

		ZInt GetNextBatchNumber => int.Parse(AccountingNumberFountainWrapperFactory.Instance.AccEInvoicingBatchNumber.GetNext(Db.Connection), CultureInfo.InvariantCulture);

		#endregion
	}
}
