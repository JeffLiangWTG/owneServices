using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	class WarehouseClientAccountAssociationTest : TestCaseWithFactory
	{
		public void TestWarehouseClientAccountAssociationImport()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var salesChannel = Factory.New<IWhsSalesChannel>();
			salesChannel.WSH_Code = "ABC";
			salesChannel.WSH_Description = "ABC Test";
			Factory.Save();
			var association = $@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Header />
  <Body>
    <WarehouseClientAccountAssociation>
      <OrgWhsClientAccountAssociation Action=""MERGE"">
        <Client>
          <PK>{client.PK}</PK>
        </Client>
        <SalesChannel>
          <PK>{salesChannel.PK}</PK>
        </SalesChannel>
        <CarrierAccount>
          <PK>{carrierAccount.PK}</PK>
        </CarrierAccount>
      </OrgWhsClientAccountAssociation>
    </WarehouseClientAccountAssociation>
  </Body>
</Native>";

			var manager = new ImportServiceManagerForTesting();
			using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(association)))
			{
				manager.ImportService.Import(memoryStream);
			}

			string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Processed: WarehouseClientAccountAssociation
--- Import Process Finished -----------------------------------------------------------
OrgWhsClientAccountAssociation - 1 inserts, 0 updates, 0 deletes";

			var logs = manager.GetLogs();
			AssertMultilineASCIIEquals(expectedLog, logs);
		}

		public void TestWarehouseClientAccountAssociationExport()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1 Name", "WH1", "A");
			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "1234";
			carrierAccount.OAN_MerchantNumber = "5848";
			var association = Factory.New<OrgWhsClientAccountAssociation>();
			association.OWC_OAN_CarrierAccount = carrierAccount.PK;
			association.OWC_OH_Client = client.PK;
			association.OWC_WW_Warehouse = warehouse.PK;
			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			string associationXml = "";
			using (var dataStream = xmlSerializer.SerializeToStream(association))
			using (var reader = new StreamReader(dataStream))
			{
				associationXml = reader.ReadToEnd();
			}

			AssertNotNullOrEmpty("WarehouseClientAccountAssociation xml was generated.", associationXml);
			AssertContains("WarehouseClientAccountAssociation details are correct.", "<OrgWhsClientAccountAssociation Action=\"MERGE\">", associationXml);
			AssertContains("WarehouseClientAccountAssociation details are correct.", "<SalesChannel TableName=\"WhsSalesChannel\" />", associationXml);
			AssertContains("WarehouseClientAccountAssociation details are correct.", "<MerchantNumber>5848</MerchantNumber>", associationXml);
		}
	}
}
