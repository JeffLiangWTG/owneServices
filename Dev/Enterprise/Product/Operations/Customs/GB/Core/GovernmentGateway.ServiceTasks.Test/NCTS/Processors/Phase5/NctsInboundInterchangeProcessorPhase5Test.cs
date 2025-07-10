using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	class NctsInboundInterchangeProcessorPhase5Test : TestCaseWithFactory
	{
		public void TestSpawnOneRealExampleWithXml()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "NCT00001";
			var outgoingMessage = nctsHeader.Messages.AddNew();
			outgoingMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCustomsNCTS);
			outgoingMessage.EM_MessageNum = "9";

			var interchangeBody = CTCMessageBuilderUtilities.GetEmbeddedResourceFile("TestResponseInterchangeFromEhub.xml", "Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Testing.NCTS.Processors.Phase5.TestFiles.");
			interchangeBody = interchangeBody.Replace("{{COMMONACCESSREFERENCE}}", GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outgoingMessage));

			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "1";
			outgoingInterchange.EI_From = "CCSUK";
			outgoingInterchange.EI_To = "WISETECHGLOBAL";
			outgoingMessage.EM_EI = outgoingInterchange.PK;

			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.GbCustomsNCTS;
			incomingInterchange.EI_InterchangeNum = "00001";
			incomingInterchange.EI_Status = EDIInterchange.Status.Queued;
			incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingInterchange.EI_BodyText = interchangeBody;

			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new NctsInboundInterchangeProcessorPhase5(logger);
			processor.ExecuteBatch();

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbCustomsNCTS);
			query.AddToFilter(EDIMessageSchema.EM_EI, new[] { incomingInterchange.PK });
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;

			var message = new BusinessObjectFactory().LoadTop1<EDIMessage>(query);

			AssertStartsWith("Message should be CC019C", "<q1:CC019C ", message.EM_MessageText);
			AssertEquals("Message.EM_ApplicationReference", "16763", message.EM_ApplicationReference);
			AssertEquals("16763", message.EM_MessageNum);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("19C", message.EM_MessageSubType);
			AssertEquals("QUE", message.EM_Status);
			AssertEquals("RCV", message.EM_ReceiveTransmit);
			AssertEquals(incomingInterchange.EI_ApplicationCode, message.EM_MessageType);
			AssertEquals(EDIMessage.ApplicationCodes.GbCustomsNCTS, message.EM_MessageType);
		}
	}
}
