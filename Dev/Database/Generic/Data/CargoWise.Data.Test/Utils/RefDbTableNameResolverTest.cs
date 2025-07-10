using System;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class RefDbTableNameResolverTest : TestCase
	{
		public void TestGetRefDbTableSynonym()
		{
			AssertEquals(
				RefDbTableNameResolver.RefDbAffix + "EntUS_USCTable",
				RefDbTableNameResolver.GetRefDbTableSynonym(RefDbTypeEnum.Enterprise, "US", "USCTable"));
			AssertEquals(
				RefDbTableNameResolver.RefDbAffix + "TrfNZ_NZTable",
				RefDbTableNameResolver.GetRefDbTableSynonym(RefDbTypeEnum.Tariff, "NZ", "NZTable"));
			AssertEquals(
				RefDbTableNameResolver.SingleRefDatabaseNameSynonymPrefix + "_SingleTable",
				RefDbTableNameResolver.GetRefDbTableSynonym(RefDbTypeEnum.Single, "", "SingleTable"));
		}

		public void TestGetRefDbSchemaClassFolder()
		{
			AssertEquals(
				"RefDb_US_Enterprise",
				RefDbTableNameResolver.GetRefDbSchemaClassFolder(RefDbTypeEnum.Enterprise, "US"));
			AssertEquals(
				"RefDb_NZ_Tariff",
				RefDbTableNameResolver.GetRefDbSchemaClassFolder(RefDbTypeEnum.Tariff, "NZ"));
			AssertEquals(
				"RefDatabase",
				RefDbTableNameResolver.GetRefDbSchemaClassFolder(RefDbTypeEnum.Single, ""));
		}

		public void TestGetRefDbSynonymPrefix()
		{
			AssertEquals(
				RefDbTableNameResolver.RefDbAffix + "EntBR_",
				RefDbTableNameResolver.GetRefDbSynonymPrefix(RefDbTypeEnum.Enterprise, "BR"));
			AssertEquals(
				RefDbTableNameResolver.RefDbAffix + "TrfIT_",
				RefDbTableNameResolver.GetRefDbSynonymPrefix(RefDbTypeEnum.Tariff, "IT"));
			AssertEquals(
				RefDbTableNameResolver.RefDbAffix + "CmrJP_",
				RefDbTableNameResolver.GetRefDbSynonymPrefix(RefDbTypeEnum.Customs, "JP"));
			AssertExceptionThrown<NotSupportedException>(
				"GetRefDbSynonymPrefix() does not support RefDbTypeEnum.Single",
				() => RefDbTableNameResolver.GetRefDbSynonymPrefix(RefDbTypeEnum.Single, "BR"));
		}

		public void TestGetDbType3LetterCode()
		{
			AssertEquals("Ent", RefDbTableNameResolver.GetDbType3LetterCode(RefDbTypeEnum.Enterprise));
			AssertEquals("Trf", RefDbTableNameResolver.GetDbType3LetterCode(RefDbTypeEnum.Tariff));
			AssertEquals("Cmr", RefDbTableNameResolver.GetDbType3LetterCode(RefDbTypeEnum.Customs));
			AssertExceptionThrown<NotSupportedException>("GetDbType3LetterCode() does not support RefDbTypeEnum.Single", () => RefDbTableNameResolver.GetDbType3LetterCode(RefDbTypeEnum.Single));
		}

		public void TestIsSharedDatabase()
		{
			AssertEquals("Is CW-RefDb-123 a shared database?", true, RefDbTableNameResolver.IsSharedDatabase("CW-RefDb-123"));
			AssertEquals("Is CW-RefDb-Abcde a shared database?", true, RefDbTableNameResolver.IsSharedDatabase("CW-RefDb-Abcde"));
			AssertEquals("Is CW-AG-RefDb-123 a shared database?", true, RefDbTableNameResolver.IsSharedDatabase("CW-AG-RefDb-123"));
			AssertEquals("Is CW-AG-RefDb-Abcde a shared database?", true, RefDbTableNameResolver.IsSharedDatabase("CW-AG-RefDb-Abcde"));
			AssertEquals("Is CW-RefDatabase a shared database?", true, RefDbTableNameResolver.IsSharedDatabase("CW-RefDatabase"));
			AssertEquals("Is AnyOtherDbName a shared database?", false, RefDbTableNameResolver.IsSharedDatabase("AnyOtherDbName"));
			AssertEquals("Is NULL a shared database?", false, RefDbTableNameResolver.IsSharedDatabase(null));
		}

		public void TestIsExclusiveDatabase()
		{
			AssertEquals("Is NULL a exclusive database?", false, RefDbTableNameResolver.IsExclusiveDatabase(null, "MainDb_RefDb_Ent_CA"));
			AssertEquals("Is NULL a exclusive database?", false, RefDbTableNameResolver.IsExclusiveDatabase("MainDb", null));
			AssertEquals("Is MainDb_RefDb_Ent_CA a exclusive database?", false, RefDbTableNameResolver.IsExclusiveDatabase("AAA", "MainDb_RefDb_Ent_CA"));
			AssertEquals("Is MainDb_RefDb_Ent_CA a exclusive database?", true, RefDbTableNameResolver.IsExclusiveDatabase("MainDb", "MainDb_RefDb_Ent_CA"));
			AssertEquals("Is MainDb_RefDb_Abcde a exclusive database?", true, RefDbTableNameResolver.IsExclusiveDatabase("MainDb", "MainDb_RefDb_Abcde"));
			AssertEquals("Is AnyOtherDbName a exclusive database?", false, RefDbTableNameResolver.IsExclusiveDatabase("MainDb", "AnyOtherDbName"));
		}

		public void TestGetExclusiveRefDbName()
		{
			AssertEquals("MainDb_RefDb_Ent_AU", RefDbTableNameResolver.GetExclusiveRefDbName("MainDb", RefDbTypeEnum.Enterprise, "AU"));
			AssertEquals("MainDb_RefDb_Trf_AU", RefDbTableNameResolver.GetExclusiveRefDbName("MainDb", RefDbTypeEnum.Tariff, "AU"));
			AssertEquals("MainDb_RefDb_Cmr_UA", RefDbTableNameResolver.GetExclusiveRefDbName("MainDb", RefDbTypeEnum.Customs, "UA"));
		}

		public void TestGetExclusiveRefDbNameSuffix()
		{
			AssertEquals("_RefDb_Ent_AU", RefDbTableNameResolver.GetExclusiveRefDbNameSuffix(RefDbTypeEnum.Enterprise, "AU"));
			AssertEquals("_RefDb_Trf_AU", RefDbTableNameResolver.GetExclusiveRefDbNameSuffix(RefDbTypeEnum.Tariff, "AU"));
			AssertEquals("_RefDb_Cmr_UA", RefDbTableNameResolver.GetExclusiveRefDbNameSuffix(RefDbTypeEnum.Customs, "UA"));
			AssertExceptionThrown<NotSupportedException>("GetExclusiveRefDbNameSuffix() does not support RefDbTypeEnum.Single", () => RefDbTableNameResolver.GetExclusiveRefDbNameSuffix(RefDbTypeEnum.Single, "AU"));
		}

		public void TestGetSharedAvailabilityGroupRefDbPrefix()
		{
			AssertEquals("CW-AG-RefDb-ORDWP4-CP1AS1-Ent-AU-", RefDbTableNameResolver.GetSharedAvailabilityGroupRefDbPrefix("ORDWP4-CP1AS1", RefDbTypeEnum.Enterprise, "AU"));
			AssertEquals("CW-AG-RefDb-ORDWP4-CP1AS1-Trf-AU-", RefDbTableNameResolver.GetSharedAvailabilityGroupRefDbPrefix("ORDWP4-CP1AS1", RefDbTypeEnum.Tariff, "AU"));
			AssertEquals("CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-UA-", RefDbTableNameResolver.GetSharedAvailabilityGroupRefDbPrefix("ORDWP4-CP1AS1", RefDbTypeEnum.Customs, "UA"));
		}

		public void TestShouldUseSharedDatabases()
		{
			AssertEquals(false, RefDbTableNameResolver.ShouldUseSharedDatabases(Db.Connection));
		}

		public void TestShouldUseSharedAvailabilityGroupDatabases()
		{
			AssertEquals(false, RefDbTableNameResolver.ShouldUseSharedAvailabilityGroupDatabases(Db.Connection));
		}

		[UseSnapshotProtection]
		public void TestGetSelectBizoViewNameQueryFromDbWithVersionedViewName()
		{
			var testDb = "DBForTestAA206DFE-9E53-495F-8157-A57FBBF4DFEC";

			using (var createDbConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(createDbConnection, testDb);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testDb);

					using (var testAdminConnection = Db.NewAdminConnection(Db.ServerName, testDb))
					{
						var sql = @"CREATE TABLE [dbo].[XXX](
	[AT_PK] [uniqueidentifier] NOT NULL,
	[AT_V] [varchar](10) NOT NULL
)";
						testAdminConnection.ExecuteNonQuery(sql);
						sql = @"CREATE VIEW AView_V1 AS
SELECT AT_PK, AT_V FROM XXX";
						testAdminConnection.ExecuteNonQuery(sql);
						sql = @"CREATE VIEW XXXTableView_V1 AS
SELECT AT_PK, AT_V FROM XXX";
						testAdminConnection.ExecuteNonQuery(sql);

						AssertEquals("AView", testAdminConnection.ExecuteScalar(RefDbTableNameResolver.GetSelectBizoViewNameQueryFromDbWithVersionedViewNameList(testDb, "'AView_V1'")).ToString());

						AssertEquals("XXX", testAdminConnection.ExecuteScalar(RefDbTableNameResolver.GetSelectBizoViewNameQueryFromDbWithVersionedViewNameList(testDb, "'XXXTableView_V1'")).ToString());
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(createDbConnection, testDb);
				}
			}
		}
	}
}
