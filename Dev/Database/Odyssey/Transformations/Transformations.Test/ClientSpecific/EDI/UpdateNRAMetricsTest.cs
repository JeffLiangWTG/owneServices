using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI.Testing
{
	[TestedType(typeof(UpdateNRAMetrics))]
	class UpdateNRAMetricsTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateNRAMetrics();
		}

		protected override void AssertTransformationResults()
		{
			var resultQuery = $"SELECT IME_CalculatedMetric FROM dbo.IncidentMetrics WHERE IME_PK = '{incidentMetricsGuids[0]}'";
			var result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("IME_CalculatedMetric should not be updated because it's not NRA's metric", 3600, result);

			resultQuery = $"SELECT IME_CalculatedMetric FROM dbo.IncidentMetrics WHERE IME_PK = '{incidentMetricsGuids[1]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("IME_CalculatedMetric should not be updated because the incident's IM_ResolutionCode != 'CLS'", 3600, result);

			resultQuery = $"SELECT IME_CalculatedMetric FROM dbo.IncidentMetrics WHERE IME_PK = '{incidentMetricsGuids[2]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("IME_CalculatedMetric should not be updated because NRA's IME_CalculatedMetric > 0", 3600, result);

			resultQuery = $"SELECT IME_CalculatedMetric FROM dbo.IncidentMetrics WHERE IME_PK = '{incidentMetricsGuids[3]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("IME_CalculatedMetric should be updated to TRA's IME_CalculatedMetric because the incident's IM_ResolutionCode = 'CLS' and NRA's IME_CalculatedMetric = 0", 4200, result);
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
		};

		readonly List<Guid> incidentMetricsGuids = new List<Guid>
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
	[IM_IncidentNumber] VARCHAR(20) NOT NULL DEFAULT '' ,
	[IM_IncidentType] VARCHAR(3) NOT NULL DEFAULT '' ,
	[IM_Status] CHAR(3) NOT NULL DEFAULT '' ,
	[IM_ResolutionCode] CHAR(3) NOT NULL DEFAULT '' ,
	[IM_SystemCreateTimeUtc] SMALLDATETIME NULL,
	[IM_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '' ,
	[IM_SystemLastEditTimeUtc] DATETIME NULL,
	[IM_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT ''
);");

			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "IncidentMetrics", @"
CREATE TABLE dbo.IncidentMetrics
(
	[IME_PK] UNIQUEIDENTIFIER NOT NULL,
	[IME_MetricCode] VARCHAR(3) NOT NULL,
	[IME_IncidentNumber] VARCHAR(20) NOT NULL,
	[IME_StartTimeUtc] DATETIME NULL,
	[IME_EndTimeUtc] DATETIME NULL,
	[IME_CalculatedMetric] INT NOT NULL,
	[IME_SystemCreateTimeUtc] SMALLDATETIME NOT NULL,
	[IME_MetricCount] INT NOT NULL,
	[IME_SystemCreateUser] VARCHAR(3) NOT NULL,
	[IME_SystemLastEditTimeUtc] DATETIME NULL,
	[IME_SystemLastEditUser] VARCHAR(3) NOT NULL
);");

			var dataQuery = $@"
INSERT INTO dbo.IncidentMain(
	[IM_PK],
	[IM_IncidentNumber],
	[IM_IncidentType],
	[IM_Status],
	[IM_ResolutionCode],
	[IM_SystemCreateTimeUtc],
	[IM_SystemCreateUser],
	[IM_SystemLastEditTimeUtc],
	[IM_SystemLastEditUser])
VALUES
('{incidentMainGuids[0]}', 'CS99990001', 'INC', 'CLS', 'WRK', '2025-01-01 12:00:00', 'E', '2025-01-09 12:00:00.000', 'E'),
('{incidentMainGuids[1]}', 'CS99990002', 'INC', 'CLS', 'CWR', '2025-01-02 12:00:00', 'E', '2025-01-10 12:00:00.000', 'E'),
('{incidentMainGuids[2]}', 'CS99990003', 'INC', 'CLS', 'SLV', '2025-01-03 12:00:00', 'E', '2025-01-11 12:00:00.000', 'E'),
('{incidentMainGuids[3]}', 'CS99990004', 'INC', 'CLS', 'CLS', '2025-01-04 12:00:00', 'E', '2025-01-12 12:00:00.000', 'E')

INSERT INTO dbo.IncidentMetrics(
	[IME_PK],
	[IME_MetricCode],
	[IME_IncidentNumber],
	[IME_StartTimeUtc],
	[IME_EndTimeUtc],
	[IME_CalculatedMetric],
	[IME_MetricCount],
	[IME_SystemCreateTimeUtc],
	[IME_SystemCreateUser],
	[IME_SystemLastEditTimeUtc],
	[IME_SystemLastEditUser])
VALUES
('{incidentMetricsGuids[0]}', 'FRT', 'CS99990001', '2025-01-01 12:00:00', '2025-01-01 13:00:00', 3600, 1, '2025-01-01 12:00:00', 'E', '2025-01-01 13:00:00.000', 'E'),
('{incidentMetricsGuids[1]}', 'NRA', 'CS99990002', '2025-01-01 12:00:00', '2025-01-01 13:00:00', 3600, 1, '2025-01-01 12:00:00', 'E', '2025-01-01 13:00:00.000', 'E'),
('{incidentMetricsGuids[2]}', 'NRA', 'CS99990004', '2025-01-01 12:00:00', '2025-01-01 13:00:00', 3600, 1, '2025-01-01 12:00:00', 'E', '2025-01-01 13:00:00.000', 'E'),
('{incidentMetricsGuids[3]}', 'NRA', 'CS99990004', '2025-01-01 12:00:00', '2025-01-01 13:00:00', 0, 1, '2025-01-01 12:00:00', 'E', '2025-01-01 13:00:00.000', 'E'),
('{incidentMetricsGuids[4]}', 'TRA', 'CS99990001', '2025-01-01 12:00:00', '2025-01-01 14:00:00', 7200, 1, '2025-01-01 12:00:00', 'E', '2025-01-01 14:00:00.000', 'E'),
('{incidentMetricsGuids[5]}', 'TRA', 'CS99990002', '2025-01-01 12:00:00', '2025-01-01 14:00:00', 7200, 1, '2025-01-01 12:00:00', 'E', '2025-01-01 14:00:00.000', 'E'),
('{incidentMetricsGuids[6]}', 'TRA', 'CS99990003', '2025-01-01 12:00:00', '2025-01-01 14:00:00', 7200, 1, '2025-01-01 12:00:00', 'E', '2025-01-01 14:00:00.000', 'E'),
('{incidentMetricsGuids[7]}', 'TRA', 'CS99990004', '2025-01-01 12:00:00', '2025-01-01 14:00:00', 7200, 1, '2025-01-01 12:00:00', 'E', '2025-01-01 14:00:00.000', 'E'),
('{incidentMetricsGuids[8]}', 'ACT', 'CS99990004', '2025-01-01 12:00:00', '2025-01-01 14:00:00', 1800, 1, '2025-01-01 12:00:00', 'E', '2025-01-01 14:00:00.000', 'E'),
('{incidentMetricsGuids[9]}', 'ACT', 'CS99990004', '2025-01-01 12:00:00', '2025-01-01 14:00:00', 1200, 1, '2025-01-01 12:00:00', 'E', '2025-01-01 14:00:00.000', 'E')
";

			using (var cmd = Db.Connection.Command(dataQuery))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
