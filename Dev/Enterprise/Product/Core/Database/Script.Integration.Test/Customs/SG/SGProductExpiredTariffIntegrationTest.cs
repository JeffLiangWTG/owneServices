using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.SG
{
	class SGProductExpiredTariffIntegrationTest : TransactionedTestCase
	{
		public void TestSGProductExpiredTariff()
		{
			var organisationPK = TestDataCreator.CreateOrganisation(new string('X', OrgHeaderSchema.OH_Code.MaxLength), new string('X', OrgHeaderSchema.OH_FullName.MaxLength), "CAYVR");
			var productPK = Guid.NewGuid();
			var partRelationPK = Guid.NewGuid();
			var classPartPivotPK = Guid.NewGuid();

			CreateExpiredTariff();
			var createRecordsSql = @"
INSERT INTO dbo.OrgSupplierPart (OP_PK, OP_PartNum, OP_Desc, OP_SystemCreateTimeUtc, OP_SystemCreateUser, OP_SystemLastEditTimeUtc, OP_SystemLastEditUser) VALUES (@productPK, @partNum, @partDesc, '2015-01-01 00:00:00.000', '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.OrgPartRelation (OU_PK, OU_OH, OU_OP, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser) VALUES (@partRelationPK, @organisationPK, @productPK, 'BTH', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

			using (var command = Db.Connection.Command(createRecordsSql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPK);
				command.AddParameter("@productPK", SqlDbType.UniqueIdentifier, productPK);
				command.AddParameter("@partRelationPK", SqlDbType.UniqueIdentifier, partRelationPK);
				command.AddParameter("@classPartPivotPK", SqlDbType.UniqueIdentifier, classPartPivotPK);
				command.AddParameter("@partNum", SqlDbType.VarChar, "P123");
				command.AddParameter("@partDesc", SqlDbType.VarChar, "P123 DESC");

				command.ExecuteNonQuery();
			}

			var factory = new BusinessObjectFactory();
			var partPivot = factory.New<Integration.Customs.SG.ICusClassPartPivot>() as BusinessObject;
			partPivot[CusClassPartPivotSchema.CI_OP] = productPK;
			partPivot[CusClassPartPivotSchema.CI_RN_NKCountry] = "SG";
			partPivot[CusClassPartPivotSchema.CI_ChildType] = "HTI";
			partPivot[CusClassPartPivotSchema.CI_TariffNum] = "9301901021";

			factory.Save();

			AssertResult(organisationPK, "2018-04-01 00:00:00.000", true);
			AssertResult(organisationPK, "2015-04-01 00:00:00.000", false);
		}

		void CreateExpiredTariff()
		{
			var zzSQL = @"
IF NOT EXISTS(SELECT 1 FROM dbo.RefDatabase_RefDataGrouping WHERE ZZZ_DataGrouping = 'SG')
INSERT INTO dbo.RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES
(NEWID(), 'SG', 'Singapore', null)

			DECLARE @TariffTypePK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.RefDatabase_RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZ9_NKNomenclatureGroupType, ZZI_ZZZ_NKDataGrouping) VALUES
(@TariffTypePK, 'HSN', 'Import', '', 'SG')

			DECLARE @ExpiredTariffPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.RefDatabase_RefCusTariff(ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZZ_NKDataGrouping, ZZ1_ZZF_NKTaxOrFeeCode) VALUES
(@ExpiredTariffPK, @TariffTypePK, '9301901021', 'EXPIRED TARIFF', '2014-01-01', '2016-12-31', 'SG', '')";

			using (var command = Db.Connection.Command(zzSQL))
			{
				command.ExecuteNonQuery();
			}
		}

		void AssertResult(Guid organisationPK, string effectiveDateString, bool found)
		{
			var spSql = $"SELECT PartNum FROM SGProductExpiredTariff('OWN', '{organisationPK}', '2014-01-01 00:00:00.000','2019-01-01 00:00:00.000','{effectiveDateString}')";
			using (var command = Db.Connection.Command(spSql))
			{
				using (var reader = command.ExecuteReader())
				{
					if (found)
					{
						Assert(reader.Read());
						AssertEquals("P123", reader.GetString(0));
					}
					else
					{
						Assert(!reader.Read());
					}
				}
			}
		}
	}
}
