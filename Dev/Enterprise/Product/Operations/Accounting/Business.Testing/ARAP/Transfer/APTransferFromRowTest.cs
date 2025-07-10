using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APTransferFromRow))]
	public class APTransferFromRowTest : TransferRowTest
	{
		protected override Type TypeOfOtherRowInPair
		{
			get { return typeof(APTransferToRow); }
		}

		protected override ZByte TransactionCountForThisRow
		{
			get { return 1; }
		}

		protected override ZByte TransactionCountForOtherRow
		{
			get { return 2; }
		}

		protected override TransferRow TestTransferRow
		{
			get { return Header as APTransferFromRow; }
		}

		public override void TestGetTopLevelTransaction()
		{
			SetupForSave();
			TestTransferRow.AH_TransactionCount = 1;
			APTransferToRow testAPToRow = Factory.NewWithValidTestData(typeof(APTransferToRow)) as APTransferToRow;
			testAPToRow.AH_TransactionNum = Header.AH_TransactionNum;
			testAPToRow.AH_TransactionCount = 2;

			Factory.Save();

			Base.Transaction.IMatching topLevelTrans = TestTransferRow.GetTopLevelTransaction;
			Assert("Top Level Trans should be a Transfer", topLevelTrans is Transfer);
			Transfer topLevelTransfer = topLevelTrans as Transfer;
			AssertEquals("TopLevelTransfer should have the correct Header objects",
				TestTransferRow.PK, topLevelTransfer.TransferFrom.PK);
			AssertEquals("TopLevelTransfer should have the correct TransferTo object",
				testAPToRow.PK, topLevelTransfer.TransferTo.PK);
		}
	}
}
