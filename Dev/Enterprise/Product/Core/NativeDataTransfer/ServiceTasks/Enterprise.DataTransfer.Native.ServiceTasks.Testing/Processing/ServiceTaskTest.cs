using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DataTransfer.Native.ServiceTasks.Testing
{
	[TestedType(typeof(ServiceTask))]
	public class ServiceTaskTest : ServiceTaskTestCase<ServiceTask>
	{
		public void TestNewServiceTaskProcessesValidShipmentMessage()
		{
			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();

			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NativeDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlNativeShipment;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = NativeShipmentXML;

			Factory.Save();

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("message.EM_ApplicationCode", EDIMessage.ApplicationCodes.NativeDataMessaging, message.EM_ApplicationCode);
			});

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(-1).ToDateTime();

			var serviceTask = new ServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			message.Reload();

			StmNoteCollection logs = null;
			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessage.Status.Rejected, message.EM_Status);
				logs = (StmNoteCollection)message.Notes.GetAllNotes();
				var messageNote = logs[0].ST_NoteDataAsText;
				AssertMultilineASCIIEquals("Message Note text", @"
Error - The 'Shipment' Native XML dataset has been deprecated. Please use the Universal Shipment XML instead.
Message Rejected.
					".Trim(), messageNote);
			});

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();
			message.EM_Status = EDIMessage.Status.Queued;
			logs.DeleteAll();
			Factory.Save();
			serviceTask = new ServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var factory = new BusinessObjectFactory();
			message = factory.Load<EDIMessage>(message.PK);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				var messageNote = ((StmNoteCollection)message.Notes.GetAllNotes())[0].ST_NoteDataAsText;
				AssertMultilineASCIIEquals("Message Note text", @"
JobShipment - 1 inserts, 0 updates, 0 deletes
JobPackLines - 2 inserts, 0 updates, 0 deletes
--------------------------------------------------------------------------------
Imported: Shipment
					".Trim(), messageNote);
			});
		}

		#region NativeShipmentXML

		const string NativeShipmentXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Header>
    <OwnerCode>BENGOVSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Shipment version=""1.0"">
      <JobShipment Action=""MERGE"">
        <HouseBill>CRAZ1237891243</HouseBill>
        <AWBServiceLevel>STD</AWBServiceLevel>
        <IsForwardRegistered>true</IsForwardRegistered>
        <TransportMode>AIR</TransportMode>
        <PackingMode>LSE</PackingMode>
        <ActualVolume>0.000</ActualVolume>
        <UnitOfVolume>M3</UnitOfVolume>
        <ActualWeight>0.000</ActualWeight>
        <UnitOfWeight>KG</UnitOfWeight>
        <NoOriginalBills>3</NoOriginalBills>
        <NoCopyBills>3</NoCopyBills>
        <ShippedOnBoard>SHP</ShippedOnBoard>
        <ServiceLevel TableName=""RefServiceLevel"">
          <Code>STD</Code>
        </ServiceLevel>
        <GoodsValueCurr TableName=""RefCurrency"">
          <Code>AUD</Code>
        </GoodsValueCurr>
        <InsuranceCurrency TableName=""RefCurrency"">
          <Code>AUD</Code>
        </InsuranceCurrency>
        <TotalCountPackType TableName=""RefPackType"">
          <Code>CTN</Code>
        </TotalCountPackType>
        <PackType TableName=""RefPackType"">
          <Code>PLT</Code>
        </PackType>
        <JobPackLinesCollection>
          <JobPackLines Action=""MERGE"">
            <FreightMode>OUT</FreightMode>
            <Length>10.000</Length>
            <Height>20.000</Height>
            <Width>30.000</Width>
            <PackType TableName=""RefPackType"">
              <Code>PLT</Code>
            </PackType>
          </JobPackLines>
          <JobPackLines Action=""INSERT"">
            <FreightMode>STD</FreightMode>
            <PackageCount>5</PackageCount>
            <ActualWeight>12</ActualWeight>
            <ActualWeightUQ>KG</ActualWeightUQ>
            <Length>97.000</Length>
            <Height>98.000</Height>
            <Width>99.000</Width>
            <UnitOfDimension>CM</UnitOfDimension>
            <PackType TableName=""RefPackType"">
              <Code>CNT</Code>
            </PackType>
          </JobPackLines>
        </JobPackLinesCollection>
      </JobShipment>
    </Shipment>
  </Body>
