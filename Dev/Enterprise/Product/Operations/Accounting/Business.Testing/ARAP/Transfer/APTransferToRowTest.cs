using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APTransferToRow))]
	public class APTransferToRowTest : TransferRowTest
	{
		protected override TransferRow TestTransferRow
		{
			get { return Header as APTransferToRow; }
		}

		protected override Type TypeOfOtherRowInPair
		{
			get { return typeof(APTransferFromRow); }
		}

		protected override ZByte TransactionCountForThisRow
		{
			get { return 2; }
		}

		protected override ZByte TransactionCountForOtherRow
		{
			get { return 1; }
		}

		public override void TestGetTopLevelTransaction()
		{
			SetupForSave();
			TestTransferRow.AH_TransactionCount = 2;
			APTransferFromRow testAPFromRow = Factory.NewWithValidTestData(typeof(APTransferFromRow)) as APTransferFromRow;
			testAPFromRow.AH_TransactionNum = Header.AH_TransactionNum;
			testAPFromRow.AH_TransactionCount = 1;

			Factory.Save();

			Base.Transaction.IMatching topLevelTrans = TestTransferRow.GetTopLevelTransaction;
			Assert("Top Level Trans should be a Transfer", topLevelTrans is Transfer);
			Transfer topLevelTransfer = topLevelTrans as Transfer;
			AssertEquals("TopLevelTransfer should have the correct Header objects",
				testAPFromRow.PK, topLevelTransfer.TransferFrom.PK);
			AssertEquals("TopLevelTransfer should have the correct TransferTo object",
				TestTransferRow.PK, topLevelTransfer.TransferTo.PK);
		}
	}
}
