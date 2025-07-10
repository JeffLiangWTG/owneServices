using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.CA.Testing
{
	sealed class UpgradeToVersionPGAHSCodesTest : CAReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
			base.PrepareTestData(conn);

			foreach (var table in refDbUpgrader.Tables)
			{
				foreach (var index in table.CreateIndexScripts)
				{
					conn.ExecuteNonQuery(DbSchemaChange.GetDropIndexIfExistsScript(table.TableName, index.IndexName));
				}
			}
		}

		protected override void AssertUpgradeResult()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Total CFIA tariffs count", 244, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA'"));
				AssertEquals("CFIA tariffs to be expired after 10/01/2019", 13, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateTo = '2019-01-10 23:59:00'"));
				AssertEquals("CFIA tariffs effective from 23/04/1998", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '1998-04-23 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 28/10/1998", 2, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '1998-10-28 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 01/01/2002", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2002-01-01 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 16/06/2003", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2003-06-16 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 03/05/2007", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2007-05-03 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 22/06/2007", 10, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2007-06-22 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 18/12/2007", 3, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2007-12-18 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 15/03/2010", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2010-03-15 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 21/06/2010", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2010-06-21 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 08/11/2010", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2010-11-08 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 06/04/2011", 7, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2011-04-06 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 25/05/2011", 6, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2011-05-25 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 01/01/2012", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2012-01-01 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 05/01/2012", 21, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2012-01-05 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 18/01/2012", 4, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2012-01-18 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 27/02/2012", 19, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2012-02-27 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 25/06/2012", 17, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2012-06-25 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 06/11/2012", 23, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2012-11-06 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 03/06/2013", 6, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2013-06-03 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 09/10/2014", 5, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2014-10-09 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 09/12/2015", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2015-12-09 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 01/01/2017", 47, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2017-01-01 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 27/01/2017", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2017-01-27 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 04/04/2017", 2, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2017-04-04 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 28/08/2017", 3, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2017-08-28 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 07/05/2018", 30, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2018-05-07 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 11/01/2019", 7, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2019-01-11 00:00:00'"));
				AssertEquals("CFIA tariffs effective from 01/04/2019", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateFrom = '2019-04-01 00:00:00'"));
				AssertEquals("CFIA tariffs to be expired after 31/03/2019", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'CFIA' AND HC_EffectiveDateTo = '2019-03-31 23:59:00'"));
				AssertEquals("NRCan EFF tariffs effective from 23/01/2019", 11, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'NRCAN' AND HC_ProgramInd = 'EEF' AND HC_EffectiveDateFrom = '2019-01-23 00:00:00'"));
				AssertEquals("NRCan EFF tariffs effective from 30/04/2019", 18, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'NRCAN' AND HC_ProgramInd = 'EEF' AND HC_EffectiveDateFrom = '2019-04-30 00:00:00'"));
				AssertEquals("NRCan EFF tariffs effective from 2023-08-29", 10, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'NRCAN' AND HC_ProgramInd = 'EEF' AND HC_EffectiveDateFrom = '08-29-2023 00:00:00'"));
				AssertEquals("NRCan EFF tariffs effective to 08-28-2023 23:59:00", 22, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'NRCAN' AND HC_ProgramInd = 'EEF' AND HC_EffectiveDateTo = '08-28-2023 23:59:00'"));
				AssertEquals("NRCan EFF tariffs to be expired after 30/04/2019", 2, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'NRCAN' AND HC_EffectiveDateTo = '2019-04-29 23:59:00'"));
				AssertEquals("NRCan EFF tariffs count", 205, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'NRCAN' AND HC_ProgramInd = 'EEF'"));
				AssertEquals("TC VPR tariffs effective from 06/05/2019", 3, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'TC' AND HC_ProgramInd = 'VPR' AND HC_EffectiveDateFrom = '2019-05-06 12:00:00'"));
				AssertEquals("TC VPR tariffs effective from 17/09/2019", 2, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'TC' AND HC_ProgramInd = 'VPR' AND HC_EffectiveDateFrom = '2019-09-17 12:00:00'"));
				AssertEquals("TC VPR tariffs count", 41, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'TC' AND HC_ProgramInd = 'VPR' AND HC_EffectiveDateTo = '2079-06-06 23:59'"));
				AssertEquals("TC VPR tariffs count", 188, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'TC' AND HC_ProgramInd = 'VPR' AND HC_EffectiveDateTo = '2079-06-06 12:00:00'"));
				AssertEquals("HC HDR tariffs effective from 06/05/2019", 5, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'HDR' AND HC_EffectiveDateFrom = '2019-05-06 12:00:00'"));
				AssertEquals("HC tariffs to be expired after 06/05/2019", 2, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_EffectiveDateTo = '2019-05-05 23:59:00'"));
				AssertEquals("TC tariffs to be expired after 03/03/2019", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'TC' AND HC_EffectiveDateTo = '2019-03-03 12:00:00'"));
				AssertEquals("Total HC tariffs count", 6247, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC'"));
				AssertEquals("HC tariffs count of API", 636, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'API'"));
				AssertEquals("HC tariffs count of BBC", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'BBC'"));
				AssertEquals("HC tariffs count of CTO", 2, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'CTO'"));
				AssertEquals("HC tariffs count of CPR", 5268, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'CPR'"));
				AssertEquals("HC tariffs count of DSE", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'DSE'"));
				AssertEquals("HC tariffs count of HDR", 22, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'HDR'"));
				AssertEquals("HC tariffs count of OCS", 108, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'OCS'"));
				AssertEquals("HC tariffs count of MDE", 153, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'MDE'"));
				AssertEquals("HC tariffs count of NHP", 26, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'NHP'"));
				AssertEquals("HC tariffs count of PES", 7, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'PES'"));
				AssertEquals("HC tariffs count of RED", 14, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'RED'"));
				AssertEquals("HC tariffs count of VET", 9, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'VET'"));
				AssertEquals("ECCC tariffs count of WEN", 434, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACPGAHSCode WHERE HC_Type = 'ECCC' AND HC_ProgramInd = 'WEN'"));
				AssertEquals("Document Types Global Codes of GAC", 9, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACDocumentTypes WHERE FR_GovAgencyIDCode = 'GAC'"));
				AssertEquals("CFIA Reg Types of SubType W", 3, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACFIARegTypes WHERE FR_RegSubType = 'W'"));
				AssertEquals("HC tariffs count of MDE", "3002150000", UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT TOP 1 HC_FROMHS FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'MDE' AND HC_EffectiveDateFrom = '2020-03-12 12:00:00'"));
				AssertEquals("HC tariffs Expired of CPR", 19, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT Count(1) FROM CACPGAHSCode WHERE HC_Type = 'HC' AND HC_ProgramInd = 'CPR' AND HC_EffectiveDateTo = '2022-12-31 23:59:00'"));
			});
			foreach (var table in refDbUpgrader.Tables)
			{
				foreach (var index in table.CreateIndexScripts)
				{
					Assert(index.IndexName + " exists", DbObjectCreator.IndexExists(testConnection, table.TableName, index.IndexName));
				}
			}
			Assert("CACCarrier dropped", !DbObjectCreator.TableExists(testConnection, "CACCarrier"));
		}

		protected override int LatestVersionNumber
		{
			get { return 171; }
		}
	}
}
