using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	abstract class AHREventProcessorAbstractTest<T> : AFREventProcessorAbstractTest<T>
		where T : AFREventProcessor
	{
		protected void AssertProcessClearEventFromJapanCustomsForHouse(ZString messageType, ZString existingMessageStatus, ZString expectedMessageStatus, ZString subject)
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
", messageType);
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MB202576543";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "HB89556";
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "HB32342";
			bill2.JPB_MessageStatus = existingMessageStatus;
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
			AssertEquals("bill1.JPB_ReleaseStatus", ZString.Empty, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", expectedMessageStatus, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", expectedMessageStatus == MessageStatusList.Codes.ClearHouseBillDelete ? AFRBillCustomsStatusList.Codes.NotRegistered : AFRBillCustomsStatusList.Codes.Registered, bill2.JPB_ReleaseStatus);
			AssertHasEmail(subject + " Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress); // send to group
			AssertEquals(0, header.Messages.Count);

			CombineAssertions("Test the Auto-trigger of Completion message generation", () =>
			{
				#region Test the Auto-trigger of Completion message generation
				if (messageType == MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse)
				{
					eventXmlText = string.Format(@"
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
        <Value>HB89556</Value>
      </Context>
      <Context>
        <Type>NotificationDetails</Type>
        <Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
", messageType);
					xmlEvent = eventDeserializer.Parse(eventXmlText);
					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					AssertEquals("PreProcess OutgoingMessage Count", 0, header.Messages.ToArray<EDIMessage>().Count(msg => msg.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit));
					logParents = subscriber.GetLogParentsForEvent(xmlEvent);
					AssertEquals("PostProcess OutgoingMessage Count", 1, header.Messages.ToArray<EDIMessage>().Count(msg => msg.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit));
					AssertEquals("LogParent", 1, logParents.Length);
					logParent = logParents[0];
					AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
					AssertEquals("bill1.JPB_MessageStatus", expectedMessageStatus, bill1.JPB_MessageStatus);
					AssertEquals("bill1.JPB_ReleaseStatus", AFRBillCustomsStatusList.Codes.Registered, bill1.JPB_ReleaseStatus);
					AssertEquals("bill2.JPB_MessageStatus", expectedMessageStatus, bill2.JPB_MessageStatus);
					AssertEquals("bill2.JPB_ReleaseStatus", AFRBillCustomsStatusList.Codes.Registered, bill2.JPB_ReleaseStatus);
					AssertHasEmail(subject + " Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress); // send to group
					AssertEquals(1, header.Messages.Count);
					var message = header.Messages[0];
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouseCompletion, message.EM_MessageOwner);
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
				#endregion
			});

			CombineAssertions("Test the Auto-trigger of Completion message generation doesn't happen twice", () =>
			{
				#region Test the Auto-trigger of Completion message generation doesn't happen twice
				if (messageType == MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse)
				{
					eventXmlText = string.Format(@"
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
        <Value>HB89556</Value>
      </Context>
      <Context>
        <Type>NotificationDetails</Type>
        <Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
", messageType);
					xmlEvent = eventDeserializer.Parse(eventXmlText);
					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					AssertEquals("PreProcess OutgoingMessage Count", 1, header.Messages.ToArray<EDIMessage>().Count(msg => msg.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit));
					logParents = subscriber.GetLogParentsForEvent(xmlEvent);
					AssertEquals("PostProcess OutgoingMessage Count", 1, header.Messages.ToArray<EDIMessage>().Count(msg => msg.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit));
					AssertEquals("LogParent", 1, logParents.Length);
					logParent = logParents[0];
					AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
					AssertEquals("bill1.JPB_MessageStatus", expectedMessageStatus, bill1.JPB_MessageStatus);
					AssertEquals("bill1.JPB_ReleaseStatus", AFRBillCustomsStatusList.Codes.Registered, bill1.JPB_ReleaseStatus);
					AssertEquals("bill2.JPB_MessageStatus", expectedMessageStatus, bill2.JPB_MessageStatus);
					AssertEquals("bill2.JPB_ReleaseStatus", AFRBillCustomsStatusList.Codes.Registered, bill2.JPB_ReleaseStatus);
					AssertHasEmail(subject + " Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress); // send to group
					AssertEquals("Header Message Count in Total", 1, header.Messages.Count);
					var message = header.Messages[0];
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouseCompletion, message.EM_MessageOwner);
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
				#endregion
			});
			header.Delete();

			AssertProcessClearEventFromJapanCustomsForHouse(messageType, existingMessageStatus, expectedMessageStatus, subject, "1", Common.JP.AFR.AFRBillCustomsStatusList.Codes.NL1);
			AssertProcessClearEventFromJapanCustomsForHouse(messageType, existingMessageStatus, expectedMessageStatus, subject, "2", Common.JP.AFR.AFRBillCustomsStatusList.Codes.NL2);
		}

		protected void AssertProcessClearEventFromJapanCustomsForHouse(ZString messageType, ZString existingMessageStatus, ZString expectedMessageStatus, ZString subject, ZString discrepancyCode, ZString expectedReleaseCode)
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
        <Type>DiscrepancyCode</Type>
        <Value>{1}</Value>
      </Context>
      <Context>
        <Type>NotificationDetails</Type>
        <Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
", messageType, discrepancyCode);
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MB202576543";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "HB89556";
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "HB32342";
			bill2.JPB_MessageStatus = existingMessageStatus;
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
			AssertEquals("bill1.JPB_ReleaseStatus", ZString.Empty, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", expectedMessageStatus, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", expectedMessageStatus == MessageStatusList.Codes.ClearHouseBillDelete ? AFRBillCustomsStatusList.Codes.NotRegistered : expectedReleaseCode.ToString(), bill2.JPB_ReleaseStatus);
			AssertHasEmail(subject + " Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress); // send to group
			AssertEquals(0, header.Messages.Count);
			header.Delete();
		}

		protected void AssertProcessErrorEventFromJapanCustomsForHouse(ZString messageType, ZString existingMessageStatus, ZString expectedMessageStatus, ZString subject)
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MB202576543";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "HB89556";
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "HB32342";
			bill2.JPB_MessageStatus = existingMessageStatus;
			SetupOriginalMessageAndSave(header, messageType);
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
    <EventReference>{0}-REJECTED</EventReference>

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
        <Type>InternalTransactionNumber</Type>
        <Value>{1}</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
", messageType, header.Messages[0].EM_MessageNum);

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match two LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals("bill1.JPB_MessageStatus", MessageStatusList.Codes.AwaitingHouseBillRegistration, bill1.JPB_MessageStatus);
			AssertEquals("bill1.JPB_ReleaseStatus", ZString.Empty, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", expectedMessageStatus, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", AFRBillCustomsStatusList.Codes.NotRegistered, bill2.JPB_ReleaseStatus);
			AssertHasEmail(subject + " Response (Failure) for " + header.JPH_JobReference, @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Manifest Details</th></tr></thead><tr><td>Master Bill</td><td>MB202576543</td></tr><tr><td>House Bill</td><td>HB32342</td></tr></table>", Staff2.GS_EmailAddress); // send to original user
																																																																																															 //header.Delete();
		}
	}

	[TestedType(typeof(AHREventProcessor))]
	sealed class AHREventProcessorBaseOnlyTest : AHREventProcessorAbstractTest<AHREventProcessor>
	{
		protected override AHREventProcessor GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			return new AHREventProcessor(eventDataObject, logger, factory, DefaultDataObjectWriterStrategy.Instance);
		}

		[SnailTest]
		public void TestRegistrationCompletion_WorksForAllCountries_WI00258401()
		{
			foreach (var countryCode in ZArchitecture.Environment.Country.LicenceKeyBuilderSupportedCountryCodes)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					TestRegistrationCompletionCore(countryCode + "PDSITGDLSAS93844");
				}
			}
		}

		void TestRegistrationCompletionCore(ZString masterBillNumber)
		{
			const string eventXmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <ActionPurpose>
        <Code>AHR</Code>
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
    <EventReference>AHR-ACCEPTED</EventReference>

    <ContextCollection>
      <Context>
        <Type>InternalTransactionNumber</Type>
        <Value>{0}</Value>
      </Context>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>{1}</Value>
      </Context>
      <Context>
        <Type>NotificationDetails</Type>
        <Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			var header = Factory.NewWithValidTestData<JPAFRHeader>();
			header.JPH_MasterBillNumber = masterBillNumber;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "J3DUWCD01236398";
			bill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			Factory.SaveForTesting();
			new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance, true).SendCompletionMessageToCustoms(ActionCode.RegisterCompletionByRegistration);
			Factory.SaveForTesting();
			header.Reload();
			var originalMessage = header.AFRMessages[0];

			var newFactory = new BusinessObjectFactory();
			var subscriber = GetNewEventParentFinder(factory: newFactory);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, originalMessage.EM_MessageNum, masterBillNumber));
			subscriber.GetLogParentsForEvent(xmlEvent);

			header = newFactory.Load<JPAFRHeader>(header.PK);
			CombineAssertions(() =>
			{
				var countryCode = GlbCompany.CurrentCompany.Country.Code;
				AssertEquals(countryCode + "->JPH_MessageStatus", MessageStatusList.Codes.ClearHouseBillRegistrationCompletion, header.JPH_MessageStatus);
				AssertEquals(countryCode + "->JPH_BillRegistrationStatus", "Completed", header.JPH_BillRegistrationStatus);
			});
		}

		public void TestAHREventProcessorMissingBill()
		{
			var logger = new TestErrorLogger();
			var eventDataObject = new UniversalEvent();
			eventDataObject.ContextCollection = new List<Context>()
			{
				CreateContext("HBOLNumber", "HB3242"),
				CreateContext("NotificationDetails", "HELLO WORLD")
			};
			var processor = new AHREventProcessor(eventDataObject, logger, Factory.BOFactory, DefaultDataObjectWriterStrategy.Instance);
			var header = Factory.New<JPAFRHeader>();
			header.JPH_JobReference = "AFR23423";
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process(header);
			AssertEquals("Warning - Bill 'HB3242' could not be found on AFR Job 'AFR23423'.", logger.Logs);
			AssertHasEmail("Advance Cargo Information Registration Response (Failure) for AFR23423", "HELLO WORLD", Staff1.GS_EmailAddress);
		}

		public void TestProcessBillCompletionEventFromJapanCustoms_REG()
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
			bill1.JPB_BillNumber = "HB89556";
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "HB32342";
			bill2.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			Factory.SaveForTesting();
			new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance, true).SendCompletionMessageToCustoms(ActionCode.RegisterCompletionByRegistration);
			Factory.SaveForTesting();
			AssertEquals(1, header.AFRMessages.Count);
			var originalMessage = header.AFRMessages[0];
			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 0, logParents.Length);
			AssertEquals("IsBillRegistrationCompleted", false, header.IsBillRegistrationCompleted);
			AssertEquals("header.JPH_MessageStatus", MessageStatusList.Codes.AwaitingHouseBillRegistrationCompletion, header.JPH_MessageStatus);
			AssertEquals("bill1.JPB_MessageStatus", MessageStatusList.Codes.AwaitingHouseBillRegistration, bill1.JPB_MessageStatus);
			AssertEquals("bill1.JPB_ReleaseStatus", ZString.Empty, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", MessageStatusList.Codes.ClearHouseBillRegistration, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", ZString.Empty, bill2.JPB_ReleaseStatus);
			AssertHasEmail("Advance Cargo Information Registration Response", "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress); // send to group

			eventXmlText = string.Format(@"
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
    <EventReference>{0}-REJECTED</EventReference>

    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MB202576543</Value>
      </Context>
      <Context>
        <Type>InternalTransactionNumber</Type>
        <Value>{1}</Value>
      </Context>
      <Context>
        <Type>NotificationDetails</Type>
        <Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, originalMessage.EM_MessageNum);
			xmlEvent = eventDeserializer.Parse(eventXmlText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals("IsBillRegistrationCompleted", false, header.IsBillRegistrationCompleted);
			AssertEquals("header.JPH_MessageStatus", MessageStatusList.Codes.ErrorHouseBillRegistrationCompletion, header.JPH_MessageStatus);
			AssertEquals("bill1.JPB_MessageStatus", MessageStatusList.Codes.AwaitingHouseBillRegistration, bill1.JPB_MessageStatus);
			AssertEquals("bill1.JPB_ReleaseStatus", ZString.Empty, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", MessageStatusList.Codes.ClearHouseBillRegistration, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", ZString.Empty, bill2.JPB_ReleaseStatus);
			AssertHasEmail("Advance Cargo Information Registration Completion Response (Failure) for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress); // send to group

			eventXmlText = string.Format(@"
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
        <Type>InternalTransactionNumber</Type>
        <Value>{1}</Value>
      </Context>
      <Context>
        <Type>NotificationDetails</Type>
        <Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, originalMessage.EM_MessageNum);
			xmlEvent = eventDeserializer.Parse(eventXmlText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals("IsBillRegistrationCompleted", true, header.IsBillRegistrationCompleted);
			AssertEquals("header.JPH_MessageStatus", MessageStatusList.Codes.ClearHouseBillRegistrationCompletion, header.JPH_MessageStatus);
			AssertEquals("bill1.JPB_MessageStatus", MessageStatusList.Codes.AwaitingHouseBillRegistration, bill1.JPB_MessageStatus);
			AssertEquals("bill1.JPB_ReleaseStatus", ZString.Empty, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", MessageStatusList.Codes.ClearHouseBillRegistration, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", ZString.Empty, bill2.JPB_ReleaseStatus);
			AssertHasEmail("Advance Cargo Information Registration Completion Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress); // send to group
		}

		public void TestProcessBillCompletionEventFromJapanCustoms_ADD()
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
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "HB32342";
			bill2.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			Factory.SaveForTesting();
			new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance, true).SendCompletionMessageToCustoms(ActionCode.RegisterCompletionByAmendment);
			Factory.SaveForTesting();
			AssertEquals(1, header.AFRMessages.Count);
			var originalMessage = header.AFRMessages[0];
			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 0, logParents.Length);
			AssertEquals("IsBillRegistrationCompleted", false, header.IsBillRegistrationCompleted);
			AssertEquals("header.JPH_MessageStatus", MessageStatusList.Codes.AwaitingHouseBillRegistrationCompletion, header.JPH_MessageStatus);
			AssertEquals("bill1.JPB_MessageStatus", MessageStatusList.Codes.AwaitingHouseBillRegistration, bill1.JPB_MessageStatus);
			AssertEquals("bill1.JPB_ReleaseStatus", ZString.Empty, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", MessageStatusList.Codes.ClearHouseBillRegistration, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", ZString.Empty, bill2.JPB_ReleaseStatus);
			AssertHasEmail("Update Advance Cargo Information Registration Response", "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress); // send to group

			eventXmlText = string.Format(@"
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
    <EventReference>{0}-REJECTED</EventReference>

    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MB202576543</Value>
      </Context>
      <Context>
        <Type>InternalTransactionNumber</Type>
        <Value>{1}</Value>
      </Context>
      <Context>
        <Type>NotificationDetails</Type>
        <Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, originalMessage.EM_MessageNum);
			xmlEvent = eventDeserializer.Parse(eventXmlText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals("IsBillRegistrationCompleted", false, header.IsBillRegistrationCompleted);
			AssertEquals("header.JPH_MessageStatus", MessageStatusList.Codes.ErrorHouseBillRegistrationCompletion, header.JPH_MessageStatus);
			AssertEquals("bill1.JPB_MessageStatus", MessageStatusList.Codes.AwaitingHouseBillRegistration, bill1.JPB_MessageStatus);
			AssertEquals("bill1.JPB_ReleaseStatus", ZString.Empty, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", MessageStatusList.Codes.ClearHouseBillRegistration, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", ZString.Empty, bill2.JPB_ReleaseStatus);
			AssertHasEmail("Advance Cargo Information Registration Completion Response (Failure) for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress); // send to group

			eventXmlText = string.Format(@"
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
        <Type>InternalTransactionNumber</Type>
        <Value>{1}</Value>
      </Context>
      <Context>
        <Type>NotificationDetails</Type>
        <Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, originalMessage.EM_MessageNum);
			xmlEvent = eventDeserializer.Parse(eventXmlText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals("IsBillRegistrationCompleted", true, header.IsBillRegistrationCompleted);
			AssertEquals("header.JPH_MessageStatus", MessageStatusList.Codes.ClearHouseBillRegistrationCompletion, header.JPH_MessageStatus);
			AssertEquals("bill1.JPB_MessageStatus", MessageStatusList.Codes.AwaitingHouseBillRegistration, bill1.JPB_MessageStatus);
			AssertEquals("bill1.JPB_ReleaseStatus", ZString.Empty, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", MessageStatusList.Codes.ClearHouseBillRegistration, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", ZString.Empty, bill2.JPB_ReleaseStatus);
			AssertHasEmail("Advance Cargo Information Registration Completion Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress); // send to group
		}

		public void TestProcessBillCompletionEventWillCancelPreviousLog()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "NACC";
			header.JPH_VesselName = Vessel1.RV_Code;
			header.JPH_Voyage = "E434";
			header.JPH_RL_NKLoading = "SGSIN";
			header.JPH_LoadingPortSuffix = "2";
			header.JPH_MessageStatus = MessageStatusList.Codes.AwaitingDepartureTimeRegistration;
			header.JPH_IsShippingLineEntry = true;
			Assert(!header.IsBillRegistrationCompleted);
			var allCompletionRefEvent = new ZQuery(StmALogSchema.SL_Reference, "Bill Registration");
			AssertEquals(0, header.Logs.Find(allCompletionRefEvent).Length);
			header.LogBillRegistrationCompletion();
			header.LogBillRegistrationCompletion();
			Assert(header.IsBillRegistrationCompleted);
			AssertEquals(2, header.Logs.Find(allCompletionRefEvent).Length);
			AssertEquals(2, header.Logs.Find(JPAFRHeader.BillRegistrationCompletedQuery).Length);
			SetupOriginalMessageAndSave(header, MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouseCompletion);

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
        <Type>InternalTransactionNumber</Type>
        <Value>{1}</Value>
      </Context>
      <Context>
        <Type>NotificationDetails</Type>
        <Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, header.Messages[0].EM_MessageNum);

			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			CombineAssertions(() =>
			{
				AssertEquals(3, header.Logs.Find(allCompletionRefEvent).Length);
				AssertEquals(1, header.Logs.Find(JPAFRHeader.BillRegistrationCompletedQuery).Length);
			});
		}

		public void TestProcessClearEventFromJapanCustomsForAHR()
		{
			AssertProcessClearEventFromJapanCustomsForHouse(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, MessageStatusList.Codes.AwaitingHouseBillRegistration, MessageStatusList.Codes.ClearHouseBillRegistration, "Advance Cargo Information Registration");
		}

		public void TestProcessErrorEventFromJapanCustomsForAHR()
		{
			AssertProcessErrorEventFromJapanCustomsForHouse(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, MessageStatusList.Codes.AwaitingHouseBillRegistration, MessageStatusList.Codes.ErrorHouseBillRegistration, "Advance Cargo Information Registration");
		}
	}
}
