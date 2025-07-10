using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;

namespace Enterprise.Accounting.Business.Testing
{
	public class ReceivableReceiptDataTest : TestCaseWithFactory
	{
		public void TestGetEDocViaUniversalXmlSupport()
		{
			var uxmlSupport = new ReceivableReceiptData().GetEDocsViaUniversalXmlSupport();
			AssertEquals(null, uxmlSupport.LoadBusinessObjectFromCode(Factory, "invalid-code"));

			var arReceipt1 = Factory.NewWithValidTestData<ARReceipt>();
			Factory.Save();
			AssertEquals("00001000", arReceipt1.AH_TransactionNum);

			var arReceipt2 = Factory.NewWithValidTestData<ARReceipt>();
			Factory.Save();
			AssertEquals("00001001", arReceipt2.AH_TransactionNum);

			var arReceipt3 = Factory.NewWithValidTestData<ARReceipt>();
			Factory.Save();
			AssertEquals("00001002", arReceipt3.AH_TransactionNum);

			AssertEquals(arReceipt2.PK, uxmlSupport.LoadBusinessObjectFromCode(Factory, "00001001")?.PK);

			AssertNull("There is no business object matching the transaction number", uxmlSupport.LoadBusinessObjectFromCode(Factory, "00000000"));
		}

		public void TestNullOrWhiteSpaceArqumentThrows()
		{
			var uxmlSupport = new ReceivableReceiptData().GetEDocsViaUniversalXmlSupport();
			AssertExceptionThrown<ArgumentException>("Factory Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(null, "00001000"));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be string empty", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, ""));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, null));
		}

		public void TestGetEDocViaUniversalXmlSupportExampleCodeFormat()
		{
			AssertEquals("100001", new ReceivableReceiptData().GetEDocsViaUniversalXmlSupport().ExampleCodeFormat);
		}
	}
}
