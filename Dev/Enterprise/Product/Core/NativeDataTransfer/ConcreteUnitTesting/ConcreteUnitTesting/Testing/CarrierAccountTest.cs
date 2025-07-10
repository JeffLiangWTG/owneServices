using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	class CarrierAccountTest : TestCaseWithFactory
	{
		public void TestCarrierAccountImport()
		{
			var billToPartyPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var carrierPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();

			string carrierAccountXml = $@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Header />
  <Body>
    <CarrierAccount>
      <OrgCarrierAccount Action=""MERGE"">
        <AccountNumber>4324</AccountNumber>
        <DepotID>Sydney4</DepotID>
        <MerchantNumber>4635</MerchantNumber>
        <IsActive>true</IsActive>
        <BillToParty>
          <PK>{billToPartyPK}</PK>
        </BillToParty>
        <Carrier>
          <PK>{carrierPK}</PK>
			  </Carrier>
      </OrgCarrierAccount>
    </CarrierAccount>
  </Body>
</Native>";

			var manager = new ImportServiceManagerForTesting();
			using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(carrierAccountXml)))
			{
				manager.ImportService.Import(memoryStream);
			}

			string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Processed: CarrierAccount
--- Import Process Finished -----------------------------------------------------------
OrgCarrierAccount - 1 inserts, 0 updates, 0 deletes";

			var logs = manager.GetLogs();
			AssertMultilineASCIIEquals(expectedLog, logs);
		}

		public void TestCarrierAccountExport()
		{
			var carrierAccount = Factory.New<OrgCarrierAccount>();
			carrierAccount.OAN_AccountNumber = "ABC";
			carrierAccount.OAN_DepotID = "ABCTest";
			carrierAccount.OAN_MerchantNumber = "ABC Test";
			carrierAccount.OAN_OH_BillToParty = Factory.NewWithValidTestData<OrgHeader>().PK;
			carrierAccount.OAN_OH_Carrier = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			string carrierAccountXml = "";
			using (var dataStream = xmlSerializer.SerializeToStream(carrierAccount))
			using (var reader = new StreamReader(dataStream))
			{
				carrierAccountXml = reader.ReadToEnd();
			}

			AssertNotNullOrEmpty("CarrierAccount xml was generated.", carrierAccountXml);
			AssertContains("CarrierAccount details are correct.", "<OrgCarrierAccount Action=\"MERGE\">", carrierAccountXml);
			AssertContains("CarrierAccount details are correct.", "<AccountNumber>ABC</AccountNumber>", carrierAccountXml);
			AssertContains("CarrierAccount details are correct.", "<DepotID>ABCTest</DepotID>", carrierAccountXml);
			AssertContains("CarrierAccount details are correct.", "<MerchantNumber>ABC Test</MerchantNumber>", carrierAccountXml);
			AssertContains("CarrierAccount details are correct.", $"<PK>{carrierAccount.OAN_OH_BillToParty}</PK>", carrierAccountXml);
			AssertContains("CarrierAccount details are correct.", $"<PK>{carrierAccount.OAN_OH_Carrier}</PK>", carrierAccountXml);
		}
	}
}
