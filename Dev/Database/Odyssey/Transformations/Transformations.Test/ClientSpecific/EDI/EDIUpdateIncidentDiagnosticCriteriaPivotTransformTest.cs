using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(EDIUpdateIncidentDiagnosticCriteriaPivotTransform))]
	public class EDIUpdateIncidentDiagnosticCriteriaPivotTransformTest : DataTransformationTestCase
	{
		Guid parentGuid;
		Guid pivot1Guid;
		Guid pivot2Guid;
		Guid pivot3Guid;

		protected override void AssertTransformationResults()
		{
			AssertEquals(1, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.IncidentDiagnosticCriteriaPivot"));

			var loadIncidentDiagnosticCriteriaPivot = @"
SELECT IMV_ParentID, IMV_ParentTableCode FROM dbo.IncidentDiagnosticCriteriaPivot;
;";
			using (var reader = Db.Connection.Command(loadIncidentDiagnosticCriteriaPivot).ExecuteReader())
			{
				while (reader.Read())
				{
					var parentPk = reader.GetGuid(0);
					var parentTableCode = reader.GetString(1);
					AssertEquals("Rename transform should move IMV_IM_Incident to IMV_ParentID", parentGuid, parentPk);
					AssertEquals("Post upgrade transform should add IM IMV_ParentTableCode to all existing rows", "IM", parentTableCode);
				}
			}
		}

		protected void AssertIMV_ParentTableCodeColumnAlreadyExistsTransformationResults()
		{
			AssertEquals(3, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.IncidentDiagnosticCriteriaPivot"));

			var loadIncidentDiagnosticCriteriaPivot1 = @$"
SELECT IMV_ParentID, IMV_ParentTableCode FROM dbo.IncidentDiagnosticCriteriaPivot WHERE IMV_PK = '{pivot1Guid}';
;";
			using (var reader = Db.Connection.Command(loadIncidentDiagnosticCriteriaPivot1).ExecuteReader())
			{
				while (reader.Read())
				{
					var parentPk = reader.GetGuid(0);
					var parentTableCode = reader.GetString(1);
					AssertEquals("Rename transform should move IMV_IM_Incident to IMV_ParentID", parentGuid, parentPk);
					AssertEquals("Post upgrade transform should add IM IMV_ParentTableCode to all existing empty rows", "IM", parentTableCode);
				}
			}

			var loadIncidentDiagnosticCriteriaPivot2 = @$"
SELECT IMV_ParentID, IMV_ParentTableCode FROM dbo.IncidentDiagnosticCriteriaPivot WHERE IMV_PK = '{pivot2Guid}';
;";
			using (var reader = Db.Connection.Command(loadIncidentDiagnosticCriteriaPivot2).ExecuteReader())
			{
				while (reader.Read())
				{
					var parentPk = reader.GetGuid(0);
					var parentTableCode = reader.GetString(1);
					AssertEquals("Rename transform should move IMV_IM_Incident to IMV_ParentID", parentGuid, parentPk);
					AssertEquals("Post upgrade transform should not add IM IMV_ParentTableCode to already populated rows", "ING", parentTableCode);
				}
			}

			var loadIncidentDiagnosticCriteriaPivot3 = @$"
SELECT IMV_ParentID, IMV_ParentTableCode FROM dbo.IncidentDiagnosticCriteriaPivot WHERE IMV_PK = '{pivot3Guid}';
;";
			using (var reader = Db.Connection.Command(loadIncidentDiagnosticCriteriaPivot3).ExecuteReader())
			{
				while (reader.Read())
				{
					var parentPk = reader.GetGuid(0);
					var parentTableCode = reader.GetString(1);
					AssertEquals("Rename transform should move IMV_IM_Incident to IMV_ParentID", parentGuid, parentPk);
					AssertEquals("Post upgrade transform should change IMV_ParentTableCode to IM if it has an invalid value", "IM", parentTableCode);
				}
			}
		}

		public void TestRunAndAssertResultsTwice_IMV_ParentTableCodeColumnAlreadyExists()
		{
			TestRunAndAssertResultsTwice<object>(() =>
			{
				PrepareTestDataAndAddIMV_ParentTableCode();
				return null;
			},
				_ => AssertPreConditions(),
				_ => AssertIMV_ParentTableCodeColumnAlreadyExistsTransformationResults());
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new EDIUpdateIncidentDiagnosticCriteriaPivotTransform();
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "IncidentDiagnosticCriteriaPivot", @"
CREATE TABLE dbo.IncidentDiagnosticCriteriaPivot
(
   [IMV_PK] UNIQUEIDENTIFIER NOT NULL,
   [IMV_IMD_DiagnosticCriteria] UNIQUEIDENTIFIER NOT NULL,
   [IMV_IM_Incident] UNIQUEIDENTIFIER NOT NULL,
   [IMV_Status] VARCHAR(3) NOT NULL DEFAULT '',
   [IMV_SystemCreateTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMV_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [IMV_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMV_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
");
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "IncidentDiagnosticCriteria", @"
CREATE TABLE dbo.IncidentDiagnosticCriteria
(
   [IMD_PK] UNIQUEIDENTIFIER NOT NULL,
   [IMD_Type] CHAR(3) NOT NULL DEFAULT '',
   [IMD_InternalSupportNote] NVARCHAR(MAX) NOT NULL DEFAULT '',
   [IMD_SystemCreateTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMD_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [IMD_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMD_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
");
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "IncidentMain", @"
CREATE TABLE dbo.IncidentMain
(
   [IM_PK] UNIQUEIDENTIFIER NOT NULL,
   [IM_IncidentNumber] VARCHAR(20) NOT NULL DEFAULT '' ,
   [IM_Source] CHAR(3) NOT NULL DEFAULT '' ,
   [IM_SystemCreateTimeUtc] SMALLDATETIME NULL,
   [IM_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_SystemLastEditTimeUtc] DATETIME NULL,
   [IM_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT ''
);
");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "IncidentDiagnosticCriteriaPivot", "IMV_IM_Incident", "UNIQUEIDENTIFIER");
			parentGuid = Guid.NewGuid();
			pivot1Guid = Guid.NewGuid();
			var incidentDiagnosticCriteriaPK = Guid.NewGuid();

			var sql = $@"
INSERT INTO [IncidentMain](
	[IM_PK],
	[IM_IncidentNumber],
	[IM_Source],
	[IM_SystemCreateTimeUtc],
	[IM_SystemCreateUser],
	[IM_SystemLastEditTimeUtc],
	[IM_SystemLastEditUser])
VALUES
('{parentGuid}', 'CS99991112', 'PRJ', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E');

INSERT INTO [IncidentDiagnosticCriteria](
	[IMD_PK],
	[IMD_Type],
	[IMD_InternalSupportNote],
	[IMD_SystemCreateTimeUtc],
	[IMD_SystemCreateUser],
	[IMD_SystemLastEditTimeUtc],
	[IMD_SystemLastEditUser])
VALUES
('{incidentDiagnosticCriteriaPK}', 'INV', 'Test', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E');

INSERT INTO [IncidentDiagnosticCriteriaPivot](
	[IMV_PK],
	[IMV_IMD_DiagnosticCriteria],
	[IMV_IM_Incident],
	[IMV_Status],
	[IMV_SystemCreateTimeUtc],
	[IMV_SystemCreateUser],
	[IMV_SystemLastEditTimeUtc],
	[IMV_SystemLastEditUser]
)
VALUES
('{pivot1Guid}', '{incidentDiagnosticCriteriaPK}', '{parentGuid}', 'INV', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E');
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		protected void PrepareTestDataAndAddIMV_ParentTableCode()
		{
			PrepareTestData();
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "IncidentDiagnosticCriteriaPivot", "IMV_ParentTableCode", "VARCHAR(3)", "''");

			var incidentDiagnosticCriteria2PK = Guid.NewGuid();
			var incidentDiagnosticCriteria3PK = Guid.NewGuid();
			var pivot2Guid = Guid.NewGuid();
			var pivot3Guid = Guid.NewGuid();
			var sql = $@"
INSERT INTO [IncidentMain](
	[IM_PK],
	[IM_IncidentNumber],
	[IM_Source],
	[IM_SystemCreateTimeUtc],
	[IM_SystemCreateUser],
	[IM_SystemLastEditTimeUtc],
	[IM_SystemLastEditUser])
VALUES
('{parentGuid}', 'CS99991112', 'PRJ', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E');

INSERT INTO [IncidentDiagnosticCriteria](
	[IMD_PK],
	[IMD_Type],
	[IMD_InternalSupportNote],
	[IMD_SystemCreateTimeUtc],
	[IMD_SystemCreateUser],
	[IMD_SystemLastEditTimeUtc],
	[IMD_SystemLastEditUser])
VALUES
('{incidentDiagnosticCriteria2PK}', 'INV', 'Test2', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E');

INSERT INTO [IncidentDiagnosticCriteria](
	[IMD_PK],
	[IMD_Type],
	[IMD_InternalSupportNote],
	[IMD_SystemCreateTimeUtc],
	[IMD_SystemCreateUser],
	[IMD_SystemLastEditTimeUtc],
	[IMD_SystemLastEditUser])
VALUES
('{incidentDiagnosticCriteria3PK}', 'INV', 'Test3', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E');

INSERT INTO [IncidentDiagnosticCriteriaPivot](
	[IMV_PK],
	[IMV_IMD_DiagnosticCriteria],
	[IMV_IM_Incident],
	[IMV_Status],
	[IMV_ParentTableCode],
	[IMV_SystemCreateTimeUtc],
	[IMV_SystemCreateUser],
	[IMV_SystemLastEditTimeUtc],
	[IMV_SystemLastEditUser]
)
VALUES
('{pivot2Guid}', '{incidentDiagnosticCriteria2PK}', '{parentGuid}', 'INV', 'ING', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E');

INSERT INTO [IncidentDiagnosticCriteriaPivot](
	[IMV_PK],
	[IMV_IMD_DiagnosticCriteria],
	[IMV_IM_Incident],
	[IMV_Status],
	[IMV_ParentTableCode],
	[IMV_SystemCreateTimeUtc],
	[IMV_SystemCreateUser],
	[IMV_SystemLastEditTimeUtc],
	[IMV_SystemLastEditUser]
)
VALUES
('{pivot3Guid}', '{incidentDiagnosticCriteria3PK}', '{parentGuid}', 'INV', 'ZZZ', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E');
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
