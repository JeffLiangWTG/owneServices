using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using CargoWise.Types;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USProductListing))]
	class ProductListingTest : DbCreateScriptTest
	{
		public void TestProvAddTariff()
		{
			var htiPivotPK1 = Guid.NewGuid();
			var htiPivotPK2 = Guid.NewGuid();
			var htiPivotPK3 = Guid.NewGuid();
			var htiPivotPK4 = Guid.NewGuid();
			var htiPivotPK5 = Guid.NewGuid();
			var htiPivotPK6 = Guid.NewGuid();
			var htiPivotPK7 = Guid.NewGuid();
			var shbPivotPK = Guid.NewGuid();
			var htePivotPK = Guid.NewGuid();

			var importerPK = TestDataCreator.CreateOrganisation("oh1", "OrgH1");
			var createTestDataSQL = $@"
DECLARE @productPK1 UNIQUEIDENTIFIER = NEWID();
DECLARE @productPK2 UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.OrgSupplierPart(OP_PK, OP_PartNum) VALUES (@productPK1, 'WI00888703-1');
INSERT INTO dbo.OrgPartRelation(OU_PK, OU_OP, OU_OH, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser) VALUES(NEWID(), @productPK1, '{importerPK}', 'BTH', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusClassPartPivot (CI_PK, CI_OP, CI_RN_NKCountry, CI_ChildType, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser) VALUES ('{htiPivotPK1}', @productPK1, 'US', 'HTI',GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK1}', 'CI', '11111111', 'AT1', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK1}', 'CI', '22222222', 'AT2', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK1}', 'CI', '33333333', 'AT3', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK1}', 'CI', '44444444', 'AT4', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK1}', 'CI', '55555555', 'AT5', 'US');
INSERT INTO dbo.CusClassPartPivot (CI_PK, CI_OP, CI_RN_NKCountry, CI_ChildType, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser) VALUES ('{htiPivotPK2}', @productPK1, 'US', 'HTI',GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK2}', 'CI', '66666666', 'AT1', 'US');
INSERT INTO dbo.CusClassPartPivot (CI_PK, CI_OP, CI_RN_NKCountry, CI_ChildType, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser) VALUES ('{htiPivotPK3}', @productPK1, 'US', 'HTI',GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK3}', 'CI', '77777777', 'AT2', 'US');
INSERT INTO dbo.CusClassPartPivot (CI_PK, CI_OP, CI_RN_NKCountry, CI_ChildType, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser) VALUES ('{htiPivotPK4}', @productPK1, 'US', 'HTI',GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK4}', 'CI', '88888888', 'AT3', 'US');
INSERT INTO dbo.CusClassPartPivot (CI_PK, CI_OP, CI_RN_NKCountry, CI_ChildType, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser) VALUES ('{htiPivotPK5}', @productPK1, 'US', 'HTI',GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK5}', 'CI', '99999999', 'AT4', 'US');
INSERT INTO dbo.CusClassPartPivot (CI_PK, CI_OP, CI_RN_NKCountry, CI_ChildType, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser) VALUES ('{htiPivotPK6}', @productPK1, 'US', 'HTI',GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK6}', 'CI', '11112222', 'AT5', 'US');
INSERT INTO dbo.CusClassPartPivot (CI_PK, CI_OP, CI_RN_NKCountry, CI_ChildType, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser) VALUES ('{shbPivotPK}', @productPK1, 'US', 'SHB',GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{shbPivotPK}', 'CI', '11113333', 'AT1', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{shbPivotPK}', 'CI', '11114444', 'AT2', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{shbPivotPK}', 'CI', '11115555', 'AT3', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{shbPivotPK}', 'CI', '11116666', 'AT4', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{shbPivotPK}', 'CI', '11117777', 'AT5', 'US');
INSERT INTO dbo.CusClassPartPivot (CI_PK, CI_OP, CI_RN_NKCountry, CI_ChildType, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser) VALUES ('{htePivotPK}', @productPK1, 'US', 'SHB',GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htePivotPK}', 'CI', '11118888', 'AT1', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htePivotPK}', 'CI', '11119999', 'AT2', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htePivotPK}', 'CI', '22221111', 'AT3', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htePivotPK}', 'CI', '22223333', 'AT4', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htePivotPK}', 'CI', '22224444', 'AT5', 'US');

