using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	class USProductExpiredTariffIntegrationTest : TransactionedTestCase
	{
		public void TestUSProductExpiredTariff()
		{
			var organisationPK = TestDataCreator.CreateOrganisation(new string('X', OrgHeaderSchema.OH_Code.MaxLength), new string('X', OrgHeaderSchema.OH_FullName.MaxLength), "CAYVR");
			var productPK = Guid.NewGuid();
			var partRelationPK = Guid.NewGuid();
			var classPartPivotPK = Guid.NewGuid();

			var createRecordsSql = @"
INSERT INTO dbo.OrgSupplierPart (OP_PK, OP_PartNum, OP_Desc, OP_SystemCreateTimeUtc, OP_SystemCreateUser, OP_SystemLastEditTimeUtc, OP_SystemLastEditUser) VALUES (@productPK, @partNum, @partDesc, '2015-01-01 00:00:00.000', '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.OrgPartRelation (OU_PK, OU_OH, OU_OP, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser) VALUES (@partRelationPK, @organisationPK, @productPK, 'BTH', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.RefDbEntUs_USCTariff (UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo) VALUES (NEWID(), '6111203001', '2014-01-01 00:00:00.000', '2016-12-31 00:00:00.000')
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
			var partPivot = factory.New<Integration.Customs.US.ICusClassPartPivot>() as BusinessObject;
			partPivot[CusClassPartPivotSchema.CI_OP] = productPK;
			partPivot[CusClassPartPivotSchema.CI_RN_NKCountry] = "US";
			partPivot[CusClassPartPivotSchema.CI_ChildType] = "HTI";
			partPivot[CusClassPartPivotSchema.CI_TariffNum] = "6111203001";

			factory.Save();

			AssertResult(organisationPK, "2018-04-01 00:00:00.000", true); // CI_TariffNum espired
			AssertResult(organisationPK, "2015-04-01 00:00:00.000", false);

			partPivot[CusClassPartPivotSchema.CI_TariffNum] = string.Empty;
			partPivot[CusClassPartPivotSchema.CI_SupplementalTariff] = "6111203001";

			factory.Save();

			AssertResult(organisationPK, "2018-04-01 00:00:00.000", true); // CI_SupplementalTariff espired
			AssertResult(organisationPK, "2015-04-01 00:00:00.000", false);

			var cusClassification = factory.New<Integration.Customs.US.ICusClassification>() as BusinessObject;
			cusClassification[CusClassificationSchema.CC_LookupCode] = "6111.20.3001";
			cusClassification[CusClassificationSchema.CC_TariffNum] = "6111203001";
			cusClassification[CusClassificationSchema.CC_RN_NKCountryCode] = "US";
			cusClassification[CusClassificationSchema.CC_ClassificationType] = "IMP";

			partPivot[CusClassPartPivotSchema.CI_SupplementalTariff] = string.Empty;
			partPivot[CusClassPartPivotSchema.CI_CC] = cusClassification[CusClassificationSchema.PK];

			factory.Save();

			AssertResult(organisationPK, "2018-04-01 00:00:00.000", true); // CC_TariffNum espired
			AssertResult(organisationPK, "2015-04-01 00:00:00.000", false);

			cusClassification[CusClassificationSchema.CC_LookupCode] = string.Empty;
			cusClassification[CusClassificationSchema.CC_TariffNum] = string.Empty;

			var childPartPivot = factory.New<Integration.Customs.US.ICusClassPartPivot>() as BusinessObject;
			childPartPivot[CusClassPartPivotSchema.CI_OP] = productPK;
			childPartPivot[CusClassPartPivotSchema.CI_RN_NKCountry] = "US";
			childPartPivot[CusClassPartPivotSchema.CI_CI_Parent] = partPivot.PK;
			childPartPivot[CusClassPartPivotSchema.CI_TariffNum] = "6111203001";

			AssertResult(organisationPK, "2018-04-01 00:00:00.000", true); // child CI_TariffNum espired
			AssertResult(organisationPK, "2015-04-01 00:00:00.000", false);
		}

		void AssertResult(Guid organisationPK, string effectiveDateString, bool found)
		{
			var spSql = $"SELECT PartNum FROM USProductExpiredTariff('OWN', '{organisationPK}', '2014-01-01 00:00:00.000','2019-01-01 00:00:00.000','{effectiveDateString}')";
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
