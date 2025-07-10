
using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class CollectionBatchDataTest : TestCaseWithFactory
	{
		public void TestGetEDocViaUniversalXmlSupport()
		{
			var uxmlSupport = new CollectionBatchData().GetEDocsViaUniversalXmlSupport();
			AssertEquals(null, uxmlSupport.LoadBusinessObjectFromCode(Factory, "invalid-code"));

			var collectionBatch1 = Factory.NewWithValidTestData<AccCollectionBatch>();
			collectionBatch1.ACB_BatchNumber = string.Empty;
			collectionBatch1.ACB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			AssertEquals("00001000", collectionBatch1.ACB_BatchNumber);

			var collectionBatch2 = Factory.NewWithValidTestData<AccCollectionBatch>();
			collectionBatch2.ACB_BatchNumber = string.Empty;
			collectionBatch2.ACB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			AssertEquals("00001001", collectionBatch2.ACB_BatchNumber);
			AssertEquals(collectionBatch1.PK, uxmlSupport.LoadBusinessObjectFromCode(Factory, "00001000")?.PK);

			AssertNull("There is no business object matching the batch number", uxmlSupport.LoadBusinessObjectFromCode(Factory, "00000000"));
		}

		public void TestNullOrWhiteSpaceArqumentThrows()
		{
			var uxmlSupport = new CollectionBatchData().GetEDocsViaUniversalXmlSupport();
			AssertExceptionThrown<ArgumentException>("Factory Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(null, "00001000"));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be string empty", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, ""));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, null));
		}

		public void TestGetEDocViaUniversalXmlSupportExpectedCodeAndExampleCodeFormat()
		{
			AssertEquals("00001000", new CollectionBatchData().GetEDocsViaUniversalXmlSupport().ExampleCodeFormat);
			AssertEquals("BatchNumber", new CollectionBatchData().GetEDocsViaUniversalXmlSupport().ExpectedCodeFormat);
		}
	}
}
