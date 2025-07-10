using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	public class DataMessageProcessorTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestUniversalEventLinkedByDataContextKeyWithoutCodesMappedToTargetFlagAndMatchingEnterpriseAndSystemIDInAnotherCompany()
		{
			var logParent = Factory.BOFactory.New<IDummyWithWorkflow>();
			logParent.Z0_Description = "XXX00001234";
			Factory.SaveForTesting();

			#region eventXML

			const string eventXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DummyBusinessObject</Type>
          <Key>XXX00001234</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>SIN</Code>
        <Name>Eagle Datamation International Pte Ltd</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <EventType>
        <Code>FUL</Code>
        <Description>Freight Unloaded</Description>
      </EventType>
      <ServerID>DAT</ServerID>
      <DataProvider>CargoWise One</DataProvider>
    </DataContext>
    <EventTime>2011-07-12T00:00:00.000</EventTime>
    <EventType>FUL</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>USCIT</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>USCIT</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			#endregion

			var message = GetQueuedUniversalEventMessage(eventXML);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Dummy Business Object XXX00001234.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Dummy Business Object XXX00001234.
".Trim(), message.GetLogNoteText());

				var logs = ((BusinessObject)logParent).GetLogs();
				var fulLogs = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "FUL"));
				AssertEquals("fulLogs.Length", 1, fulLogs.Length);
				var athLog = fulLogs[0];
				AssertEquals("athLog.SL_EventTime", new ZDateTime(2011, 7, 12, 0, 0, 0), athLog.SL_EventTime);
			});
		}

		public void TestUniversalEventLinkedByDataContextKeyWithoutCodesMappedToTargetFlagAndMatchingEnterpriseAndSystemID()
		{
			var logParent = Factory.BOFactory.New<IDummyWithWorkflow>();
			logParent.Z0_Description = "XXX00001234";
			Factory.SaveForTesting();

			const string eventXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DummyBusinessObject</Type>
          <Key>XXX00001234</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <EventType>
        <Code>FUL</Code>
        <Description>Freight Unloaded</Description>
      </EventType>
      <ServerID>DAT</ServerID>
      <DataProvider>CargoWise One</DataProvider>
    </DataContext>
    <EventTime>2011-07-12T00:00:00.000</EventTime>
    <EventType>FUL</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>USCIT</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>USCIT</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(eventXML);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Dummy Business Object XXX00001234.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Dummy Business Object XXX00001234.
".Trim(), message.GetLogNoteText());

				var logs = ((BusinessObject)logParent).GetLogs();
				var fulLogs = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "FUL"));
				AssertEquals("fulLogs.Length", 1, fulLogs.Length);
				var athLog = fulLogs[0];
				AssertEquals("athLog.SL_EventTime", new ZDateTime(2011, 7, 12, 0, 0, 0), athLog.SL_EventTime);
			});
		}

		public void TestUniversalEventLinkedByDataContextKeyWithCodesMappedToTargetFlagOnAndNonMatchingEnterpriseAndSystemID()
		{
			var logParent = Factory.BOFactory.New<IDummyWithWorkflow>();
			logParent.Z0_Description = "XXX00001238";
			Factory.SaveForTesting();

			string eventXML = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DummyBusinessObject</Type>
          <Key>XXX00001238</Key>
        </DataTarget>
      </DataTargetCollection>
      <CodesMappedToTarget>true</CodesMappedToTarget>
      <Company>
        <Code>{0}</Code>
        <Name>Fat Old Olgas</Name>
      </Company>
      <EnterpriseID>WAG</EnterpriseID>
      <EventType>
        <Code>FUL</Code>
        <Description>Freight Unloaded</Description>
      </EventType>
      <ServerID>BOT</ServerID>
      <DataProvider>CargoWise One</DataProvider>
    </DataContext>
    <EventTime>2011-07-12T00:00:00.000</EventTime>
    <EventType>FUL</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>USCIT</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>USCIT</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>", Environment.Env.CurrentCompany.Code);

			var message = GetQueuedUniversalEventMessage(eventXML);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Dummy Business Object XXX00001238.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Dummy Business Object XXX00001238.
".Trim(), message.GetLogNoteText());

				var logs = ((BusinessObject)logParent).GetLogs();
				var fulLogs = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "FUL"));
				AssertEquals("fulLogs.Length", 1, fulLogs.Length);
				var athLog = fulLogs[0];
				AssertEquals("athLog.SL_EventTime", new ZDateTime(2011, 7, 12, 0, 0, 0), athLog.SL_EventTime);
			});
		}

		public void TestUniversalEventLinkedByDataContextKeyWithCodesMappedToTargetFlagOnAndNoEnterpriseAndSystemID()
		{
			var nonMatchingLogParent = Factory.BOFactory.New<IDummyWithWorkflow>();
			nonMatchingLogParent.Z0_Description = "XXX00001238";

			var logParent = Factory.BOFactory.New<IDummyWithWorkflow>();
			logParent.Z0_Description = "XXX00004238";
			Factory.SaveForTesting();

			const string eventXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DummyBusinessObject</Type>
          <Key>XXX00004238</Key>
        </DataTarget>
      </DataTargetCollection>
      <CodesMappedToTarget>true</CodesMappedToTarget>
      <DataProvider>CargoWise One</DataProvider>
    </DataContext>
    <EventTime>2011-07-12T00:00:00.000</EventTime>
    <EventType>FUL</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>USCIT</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>USCIT</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(eventXML);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Dummy Business Object XXX00004238.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Dummy Business Object XXX00004238.
".Trim(), message.GetLogNoteText());

				var logs = ((BusinessObject)logParent).GetLogs();
				var fulLogs = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "FUL"));
				AssertEquals("fulLogs.Length", 1, fulLogs.Length);
				var athLog = fulLogs[0];
				AssertEquals("athLog.SL_EventTime", new ZDateTime(2011, 7, 12, 0, 0, 0), athLog.SL_EventTime);
			});
		}

		public void TestUniversalEventLinkedByDataContextKeyWithoutCodesMappedToTargetFlagAndNonMatchingEnterpriseAndSystemID()
		{
			var logParent = Factory.BOFactory.New<IDummyWithWorkflow>();
			logParent.Z0_Description = "XXX00001238";
			Factory.SaveForTesting();

			const string eventXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DummyBusinessObject</Type>
          <Key>XXX00001238</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>FOO</Code>
        <Name>Fat Old Olgas</Name>
      </Company>
      <EnterpriseID>WAG</EnterpriseID>
      <EventType>
        <Code>FUL</Code>
        <Description>Freight Unloaded</Description>
      </EventType>
      <ServerID>BOT</ServerID>
      <DataProvider>CargoWise One</DataProvider>
    </DataContext>
    <EventTime>2011-07-12T00:00:00.000</EventTime>
    <EventType>FUL</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>USCIT</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>USCIT</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(eventXML);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Dummy Business Object XXX00001238.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Dummy Business Object XXX00001238.
".Trim(), message.GetLogNoteText());

				var logs = ((BusinessObject)logParent).GetLogs();
				var fulLogs = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "FUL"));
				AssertEquals("fulLogs.Length", 1, fulLogs.Length);
				var athLog = fulLogs[0];
				AssertEquals("athLog.SL_EventTime", new ZDateTime(2011, 7, 12, 0, 0, 0), athLog.SL_EventTime);
			});
		}

		public void TestEDICodeMapperRules()
		{
			#region shipmentXML

			const string shipmentXML =
@"<UniversalShipment>
  <Shipment>
    <DataContext>
      <DataProvider>AAA</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Type>WarehouseOrder</Type>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>DDE</Code>
      </Company>
      <EnterpriseID>HYE</EnterpriseID>
      <ServerID>XXX</ServerID>
    </DataContext>
    <LocalProcessing>
      <DeliveryRequiredBy>2017-08-28T00:00:00.0000000</DeliveryRequiredBy>
    </LocalProcessing>
    <Order>
      <OrderNumber>TMIL2487-8</OrderNumber>
      <TotalUnits>2</TotalUnits>
      <Warehouse>
        <Code>WWW</Code>
      </Warehouse>
      <OrderLineCollection>
        <OrderLine>
          <OrderedQty>1</OrderedQty>
          <Product>
            <Code>EC-12SW-GB</Code>
          </Product>
        </OrderLine>
        <OrderLine>
          <OrderedQty>1</OrderedQty>
          <Product>
            <Code>ECL-90C</Code>
          </Product>
        </OrderLine>
      </OrderLineCollection>
    </Order>
  </Shipment>
</UniversalShipment>";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(shipmentXML, createInterchange: true);
			var org_Proxy = Factory.BOFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			var org_ProxyMapping = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
			var warehouse_ProxyMapping = (BusinessObject)Factory.BOFactory.New<IWhsWarehouse>();
			warehouse_ProxyMapping.FillWithValidTestData();

			var branch_ServiceTask = Factory.BOFactory.NewWithValidTestData<GlbBranch>();
			var company_ServiceTask = branch_ServiceTask.Company;
			var org_ServiceTask = company_ServiceTask.GetNewOrgProxy(Factory.BOFactory);
			company_ServiceTask.GC_OH_OrgProxy = org_ServiceTask.PK;
			var org_ServiceTaskMapping = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
			var warehouse_ServiceTaskMapping = (BusinessObject)Factory.BOFactory.New<IWhsWarehouse>();
			warehouse_ServiceTaskMapping.FillWithValidTestData();

			CreateOrgPatternMatchOverride(org_Proxy, "AAA", Constants.OrgPatternMatchOverrideRelationships.Organisation, org_ProxyMapping.PK);
			CreateOrgPatternMatchOverride(org_ProxyMapping, "WWW", Constants.OrgPatternMatchOverrideRelationships.Warehouse, warehouse_ProxyMapping.PK);
			CreateOrgPatternMatchOverride(org_ServiceTask, "AAA", Constants.OrgPatternMatchOverrideRelationships.Organisation, org_ServiceTaskMapping.PK);
			CreateOrgPatternMatchOverride(org_ServiceTaskMapping, "WWW", Constants.OrgPatternMatchOverrideRelationships.Warehouse, warehouse_ServiceTaskMapping.PK);
			Factory.SaveForTesting();

			using (DisposableEnvironment.ForCompany(company_ServiceTask.GC_Code, false))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				AssertNoExceptionThrown(() => manager.Process(message));
				AssertStartsWith("Code mapping should work", string.Format("Used code mapping defined in Organization(Code: {0}) > Config > EDI Code Mapping.\r\nLine 23: Mapped Warehouse code 'WWW' to '{1}'.", org_ProxyMapping.OH_Code, ((IWhsWarehouse)warehouse_ProxyMapping).WW_WarehouseCode), message.GetLogNoteText());
			}
		}

		public void TestInvalidEDICodeMappingsAreHandled()
		{
			#region shipmentXML

			const string shipmentXML =
@"<UniversalShipment>
  <Shipment>
	<DataContext>
	  <CodesMappedToTarget>true</CodesMappedToTarget>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		  <Key>S00001000</Key>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<TransportMode>
	  <Code>AIR</Code>
	</TransportMode>
	<VoyageFlightNo>JL0033</VoyageFlightNo>
	<WayBillNumber>13148451675</WayBillNumber>
	<WayBillType>
	  <Code>MWB</Code>
	  <Description>Master Waybill</Description>
	</WayBillType>
	<ContainerCollection>
		<Container>
			<ContainerNumber>OOCL0000006</ContainerNumber>
			<Commodity>
				 <Code>^COM</Code>
			</Commodity>
		       <ContainerType>
				<Code>20GP</Code>
				<Description>Twenty foot flatrack</Description>
				<ISOCode>22P1</ISOCode>
			</ContainerType>
		</Container>
	</ContainerCollection>
	<Order>
        <OrderNumber>REFERENCE</OrderNumber>
		<DropMode>
			<Code>^DM</Code>
		</DropMode>
	</Order>
    <InstructionCollection>
      <Instruction>
		<Equipment>^EQP</Equipment>
	  </Instruction>
    </InstructionCollection>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>DepartureCTOAddress</AddressType>
		<OrganizationCode>1HWU3</OrganizationCode>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(shipmentXML, createInterchange: true);
			var org = Factory.BOFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);

			CreateOrgPatternMatchOverride(org, "1HWU3", Constants.OrgPatternMatchOverrideRelationships.Organisation, new ZGuid("0dd209ca-cf8a-45c7-ad00-61ad2c01b28f"));
			CreateOrgPatternMatchOverride(org, "^COM", Constants.OrgPatternMatchOverrideRelationships.Commodities, new ZGuid("0dd209ca-cf8a-45c7-ad00-61ad2c01b28f"));
			CreateOrgPatternMatchOverride(org, "^EQP", Constants.OrgPatternMatchOverrideRelationships.Equipment, new ZGuid("0dd209ca-cf8a-45c7-ad00-61ad2c01b28f"));
			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			AssertNoExceptionThrown(() => manager.Process(message));
			AssertEquals("Message status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
			AssertEquals(@"
Used code mapping defined in Organization(Code: EDICUS) > Config > EDI Code Mapping.
Warning - Line 25: Commodity mapping with code '^COM' is invalid [Local GUID: '0dd209ca-cf8a-45c7-ad00-61ad2c01b28f'].
Warning - Line 42: Equipment mapping with code '^EQP' is invalid [Local GUID: '0dd209ca-cf8a-45c7-ad00-61ad2c01b28f'].
Warning - Line 48: Organization mapping with code '1HWU3' is invalid [Local GUID: '0dd209ca-cf8a-45c7-ad00-61ad2c01b28f'].
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingContainer found, creating new ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Warning - Matching 'DepartureCTOAddress':- No match found for '[Org. Code: 1HWU3]'.
Added Consol (Master Bill='13148451675') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='13148451675') with 1 x ForwardingContainer.".Trim(), message.GetLogNoteText());
		}

		void CreateOrgPatternMatchOverride(OrgHeader codeMapSource, ZString foreignCode, ZString relationship, ZGuid localGuid)
		{
			var matchOverride = Factory.New<OrgPatternMatchOverride>();
			matchOverride.OO_OH = codeMapSource.PK;
			matchOverride.OO_ForeignCode = foreignCode;
			matchOverride.OO_Relationship = relationship;
			matchOverride.OO_LocalGuid = localGuid;
		}
	}
}
