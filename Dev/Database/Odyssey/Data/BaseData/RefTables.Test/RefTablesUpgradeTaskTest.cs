using System;
using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.RefTables
{
	sealed class RefTablesUpgradeTaskTest : TransactionedTestCase
	{
		public void TestAllOhSupplierReferencesInXmlAreNull()
		{
			RefTablesUpgradeTask testTask = new RefTablesUpgradeTask();
			testTask.Run();

			string sqlText = "SELECT TOP 1 RP_PK FROM dbo.RefPacks WHERE RP_OH_Supplier is not null";
			AssertEquals("Non null RP_OH_Supplier exists?", false, BaseDataUpgradeTask.IsRecordInDatabase(sqlText));
		}

		public void TestRun()
		{
			// Prepare test data
			Guid newCommodityCodePk = Guid.NewGuid();
			Guid generalCommodityPk = new Guid("E3CA23C3-2DBF-4660-BD2B-77573B26CFAA");

			Guid newContainerCodePk = Guid.NewGuid();
			Guid tipperContainerPk = new Guid("2D12921E-DFC9-4540-B94C-65860BFB7683");

			Guid newContainerCodeMapPk = Guid.NewGuid();
			Guid newContainerCodeMapPk1 = Guid.NewGuid();
			Guid existingContainerPK = new Guid("7405f604-9665-4a55-890d-113a4671308d");

			Guid newServiceLevelPk = Guid.NewGuid();
			Guid directServiceLevelPk = new Guid("7C6C81A8-7B76-43C2-8A27-1080BABE87C1");

			Guid newPackslPk = Guid.NewGuid();
			Guid m3cuauPacksPk = new Guid("237DEFE4-A60C-4796-8E5D-5A7F1C849744");

			string sqlText = String.Format(@"
				-- RefCommodityCode
				INSERT dbo.RefCommodityCode (RH_PK, RH_Code, RH_Description) VALUES ('{0}', '~C1', '~CommodityDescription');
				UPDATE dbo.RefCommodityCode SET RH_Description = 'Some~Desc~Not~To~Be~Changed', RH_SystemLastEditTimeUtc = '', RH_SystemLastEditUser = '' WHERE RH_PK = '{1}';
				-- RefContainer
				INSERT dbo.RefContainer (RC_PK, RC_Code, RC_Description) VALUES ('{2}', '~C1', '~ContainerDescription');
				UPDATE dbo.RefContainer SET RC_Description = 'Some~Desc~Not~To~Be~Changed' WHERE RC_PK = '{3}';
				-- RefContainerCodeMap
				INSERT dbo.RefContainerCodeMap (RCM_PK, RCM_RC_Container, RCM_RN_NKCountry, RCM_Usage, RCM_Code) VALUES ('{4}', '{2}', 'US', '', 'U1');
				UPDATE dbo.RefContainerCodeMap SET RCM_Code = '~U' WHERE RCM_RC_Container = '{5}' AND RCM_RN_NKCountry='US';
				INSERT dbo.RefContainerCodeMap (RCM_PK, RCM_RC_Container, RCM_RN_NKCountry, RCM_Usage, RCM_Code) VALUES ('{10}', '{2}', 'CN', '', 'U1');
				UPDATE dbo.RefContainerCodeMap SET RCM_Code = '~U' WHERE RCM_RC_Container = '{5}' AND RCM_RN_NKCountry='CN';
				-- RefServiceLevel
				INSERT dbo.RefServiceLevel (RS_PK, RS_Code, RS_Description) VALUES ('{6}', '~C1', '~ServiceLevelDescription');
				UPDATE dbo.RefServiceLevel SET RS_Description = 'Some~Desc~Not~To~Be~Changed' WHERE RS_PK = '{7}';
				-- RefPacks
				INSERT dbo.RefPacks (RP_PK, RP_CommercialPack, RP_CustomsPack, RP_CustomsCountry) VALUES ('{8}', '~OP', '~UP', 'CC');
				UPDATE dbo.RefPacks SET RP_CommercialPack = 'CoP', RP_CustomsPack = 'CuP', RP_CustomsCountry = 'CO' WHERE RP_PK = '{9}';
				",
				newCommodityCodePk.ToString(),
				generalCommodityPk.ToString(),
				newContainerCodePk.ToString(),
				tipperContainerPk.ToString(),
				newContainerCodeMapPk,
				existingContainerPK,
				newServiceLevelPk.ToString(),
				directServiceLevelPk.ToString(),
				newPackslPk.ToString(),
				m3cuauPacksPk.ToString(),
				newContainerCodeMapPk1.ToString());

			TestConnection.ExecuteNonQuery(sqlText);

			// RefCommodityCode
			AssertEquals("[BEFORE] New RefCommodityCode in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefCommodityCodeSchema.PK, newCommodityCodePk));
			AssertEquals("[BEFORE] Existing RefCommodityCode in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefCommodityCodeSchema.PK, generalCommodityPk));

			// RefContainer
			AssertEquals("[BEFORE] New RefContainer in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefContainerSchema.PK, newContainerCodePk));
			AssertEquals("[BEFORE] Existing RefContainer in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefContainerSchema.PK, tipperContainerPk));

			// RefContainerCodeMap
			Assert("[BEFORE] New RefContainerCodeMap should be in database", BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefContainerCodeMapSchema.PK, newContainerCodeMapPk));
			Assert("[BEFORE] Existing RefContainerCodeMap should be in database", BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, $"SELECT 1 FROM dbo.RefContainerCodeMap WHERE RCM_RC_Container='{existingContainerPK}' and RCM_RN_NKCountry = 'CN' and RCM_Usage = ''"));
			Assert("[BEFORE] Existing RefContainerCodeMap should be in database", BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, $"SELECT 1 FROM dbo.RefContainerCodeMap WHERE RCM_RC_Container='{existingContainerPK}' and RCM_RN_NKCountry = 'US' and RCM_Usage = ''"));

			// RefServiceLevel
			AssertEquals("[BEFORE] New RefServiceLevel in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefServiceLevelSchema.PK, newServiceLevelPk));
			AssertEquals("[BEFORE] Existing RefServiceLevel in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefServiceLevelSchema.PK, directServiceLevelPk));

			RefTablesUpgradeTask testTask = new RefTablesUpgradeTask();
			testTask.Run();

			// Assert results

			// RefCommodityCode
			AssertEquals("New RefCommodityCode in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefCommodityCodeSchema.PK, newCommodityCodePk));
			AssertEquals("Update dbo.RefCommodityCode name not changed back", "Some~Desc~Not~To~Be~Changed", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, RefCommodityCodeSchema.PK, generalCommodityPk, RefCommodityCodeSchema.RH_Description));

			// RefContainer
			AssertEquals("New RefContainer in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefContainerSchema.PK, newContainerCodePk));
			AssertEquals("Update dbo.RefContainer name not changed back", "Some~Desc~Not~To~Be~Changed", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, RefContainerSchema.PK, tipperContainerPk, RefContainerSchema.RC_Description));

			// RefContainerMap
			Assert("Newly inserted RefContainerMap should remain after UpgradeTask.", BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefContainerCodeMapSchema.PK, newContainerCodeMapPk));
			AssertEquals("User changed RefContainerMap should not be changed by UpgradeTask.", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, $"SELECT 1 FROM dbo.RefContainerCodeMap WHERE RCM_RC_Container='{existingContainerPK}' AND RCM_Code='~U' AND RCM_RN_NKCountry = 'CN' and RCM_Usage = ''"));
			AssertEquals("User changed RefContainerMap should not be changed by UpgradeTask.", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, $"SELECT 1 FROM dbo.RefContainerCodeMap WHERE RCM_RC_Container='{existingContainerPK}' AND RCM_Code='~U' AND RCM_RN_NKCountry = 'US' and RCM_Usage = ''"));

			// RefServiceLevel
			AssertEquals("New RefServiceLevel in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefServiceLevelSchema.PK, newServiceLevelPk));
			AssertEquals("Update dbo.RefServiceLevel name not changed back", "Some~Desc~Not~To~Be~Changed", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, RefServiceLevelSchema.PK, directServiceLevelPk, RefServiceLevelSchema.RS_Description));

			// RefPacks
			AssertEquals("New RefPacks in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefPacksSchema.PK, newPackslPk));
			AssertEquals("Update dbo.RefPacks CommercialPack not changed back", "CoP", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, RefPacksSchema.PK, m3cuauPacksPk, RefPacksSchema.RP_CommercialPack));
			AssertEquals("Update dbo.RefPacks CustomsPack not changed back", "CuP", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, RefPacksSchema.PK, m3cuauPacksPk, RefPacksSchema.RP_CustomsPack));
			AssertEquals("Update dbo.RefPacks CustomsCountry not changed back", "CO", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, RefPacksSchema.PK, m3cuauPacksPk, RefPacksSchema.RP_CustomsCountry));
		}
	}
}
