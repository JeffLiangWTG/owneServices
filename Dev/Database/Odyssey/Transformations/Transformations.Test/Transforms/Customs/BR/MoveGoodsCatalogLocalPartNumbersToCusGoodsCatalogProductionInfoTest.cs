using System;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.BR.Testing
{
	[TestedType(typeof(MoveGoodsCatalogLocalPartNumbersToCusGoodsCatalogProductionInfo))]
	class MoveGoodsCatalogLocalPartNumbersToCusGoodsCatalogProductionInfoTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new MoveGoodsCatalogLocalPartNumbersToCusGoodsCatalogProductionInfo();
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("Should not have any CusReference linked to CusGoodsCatalog", 0, GetAmountOfCusReferenceCGC("=", "OR"));
			AssertEquals("Should have 3 CusReference", 3, GetAmountOfCusReferenceCGC("!=", "AND"));
			AssertEquals("Should have 3 CusReference", 3, GetAmountOfCusReferenceCGC("!=", "OR"));
			AssertEquals("Should have 3 CusGoodsCatalogProductionInfo with CGI_Type = FOR", 3, GetAmountCusGoodsCatalogProductionInfoByType("FOR"));
			AssertEquals("Should have 3 CusGoodsCatalogProductionInfo with CGI_Type = LPN", 5, GetAmountCusGoodsCatalogProductionInfoByType("LPN"));
			
			CombineAssertions(() =>
			{
				AssertEquals("LPN1.CGI_Reference", "123", GetDataFromCusGoodsCatalogProductionInfo("CGI_Reference", "LPN", "123"));
				AssertEquals("LPN1.CGI_CGC_Catalog", goodsCatalog.ToString(), GetDataFromCusGoodsCatalogProductionInfo("CGI_CGC_Catalog", "LPN", "123"));
				AssertEquals("LPN1.CGI_Type", "LPN", GetDataFromCusGoodsCatalogProductionInfo("CGI_Type", "LPN", "123"));
				AssertEquals("LPN1.CGI_CustomsStatus", "ACT", GetDataFromCusGoodsCatalogProductionInfo("CGI_CustomsStatus", "LPN", "123"));
				AssertEquals("LPN1.CGI_SystemCreateTimeUtc", date.ToString(), GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemCreateTimeUtc", "LPN", "123"));
				AssertEquals("LPN1.CGI_SystemLastEditTimeUtc", date.ToString(), GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemLastEditTimeUtc", "LPN", "123"));
				AssertEquals("LPN1.CGI_SystemCreateUser", "~E", GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemCreateUser", "LPN", "123"));
				AssertEquals("LPN1.CGI_SystemLastEditUser", "~E", GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemLastEditUser", "LPN", "123"));

				AssertEquals("LPN2.CGI_Reference", "234", GetDataFromCusGoodsCatalogProductionInfo("CGI_Reference", "LPN", "234"));
				AssertEquals("LPN2.CGI_CGC_Catalog", goodsCatalog.ToString(), GetDataFromCusGoodsCatalogProductionInfo("CGI_CGC_Catalog", "LPN", "234"));
				AssertEquals("LPN2.CGI_Type", "LPN", GetDataFromCusGoodsCatalogProductionInfo("CGI_Type", "LPN", "234"));
				AssertEquals("LPN2.CGI_CustomsStatus", "ACT", GetDataFromCusGoodsCatalogProductionInfo("CGI_CustomsStatus", "LPN", "234"));
				AssertEquals("LPN2.CGI_SystemCreateTimeUtc", date.ToString(), GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemCreateTimeUtc", "LPN", "234"));
				AssertEquals("LPN2.CGI_SystemLastEditTimeUtc", date.ToString(), GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemLastEditTimeUtc", "LPN", "234"));
				AssertEquals("LPN2.CGI_SystemCreateUser", "~E", GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemCreateUser", "LPN", "234"));
				AssertEquals("LPN2.CGI_SystemLastEditUser", "~E", GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemLastEditUser", "LPN", "234"));

				AssertEquals("LPN3.CGI_Reference", "456", GetDataFromCusGoodsCatalogProductionInfo("CGI_Reference", "LPN", "456"));
				AssertEquals("LPN3.CGI_CGC_Catalog", goodsCatalog.ToString(), GetDataFromCusGoodsCatalogProductionInfo("CGI_CGC_Catalog", "LPN", "456"));
				AssertEquals("LPN3.CGI_Type", "LPN", GetDataFromCusGoodsCatalogProductionInfo("CGI_Type", "LPN", "456"));
				AssertEquals("LPN3.CGI_CustomsStatus", "ACT", GetDataFromCusGoodsCatalogProductionInfo("CGI_CustomsStatus", "LPN", "456"));
				AssertEquals("LPN3.CGI_SystemCreateTimeUtc", date.ToString(), GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemCreateTimeUtc", "LPN", "456"));
				AssertEquals("LPN3.CGI_SystemLastEditTimeUtc", date.ToString(), GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemLastEditTimeUtc", "LPN", "456"));
				AssertEquals("LPN3.CGI_SystemCreateUser", "~E", GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemCreateUser", "LPN", "456"));
				AssertEquals("LPN3.CGI_SystemLastEditUser", "~E", GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemLastEditUser", "LPN", "456"));

				AssertEquals("LPN4.CGI_Reference", "678", GetDataFromCusGoodsCatalogProductionInfo("CGI_Reference", "LPN", "678"));
				AssertEquals("LPN4.CGI_CGC_Catalog", goodsCatalog.ToString(), GetDataFromCusGoodsCatalogProductionInfo("CGI_CGC_Catalog", "LPN", "678"));
				AssertEquals("LPN4.CGI_Type", "LPN", GetDataFromCusGoodsCatalogProductionInfo("CGI_Type", "LPN", "456"));
				AssertEquals("LPN4.CGI_CustomsStatus", "ACT", GetDataFromCusGoodsCatalogProductionInfo("CGI_CustomsStatus", "LPN", "678"));
				AssertEquals("LPN4.CGI_SystemCreateTimeUtc", dateInsert.Date, DateTimeOffset.Parse(GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemCreateTimeUtc", "LPN", "678")).Date);
				AssertEquals("LPN4.CGI_SystemLastEditTimeUtc", dateInsert.Date, DateTimeOffset.Parse(GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemLastEditTimeUtc", "LPN", "678")).Date);
				AssertEquals("LPN4.CGI_SystemCreateUser", "~BP", GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemCreateUser", "LPN", "678"));
				AssertEquals("LPN4.CGI_SystemLastEditUser", "~BP", GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemLastEditUser", "LPN", "678"));

				AssertEquals("LPN5.CGI_Reference", "891", GetDataFromCusGoodsCatalogProductionInfo("CGI_Reference", "LPN", "891"));
				AssertEquals("LPN5.CGI_CGC_Catalog", goodsCatalog.ToString(), GetDataFromCusGoodsCatalogProductionInfo("CGI_CGC_Catalog", "LPN", "891"));
				AssertEquals("LPN5.CGI_Type", "LPN", GetDataFromCusGoodsCatalogProductionInfo("CGI_Type", "LPN", "891"));
				AssertEquals("LPN5.CGI_CustomsStatus", "ACT", GetDataFromCusGoodsCatalogProductionInfo("CGI_CustomsStatus", "LPN", "891"));
				AssertEquals("LPN5.CGI_SystemCreateTimeUtc", dateInsert.Date, DateTimeOffset.Parse(GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemCreateTimeUtc", "LPN", "891")).Date);
				AssertEquals("LPN5.CGI_SystemLastEditTimeUtc", dateInsert.Date, DateTimeOffset.Parse(GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemLastEditTimeUtc", "LPN", "891")).Date);
				AssertEquals("LPN5.CGI_SystemCreateUser", "~BP", GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemCreateUser", "LPN", "891"));
				AssertEquals("LPN5.CGI_SystemLastEditUser", "~BP", GetDataFromCusGoodsCatalogProductionInfo("CGI_SystemLastEditUser", "LPN", "891"));
			});
		}

		protected override void PrepareTestData()
		{
			var helper = new TestDbHelper(TestConnection);
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var orgHeader = helper.InsertOrgHeader("OH1", "OH1Name");

			DBTransformationTestHelper.DropConstraintIfExists(CusReferenceSchema.Constants.TableName, "Constraint_CFR_Code");
			DBTransformationTestHelper.DropConstraintIfExists(CusReferenceSchema.Constants.TableName, "Constraint_CFR_ParentTableCode");
			DBTransformationTestHelper.DropConstraintIfExists(CusReferenceSchema.Constants.TableName, "Constraint_CFR_Type");
			DBTransformationTestHelper.DropConstraintIfExists(CusGoodsCatalogProductionInfoSchema.Constants.TableName, "Constraint_CGI_Type");
			TestConnection.ExecuteScalar("DROP TRIGGER IF EXISTS TG_CusReference_AuditDetailsAreNotMissing_Insert");

			helper.Insert(CusGoodsCatalogSchema.Constants.TableName, new
			{
				CGC_PK = Guid.NewGuid(),
				CGC_IsValid = true,
				CGC_GC_Company = companyPK,
				CGC_OH_Owner = orgHeader,
				CGC_CatalogCode = "00000001",
				CGC_Type = "IMP",
				CGC_Description = "TEST"
			});

			goodsCatalog = TestConnection.ExecuteScalar<Guid>("SELECT TOP 1 CGC_PK FROM CusGoodsCatalog");

			InsertCusGoodsCatalogProductionInfo(helper, "AU", string.Empty);
			InsertCusGoodsCatalogProductionInfo(helper, "BR", string.Empty);
			InsertCusGoodsCatalogProductionInfo(helper, "DE", string.Empty);

			InsertCusReferenceWithAuditDetails(helper, "XXX", "XXX", "123", "123");
			InsertCusReferenceWithAuditDetails(helper, "AAA", "AAA", "123", "123");
			InsertCusReferenceWithAuditDetails(helper, "BBB", "BBB", "123", "123");
			InsertCusReferenceWithAuditDetails(helper, CusGoodsCatalogSchema.Constants.Prefix, "CGC", "123", "123");
			InsertCusReferenceWithAuditDetails(helper, CusGoodsCatalogSchema.Constants.Prefix, "CGC", "234", "234");
			InsertCusReferenceWithAuditDetails(helper, CusGoodsCatalogSchema.Constants.Prefix, "CGC", "456", "456");

			TestConnection.ExecuteScalar(GetSqlInsertCusReferenceWithEmptyAuditDetails("CGC", "CGC", "678", "678"));
			TestConnection.ExecuteScalar(GetSqlInsertCusReferenceWithEmptyAuditDetails("CGC", "CGC", "891", "891"));

			dateInsert = DateTime.UtcNow;
		}

		Guid goodsCatalog;
		DateTime date => new DateTime(2024, 10, 30, 12, 00, 00);
		DateTime dateInsert;

		void InsertCusGoodsCatalogProductionInfo(TestDbHelper helper, string reference, string type)
		{
			helper.Insert(CusGoodsCatalogProductionInfoSchema.Constants.TableName, new
			{
				CGI_PK = Guid.NewGuid(),
				CGI_IsValid = true,
				CGI_CGC_Catalog = goodsCatalog,
				CGI_Reference = reference,
				CGI_Type = type
			});
		}

		void InsertCusReferenceWithAuditDetails(TestDbHelper helper, string parentTableCode, string type, string reference, string code)
		{
			helper.Insert(CusReferenceSchema.Constants.TableName, new
			{
				CFR_PK = Guid.NewGuid(),
				CFR_AutoVersion = false,
				CFR_ParentID = goodsCatalog,
				CFR_ParentTableCode = parentTableCode,
				CFR_Type = type,
				CFR_Reference = reference,
				CFR_Code = code,
				CFR_SystemCreateTimeUtc = date,
				CFR_SystemLastEditTimeUtc = date,
				CFR_SystemCreateUser = "~E",
				CFR_SystemLastEditUser = "~E"
			});
		}

		string GetDataFromCusGoodsCatalogProductionInfo(string column, string type, string reference)
		{
			var sql = $@"
SELECT {column} FROM CusGoodsCatalogProductionInfo
WHERE CGI_Type = '{type}' AND CGI_Reference = '{reference}'";

			return TestConnection.ExecuteScalar(sql).ToString();
		}

		int GetAmountCusGoodsCatalogProductionInfoByType(string type)
		{
			var sql = $@"SELECT COUNT(*) FROM CusGoodsCatalogProductionInfo
WHERE CGI_Type = '{type}'";

			return TestConnection.ExecuteScalar<int>(sql);
		}

		int GetAmountOfCusReferenceCGC(string comparison, string condition)
		{
			var sql = $@"
SELECT COUNT(*) FROM dbo.CusReference
WHERE CFR_Type {comparison} 'CGC' {condition} CFR_ParentTableCode {comparison} 'CGC'";

			return TestConnection.ExecuteScalar<int>(sql);
		} 

		string GetSqlInsertCusReferenceWithEmptyAuditDetails(string parentTableCode, string type, string reference, string code) => $@"
INSERT INTO [dbo].[CusReference]
           ([CFR_PK]
           ,[CFR_AutoVersion]
           ,[CFR_ParentID]
           ,[CFR_ParentTableCode]
           ,[CFR_Type]
           ,[CFR_Reference]
           ,[CFR_OA_Owner]
           ,[CFR_Code]
           ,[CFR_SystemCreateTimeUtc]
           ,[CFR_SystemCreateUser]
           ,[CFR_SystemLastEditTimeUtc]
           ,[CFR_SystemLastEditUser])
     VALUES
           (NEWID()
           ,0
           ,'{goodsCatalog}'
           ,'{parentTableCode}'
           ,'{type}'
           ,'{reference}'
           ,NULL
           ,'{code}'
           ,NULL
           ,''
           ,NULL
           ,'')";
	}
}
