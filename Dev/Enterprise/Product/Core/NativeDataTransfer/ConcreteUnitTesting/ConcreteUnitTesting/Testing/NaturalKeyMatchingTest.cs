using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Loaders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class NaturalKeyMatchingTest : TestCaseWithFactory
	{
		public void TestCombinationDateBoolAndNumberCandidateKeyWorksFine()
		{
			TestUtil.AddDummyBizoToGlobalDefinitions();
			TestUtil.Connection.ExecuteNonQuery(@"Create UNIQUE INDEX NR_UX__Z0_Number_Z0_Date_Z0_Bool ON DummyBizo(Z0_Number,Z0_Date,Z0_Bool)");

			var loader = new DefinitionAssemblyLoader(new[]
			{
				typeof(NaturalKeyMatchingTest).Assembly.FullName,
				typeof(DefinitionAssemblyLoader).Assembly.FullName
			});
			EntitySetDefinitionCache.SetAlternateDefinitionLoaderForTesting(loader);

			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Number = 1;
			dummyBO.Z0_Date = new DateTime(2011, 9, 16, 3, 2, 1);
			dummyBO.Z0_Bool = true;

			Factory.Save();

			var dummyXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Body>
    <Dummy>
      <DummyBizo Action=""MERGE"">
        <Number>1</Number>
        <Date>2011-09-16T03:02:01</Date>
        <Bool>true</Bool>
        <VarCharMax>FAT</VarCharMax>
      </DummyBizo>
    </Dummy>
  </Body>
</ReferenceData>";

			using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(dummyXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Dummy
--- Import Process Finished -----------------------------------------------------------
DummyBizo - 0 inserts, 1 updates, 0 deletes
			".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}

			var factory = new BusinessObjectFactory();
			var reloadedBO = factory.Load<DummyBusinessObject>(dummyBO.PK);

			AssertEquals("reloadedBO.Z0_VarCharMax got updated", "FAT", reloadedBO.Z0_VarCharMax);
		}

		public void TestImportingWithDateAsPartOfNaturalKeyAndDayNumberOver12()
		{
			TestCaseHelper.ClearTable(RefExchangeRate.Schema.TableName);

			using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(XMLImportRefExchangeRateWithDayNumberOver12)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: CurrencyExchangeRate
--- Import Process Finished -----------------------------------------------------------
RefExchangeRate - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on first import", expectedLog, manager.GetLogs());
			}

			CombineAssertions(delegate
			{
				var exchangeRate = Factory.LoadTop1<RefExchangeRate>(new ZQuery());
				AssertEquals("exchangeRate.RE_ExRateType", "CUS", exchangeRate.RE_ExRateType);
				AssertEquals("exchangeRate.RE_StartDate", new CargoWise.Types.ZDateTime(2011, 9, 15, 0, 0, 0), exchangeRate.RE_StartDate);
				AssertEquals("exchangeRate.RE_ExpiryDate", new CargoWise.Types.ZDateTime(2011, 9, 15, 23, 59, 0), exchangeRate.RE_ExpiryDate);
				AssertEquals("exchangeRate.RE_SellRate", 189.234m, exchangeRate.RE_SellRate);
				AssertEquals("exchangeRate.RE_RX_NKExCurrency", "HUF", exchangeRate.RE_RX_NKExCurrency);
				AssertEquals("exchangeRate.Company.GC_Code", "EDI", exchangeRate.Company.GC_Code);
			});

			using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(XMLImportRefExchangeRateWithDayNumberOver12)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: CurrencyExchangeRate
--- Import Process Finished -----------------------------------------------------------
RefExchangeRate - 0 inserts, 1 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on second import", expectedLog, manager.GetLogs());
			}
		}

		#region XMLImportRefExchangeRateWithDayNumberOver12

		const string XMLImportRefExchangeRateWithDayNumberOver12 = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Body>
     <CurrencyExchangeRate>
        <RefExchangeRate Action=""Merge"">
          <ExRateType>CUS</ExRateType>
          <StartDate>2011-09-15T00:00:00</StartDate>
          <ExpiryDate>2011-09-15T23:59:59</ExpiryDate>
          <SellRate>189.234000000</SellRate>
          <RefCurrency>
            <Code>HUF</Code>
          </RefCurrency>
          <GlbCompany>
            <Code>EDI</Code>
          </GlbCompany>
        </RefExchangeRate>
      </CurrencyExchangeRate>
  </Body>
