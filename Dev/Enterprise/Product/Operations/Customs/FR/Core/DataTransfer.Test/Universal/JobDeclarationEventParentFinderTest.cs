using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing
{
	class JobDeclarationEventParentFinderTest : TestCaseWithFactory
	{
		public void TestOnlyTheMessageMatchingInterchangeNumberAndRejectedStatusShouldBeUpdated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "DEC001";
			var entry1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_BGMReference = "BGM001";
			entry1.CorrelationID = "CID001";
			var entry2 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_BGMReference = "BGM001";
			entry2.CorrelationID = "CID002";

			var message1 = CreateTransmitMessageAndInterchange(entry1, "CW1", "EASYLOG", ReceiveTransmitList.Codes.Transmit, "MSG001", EDIMessageStatusList.Codes.Sent, "INT001");
			var message1Rcv = CreateTransmitMessageAndInterchange(entry1, "EASYLOG", "CW11", ReceiveTransmitList.Codes.Receive, "MSG001RCV", EDIMessageStatusList.Codes.ProcessedOK, "INT001");
			var message2 = CreateTransmitMessageAndInterchange(entry1, "CW1", "EASYLOG", ReceiveTransmitList.Codes.Transmit, "MSG002", EDIMessageStatusList.Codes.Sent, "INT002");
			var message3 = CreateTransmitMessageAndInterchange(entry1, "CW1", "EASYLOG", ReceiveTransmitList.Codes.Transmit, "MSG003", EDIMessageStatusList.Codes.Queued, "");

			var message1_2 = CreateTransmitMessageAndInterchange(entry2, "CW1", "EASYLOG_2", ReceiveTransmitList.Codes.Transmit, "MSG001_2", EDIMessageStatusList.Codes.Sent, "INT001");

			Factory.Save();

			var testEventXml = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>CustomsDeclaration</Type>
              <Key>DEC001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>InterchangeNumber</Type>
            <Value>INT001</Value>
          </Context>
          <Context>
            <Type>CorrelationID</Type>
            <Value>CID001</Value>
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
			AssertEquals("The message not matching correlation ID should keep status unchanged.", EDIMessageStatusList.Codes.Sent, message1_2.EM_Status);

			var testEventXml2 = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>CustomsDeclaration</Type>
              <Key>DEC001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>InterchangeNumber</Type>
            <Value>INT001</Value>
          </Context>
          <Context>
            <Type>CorrelationID</Type>
            <Value>CID001</Value>
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
			AssertEquals("The message not matching correlation ID should keep status unchanged.", EDIMessageStatusList.Codes.Sent, message1_2.EM_Status);

			EDIMessage CreateTransmitMessageAndInterchange(CusEntryHeader entry, ZString from, ZString to, ZString messageDirection, ZString messageNumber, ZString messageStatus, ZString interchangeNumber)
			{
				var message = entry.Messages.AddNew(typeof(TestEDIMessage));
				message.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
				message.EM_ReceiveTransmit = messageDirection;
				message.EM_MessageNum = messageNumber;
				message.EM_Status = messageStatus;

				if (!interchangeNumber.IsEmpty)
				{
					var interchange = Factory.New<EDIInterchange>();
					interchange.EI_From = from;
					interchange.EI_To = to;
					interchange.EI_InterchangeNum = interchangeNumber;
					message.EM_EI = interchange.PK;
				}

				return message;
			}
		}

		public void TestGetLogParentsForEventUsingContext_NotReturnEnumerablesContainingNull()
		{
			var declaration = Factory.New<JobDeclaration>();
			var e = declaration.ActiveEntryHeaders.AddNew();
			e.CH_BGMReference = "123456";
			var msg = (TestEDIMessage)e.Messages.AddNew(typeof(TestEDIMessage));
			msg.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			msg.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			msg.EM_Status = EDIMessageStatusList.Codes.Queued;
			msg.EM_MessageText = "BLA,Bla MSG0001";
			Factory.Save();

			var testEventXml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<EventTime>2015-04-16T08:12:21.637</EventTime>
		<EventType>{0}</EventType>

		<ContextCollection>
			<Context>
				<Type>EntryNumber</Type>
				<Value></Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(ZString.Format(testEventXml, EDIMessageStatusList.Codes.Acknowledged));

			AssertNoExceptionThrown("When import not valid XML, no exceptions are thrown", () =>
			{
				var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertNull("LogParent couldn't be enumerables containing null but null", logParents);
			});
		}

		public void TestGetAndUpdateEntryHeader_MDL()
		{
			AssertEntryAndOutgoingMessageStatus(Events.FrenchCustomsMessageStatusCode, CustomsMessageStatusList.Codes.MessageDeliveredToCustoms, EDIMessageStatusList.Codes.Queued, "");
		}

		public void TestGetAndUpdateEntryHeader_MAK()
		{
			AssertEntryAndOutgoingMessageStatus(Events.FrenchCustomsMessageStatusCode, CustomsMessageStatusList.Codes.MessageAcknowledged, EDIMessageStatusList.Codes.Queued, "");
		}

		public void TestGetAndUpdateEntryHeader_MND()
		{
			AssertEntryAndOutgoingMessageStatus(Events.FrenchCustomsMessageStatusCode, CustomsMessageStatusList.Codes.MessageNotDeliveredToCustoms, EDIMessageStatusList.Codes.Rejected, EDIMessageStatusList.Codes.Rejected);
		}

		public void TestGetAndUpdateEntryHeader_MNA()
		{
			AssertEntryAndOutgoingMessageStatus(Events.FrenchCustomsMessageStatusCode, CustomsMessageStatusList.Codes.MessageNotAcknowledged, EDIMessageStatusList.Codes.Rejected, EDIMessageStatusList.Codes.Rejected);
		}

		public void TestGetAndUpdateEntryHeader_MRJ()
		{
			AssertEntryAndOutgoingMessageStatus(Events.FrenchCustomsMessageStatusCode, CustomsMessageStatusList.Codes.MessageRejected, EDIMessageStatusList.Codes.Rejected, EDIMessageStatusList.Codes.Rejected);
		}

		public void TestCinEventLinksToEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "12345";
			Factory.Save();

			AssertCinEventLinksToEntryHeader(entryHeader, "CIN", "STA", "12345", "MAN_REC", true, "REC");
			AssertCinEventLinksToEntryHeader(entryHeader, "MAA", "STA", "12345", "MAN_REC", false, "");
			AssertCinEventLinksToEntryHeader(entryHeader, "CIN", "ERR", "12345", "MAN_REC", true, "");
			AssertCinEventLinksToEntryHeader(entryHeader, "CIN", "STA", "CRAP+BOLLOCKS+12345", "MAN_REC", true, "REC");
			AssertCinEventLinksToEntryHeader(entryHeader, "CIN", "STA", "CRAP+12345", "MAN_REC", false, "");
			AssertCinEventLinksToEntryHeader(entryHeader, "CIN", "STA", "123456", "MAN_REC", false, "");
			AssertCinEventLinksToEntryHeader(entryHeader, "CIN", "STA", "12345", "331_AEA", true, "AEA");
			AssertCinEventLinksToEntryHeader(entryHeader, "CIN", "STA", "12345", "331_SRA", true, "SRA");
		}

		void AssertCinEventLinksToEntryHeader(Customs.Business.CusEntryHeader entryHeader, string eventType, string eventReference, string entryReference, string statusCode, bool expectedIsMatch, string expectedStatusCode)
		{
			entryHeader.CH_EntryStatus = "";
			Factory.Save();

			var testEventXml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<EventType>{0}</EventType>
		<EventReference>{1}</EventReference>
		<ContextCollection>
			<Context>
				<Type>EntryReference</Type>
				<Value>{2}</Value>
			</Context>
			<Context>
				<Type>StatusCode</Type>
				<Value>{3}</Value>
			</Context>
			<Context>
				<Type>StatusDescription</Type>
				<Value>Réception du manifeste ET du départ du moyen de transport</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(ZString.Format(testEventXml, eventType, eventReference, entryReference, statusCode));
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			if (expectedIsMatch)
			{
				var matchEntry = (CusEntryHeader)logParents.First();
				AssertEquals(entryHeader.PK, matchEntry.PK);
				AssertEquals(expectedStatusCode, matchEntry.CH_EntryStatus);
			}
			else
			{
				AssertNull(logParents);
				AssertEquals(expectedStatusCode, entryHeader.CH_EntryStatus);
			}
		}

		void AssertEntryAndOutgoingMessageStatus(ZString eventType, ZString eventReference, ZString messageStatus, ZString status)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "DEC001";
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "123456";
			entryHeader.CorrelationID = "00001";

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "INT001";

			var message = (TestEDIMessage)entryHeader.Messages.AddNew(typeof(TestEDIMessage));
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
              <Type>CustomsDeclaration</Type>
              <Key>DEC001</Key>
            </DataTarget>
          </DataTargetCollection>
		  <Company>ABN</Company>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>InterchangeNumber</Type>
            <Value>INT001</Value>
          </Context>
          <Context>
            <Type>CorrelationID</Type>
            <Value>00001</Value>
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
			var foundEntry = logParents.First();
			AssertEquals("Existing entry should match event.", foundEntry, entryHeader);
			AssertEquals("Entry CH_Status should reflect event.", status, entryHeader.CH_Status);
			AssertEquals("Status of the message matching interchange number should reflect event.", messageStatus, message.EM_Status);
		}

		JobDeclarationEventParentFinder GetNewEventParentFinderWithLogger(IXmlImportLogger logger)
			=> new JobDeclarationEventParentFinder(Factory, new Customs.DataTransfer.Universal.JobDeclarationDataContextManager(), logger);

		class TestEDIMessage : Business.EdiMessages.FREDIMessage
		{
			public TestEDIMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber() => "test";
		}
	}
}
