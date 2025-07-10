using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Taiwan.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForTaiwan))]
	public class EInvoicingBatchCreatorForTaiwanTest : EInvoicingBatchCreatorBaseTest
	{
		[UseSnapshotProtection]
		[TestDate(2020, 7, 30, 8, 0, 0)]
		public void TestCompanySpecificBatchNumbers()
		{
			var company1 = Helper.CreateCompanyAndBranch("TW1", "BR1", Core.Constants.CountryCodes.Taiwan, true);
			var company2 = Helper.CreateCompanyAndBranch("TW2", "BR2", Core.Constants.CountryCodes.Taiwan, true);

			Helper.CreateARComplianceDocumentWithQueuedStatus(company1.Branches[0]);
			Helper.CreateARComplianceDocumentWithQueuedStatus(company1.Branches[0]);

			AssertCompanySpecificBatchNumbers(company1, 1);

			Helper.CreateARComplianceDocumentWithQueuedStatus(company1.Branches[0]);
			Helper.CreateARComplianceDocumentWithQueuedStatus(company1.Branches[0], TransactionTypes.CreditNote);

			AssertCompanySpecificBatchNumbers(company1, 3);

			Helper.CreateARComplianceDocumentWithQueuedStatus(company1.Branches[0]);
			Helper.CreateARComplianceDocumentWithQueuedStatus(company1.Branches[0]);
			Helper.CreateARComplianceDocumentWithQueuedStatus(company2.Branches[0]);
			Helper.CreateARComplianceDocumentWithQueuedStatus(company2.Branches[0]);

			AssertCompanySpecificBatchNumbers(company1, 4);
			AssertCompanySpecificBatchNumbers(company2, 1);
		}

		[TestDate(2020, 7, 30, 8, 0, 0)]
		public void TestSpecialVoidingCompanyBatchNumbers()
		{
			var company1 = Helper.CreateCompanyAndBranch("TW1", "BR1", Core.Constants.CountryCodes.Taiwan, true);
			var company2 = Helper.CreateCompanyAndBranch("TW2", "BR2", Core.Constants.CountryCodes.Taiwan, true);

			Helper.CreateARComplianceDocumentForSpecialVoidingWithQueuedStatus(company1.Branches[0],"voidingReason");
			Helper.CreateARComplianceDocumentForSpecialVoidingWithQueuedStatus(company1.Branches[0], "voidingReason1");

			AssertCompanySpecificBatchNumbers(company1, 1);

			Helper.CreateARComplianceDocumentWithQueuedStatus(company1.Branches[0], TransactionTypes.CreditNote);
			Helper.CreateARComplianceDocumentForSpecialVoidingWithQueuedStatus(company1.Branches[0], "voidingReasoning",TransactionTypes.CreditNote);

			AssertCompanySpecificBatchNumbers(company1, 3);

			Helper.CreateARComplianceDocumentWithQueuedStatus(company2.Branches[0]);
			Helper.CreateARComplianceDocumentForSpecialVoidingWithQueuedStatus(company2.Branches[0],"voidingReason");

			AssertCompanySpecificBatchNumbers(company2, 2);

			Helper.CreateARComplianceDocumentWithQueuedStatus(company2.Branches[0], TransactionTypes.CreditNote);
			Helper.CreateARComplianceDocumentForSpecialVoidingWithQueuedStatus(company2.Branches[0],"voidingReasoning", TransactionTypes.CreditNote);

			AssertCompanySpecificBatchNumbers(company2, 4);
		}

		public override void TestDefaultPivotStatus()
		{
			var company = Helper.CreateCompanyAndBranch("TW1", "BR1", CountryCodes.Taiwan, true);
			Helper.CreateARComplianceDocumentForSpecialVoidingWithQueuedStatus(company.Branches[0], "voidingReason");
			var pivot = EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, EInvoicingPivotState.Queued, ZGuid.Empty)[0];

			var batchingResult = GetBatchProcessor(company).PerformBatching(new DetailedLoggerForTest());
			AssertEquals("Batching should create a batch", true, batchingResult);

			pivot.Reload();
			AssertEquals("Previously queued pivot should now be batched", EInvoicingPivotState.Batched, pivot.AIP_Status);
		}

		void AssertCompanySpecificBatchNumbers(GlbCompany company, int batchNumber)
		{
			var transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, Core.Constants.EInvoicingPivotState.Queued, ZGuid.Empty);
			AssertEquals("Number of transactions ready for batching.", 2, transactionPivots.Length);

			var processor = GetBatchProcessor(company);
			processor.PerformBatching(new DetailedLoggerForTest());

			var invoiceBatches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, Core.Constants.EInvoicingBatchState.Ready);
			AssertEquals("Batch number starts as expected.", batchNumber, invoiceBatches[invoiceBatches.Length - 1].AIB_BatchNumber);
		}

		public override void TestBatching_ExcludesPendingEInvoicingPivots()
			=> Assert("Taiwan is compliance document based: test does not apply", true);

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company)
		{
			return new EInvoicingBatchCreatorForTaiwan(company);
		}
	}
}
