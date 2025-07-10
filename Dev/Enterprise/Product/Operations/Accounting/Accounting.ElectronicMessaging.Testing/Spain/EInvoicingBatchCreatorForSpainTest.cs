using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Spain.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForSpain))]
	public class EInvoicingBatchCreatorForSpainTest : EInvoicingBatchCreatorBaseTest
	{
		public void TestBatchGrouping_LimitOfSix_With19Transactions_GroupByLimit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Spain))
			{
				BatchLimit = 6;

				var company = GlbCompany.CurrentCompany;
				var branch = company.Branches[0];

				for (int i = 1; i <= 19; i++)
				{
					CreateTestTransactionPivot(i, company, branch, LedgerTypes.AccountsReceivable);
				}

				var batchingResult = GetBatchProcessor(company)
					.PerformBatching(new DetailedLoggerForTest());

				AssertEquals("Batching should create batch", true, batchingResult);

				var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(
					company.PK, EInvoicingBatchState.Ready);
				AssertEquals($"Batchs should be created", 4, batches.Length);
				Assert("Batch size should limited to 6", batches.All(x => x.TransactionPivots.Count <= 6));
				AssertEquals("Should have 3 batches with 6 pivots", 3, batches.Count(x => x.TransactionPivots.Count == 6));
				AssertEquals("Should have 1 batches with 1 pivots", 1, batches.Count(x => x.TransactionPivots.Count == 1));
			}
		}

		public void TestBatchGrouping_LimitOf98_WithDifferentBranch()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Spain))
			{
				BatchLimit = AccountingElectronicMessagingRegistry.MaximumBatchSizeSupportedInXUEProcessor; //Size limit is not grouping factor;

				var company = GlbCompany.CurrentCompany;
				var branch1 = company.Branches[0];
				var branch2 = ObjectCreator.CreateNewBranch(company, "BR2");
				branch2.Factory.Save();
				var branch3 = ObjectCreator.CreateNewBranch(company, "BR3");
				branch3.Factory.Save();

				//Branch 1 - 4 items
				var pivot1ForBranch1 = CreateTestTransactionPivot(1, company, branch1, LedgerTypes.AccountsReceivable);
				var pivot4ForBranch1 = CreateTestTransactionPivot(4, company, branch1, LedgerTypes.AccountsReceivable);
				var pivot6ForBranch1 = CreateTestTransactionPivot(6, company, branch1, LedgerTypes.AccountsReceivable);
				var pivot7ForBranch1 = CreateTestTransactionPivot(7, company, branch1, LedgerTypes.AccountsReceivable);
				//Branch 2
				var pivot2ForBranch2 = CreateTestTransactionPivot(2, company, branch2, LedgerTypes.AccountsReceivable);
				var pivot5ForBranch2 = CreateTestTransactionPivot(5, company, branch2, LedgerTypes.AccountsReceivable);
				//Branch 3
				var pivot3ForBranch3 = CreateTestTransactionPivot(3, company, branch3, LedgerTypes.AccountsReceivable);
			
				var batchingResult = GetBatchProcessor(company)
					.PerformBatching(new DetailedLoggerForTest());

				AssertEquals("Batching should create batch", true, batchingResult);

				var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(
					company.PK, EInvoicingBatchState.Ready);
				AssertEquals($"Batchs should be created", 3, batches.Length);

				var pivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(
					company.PK, EInvoicingPivotState.Batched);

				AssertBeInSameGroup(pivots,
					pivot1ForBranch1,
					pivot4ForBranch1,
					pivot6ForBranch1,
					pivot7ForBranch1);

				AssertBeInSameGroup(pivots,
					pivot2ForBranch2,
					pivot5ForBranch2);

				AssertBeInSameGroup(pivots,
					pivot3ForBranch3);
			}
		}

		public void TestBatchGrouping_LimitOf4_WithDifferentBranch_GroupByLimit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Spain))
			{
				BatchLimit = 4;

				var company = GlbCompany.CurrentCompany;
				var branch1 = company.Branches[0];
				var branch2 = ObjectCreator.CreateNewBranch(company, "BR2");
				branch2.Factory.Save();
				var branch3 = ObjectCreator.CreateNewBranch(company, "BR3");
				branch3.Factory.Save();

				//Branch2 - 2 items
				var pivot2ForBranch2 = CreateTestTransactionPivot(2, company, branch2, LedgerTypes.AccountsReceivable);
				var pivot5ForBranch2 = CreateTestTransactionPivot(5, company, branch2, LedgerTypes.AccountsReceivable);
				//Branch3 - 2 items
				var pivot3ForBranch3 = CreateTestTransactionPivot(3, company, branch3, LedgerTypes.AccountsReceivable);
				var pivot9ForBranch3 = CreateTestTransactionPivot(9, company, branch3, LedgerTypes.AccountsReceivable);
				//Branch1 - 5 items
				var pivot1ForBranch1 = CreateTestTransactionPivot(1, company, branch1, LedgerTypes.AccountsReceivable);
				var pivot4ForBranch1 = CreateTestTransactionPivot(4, company, branch1, LedgerTypes.AccountsReceivable);
				var pivot6ForBranch1 = CreateTestTransactionPivot(6, company, branch1, LedgerTypes.AccountsReceivable);
				var pivot7ForBranch1 = CreateTestTransactionPivot(7, company, branch1, LedgerTypes.AccountsReceivable);
				var pivot8ForBranch1 = CreateTestTransactionPivot(8, company, branch1, LedgerTypes.AccountsReceivable);

				var batchingResult = GetBatchProcessor(company)
					.PerformBatching(new DetailedLoggerForTest());

				AssertEquals("Batching should create batch", true, batchingResult);

				var pivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(
					company.PK, EInvoicingPivotState.Batched);

				var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(
					company.PK, EInvoicingBatchState.Ready);
				AssertEquals($"Batchs should be created", 4, batches.Length);

				AssertBeInSameGroup(pivots,
					pivot2ForBranch2,
					pivot5ForBranch2);

				AssertBeInSameGroup(pivots,
					pivot3ForBranch3,
					pivot9ForBranch3);

				//The rest belongs to branch1 should be split into two group,
				// - one with 4 items,
				// - one with 1 items
			}
		}

		public void TestBatchGrouping_LimitOf98_WithDifferentLedger()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Spain))
			{
				BatchLimit = AccountingElectronicMessagingRegistry.MaximumBatchSizeSupportedInXUEProcessor; //Size limit is not grouping factor

				var company = GlbCompany.CurrentCompany;
				var branch = company.Branches[0];

				//AR
				var pivot1AR = CreateTestTransactionPivot(1, company, branch, LedgerTypes.AccountsReceivable);
				var pivot2AR = CreateTestTransactionPivot(2, company, branch, LedgerTypes.AccountsReceivable);
				var pivot4AR = CreateTestTransactionPivot(4, company, branch, LedgerTypes.AccountsReceivable);
				var pivot6AR = CreateTestTransactionPivot(6, company, branch, LedgerTypes.AccountsReceivable);
				//AP
				var pivot3AP = CreateTestTransactionPivot(3, company, branch, LedgerTypes.AccountsPayable);
				var pivot5AP = CreateTestTransactionPivot(5, company, branch, LedgerTypes.AccountsPayable);
				var pivot7AP = CreateTestTransactionPivot(7, company, branch, LedgerTypes.AccountsPayable);

				var batchingResult = GetBatchProcessor(company)
					.PerformBatching(new DetailedLoggerForTest());

				AssertEquals("Batching should create batch", true, batchingResult);

				var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(
					company.PK, EInvoicingBatchState.Ready);
				AssertEquals($"Batchs should be created", 2, batches.Length);

				var pivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(
					company.PK, EInvoicingPivotState.Batched);

				AssertBeInSameGroup(pivots,
					pivot1AR,
					pivot2AR,
					pivot4AR,
					pivot6AR);

				AssertBeInSameGroup(pivots,
					pivot3AP,
					pivot5AP,
					pivot7AP);
			}
		}

		public void TestBatchGrouping_LimitOfTwo_WithDifferentLedger()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Spain))
			{
				BatchLimit = 2;

				var company = GlbCompany.CurrentCompany;
				var branch = company.Branches[0];

				//AR - 5 items
				var pivot1AR = CreateTestTransactionPivot(1, company, branch, LedgerTypes.AccountsReceivable);
				var pivot2AR = CreateTestTransactionPivot(2, company, branch, LedgerTypes.AccountsReceivable);
				var pivot4AR = CreateTestTransactionPivot(4, company, branch, LedgerTypes.AccountsReceivable);
				var pivot6AR = CreateTestTransactionPivot(6, company, branch, LedgerTypes.AccountsReceivable);
				var pivot7AR = CreateTestTransactionPivot(7, company, branch, LedgerTypes.AccountsReceivable);
				//AP - 3 items
				var pivot3AP = CreateTestTransactionPivot(3, company, branch, LedgerTypes.AccountsPayable);
				var pivot5AP = CreateTestTransactionPivot(5, company, branch, LedgerTypes.AccountsPayable);
				var pivot8AP = CreateTestTransactionPivot(8, company, branch, LedgerTypes.AccountsPayable);

				var batchingResult = GetBatchProcessor(company)
					.PerformBatching(new DetailedLoggerForTest());

				AssertEquals("Batching should create batch", true, batchingResult);

				//We should see two group for AR, and two group for AP
				var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(
					company.PK, EInvoicingBatchState.Ready);
				AssertEquals($"Batchs should be created", 5, batches.Length);

				AssertHasBatchLike(batchCount: 2, pivotCount: 2, ledger: LedgerTypes.AccountsReceivable);
				AssertHasBatchLike(batchCount: 1, pivotCount: 1, ledger: LedgerTypes.AccountsReceivable);
				AssertHasBatchLike(batchCount: 1, pivotCount: 2, ledger: LedgerTypes.AccountsPayable);
				AssertHasBatchLike(batchCount: 1, pivotCount: 1, ledger: LedgerTypes.AccountsPayable);

				void AssertHasBatchLike(int batchCount, int pivotCount, string ledger)
				{
					AssertEquals($"Expect {batchCount} batch(es) having {pivotCount} {ledger} pivot(s)",
						batchCount,
						batches.Count(b => b.TransactionPivots.Count == pivotCount
										&& b.TransactionPivots.All(p =>
											(p as AccEInvoicingTransactionPivot)
												.ParentTransactionHeader.AH_Ledger == ledger)));
				}
			}
		}

		static void AssertBeInSameGroup(
			AccEInvoicingTransactionPivot[] allPivots,
			params AccEInvoicingTransactionPivot[] assertItems)
		{
			var expectedPks = assertItems.Select(a => a.PK).ToHashSet();
			var actualPks = allPivots.First(p => p.PK == assertItems.First().PK).Batch.TransactionPivots
									.Cast<AccEInvoicingTransactionPivot>()
									.Select(p => p.PK).ToHashSet();

			CombineAssertions(() =>
			{
				AssertEquals(expectedPks.Count, actualPks.Count);
				Assert(expectedPks.SetEquals(actualPks));
			});
		}

		public void TestBatchGrouping_WithDifferentBranchAndLedger()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Spain))
			{
				var company = GlbCompany.CurrentCompany;
				var branch1 = ObjectCreator.CreateNewBranch(company, "BR1");
				branch1.Factory.Save();
				var branch2 = ObjectCreator.CreateNewBranch(company, "BR2");
				branch2.Factory.Save();
				Factory.Save();

				var pivotsExpectedInSameGroup = new List<AccEInvoicingTransactionPivot>();

				CreateTestTransactionPivot(1, company, branch1, LedgerTypes.AccountsReceivable);
				CreateTestTransactionPivot(2, company, branch2, LedgerTypes.AccountsReceivable);
				CreateTestTransactionPivot(3, company, branch1, LedgerTypes.AccountsPayable);

				pivotsExpectedInSameGroup.Add(CreateTestTransactionPivot(4, company, branch2, LedgerTypes.AccountsPayable));
				pivotsExpectedInSameGroup.Add(CreateTestTransactionPivot(5, company, branch2, LedgerTypes.AccountsPayable));
				pivotsExpectedInSameGroup.Add(CreateTestTransactionPivot(6, company, branch2, LedgerTypes.AccountsPayable));

				var batchingResult = GetBatchProcessor(company)
					.PerformBatching(new DetailedLoggerForTest());

				AssertEquals("Batching should create batch", true, batchingResult);

				var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(
					company.PK, EInvoicingBatchState.Ready);
				AssertEquals($"Batchs should be created", 4, batches.Length);

				var pivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(
					company.PK, EInvoicingPivotState.Batched);

				var batchToCheck = pivots.First(p => p.PK == pivotsExpectedInSameGroup[0].PK).Batch;
				var pivotsActuallyInLastBatch = pivots.Where(p => p.AIP_AIB == batchToCheck.PK)
					.ToList();

				Assert("All last three Pivots belong to same group.",
					pivotsExpectedInSameGroup.All(p => pivotsActuallyInLastBatch.Any(a => a.PK == p.PK)));
			}
		}

		public void TestTransactionGrouping_CreateDifferentBatches_TransactionsWithTheSameUniqueKeys_Spain()
		{
			AssertTransactionGroupingWithTheSameUniqueKeys(CountryCodes.Spain, 1);
		}

		AccEInvoicingTransactionPivot CreateTestTransactionPivot(int seq, GlbCompany company, GlbBranch branch, string ledger)
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_TransactionNum = $"{seq}-{branch.GB_Code}{ledger}";
			transaction.AH_Ledger = ledger;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_GC = company.PK;
			transaction.AH_GB = branch.PK;

			Factory.Save();

			var pivot = ObjectCreator.CreateEInvoicingTransactionPivot(
				Factory.Load<InvoicingBase>(transaction.PK));
			Factory.Save();

			return pivot;
		}

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company)
		{
			return new EInvoicingBatchCreatorForSpain(company, BatchLimit);
		}

		int BatchLimit = 3;
	}
}
