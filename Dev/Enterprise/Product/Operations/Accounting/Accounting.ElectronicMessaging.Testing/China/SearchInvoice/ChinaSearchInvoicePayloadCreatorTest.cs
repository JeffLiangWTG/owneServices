using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.Accounting.ElectronicMessaging.China.Testing
{
	class ChinaSearchInvoicePayloadCreatorTest : TestCaseWithFactory
	{
		public void TestCreatePayloadAsJson()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 2m, TestObjectCreator.ABIGAS);
			var creator = new ChinaSearchEInvoicePayloadCreator(invoice) as IChinaPayloadCreator;

			AssertNotNull(creator);

			var generatedJson = creator.CreatePayloadAsJson();

			var searchInvoiceObj = (JObject)JsonConvert.DeserializeObject(generatedJson);

			AssertNotNull(searchInvoiceObj);
			AssertEquals("ReqType", "05", (string)searchInvoiceObj["reqType"]);

			var dataJson = (JObject)searchInvoiceObj["data"];

			AssertNotNull(dataJson);
			AssertEquals("serialNumber", invoice.PK.ToString() + "|EDIEDIDAT", (string)dataJson["serialNumber"]);
			AssertEquals("extend", "PDF", (string)dataJson["extend"]);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;
	}
}
