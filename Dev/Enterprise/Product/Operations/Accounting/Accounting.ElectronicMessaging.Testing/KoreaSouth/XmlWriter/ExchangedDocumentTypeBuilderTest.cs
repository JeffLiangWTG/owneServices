using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EInvoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	public class ExchangedDocumentTypeBuilderTest : TestCaseWithFactory
	{
		[TestDate(2022, 03, 03, 15, 15, 00)]
		public void TestBuildExchangedDocument()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();

			var element = new ExchangedDocumentTypeBuilder("TestNameSpace", accBatch).Build("ExchangedDocument");

			var expectedResult = GetExpectedResult(
				"ExchangedDocument",
				accBatch.AIB_BatchNumber,
				"20220303000000",
				ReadyKoreaConstants.BusinessRegistrationNumber);

			AssertEquals(expectedResult, element.ToString());
		}

		[TestDate(2022, 03, 03, 15, 15, 00)]
		public void TestBuildEmptyExchangedDocument_ID()
		{
			var accBatch = Factory.New<AccEInvoicingBatch>();

			var element = new ExchangedDocumentTypeBuilder("TestNameSpace", accBatch).Build("ExchangedDocument");

			var expectedResult = GetExpectedResult(
				"ExchangedDocument",
				0,
				"20220303000000",
				ReadyKoreaConstants.BusinessRegistrationNumber);

			AssertEquals(expectedResult, element.ToString());
		}

		string GetExpectedResult(string nodeName, int batchNum, string dateTime, string id) => $@"<{nodeName} xmlns=""TestNameSpace"">
  <ID>{batchNum}</ID>
  <IssueDateTime>{dateTime}</IssueDateTime>
  <ReferencedDocument>
    <ID>{id}</ID>
  </ReferencedDocument>
</{nodeName}>";
	}
}

