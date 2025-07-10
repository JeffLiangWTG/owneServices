using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Environment;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ATDEventProcessor))]
	class ATDEventProcessorTest : AFREventProcessorAbstractTest<ATDEventProcessor>
	{
		protected override ATDEventProcessor GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			return new ATDEventProcessor(eventDataObject, logger, factory);
		}

		public void TestProcessATDClearEventFromJapanCustomsForDepartureTimeMessaging()
		{
			AssertProcessATDClearEventForHeaderFromJapanCustomsFor(MessagingTypeList.Codes.DepartureTimeRegistration,
 MessageStatusList.Codes.AwaitingDepartureTimeRegistration,
 MessageStatusList.Codes.ClearDepartureTimeRegistration,
 MessagingTypeList.Descriptions.DepartureTimeRegistration);
		}

		public void TestProcessATDErrorEventFromJapanCustomsForDepartureTimeMessaging()
		{
			AssertProcessATDErrorEventForHeaderFromJapanCustomsFor(MessagingTypeList.Codes.DepartureTimeRegistration,
 MessageStatusList.Codes.AwaitingDepartureTimeRegistration,
 MessageStatusList.Codes.ErrorDepartureTimeRegistration,
 MessagingTypeList.Descriptions.DepartureTimeRegistration);
		}

		public void TestProcessATDClearEventWillCancelPreviousLog()
		{
			var eventXmlText = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <ActionPurpose>
        <Code>DTR</Code>
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
        <Type>LloydsNumber</Type>
        <Value>8732342</Value>
      </Context>
      <Context>
        <Type>VesselCallSign</Type>
        <Value>CALLME</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>E434</Value>
      </Context>
      <Context>
        <Type>CarrierCode</Type>
        <Value>NACC</Value>
      </Context>
      <Context>
        <Type>PortOfLoadingUNLOCO</Type>
        <Value>SGSIN</Value>
      </Context>
      <Context>
        <Type>PortOfLoadingSuffix</Type>
        <Value>2</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "NACC";
			header.JPH_VesselName = Vessel1.RV_Code;
			header.JPH_Voyage = "E434";
			header.JPH_RL_NKLoading = "SGSIN";
			header.JPH_LoadingPortSuffix = "2";
			header.JPH_MessageStatus = MessageStatusList.Codes.AwaitingDepartureTimeRegistration;
			header.JPH_IsShippingLineEntry = true;
			Assert(!header.IsDepartureTimeRegistered);
			var allDepartureTimeRefEvent = new ZQuery(StmALogSchema.SL_Reference, "Departure Time Registration");
			AssertEquals(0, header.Logs.Find(allDepartureTimeRefEvent).Length);
			header.LogDepartureTimeRegistration();
			header.LogDepartureTimeRegistration();
			Assert(header.IsDepartureTimeRegistered);
			AssertEquals(2, header.Logs.Find(allDepartureTimeRefEvent).Length);
			AssertEquals(2, header.Logs.Find(JPAFRHeader.DepartureTimeRegisteredQuery).Length);
			SetupOriginalMessageAndSave(header, MessagingTypeList.Codes.DepartureTimeRegistration);

			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			CombineAssertions(() =>
			{
				AssertEquals(3, header.Logs.Find(allDepartureTimeRefEvent).Length);
				AssertEquals(1, header.Logs.Find(JPAFRHeader.DepartureTimeRegisteredQuery).Length);
			});
		}

		void AssertProcessATDClearEventForHeaderFromJapanCustomsFor(ZString messageType, ZString existingMessageStatus, ZString expectedMessageStatus, ZString subject)
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
        <Type>LloydsNumber</Type>
        <Value>8732342</Value>
      </Context>
      <Context>
        <Type>VesselCallSign</Type>
        <Value>CALLME</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>E434</Value>
      </Context>
      <Context>
        <Type>CarrierCode</Type>
        <Value>NACC</Value>
      </Context>
      <Context>
        <Type>PortOfLoadingUNLOCO</Type>
        <Value>SGSIN</Value>
      </Context>
      <Context>
        <Type>PortOfLoadingSuffix</Type>
        <Value>2</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
