using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	class ProductExpiredTariffIntegrationTest : TransactionedTestCase
	{
		public void TestProductExpiredTariff()
		{
			var organisationPK = TestDataCreator.CreateOrganisation(new string('X', OrgHeaderSchema.OH_Code.MaxLength), new string('X', OrgHeaderSchema.OH_FullName.MaxLength), "CAYVR");
			var companyPk = TestDataCreator.CreateCompany("DDE", "DE ", "EUR");
			var productPK = Guid.NewGuid();
			var partRelationPK = Guid.NewGuid();
			var classPartPivotPK = Guid.NewGuid();
			var tariffTypePK = Guid.NewGuid();

			var createRecordsSql = @"
INSERT INTO dbo.OrgSupplierPart (OP_PK, OP_PartNum, OP_Desc, OP_SystemCreateTimeUtc, OP_SystemCreateUser, OP_SystemLastEditTimeUtc, OP_SystemLastEditUser) VALUES (@productPK, @partNum, @partDesc, '2015-01-01 00:00:00.000', '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.OrgPartRelation (OU_PK, OU_OH, OU_OP, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser) VALUES (@partRelationPK, @organisationPK, @productPK, 'BTH', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'EUN')
INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES(NEWID(), 'DE', 'South Africa', NULL)

IF NOT EXISTS(Select 1 from RefDatabase_RefCusTariffType where ZZI_TariffType = '1P1')
INSERT RefDatabase_RefCusTariffType(ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) VALUES(@tariffTypePK, '1P1', 'Sched 1 Part 1', 'EUN')

INSERT INTO dbo.RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) VALUES (NEWID(), @tariffTypePK, '61046111', 'tariff desc', 'EUN', '2014-01-01 00:00:00.000', '2016-12-31 00:00:00.000', '')
";

			using (var command = Db.Connection.Command(createRecordsSql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPK);
				command.AddParameter("@productPK", SqlDbType.UniqueIdentifier, productPK);
				command.AddParameter("@partRelationPK", SqlDbType.UniqueIdentifier, partRelationPK);
				command.AddParameter("@classPartPivotPK", SqlDbType.UniqueIdentifier, classPartPivotPK);
				command.AddParameter("@partNum", SqlDbType.VarChar, "P123");
				command.AddParameter("@partDesc", SqlDbType.VarChar, "P123 DESC");
				command.AddParameter("@tariffTypePK", SqlDbType.UniqueIdentifier, tariffTypePK);

				command.ExecuteNonQuery();
			}

			var factory = new BusinessObjectFactory();
			var partPivot = factory.New<Integration.Customs.DE.ICusClassPartPivot>() as BusinessObject;
			partPivot[CusClassPartPivotSchema.CI_OP] = productPK;
			partPivot[CusClassPartPivotSchema.CI_RN_NKCountry] = "DE";
			partPivot[CusClassPartPivotSchema.CI_ChildType] = "HTI";
			partPivot[CusClassPartPivotSchema.CI_TariffNum] = "61046111";

			factory.Save();

			AssertResult(companyPk, organisationPK, "2018-04-01 00:00:00.000", true);
			AssertResult(companyPk, organisationPK, "2015-04-01 00:00:00.000", false);
		}

		void AssertResult(Guid companyPK, Guid organisationPK, string effectiveDateString, bool found)
		{
			var spSql = $"SELECT PartNum FROM ProductExpiredTariff('{companyPK}','OWN', '{organisationPK}', '2014-01-01 00:00:00.000','2019-01-01 00:00:00.000','{effectiveDateString}')";
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
