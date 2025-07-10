using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	class ImportServiceUseBOTemplateTest : TestCaseWithFactory
	{
		public void TestCanImportNewOrgHeaderWithAddress()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_OrgHeaderWithAddress)))
			{
				var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_ProcessType = OrgHeaderWorkflowDescriptor.WorkflowTypeCode;

				ProcessTask milestoneTemplate = template.WorkflowItems.Milestones.AddNew();
				milestoneTemplate.TriggerConditions.TriggerConditionValue = "ATH";
				milestoneTemplate.P9_EstimatedDefaultTimeDelta = new ZDateTime(2000, 1, 1).AddDays(-2);
				Factory.Save();

				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 2 inserts, 0 updates, 0 deletes
OrgAddressCapability - 2 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			var createdOrgHeaderList = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FURFORHKG"));
			AssertEquals(1, createdOrgHeaderList.Length);
			AssertEquals("1 milestone created", 1, createdOrgHeaderList[0].WorkflowItems.Milestones.Count);
			AssertEquals("Event Reference exists", "ATH", createdOrgHeaderList[0].WorkflowItems.Milestones[0].P9_TriggerConditionValue);
		}

		#region XML_OrgHeaderWithAddress

		const string XML_OrgHeaderWithAddress = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>AUDEMOALX</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>ASFURNHKG</Code>
        <IsActive>true</IsActive>
        <FullName>FURNISH FORWARDING</FullName>
        <IsForwarder>true</IsForwarder>
        <Language>EN</Language>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>Pick Up Address</Code>
            <Language>EN</Language>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>UNIT 1, 1ST FLOOR, BLOCK B</Address1>
            <Address2>HOI BUN IND BLDG                         6 WIN YIP</Address2>
            <City>KONG</City>
            <FCLEquipmentNeeded>ASK</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>ASK</LCLEquipmentNeeded>
            <AIREquipmentNeeded>ASK</AIREquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>PIC</AddressType>
                <IsMainAddress>false</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>HKHKG</Code>
            </RelatedPortCode>
          </OrgAddress>
          <OrgAddress Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>PST: UNIT A1, 4TH FLOOR,</Code>
            <Language>EN</Language>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
            <Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
            <City>KOWLOON  HONG KONG</City>
            <FCLEquipmentNeeded>ASK</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>ASK</LCLEquipmentNeeded>
            <AIREquipmentNeeded>ASK</AIREquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>HKHKG</Code>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>HKHKG</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>
";

		#endregion

		public void TestMethodGetBusinessObjectBaseTypeFromTablePrefix_CanResolveAllNativeTableNames()
		{
			CombineAssertions(() =>
				{
					foreach (var tableMapping in GlobalDefinition.Instance.TableMapping)
					{
						string tableName = tableMapping.Key;
						string tablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(tableName);
						var boTypeForTablePrefix = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(tablePrefix);
						AssertNotNull("boTypeForTablePrefix - " + tablePrefix + " - " + tableName, boTypeForTablePrefix);
					}
				});
		}
	}
}
