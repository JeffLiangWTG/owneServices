using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EdecXmlSchemaErrorMessageProcessor))]
sealed class EdecXmlSchemaErrorMessageProcessorTest : BaseInboundMessageProcessorTest
{
	protected override (string MessageType, Event ExpectedEvent)[] MessageTypes => new[] { (JobMessageTypeList.Codes.Import, Events.DeclarationRejected), (JobMessageTypeList.Codes.Export, Events.DeclarationRejected) };

	protected override ZString ApplicationCode => ApplicationCodeList.Codes.CHCustomsEdec;

	protected override ZString MessageSubType => MessageSubTypeCodeList.Codes.XmlSchemaError;

	protected override ApplicationTypeMessageProcessor GetMessageProcessor(LoggingInformation logger) => new EdecXmlSchemaErrorMessageProcessor(logger);

	protected override ZString ExpectedMessageFriendlyName => "XML Schema Error Message Processor";

	protected override ZString BGMReference => "";

	readonly ZString rejectionDate = "2021-11-26";

	readonly ZString rejectionTime = "08:31:45";

	#region Message Response

	protected override ZString ReceivedMessage => $@"<?xml version= ""1.0"" encoding= ""UTF-8""?>
       <goodsDeclarationsResponse schemaVersion=""4.0"" xmlns=""http://www.e-dec.ch/xml/schema/edecResponse/v4"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecResponse/v4 http://www.ezv.admin.ch/pdf_linker.php?doc=edecResponse_v_4_0"" >
        <goodsDeclarationRejection>
          <rejectionDate>{rejectionDate}</rejectionDate>
            <rejectionTime>{rejectionTime}</rejectionTime>
            <errors>
            <XMLSchemaErrors>
              <schema>
                <location>http://www.ezv.admin.ch/pdf_linker.php?doc=edec_v_4_0</location>
                  <namespace>http://www.e-dec.ch/xml/schema/edec/v4</namespace>
                  <version>4.0</version>
                  </schema>
                <parser>
                <name>Xerces-J 2.12.0</name>
                  </parser>
                <error>
                <message>Parsing Error: Line: 10, URI: null, Message: cvc-type.3.1.3: The value &amp;apos;&amp;apos; of element &amp;apos;language&amp;apos; is not valid.</message>
                  </error>
                </XMLSchemaErrors>
              </errors>
            </goodsDeclarationRejection>
          </goodsDeclarationsResponse>";

	#endregion

	public override (CusEntryHeader entryHeader, EDIMessage ediMessage) PrepareDataForLinkToParent_Success(BusinessObjectFactory factory, ZString messageType)
			=> MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(factory, ApplicationCodeList.Codes.CHCustomsEdec, messageType, string.Empty, MessageSubType, ReceivedMessage);

	public void TestResponseErrorMessage()
	{
		CombineAssertions(() =>
		{
			foreach (var messageType in MessageTypes)
			{
				var factory = new BusinessObjectFactory();

				var (entryHeader, receivedEdiMessage) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(factory, ApplicationCodeList.Codes.CHCustomsEdec, messageType.MessageType, string.Empty, MessageSubType, ReceivedMessage);
				entryHeader.CH_PhaseStatus = PassarDeclarationPhaseList.Codes.Amendment;
				factory.Save();

				MessageProcessorTestHelper.ProcessMessage(Processor, receivedEdiMessage);
				factory.Save();

				var sentEdiMessage = receivedEdiMessage.Factory.GetOutgoingMessageFromSessionId(receivedEdiMessage.Interchange.EI_SessionGUID);
				var logEntry = factory.Load<StmALog>(new ZQuery(ZArchitecture.Schema.StmALogSchema.SL_Parent, entryHeader.PK)).FirstOrDefault();

				AssertEquals("CH_Status", CHLogicalStatusList.Codes.Invalid, entryHeader.CH_Status);
				AssertEquals("CH_PhaseStatus", PassarDeclarationPhaseList.Codes.Amendment, entryHeader.CH_PhaseStatus);
				AssertNotNull("CusEntryHeader log", logEntry);
				AssertEquals("Event type", messageType.ExpectedEvent, logEntry.Event);
				AssertEquals("EM_LinkTable should be CusEntryHeader", "CusEntryHeader", receivedEdiMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", sentEdiMessage.EM_LinkUniqueID, receivedEdiMessage.EM_LinkUniqueID);
				AssertEquals($"SL_EventTimeOffset", $"{rejectionDate} {rejectionTime}", logEntry.SL_EventTimeOffset.ToString("yyyy-MM-dd HH:mm:ss"));
			}
		});
	}
}
