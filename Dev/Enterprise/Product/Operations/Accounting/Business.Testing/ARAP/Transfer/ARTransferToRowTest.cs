using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARTransferToRow))]
	public class ARTransferToRowTest : TransferRowTest
	{
		protected override Type TypeOfOtherRowInPair
		{
			get { return typeof(ARTransferFromRow); }
		}

		protected override TransferRow TestTransferRow
		{
			get { return Header as ARTransferToRow; }
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
			ARTransferFromRow testARFromRow = Factory.NewWithValidTestData(typeof(ARTransferFromRow)) as ARTransferFromRow;
			testARFromRow.AH_TransactionNum = Header.AH_TransactionNum;
			testARFromRow.AH_TransactionCount = 1;

			Factory.Save();

			Base.Transaction.IMatching topLevelTrans = TestTransferRow.GetTopLevelTransaction;
			Assert("Top level trans should be a Transfer", topLevelTrans is Transfer);
			Transfer topLevelTransfer = topLevelTrans as Transfer;
			AssertEquals("TopLevelTransfer should have correct TransferFrom object",
				testARFromRow.PK, topLevelTransfer.TransferFrom.PK);
			AssertEquals("TopLevelTransfer should have correct TransferTo object",
				TestTransferRow.PK, topLevelTransfer.TransferTo.PK);
		}
	}
}
