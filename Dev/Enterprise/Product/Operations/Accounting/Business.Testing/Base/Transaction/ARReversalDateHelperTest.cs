using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class ARReversalDateHelperTest : TestCaseWithFactory
	{
		public void TestReplaceTransferToWithTransferFromWhenPrintAccountingVoucher()
		{
			TransactionHeader[] originalTransactonHeaders = new TransactionHeader[2];
			ARTransferToRow transferTo1 = Factory.NewWithValidTestData<ARTransferToRow>();
			transferTo1.AH_TransactionCount = 2;

			ARTransferFromRow transferFrom1 = Factory.NewWithValidTestData<ARTransferFromRow>();
			transferFrom1.AH_TransactionNum = transferTo1.AH_TransactionNum;
			transferFrom1.AH_TransactionCount = 1;

			ARTransferFromRow transferFrom2 = Factory.NewWithValidTestData<ARTransferFromRow>();
			transferFrom2.AH_TransactionCount = 1;

			ARTransferToRow transferTo2 = Factory.NewWithValidTestData<ARTransferToRow>();
			transferTo2.AH_TransactionNum = transferFrom2.AH_TransactionNum;
			transferTo2.AH_TransactionCount = 2;

			originalTransactonHeaders[0] = transferTo1;
			originalTransactonHeaders[1] = transferFrom2;
			TransactionHeader[] updatedTransactonHeaders = TransactionHeaderHelper.ReplaceTransferToWithTransferFromWhenPrintingAccountingVoucher(originalTransactonHeaders);
			AssertEquals("ARTransferFromRow", updatedTransactonHeaders[0].GetType().Name);
			AssertEquals(transferFrom1.PK, updatedTransactonHeaders[0].PK);
			AssertEquals("ARTransferFromRow", updatedTransactonHeaders[1].GetType().Name);
			AssertEquals(transferFrom2.PK, updatedTransactonHeaders[1].PK);
		}
	}
}