", messageType);
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_CarrierCode = "NACC";
			header1.JPH_VesselName = Vessel1.RV_Code;
			header1.JPH_Voyage = "E434";
			header1.JPH_RL_NKLoading = "SGSIN";
			header1.JPH_LoadingPortSuffix = "1";
			header1.JPH_MessageStatus = existingMessageStatus;
			header1.JPH_IsShippingLineEntry = true;
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_CarrierCode = "NACC";
			header2.JPH_VesselName = Vessel1.RV_Code;
			header2.JPH_Voyage = "E434";
			header2.JPH_RL_NKLoading = "SGSIN";
			header2.JPH_LoadingPortSuffix = "2";
			header2.JPH_MessageStatus = existingMessageStatus;
			header2.JPH_IsShippingLineEntry = true;
			SetupOriginalMessageAndSave(header2, messageType);
			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should get best matching Header 2.", GetHumanReadableID(header2), GetHumanReadableID(logParent));
				AssertEquals("header1.JPH_MessageStatus", existingMessageStatus, header1.JPH_MessageStatus);
				AssertEquals("header2.JPH_MessageStatus", expectedMessageStatus, header2.JPH_MessageStatus);
				AssertEquals("header1.JPH_BillRegistrationStatus", "Pending", header1.JPH_BillRegistrationStatus);
				AssertEquals("header2.JPH_BillRegistrationStatus", "Completed", header2.JPH_BillRegistrationStatus);
				AssertEquals("header1.IsDepartureTimeRegistered", false, header1.IsDepartureTimeRegistered);
				AssertEquals("header2.IsDepartureTimeRegistered", true, header2.IsDepartureTimeRegistered);
				AssertHasEmail(subject + " Response for " + header2.JPH_JobReference, @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Manifest Details</th></tr></thead><tr><td>Carrier Code</td><td>NACC</td></tr><tr><td>Vessel Call Sign</td><td>CALLME</td></tr><tr><td>Voyage Number</td><td>E434</td></tr><tr><td>Port Of Loading</td><td>SGSIN</td></tr><tr><td>Port Of Loading Suffix</td><td>2</td></tr></table>", Staff3.GS_EmailAddress); // send to the last user that send a message
			});
		}

		void AssertProcessATDErrorEventForHeaderFromJapanCustomsFor(ZString messageType, ZString existingMessageStatus, ZString expectedMessageStatus, ZString subject)
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
    <EventReference>{0}-REJECTED</EventReference>

    <ContextCollection>
      <Context>
        <Type>VesselCallSign</Type>
        <Value>CALLME</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>E434</Value>
      </Context>
      <Context>
        <Type>CarrierCode</Type>
        <Value>NACC</Value>
      </Context>
      <Context>
        <Type>PortOfLoadingUNLOCO</Type>
        <Value>SGSIN</Value>
      </Context>
      <Context>
        <Type>PortOfLoadingSuffix</Type>
        <Value>2</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
", messageType);
			afrGroup.Staff.Add(Staff3);
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_CarrierCode = "NACC";
			header1.JPH_VesselName = Vessel1.RV_Code;
			header1.JPH_Voyage = "E434";
			header1.JPH_RL_NKLoading = "SGSIN";
			header1.JPH_LoadingPortSuffix = "1";
			header1.JPH_MessageStatus = existingMessageStatus;
			header1.JPH_IsShippingLineEntry = true;
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_CarrierCode = "NACC";
			header2.JPH_VesselName = Vessel1.RV_Code;
			header2.JPH_Voyage = "E434";
			header2.JPH_RL_NKLoading = "SGSIN";
			header2.JPH_LoadingPortSuffix = "2";
			header2.JPH_MessageStatus = existingMessageStatus;
			header2.JPH_IsShippingLineEntry = true;
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match two LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should get best matching Header 2.", GetHumanReadableID(header2), GetHumanReadableID(logParent));
				AssertEquals("header1.JPH_MessageStatus", existingMessageStatus, header1.JPH_MessageStatus);
				AssertEquals("header2.JPH_MessageStatus", expectedMessageStatus, header2.JPH_MessageStatus);
				AssertEquals("header1.JPH_BillRegistrationStatus", "Pending", header1.JPH_BillRegistrationStatus);
				AssertEquals("header2.JPH_BillRegistrationStatus", string.Empty, header2.JPH_BillRegistrationStatus);
				AssertEquals("header1.IsDepartureTimeRegistered", false, header1.IsDepartureTimeRegistered);
				AssertEquals("header2.IsDepartureTimeRegistered", false, header2.IsDepartureTimeRegistered);
				AssertHasEmail(subject + " Response (Failure) for " + header2.JPH_JobReference, @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Manifest Details</th></tr></thead><tr><td>Carrier Code</td><td>NACC</td></tr><tr><td>Vessel Call Sign</td><td>CALLME</td></tr><tr><td>Voyage Number</td><td>E434</td></tr><tr><td>Port Of Loading</td><td>SGSIN</td></tr><tr><td>Port Of Loading Suffix</td><td>2</td></tr></table>", Staff1.GS_EmailAddress, Staff3.GS_EmailAddress); // send to all users in group
			});
			header1.Delete();
			header2.Delete();
		}
	}
}
