using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DataTools.DbBackupAndRestore.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class GlowThemeToPreserveTest : TestCase
	{
		const string TargetDbName = "TestDb-GlowThemeToPreserve";
		const string VaultDbName = TargetDbName + "-DataVault";
		const string ItemTableName = "BPMConfigurationItem";
		const string TmplTableName = "BPMConfigurationTmpl";

		[UseSnapshotProtection]
		public void TestPreserveGlowThemeItem_WhenPreserveGlowThemeRegistryIsEnabled()
		{
			var expectedTmpls = new BPMConfigurationTmpl[] {
				new BPMConfigurationTmpl { VCT_PK = Guid.Parse("11111111-1111-1111-2222-111111111111") },
				new BPMConfigurationTmpl { VCT_PK = Guid.Parse("11111111-1111-1111-2222-222222222222") },
				new BPMConfigurationTmpl { VCT_PK = Guid.Parse("11111111-1111-1111-2222-333333333333") },
				new BPMConfigurationTmpl { VCT_PK = Guid.Parse("11111111-1111-1111-2222-555555555555") },
			};

			var expectedItems = new BPMConfigurationItem[] {
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-333333333333"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-111111111111"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> origin3 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "Logo" },
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-888888888888"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-555555555555"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> new2 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "Logo" },
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-111111111111"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-111111111111"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> origin1 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "CoBranding" },
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-222222222222"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-111111111111"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> origin2 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "Theme" },
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-555555555555"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-222222222222"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> origin5 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "CoBranding" }
			};
			TestPreserveGlowThemeItem(true, expectedTmpls, expectedItems);
		}

		[UseSnapshotProtection]
		public void TestPreserveGlowThemeItem_WhenPreserveGlowThemeRegistryIsNotEnabled()
		{
			TestPreserveGlowThemeItem(false, null, null);
		}

		void TestPreserveGlowThemeItem(bool isPreserveGlowTheme, IList<BPMConfigurationTmpl> expectedTmpls, IList<BPMConfigurationItem> expectedItems)
		{
			var initialTmpls = new BPMConfigurationTmpl[] {
				new BPMConfigurationTmpl { VCT_PK = Guid.Parse("11111111-1111-1111-2222-111111111111") },
				new BPMConfigurationTmpl { VCT_PK = Guid.Parse("11111111-1111-1111-2222-222222222222") },
				new BPMConfigurationTmpl { VCT_PK = Guid.Parse("11111111-1111-1111-2222-333333333333") },
				new BPMConfigurationTmpl { VCT_PK = Guid.Parse("11111111-1111-1111-2222-444444444444") },
			};

			var initialItems = new BPMConfigurationItem[] {
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-111111111111"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-111111111111"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> origin1 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "CoBranding" },
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-222222222222"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-111111111111"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> origin2 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "Theme" },
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-333333333333"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-111111111111"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> origin3 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "Logo" },
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-444444444444"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-111111111111"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> origin4 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "" },
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-555555555555"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-222222222222"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> origin5 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "CoBranding" },
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-666666666666"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-222222222222"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> origin5 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "Foo" },
			};

			var resotreTmpls = new BPMConfigurationTmpl[] {
				new BPMConfigurationTmpl { VCT_PK = Guid.Parse("11111111-1111-1111-2222-111111111111") },
				new BPMConfigurationTmpl { VCT_PK = Guid.Parse("11111111-1111-1111-2222-333333333333") },
				new BPMConfigurationTmpl { VCT_PK = Guid.Parse("11111111-1111-1111-2222-555555555555") },
			};

			var restoreItems = new BPMConfigurationItem[] {
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-111111111111"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-111111111111"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> edited1 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "CoBranding" },
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-333333333333"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-111111111111"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> origin3 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "Logo" },
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-777777777777"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-555555555555"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> new1 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "Theme" },
				new BPMConfigurationItem { VCM_PK = Guid.Parse("11111111-1111-1111-1111-888888888888"), VCM_VCT_Template = Guid.Parse("11111111-1111-1111-2222-555555555555"), VCM_ConfigurationData = "<ArrayOfConfigurableKeyValuePair> new2 </ArrayOfConfigurableKeyValuePair>", VCM_ConfigurationKey = "Logo" },
			};

			if (!isPreserveGlowTheme)
			{
				expectedTmpls = resotreTmpls;
				expectedItems = restoreItems;
			}

			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				try
				{
					CreateTablesAndInsertData(connection, initialTmpls, initialItems);
					if (isPreserveGlowTheme)
					{
						InsertRegistry(connection, "PreserveGlowTheme", "True");
					}

					AdoTestUtils.CreateDbDropExisting(connection, VaultDbName);

					var registryItemsToPreserve = new RegistryItemsToPreserve();
					var popluateScript = registryItemsToPreserve.GetPopulateTemporaryDataScript(VaultDbName, TargetDbName);
					var glowThemeTmplToPreserve = new GlowThemeTmplToPreserve();
					popluateScript += glowThemeTmplToPreserve.GetPopulateTemporaryDataScript(VaultDbName, TargetDbName);
					var glowThemeItemToPreserve = new GlowThemeItemToPreserve();
					popluateScript += glowThemeItemToPreserve.GetPopulateTemporaryDataScript(VaultDbName, TargetDbName);
					connection.ExecuteNonQuery(popluateScript);

					Restore(connection, resotreTmpls, restoreItems);

					AssertRecords(connection, restoreItems, resotreTmpls);

					var clearScript = glowThemeItemToPreserve.GetClearDataToBeOverwrittenByTestDataScript(VaultDbName, TargetDbName);
					clearScript += glowThemeTmplToPreserve.GetClearDataToBeOverwrittenByTestDataScript(VaultDbName, TargetDbName);
					connection.ExecuteNonQuery(clearScript);

					var copyScript = glowThemeTmplToPreserve.GetCopyTempDbDataToTestDbScript(VaultDbName, TargetDbName);
					copyScript += glowThemeItemToPreserve.GetCopyTempDbDataToTestDbScript(VaultDbName, TargetDbName);
					connection.ExecuteNonQuery(copyScript);

					AssertRecords(connection, expectedItems, expectedTmpls);
					AssertForeignKeyIsEnabled(connection);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(connection, TargetDbName);
					AdoTestUtils.DropDbIfExists(connection, VaultDbName);
				}
			}
		}

		static void CreateTablesAndInsertData(DbConnection connection, IList<BPMConfigurationTmpl> tmpls, IList<BPMConfigurationItem> items)
		{
			CreateTablesDropExisting(connection);

			InsertTmpls(connection, tmpls);
			InsertItems(connection, items);
		}

		static void CreateTablesDropExisting(DbConnection connection)
		{
			AdoTestUtils.CreateDbDropExisting(TargetDbName);

			var schemaHelper = new SchemaHelper(connection, TargetDbName);
			schemaHelper.CreateTable(TmplTableName);
			schemaHelper.CreateTable(ItemTableName);
			schemaHelper.CreateTable("StmData");

			CreatePrimaryKeyAndForeignKey();
		}

		static void CreatePrimaryKeyAndForeignKey()
		{
			using (var connection = Db.NewAdminConnection(TargetDbName))
			{
				connection.ExecuteNonQuery(@"
-- Unique Key for BPMConfigurationTmpl
ALTER TABLE  [BPMConfigurationTmpl]
ADD CONSTRAINT [PK_UX__VCT_PK] PRIMARY KEY NONCLUSTERED  ([VCT_PK] ASC)

-- Unique Key for BPMConfigurationItem
ALTER TABLE  [BPMConfigurationItem]
ADD CONSTRAINT [PK_UX__VCM_PK] PRIMARY KEY NONCLUSTERED  ([VCM_PK] ASC)

-- Foreign Key
ALTER TABLE [BPMConfigurationItem] WITH NOCHECK
      ADD   CONSTRAINT [BPMConfigurationItem_VCM_VCT_Template_FK2_BPMConfigurationTmpl_RRR_120N] FOREIGN KEY
          ( [VCM_VCT_Template] )
          REFERENCES [BPMConfigurationTmpl]
          ( [VCT_PK] )");
			}
		}

		static void InsertItems(DbConnection connection, IList<BPMConfigurationItem> items)
		{
			foreach (var item in items)
			{
				connection.ExecuteNonQuery(@$"
INSERT INTO [{TargetDbName}].dbo.{ItemTableName} (VCM_PK,VCM_VCT_Template,VCM_ConfigurationData,VCM_ConfigurationType,VCM_ConfigurationKey,VCM_RequiresReview,VCM_AutoVersion,VCM_SystemCreateTimeUtc,VCM_SystemCreateUser,VCM_SystemLastEditTimeUtc,VCM_SystemLastEditUser)
	VALUES (N'{item.VCM_PK}',N'{item.VCM_VCT_Template}',N'{item.VCM_ConfigurationData}',N'CFG',N'{item.VCM_ConfigurationKey}',0,0,'2023-09-17 22:55:00.000',N'TST','2023-09-17 22:55:00.000',N'TST');
");
			}
		}

		static void InsertTmpls(DbConnection connection, IList<BPMConfigurationTmpl> tmpls)
		{
			foreach (var tmpl in tmpls)
			{
				connection.ExecuteNonQuery(@$"
INSERT INTO [{TargetDbName}].dbo.{TmplTableName} (VCT_PK,VCT_IsActive,VCT_ParentTableCode,VCT_ParentID,VCT_SystemCreateTimeUtc,VCT_SystemCreateUser,VCT_SystemLastEditTimeUtc,VCT_SystemLastEditUser,VCT_AutoVersion)
	VALUES (N'{tmpl.VCT_PK}',1,N'FF3',N'33333333-3333-3333-3333-111111111111','2022-04-11 05:29:00.000',N'TST','2022-04-11 05:29:00.000',N'TST',0);
");
			}
		}

		static void InsertRegistry(DbConnection connection, string name, string value)
		{
			connection.ExecuteNonQuery(@$"
INSERT INTO [{TargetDbName}].dbo.StmData (SD_PK,SD_Name,SD_Type,SD_BinaryValue,SD_IsLogged,SD_IsCancelled,SD_PreserveTestValue,SD_SystemCreateTimeUtc,SD_SystemCreateUser,SD_SystemLastEditTimeUtc,SD_SystemLastEditUser)
	VALUES (N'{Guid.NewGuid()}',N'{name}',N'BOL',CONVERT(VARBINARY(MAX), CONVERT(NVARCHAR(MAX), '{value}')),1,0,1,'2024-07-17 03:12:00.000',N'TST','2024-07-17 03:32:00.000',N'TST');
");
		}

		static void Restore(DbConnection connection, IList<BPMConfigurationTmpl> tmpls, IList<BPMConfigurationItem> items)
		{
			CreateTablesAndInsertData(connection, tmpls, items);
		}

		void AssertRecords(DbConnection connection, IList<BPMConfigurationItem> expectedItems, IList<BPMConfigurationTmpl> expectedTmpls)
		{
			var sqlScript = $"SELECT VCM_PK, VCM_VCT_Template, VCM_ConfigurationData, VCM_ConfigurationKey FROM [{TargetDbName}].dbo.{ItemTableName}";
			using (var reader = connection.Command(sqlScript).ExecuteReader())
			{
				var recordNum = 0;

				while (reader.Read())
				{
					recordNum++;
					var pk = reader["VCM_PK"].ToString();
					var vct_pk = reader["VCM_VCT_Template"].ToString();
					var data = reader["VCM_ConfigurationData"].ToString();
					var key = reader["VCM_ConfigurationKey"].ToString();

					var expectedItem = expectedItems.FirstOrDefault(n => n.VCM_PK == Guid.Parse(pk));
					AssertNotNull($"VCM_PK: {pk} shouldn't exist", expectedItem);
					Assert($"VCM_VCT_Template: {vct_pk} should be equal to the expected: {expectedItem.VCM_VCT_Template}", Guid.Parse(vct_pk) == expectedItem.VCM_VCT_Template);
					Assert($"VCM_ConfigurationData: {data} should be equal to the expected: {expectedItem.VCM_ConfigurationData}", data == expectedItem.VCM_ConfigurationData);
					Assert($"VCM_ConfigurationKey: {key} should be equal to the expected: {expectedItem.VCM_ConfigurationKey}", key == expectedItem.VCM_ConfigurationKey);
				}

				Assert("records count should meet expect", recordNum == expectedItems.Count);
			}

			sqlScript = $"SELECT VCT_PK FROM [{TargetDbName}].dbo.{TmplTableName}";
			using (var reader = connection.Command(sqlScript).ExecuteReader())
			{
				var recordNum = 0;

				while (reader.Read())
				{
					recordNum++;
					var pk = reader["VCT_PK"].ToString();

					var expectedTmpl = expectedTmpls.FirstOrDefault(n => n.VCT_PK == Guid.Parse(pk));
					AssertNotNull($"VCT_PK: {pk} shouldn't exist", expectedTmpl);
				}

				Assert("records count should meet expect", recordNum == expectedTmpls.Count);
			}
		}

		void AssertForeignKeyIsEnabled(DbConnection connection)
		{
			var isForeignKeyEnabled = !connection.ExecuteScalar<bool>($"USE [{TargetDbName}]; SELECT is_disabled FROM sys.foreign_keys WHERE name = 'BPMConfigurationItem_VCM_VCT_Template_FK2_BPMConfigurationTmpl_RRR_120N'; USE [{Db.SqlMasterDb}]");
			Assert("Foreign key: BPMConfigurationItem_VCM_VCT_Template_FK2_BPMConfigurationTmpl_RRR_120N should be enabled", isForeignKeyEnabled);
		}
	}

	class BPMConfigurationItem
	{
		public Guid VCM_PK { get; set; }
		public string VCM_ConfigurationData { get; set; }
		public string VCM_ConfigurationKey { get; set; }
		public Guid VCM_VCT_Template { get; set; }
	}

	class BPMConfigurationTmpl
	{
		public Guid VCT_PK { get; set; }
	}
}