</Native>";

		#endregion

		public void TestNewServiceTaskProcessesValidOrganizationMessage()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NativeDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlNativeOrganization;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = NativeOrganisationXML;

			Factory.Save();

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("message.EM_ApplicationCode", EDIMessage.ApplicationCodes.NativeDataMessaging, message.EM_ApplicationCode);
			});

			var serviceTask = new ServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			message.Reload();

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);

				var organisation = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FLAASPMEL"));
				AssertEquals("organisation.OH_FullName", "FLAT ASP ROADWORKS", organisation.OH_FullName);
				AssertEquals("organisation.OH_Code", "FLAASPMEL", organisation.OH_Code);

				var messageNote = ((StmNoteCollection)message.Notes.GetAllNotes())[0].ST_NoteDataAsText;
				AssertMultilineASCIIEquals("Message Note text", @"
Property: ""FullName"" of Entity: ""OrgHeader"" was trimmed of white space and stripped of CRLF characters
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgContact - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
--------------------------------------------------------------------------------
Imported: Organization
Local Code: FLAASPMEL
External Code: FLAASPMEL
					".Trim(), messageNote);

				var orgHeader = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FLAASPMEL"));
				AssertNotNull("OrgHeader witht he code 'FLAASPMEL' laoded in new factory to make sure it's in the DB.", orgHeader);
			});
		}

		public void TestNewLineCharsNormalisedToWindowsNewLineChar()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NativeDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlNativeOrganization;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = NativeOrganisationXML.Replace("<PersonalInfo></PersonalInfo>", "<PersonalInfo>This\ris\nspecial information\r\nWe want to see what happens\r\nWhen this is imported in remotely\r\n\r\nAlrighty then</PersonalInfo>");

			AssertContains("Precondition", "This\ris\nspecial information\r\nWe want to see what happens\r\nWhen this is imported in remotely\r\n\r\nAlrighty then", message.EM_MessageText);

			Factory.Save();

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("message.EM_ApplicationCode", EDIMessage.ApplicationCodes.NativeDataMessaging, message.EM_ApplicationCode);
			});

			var serviceTask = new ServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			message.Reload();

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);

				var organisation = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FLAASPMEL"));
				AssertEquals("StripCRLF = true for FullName", "FLAT ASP ROADWORKS", organisation.OH_FullName);
				AssertEquals(1, organisation.Contacts.Count);
				AssertEquals("StripCRLF = false for PersonalInfo", "This\r\nis\r\nspecial information\r\nWe want to see what happens\r\nWhen this is imported in remotely\r\n\r\nAlrighty then", organisation.Contacts[0].OC_PersonalInfo);
			});
		}

		#region NativeOrganisationXML

		const string NativeOrganisationXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Header>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""MERGE"">
				<Code>FLAASPMEL</Code>
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS
</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
        <OrgContactCollection>
          <OrgContact Action=""MERGE"">
            <PK>7d4b40fe-c078-4766-a059-e855fd8907ab</PK>
            <ContactName>test</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title>testt</Title>
            <JobCategory>EMU</JobCategory>
            <Phone></Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <WebContractSignedDate></WebContractSignedDate>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <Gender>false</Gender>
            <ProfilePhoto></ProfilePhoto>
            <IsActive>true</IsActive>
            <WebAccessEnabled>false</WebAccessEnabled>
            <AttachmentType>PDF</AttachmentType>
            <Email>test@example.com</Email>
            <PersonalInfo></PersonalInfo>
            <PasswordHash></PasswordHash>
            <PasswordHashIterations>0</PasswordHashIterations>
            <PasswordSalt></PasswordSalt>
            <SystemCreateTimeUtc>2020-12-13T23:37:00</SystemCreateTimeUtc>
            <SystemLastEditTimeUtc>2020-12-13T23:37:00</SystemLastEditTimeUtc>
            <WebAccessSuperseded>false</WebAccessSuperseded>
            <AddressOverride TableName=""OrgHeader"" />
            <OrgAddress />
            <Nationality TableName=""RefCountry"">
              <Code></Code>
            </Nationality>
          </OrgContact>
        </OrgContactCollection>
				<OrgAddressCollection>
					<OrgAddress Action=""MERGE"">
						<Code>AUMEL - 42SALISBURYLANE</Code>
						<Language>EN</Language>
						<Address1>42 SALISBURY LANE</Address1>
						<City>TULLAMARINE</City>
						<State>VIC</State>
						<PostCode>3043</PostCode>
						<Phone>+61383361000</Phone>
						<Fax>+61393361001</Fax>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<RelatedPortCode>
							<Code>AUMEL</Code>
						</RelatedPortCode>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
				</OrgAddressCollection>
			</OrgHeader>
		</Organization>
	</Body>
</Native>";

		#endregion

		public void TestNewServiceTaskResetsStatusForInvalidMessages()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NativeDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlNativeOrganization;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = InvalidReferenceOrganisationXML;

			Factory.Save();

			var serviceTask = new ServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			message.Reload();

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessage.Status.Rejected, message.EM_Status);
				var messageNote = ((StmNoteCollection)message.Notes.GetAllNotes())[0].ST_NoteDataAsText;
				AssertMultilineASCIIEquals("Message Note text", @"
Error - XML cannot be processed as it does not adhere to the native XML format. There should only be one element within the 'Organization' section but 0 elements were found.
Message Rejected.
					".Trim(), messageNote);
			});
		}

		#region InvalidReferenceOrganisationXML

		const string InvalidReferenceOrganisationXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Body>
  <Organization>
  </Organization>
  </Body>
