using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForTurkey))]
	public class EInvoicingBatchCreatorForTurkeyTest : EInvoicingBatchCreatorBaseTest
	{
		[UseSnapshotProtection]
		[TestDate(2020, 01, 29)]
		public void TestCompanySpecificBatchNumbers_AREnabled()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			{
				var company1 = HelperTR.CommonHelper.CreateCompanyAndBranch("TR1", "BR1", CountryCodes.Turkey, true);
				var company2 = HelperTR.CommonHelper.CreateCompanyAndBranch("TR2", "BR2", CountryCodes.Turkey, true);
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(company1.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(company2.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "33334444");

				HelperTR.CreateARINVTransactions(company1.FirstActiveBranch, ObjectCreator.KDV18, ComplianceSubTypeCodes.EIN, "ABC2020000000001");
				AssertCompanySpecificBatchNumbers(company1, 1);

				HelperTR.CreateARINVTransactions(company1.FirstActiveBranch, ObjectCreator.KDV18, ComplianceSubTypeCodes.EIN, "ABC2020000000002");
				HelperTR.CreateARINVTransactions(company2.FirstActiveBranch, ObjectCreator.KDV18, ComplianceSubTypeCodes.EIN, "ABC2020000000003");
				AssertCompanySpecificBatchNumbers(company1, 2);
				AssertCompanySpecificBatchNumbers(company2, 1);

				HelperTR.CreateARINVTransactions(company1.FirstActiveBranch, ObjectCreator.KDV18, ComplianceSubTypeCodes.EIN, "ABC2020000000004");
				HelperTR.CreateARINVTransactions(company2.FirstActiveBranch, ObjectCreator.KDV18, ComplianceSubTypeCodes.EIN, "ABC2020000000005");
				AssertCompanySpecificBatchNumbers(company1, 3);
				AssertCompanySpecificBatchNumbers(company2, 2);

				HelperTR.CreateARINVTransactions(company1.FirstActiveBranch, ObjectCreator.KDV18, ComplianceSubTypeCodes.EIN, "ABC2020000000006");
				AssertCompanySpecificBatchNumbers(company1, 4);

				HelperTR.CreateARINVTransactions(company2.FirstActiveBranch, ObjectCreator.KDV18, ComplianceSubTypeCodes.EIN, "ABC2020000000007");
				AssertCompanySpecificBatchNumbers(company2, 3);

				HelperTR.CreateARINVTransactions(company2.FirstActiveBranch, ObjectCreator.KDV18, ComplianceSubTypeCodes.EIN, "ABC2020000000008");
				AssertCompanySpecificBatchNumbers(company2, 4);
			}
		}

		[TestDate(2020, 01, 29)]
		public void TestBatchCreatedForPivotsWithCancelActionType()
		{
			const string failedSubmissionError = "Cancellation request was not sent because the original transaction had encountered an error during submission.";

			HelperTR.TestObjectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, ComplianceSubTypeCodes.EAR);
			HelperTR.TestObjectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, ComplianceSubTypeCodes.ICN);

			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				AssertTransactionPivotCreation(subPivotStatus: EInvoicingPivotState.Failed, expectedSUBPivotStatus: EInvoicingPivotState.Failed, expectedCANErrorDescription: failedSubmissionError);
				AssertTransactionPivotCreation(subPivotStatus: EInvoicingPivotState.Succeed, expectedSUBPivotStatus: EInvoicingPivotState.Succeed, expectedCANPivotStatus: EInvoicingPivotState.Batched, governmentAllocatedNumber: "1234");
			}
		}

		void AssertTransactionPivotCreation(
			string subPivotStatus,
			string expectedSUBPivotStatus,
			string expectedSUBBatchStatus = EInvoicingBatchState.Sent,
			string expectedCANPivotStatus = EInvoicingPivotState.Discarded,
			string governmentAllocatedNumber = "",
			string expectedCANErrorDescription = "")
		{
			HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(GlbCompany.CurrentCompany.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
			var signatureCredential = HelperTR.CreateCompanySignatureCredential(GlbCompany.CurrentCompany);
			GlbCompany.CurrentCompany.Factory.Save();

			var arInvoice = HelperTR.TestObjectCreator.CreateARInvoice<ARInvoice>("0001", HelperTR.TestObjectCreator.TRY, 1m, HelperTR.TestObjectCreator.AALSHI);
			arInvoice.AH_ComplianceSubType = ComplianceSubTypeCodes.EAR;
			arInvoice.Factory.Save();

			var pivot = arInvoice.GetMostRecentEInvoicingTransactionPivot();
			AssertNotNull(pivot);

			var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
			processor.PerformBatching(new DetailedLoggerForTest());

			var pivotAfterProcess = new BusinessObjectFactory().Load<AccEInvoicingTransactionPivot>(pivot.PK);
			AssertNotNull(pivotAfterProcess);

			var batch = Factory.Load<AccEInvoicingBatch>(pivotAfterProcess.AIP_AIB);
			AssertNotNull(batch);

			var expectedOriginalTransactionPivotMessage = $"[{subPivotStatus}] Sometimes, original transaction pivots have error messages.";
			pivotAfterProcess.AIP_ErrorDescription = expectedOriginalTransactionPivotMessage;
			pivotAfterProcess.AIP_Status = subPivotStatus;
			pivotAfterProcess.Factory.Save();
			batch.AIB_GovernmentAllocatedNumber = governmentAllocatedNumber;
			batch.AIB_Status = EInvoicingBatchState.Sent;
			batch.Factory.Save();

			var arCreditNote = HelperTR.TestObjectCreator.ReverseTransaction(arInvoice, out _) as ARCreditNote;
			arCreditNote.AH_ComplianceSubType = ComplianceSubTypeCodes.ICN;
			arCreditNote.Factory.Save();

			processor.PerformBatching(new DetailedLoggerForTest());

			var invoicePivotAfterProcessedCreditNote = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
			AssertNotNull(invoicePivotAfterProcessedCreditNote);
			AssertEquals(expectedSUBPivotStatus, invoicePivotAfterProcessedCreditNote.AIP_Status);
			AssertEquals(expectedOriginalTransactionPivotMessage, invoicePivotAfterProcessedCreditNote.AIP_ErrorDescription);

			var invoiceBatchAfterProcessedCreditNote = new BusinessObjectFactory().Load<AccEInvoicingBatch>(invoicePivotAfterProcessedCreditNote.AIP_AIB);
			AssertNotNull(invoiceBatchAfterProcessedCreditNote);
			AssertEquals(expectedSUBBatchStatus, invoiceBatchAfterProcessedCreditNote.AIB_Status);

			var creditPivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arCreditNote.PK));
			AssertEquals(subPivotStatus != EInvoicingPivotState.Discarded, creditPivot != null);
			if (subPivotStatus != EInvoicingPivotState.Discarded)
			{
				AssertEquals(EInvoicingPivotActionType.Cancel, creditPivot.AIP_ActionType);
				AssertEquals(expectedCANPivotStatus, creditPivot.AIP_Status);
				AssertEquals($"When SUB is {expectedSUBPivotStatus}, CAN should say '{expectedCANErrorDescription}'", expectedCANErrorDescription, creditPivot.AIP_ErrorDescription);
			}
		}

		[TestDate(2020, 01, 29)]
		public void TestInvalidCancellationPivotsAreDiscarded()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(GlbCompany.CurrentCompany.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
				GlbCompany.CurrentCompany.Factory.Save();

				AssertInvalidCancellationPivotHandlingForAP(originalSubType: ComplianceSubTypeCodes.PIN, reversalSubtype: ComplianceSubTypeCodes.DIN);
				AssertInvalidCancellationPivotHandlingForAP(originalSubType: ComplianceSubTypeCodes.CIN, reversalSubtype: ComplianceSubTypeCodes.CCN);
			}
		}

		void AssertInvalidCancellationPivotHandlingForAP(string originalSubType = "", string reversalSubtype = "")
		{
			var apInvoice = HelperTR.TestObjectCreator.CreateAPInvoice<APInvoice>($"{originalSubType}0001", HelperTR.TestObjectCreator.TRY, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, HelperTR.TestObjectCreator.AALSHI);
			apInvoice.AH_ComplianceSubType = originalSubType;

			var originalPivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, apInvoice.PK));
			AssertNull("Precondition: An APInvoice should not create a SUB pivot.", originalPivot);

			var apCreditNote = HelperTR.TestObjectCreator.ReverseTransaction(apInvoice, out _) as APCreditNote;
			apCreditNote.AH_TransactionNum = TestObjectCreator.GetRandomString(5);
			apCreditNote.AH_TransactionReference = apInvoice.AH_TransactionNum;
			apCreditNote.AH_ComplianceSubType = reversalSubtype;
			apCreditNote.Factory.Save();

			var creditPivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, apCreditNote.PK));
			AssertNotNull($"Precondition: APCreditNote ({reversalSubtype}) should create a pivot.", creditPivot);
			AssertEquals($"Precondition: APCreditNote ({reversalSubtype}) pivot action should be SUB.", EInvoicingPivotActionType.Submit, creditPivot.AIP_ActionType);
			AssertEquals($"Precondition: APCreditNote ({reversalSubtype}) pivot should start as QUE.", EInvoicingPivotState.Queued, creditPivot.AIP_Status);

			// Having a CAN pivot without an original transaction is an error. It should not happen, yet, it did once.
			// Batch processor should Discard such faulty pivots instead of leaving them in the queue indefinitely.
			creditPivot.AIP_ActionType = EInvoicingPivotActionType.Cancel;
			creditPivot.Factory.Save();
			ErrorReporter.Clear();

			var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
			processor.PerformBatching(new DetailedLoggerForTest());

			const string expectedErrorDescription = "This transaction was erroneously queued for cancellation even though the original transaction had not been submitted for e-Reporting. Please contact support.";

			var creditPivotAfterProcessing = new BusinessObjectFactory().Load<AccEInvoicingTransactionPivot>(creditPivot.PK);
			AssertEquals("Invalid Cancellation pivots should be Discarded.", EInvoicingPivotState.Discarded, creditPivotAfterProcessing.AIP_Status);
			AssertEquals("Discarded pivot should display an explanation.", expectedErrorDescription, creditPivotAfterProcessing.AIP_ErrorDescription);
			AssertEquals("Invalid Cancellation pivots should not be batched.", Guid.Empty, creditPivotAfterProcessing.AIP_AIB);
		}

		void AssertCompanySpecificBatchNumbers(GlbCompany company, int batchNumber)
		{
			var transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, EInvoicingPivotState.Queued, ZGuid.Empty);
			AssertEquals("Number of transactions ready for batching.", 1, transactionPivots.Length);

			var processor = GetBatchProcessor(company);
			processor.PerformBatching(new DetailedLoggerForTest());
			var invoiceBatches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, EInvoicingBatchState.Ready);
			AssertEquals("Batch number starts as expected.", batchNumber, invoiceBatches[invoiceBatches.Length - 1].AIB_BatchNumber);
		}

		[TestDate(2020, 1, 29, 23, 08, 00)]
		public void TestGovernmentAllocatedNumberForReverseARCreditNoteWhenCancelledInvoicePivotIsSucceed()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (HelperTR.TestObjectCreator.SetUpForTestingEInvoicingTurkey_ARAP(GlbBranch.CurrentBranch.PK.ToGuid(), complianceDate.ToDateTime()))
			{
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(GlbCompany.CurrentCompany.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
				var signatureCredential = HelperTR.CreateCompanySignatureCredential(GlbCompany.CurrentCompany);
				GlbCompany.CurrentCompany.Factory.Save();

				HelperTR.TestObjectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, ComplianceSubTypeCodes.EAR);
				HelperTR.TestObjectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, ComplianceSubTypeCodes.ICN);

				var arInvoice = HelperTR.TestObjectCreator.CreateARInvoice<ARInvoice>("0001", HelperTR.TestObjectCreator.TRY, 1m, HelperTR.TestObjectCreator.AALSHI);
				arInvoice.AH_ComplianceSubType = ComplianceSubTypeCodes.EAR;
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

				var arCreditNote = (ARCreditNote)HelperTR.TestObjectCreator.ReverseTransaction(arInvoice, out _);
				arCreditNote.AH_ComplianceSubType = ComplianceSubTypeCodes.ICN;
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

		[TestDate(2020, 1, 29, 23, 08, 00)]
		public void TestPerformBatchingForMiscellaneousScenarios()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (HelperTR.TestObjectCreator.SetUpForTestingEInvoicingTurkey_ARAP(GlbBranch.CurrentBranch.PK.ToGuid(), complianceDate.ToDateTime()))
			{
				var apTransactionListRequestBatchId = GetApTransactionListRequestBatchId(GlbCompany.CurrentCompany.Country.Code);

				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(GlbCompany.CurrentCompany.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
				var signatureCredential = HelperTR.CreateCompanySignatureCredential(GlbCompany.CurrentCompany);
				GlbCompany.CurrentCompany.Factory.Save();

				HelperTR.TestObjectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, ComplianceSubTypeCodes.EAR);

				var arInvoice = HelperTR.TestObjectCreator.CreateARInvoice<ARInvoice>("0001", HelperTR.TestObjectCreator.TRY, 1m, HelperTR.TestObjectCreator.AALSHI);
				arInvoice.AH_ComplianceSubType = ComplianceSubTypeCodes.EAR;
				arInvoice.Factory.Save();

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				var serviceLogger = new DetailedLoggerForTest();

				// Asserts PerformBatching for batching the existing queued pivot and periodic batch
				AssertEquals("Result has to be true", true, processor.PerformBatching(serviceLogger));
				var factory1 = new BusinessObjectFactory();
				var batches = factory1.Load<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.AIB_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("Should have 2 created batches", 2, batches.Length);
				AssertNotNull(batches.Where(x => x.AIB_GovernmentAllocatedNumber == string.Empty));
				AssertNotNull(batches.Where(x => x.AIB_GovernmentAllocatedNumber == apTransactionListRequestBatchId));
				AssertEquals(7, serviceLogger.Logs.Count);
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Batching started."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Processing all queued pivots."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Started creating batch"));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Completed creating batch."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == $"Start creating AP transaction list batch for company {GlbCompany.CurrentCompany.GC_Code}."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "AP transaction list request batch has been created."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Information && x.Item2 == "Batching completed."));
				batches.ForEach(x => x.AIB_Status = EInvoicingBatchState.Discarded);
				factory1.Save();

				// Asserts PerformBatching for batching no queued pivot and has not been expired periodic batch yet
				serviceLogger = new DetailedLoggerForTest();
				AssertEquals("Result has to be false", false, processor.PerformBatching(serviceLogger));
				var factory2 = new BusinessObjectFactory();
				batches = factory2.Load<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.AIB_GC, GlbCompany.CurrentCompany.PK).AddToFilter(AccEInvoicingBatchSchema.AIB_Status, SQLComparisonOperator.NotEqual, EInvoicingBatchState.Discarded));
				AssertEquals("Should not have created new batch", 0, batches.Length);
				AssertEquals(4, serviceLogger.Logs.Count);
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Batching started."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == $"Start creating AP transaction list batch for company {GlbCompany.CurrentCompany.GC_Code}."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "No AP transaction list request batch was created."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Information && x.Item2 == "Batching completed - No transactions were available for batching."));

				// Asserts PerformBatching for batching no queued pivot and has not been expired periodic batch yet (Edge case)
				var apListAutomatedRequestScheduleValue = AccountingMasterFilesRegistry.Instance.APListAutomatedRequestSchedule.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				TestDateAttribute.AddMinutes(apListAutomatedRequestScheduleValue);
				serviceLogger = new DetailedLoggerForTest();
				AssertEquals("Result has to be false", false, processor.PerformBatching(serviceLogger));
				var factory3 = new BusinessObjectFactory();
				batches = factory3.Load<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.AIB_GC, GlbCompany.CurrentCompany.PK).AddToFilter(AccEInvoicingBatchSchema.AIB_Status, SQLComparisonOperator.NotEqual, EInvoicingBatchState.Discarded));
				AssertEquals("Should not have created new batch", 0, batches.Length);
				AssertEquals(4, serviceLogger.Logs.Count);
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Batching started."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == $"Start creating AP transaction list batch for company {GlbCompany.CurrentCompany.GC_Code}."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "No AP transaction list request batch was created."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Information && x.Item2 == "Batching completed - No transactions were available for batching."));

				// Asserts PerformBatching for batching no queued pivot and expired periodic batch
				TestDateAttribute.AddMinutes(1);
				serviceLogger = new DetailedLoggerForTest();
				AssertEquals("Result has to be true", true, processor.PerformBatching(serviceLogger));
				var factory4 = new BusinessObjectFactory();
				batches = factory4.Load<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.AIB_GC, GlbCompany.CurrentCompany.PK).AddToFilter(AccEInvoicingBatchSchema.AIB_Status, SQLComparisonOperator.NotEqual, EInvoicingBatchState.Discarded));
				AssertEquals("Should have 1 created batch", 1, batches.Length);
				AssertNotNull(batches.Where(x => x.AIB_GovernmentAllocatedNumber == apTransactionListRequestBatchId));
				AssertEquals(4, serviceLogger.Logs.Count);
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Batching started."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == $"Start creating AP transaction list batch for company {GlbCompany.CurrentCompany.GC_Code}."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "AP transaction list request batch has been created."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Information && x.Item2 == "Batching completed."));
			}
		}

		public void TestPerformBatchingForTransactionsPendingAllocation()
		{
			var factory = HelperTR.TestObjectCreator.Factory;
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (HelperTR.TestObjectCreator.SetUpForTestingEInvoicingTurkey_ARAP(GlbBranch.CurrentBranch.PK.ToGuid(), complianceDate.ToDateTime()))
			{
				var paInvoice = factory.NewWithValidTestData<TransactionPendingAllocation>();
				paInvoice.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
				paInvoice.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
				paInvoice.AH_TransactionNum = "TRN0001";
				paInvoice.AH_ComplianceSubType = "EIN";
				paInvoice.AH_TransactionReference = "EIN20220000001";
				paInvoice.AH_GovernmentAllocatedID = "220F85B8-DCBC-4D0B-B8C6-3C551A64FE0B";
				HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(paInvoice, EInvoicingPivotActionType.ConfirmTransactionReceived);

				var arInvoice = factory.NewWithValidTestData<ARInvoice>();
				arInvoice.AH_TransactionNum = "TRN0002";
				arInvoice.AH_ComplianceSubType = "EIN";
				arInvoice.AH_TransactionReference = "EIN20220000002";
				HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, EInvoicingPivotActionType.Submit);
				factory.Save();

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				var serviceLogger = new DetailedLoggerForTest();

				processor.PerformBatching(serviceLogger);

				var newFactory = new BusinessObjectFactory();
				var batches = newFactory.Load<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.AIB_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("Should have 3 created batch", 3, batches.Length);

				var paBatch = batches.Where(x => x.AIB_GovernmentAllocatedNumber == paInvoice.AH_GovernmentAllocatedID).FirstOrDefault();
				AssertNotNull(paBatch);

				var paPivot = paBatch.TransactionPivots.FirstOrDefault() as AccEInvoicingTransactionPivot;
				AssertEquals(EInvoicingPivotState.Batched, paPivot.AIP_Status);

				var pivots = newFactory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_GC, GlbCompany.CurrentCompany.PK));
				var arPivot = pivots.Where(x => x.AIP_ParentID == arInvoice.PK).FirstOrDefault();
				AssertNotNull(arPivot);
				AssertEquals(EInvoicingPivotState.Batched, arPivot.AIP_Status);

				var arBatch = batches.Where(x => x.PK == arPivot.AIP_AIB).FirstOrDefault();
				AssertNotNull(arBatch);

				AssertEquals(7, serviceLogger.Logs.Count);
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Batching started."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Processing all queued pivots."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Started creating batch"));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Completed creating batch."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == $"Start creating AP transaction list batch for company {GlbCompany.CurrentCompany.GC_Code}."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "AP transaction list request batch has been created."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Information && x.Item2 == "Batching completed."));
			}
		}

		public void TestPerformBatchingForTransactionsPendingAllocationApprovalRequest_Approve() =>
			TestPerformBatchingForTransactionsPendingAllocationApprovalRequest(EInvoicingPivotActionType.Approve);

		public void TestPerformBatchingForTransactionsPendingAllocationApprovalRequest_Reject() =>
			TestPerformBatchingForTransactionsPendingAllocationApprovalRequest(EInvoicingPivotActionType.Reject);

		void TestPerformBatchingForTransactionsPendingAllocationApprovalRequest(string pivotActionType)
		{
			var factory = HelperTR.TestObjectCreator.Factory;
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (HelperTR.TestObjectCreator.SetUpForTestingEInvoicingTurkey_ARAP(GlbBranch.CurrentBranch.PK.ToGuid(), complianceDate.ToDateTime()))
			{
				var paInvoice = factory.NewWithValidTestData<TransactionPendingAllocation>();
				paInvoice.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
				paInvoice.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
				paInvoice.AH_TransactionNum = "TRN0001";
				paInvoice.AH_ComplianceSubType = "PIC";
				paInvoice.AH_TransactionReference = "EIN20220000001";
				paInvoice.AH_GovernmentAllocatedID = "220F85B8-DCBC-4D0B-B8C6-3C551A64FE0B";
				var paCRXPivot = HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(paInvoice, EInvoicingPivotActionType.ConfirmTransactionReceived);
				factory.Save();

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				var serviceLogger = new DetailedLoggerForTest();

				processor.PerformBatching(serviceLogger);

				var newFactory = new BusinessObjectFactory();
				var batches = newFactory.Load<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.AIB_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("Should have 2 created batches. PIL and CRX", 2, batches.Length);

				var batchPIL = batches.First(x => x.TransactionPivots.Count == 0);
				batchPIL.AIB_SystemLastEditTimeUtc = batchPIL.AIB_SystemCreateTimeUtc.AddMinutes(5);
				newFactory.Save();

				HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(paInvoice, pivotActionType);
				factory.Save();

				processor.PerformBatching(serviceLogger);

				var newFactory2 = new BusinessObjectFactory();
				batches = newFactory2.Load<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.AIB_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals($"Should have 3 created batches. PIL, CRX, {pivotActionType}", 3, batches.Length);

				var paBatch = batches.FirstOrDefault(x => x.AIB_GovernmentAllocatedNumber == paInvoice.AH_GovernmentAllocatedID
													&& (x.TransactionPivots?.OfType<AccEInvoicingTransactionPivot>().Any(y => y.AIP_ActionType == pivotActionType) ?? false));
				AssertNotNull(paBatch);
				AssertEquals(EInvoicingPivotState.Batched, paBatch.TransactionPivots[0].AIP_Status);

				AssertEquals(14, serviceLogger.Logs.Count);
				AssertEquals(2, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Batching started."));
				AssertEquals(2, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Processing all queued pivots."));
				AssertEquals(2, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Started creating batch"));
				AssertEquals(2, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Completed creating batch."));
				AssertEquals(2, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == $"Start creating AP transaction list batch for company {GlbCompany.CurrentCompany.GC_Code}."));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "AP transaction list request batch has been created."));
				AssertEquals(2, serviceLogger.Logs.Count(x => x.Item1 == LogType.Information && x.Item2 == "Batching completed."));
			}
		}

		[UseSnapshotProtection(true)]
		[TestDate(2024, 03, 25, 16, 00, 00)]
		public void TestTriggerStatusUpdateThrottling()
		{
			var retryPeriod = TimeSpan.FromHours(2);
			var queuedPivotCount = 7;
			var eligiblePivotCount = 103;
			var maximumRetrySlots = 100;

			// Clamp the number within a hardcoded maximum and subtract those already in the queue from available slots:
			var expectedPivotsToRetry = Math.Min(eligiblePivotCount, maximumRetrySlots) - queuedPivotCount;
			AssertEquals("Precondition: Developer must have calculated correctly.", 93, expectedPivotsToRetry);

			using (AccountingMasterFilesRegistry.Instance.TaxInvoiceStatusUpdateAutomatedRequestSchedule.SetTemporaryValue(HelperTR.TurkeyBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, (int)retryPeriod.TotalMinutes))
			using (HelperTR.TestObjectCreator.SetUpForTestingEInvoicingTurkey_ARAP(HelperTR.TurkeyBranch.Company.FirstActiveBranch.PK.ToGuid(), ZDateTime.UtcToday.AddDays(-1).ToDateTime()))
			{
				var batch = Factory.New<AccEInvoicingBatch>();
				batch.AIB_BatchNumber = 9876;
				batch.AIB_Status = EInvoicingBatchState.Sent;
				batch.AIB_GC = HelperTR.TurkeyBranch.Company.PK;
				batch.AIB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-5);
				batch.AIB_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;

				CreateDummyPivots(age: TimeSpan.FromHours(-1), count: 17, EInvoicingPivotState.Sent, "These will not be eligible within the 120-minute window.");

				CreateDummyPivots(age: TimeSpan.FromHours(-3), count: eligiblePivotCount, EInvoicingPivotState.Sent, "These are eligible, but we don't want them ALL to be retried.");
				CreateDummyPivots(age: TimeSpan.FromHours(-5), count: queuedPivotCount, EInvoicingPivotState.Batched, "These should count against the upper limit.");

				CreateDummyPivots(age: TimeSpan.FromHours(-7), count: 5, EInvoicingPivotState.BatchedWithError, "BER should not affect the outcome.");
				CreateDummyPivots(age: TimeSpan.FromHours(-8), count: 3, EInvoicingPivotState.Failed, "FAL should not affect the outcome.");
				CreateDummyPivots(age: TimeSpan.FromHours(-9), count: 2, EInvoicingPivotState.Succeed, "SUC should not affect the outcome.");
				Factory.Save();

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany) as EInvoicingBatchCreatorForTurkeyTestWrapper;
				var pivotsToRetry = processor.GetAllReadyToRetryStatusPivotsForTesting();

				AssertNotEquals("Not all eligible pivots must be retried.", eligiblePivotCount, pivotsToRetry.Count);
				AssertEquals("Active (batched) pivots must be subtracted from the upper limit.", expectedPivotsToRetry, pivotsToRetry.Count);

				void CreateDummyPivots(TimeSpan age, int count, ZString pivotState, string comments)
				{
					for (var index = 0; index < count; index++)
					{
						var pivot = Factory.New<AccEInvoicingTransactionPivot>();
						pivot.AIP_ParentID = ZGuid.NewZGuid();
						pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
						pivot.AIP_ActionType = EInvoicingPivotActionType.StatusCheck;
						pivot.SetCompanyAndCountryCode(batch.Company);
						pivot.AIP_AIB = batch.PK;
						pivot.AIP_LastSentTimeUtc = ZDateTime.UtcNow.Add(age).AddSeconds(-index);
						pivot.AIP_Status = pivotState;
						pivot.AIP_ErrorDescription = comments;
					}
				}
			}
		}

		string GetApTransactionListRequestBatchId(string countryCode) => GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(countryCode).ApTransactionListRequestBatchId;

		AccEInvoicingTransactionPivot LoadPivotForTransaction(BusinessObjectFactory factory, ZGuid transactionPk)
		{
			return factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionPk));
		}

		(AccEInvoicingTransactionPivot, AccEInvoicingBatch) LoadPivotAndBatchForTransaction(BusinessObjectFactory factory, ZGuid transactionPk)
		{
			var pivot = LoadPivotForTransaction(factory, transactionPk);
			return (pivot, factory.Load<AccEInvoicingBatch>(pivot.AIP_AIB));
		}

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company) => new EInvoicingBatchCreatorForTurkeyTestWrapper(company);

		protected TurkeyEInvoiceTestHelper HelperTR => helper ?? (helper = new TurkeyEInvoiceTestHelper());
		TurkeyEInvoiceTestHelper helper;
	}
}
