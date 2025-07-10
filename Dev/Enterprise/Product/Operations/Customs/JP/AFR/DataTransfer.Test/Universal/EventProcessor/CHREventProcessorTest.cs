using CargoWise.EntityFramework;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	[TestedType(typeof(CHREventProcessor))]
	class CHREventProcessorTest : AHREventProcessorAbstractTest<CHREventProcessor>
	{
		protected override CHREventProcessor GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			return new CHREventProcessor(eventDataObject, logger, factory, DefaultDataObjectWriterStrategy.Instance);
		}

		[TestDate(2017, 10, 10)]
		public void TestAutoTriggerCompletionMessageGeneration_CHRADD()
		{
			var eventXmlText = string.Format(@"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <ActionPurpose>
        <Code>{0}</Code>
      </ActionPurpose>
      <DataProvider>AFR</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Type>AFRHeader</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2013-10-01T21:23:57.71</EventTime>
    <EventType>MSC</EventType>
    <EventReference>{0}-ACCEPTED</EventReference>

    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MB202576543</Value>
      </Context>
      <Context>
        <Type>HBOLNumber</Type>
        <Value>HB32342</Value>
      </Context>
      <Context>
        <Type>NotificationDetails</Type>
        <Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
", MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse);
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MB202576543";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "HB89556";
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "HB32342";
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillAdd;
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals("bill1.JPB_MessageStatus", MessageStatusList.Codes.AwaitingHouseBillRegistration, bill1.JPB_MessageStatus);
			AssertEquals("bill1.JPB_ReleaseStatus", AFRBillCustomsStatusList.Codes.Registered, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", MessageStatusList.Codes.ClearHouseBillAdd, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", AFRBillCustomsStatusList.Codes.Registered, bill2.JPB_ReleaseStatus);
			AssertHasEmail("Update Advance Cargo Information Registration" + " Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress); // send to group
			AssertEquals(1, header.Messages.Count);

			var message = header.Messages[0];
			AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
			AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouseCompletion, message.EM_MessageOwner);
			AssertMultilineASCIIEquals("message.EM_MessageText", @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>AFR00000001</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>CHR</Code>
        <Description>Update Advance Cargo Information Registration House</Description>
      </ActionPurpose>
      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>AFR</Code>
          <Description>Japan Customs Advance Filing Rules</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>BNE</Code>
      <Name>BN - AUBNE</Name>
    </Branch>
    <PortOfDischarge>
      <Code></Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code></Code>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName></VesselName>
    <VoyageFlightNo></VoyageFlightNo>
    <WayBillNumber>MB202576543</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPCarrierCode</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingSuffix</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSign</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselDetailsChanged</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>__(AFR MESSAGE NO)__</Value>
      </AddInfo>
    </AddInfoCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>

          <ActionPurpose>
            <Code>ADD</Code>
            <Description>Add</Description>
          </ActionPurpose>
          <TriggerCount>0</TriggerCount>
          <TriggerDescription></TriggerDescription>
          <TriggerType>Manual</TriggerType>

          <DataTargetCollection>
            <DataTarget>
              <Type>AFRBill</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>


        <AddInfoCollection>
          <AddInfo>
            <Key>JPHouseBillRegisterCompletion</Key>
            <Value>Y</Value>
          </AddInfo>
        </AddInfoCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
", message.EM_MessageText);
			var interchange = message.Interchange;
			var reloadMessage = Factory.Load<XmlEDIMessage>(message.PK);
			AssertEquals("Interchange message should be the same as Header message", reloadMessage, interchange.ContainedMessages[0]);
			AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
			AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
			AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
			AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
			AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
			AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
		}

		[TestDate(2017, 10, 10)]
		public void TestAutoTriggerCompletionMessageGeneration_AHR()
		{
			var eventXmlText = string.Format(@"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <ActionPurpose>
        <Code>{0}</Code>
      </ActionPurpose>
      <DataProvider>AFR</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Type>AFRHeader</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2013-10-01T21:23:57.71</EventTime>
    <EventType>MSC</EventType>
    <EventReference>{0}-ACCEPTED</EventReference>

    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MB202576543</Value>
      </Context>
      <Context>
        <Type>HBOLNumber</Type>
        <Value>HB32342</Value>
      </Context>
      <Context>
        <Type>NotificationDetails</Type>
        <Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse);
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MB202576543";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "HB32342";
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals("bill1.JPB_MessageStatus", MessageStatusList.Codes.ClearHouseBillRegistration, bill1.JPB_MessageStatus);
			AssertEquals("bill1.JPB_ReleaseStatus", AFRBillCustomsStatusList.Codes.Registered, bill1.JPB_ReleaseStatus);
			AssertHasEmail("Advance Cargo Information Registration" + " Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress); // send to group
			AssertEquals(1, header.Messages.Count);

			var message = header.Messages[0];
			AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
			AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouseCompletion, message.EM_MessageOwner);
			AssertMultilineASCIIEquals("message.EM_MessageText", @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>AFR00000001</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>AHR</Code>
        <Description>Advance Cargo Information Registration House</Description>
      </ActionPurpose>
      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>AFR</Code>
          <Description>Japan Customs Advance Filing Rules</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>BNE</Code>
      <Name>BN - AUBNE</Name>
    </Branch>
    <PortOfDischarge>
      <Code></Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code></Code>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName></VesselName>
    <VoyageFlightNo></VoyageFlightNo>
    <WayBillNumber>MB202576543</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPCarrierCode</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingSuffix</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSign</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselDetailsChanged</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>__(AFR MESSAGE NO)__</Value>
      </AddInfo>
    </AddInfoCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>

          <ActionPurpose>
            <Code>REG</Code>
            <Description>Registration</Description>
          </ActionPurpose>
          <TriggerCount>0</TriggerCount>
          <TriggerDescription></TriggerDescription>
          <TriggerType>Manual</TriggerType>

          <DataTargetCollection>
            <DataTarget>
              <Type>AFRBill</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>


        <AddInfoCollection>
          <AddInfo>
            <Key>JPHouseBillRegisterCompletion</Key>
            <Value>Y</Value>
          </AddInfo>
        </AddInfoCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
", message.EM_MessageText);
			var interchange = message.Interchange;
			var reloadMessage = Factory.Load<XmlEDIMessage>(message.PK);
			AssertEquals("Interchange message should be the same as Header message", reloadMessage, interchange.ContainedMessages[0]);
			AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
			AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
			AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
			AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
			AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
			AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
		}

		public void TestProcessClearEventFromJapanCustomsForCHR()
		{
			AssertProcessClearEventFromJapanCustomsForHouse(MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, MessageStatusList.Codes.AwaitingHouseBillDelete, MessageStatusList.Codes.ClearHouseBillDelete, "Update Advance Cargo Information Registration");
			AssertProcessClearEventFromJapanCustomsForHouse(MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, MessageStatusList.Codes.AwaitingHouseBillUpdate, MessageStatusList.Codes.ClearHouseBillUpdate, "Update Advance Cargo Information Registration");
		}

		public void TestProcessErrorEventFromJapanCustomsForCHR()
		{
			AssertProcessErrorEventFromJapanCustomsForHouse(MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, MessageStatusList.Codes.AwaitingHouseBillAdd, MessageStatusList.Codes.ErrorHouseBillAdd, "Update Advance Cargo Information Registration");
			AssertProcessErrorEventFromJapanCustomsForHouse(MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, MessageStatusList.Codes.AwaitingHouseBillDelete, MessageStatusList.Codes.ErrorHouseBillDelete, "Update Advance Cargo Information Registration");
			AssertProcessErrorEventFromJapanCustomsForHouse(MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, MessageStatusList.Codes.AwaitingHouseBillUpdate, MessageStatusList.Codes.ErrorHouseBillUpdate, "Update Advance Cargo Information Registration");
		}

		public void TestBillRegistrationCompleteLogIsCancelledIfAllBillAreDeleted()
		{
			#region TestMessage
			var eventXmlTextBase = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <ActionPurpose>
        <Code>CHR</Code>
      </ActionPurpose>
      <DataProvider>AFR</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Type>AFRHeader</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2013-10-01T21:23:57.71</EventTime>
    <EventType>MSC</EventType>
    <EventReference>CHR-ACCEPTED</EventReference>

    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MB202576543</Value>
      </Context>
      <Context>
        <Type>HBOLNumber</Type>
        <Value>{0}</Value>
      </Context>
      <Context>
        <Type>NotificationDetails</Type>
        <Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			#endregion
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MB202576543";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "HB89556";
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillDelete;
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "HB32342";
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillDelete;
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.LogBillRegistrationCompletion();
			header.LogBillRegistrationCompletion();
			Factory.SaveForTesting();
			var logs = header.Logs.Find(JPAFRHeader.BillRegistrationCompletedQuery);
			AssertEquals(2, logs.Length);
			var log1 = logs[0];
			var log2 = logs[1];

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlTextBase, "HB32342"));
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals("header.IsBillRegistrationCompleted", true, header.IsBillRegistrationCompleted);
			AssertEquals("bill1.JPB_MessageStatus", MessageStatusList.Codes.AwaitingHouseBillDelete, bill1.JPB_MessageStatus);
			AssertEquals("bill1.JPB_ReleaseStatus", AFRBillCustomsStatusList.Codes.Registered, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", MessageStatusList.Codes.ClearHouseBillDelete, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", AFRBillCustomsStatusList.Codes.NotRegistered, bill2.JPB_ReleaseStatus);
			AssertHasEmail("Update Advance Cargo Information Registration Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress);
			AssertEquals("log1.IsCancelled", false, log1.IsCancelled);
			AssertEquals("log2.IsCancelled", false, log2.IsCancelled);
			logs = header.Logs.Find(JPAFRHeader.BillRegistrationCompletedQuery);
			AssertEquals(2, logs.Length);
			AssertCollectionContains(log1, logs);
			AssertCollectionContains(log2, logs);

			xmlEvent = eventDeserializer.Parse(string.Format(eventXmlTextBase, "HB89556"));
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals("header.IsBillRegistrationCompleted", false, header.IsBillRegistrationCompleted);
			AssertEquals("bill1.JPB_MessageStatus", MessageStatusList.Codes.ClearHouseBillDelete, bill1.JPB_MessageStatus);
			AssertEquals("bill1.JPB_ReleaseStatus", AFRBillCustomsStatusList.Codes.NotRegistered, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", MessageStatusList.Codes.ClearHouseBillDelete, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", AFRBillCustomsStatusList.Codes.NotRegistered, bill2.JPB_ReleaseStatus);
			AssertHasEmail("Update Advance Cargo Information Registration Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress);
			AssertEquals("log1.IsCancelled", true, log1.IsCancelled);
			AssertEquals("log2.IsCancelled", true, log2.IsCancelled);
			logs = header.Logs.Find(JPAFRHeader.BillRegistrationCompletedQuery);
			AssertEquals(0, logs.Length);
		}
	}
}
