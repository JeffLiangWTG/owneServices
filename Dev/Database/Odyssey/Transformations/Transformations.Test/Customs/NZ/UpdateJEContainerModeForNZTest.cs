using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.NZ;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.NZ
{
	[TestedType(typeof(UpdateJEContainerModeForNZ))]
	internal class UpdateJEContainerModeForNZTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateJEContainerModeForNZ();

		public override string[] expectedIndex => new[] { "NONCLUSTERED INDEX [_WTG__Update NZ declaration with wrong containerMode due to WI00799676._1] ON [dbo].[JobDeclaration] ([JE_ContainerMode]) INCLUDE ([JE_JS], [JE_OverrideFreightDefaults], [JE_PK], [JE_SystemLastEditTimeUtc], [JE_SystemLastEditUser]) WHERE ([JE_DataModel]='NZ') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)" };

		protected override void AssertTransformationResults()
		{
			var resultList = new List<Tuple<string, string>>();

			var sql = string.Format(@"
			SELECT
				JE_ContainerMode, JE_DeclarationReference
			FROM
				dbo.JobDeclaration where JE_ClusterKey in (1,2,3,4,5,6,7,8,9,10);");
			TestConnection.ExecuteReader(sql, reader => resultList.Add(Tuple.Create((string)reader["JE_DeclarationReference"], (string)reader["JE_ContainerMode"])));

			AssertContainsExactElementsInAnyOrder(
[
				Tuple.Create("JOB1", "CNT"),
				Tuple.Create("JOB2", "CNT"),
				Tuple.Create("JOB3", "FCL"),
				Tuple.Create("JOB4", ""),
				Tuple.Create("JOB5", "FCL"),
				Tuple.Create("JOB6", ""),
				Tuple.Create("JOB7", "CNT"),
				Tuple.Create("JOB8", "CNT"),
				Tuple.Create("JOB9", ""),
				Tuple.Create("JOB0", "FCL"),
			], resultList);
		}

		protected override void PrepareTestData()
		{
			var sql = $@"
DECLARE @nzCompanyPK UNIQUEIDENTIFIER = NEWID(),
		@frCompanyPK UNIQUEIDENTIFIER = NEWID();

DECLARE @nzBranchPK UNIQUEIDENTIFIER = NEWID(),
		@frBranchPK UNIQUEIDENTIFIER = NEWID();

DECLARE @shipmentPK0 UNIQUEIDENTIFIER = NEWID(),
		@shipmentPK1 UNIQUEIDENTIFIER = NEWID(),
		@shipmentPK2 UNIQUEIDENTIFIER = NEWID(),
		@shipmentPK3 UNIQUEIDENTIFIER = NEWID(),
		@shipmentPK4 UNIQUEIDENTIFIER = NEWID();

DECLARE	@declarationPK0 UNIQUEIDENTIFIER = NEWID(),
		@declarationPK1 UNIQUEIDENTIFIER = NEWID(),
		@declarationPK2 UNIQUEIDENTIFIER = NEWID(),
		@declarationPK3 UNIQUEIDENTIFIER = NEWID(),
		@declarationPK4 UNIQUEIDENTIFIER = NEWID(),
		@declarationPK5 UNIQUEIDENTIFIER = NEWID(),
		@declarationPK6 UNIQUEIDENTIFIER = NEWID(),
		@declarationPK7 UNIQUEIDENTIFIER = NEWID(),
		@declarationPK8 UNIQUEIDENTIFIER = NEWID(),
		@declarationPK9 UNIQUEIDENTIFIER = NEWID();

DECLARE	@cusContainerPK0 UNIQUEIDENTIFIER = NEWID(),
		@cusContainerPK1 UNIQUEIDENTIFIER = NEWID(),
		@cusContainerPK2 UNIQUEIDENTIFIER = NEWID(),
		@cusContainerPK3 UNIQUEIDENTIFIER = NEWID(),
		@cusContainerPK4 UNIQUEIDENTIFIER = NEWID(),
		@cusContainerPK5 UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.GlbCompany
	(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
VALUES
	(@frCompanyPK, 'FR', 'EUR', 'DFR', 'FR company', GETDATE(), '~BP', GETDATE(), '~BP'),
	(@nzCompanyPK, 'NZ', 'NZY', 'DCN', 'NZ company', GETDATE(), '~BP', GETDATE(), '~BP');

INSERT INTO dbo.GlbBranch
	(GB_PK, GB_RN_NKCountryCode, GB_Code, GB_GC, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
VALUES
	(@frBranchPK, 'FR', 'PAR', @frCompanyPK, GETDATE(), '~BP', GETDATE(), '~BP'),
	(@nzBranchPK, 'NZ', 'NJG', @nzCompanyPK, GETDATE(), '~BP', GETDATE(), '~BP');

	INSERT INTO JobShipment
	(JS_PK, JS_UniqueConsignRef, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser)
VALUES
	(@shipmentPK0, 1, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@shipmentPK1, 2, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@shipmentPK2, 3, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@shipmentPK3, 4, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@shipmentPK4, 5, getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO JobDeclaration
	(JE_PK, JE_ClusterKey, JE_DataModel, JE_DeclarationReference, JE_GC, JE_GB, JE_JS, JE_ContainerMode, JE_OverrideFreightDefaults, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES
	(@declarationPK0, 1, 'NZ', 'JOB1', @nzCompanyPK, @nzBranchPK, @shipmentPK0, '', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@declarationPK1, 2, 'NZ', 'JOB2', @nzCompanyPK, @nzBranchPK, @shipmentPK1, 'FCL', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@declarationPK2, 3, 'NZ', 'JOB3', @nzCompanyPK, @nzBranchPK, @shipmentPK2, 'FCL', 0, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@declarationPK3, 4, 'NZ', 'JOB4', @nzCompanyPK, @nzBranchPK, @shipmentPK3, 'FCL', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@declarationPK4, 5, 'NZ', 'JOB5', @nzCompanyPK, @nzBranchPK, @shipmentPK4, 'FCL', 0, getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO JobDeclaration
	(JE_PK, JE_ClusterKey, JE_DataModel, JE_DeclarationReference, JE_GC, JE_GB, JE_ContainerMode, JE_OverrideFreightDefaults, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES
	(@declarationPK5, 6, 'NZ', 'JOB6', @nzCompanyPK, @nzBranchPK, 'FCL', 0, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@declarationPK6, 7, 'NZ', 'JOB7', @nzCompanyPK, @nzBranchPK, 'FCL', 0, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@declarationPK7, 8, 'NZ', 'JOB8', @nzCompanyPK, @nzBranchPK, '', 0, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@declarationPK8, 9, 'FR', 'JOB9', @frCompanyPK, @frBranchPK, '', 0, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@declarationPK9, 10, 'FR', 'JOB0', @frCompanyPK, @frBranchPK, 'FCL', 0, getutcdate(), '~BP', getutcdate(), '~BP');

	INSERT INTO CusContainer
	(CO_PK, CO_DataModel, CO_JE, CO_ClusterKey, CO_SystemCreateTimeUtc, CO_SystemCreateUser, CO_SystemLastEditTimeUtc, CO_SystemLastEditUser)
VALUES
	(@cusContainerPK0, 'NZ', @declarationPK0, 1, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@cusContainerPK1, 'NZ', @declarationPK1, 2, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@cusContainerPK2, 'NZ', @declarationPK2, 3, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@cusContainerPK3, 'NZ', @declarationPK6, 4, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@cusContainerPK4, 'NZ', @declarationPK7, 5, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@cusContainerPK5, 'FR', @declarationPK8, 6, getutcdate(), '~BP', getutcdate(), '~BP');";

			var cmd = Db.Connection.Command(sql);
			cmd.ExecuteNonQuery();
		}
	}
}
