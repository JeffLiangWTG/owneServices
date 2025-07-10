using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Environment;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	abstract class AMREventProcessorAbstractTest<T> : AFREventProcessorAbstractTest<T>
		where T : AFREventProcessor
	{
		protected void AssertProcessClearEventFromJapanCustomsForMaster(ZString messageType, ZString existingMessageStatus, ZString expectedMessageStatus, ZString subject)
		{
			AssertProcessClearEventFromJapanCustomsForMaster(messageType, existingMessageStatus, expectedMessageStatus, subject, "1", Common.JP.AFR.AFRBillCustomsStatusList.Codes.NL2);
			AssertProcessClearEventFromJapanCustomsForMaster(messageType, existingMessageStatus, expectedMessageStatus, subject, "2", Common.JP.AFR.AFRBillCustomsStatusList.Codes.NL1);
		}

		protected void AssertProcessClearEventFromJapanCustomsForMaster(ZString messageType, ZString existingMessageStatus, ZString expectedMessageStatus, ZString subject, ZString discrepancyCode, ZString expectedReleaseCode)
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
        <Value>VICTM002</Value>
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
			header.JPH_IsShippingLineEntry = true;
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "VICTM001";
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingMasterBillRegistration;
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "VICTM002";
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
			AssertEquals("bill1.JPB_MessageStatus", MessageStatusList.Codes.AwaitingMasterBillRegistration, bill1.JPB_MessageStatus);
			AssertEquals("bill1.JPB_ReleaseStatus", ZString.Empty, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", expectedMessageStatus, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", expectedMessageStatus == MessageStatusList.Codes.ClearMasterBillDelete ? AFRBillCustomsStatusList.Codes.NotRegistered : expectedReleaseCode.ToString(), bill2.JPB_ReleaseStatus);
			AssertHasEmail(subject + " Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress); // send to group
			AssertEquals(0, header.Messages.Count);
			header.Delete();
		}

		protected void AssertProcessErrorEventFromJapanCustomsForMaster(ZString messageType, ZString existingMessageStatus, ZString expectedMessageStatus, ZString subject)
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "VICTM001";
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingMasterBillRegistration;
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "VICTM002";
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
        <Value>VICTM002</Value>
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
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			AssertEquals("Should get best matching Header.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals("bill1.JPB_MessageStatus", MessageStatusList.Codes.AwaitingMasterBillRegistration, bill1.JPB_MessageStatus);
			AssertEquals("bill1.JPB_ReleaseStatus", ZString.Empty, bill1.JPB_ReleaseStatus);
			AssertEquals("bill2.JPB_MessageStatus", expectedMessageStatus, bill2.JPB_MessageStatus);
			AssertEquals("bill2.JPB_ReleaseStatus", AFRBillCustomsStatusList.Codes.NotRegistered, bill2.JPB_ReleaseStatus);
			AssertHasEmail(subject + " Response (Failure) for " + header.JPH_JobReference, @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Manifest Details</th></tr></thead><tr><td>Master Bill</td><td>VICTM002</td></tr></table>", Staff2.GS_EmailAddress); // send to original user
		}
	}

	[TestedType(typeof(AMREventProcessor))]
	sealed class AMREventProcessorBaseOnlyTest : AMREventProcessorAbstractTest<AMREventProcessor>
	{
		protected override AMREventProcessor GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			return new AMREventProcessor(eventDataObject, logger, factory);
		}

		public void TestProcessClearEventFromJapanCustomsForAMR()
		{
			AssertProcessClearEventFromJapanCustomsForMaster(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster, MessageStatusList.Codes.AwaitingMasterBillRegistration, MessageStatusList.Codes.ClearMasterBillRegistration, "Advance Cargo Information Registration");
		}

		public void TestProcessErrorEventFromJapanCustomsForAMR()
		{
			AssertProcessErrorEventFromJapanCustomsForMaster(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster, MessageStatusList.Codes.AwaitingMasterBillRegistration, MessageStatusList.Codes.ErrorMasterBillRegistration, "Advance Cargo Information Registration");
		}
	}
}
