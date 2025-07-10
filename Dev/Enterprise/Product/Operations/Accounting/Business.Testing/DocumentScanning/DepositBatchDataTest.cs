using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public class DepositBatchDataTest : TestCaseWithFactory
	{
		public void TestGetEDocViaUniversalXmlSupport()
		{
			var uxmlSupport = new DepositBatchData().GetEDocsViaUniversalXmlSupport();
			AssertEquals(null, uxmlSupport.LoadBusinessObjectFromCode(Factory, "invalid-code"));

			var depositBatchValid = Factory.NewWithValidTestData<DepositBatch>();
			depositBatchValid.AH_TransactionNum = "00001000";

			var depositBatchBadLedger = Factory.NewWithValidTestData<DepositBatch>();
			depositBatchBadLedger.AH_Ledger = LedgerTypes.AccountsPayable;
			depositBatchBadLedger.AH_TransactionNum = "00001001";

			var depositBatchBadTransactionType = Factory.NewWithValidTestData<DepositBatch>();
			depositBatchBadTransactionType.AH_TransactionType = TransactionTypes.DirectReceipt;
			depositBatchBadTransactionType.AH_TransactionNum = "00001002";
			Factory.Save();

			AssertNull("There is no business object matching the criteria, transaction number is incorrect", uxmlSupport.LoadBusinessObjectFromCode(Factory, "00000003"));

			AssertEquals(depositBatchValid.PK, uxmlSupport.LoadBusinessObjectFromCode(Factory, "00001000")?.PK);

			AssertNull("Filter should remove invalid ledger types", uxmlSupport.LoadBusinessObjectFromCode(Factory, "00001001"));
			AssertNull("Filter should remove invalid transaction types", uxmlSupport.LoadBusinessObjectFromCode(Factory, "00001002"));
		}

		public void TestGetEDocViaUniversalXmlSupport_NullOrWhiteSpaceArgumentThrows()
		{
			var uxmlSupport = new DepositBatchData().GetEDocsViaUniversalXmlSupport();
			AssertExceptionThrown<ArgumentException>("Factory Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(null, "00001000"));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be string empty", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, ""));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, null));
		}

		public void TestGetEDocViaUniversalXmlSupport_ExampleCodeFormat()
		{
			AssertEquals("100001", new DepositBatchData().GetEDocsViaUniversalXmlSupport().ExampleCodeFormat);
		}
	}
}