</ReferenceData>";

		#endregion

		public void TestImportingWithMultiFieldNaturalKeyWithPartOfTheKeyBeingEmpty()
		{
			DataRowExtension.ClearConstraintCache();
			TestUtil.AddDummyBizoToGlobalDefinitions();
			TestUtil.Connection.ExecuteNonQuery(@"Create UNIQUE INDEX NR_UX__Z0_Number_Z0_Guid ON DummyBizo(Z0_Number,Z0_Guid)");

			var loader = new DefinitionAssemblyLoader(new[]
			{
				typeof(NaturalKeyMatchingTest).Assembly.FullName,
				typeof(DefinitionAssemblyLoader).Assembly.FullName
			});
			EntitySetDefinitionCache.SetAlternateDefinitionLoaderForTesting(loader);

			var dummyBOs = new DummyBusinessObject[5];

			dummyBOs[0] = Factory.New<DummyBusinessObject>();
			dummyBOs[0].Z0_Number = 1;
			dummyBOs[0].Z0_Guid = Guid.NewGuid();

			dummyBOs[1] = Factory.New<DummyBusinessObject>();
			dummyBOs[1].Z0_Number = 2;

			dummyBOs[2] = Factory.New<DummyBusinessObject>();
			dummyBOs[2].Z0_Number = 3;
			dummyBOs[2].Z0_Guid = dummyBOs[0].Z0_Guid;

			dummyBOs[3] = Factory.New<DummyBusinessObject>();
			dummyBOs[3].Z0_Number = 1;
			dummyBOs[3].Z0_Guid = Guid.NewGuid();

			dummyBOs[4] = Factory.New<DummyBusinessObject>();
			dummyBOs[4].Z0_Number = 2;
			dummyBOs[4].Z0_Guid = dummyBOs[0].Z0_Guid;

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };
			string[] dummyString = { "FOO", "BAR", "BAZ", "GEE", "TOO" };
			var dummyBOXML = new string[5];

			for (int index = 0; index < dummyBOs.Length; index++)
			{
				string dummyXML = string.Empty;

				using (var dataStream = xmlSerializer.SerializeToStream(dummyBOs[index]))
				using (var reader = new StreamReader(dataStream))
				{
					dummyXML = reader.ReadToEnd();
				}

				dummyBOXML[index] = dummyXML.Replace("<VarCharMax></VarCharMax>", "<VarCharMax>" + dummyString[index] + "</VarCharMax>").Replace(string.Format("<PK>{0}</PK>", dummyBOs[index].PK.ToString()), "");

				using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(dummyBOXML[index])))
				{
					var manager = new ImportServiceManagerForTesting();
					manager.ImportService.Import(stream);

					string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Dummy
--- Import Process Finished -----------------------------------------------------------
DummyBizo - 0 inserts, 1 updates, 0 deletes
			".Trim();

					AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
				}

				var factory = new BusinessObjectFactory();
				var reloadedBO = factory.Load<DummyBusinessObject>(dummyBOs[index].PK);

				AssertEquals(dummyString[index], reloadedBO.Z0_VarCharMax);
			}
			DataRowExtension.ClearConstraintCache();
		}

		#region XMLImportRefExchangeRate

		const string XMLImportRefExchangeRate = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Body>
     <CurrencyExchangeRate>
        <RefExchangeRate Action=""MERGE"">
          <ExRateType>CUS</ExRateType>
          <StartDate>01/01/2011 12:00:00 AM</StartDate>
          <ExpiryDate>01/01/2011 11:59:00 PM</ExpiryDate>
          <SellRate>300.000000000</SellRate>
          <RefCurrency>
            <Code>HUF</Code>
          </RefCurrency>
          <GlbCompany>
            <Code>EDI</Code>
          </GlbCompany>
        </RefExchangeRate>
      </CurrencyExchangeRate>
  </Body>
