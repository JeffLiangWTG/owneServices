using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ZTestHelperTest : TestCaseWithFactory
	{
		public void TestPopulateSimpleTestDeclaration()
		{
			ZTestHelper testHelper = new ZTestHelper(Factory);
			AssertNull("Test Declaration should be null", testHelper.Declaration);
			testHelper.PopulateSimpleImportDeclaration();
			AssertEquals("One test header should have been created", 1, testHelper.Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
			AssertEquals("One test Line should have been created", 1, testHelper.Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.Count);
			AssertNotNull("Test Declaration Header", testHelper.Header1);
			AssertNotNull("Test Declaration Line", testHelper.Line1);
		}

		public void TestPopulateComplexTestDeclaration()
		{
			ZTestHelper testHelper = new ZTestHelper(Factory);
			testHelper.PopulateComplexImportDeclaration();
			AssertEquals("One test header should have been created", 1, testHelper.Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
			AssertEquals("One test Line should have been created", 6, testHelper.Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.Count);
			AssertNotNull("Test Declaration Header", testHelper.Header1);
			AssertNotNull("Test Declaration Line 1", testHelper.Line1);
			AssertNotNull("Test Declaration Line 2", testHelper.Line2);
			AssertNotNull("Test Declaration Line 3", testHelper.Line3);
			AssertNotNull("Test Declaration Line 4", testHelper.Line4);
			AssertNotNull("Test Declaration Line 5", testHelper.Line5);
			AssertNotNull("Test Declaration Line 6", testHelper.Line6);
		}

		public void TestPopulateSimpleQuarantineDeclaration()
		{
			var testHelper = new ZTestHelper(Factory);
			testHelper.PopulateSimpleQuarantineDeclaration();
			AssertEquals(JobMessageTypeList.Codes.Quarantine, testHelper.Declaration.JE_MessageType);
			AssertNotNull(testHelper.Header1);
			AssertNotNull(testHelper.Line1);
		}
	}
}
