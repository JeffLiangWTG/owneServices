using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class PayableReceiptDataTest : TestCaseWithFactory
	{
		public void TestGetEDocViaUniversalXmlSupport()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "MYTESTORG";

			var uxmlSupport = new PayableReceiptData().GetEDocsViaUniversalXmlSupport();
			AssertEquals(null, uxmlSupport.LoadBusinessObjectFromCode(Factory, "invalid-code"));

			var apReceipt1 = Factory.NewWithValidTestData<APReceipt>();
			apReceipt1.AH_OH = orgHeader.PK;

			Factory.Save();
			AssertEquals("00001000", apReceipt1.AH_TransactionNum);

			var apReceipt2 = Factory.NewWithValidTestData<APReceipt>();
			apReceipt2.AH_OH = orgHeader.PK;
			Factory.Save();
			AssertEquals("00001001", apReceipt2.AH_TransactionNum);

			AssertEquals(apReceipt1.PK, uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG|00001000")?.PK);
			AssertNull("Expecting a key in the format 'ACP OrgCode|TransactionNumber, org code is required", uxmlSupport.LoadBusinessObjectFromCode(Factory, "00001000"));
			AssertNull("Expecting a key in the format 'ACP OrgCode|TransactionNumber, transaction number is required", uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG"));
			AssertNull("There is no business object matching the criteria, transaction number is incorrect", uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG|00000003"));
			AssertNull("There is no business object matching the criteria,org code is incorrect", uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG0 |00001000"));
		}

		public void TestNullOrWhiteSpaceArqumentThrows()
		{
			var uxmlSupport = new PayableReceiptData().GetEDocsViaUniversalXmlSupport();
			AssertExceptionThrown<ArgumentException>("Factory Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(null, "MYTESTORG|00001000"));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be string empty", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, ""));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, null));
		}

		public void TestGetEDocViaUniversalXmlSupportExampleCodeFormat()
		{
			AssertEquals("ABIGAS|100001", new PayableReceiptData().GetEDocsViaUniversalXmlSupport().ExampleCodeFormat);
		}
	}
}
