using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	internal class OrgHeaderTariffLevelsTest : TestCaseWithFactory
	{
		#region OrgHeaderXML

		const string XML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUSSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>RANDOMSYD</Code>
        <FullName>RANDOMNAME</FullName>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
        </ClosestPort>
        <OrgRateTariffLevelCollection>{0}
        </OrgRateTariffLevelCollection>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		string CreateOrgTariffLevelXML(string type, string direction, string mode, ZDate startDate, ZDate expiryDate, string company = "EDI")
		{
			return string.Format(@"
          <OrgRateTariffLevel Action=""MERGE"">
            <TariffType>{0}</TariffType>
            <TariffLevel>1</TariffLevel>
            <Mode>{1}</Mode>
            <Direction>{2}</Direction>
            <ApplyGroupRate>true</ApplyGroupRate>
            <ExpiryDate>{4}</ExpiryDate>
            <StartDate>{3}</StartDate>
            <GlbCompany>
              <Code>{5}</Code>
            </GlbCompany>
          </OrgRateTariffLevel>", type, direction, mode, startDate.ToString("yyyy-MM-ddTHH:mm:ss"), expiryDate.ToString("yyyy-MM-ddTHH:mm:ss"), company);
		}

		#endregion

		#region No Existing Levels

		public void TestMergeOrgRateTariffLevels_NoExistingLevels()
		{
			var orgHeader = new BusinessObjectFactory().New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			orgHeader.Factory.Save();

			var xmlLevel1 = CreateOrgTariffLevelXML("DEF", "ALL", "ALL", ZDate.Today, ZDate.Today.AddDays(1));
			var xmlLevel2 = CreateOrgTariffLevelXML("FRT", "ALL", "ALL", ZDate.Today.AddDays(1), ZDate.Empty);

			var xml = string.Format(XML, xmlLevel1 + xmlLevel2);
			string actualLog = xml.ImportNativeXmlReturningLog();

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 2 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
		}

		public void TestMergeOrgRateTariffLevels_NoExistingLevels_ConflictsInIncoming()
		{
			var orgHeader = new BusinessObjectFactory().New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			orgHeader.Factory.Save();

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Record: Organization failed to Import:
Incoming OrgRateTariffLevel date range conflicts with existing OrgRateTariffLevel.
Existing OrgRateTariffLevel date range: 04-Sep-24 00:00:00 - 05-Sep-24 00:00:00.
Incoming OrgRateTariffLevel date range {0} - {1}
TariffType[DEF]
Mode[ALL]
Direction[ALL]
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.
			".Trim();

			TestCase(new ZDate(2024, 9, 3), new ZDate(2024, 9, 5));
			TestCase(new ZDate(2024, 9, 4), new ZDate(2024, 9, 6));
			TestCase(new ZDate(2024, 9, 4), ZDate.Empty);

			void TestCase(ZDate startDate, ZDate expiryDate)
			{
				var xmlLevel1 = CreateOrgTariffLevelXML("DEF", "ALL", "ALL", new ZDate(2024, 9, 4), new ZDate(2024, 9, 5));
				var xmlLevel2 = CreateOrgTariffLevelXML("DEF", "ALL", "ALL", startDate, expiryDate);

				var startDateString = startDate.IsValid ? startDate.ToZDateTime().ToString() : "Empty date";
				var expiryDateString = expiryDate.IsValid ? expiryDate.ToZDateTime().ToString() : "Empty date";

				var expectedLogWithTime = string.Format(expectedLog, startDateString, expiryDateString);

				var xml = string.Format(XML, xmlLevel1 + xmlLevel2);
				string actualLog = xml.ImportNativeXmlReturningLog();
				AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLogWithTime, actualLog);
			}
		}

		public void TestMergeOrgRateTariffLevels_NoExistingLevels_ConflictsInIncoming_DifferentCompanies()
		{
			var orgHeader = new BusinessObjectFactory().New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			orgHeader.Factory.Save();

			var xmlLevel1 = CreateOrgTariffLevelXML("DEF", "ALL", "ALL", new ZDate(2024, 9, 4), ZDate.Empty);
			var xmlLevel2 = CreateOrgTariffLevelXML("DEF", "ALL", "ALL", new ZDate(2024, 9, 4), ZDate.Empty, "DEM");

			var xml = string.Format(XML, xmlLevel1 + xmlLevel2);
			string actualLog = xml.ImportNativeXmlReturningLog();

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 2 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
		}

		public void TestMergeOrgRateTariffLevels_NoExistingLevels_IncomingLevelMissingStartDate()
		{
			var orgHeader = new BusinessObjectFactory().New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			orgHeader.Factory.Save();

			var xmlLevel = CreateOrgTariffLevelXML("DEF", "ALL", "ALL", ZDate.Empty, new ZDate(2024, 9, 5));

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes
			".Trim();

			var xml = string.Format(XML, xmlLevel);
			string actualLog = xml.ImportNativeXmlReturningLog();

			AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
		}

		public void TestMergeOrgRateTariffLevels_NoExistingLevels_OverlappingIncomingLevels()
		{
			var orgHeader = new BusinessObjectFactory().New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			orgHeader.Factory.Save();

			var xmlLevel1 = CreateOrgTariffLevelXML("DEF", "ALL", "ALL", new ZDate(2024, 9, 4), new ZDate(2024, 9, 6));
			var xmlLevel2 = CreateOrgTariffLevelXML("DEF", "ALL", "ALL", new ZDate(2024, 9, 6), ZDate.Empty);

			var xml = string.Format(XML, xmlLevel1 + xmlLevel2);
			string actualLog = xml.ImportNativeXmlReturningLog();

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing OrgRateTariffLevel was expired by incoming OrgRateTariffLevel
TariffType[DEF]
Mode[ALL]
Direction[ALL]
ExpiryDate changed from value 6/09/2024 12:00:00 AM to value 5/09/2024 12:00:00 AM


Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 2 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
		}

		public void TestMergeOrgRateTariffLevels_NoExistingLevels_MissingColumns()
		{
			var orgHeader = new BusinessObjectFactory().New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			orgHeader.Factory.Save();

			var xml = string.Format(XML, @"
          <OrgRateTariffLevel Action=""MERGE"">
            <TariffType>DEF</TariffType>
            <TariffLevel>1</TariffLevel>
            <Mode>ALL</Mode>
            <Direction>ALL</Direction>
            <ApplyGroupRate>true</ApplyGroupRate>
            <GlbCompany>
              <Code>EDI</Code>
            </GlbCompany>
          </OrgRateTariffLevel>");
			string actualLog = xml.ImportNativeXmlReturningLog();

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes
			".Trim();

			var factory = NewFactory();
			var organisation = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "RANDOMSYD"));
			var level = (OrgRateTariffLevel)organisation.CompanyData.RateTariffLevels.FirstOrDefault();

			CombineAssertions(delegate
			{
				AssertEquals("Should use empty start date", ZDate.Empty, level.P7_StartDate);
				AssertEquals("Should use empty expiry date", ZDate.Empty, level.P7_ExpiryDate);
				AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
			});
		}

		#endregion

		#region Existing Levels

		public void TestMergeOrgRateTariffLevels_ExistingLevels()
		{
			var orgHeader = new BusinessObjectFactory().New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			orgHeader.CompanyData.RateTariffLevels.AddLevel("DEF", "ALL", "ALL", new ZDate(2024, 9, 1), new ZDate(2024, 9, 2), 1);
			orgHeader.Factory.Save();

			var xmlLevel = CreateOrgTariffLevelXML("DEF", "ALL", "ALL", new ZDate(2024, 9, 4), new ZDate(2024, 9, 6));

			var xml = string.Format(XML, xmlLevel);
			string actualLog = xml.ImportNativeXmlReturningLog();

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes
			".Trim();

			var factory = NewFactory();
			var organisation = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "RANDOMSYD"));

			CombineAssertions(delegate
			{
				AssertEquals("Should have 2 tariff levels", 2, organisation.CompanyData.RateTariffLevels.Count);
				AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
			});
		}

		public void TestMergeOrgRateTariffLevels_ExistingLevels_OverlappingWithIncoming_ExpiryDate()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			var level = orgHeader.CompanyData.RateTariffLevels.AddLevel("DEF", "ALL", "ALL", new ZDate(2024, 9, 2), new ZDate(2024, 9, 3), 1);
			Factory.Save();

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing OrgRateTariffLevel was expired by incoming OrgRateTariffLevel
TariffType[DEF]
Mode[ALL]
Direction[ALL]
ExpiryDate changed from value 3/09/2024 12:00:00 AM to value 2/09/2024 12:00:00 AM


Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes
			".Trim();

			var xml = string.Format(XML, CreateOrgTariffLevelXML("DEF", "ALL", "ALL", new ZDate(2024, 9, 3), new ZDate(2024, 9, 5)));
			string actualLog = xml.ImportNativeXmlReturningLog();

			var factory = NewFactory();
			var organisation = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "RANDOMSYD"));
			var newLevel = organisation.CompanyData.RateTariffLevels.Where(x => x.P7_ExpiryDate == new ZDate(2024, 9, 2));

			CombineAssertions(delegate
			{
				AssertEquals("Should find one level with new date", 1, newLevel.Count());
				AssertEquals("Should have correct number of levels", 2, organisation.CompanyData.RateTariffLevels.Count);
				AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
			});
		}

		public void TestMergeOrgRateTariffLevels_ExistingLevels_OverlappingWithIncoming_NoExpiryDate()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			var level = orgHeader.CompanyData.RateTariffLevels.AddLevel("DEF", "ALL", "ALL", new ZDate(2024, 9, 2), ZDate.Empty, 1);
			Factory.Save();

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing OrgRateTariffLevel was expired by incoming OrgRateTariffLevel
TariffType[DEF]
Mode[ALL]
Direction[ALL]
ExpiryDate changed from value Empty date to value 2/09/2024 12:00:00 AM


Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes
			".Trim();

			var xml = string.Format(XML, CreateOrgTariffLevelXML("DEF", "ALL", "ALL", new ZDate(2024, 9, 3), new ZDate(2024, 9, 5)));
			string actualLog = xml.ImportNativeXmlReturningLog();

			var factory = NewFactory();
			var organisation = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "RANDOMSYD"));
			var newLevel = organisation.CompanyData.RateTariffLevels.Where(x => x.P7_ExpiryDate == new ZDate(2024, 9, 2));

			CombineAssertions(delegate
			{
				AssertEquals("Should find one level with new date", 1, newLevel.Count());
				AssertEquals("Should have correct number of levels", 2, organisation.CompanyData.RateTariffLevels.Count);
				AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
			});
		}

		public void TestMergeOrgRateTariffLevels_ExistingLevels_OverlappingWithIncoming_StartDate()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			var level = orgHeader.CompanyData.RateTariffLevels.AddLevel("DEF", "ALL", "ALL", new ZDate(2024, 9, 5), new ZDate(2024, 9, 7), 1);
			Factory.Save();

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing OrgRateTariffLevel was expired by incoming OrgRateTariffLevel
TariffType[DEF]
Mode[ALL]
Direction[ALL]
StartDate changed from value 5/09/2024 12:00:00 AM to value 6/09/2024 12:00:00 AM


Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes
			".Trim();

			var xml = string.Format(XML, CreateOrgTariffLevelXML("DEF", "ALL", "ALL", new ZDate(2024, 9, 3), new ZDate(2024, 9, 5)));
			string actualLog = xml.ImportNativeXmlReturningLog();

			var factory = NewFactory();
			var organisation = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "RANDOMSYD"));
			var newLevel = organisation.CompanyData.RateTariffLevels.Where(x => x.P7_StartDate == new ZDate(2024, 9, 6));

			CombineAssertions(delegate
			{
				AssertEquals("Should find one level with new date", 1, newLevel.Count());
				AssertEquals("Should have correct number of levels", 2, organisation.CompanyData.RateTariffLevels.Count);
				AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
			});
		}

		public void TestMergeOrgRateTariffLevels_ExistingLevels_OverlappingWithIncoming_NoStartDate()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			var level = orgHeader.CompanyData.RateTariffLevels.AddLevel("DEF", "ALL", "ALL", ZDate.Empty, new ZDate(2024, 9, 7), 1);
			Factory.Save();

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing OrgRateTariffLevel was expired by incoming OrgRateTariffLevel
TariffType[DEF]
Mode[ALL]
Direction[ALL]
ExpiryDate changed from value 7/09/2024 12:00:00 AM to value 2/09/2024 12:00:00 AM


Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes
			".Trim();

			var xml = string.Format(XML, CreateOrgTariffLevelXML("DEF", "ALL", "ALL", new ZDate(2024, 9, 3), new ZDate(2024, 9, 5)));
			string actualLog = xml.ImportNativeXmlReturningLog();

			var factory = NewFactory();
			var organisation = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "RANDOMSYD"));

			CombineAssertions(delegate
			{
				AssertEquals("Should have correct number of levels", 2, organisation.CompanyData.RateTariffLevels.Count);
				AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
			});
		}

		public void TestMergeOrgRateTariffLevels_ExistingLevels_XMLSharesPKWithExistingLevel()
		{
			var orgHeader = new BusinessObjectFactory().New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			var level = orgHeader.CompanyData.RateTariffLevels.AddLevel("DEF", "ALL", "ALL", new ZDate(2024, 9, 1), new ZDate(2024, 9, 2), 1);
			orgHeader.Factory.Save();

			var xmlLevel = string.Format(@"
          <OrgRateTariffLevel Action=""MERGE"">
            <PK>{0}</PK>
            <TariffType>DEF</TariffType>
            <TariffLevel>1</TariffLevel>
            <Mode>ALL</Mode>
            <Direction>ALL</Direction>
            <ApplyGroupRate>false</ApplyGroupRate>
            <ExpiryDate></ExpiryDate>
            <StartDate>2024-08-3T00:00:00</StartDate>
            <GlbCompany>
              <Code>EDI</Code>
            </GlbCompany>
          </OrgRateTariffLevel>", level.PK);

			var xml = string.Format(XML, xmlLevel);
			string actualLog = xml.ImportNativeXmlReturningLog();

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 0 inserts, 1 updates, 0 deletes
			".Trim();

			var factory = NewFactory();
			var organisation = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "RANDOMSYD"));
			var newLevel = (OrgRateTariffLevel)organisation.CompanyData.RateTariffLevels.FirstOrDefault();

			CombineAssertions(delegate
			{
				AssertEquals("Should use existing level start date", new ZDate(2024, 8, 3), newLevel.P7_StartDate);
				AssertEquals("Should use existing level expiry date", ZDate.Empty, newLevel.P7_ExpiryDate);
				AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
			});
		}

		public void TestMergeOrgRateTariffLevels_ExistingLevels_XMLSharesPKWithExistingLevel_StartDateAfterExpiry()
		{
			var orgHeader = new BusinessObjectFactory().New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			var level = orgHeader.CompanyData.RateTariffLevels.AddLevel("DEF", "ALL", "ALL", new ZDate(2024, 9, 1), new ZDate(2024, 9, 2), 1);
			orgHeader.Factory.Save();

			var xmlLevel = string.Format(@"
          <OrgRateTariffLevel Action=""MERGE"">
            <PK>{0}</PK>
            <TariffType>DEF</TariffType>
            <TariffLevel>1</TariffLevel>
            <Mode>ALL</Mode>
            <Direction>ALL</Direction>
            <ApplyGroupRate>false</ApplyGroupRate>
            <StartDate>2024-09-3T00:00:00</StartDate>
            <GlbCompany>
              <Code>EDI</Code>
            </GlbCompany>
          </OrgRateTariffLevel>", level.PK);

			var xml = string.Format(XML, xmlLevel);
			string actualLog = xml.ImportNativeXmlReturningLog();

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Record: Organization failed to Import:
Cannot import to table 'OrgRateTariffLevel' because constraint 'Constraint_ExpiryDate' failed - ([ExpiryDate]>=[StartDate]).
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.
			".Trim();

			var factory = NewFactory();
			var organisation = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "RANDOMSYD"));
			var newLevel = (OrgRateTariffLevel)organisation.CompanyData.RateTariffLevels.FirstOrDefault();

			CombineAssertions(delegate
			{
				AssertEquals("Should use existing level start date", level.P7_StartDate, newLevel.P7_StartDate);
				AssertEquals("Should use existing level expiry date", level.P7_ExpiryDate, newLevel.P7_ExpiryDate);
				AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
			});
		}

		public void TestMergeOrgRateTariffLevels_ExistingLevels_MissingColumns_SharesPKWithExistingLevel()
		{
			var orgHeader = new BusinessObjectFactory().New<OrgHeader>();
			orgHeader.OH_FullName = "RANDOMNAME";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "RANDOMSYD";
			var level = orgHeader.CompanyData.RateTariffLevels.AddLevel("DEF", "ALL", "ALL", new ZDate(2024, 9, 1), new ZDate(2024, 9, 2), 1);
			orgHeader.Factory.Save();

			var xmlLevel = string.Format(@"
          <OrgRateTariffLevel Action=""MERGE"">
            <PK>{0}</PK>
            <TariffType>DEF</TariffType>
            <TariffLevel>1</TariffLevel>
            <Mode>ALL</Mode>
            <Direction>ALL</Direction>
            <ApplyGroupRate>false</ApplyGroupRate>
            <GlbCompany>
              <Code>EDI</Code>
            </GlbCompany>
          </OrgRateTariffLevel>", level.PK);

			var xml = string.Format(XML, xmlLevel);
			string actualLog = xml.ImportNativeXmlReturningLog();

			string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 0 inserts, 0 updates, 0 deletes
			".Trim();

			var factory = NewFactory();
			var organisation = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "RANDOMSYD"));
			var newLevel = (OrgRateTariffLevel)organisation.CompanyData.RateTariffLevels.FirstOrDefault();

			CombineAssertions(delegate
			{
				AssertEquals("Should use existing level start date", level.P7_StartDate, newLevel.P7_StartDate);
				AssertEquals("Should use existing level expiry date", level.P7_ExpiryDate, newLevel.P7_ExpiryDate);
				AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
			});
		}

		#endregion
	}
}
