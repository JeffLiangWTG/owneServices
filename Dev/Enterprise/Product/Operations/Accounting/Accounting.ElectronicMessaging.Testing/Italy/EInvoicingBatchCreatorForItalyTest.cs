
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForItaly))]
	public class EInvoicingBatchCreatorForItalyTest : EInvoicingBatchCreatorBaseTest
	{
		[UseSnapshotProtection]
		[TestDate(2016, 5, 10)]
		public void TestCompanySpecificBatchNumbers()
		{
			var company1 = Helper.CreateCompanyAndBranch("IT1", "BR1", Core.Constants.CountryCodes.Italy, true);
			var company2 = Helper.CreateCompanyAndBranch("IT2", "BR2", Core.Constants.CountryCodes.Italy, true);

			Helper.CreateARAPINVCRDADJTransactions(company1.Branches[0], ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company1, 3);

			Helper.CreateARAPINVCRDADJTransactions(company1.Branches[0], ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.Branches[0], ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company1, 6);
			AssertCompanySpecificBatchNumbers(company2, 3);

			Helper.CreateARAPINVCRDADJTransactions(company1.Branches[0], ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.Branches[0], ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company1, 9);
			AssertCompanySpecificBatchNumbers(company2, 6);

			Helper.CreateARAPINVCRDADJTransactions(company1.Branches[0], ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company1, 12);

			Helper.CreateARAPINVCRDADJTransactions(company2.Branches[0], ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company2, 9);
			Helper.CreateARAPINVCRDADJTransactions(company2.Branches[0], ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company2, 12);
		}

		void AssertCompanySpecificBatchNumbers(GlbCompany company, int batchNumber)
		{
			var transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, Core.Constants.EInvoicingPivotState.Queued, ZGuid.Empty);
			AssertEquals("Number of transactions ready for batching.", 3, transactionPivots.Length);

			var processor = GetBatchProcessor(company);
			processor.PerformBatching(new DetailedLoggerForTest());

			var invoiceBatches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, Core.Constants.EInvoicingBatchState.Ready);
			AssertEquals("Batch number starts as expected.", batchNumber, invoiceBatches[invoiceBatches.Length - 1].AIB_BatchNumber);
		}

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company)
		{
			return new EInvoicingBatchCreatorForItaly(company);
		}
	}
}
