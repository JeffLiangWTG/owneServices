using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(QueryCADMessageManager))]
	sealed class QueryCADMessageManagerTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var manager = new QueryCADMessageManager(EntryHeader);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			manager.Validate();
			AssertEquals("Entry should have a valid CAD Submitted date.", UnitTestUserNotification.Instance.LastMessage.Text);

			EntryHeader.CH_EntrySubmittedDate = new ZDateTime(2022, 12, 12);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			manager.Validate();
			AssertEquals("CARM API Key and CARM End Point need to have a valid value, please setup in Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CARM -> CARM API Key and Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CARM -> CARM End Point.", UnitTestUserNotification.Instance.LastMessage.Text);

			using (CACustomsDataRegistry.Instance.CARMAPIKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test"))
			using (CACustomsDataRegistry.Instance.CARMEndPoint.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://Test.com"))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				manager.Validate();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuery()
		{
			using (CACustomsDataRegistry.Instance.CARMAPIKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test"))
			using (CACustomsDataRegistry.Instance.CARMEndPoint.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://Test.com"))
			{
				EntryHeader.CH_EntrySubmittedDate = new ZDateTime(2022, 12, 12);
				var manager = new QueryCADMessageManagerForTest(EntryHeader);
				manager.ResponseStatus =  HttpStatusCode.Forbidden;
				manager.ResponseContent = "Test Response Content";
				manager.Query();
				AssertEquals(0, EntryHeader.Messages.Count);

				manager.ResponseContent = $"<DocumentMetaData xmlns=\"urn:wco:datamodel:WCO:Declaration:1\">\r\n    <ResponsibleAgencyName>CBSA</ResponsibleAgencyName>\r\n    <AgencyAssignedCustomizationCode>CAD</AgencyAssignedCustomizationCode>\r\n    <AgencyAssignedCustomizationVersionCode>001</AgencyAssignedCustomizationVersionCode>\r\n    <FunctionalDefinition>CAD-OUT</FunctionalDefinition>\r\n    <CommunicationMetaData/>\r\n    <Response>\r\n        <IssueDateTime>\r\n            <DateTimeString>20240102005453</DateTimeString>\r\n        </IssueDateTime>\r\n        <Error>\r\n            <Description>No Data Found or Access Denied</Description>\r\n            <ValidationCode>077</ValidationCode>\r\n            <Pointer>\r\n                <Location>DocumentMetaData/Declaration</Location>\r\n            </Pointer>\r\n        </Error>\r\n        <Status>\r\n            <NameCode>41</NameCode>\r\n        </Status>\r\n    </Response>\r\n</DocumentMetaData>";
				manager.Query();
				AssertEquals(2, EntryHeader.Messages.Count);
				var sentMessage = EntryHeader.Messages.Where(m => m.EM_ReceiveTransmit == EDIMessage.Direction.Transmit).FirstOrDefault();
				AssertEquals(EDIMessage.Status.Sent, sentMessage.EM_Status);
				AssertEquals("QueryUrl: http://Test.com?TransactionNumber=\nx-api-key: Test", sentMessage.EM_MessageText);

				var recMessage = EntryHeader.Messages.Where(m => m.EM_ReceiveTransmit == EDIMessage.Direction.Receive).FirstOrDefault();
				AssertEquals(EDIMessage.Status.Queued, recMessage.EM_Status);
				AssertEquals(sentMessage.EM_MessageNum, recMessage.EM_MessageNum);

				EntryHeader.CH_BGMReference = "00581155";
				manager.Query();
				AssertEquals(4, EntryHeader.Messages.Count);
				sentMessage = EntryHeader.Messages.Where(m => m.EM_ReceiveTransmit == EDIMessage.Direction.Transmit).LastOrDefault();
				AssertEquals(EDIMessage.Status.Sent, sentMessage.EM_Status);
				AssertEquals("QueryUrl: http://Test.com?TransactionNumber=00581155\nx-api-key: Test", sentMessage.EM_MessageText);
			}
		}

		public void TestQueryFailed()
		{
			using (CACustomsDataRegistry.Instance.CARMAPIKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test"))
			using (CACustomsDataRegistry.Instance.CARMEndPoint.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://Test.com"))
			{
				EntryHeader.CH_EntrySubmittedDate = new ZDateTime(2022, 12, 12);
				var manager = new QueryCADMessageManagerForTest(EntryHeader);
				manager.ResponseStatus = HttpStatusCode.OK;
				manager.ResponseContent = "";
				manager.Query();
				AssertEquals(0, EntryHeader.Messages.Count);
				AssertEquals("CAD Query Failed, Response results ''.", UnitTestUserNotification.Instance.LastMessage.Text);

				manager.ResponseContent = "Test Response.";
				manager.Query();
				AssertEquals(0, EntryHeader.Messages.Count);
				AssertEquals("CAD Query Failed, Response results 'Test Response.'.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		CusEntryHeader EntryHeader
		{
			get
			{
				if (entryHeader == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = declaration.InvoiceLines.AddNew();
					invoiceLine.JI_CustomsQuantity = 100m;
					invoiceLine.JI_CustomsUnitQty = "KGM";
					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
					declaration.DoMerge();
					entryHeader = declaration.ActiveEntryHeaders.AddNew();
				}
				return entryHeader;
			}
		}
		CusEntryHeader entryHeader;
	}

	class QueryCADMessageManagerForTest : QueryCADMessageManager
	{
		public QueryCADMessageManagerForTest(CusEntryHeader entry) : base(entry)
		{
		}

		public override HttpClient GetHttpClient => new HttpClient(GetMockHttpMsgHandler());

		public string ResponseContent
		{
			get
			{
				if (responseContent == null)
				{
					responseContent = "";
				}
				return responseContent;
			}
			set
			{
				responseContent = value;
			}
		}
		string responseContent;

		public HttpStatusCode ResponseStatus
		{
			get
			{
				return responseStatus;
			}
			set
			{
				responseStatus = value;
			}
		}
		HttpStatusCode responseStatus = HttpStatusCode.OK;

		HttpMessageHandler GetMockHttpMsgHandler()
		{
			var mockHttpMsgHandler = new Mock<HttpMessageHandler>();
			mockHttpMsgHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync(new HttpResponseMessage { StatusCode = ResponseStatus, Content = new StringContent(ResponseContent) });

			return mockHttpMsgHandler.Object;
		}
	}
}
