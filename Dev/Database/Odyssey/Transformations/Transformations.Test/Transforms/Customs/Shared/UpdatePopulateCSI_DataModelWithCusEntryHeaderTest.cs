using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared;

[TestedType(typeof(UpdatePopulateCSI_DataModelWithCusEntryHeader))]
sealed class UpdatePopulateCSI_DataModelWithCusEntryHeaderTest : DataTransformationTestCase
{
	public void TestOnlinePreUpgrade()
	{
		using (TransformationRunTwiceTestContextSetupAndDispose())
		{
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Trigger should be created.", expected: true, DbObjectCreator.TriggerExists(TestConnection, CusSupportingInfoSchema.Constants.TableName, UpdatePopulateCSI_DataModelWithCusEntryHeader.SyncTriggerName));

			AddCusSupportingInfoWithParent();
			AssertTransformationResults();
		}
	}

	protected override DataTransformation GetNewTestTransformationInstance()
	{
		var manager = new UpgradeManagerForTestWithOutputBuffer();
		var transform = new UpdatePopulateCSI_DataModelWithCusEntryHeader(8);
		transform.Initialise(null, manager);
		return transform;
	}

	protected override void AssertPreConditions()
	{
		AssertEquals(12, Db.Connection.ExecuteScalar("SELECT count(*) FROM dbo.CusSupportingInfo"));
	}

	protected override void AssertTransformationResults()
	{
		var values = new List<(string, string)>();
		var sqlText = @"SELECT CSI_ParentTableCode, CSI_DataModel FROM dbo.CusSupportingInfo ORDER BY CSI_ParentTableCode";

		using (var cmd = Db.Connection.Command(sqlText))
		using (var reader = cmd.ExecuteReader())
		{
			while (reader.Read())
			{
				values.Add((reader.GetString(0), reader.GetString(1)));
			}
		}

		AssertContainsExactElementsInAnyOrder(new[] {
				("CH", "AI"),
				("CH", "CN"),
				("CH", "US"),
				("CH", "US"),
				("CH", "CH"),
				("CH", "CH"),
				("CH", "ES"),
				("TW1", ""),
			}, values);
	}

	protected override void PrepareTestData()
	{
		AddCusSupportingInfoWithParent();
		AddCusSupportingInfoWithoutParent();
	}

	void AddCusSupportingInfoWithParent()
	{
		var clusterKey = 1;
		foreach (var countryCode in new[] { "AI", "CN", "PR", "US", "LI", "CH", "ES" })
		{
			var sqlText = $@"
DECLARE @CompanyPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES (@CompanyPK, 'D{countryCode}', 'Company Name', '{countryCode}', 'XCD', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
DECLARE @BranchPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES(@BranchPK, @CompanyPK, 'B{countryCode}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

DECLARE @JobDeclarationPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES (@JobDeclarationPK, @BranchPK, @CompanyPK, {clusterKey}, '{countryCode}', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');

DECLARE @CusEntryHeaderPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_EntryStatus, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@CusEntryHeaderPK, '{countryCode}', @JobDeclarationPK, 'CLR', {clusterKey}, GETDATE(), '~BP', GETDATE(), '~BP');
INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
VALUES (NEWID(), @CusEntryHeaderPK, 'CH', 'ABC', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
";
			Db.Connection.ExecuteNonQuery(sqlText);
			clusterKey++;
		}

		Db.Connection.ExecuteNonQuery(@"INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
VALUES (NEWID(), NEWID(), 'TW1', 'ABC', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');");
	}

	void AddCusSupportingInfoWithoutParent()
	{
		for (var i = 0; i < 4; i++)
		{
			Db.Connection.ExecuteNonQuery(@$"INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
VALUES (NEWID(), NEWID(), 'CH', 'ABC', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');");
		}
	}

	protected override IDisposable TransformationRunTwiceTestContextSetupAndDispose()
	{
		DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, CusSupportingInfoSchema.Constants.SqlSchemaName, CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_DataModel);
		DBTransformationTestHelper.DropConstraintIfExists(CusSupportingInfoSchema.Constants.TableName, "Constraint_CSI_DataModel");
		DBTransformationTestHelper.DropConstraintIfExists(CusSupportingInfoSchema.Constants.TableName, "Constraint_CSI_AddInfo");
		DBTransformationTestHelper.DropDefaultConstraintFor(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_DataModel);
		DBTransformationTestHelper.DropColumnIfExists(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_DataModel);
		return DisposableAction.NoAction;
	}
}
