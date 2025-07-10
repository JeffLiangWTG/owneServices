using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI.Testing
{
	[TestedType(typeof(UpdateExpiryCountdownStartTimeUtc))]
	public class UpdateExpiryCountdownStartTimeUtcTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var transformation = new UpdateExpiryCountdownStartTimeUtc();
			transformation.Initialise(null, new DummyUpgradeManager());
			return transformation;
		}

		protected override void AssertTransformationResults()
		{
			var resultQuery = $"SELECT INC_ExpiryCountdownStartTimeUtc FROM dbo.IncidentRequest WHERE INC_PK = '{incidentRequestGuids[0]}'";
			var result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("INC_ExpiryCountdownStartTimeUtc should be updated to SL_EventTimeUtc because the incident's IM_ResolutionCode = 'CWR' and has 'IWR' SL_SE_NKEvent", new DateTime(2024, 7, 1, 13, 00, 00), result);

			resultQuery = $"SELECT INC_ExpiryCountdownStartTimeUtc FROM dbo.IncidentRequest WHERE INC_PK = '{incidentRequestGuids[1]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("INC_ExpiryCountdownStartTimeUtc should be updated to IWR event's SL_EventTimeUtc which is used over the STC event", new DateTime(2024, 7, 2, 13, 00, 00), result);

			resultQuery = $"SELECT INC_ExpiryCountdownStartTimeUtc FROM dbo.IncidentRequest WHERE INC_PK = '{incidentRequestGuids[2]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("INC_ExpiryCountdownStartTimeUtc should be updated to SL_EventTimeUtc because the incident's IM_ResolutionCode = 'CWR' and has 'to CWR' SL_Reference", new DateTime(2024, 7, 3, 13, 00, 00), result);

			resultQuery = $"SELECT INC_ExpiryCountdownStartTimeUtc FROM dbo.IncidentRequest WHERE INC_PK = '{incidentRequestGuids[3]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("INC_ExpiryCountdownStartTimeUtc should be updated to IM_ResolveTimeUtc because the incident's IM_ResolveTimeUtc is not NULL", new DateTime(2024, 7, 4, 12, 00, 00), result);

			resultQuery = $"SELECT INC_ExpiryCountdownStartTimeUtc FROM dbo.IncidentRequest WHERE INC_PK = '{incidentRequestGuids[4]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("INC_ExpiryCountdownStartTimeUtc should be updated to IM_ResolveTimeUtc because the incident's IM_ResolveTimeUtc is not NULL", new DateTime(2024, 7, 5, 12, 00, 00), result);

			resultQuery = $"SELECT INC_ExpiryCountdownStartTimeUtc FROM dbo.IncidentRequest WHERE INC_PK = '{incidentRequestGuids[5]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("INC_ExpiryCountdownStartTimeUtc should not be updated because the incident's IM_ResolveTimeUtc is NULL and IM_ResolutionCode is not 'CWR'", DBNull.Value, result);

			resultQuery = $"SELECT INC_ExpiryCountdownStartTimeUtc FROM dbo.IncidentRequest WHERE INC_PK = '{incidentRequestGuids[6]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("INC_ExpiryCountdownStartTimeUtc should not be updated because the incident's IM_ResolveTimeUtc is NULL and IM_ResolutionCode is not 'CWR'", DBNull.Value, result);

			resultQuery = $"SELECT INC_ExpiryCountdownStartTimeUtc FROM dbo.IncidentRequest WHERE INC_PK = '{incidentRequestGuids[7]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("INC_ExpiryCountdownStartTimeUtc should not be updated because the incident's IM_ResolveTimeUtc is NULL and IM_ResolutionCode is not 'CWR'", DBNull.Value, result);

			resultQuery = $"SELECT INC_ExpiryCountdownStartTimeUtc FROM dbo.IncidentRequest WHERE INC_PK = '{incidentRequestGuids[8]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("INC_ExpiryCountdownStartTimeUtc should be updated to SL_EventTimeUtc because the incident's IM_ResolutionCode = 'CWR' and has 'IWR' SL_SE_NKEvent even though IM_ResolveTimeUtc is NULL", new DateTime(2024, 7, 9, 13, 00, 00), result);

			resultQuery = $"SELECT INC_ExpiryCountdownStartTimeUtc FROM dbo.IncidentRequest WHERE INC_PK = '{incidentRequestGuids[9]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("INC_ExpiryCountdownStartTimeUtc should be updated to SL_EventTimeUtc because the incident's IM_ResolutionCode = 'CWR' and has 'to CWR' SL_Reference even though IM_ResolveTimeUtc is NULL", new DateTime(2024, 7, 10, 13, 00, 00), result);
		}

		readonly List<Guid> incidentMainGuids = new List<Guid>
		{
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
		};

		readonly List<Guid> incidentRequestGuids = new List<Guid>
		{
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
		};

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "IncidentMain", @"
CREATE TABLE dbo.IncidentMain
(
	[IM_PK] UNIQUEIDENTIFIER NOT NULL,
	[IM_INC_Request] UNIQUEIDENTIFIER NULL,
	[IM_IncidentNumber] VARCHAR(20) NOT NULL DEFAULT '' ,
	[IM_IncidentType] VARCHAR(3) NOT NULL DEFAULT '' ,
	[IM_Status] CHAR(3) NOT NULL DEFAULT '' ,
	[IM_ResolutionCode] CHAR(3) NOT NULL DEFAULT '' ,
	[IM_ResolveTimeUtc] DATETIME NULL,
	[IM_SystemCreateTimeUtc] SMALLDATETIME NULL,
	[IM_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '' ,
	[IM_SystemLastEditTimeUtc] DATETIME NULL,
	[IM_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT ''
);");

			var dataQuery = $@"
INSERT INTO dbo.IncidentRequest (
	[INC_PK],
	[INC_IncidentNumber],
	[INC_Type],
	[INC_SubType],
	[INC_Criticality],
	[INC_Status],
	[INC_Summary],
	[INC_Details],
	[INC_SystemCreateTimeUtc],
	[INC_SystemCreateUser],
	[INC_SystemLastEditTimeUtc],
	[INC_SystemLastEditUser],
	[INC_ExpiryCountdownStartTimeUtc])
VALUES
('{incidentRequestGuids[0]}', 'CS99990001', 'ENT', 'VAL', 'CR4', 'APR', 'Summary', 'Details', '2024-06-01 12:00:00.000', '~BP', '2024-07-01 12:00:00.000', '~BP', NULL),
('{incidentRequestGuids[1]}', 'CS99990002', 'ENT', 'VAL', 'CR4', 'APR', 'Summary', 'Details', '2024-06-02 12:00:00.000', '~BP', '2024-07-02 12:00:00.000', '~BP', NULL),
('{incidentRequestGuids[2]}', 'CS99990003', 'ENT', 'VAL', 'CR4', 'APR', 'Summary', 'Details', '2024-06-03 12:00:00.000', '~BP', '2024-07-03 12:00:00.000', '~BP', NULL),
('{incidentRequestGuids[3]}', 'CS99990004', 'ENT', 'VAL', 'CR4', 'APR', 'Summary', 'Details', '2024-06-04 12:00:00.000', '~BP', '2024-07-04 12:00:00.000', '~BP', NULL),
('{incidentRequestGuids[4]}', 'CS99990005', 'ENT', 'VAL', 'CR4', 'APR', 'Summary', 'Details', '2024-06-05 12:00:00.000', '~BP', '2024-07-05 12:00:00.000', '~BP', NULL),
('{incidentRequestGuids[5]}', 'CS99990006', 'ENT', 'VAL', 'CR4', 'APR', 'Summary', 'Details', '2024-06-06 12:00:00.000', '~BP', '2024-07-06 12:00:00.000', '~BP', NULL),
('{incidentRequestGuids[6]}', 'CS99990007', 'ENT', 'VAL', 'CR4', 'APR', 'Summary', 'Details', '2024-06-07 12:00:00.000', '~BP', '2024-07-07 12:00:00.000', '~BP', NULL),
('{incidentRequestGuids[7]}', 'CS99990008', 'ENT', 'VAL', 'CR4', 'APR', 'Summary', 'Details', '2024-06-08 12:00:00.000', '~BP', '2024-07-08 12:00:00.000', '~BP', NULL),
('{incidentRequestGuids[8]}', 'CS99990009', 'ENT', 'VAL', 'CR4', 'APR', 'Summary', 'Details', '2024-06-09 12:00:00.000', '~BP', '2024-07-09 12:00:00.000', '~BP', NULL),
('{incidentRequestGuids[9]}', 'CS99990010', 'ENT', 'VAL', 'CR4', 'APR', 'Summary', 'Details', '2024-06-10 12:00:00.000', '~BP', '2024-07-10 12:00:00.000', '~BP', NULL),
(NEWID(), 'CSM0000001', 'ENT', 'VAL', 'CR4', 'APR', 'Summary', 'Details', '2024-06-10 12:00:00.000', '~BP', '2024-07-10 12:00:00.000', '~BP', NULL),
(NEWID(), 'CHG0000001', 'ENT', 'VAL', 'CR4', 'APR', 'Summary', 'Details', '2024-06-10 12:00:00.000', '~BP', '2024-07-10 12:00:00.000', '~BP', NULL)

INSERT INTO dbo.IncidentMain(
	[IM_PK],
	[IM_INC_Request],
	[IM_IncidentNumber],
	[IM_IncidentType],
	[IM_Status],
	[IM_ResolutionCode],
	[IM_ResolveTimeUtc],
	[IM_SystemCreateTimeUtc],
	[IM_SystemCreateUser],
	[IM_SystemLastEditTimeUtc],
	[IM_SystemLastEditUser])
VALUES
('{incidentMainGuids[0]}', '{incidentRequestGuids[0]}', 'CS99990001', 'INC', 'CLS', 'CWR', '2024-07-01 12:00:00.000', '2024-06-01 12:00:00', 'E', '2024-07-01 12:00:00.000', 'E'),
('{incidentMainGuids[1]}', '{incidentRequestGuids[1]}', 'CS99990002', 'INC', 'CLS', 'CWR', '2024-07-02 12:00:00.000', '2024-06-02 12:00:00', 'E', '2024-07-02 12:00:00.000', 'E'),
('{incidentMainGuids[2]}', '{incidentRequestGuids[2]}', 'CS99990003', 'INC', 'CLS', 'CWR', '2024-07-03 12:00:00.000', '2024-06-03 12:00:00', 'E', '2024-07-03 12:00:00.000', 'E'),
('{incidentMainGuids[3]}', '{incidentRequestGuids[3]}', 'CS99990004', 'INC', 'CLS', 'CWR', '2024-07-04 12:00:00.000', '2024-06-04 12:00:00', 'E', '2024-07-04 12:00:00.000', 'E'),
('{incidentMainGuids[4]}', '{incidentRequestGuids[4]}', 'CS99990005', 'INC', 'CLS', 'SLV', '2024-07-05 12:00:00.000', '2024-06-05 12:00:00', 'E', '2024-07-05 12:00:00.000', 'E'),
('{incidentMainGuids[5]}', '{incidentRequestGuids[5]}', 'CS99990006', 'INC', 'CLS', 'SLV', NULL, '2024-06-06 12:00:00', 'E', '2024-07-06 12:00:00.000', 'E'),
('{incidentMainGuids[6]}', '{incidentRequestGuids[6]}', 'CS99990007', 'INC', 'CLS', 'WRK', NULL, '2024-06-07 12:00:00', 'E', '2024-07-07 12:00:00.000', 'E'),
('{incidentMainGuids[7]}', '{incidentRequestGuids[7]}', 'CS99990008', 'INC', 'CLS', 'WRK', NULL, '2024-06-08 12:00:00', 'E', '2024-07-08 12:00:00.000', 'E'),
('{incidentMainGuids[8]}', '{incidentRequestGuids[8]}', 'CS99990009', 'INC', 'CLS', 'CWR', NULL, '2024-06-09 12:00:00', 'E', '2024-07-09 12:00:00.000', 'E'),
('{incidentMainGuids[9]}', '{incidentRequestGuids[9]}', 'CS99990010', 'INC', 'CLS', 'CWR', NULL, '2024-06-10 12:00:00', 'E', '2024-07-10 12:00:00.000', 'E')

INSERT INTO dbo.StmALog(
	[SL_PK],
	[SL_Table],
	[SL_SE_NKEvent],
	[SL_Parent],
	[SL_Reference],
	[SL_EventTime],
	[SL_EventTimeUtc],
	[SL_PostedTimeUtc],
	[SL_GS_NKUser],
	[SL_GB_NKBranch])
VALUES
(NEWID(), 'IncidentMain', 'IWR', '{incidentMainGuids[0]}', 'Incident has been set to Awaiting Client Response.', '2024-07-01 23:00:00', '2024-07-01 13:00:00', '2023-07-01 13:00:00', 'E', 'SYD'),
(NEWID(), 'IncidentMain', 'IWR', '{incidentMainGuids[1]}', 'Incident has been set to Awaiting Client Response.', '2024-07-02 23:00:00', '2024-07-02 13:00:00', '2023-07-02 13:00:00', 'E', 'SYD'),
(NEWID(), 'IncidentMain', 'STC', '{incidentMainGuids[1]}', 'Status - WRK to CWR', '2024-07-02 23:00:00', '2024-07-03 13:00:00', '2023-07-03 13:00:00', 'E', 'SYD'),
(NEWID(), 'IncidentMain', 'STC', '{incidentMainGuids[2]}', 'Status - WRK to CWR', '2024-07-03 23:00:00', '2024-07-03 13:00:00', '2023-07-03 13:00:00', 'E', 'SYD'),
(NEWID(), 'IncidentMain', 'STC', '{incidentMainGuids[3]}', 'Status - WRK to CLS', '2024-07-04 23:00:00', '2024-07-04 13:00:00', '2023-07-04 13:00:00', 'E', 'SYD'),
(NEWID(), 'IncidentMain', 'STC', '{incidentMainGuids[4]}', 'Status - WRK to CWR', '2024-07-05 23:00:00', '2024-07-05 13:00:00', '2023-07-05 13:00:00', 'E', 'SYD'),
(NEWID(), 'IncidentMain', 'STC', '{incidentMainGuids[5]}', 'Status - WRK to CWR', '2024-07-06 23:00:00', '2024-07-06 13:00:00', '2023-07-06 13:00:00', 'E', 'SYD'),
(NEWID(), 'IncidentMain', 'STC', '{incidentMainGuids[6]}', 'Status - WRK to CLS', '2024-07-07 23:00:00', '2024-07-07 13:00:00', '2023-07-07 13:00:00', 'E', 'SYD'),
(NEWID(), 'IncidentMain', 'STC', '{incidentMainGuids[7]}', 'Status - WRK to CLS', '2024-07-08 23:00:00', '2024-07-08 13:00:00', '2023-07-08 13:00:00', 'E', 'SYD'),
(NEWID(), 'IncidentMain', 'IWR', '{incidentMainGuids[8]}', 'Incident has been set to Awaiting Client Response.', '2024-07-09 23:00:00', '2024-07-09 13:00:00', '2023-07-09 13:00:00', 'E', 'SYD'),
(NEWID(), 'IncidentMain', 'STC', '{incidentMainGuids[9]}', 'Status - WRK to CWR', '2024-07-10 23:00:00', '2024-07-10 13:00:00', '2023-07-10 13:00:00', 'E', 'SYD')
"
;
			using (var cmd = Db.Connection.Command(dataQuery))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}

	[TestedType(typeof(UpdateExpiryCountdownStartTimeUtc))]
	public class UpdateExpiryCountdownStartTimeUtcEmptyDatabaseTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var transformation = new UpdateExpiryCountdownStartTimeUtc();
			transformation.Initialise(null, new DummyUpgradeManager());
			return transformation;
		}

		protected override void PrepareTestData()
		{
			AssertEquals(false, Db.Connection.Exists("FROM IncidentRequest"));
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(false, Db.Connection.Exists("FROM IncidentRequest"));
		}
	}
}
