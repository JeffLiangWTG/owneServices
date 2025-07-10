using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;

namespace Enterprise.Accounting.Business.Testing
{
	public class ReceivablePaymentDataTest : TestCaseWithFactory
	{
		public void TestGetEDocViaUniversalXmlSupport()
		{
			var uxmlSupport = new ReceivablePaymentData().GetEDocsViaUniversalXmlSupport();
			AssertEquals(null, uxmlSupport.LoadBusinessObjectFromCode(Factory, "invalid-code"));

			var arPayment1 = Factory.NewWithValidTestData<ARPayment>();
			Factory.Save();
			AssertEquals("00001000", arPayment1.AH_TransactionNum);

			var arPayment2 = Factory.NewWithValidTestData<ARPayment>();
			Factory.Save();
			AssertEquals("00001001", arPayment2.AH_TransactionNum);

			AssertEquals(arPayment1.PK, uxmlSupport.LoadBusinessObjectFromCode(Factory, "00001000")?.PK);

			AssertNull("There is no business object matching the transaction number", uxmlSupport.LoadBusinessObjectFromCode(Factory, "00000000"));
		}

		public void TestNullOrWhiteSpaceArqumentThrows()
		{
			var uxmlSupport = new ReceivablePaymentData().GetEDocsViaUniversalXmlSupport();
			AssertExceptionThrown<ArgumentException>("Factory Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(null, "00001000"));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be string empty", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, ""));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, null));
		}

		public void TestGetEDocViaUniversalXmlSupportExampleCodeFormat()
		{
			AssertEquals("100001", new ReceivablePaymentData().GetEDocsViaUniversalXmlSupport().ExampleCodeFormat);
		}
	}
}
