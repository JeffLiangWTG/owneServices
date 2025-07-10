using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	public abstract class BankTransferRowTest : TransactionHeaderTest
	{
		protected override ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedValue) => 0;

		BankTransferRow TestBankTransferRow
		{
			get { return Header as BankTransferRow; }
		}

		public override void TestTransactionNumberGenerator()
		{
			Assert("Transaction numbers on TransferRows are set by parent Transfer", true);
		}

		public override void TestRelatedTransactions()
		{
			BankTransferFromRow transferFrom = Factory.NewWithValidTestData<BankTransferFromRow>();
			transferFrom.AH_TransactionCount = 1;
			BankTransferToRow transferTo = Factory.NewWithValidTestData<BankTransferToRow>();
			transferTo.AH_TransactionCount = 2;
			transferTo.AH_TransactionNum = transferFrom.AH_TransactionNum;

			AssertEquals("There should be 1 related Transactions", 1, transferFrom.RelatedTransactions.Count);
			AssertEquals("There should be 1 related Transactions", 1, transferTo.RelatedTransactions.Count);
			Assert("Related transactions should contain other row in the pair", transferFrom.RelatedTransactions.Contains(transferTo));
			Assert("Related transactions should contain other row in the pair", transferTo.RelatedTransactions.Contains(transferFrom));
		}

		public override void TestLocalCredit()
		{
			Header.AH_InvoiceAmount = -40m;
			AssertEquals(40m, Header.LocalCredit);
		}

		public override void TestLocalDebit()
		{
			Header.AH_InvoiceAmount = 30m;
			AssertEquals(30m, Header.LocalDebit);
		}

		public override void TestExchangeRateTypeDependsOnLedgerAndType()
		{
			Assert(true);
		}

		public override void TestTransactionNumberOnSave()
		{
			SetupForSave();
			Factory.Save();
			AssertEquals("00001000", Header.AH_TransactionNum);
		}

		public override void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			Assert("This test cannot be performed on Bank Transfer Row as it will trigger critical validation when comparing the two rows.", true);
		}

		public override void TestOSOutstandingAmountMatching()
		{
			Assert("This test cannot be performed on Bank Transfer Row as it will trigger critical validation when comparing the two rows.", true);
		}

		public void TestDefaultValues()
		{
			AssertEquals(ReceiptTypes.EFT, TestBankTransferRow.AH_ReceiptType);
		}

		public void TestDebitCredit()
		{
			TestBankTransferRow.AH_OSTotal = -40m;
			AssertEquals("Debit should be 0", 0m, TestBankTransferRow.Debit);
			AssertEquals("Credit should be 40", 40m, TestBankTransferRow.Credit);

			TestBankTransferRow.AH_OSTotal = 80m;
			AssertEquals("Debit should be 80", 80m, TestBankTransferRow.Debit);
			AssertEquals("Credit should be 0", 0m, TestBankTransferRow.Credit);
		}

		public void TestLedger()
		{
			AssertEquals(LedgerTypes.CashBook, TestBankTransferRow.Ledger_ForTestOnly);
		}

		public void TestTransactionType()
		{
			AssertEquals(TransactionTypes.Transfer, TestBankTransferRow.TransactionType_ForTestOnly);
		}

		public override void TestGetWritableProperties()
		{
			AssertEquals("GetWritableProperties().Count", 0, ((BankTransferRow)Header).GetWritableProperties_ForTestOnly().Count);
		}

		public override void TestSetTransactionBelongsToGroupField()
		{
			TestSetTransactionBelongsToGroupField(false);
		}

		protected override void CancelTransactionHeaderToBeAbleToSave(TransactionHeader header)
		{
			base.CancelTransactionHeaderToBeAbleToSave(header);

			var reversing = header as IReversing;
			var reversingTransaction = reversing.ReverseTransaction as BankTransferRow;
			ZByte otherRowCount = ZByte.Zero;

			switch (reversingTransaction.AH_TransactionCount)
			{
				case AccTransactionHeader.TransactionCountConstants.BankTransferFromRow:
					otherRowCount = AccTransactionHeader.TransactionCountConstants.BankTransferToRow;
					break;
				case AccTransactionHeader.TransactionCountConstants.BankTransferFromRowWhenReversing:
					otherRowCount = AccTransactionHeader.TransactionCountConstants.BankTransferToRowWhenReversing;
					break;
				case AccTransactionHeader.TransactionCountConstants.BankTransferToRow:
					otherRowCount = AccTransactionHeader.TransactionCountConstants.BankTransferFromRow;
					break;
				case AccTransactionHeader.TransactionCountConstants.BankTransferToRowWhenReversing:
					otherRowCount = AccTransactionHeader.TransactionCountConstants.BankTransferFromRowWhenReversing;
					break;
				default:
					Fail("Transaction count not recognised");
					break;
			}

			var filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, reversingTransaction.AH_TransactionBelongsToGroup);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, otherRowCount);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, reversingTransaction.AH_GC);
			var otherRow = header.Factory.LoadTop1<BankTransferRow>(filter);

			if (otherRow == null)
			{
				if (reversingTransaction is BankTransferFromRow)
				{
					otherRow = header.Factory.NewWithValidTestData<BankTransferToRow>();
				}
				else if (reversingTransaction is BankTransferToRow)
				{
					otherRow = header.Factory.NewWithValidTestData<BankTransferFromRow>();
				}
				AssertNotNull("Other Row", otherRow);
				otherRow.AH_TransactionBelongsToGroup = reversingTransaction.AH_TransactionBelongsToGroup;
				otherRow.AH_TransactionCount = otherRowCount;
				otherRow.AH_TransactionNum = reversingTransaction.AH_TransactionNum;
			}
		}
	}
}
