using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	public class DepositSlipPrintHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestPrintDepositSlip()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);

			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];
			Factory.Save();

			TestPrintHelper.PrintDepositSlip(testBatch.AH_ReceiptBatchNo);
		}

		public void TestPrintDepositSlipInvalidDepositNumber()
		{
			TestDepositSlipPrintHelper testPrint = new TestDepositSlipPrintHelper();
			testPrint.PrintDepositSlip("99999");
			AssertEquals("Should NOT called Print()", false, testPrint.HasCalledPrintFunction);

			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];
			Factory.Save();

			testPrint.PrintDepositSlip(testBatch.AH_ReceiptBatchNo);
			AssertEquals("Should called Print()", true, testPrint.HasCalledPrintFunction);
		}

		public void TestGetBatchTransactionHeaderFromNumber()
		{
			GlbBranch friendBranch = Factory.NewWithValidTestData<GlbBranch>();
			friendBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch friendBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			friendBranch1.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch foreignBranch = Factory.NewWithValidTestData<GlbBranch>();
			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			foreignBranch.GB_GC = newCompany.PK;

			GlbBranch foreignBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbCompany newCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			foreignBranch1.GB_GC = newCompany1.PK;

			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.ReceiptBatch, ZArchitecture.Core.LedgerTypes.CashBook, 100M, TestObjectCreator.AUDBankAccount.PK);
			testReceipt.AH_GB = friendBranch.PK;
			testReceipt.AH_ReceiptBatchNo = "TestNo1";

			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.ReceiptBatch, ZArchitecture.Core.LedgerTypes.CashBook, 200M, TestObjectCreator.AUDBankAccount.PK);
			testReceipt2.AH_GB = friendBranch1.PK;
			testReceipt2.AH_ReceiptBatchNo = "TestNo2";

			ARReceipt testReceipt3 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.ReceiptBatch, ZArchitecture.Core.LedgerTypes.CashBook, 300M, TestObjectCreator.AUDBankAccount.PK);
			testReceipt3.AH_GB = foreignBranch1.PK;
			testReceipt3.AH_ReceiptBatchNo = "TestNo3";

			ARReceipt testReceipt4 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.ReceiptBatch, ZArchitecture.Core.LedgerTypes.CashBook, 400M, TestObjectCreator.AUDBankAccount.PK);
			testReceipt4.AH_GB = foreignBranch.PK;
			testReceipt4.AH_ReceiptBatchNo = "TestNo5";
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			testDepositBatchParent.DepositBatchLines.Load();

			AssertEquals("Should be only 2 DepositBatches in the collection", testDepositBatchParent.DepositBatchLines.Count, 2);
			Assert("Collection should contain TestReceipt", ContainsBatch(testReceipt.PK, testDepositBatchParent.DepositBatchLines));
			Assert("Collection should contain TestReceipt2", ContainsBatch(testReceipt2.PK, testDepositBatchParent.DepositBatchLines));
			DepositBatch testBatch1 = testDepositBatchParent.DepositBatchLines[0];
			DepositBatch testBatch2 = testDepositBatchParent.DepositBatchLines[1];
			DepositBatch testLoadedBatch = TestPrintHelper.GetBatchTransactionHeaderFromNumber_ForTestOnly(testBatch1.AH_ReceiptBatchNo);
			AssertEquals(testLoadedBatch.PK, testBatch1.PK);
			testLoadedBatch = TestPrintHelper.GetBatchTransactionHeaderFromNumber_ForTestOnly(testBatch2.AH_ReceiptBatchNo);
			AssertEquals(testLoadedBatch.PK, testBatch2.PK);
		}

		#region Implementations

		bool ContainsBatch(ZGuid batchGuid, DepositBatchCollection collection)
		{
			foreach (DepositBatch dB in collection)
			{
				if (dB.PK == batchGuid)
				{
					return true;
				}
			}
			return false;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestPrintHelper = new DepositSlipPrintHelper();
		}
		DepositSlipPrintHelper TestPrintHelper;

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;

		class TestDepositSlipPrintHelper : DepositSlipPrintHelper
		{
			protected override void Print(DepositBatch depositBatch)
			{
				base.Print(depositBatch);
				fHasCalledPrintFunction = true;
			}

			public bool HasCalledPrintFunction
			{
				get { return fHasCalledPrintFunction; }
			}
			bool fHasCalledPrintFunction;
		}

		#endregion
	}
}
