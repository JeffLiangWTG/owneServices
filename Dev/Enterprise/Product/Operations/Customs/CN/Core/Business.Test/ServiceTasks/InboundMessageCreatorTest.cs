using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business.Testing
{
	public class InboundMessageCreatorTest : TestCaseWithFactory
	{
		public void TestCreateMessagesForInterchange()
		{
			var logger = new LoggingInformation();
			var messageContent = @"
<ImportAgrResponse>
	<ResponseInfo>
		<ResponseCode>1</ResponseCode>
		<ResponseMessage>导入成功</ResponseMessage>
		<CopCusCode>3117980008</CopCusCode>
	</ResponseInfo>
	<ConsignNo>20212243495988986</ConsignNo>
</ImportAgrResponse>";

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_InterchangeNum = "X0000001";
			interchange.EI_BodyText = messageContent;
			IInboundMessageCreator creator = new InboundMessageCreator(logger);
			AssertEquals("Should not have messages yet.", 0, interchange.ContainedMessages.Count);
			creator.CreateMessagesForInterchange(interchange);

			AssertEquals("Should have a message created.", 1, interchange.ContainedMessages.Count);
			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("EM_ApplicationCode", GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow, createdMessage.EM_ApplicationCode);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "X0000001", createdMessage.EM_MessageNum);
			AssertEquals("EM_MessageType", "ACD", createdMessage.EM_MessageType);
			AssertEquals("EM_MessageText", messageContent, createdMessage.EM_MessageText);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
		}

		public void TestIncorrectMessageText()
		{
			var logger = new LoggingInformation();
			var messageContent = @"
<ImportAgr1Response>
	<ResponseInfo>
		<ResponseCode>1</ResponseCode>
		<ResponseMessage>导入成功</ResponseMessage>
		<CopCusCode>3117980008</CopCusCode>
	</ResponseInfo>
	<ConsignNo>20212243495988986</ConsignNo>
</ImportAgr1Response>";

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			((IInboundMessageCreator)new InboundMessageCreator(logger)).CreateMessagesForInterchange(interchange);
			var query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			AssertEquals("Should not create message for interchange with empty text.", 0, Factory.Load<EDIMessage>(query).Length);
			AssertEquals("Status should be set ERR for interchange with empty text.", "ERR", interchange.EI_Status);
			AssertEquals("Error log.", "Invalid xml content or root element.", interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
			AssertCollectionContains("Error log in Logger.", "Invalid xml content or root element.", logger.Logs.Select(log => log.Message));
			logger.ClearLogs();

			var interchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange2.EI_BodyText = messageContent;
			((IInboundMessageCreator)new InboundMessageCreator(logger)).CreateMessagesForInterchange(interchange2);
			var query2 = new ZQuery(EDIMessageSchema.EM_EI, interchange2.PK);
			AssertEquals("Should not create message for interchange with incorrect text.", 0, Factory.Load<EDIMessage>(query2).Length);
			AssertEquals("Status should be set ERR for interchange with incorrect text.", "ERR", interchange2.EI_Status);
			AssertEquals("Error log in Inverchange.", "Invalid xml content or root element.", interchange2.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
			AssertCollectionContains("Error log in Logger.", "Invalid xml content or root element.", logger.Logs.Select(log => log.Message));
		}
	}
}
