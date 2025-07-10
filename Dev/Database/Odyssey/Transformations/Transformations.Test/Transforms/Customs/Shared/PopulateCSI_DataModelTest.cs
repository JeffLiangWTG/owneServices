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

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(PopulateCSI_DataModel))]
	sealed class PopulateCSI_DataModelTest : DataTransformationTestCase
	{
		public void TestTransformation_TempTrigger()
		{
			using (TransformationRunTwiceTestContextSetupAndDispose())
			{
				GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
				AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, CusSupportingInfoSchema.Constants.TableName, PopulateCSI_DataModel.SyncTriggerName));

				AddCusSupportingInfoWithParent();
				AssertTransformationResults();

				AssertExceptionThrown<SqlException>("Cannot insert/update CusSupportingInfo without CSI_DataModel",
					"Attempt to insert/update without [CSI_DataModel] for [CusSupportingInfo].\r\nThe transaction ended in the trigger. The batch has been aborted.",
					() => TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
						VALUES (NEWID(), NEWID(), 'JI', 'ABC', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"));
			}
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
				("CEI", "AI"),
				("CEI", "CN"),
				("CEI", "US"),
				("CEI", "US"),
				("CEI", "CH"),
				("CEI", "CH"),
				("CI", "AI"),
				("CI", "CN"),
				("CI", "US"),
				("CI", "US"),
				("CI", "CH"),
				("CI", "CH"),
				("CL", "AI"),
				("CL", "CN"),
				("CL", "US"),
				("CL", "US"),
				("CL", "CH"),
				("CL", "CH"),
				("JE", "AI"),
				("JE", "CN"),
				("JE", "US"),
				("JE", "US"),
				("JE", "CH"),
				("JE", "CH"),
				("JZ", "AI"),
				("JZ", "CN"),
				("JZ", "US"),
				("JZ", "US"),
				("JZ", "CH"),
				("JZ", "CH"),
				("JI", "AI"),
				("JI", "CN"),
				("JI", "US"),
				("JI", "US"),
				("JI", "CH"),
				("JI", "CH"),
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
			foreach (var countryCode in new[] { "AI", "CN", "PR", "US", "LI", "CH" })
			{
				var sqlText = $@"
DECLARE @CompanyPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES (@CompanyPK, 'D{countryCode}', 'AU company', '{countryCode}', 'XCD', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
DECLARE @BranchPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES(@BranchPK, @CompanyPK, 'B{countryCode}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

DECLARE @JobDeclarationPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES (@JobDeclarationPK, @BranchPK, @CompanyPK, {clusterKey}, '{countryCode}', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
VALUES (NEWID(), @JobDeclarationPK, 'JE', 'ABC', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');

DECLARE @JobComInvoiceHeaderPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
VALUES (@JobComInvoiceHeaderPK, @JobDeclarationPK, {clusterKey}, '{countryCode}', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
VALUES (NEWID(), @JobComInvoiceHeaderPK, 'JZ', 'ABC', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');

DECLARE @JobComInvoiceLinePK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_DataModel, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser)
VALUES (@JobComInvoiceLinePK, @JobComInvoiceHeaderPK, {clusterKey}, '{countryCode}', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
VALUES (NEWID(), @JobComInvoiceLinePK, 'JI', 'ABC', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');

DECLARE @CusEntryInstructionPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_ClusterKey, CEI_Style, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
VALUES (@CusEntryInstructionPK, '{countryCode}', @JobDeclarationPK, {clusterKey}, 'EX8', GETDATE(), '~BP', GETDATE(), '~BP');
INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
VALUES (NEWID(), @CusEntryInstructionPK, 'CEI', 'ABC', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');

DECLARE @CusEntryHeaderPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_EntryStatus, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@CusEntryHeaderPK, '{countryCode}', @JobDeclarationPK, 'CLR', {clusterKey}, GETDATE(), '~BP', GETDATE(), '~BP');
DECLARE @CusEntryLinePK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_ClusterKey, CL_CustomsValue, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
VALUES (@CusEntryLinePK, '{countryCode}', @CusEntryHeaderPK, {clusterKey}, 100, GETDATE(), '~BP', GETDATE(), '~BP');
INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
VALUES (NEWID(), @CusEntryLinePK, 'CL', 'ABC', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');

DECLARE @OrgSupplierPartPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.OrgSupplierPart(OP_PK, OP_PartNum, OP_SystemCreateTimeUtc, OP_SystemCreateUser, OP_SystemLastEditTimeUtc, OP_SystemLastEditUser)
VALUES (@OrgSupplierPartPK, 'PARTNO', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
DECLARE @CusClassPartPivotPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.CusClassPartPivot (CI_PK, CI_OP, CI_RN_NKCountry, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser)
VALUES (@CusClassPartPivotPK, @OrgSupplierPartPK, '{countryCode}', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
VALUES (NEWID(), @CusClassPartPivotPK, 'CI', 'ABC', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
";
				Db.Connection.ExecuteNonQuery(sqlText);
				clusterKey++;
			}

			Db.Connection.ExecuteNonQuery(@"INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
VALUES (NEWID(), NEWID(), 'TW1', 'ABC', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');");
		}

		void AddCusSupportingInfoWithoutParent()
		{
			string[] parentTableCodes = ["CEI", "CI", "CL", "JE", "JI", "JZ"];

			foreach (var parentTableCode in parentTableCodes)
			{
				for (var i = 0; i < 4; i++)
				{
					Db.Connection.ExecuteNonQuery(@$"INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
VALUES (NEWID(), NEWID(), '{parentTableCode}', 'ABC', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');");
				}
			}
		}

		protected override void AssertPreConditions()
		{
			AssertEquals(61, Db.Connection.ExecuteScalar("SELECT count(*) FROM dbo.CusSupportingInfo"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new PopulateCSI_DataModel(8);
			transform.Initialise(null, manager);
			return transform;
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
}
