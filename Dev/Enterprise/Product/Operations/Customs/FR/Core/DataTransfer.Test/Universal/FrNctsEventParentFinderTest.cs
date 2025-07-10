using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing
{
	class FRNctsEventParentFinderTest : TestCaseWithFactory
	{
		public void TestOnlyTheMessageMatchingInterchangeNumberShouldBeUpdated()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_JobReference = "NCTS001";

			var message1 = CreateTransmitMessageAndInterchange(ReceiveTransmitList.Codes.Transmit, "MSG001", EDIMessageStatusList.Codes.Sent, "INT001");
			var message1Rcv = CreateTransmitMessageAndInterchange(ReceiveTransmitList.Codes.Receive, "MSG001RCV", EDIMessageStatusList.Codes.ProcessedOK, "INT001");
			var message2 = CreateTransmitMessageAndInterchange(ReceiveTransmitList.Codes.Transmit, "MSG002", EDIMessageStatusList.Codes.Sent, "INT002");
			var message3 = CreateTransmitMessageAndInterchange(ReceiveTransmitList.Codes.Transmit, "MSG003", EDIMessageStatusList.Codes.Queued, "");

			Factory.Save();

			var testEventXml = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>NctsHeader</Type>
              <Key>NCTS001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>InterchangeNumber</Type>
            <Value>INT001</Value>
          </Context>
        </ContextCollection>
        <EventTime>TestTime</EventTime>
        <EventType>FRM</EventType>
        <IsEstimate>false</IsEstimate>
        <EventReference>MDL</EventReference>
      </Event>
    </UniversalEvent>";

			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(ZString.Format(testEventXml, Events.FrenchCustomsMessageStatusCode));
			_ = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals("The message matching interchange number but not rejected should keep status unchanged.", EDIMessageStatusList.Codes.Sent, message1.EM_Status);
			AssertEquals("Only the outgoing message should have status updated", EDIMessageStatusList.Codes.ProcessedOK, message1Rcv.EM_Status);
			AssertEquals("The message not matching interchange number should keep status unchanged.", EDIMessageStatusList.Codes.Sent, message2.EM_Status);
			AssertEquals("The message not matching interchange number should keep status unchanged.", EDIMessageStatusList.Codes.Queued, message3.EM_Status);

			var testEventXml2 = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>NctsHeader</Type>
              <Key>NCTS001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>InterchangeNumber</Type>
            <Value>INT001</Value>
          </Context>
        </ContextCollection>
        <EventTime>TestTime</EventTime>
        <EventType>FRM</EventType>
        <IsEstimate>false</IsEstimate>
        <EventReference>MNA</EventReference>
      </Event>
    </UniversalEvent>";

			xmlEvent = eventDeserializer.Parse(ZString.Format(testEventXml2, Events.FrenchCustomsMessageStatusCode));
			_ = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals("The message matching interchange number should have status updated.", EDIMessageStatusList.Codes.Rejected, message1.EM_Status);
			AssertEquals("Only the outgoing message should have status updated", EDIMessageStatusList.Codes.ProcessedOK, message1Rcv.EM_Status);
			AssertEquals("The message not matching interchange number should keep status unchanged.", EDIMessageStatusList.Codes.Sent, message2.EM_Status);
			AssertEquals("The message not matching interchange number should keep status unchanged.", EDIMessageStatusList.Codes.Queued, message3.EM_Status);

			EDIMessage CreateTransmitMessageAndInterchange(ZString messageDirection, ZString messageNumber, ZString messageStatus, ZString interchangeNumber)
			{
				var message = nctsHeader.Messages.AddNew(typeof(TestEDIMessage));
				message.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
				message.EM_ReceiveTransmit = messageDirection;
				message.EM_MessageNum = messageNumber;
				message.EM_Status = messageStatus;

				if (!interchangeNumber.IsEmpty)
				{
					var interchange = Factory.New<EDIInterchange>();
					interchange.EI_From = messageDirection == ReceiveTransmitList.Codes.Transmit ? "CW1" : "EASYLOG";
					interchange.EI_To = messageDirection == ReceiveTransmitList.Codes.Transmit ? "EASYLOG" : "CW1";
					interchange.EI_InterchangeNum = interchangeNumber;
					message.EM_EI = interchange.PK;
				}

				return message;
			}
		}

		public void TestGetAndUpdateEntryHeader_MDL()
		{
			AssertNctsHeaderAndOutgoingMessageStatus(Events.FrenchCustomsMessageStatusCode, CustomsMessageStatusList.Codes.MessageDeliveredToCustoms, EDIMessageStatusList.Codes.Queued, "");
		}

		public void TestGetAndUpdateEntryHeader_MAK()
		{
			AssertNctsHeaderAndOutgoingMessageStatus(Events.FrenchCustomsMessageStatusCode, CustomsMessageStatusList.Codes.MessageAcknowledged, EDIMessageStatusList.Codes.Queued, "");
		}

		public void TestGetAndUpdateEntryHeader_MND()
		{
			AssertNctsHeaderAndOutgoingMessageStatus(Events.FrenchCustomsMessageStatusCode, CustomsMessageStatusList.Codes.MessageNotDeliveredToCustoms, EDIMessageStatusList.Codes.Rejected, EDIMessageStatusList.Codes.Rejected);
		}

		public void TestGetAndUpdateEntryHeader_MNA()
		{
			AssertNctsHeaderAndOutgoingMessageStatus(Events.FrenchCustomsMessageStatusCode, CustomsMessageStatusList.Codes.MessageNotAcknowledged, EDIMessageStatusList.Codes.Rejected, EDIMessageStatusList.Codes.Rejected);
		}

		public void TestGetAndUpdateEntryHeader_MRJ()
		{
			AssertNctsHeaderAndOutgoingMessageStatus(Events.FrenchCustomsMessageStatusCode, CustomsMessageStatusList.Codes.MessageRejected, EDIMessageStatusList.Codes.Rejected, EDIMessageStatusList.Codes.Rejected);
		}

		void AssertNctsHeaderAndOutgoingMessageStatus(ZString eventType, ZString eventReference, ZString messageStatus, ZString status)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_JobReference = "NCTS001";

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "INT001";

			var message = (TestEDIMessage)nctsHeader.Messages.AddNew(typeof(TestEDIMessage));
			message.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			message.EM_EI = interchange.PK;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageText = "BLA,Bla MSG0001";
			Factory.Save();

			var testEventXml = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>NctsHeader</Type>
              <Key>NCTS001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>InterchangeNumber</Type>
            <Value>INT001</Value>
          </Context>
        </ContextCollection>
        <EventTime>TestTime</EventTime>
        <EventType>FRM</EventType>
        <IsEstimate>false</IsEstimate>
        <EventReference>{eventReference}</EventReference>
      </Event>
    </UniversalEvent>";

			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(ZString.Format(testEventXml, eventType));
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			var foundNctsHeader = (NctsHeader)logParents.First();
			AssertEquals("Existing NCTS header should match event.", "NCTS001", foundNctsHeader.BH_JobReference);
			AssertEquals("NCTS Header EffectiveMessageStatus should reflect event.", status, foundNctsHeader.EffectiveMessageStatus);
			AssertEquals("Status of the message matching interchange number should reflect event.", messageStatus, message.EM_Status);
		}

		FRNctsEventParentFinder GetNewEventParentFinderWithLogger(IXmlImportLogger logger) => new FRNctsEventParentFinder(Factory, new NctsHeaderDataContextManager(), logger);
	}
	class TestEDIMessage : EDIMessage
	{
		public TestEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber() => "test";
	}
}
