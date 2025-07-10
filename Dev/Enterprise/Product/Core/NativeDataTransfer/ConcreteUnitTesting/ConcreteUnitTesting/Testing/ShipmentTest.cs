using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class ShipmentTest : TestCaseWithFactory
	{
		public void TestExportHasWorkflowInIt()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			template.P0_SubType1 = "AIR";
			template.P0_SubType2 = "IMP";

			var customField = template.GenCustomColumnDefinitions.AddNew();

			customField.XC_Name = "DIFFICULTY";
			customField.XC_Type = "STR";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			AssertEquals("Precondition: shipment.IsImport", true, shipment.IsImport());

			var consignor = shipment.ConsignorDocumentaryAddress;
			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "ABC EXPORT CO";

			var consignee = shipment.ConsigneeDocumentaryAddress;
			consignee.E2_AddressOverride = true;
			consignee.E2_CompanyName = "XYZ IMPORT CO";

			var customFieldsBO = ((ICustomFieldProvider)shipment).GetCustomBusinessObject();

			var propertyName = ((IDynamicBusinessObject)customFieldsBO).PropertyNames[0];
			customFieldsBO[propertyName] = "HARD. REAL HARD.";

			Factory.Save();

			string actualXML = "";

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(shipment))
			using (var reader = new StreamReader(dataStream))
			{
				actualXML = reader.ReadToEnd();
			}

			var element = XElement.Load(new StringReader(actualXML));

			CombineAssertions(delegate
			{
				AssertNotEquals("Should include GenCustomAddOnValue element.", 0, element.Descendants().Count(e => e.Name.LocalName == "GenCustomAddOnValue"));
				AssertNotEquals("Should include GenCustomAddOnValueCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "GenCustomAddOnValueCollection"));
				AssertContains("Making sure Custom Field data is present in XML.", ">HARD. REAL HARD.<", actualXML);
			});
		}

		public void TestShipmentXMLImport()
		{
			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(-1).ToDateTime();
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(basicShipmentNativeXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"--- Start Import Process --------------------------------------------------------------
Property: ""Data"" of Entity: ""GenCustomAddOnValue"" was trimmed of white space and stripped of CRLF characters
Record: Shipment failed to Import:
The 'Shipment' Native XML dataset has been deprecated. Please use the Universal Shipment XML instead.
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.", insertLog);
			}

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(basicShipmentNativeXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Property: ""Data"" of Entity: ""GenCustomAddOnValue"" was trimmed of white space and stripped of CRLF characters
Processed: Shipment
--- Import Process Finished -----------------------------------------------------------
JobShipment - 1 inserts, 0 updates, 0 deletes
ProcessTasks - 1 inserts, 0 updates, 0 deletes
JobDocAddress - 4 inserts, 0 updates, 0 deletes
CusEntryNum - 1 inserts, 0 updates, 0 deletes
JobDocsAndCartage - 1 inserts, 0 updates, 0 deletes
JobOrderItem - 2 inserts, 0 updates, 0 deletes
GenCustomAddOnValue - 2 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			var shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HAL3289901890"));
			AssertNotNull(shipment);

			CombineAssertions(delegate
			{
				var addresses = string.Join("\r\n", shipment.DocAddresses
					.Cast<JobDocAddress>()
					.Select(o => o.DocAddressType.ToString() + ": " + o.AddressAsASingleLine)
					.OrderBy(o => o).ToArray());
				AssertMultilineASCIIEquals("Addresses", @"
ConsigneeDocumentaryAddress: QUEENSLAND FORKLIFT SPARES PTY LTD 129 BALHAM ROAD ROCKLEA QLD
ConsigneePickupDeliveryAddress: QUEENSLAND FORKLIFT SPARES PTY LTD 129 BALHAM ROAD ROCKLEA QLD
ConsignorDocumentaryAddress: RADIOMETER PACIFIC LTD 10-20 SYLVIA PARK RD MT WELLINGTON AUCKLAND, NEW ZEALAND NEW ZEALAND
ConsignorPickupDeliveryAddress: RADIOMETER PACIFIC LTD 10-20 SYLVIA PARK RD WELLINGTON AUCKLAND, NEW ZEALAND NEW ZEALAND
					".Trim(), addresses);

				var customFieldsBO = ((ICustomFieldProvider)shipment).GetCustomBusinessObject();
				var customWorkflowFieldValues = ((IDynamicBusinessObject)customFieldsBO).PropertyNames
					.Where(o => !o.EndsWith("Info"))
					.Select(o => o + ": " + customFieldsBO[o].ToString())
					.OrderBy(o => o).ToArray();
				AssertContains("Deserialization should strip CRLF from properties with max_length < 200.", "THIS ONE ROCKS MOCK SOCKS", string.Join(string.Empty, customWorkflowFieldValues));
				AssertMultilineASCIIEquals("customWorkflowFieldValues", @"
__HEADSPACE__prop__ZString: THIS ONE ROCKS MOCK SOCKS
__SOCK ROOM__prop__ZString: ANOTHER FIELD",
					string.Join("\r\n", customWorkflowFieldValues));

				AssertEquals("shipment.DocsAndCartage.JP_OrderItemsAsString", "123456,789654", shipment.DocsAndCartage.JP_OrderItemsAsString);

				var additionalReferences = shipment.Numbers
					.Cast<CusEntryNumber>()
					.Select(o => o.CE_Category + " - " + o.CE_EntryType + "/" + o.CE_RN_NKCountryCode + ": " + o.CE_EntryNum)
					.OrderBy(o => o).ToArray();
				AssertMultilineASCIIEquals("additionalReferences", @"
OTH - CON/AU: CON342178912
					".Trim(), string.Join("\r\n", additionalReferences));

				var processTasks = shipment.WorkflowItems.Milestones
					.Cast<ProcessTask>()
					.Select(o => o.P9_Sequence + " - " + o.P9_TaskID + " - " + o.P9_SE_NKMilestoneEvent + " - " + o.P9_Description)
					.OrderBy(o => o).ToArray();
				AssertMultilineASCIIEquals("processTasks", @"
10 - T00001158 - AID - All Import Documents Received
					".Trim(), string.Join("\r\n", processTasks));
			});

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(basicShipmentNativeXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Property: ""Data"" of Entity: ""GenCustomAddOnValue"" was trimmed of white space and stripped of CRLF characters
Processed: Shipment
--- Import Process Finished -----------------------------------------------------------
JobShipment - 0 inserts, 0 updates, 0 deletes
ProcessTasks - 0 inserts, 0 updates, 0 deletes
JobDocAddress - 0 inserts, 0 updates, 0 deletes
CusEntryNum - 0 inserts, 0 updates, 0 deletes
JobDocsAndCartage - 0 inserts, 0 updates, 0 deletes
JobOrderItem - 0 inserts, 0 updates, 0 deletes
GenCustomAddOnValue - 0 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}
		}

		#region basicShipmentNativeXML

		const string basicShipmentNativeXML = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Body>
    <Shipment version=""1.0"">
      <JobShipment Action=""MERGE"">
        <UniqueConsignRef>S00001029</UniqueConsignRef>
        <AWBServiceLevel>STD</AWBServiceLevel>
        <HouseBill>HAL3289901890</HouseBill>
        <IsForwardRegistered>true</IsForwardRegistered>
        <GoodsDescription>MACIEJ, MACIEJ, MACIEJ!!!</GoodsDescription>
        <INCO>FOB</INCO>
        <TransportMode>AIR</TransportMode>
        <PackingMode>LSE</PackingMode>
        <UnitOfVolume>M3</UnitOfVolume>
        <UnitOfWeight>KG</UnitOfWeight>
        <ShippedOnBoard>SHP</ShippedOnBoard>
        <ExportControlRegularApprovedShipper>UNK</ExportControlRegularApprovedShipper>
        <ShipmentType>STD</ShipmentType>
        <ScreeningStatus>UNK</ScreeningStatus>
        <Phase>ALL</Phase>
        <ProcessTasksCollection>
          <ProcessTasks Action=""MERGE"">
            <TaskID>T00001158</TaskID>
            <Sequence>10</Sequence>
            <Type>MIL</Type>
            <Status>NXT</Status>
            <Description>All Import Documents Received</Description>
            <Condition2>IMP</Condition2>
            <IsPublished>true</IsPublished>
            <MilestoneEvent TableName=""StmEvent"">
              <Code>AID</Code>
            </MilestoneEvent>
            <GlbCompany>
              <Code>EDI</Code>
            </GlbCompany>
          </ProcessTasks>
        </ProcessTasksCollection>
        <JobDocAddressCollection>
          <JobDocAddress Action=""MERGE"">
            <AddressType>CED</AddressType>
            <AddressOverride>false</AddressOverride>
            <Address TableName=""OrgAddress"">
              <Code>Pick Up Address</Code>
              <OrgHeader>
                <Code>QFSAUS</Code>
              </OrgHeader>
            </Address>
          </JobDocAddress>
          <JobDocAddress Action=""MERGE"">
            <AddressType>CEG</AddressType>
            <AddressOverride>false</AddressOverride>
            <Address TableName=""OrgAddress"">
              <Code>Pick Up Address</Code>
              <OrgHeader>
                <Code>QFSAUS</Code>
              </OrgHeader>
            </Address>
          </JobDocAddress>
          <JobDocAddress Action=""MERGE"">
            <AddressType>CRD</AddressType>
            <AddressOverride>false</AddressOverride>
            <Address TableName=""OrgAddress"">
              <Code>PST: 10-20 SYLVIA PARK RD</Code>
              <OrgHeader>
                <Code>RADPAC</Code>
              </OrgHeader>
            </Address>
          </JobDocAddress>
          <JobDocAddress Action=""MERGE"">
            <AddressType>CRG</AddressType>
            <AddressOverride>false</AddressOverride>
            <Address TableName=""OrgAddress"">
              <Code>Pickup and Delivery Addre</Code>
              <OrgHeader>
                <Code>RADPAC</Code>
              </OrgHeader>
            </Address>
          </JobDocAddress>
        </JobDocAddressCollection>
        <CusEntryNumCollection>
          <CusEntryNum Action=""MERGE"">
            <EntryNum>CON342178912</EntryNum>
            <EntryType>CON</EntryType>
            <Category>OTH</Category>
            <CountryCode TableName=""RefCountry"">
              <Code>AU</Code>
            </CountryCode>
          </CusEntryNum>
        </CusEntryNumCollection>
        <JobDocsAndCartageCollection>
          <JobDocsAndCartage Action=""MERGE"">
            <PrintOptionForPackagesOnAWB>DEF</PrintOptionForPackagesOnAWB>
            <JobOrderItemCollection>
              <JobOrderItem Action=""MERGE"">
                <OrderReference>123456</OrderReference>
                <Sequence>1</Sequence>
              </JobOrderItem>
              <JobOrderItem Action=""MERGE"">
                <OrderReference>789654</OrderReference>
                <Sequence>2</Sequence>
              </JobOrderItem>
            </JobOrderItemCollection>
          </JobDocsAndCartage>
        </JobDocsAndCartageCollection>
        <GenCustomAddOnValueCollection>
          <GenCustomAddOnValue Action=""MERGE"">
            <Name>HeadSpace</Name>
            <Type>STR</Type>" +
"            <Data>THIS&#xD;&#xA;ONE\r\nROCKS&#xA;MOCK\nSOCKS</Data>" +
@"            <IsRuleEnabled>true</IsRuleEnabled>
          </GenCustomAddOnValue>
          <GenCustomAddOnValue Action=""MERGE"">
            <Name>Sock Room</Name>
            <Type>STR</Type>
            <Data>ANOTHER FIELD</Data>
            <IsRuleEnabled>true</IsRuleEnabled>
          </GenCustomAddOnValue>
        </GenCustomAddOnValueCollection>
        <Origin TableName=""RefUNLOCO"">
          <Code>NZWLG</Code>
        </Origin>
        <Destination TableName=""RefUNLOCO"">
          <Code>AUBNE</Code>
        </Destination>
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
      </JobShipment>
    </Shipment>
  </Body>
</Native>";

		#endregion

		public void TestExportHasNewTablesAndContentInIt()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var milestone = shipment.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "DCD";
			milestone.P9_Description = "DOG CARRIED";

			var consignor = shipment.ConsignorDocumentaryAddress;
			consignor.E2_AddressOverride = true;
			consignor.E2_Address1 = "ABC\r\nEXPORT\nCO";

			var consignee = shipment.ConsigneeDocumentaryAddress;
			consignee.E2_AddressOverride = true;
			consignee.E2_CompanyName = "XYZ IMPORT CO";

			var additionalReference = shipment.Numbers.AddNew();
			additionalReference.CE_EntryNum = "MAMAMIA";
			additionalReference.CE_EntryType = "FOO";

			shipment.DocsAndCartage.JP_OrderItemsAsString = "123456,987654";
			AssertEquals("shipment.DocsAndCartage.OrderItems.Count", 2, shipment.DocsAndCartage.OrderItems.Count);

			Factory.Save();

			var actualMessage = string.Empty;

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(shipment))
			using (var reader = new StreamReader(dataStream))
			{
				actualMessage = reader.ReadToEnd();
			}

			var element = XElement.Load(new StringReader(actualMessage));
			AssertContains("Deserialization should strip CRLF from properties with max_length < 200.", "ABC EXPORT CO", actualMessage);

			CombineAssertions(delegate
			{
				AssertNotEquals("Compact XML should include ProcessTasks element.", 0, element.Descendants().Count(e => e.Name.LocalName == "ProcessTasks"));
				AssertNotEquals("Compact XML should include ProcessTasksCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "ProcessTasksCollection"));

				AssertNotEquals("Compact XML should include JobDocAddress element.", 0, element.Descendants().Count(e => e.Name.LocalName == "JobDocAddress"));
				AssertNotEquals("Compact XML should include JobDocAddressCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "JobDocAddressCollection"));

				AssertNotEquals("Compact XML should include JobOrderItem element.", 0, element.Descendants().Count(e => e.Name.LocalName == "JobOrderItem"));
				AssertNotEquals("Compact XML should include JobOrderItemCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "JobOrderItemCollection"));

				AssertNotEquals("Compact XML should include CusEntryNum element.", 0, element.Descendants().Count(e => e.Name.LocalName == "CusEntryNum"));
				AssertNotEquals("Compact XML should include CusEntryNumCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "CusEntryNumCollection"));

				AssertNotEquals("Compact XML should include JobDocsAndCartage element.", 0, element.Descendants().Count(e => e.Name.LocalName == "JobDocsAndCartage"));
				AssertNotEquals("Compact XML should include JobDocsAndCartageCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "JobDocsAndCartageCollection"));
			});

			shipment.DocsAndCartage.JP_OrderItemsAsString = "";
			AssertEquals("shipment.DocsAndCartage.OrderItems.Count", 0, shipment.DocsAndCartage.OrderItems.Count);

			var newOrder = shipment.AttachedOrders.AddNew();
			newOrder.JD_OrderNumber = "CRAZYHORSE22";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			newOrder.SupplierPK = orgHeader.PK;
			newOrder.BuyerPK = orgHeader.PK;

			Factory.Save();

			using (var dataStream = xmlSerializer.SerializeToStream(shipment))
			using (var reader = new StreamReader(dataStream))
			{
				actualMessage = reader.ReadToEnd();
			}

			element = XElement.Load(new StringReader(actualMessage));

			CombineAssertions(delegate
			{
				AssertNotEquals("Compact XML should include JobOrderHeader element.", 0, element.Descendants().Count(e => e.Name.LocalName == "JobOrderHeader"));
				AssertNotEquals("Compact XML should include JobOrderHeaderCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "JobOrderHeaderCollection"));
			});
		}
	}
}

