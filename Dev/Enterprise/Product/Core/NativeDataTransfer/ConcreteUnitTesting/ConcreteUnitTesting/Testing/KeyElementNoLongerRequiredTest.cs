using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class KeyElementNoLongerRequiredTest : TestCaseWithFactory
	{
		public void TestExportXMLIfNaturalKeyIsNotPresentIncludesPKAndKeyElementIsRemoved()
		{
			var airline = Factory.New<RefAirline>();

			airline.RM_AddressLine1 = "No. 166, Upper Pansodan Street";
			airline.RM_AddressLine2 = "MMB Tower, Level 5, Mingaiar Taung Nyunt";
			airline.RM_AirlineCity = "Townshi, Yangon";
			airline.RM_AirlineCountry = "Myanmar";
			airline.RM_AirlineName1 = "Yangon Airways Limited 0";
			airline.RM_AirlinePostalCode = "11221";
			airline.RM_DuplicateFlagIndicator = true;
			airline.RM_IsCASSControlled = false;
			airline.RM_MembershipFlagARINC = false;
			airline.RM_MembershipFlagATA = false;
			airline.RM_MembershipFlagIATA = false;
			airline.RM_MembershipFlagSITA = true;
			airline.RM_TwoCharacterCode = "HK";
			airline.RM_TypeOfOperationsCode = "A";

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };
			using (var dataStream = xmlSerializer.SerializeToStream(airline))
			using (var reader = new StreamReader(dataStream))
			{
				string actualMessage = reader.ReadToEnd();
				AssertNotContains(@"<Key>", actualMessage);
				AssertNotContains(@"<KeyElement Type=""Primary"" Field=""PK"" />", actualMessage);
				AssertNotContains(@"</Key>", actualMessage);
				AssertContains(airline.PK.ToString(), actualMessage);
			}
		}

		#region RefAirlineXML

		const string RefAirlineXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Airline>
      <RefAirline Action=""MERGE"">
        <PK>{0}</PK>
        <AirlineName1>Yangon Airways Limited 0</AirlineName1>
        <TwoCharacterCode>HK</TwoCharacterCode>
        <DuplicateFlagIndicator>true</DuplicateFlagIndicator>
        <AddressLine1>No. 166, Upper Pansodan Street</AddressLine1>
        <AddressLine2>MMB Tower, Level 5, Mingaiar Taung Nyunt</AddressLine2>
        <AirlineCity>Townshi, Yangon</AirlineCity>
        <AirlineCountry>Myanmar</AirlineCountry>
        <AirlinePostalCode>11221</AirlinePostalCode>
        <MembershipFlagSITA>true</MembershipFlagSITA>
        <MembershipFlagARINC>false</MembershipFlagARINC>
        <MembershipFlagIATA>false</MembershipFlagIATA>
        <MembershipFlagATA>false</MembershipFlagATA>
        <TypeOfOperationsCode>A</TypeOfOperationsCode>
        <IsCASSControlled>false</IsCASSControlled>
      </RefAirline>
    </Airline>
  </Body>
</ReferenceData>";

		#endregion

		public void TestImportXMLIfNaturalKeyIsNotPresentWorksWithoutKeyElementAndFindsByPK()
		{
			var airline = Factory.New<RefAirline>();

			Factory.Save();

			var encoding = new System.Text.UTF8Encoding();
			var messageWithPK = string.Format(RefAirlineXML, airline.PK.ToString());
			using (var stream = new MemoryStream(encoding.GetBytes(messageWithPK)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Airline
--- Import Process Finished -----------------------------------------------------------
RefAirline - 0 inserts, 1 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on second import", expectedLog, manager.GetLogs());
			}
		}

		public void TestExportXMLIfNaturalKeyIsPresentDoesNotIncludeKeyElementButStillKeepsPK()
		{
			var unloco = Factory.New<RefUNLOCO>();

			unloco.RL_Code = "TVFUZ";
			unloco.RL_IsActive = true;
			unloco.RL_IsSystem = true;
			unloco.RL_IsUpdatable = true;
			unloco.RL_PortName = "Fuzafuti";
			unloco.RL_IATA = "FUZ";
			unloco.RL_HasAirport = true;
			unloco.RL_HasSeaport = true;
			unloco.RL_HasRail = false;
			unloco.RL_HasRoad = false;
			unloco.RL_HasPost = true;
			unloco.RL_HasCustomsLodge = false;
			unloco.RL_HasUnload = false;
			unloco.RL_HasStore = false;
			unloco.RL_HasTerminal = false;
			unloco.RL_HasDischarge = false;
			unloco.RL_HasOutport = false;
			unloco.RL_HasBorderCrossing = false;
			unloco.RL_RN_NKCountryCode = "TO";

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };
			using (var dataStream = xmlSerializer.SerializeToStream(unloco))
			using (var reader = new StreamReader(dataStream))
			{
				string actualMessage = reader.ReadToEnd();
				AssertNotContains(@"<Key>", actualMessage);
				AssertNotContains(@"<KeyElement Type=""Primary"" Field=""PK"" />", actualMessage);
				AssertNotContains(@"<KeyElement Type=""Candidate"" Field=""Code"" />", actualMessage);
				AssertNotContains(@"</Key>", actualMessage);
				AssertContains(unloco.PK.ToString(), actualMessage);
			}
		}

		#region RefUNLOCOXML

		const string RefUNLOCOXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <UNLOCO>
      <RefUNLOCO Action=""MERGE"">
        <Code>TVFUZ</Code>
        <IsActive>true</IsActive>
        <IsSystem>true</IsSystem>
        <IsUpdatable>true</IsUpdatable>
        <PortName>Fuzafuti</PortName>
        <NameWithDiacriticals>Fuzafuti</NameWithDiacriticals>
        <IATA>FUZ</IATA>
        <HasAirport>true</HasAirport>
        <HasSeaport>true</HasSeaport>
        <HasRail>false</HasRail>
        <HasRoad>false</HasRoad>
        <HasPost>true</HasPost>
        <HasCustomsLodge>false</HasCustomsLodge>
        <HasUnload>false</HasUnload>
        <HasStore>false</HasStore>
        <HasTerminal>false</HasTerminal>
        <HasDischarge>false</HasDischarge>
        <HasOutport>false</HasOutport>
        <HasBorderCrossing>false</HasBorderCrossing>
        <CountryCode TableName=""RefCountry"">
          <Code>TO</Code>
        </CountryCode>
      </RefUNLOCO>
    </UNLOCO>
  </Body>
</ReferenceData>";

		#endregion

		public void TestImportXMLIfNaturalKeyIsPresent()
		{
			var encoding = new System.Text.UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(RefUNLOCOXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: UNLOCO
--- Import Process Finished -----------------------------------------------------------
RefUNLOCO - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}
		}
	}
}
