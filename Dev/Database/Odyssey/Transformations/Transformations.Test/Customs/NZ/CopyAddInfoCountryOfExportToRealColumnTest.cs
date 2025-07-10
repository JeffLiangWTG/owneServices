using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Public.Customs.AddInfoTransformationBase.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.NZ;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.NZ;

[TestedType(typeof(CopyAddInfoCountryOfExportToRealColumn))]
sealed class CopyAddInfoCountryOfExportToRealColumnTest : CopyAddInfoToRealColumnTest<CopyAddInfoCountryOfExportToRealColumn>
{
	public void TestOnlinePreUpgrade()
	{
		PrepareTestData();
		var manager = new UpgradeManagerForTestWithOutputBuffer();
		var transform = new CopyAddInfoCountryOfExportToRealColumn();
		transform.Initialise(null, manager);
		transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
		AssertTransformationResultsCore();
		transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
		AssertTransformationResultsCore();
		Assert(DbObjectCreator.ObjectExists(Db.Connection, transform.GetTriggerName()));
		Assert(DbObjectCreator.ObjectExists(Db.Connection, transform.GetClusterKeyTriggerName(JobComInvoiceHeaderSchema.JZ_ClusterKey.Name)));
		AssertEquals($"Trigger {TransformationToTest.GetTriggerName()} should have been created", true, DbObjectCreator.ObjectExists(Db.Connection, TransformationToTest.GetTriggerName()));
		AssertEquals($"Trigger {TransformationToTest.GetClusterKeyTriggerName(JobComInvoiceHeaderSchema.JZ_ClusterKey.Name)} should have been created", true, DbObjectCreator.ObjectExists(Db.Connection, TransformationToTest.GetClusterKeyTriggerName(JobComInvoiceHeaderSchema.JZ_ClusterKey.Name)));
	}

	public void TestOffLinePostUpgrade()
	{
		PrepareTestData();
		var manager = new UpgradeManagerForTestWithOutputBuffer();
		var transform = new CopyAddInfoCountryOfExportToRealColumn();
		transform.CreateOffLineProcessingTable(Db.Connection, Db.Connection.CurrentDatabase, true);
		var sqlText = $@"
INSERT INTO {transform.OffLineProcessingTableName}
SELECT JZ_PK, JZ_ClusterKey
FROM JobComInvoiceHeader
WHERE JZ_InvoiceDisplaySequence BETWEEN 1 AND 10
";
		Db.Connection.ExecuteNonQuery(sqlText);
		transform.Initialise(null, manager);
		transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
		AssertTransformationResultsCore();
		transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
		AssertTransformationResultsCore();
		AssertEquals($"Trigger {TransformationToTest.GetTriggerName()} should have been dropped", false, DbObjectCreator.ObjectExists(Db.Connection, TransformationToTest.GetTriggerName()));
		AssertEquals($"Trigger {TransformationToTest.GetClusterKeyTriggerName(JobComInvoiceHeaderSchema.JZ_ClusterKey.Name)} should have been dropped", false, DbObjectCreator.ObjectExists(Db.Connection, TransformationToTest.GetClusterKeyTriggerName(JobComInvoiceHeaderSchema.JZ_ClusterKey.Name)));
	}

	protected override void AssertTransformationResults()
	{
		AssertTransformationResultsCore();
		AssertEquals($"Trigger {TransformationToTest.GetTriggerName()} should have been dropped", false, DbObjectCreator.ObjectExists(Db.Connection, TransformationToTest.GetTriggerName()));
		AssertEquals($"Trigger {TransformationToTest.GetClusterKeyTriggerName(JobComInvoiceHeaderSchema.JZ_ClusterKey.Name)} should have been dropped", false, DbObjectCreator.ObjectExists(Db.Connection, TransformationToTest.GetClusterKeyTriggerName(JobComInvoiceHeaderSchema.JZ_ClusterKey.Name)));
	}

	void AssertTransformationResultsCore()
	{
		var values = new List<(int ClusterKey, string AddInfo, string CountryOfExport)>();
		var sqlText = @"SELECT JZ_InvoiceDisplaySequence, JZ_AddInfo, JZ_RN_NKCountryOfExport FROM JobComInvoiceHeader ORDER BY JZ_InvoiceDisplaySequence ASC";
		using (var cmd = Db.Connection.Command(sqlText))
		using (var reader = cmd.ExecuteReader())
		{
			while (reader.Read())
			{
				values.Add((
					reader.GetInt16(0),
					reader.GetString(1),
					reader.GetString(2)
				));
			}
		}
		AssertContainsExactElementsInExactOrder(new[] {
			(1, "RN_NKCountryOfExport=ER", ""),
			(2, "RN_NKCountryOfExport=ER", "ER"),
			(3, "RN_NKCountryOfExport=ER", "AU"),
			(4, "SomethingElse=ER", ""),
			(5, "", ""),
			(6, "RN_NKCountryOfExport=ER", ""),
			(7, "RN_NKCountryOfExport=ER", "ER"),
			(8, "RN_NKCountryOfExport=ER", "AU"),
			(9, "SomethingElse=ER", ""),
			(10, "", ""),
		}, values);
	}