INSERT INTO dbo.OrgSupplierPart(OP_PK, OP_PartNum) VALUES (@productPK2, 'WI00888703-2');
INSERT INTO dbo.OrgPartRelation(OU_PK, OU_OP, OU_OH, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser) VALUES(NEWID(), @productPK2, '{importerPK}', 'BTH', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusClassPartPivot (CI_PK, CI_OP, CI_RN_NKCountry, CI_ChildType, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser) VALUES ('{htiPivotPK7}', @productPK2, 'US', 'HTI',GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK7}', 'CI', '22225555', 'AT1', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK7}', 'CI', '22226666', 'AT2', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK7}', 'CI', '22227777', 'AT3', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK7}', 'CI', '22228888', 'AT4', 'US');
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_DataModel) VALUES(NEWID(), '{htiPivotPK7}', 'CI', '22229999', 'AT5', 'US');
";
			using (var command = Db.Connection.Command(createTestDataSQL))
			{
				command.ExecuteNonQuery();
			}

			var sql = $@"SELECT ProvProgAddtionalTariff1, ProvProgAddtionalTariff2, ProvProgAddtionalTariff3, ProvProgAddtionalTariff4, ProvProgAddtionalTariff5 FROM USProductListing('OWN','{importerPK}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL) WHERE CusClassPartPivotPK = @pivotPK";
			AssertProvAddTariff("HTI1", htiPivotPK1, "11111111", "22222222", "33333333", "44444444", "55555555");
			AssertProvAddTariff("HTI2", htiPivotPK2, "66666666", "", "", "", "");
			AssertProvAddTariff("HTI3", htiPivotPK3, "", "77777777", "", "", "");
			AssertProvAddTariff("HTI4", htiPivotPK4, "", "", "88888888", "", "");
			AssertProvAddTariff("HTI5", htiPivotPK5, "", "", "", "99999999", "");
			AssertProvAddTariff("HTI6", htiPivotPK6, "", "", "", "", "11112222");
			AssertProvAddTariff("SHB", shbPivotPK, "", "", "", "", "");
			AssertProvAddTariff("HTE", htePivotPK, "", "", "", "", "");
			AssertProvAddTariff("HTI7", htiPivotPK7, "22225555", "22226666", "22227777", "22228888", "22229999");

			void AssertProvAddTariff(ZString message, Guid pivotPK, ZString tariff1, ZString tariff2, ZString tariff3, ZString tariff4, ZString tariff5)
			{
				using (var command = Db.Connection.Command(sql))
				{
					command.AddParameter("@pivotPK", SqlDbType.UniqueIdentifier, pivotPK);
					using (var reader = command.ExecuteReader())
					{
						reader.Read();
						CombineAssertions(() =>
						{
							AssertEquals(message + "ProvProgAddtionalTariff1", tariff1, reader["ProvProgAddtionalTariff1"].ToString());
							AssertEquals(message + "ProvProgAddtionalTariff2", tariff2, reader["ProvProgAddtionalTariff2"].ToString());
							AssertEquals(message + "ProvProgAddtionalTariff3", tariff3, reader["ProvProgAddtionalTariff3"].ToString());
							AssertEquals(message + "ProvProgAddtionalTariff4", tariff4, reader["ProvProgAddtionalTariff4"].ToString());
							AssertEquals(message + "ProvProgAddtionalTariff5", tariff5, reader["ProvProgAddtionalTariff5"].ToString());
						});
					}
				}
			}
		}
	}
}