</ReferenceData>";

		#endregion

		public void TestXMLImportFindsObjectWithMultiPartNaturalKey()
		{
			TestCaseHelper.ClearTable(RefExchangeRate.Schema.TableName);

			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_ExRateType = "CUS";
			exchangeRate.RE_StartDate = new CargoWise.Types.ZDateTime(2011, 1, 1, 0, 0, 0);
			exchangeRate.RE_ExpiryDate = new CargoWise.Types.ZDateTime(2011, 1, 1, 23, 59, 0);
			exchangeRate.RE_SellRate = 300.000000000;
			exchangeRate.RE_RX_NKExCurrency = "HUF";
			exchangeRate.Company.GC_Code = "EDI";

			Factory.Save();

			using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(XMLImportRefExchangeRate)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: CurrencyExchangeRate
--- Import Process Finished -----------------------------------------------------------
RefExchangeRate - 0 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("RefExchangeRate should already exist", expectedLog, manager.GetLogs());
			}
		}

		#region XMLImportRefAirline

		const string XMLImportRefAirline = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>GLOFORSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Airline>
      <RefAirline Action=""MERGE"">
        <PK>{0}</PK>
        <AirlineName1>Qantas Airways Limited</AirlineName1>
        <AccountingCode>081</AccountingCode>
        <ThreeLetterCode>QFA</ThreeLetterCode>
        <TwoCharacterCode>QF</TwoCharacterCode>
        <DuplicateFlagIndicator>false</DuplicateFlagIndicator>
        <AddressLine1>ABN 16 009 661 901</AddressLine1>
        <AddressLine2>Qantas Centre, 203 Coward Street</AddressLine2>
        <AirlineCity>Mascot</AirlineCity>
        <AirlineState>New South Wales</AirlineState>
        <AirlineCountry>Australia</AirlineCountry>
        <AirlinePostalCode>2020</AirlinePostalCode>
        <ReservationsDeptTeletype>HDQRBQF</ReservationsDeptTeletype>
        <ReservationsContactName>P. Jennings</ReservationsContactName>
        <ReservationsContactTitle>Mgr. Sales Auto.</ReservationsContactTitle>
        <ReservationsContactTeletype>SYDRTQF</ReservationsContactTeletype>
        <EmergencyTeletype>SYDRXQF</EmergencyTeletype>
        <EmergencyContactTitle>Duty Supervisor</EmergencyContactTitle>
        <MembershipFlagSITA>true</MembershipFlagSITA>
        <MembershipFlagARINC>true</MembershipFlagARINC>
        <MembershipFlagIATA>true</MembershipFlagIATA>
        <MembershipFlagATA>false</MembershipFlagATA>
        <TypeOfOperationsCode>I</TypeOfOperationsCode>
        <AirlinePrefix>081</AirlinePrefix>
        <EagleAddedAirlinePrefixOrAccountingCode>081</EagleAddedAirlinePrefixOrAccountingCode>
        <IsCASSControlled>false</IsCASSControlled>
      </RefAirline>
    </Airline>
  </Body>
</ReferenceData>";

		#endregion

		public void TestXMLImportWithNoNaturalKeysUpdatesViaPK()
		{
			TestCaseHelper.ClearTable(RefAirline.Schema.TableName);

			var airline = Factory.New<RefAirline>();

			airline.RM_AirlineName1 = "Qantas Airways Limited";
			airline.RM_AccountingCode = "081";
			airline.RM_ThreeLetterCode = "QFA";
			airline.RM_TwoCharacterCode = "QF";
			airline.RM_DuplicateFlagIndicator = false;
			airline.RM_AddressLine1 = "ABN 16 009 661 901";
			airline.RM_AddressLine2 = "Qantas Centre, 203 Coward Street";
			airline.RM_AirlineCity = "Mascot";
			airline.RM_AirlineState = "New South Wales";
			airline.RM_AirlineCountry = "Australia";
			airline.RM_AirlinePostalCode = "2020";
			airline.RM_ReservationsDeptTeletype = "HDQRBQF";
			airline.RM_ReservationsContactName = "P. Jennings";
			airline.RM_ReservationsContactTitle = "Mgr. Sales Auto.";
			airline.RM_ReservationsContactTeletype = "SYDRTQF";
			airline.RM_EmergencyTeletype = "SYDRXQF";
			airline.RM_EmergencyContactTitle = "Duty Supervisor";
			airline.RM_MembershipFlagSITA = true;
			airline.RM_MembershipFlagARINC = true;
			airline.RM_MembershipFlagIATA = true;
			airline.RM_MembershipFlagATA = false;
			airline.RM_TypeOfOperationsCode = "I";
			airline.RM_AirlinePrefix = "081";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "081";
			airline.RM_IsCASSControlled = true;

			Factory.Save();

			using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(string.Format(XMLImportRefAirline, airline.PK.ToString()))))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Airline
--- Import Process Finished -----------------------------------------------------------
RefAirline - 0 inserts, 1 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}
		}

		public void TestImportUpdateViaPKUpdatesNaturalKey()
		{
			TestUtil.AddDummyBizoToGlobalDefinitions();
			TestUtil.Connection.ExecuteNonQuery(@"Create UNIQUE INDEX NR_UX__Z0_Number ON DummyBizo(Z0_Number)");

			var loader = new DefinitionAssemblyLoader(new[]
			{
				typeof(NaturalKeyMatchingTest).Assembly.FullName,
				typeof(DefinitionAssemblyLoader).Assembly.FullName
			});
			EntitySetDefinitionCache.SetAlternateDefinitionLoaderForTesting(loader);

			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Number = 1;

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			string dummyImport;
			using (var dataStream = xmlSerializer.SerializeToStream(dummyBO))
			using (var reader = new StreamReader(dataStream))
			{
				dummyImport = reader.ReadToEnd().Replace("<Number>1</Number>", "<Number>2</Number>");
			}

			using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(dummyImport)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Dummy
--- Import Process Finished -----------------------------------------------------------
DummyBizo - 0 inserts, 1 updates, 0 deletes
			".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}

			var factory = new BusinessObjectFactory();
			var reloadedBO = factory.Load<DummyBusinessObject>(dummyBO.PK);

			AssertEquals("reloadedBO.Z0_Number", 2, reloadedBO.Z0_Number);
		}
	}
}