	protected override DataTransformation GetNewTestTransformationInstance()
	{
		var manager = new UpgradeManagerForTestWithOutputBuffer();
		var transform = new CopyAddInfoCountryOfExportToRealColumn();
		transform.Initialise(null, manager);
		return transform;
	}

	protected override void PrepareTestData()
	{
		var sqlText = @"
DECLARE @CNCompanyPK UNIQUEIDENTIFIER = NEWID();
DECLARE @NZCompanyPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES (@CNCompanyPK, 'DCN', 'CN company', 'CN', 'CNY', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES (@NZCompanyPK, 'DNZ', 'NZ company', 'NZ', 'NZD', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
DECLARE @CNBranchPK UNIQUEIDENTIFIER = NEWID();
DECLARE @NZBranchPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES(@CNBranchPK, @CNCompanyPK, 'BCN', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES(@NZBranchPK, @NZCompanyPK, 'BNZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
DECLARE @CNDecPK UNIQUEIDENTIFIER = NEWID();
DECLARE @NZDecPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES (@CNDecPK, @CNBranchPK, @CNCompanyPK, 1, 'CN', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES (@NZDecPK, @NZBranchPK, @NZCompanyPK, 2, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
-- Invoices Attached to a Declaration
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_InvoiceDisplaySequence, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo, JZ_RN_NKCountryOfExport) VALUES (NEWID(), @CNDecPK, 1, 1, 'CN', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'RN_NKCountryOfExport=ER', '');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_InvoiceDisplaySequence, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo, JZ_RN_NKCountryOfExport) VALUES (NEWID(), @NZDecPK, 2, 2, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'RN_NKCountryOfExport=ER', '');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_InvoiceDisplaySequence, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo, JZ_RN_NKCountryOfExport) VALUES (NEWID(), @NZDecPK, 2, 3, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'RN_NKCountryOfExport=ER', 'AU');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_InvoiceDisplaySequence, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo, JZ_RN_NKCountryOfExport) VALUES (NEWID(), @NZDecPK, 2, 4, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'SomethingElse=ER', '');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_InvoiceDisplaySequence, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo, JZ_RN_NKCountryOfExport) VALUES (NEWID(), @NZDecPK, 2, 5, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', '', '');
-- Standalone Invoices
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_GB, JZ_ClusterKey, JZ_InvoiceDisplaySequence, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo, JZ_RN_NKCountryOfExport) VALUES (NEWID(), @CNBranchPK, 3, 6, 'CN', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'RN_NKCountryOfExport=ER', '');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_GB, JZ_ClusterKey, JZ_InvoiceDisplaySequence, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo, JZ_RN_NKCountryOfExport) VALUES (NEWID(), @NZBranchPK, 4, 7, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'RN_NKCountryOfExport=ER', '');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_GB, JZ_ClusterKey, JZ_InvoiceDisplaySequence, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo, JZ_RN_NKCountryOfExport) VALUES (NEWID(), @NZBranchPK, 5, 8, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'RN_NKCountryOfExport=ER', 'AU');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_GB, JZ_ClusterKey, JZ_InvoiceDisplaySequence, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo, JZ_RN_NKCountryOfExport) VALUES (NEWID(), @NZBranchPK, 6, 9, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'SomethingElse=ER', '');
INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_GB, JZ_ClusterKey, JZ_InvoiceDisplaySequence, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_AddInfo, JZ_RN_NKCountryOfExport) VALUES (NEWID(), @NZBranchPK, 7, 10, 'NZ', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', '', '');
";
		Db.Connection.ExecuteNonQuery(sqlText);
	}

	protected override void SetUp()
	{
		base.SetUp();
		UpdateAlreadyProcessedData([]);
		templateDbCreator = new TablePreSynchroniserTestTemplateDbCreator(UpgUtils.GetTemplateDbName(), JobComInvoiceHeaderSchema.Constants.TableName, new[]
		{
			$"{JobComInvoiceHeaderSchema.Constants.JZ_ClusterKey} INT DEFAULT 0 NOT NULL",
			$"{JobComInvoiceHeaderSchema.Constants.JZ_RN_NKCountryOfExport} VARCHAR(2) DEFAULT '' NOT NULL"
		});
		templateDbCreator.CreateDropExisting();
		TablePreSynchroniser.CreatePreAddDb_ForTest();
	}

	protected override void OnAfterBaseTestCaseRunBare()
	{
	}

	IAuxiliaryDbCreator templateDbCreator;
}
