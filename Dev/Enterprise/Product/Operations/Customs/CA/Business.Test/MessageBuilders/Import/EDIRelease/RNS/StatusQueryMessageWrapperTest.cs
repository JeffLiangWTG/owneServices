using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Edifact.D96A.Elements;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class StatusQueryMessageWrapperTest : TestCaseWithFactory
	{
		[TestDate(2010, 8, 15, 10, 30, 25)]
		public void TestStatusQueryMessageWrapper()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("98765"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "98765");
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice1 = declaration.Invoices.AddNew();
				var line1 = invoice1.JobComInvoiceLines.AddNew();
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();

				var testStatusQueryMessageWrapper = new StatusQueryMessageWrapper(declaration.CustomsEntryHeaders[0]);
				AssertEquals("DateOfArrival", new ZDateTime(2010, 8, 15, 10, 30, 25), ((IRNSRequest)testStatusQueryMessageWrapper).DateOfArrival);
				AssertEquals("CargoControlNumber", ZString.Empty, ((IRNSRequest)testStatusQueryMessageWrapper).CargoControlNumber);
				AssertEquals("TransactionNumber", "98765000000012", ((IRNSRequest)testStatusQueryMessageWrapper).TransactionNumber);
				AssertEquals("OfficeCode", ZString.Empty, ((IRNSRequest)testStatusQueryMessageWrapper).OfficeCode);
				AssertEquals("SubLocationCode", ZString.Empty, ((IRNSRequest)testStatusQueryMessageWrapper).SubLocationCode);

				var builder = new RNSRequestMessageBuilder(testStatusQueryMessageWrapper, DocumentMessageNameCodedList.PreviousCustomsDocumentMessage);
				foreach (var builderResult in builder.PopulateMessages().GetBuilderResults())
				{
					var message = (EDIMessage)builderResult.Message;
					AssertMultilineASCIIEquals("StatusQueryMessageContent", expectedStatusQueryResult, message.EM_FormattedMessageText);
				}
			}
		}
		readonly ZString expectedStatusQueryResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:96A:UN
BGM+998
DTM+132:201008151030:203
RFF+TN:98765000000012
UNT+5+<<MSGNO PLACEHOLDER>>";
	}
}
