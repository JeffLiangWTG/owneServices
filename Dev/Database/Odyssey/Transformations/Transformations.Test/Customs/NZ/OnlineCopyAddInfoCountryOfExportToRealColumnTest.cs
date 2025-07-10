using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.NZ;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.DbUpgrader.Transformations.Transforms.Customs.NZ.CopyAddInfoCountryOfExportToRealColumn;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.NZ;

[TestedType(typeof(OnlineCopyAddInfoCountryOfExportToRealColumn))]
class OnlineCopyAddInfoCountryOfExportToRealColumnTest : OnlineCopyAddInfoToRealColumnTest<CopyAddInfoCountryOfExportToRealColumn, OnlineCopyAddInfoCountryOfExportToRealColumn>
{
	protected override void AssertFirstRunResult(OnlineCopyAddInfoCountryOfExportToRealColumn testTransformation, string[] logs)
		=> AssertTransformationResults();
	protected override void AssertSecondRunResult(OnlineCopyAddInfoCountryOfExportToRealColumn testTransformation, string[] logs)
		=> AssertTransformationResults();

	void AssertTransformationResults()
	{
		var values = new List<(int ClusterKey, string AddInfo, string CountryOfExport)>();
		var sqlText = @"SELECT JZ_ClusterKey, JZ_AddInfo, JZ_RN_NKCountryOfExport FROM JobComInvoiceHeader ORDER BY JZ_ClusterKey ASC";
		using (var cmd = Db.Connection.Command(sqlText))
		using (var reader = cmd.ExecuteReader())
		{
			while (reader.Read())
			{
				values.Add((
					reader.GetInt32(0),
					reader.GetString(1),
					reader.GetString(2)
				));
			}
		}
		AssertContainsExactElementsInExactOrder(new[] {
			(1, "RN_NKCountryOfExport=ER", ""),
			(2, "RN_NKCountryOfExport=ER", "ER"),
			(3, "SomethingElse=ER", ""),
			(4, "", ""),
			(5, "RN_NKCountryOfExport=ER", ""),
			(6, "RN_NKCountryOfExport=ER", "ER"),
			(7, "SomethingElse=ER", ""),
			(8, "", ""),
		}, values);
	}

	protected override OnlineCopyAddInfoCountryOfExportToRealColumn CreateOnlineTransformation()
		=> new OnlineCopyAddInfoCountryOfExportToRealColumn(manager, Db.DatabaseName, TemplateDb);

	protected override ITableSchema TargetTableSchema => JobComInvoiceHeaderSchema.Instance;

	protected override void CreateTestData()
	{
		var sqlText = @"
DECLARE @CNCompanyPK UNIQUEIDENTIFIER = NEWID();
DECLARE @NZCompanyPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO GlbCompany(GC_PK, GC_Code, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES (@CNCompanyPK, 'DCN', 'CN', 'CNY', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO GlbCompany(GC_PK, GC_Code, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES (@NZCompanyPK, 'DNZ', 'NZ', 'NZD', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
DECLARE @CNBranchPK UNIQUEIDENTIFIER = NEWID();
DECLARE @NZBranchPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES(@CNBranchPK, @CNCompanyPK, 'BCN', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES(@NZBranchPK, @NZCompanyPK, 'BNZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
DECLARE @CNDecPK UNIQUEIDENTIFIER = NEWID();
DECLARE @NZDecPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES (@CNDecPK, @CNBranchPK, @CNCompanyPK, 1, 'CN', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES (@NZDecPK, @NZBranchPK, @NZCompanyPK, 2, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
-- Invoices Attached to a Declaration
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo) VALUES (NEWID(), @CNDecPK, 1, 'CN', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'RN_NKCountryOfExport=ER');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo) VALUES (NEWID(), @NZDecPK, 2, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'RN_NKCountryOfExport=ER');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo) VALUES (NEWID(), @NZDecPK, 3, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'SomethingElse=ER');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo) VALUES (NEWID(), @NZDecPK, 4, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', '');
-- Standalone Invoices
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_GB, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo) VALUES (NEWID(), @CNBranchPK, 5, 'CN', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'RN_NKCountryOfExport=ER');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_GB, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo) VALUES (NEWID(), @NZBranchPK, 6, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'RN_NKCountryOfExport=ER');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_GB, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo) VALUES (NEWID(), @NZBranchPK, 7, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'SomethingElse=ER');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_GB, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo) VALUES (NEWID(), @NZBranchPK, 8, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', '');
";
		Db.Connection.ExecuteNonQuery(sqlText);
	}
}