</ReferenceData>";

		#endregion

		public void TestNewServiceTask_NativeXMLImportUserVisibleException()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NativeDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlNativeOrganization;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = NativeInsertOrgWithoutUNCOLOXML;

			Factory.Save();

			var serviceTask = new ServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			message.Reload();

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessage.Status.Rejected, message.EM_Status);
				var messageNote = ((StmNoteCollection)message.Notes.GetAllNotes())[0].ST_NoteDataAsText;
				AssertMultilineASCIIEquals("Message Note text", @"
Error - The UNLOCO provided in the [ClosestPort.Code] element must not be empty when Organization Code Generation is enabled
Message Rejected.
					".Trim(), messageNote);
			});
		}

		#region NativeInsertOrgWithoutUNCOLOXML

		const string NativeInsertOrgWithoutUNCOLOXML = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Body>
    <Organization xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
      <OrgHeader Action=""MERGE"">
        <Code>AKZTESAKL5</Code>
        <OrgAddressCollection>
          <OrgAddress Action=""INSERT"">
            <Code>5 Manu Tapu Drive</Code>
            <Address1>5 Manu Tapu Drive</Address1>
            <City>Aukland</City>
            <State>AUK</State>
            <PostCode>2022</PostCode>
            <Phone>+64 (0) 9 256 0334</Phone>
            <Fax>+64 (0) 9 256 0326</Fax>
            <FCLEquipmentNeeded>ASK</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>ASK</LCLEquipmentNeeded>
            <AIREquipmentNeeded>ASK</AIREquipmentNeeded>
            <Email>test@test.com</Email>
            <RelatedPortCode>
              <Code>NZAKL</Code>
            </RelatedPortCode>
            <CountryCode>
              <Code>NZ</Code>
            </CountryCode>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""INSERT"">
                <AddressType>PAD</AddressType>
                <IsMainAddress>false</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
          </OrgAddress>
        </OrgAddressCollection>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		#endregion

		public void TestNewServiceTask_UnhandledException()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NativeDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlNativeOrganization;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = NativeInsertOrgUnhandledExceptionXML;

			Factory.Save();

			var serviceTask = new ServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			message.Reload();

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessage.Status.Rejected, message.EM_Status);
				var messageNote = ((StmNoteCollection)message.Notes.GetAllNotes())[0].ST_NoteDataAsText;
				AssertContains("Message Note text", @"
Error - Failed because of UnhandledException.
This error has been submitted to WTG for further investigation.

   at Enterprise.DataTransfer.Native.Business.Xml.Deserializers.EntitySetXmlDeserializer.Deserialize(XElement element, AncillaryImportServices sessionServices) in
					".Trim(), messageNote);
				AssertContains("Unknown exception occurred while importing Native XML, If you are a developer looking at the issue (yes you!!) please handle the exception.\r\n\r\nIf this exception occured because of a problem with the incoming XML, please wrap this exception in an exception type that has the ExceptionVisibility.User attribute on it. (eg: NativeXMLUserVisibleException) That will cause the exception to be reported to the User instead of being reported to WTG as an Issue. Make sure that you provide a clear message for the new exception that a User can follow to understand and fix the processing error that has occurred.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}

		public void TestNewServiceTaskProcessesWithSenderUserContext()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NativeDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlNativeOrganization;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = NativeOrganisationXML;
			message.EM_EI = SetInterchangeSenderProxyUsers(out var _).PK;

			Factory.Save();

			var serviceTask = new ServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			message.Reload();

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("STF", message.EM_SystemLastEditUser);

			var noteCreateUser = ((StmNoteCollection)message.Notes.GetAllNotes())[0].ST_SystemCreateUser;
			AssertEquals("STF", noteCreateUser);

			var organisation = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FLAASPMEL"));
			var query = new ZQuery(StmALogSchema.SL_Parent, organisation.PK);

			var logs = new BusinessObjectFactory().Load<StmALog>(query);
			Assert(logs.Any());
			logs.Select(log => log.User.GS_LoginName).ForEach(loginName =>
			{
				AssertEquals("SL_Staff.GS_LoginName", "TestStaff", loginName);
			});
		}

		EDIInterchange SetInterchangeSenderProxyUsers(out GlbStaff staff)
		{
			var senderCode = "THESENDER";
			var staffCode = "STF";

			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staffCode;
			staff.GS_LoginName = "TestStaff";
			Factory.Save();

			var list = eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.Value;
			var pair = list.AddNew();
			pair.Code = senderCode;
			pair.DescriptionValue = staffCode;
			eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = senderCode;
			return interchange;
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"Native Data Messaging Inbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.NativeDataMessaging,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC),
				};
			}
		}

		#region NativeInsertOrgUnhandledExceptionXML

		const string NativeInsertOrgUnhandledExceptionXML = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Body>
		<UnhandledException>Failed because of UnhandledException.</UnhandledException>
	</Body>
</Native>";

		#endregion
	}
}
