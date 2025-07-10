using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI.Testing
{
	[TestedType(typeof(UpdateNRAStartTimeAndEndTime))]
	class UpdateNRAStartTimeAndEndTimeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateNRAStartTimeAndEndTime();
		}

		protected override void AssertTransformationResults()
		{
			var resultQuery = $"SELECT IME_StartTimeUtc FROM dbo.IncidentMetrics WHERE IME_PK = '{incidentMetricsGuids[0]}'";
			var result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("IME_StartTimeUtc should be updated", new DateTime(2025, 1, 1, 12, 0, 0), result);

			resultQuery = $"SELECT IME_EndTimeUtc FROM dbo.IncidentMetrics WHERE IME_PK = '{incidentMetricsGuids[0]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("IME_EndTimeUtc should be updated", new DateTime(2025, 1, 1, 14, 0, 0), result);

			resultQuery = $"SELECT IME_StartTimeUtc FROM dbo.IncidentMetrics WHERE IME_PK = '{incidentMetricsGuids[1]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("IME_StartTimeUtc should not be updated because IME_CalculatedMetric is 0", DBNull.Value, result);

			resultQuery = $"SELECT IME_EndTimeUtc FROM dbo.IncidentMetrics WHERE IME_PK = '{incidentMetricsGuids[1]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("IME_EndTimeUtc should not be updated because IME_CalculatedMetric is 0", DBNull.Value, result);
		}

		readonly List<Guid> incidentMetricsGuids = new List<Guid>
		{
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid()
		};

		protected override void PrepareTestData()
		{
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
('{incidentMetricsGuids[0]}', 'NRA', 'CS99990001', NULL, NULL, 3600, 1, '2025-01-01 12:00:00', 'E', '2025-01-01 13:00:00.000', 'E'),
('{incidentMetricsGuids[1]}', 'NRA', 'CS99990002', NULL, NULL, 0, 1, '2025-01-01 12:00:00', 'E', '2025-01-01 13:00:00.000', 'E'),
('{incidentMetricsGuids[2]}', 'TRA', 'CS99990001', '2025-01-01 12:00:00', '2025-01-01 14:00:00', 7200, 1, '2025-01-01 12:00:00', 'E', '2025-01-01 14:00:00.000', 'E'),
('{incidentMetricsGuids[3]}', 'TRA', 'CS99990002', '2025-01-01 13:00:00', '2025-01-01 15:00:00', 7200, 1, '2025-01-01 13:00:00', 'E', '2025-01-01 15:00:00.000', 'E')
";

			using (var cmd = Db.Connection.Command(dataQuery))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update Net Resolution Age StartTime And EndTime_1] ON [dbo].[IncidentMetrics] ([IME_MetricCode], [IME_IncidentNumber]) INCLUDE ([IME_CalculatedMetric], [IME_EndTimeUtc], [IME_StartTimeUtc]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};
	}
}
