using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Germany.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForGermany))]
	public class EInvoicingBatchCreatorForGermanyTest : EInvoicingBatchCreatorBaseTest
	{
		[UseSnapshotProtection]
		[TestDate(2020, 11, 06)]
		public void TestCompanySpecificBatchNumbers()
		{
			var company1 = Helper.CreateCompanyAndBranch("DE1", "BR1", Core.Constants.CountryCodes.Germany, true);
			var company2 = Helper.CreateCompanyAndBranch("DE2", "BR2", Core.Constants.CountryCodes.Germany, true);
			ObjectCreator.AALSHI.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			ObjectCreator.AALSHI.OH_Category = OrgConstants.Category.Government;

			Helper.CreateARAPINVCRDADJTransactions(company1.Branches[0], ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company1, 2);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company1, 4);
			AssertCompanySpecificBatchNumbers(company2, 2);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company1, 6);
			AssertCompanySpecificBatchNumbers(company2, 4);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company1, 8);

			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company2, 6);

			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company2, 8);
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

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company)
		{
			return new EInvoicingBatchCreatorForGermany(company);
		}
	}
}
