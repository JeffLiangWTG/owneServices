using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI.Testing
{
	[TestedType(typeof(UpdateARTMetricCount))]
	class UpdateARTMetricCountTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateARTMetricCount();
		}

		protected override void AssertTransformationResults()
		{
			var resultQuery = $"SELECT COUNT(1) FROM dbo.IncidentMetrics WHERE IME_PK = '{incidentMetricsGuids[0]}'";
			var result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("ART Metric should be deleted", 0, result);

			resultQuery = $"SELECT COUNT(1) FROM dbo.IncidentMetrics WHERE IME_PK = '{incidentMetricsGuids[1]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("ART Metric should be deleted because the IME_CalculatedMetric and IME_MetricCount are not 0", 1, result);

			resultQuery = $"SELECT IME_MetricCount FROM dbo.IncidentMetrics WHERE IME_PK = '{incidentMetricsGuids[2]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("IME_MetricCount should be updated to 1", 1, result);

			resultQuery = $"SELECT IME_MetricCount FROM dbo.IncidentMetrics WHERE IME_PK = '{incidentMetricsGuids[3]}'";
			result = Db.Connection.ExecuteScalar(resultQuery);
			AssertEquals("IME_MetricCount should not be updated because IME_MetricCount is NOT 0", 2, result);
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
('{incidentMetricsGuids[0]}', 'ART', 'CS99990001', '2025-01-01 12:00:00', NULL, 0, 0, '2025-01-01 12:00:00', 'E', '2025-01-01 13:00:00.000', 'E'),
('{incidentMetricsGuids[1]}', 'ART', 'CS99990002', '2025-01-01 14:00:00', NULL, 3600, 1, '2025-01-01 14:00:00', 'E', '2025-01-01 15:00:00.000', 'E'),
('{incidentMetricsGuids[2]}', 'ART', 'CS99990003', '2025-01-01 16:00:00', '2025-01-01 17:00:00', 3600, 0, '2025-01-01 16:00:00', 'E', '2025-01-01 17:00:00.000', 'E'),
('{incidentMetricsGuids[3]}', 'ART', 'CS99990004', '2025-01-01 18:00:00', '2025-01-01 19:00:00', 3600, 2, '2025-01-01 18:00:00', 'E', '2025-01-01 19:00:00.000', 'E')
";

			using (var cmd = Db.Connection.Command(dataQuery))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
