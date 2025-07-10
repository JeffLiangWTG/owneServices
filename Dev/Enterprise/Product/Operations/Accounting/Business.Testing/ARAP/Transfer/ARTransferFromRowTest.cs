using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARTransferFromRow))]
	public class ARTransferFromRowTest : TransferRowTest
	{
		protected override Type TypeOfOtherRowInPair
		{
			get { return typeof(ARTransferToRow); }
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
			get { return Header as ARTransferFromRow; }
		}

		public override void TestGetTopLevelTransaction()
		{
			SetupForSave();
			TestTransferRow.AH_TransactionCount = 1;
			ARTransferToRow testARToRow = Factory.NewWithValidTestData(typeof(ARTransferToRow)) as ARTransferToRow;
			testARToRow.AH_TransactionNum = Header.AH_TransactionNum;
			testARToRow.AH_TransactionCount = 2;

			Factory.Save();

			Base.Transaction.IMatching topLevelTrans = TestTransferRow.GetTopLevelTransaction;
			Assert("Top Level Trans should be a Transfer", topLevelTrans is Transfer);
			Transfer topLevelTransfer = topLevelTrans as Transfer;
			AssertEquals("TopLevelTransfer should have the correct Header objects",
				TestTransferRow.PK, topLevelTransfer.TransferFrom.PK);
			AssertEquals("TopLevelTransfer should have the correct TransferTo object",
				testARToRow.PK, topLevelTransfer.TransferTo.PK);
		}
	}
}
