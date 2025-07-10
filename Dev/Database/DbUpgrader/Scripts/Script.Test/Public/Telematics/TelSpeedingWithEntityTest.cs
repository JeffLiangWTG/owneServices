using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics
{
	[TestedType(typeof(TelSpeedingWithEntity))]
	class TelSpeedingWithEntityTest : TelDeviceMarksWithEntityTestBase
	{
		public void TestSelectsWithSpeedLimit()
		{
			// Arrange
			const string locationId = "00000001-0000-0000-0000-000000000007";
			CreateData();
			Db.Connection.ExecuteNonQuery($@"
UPDATE dbo.GlbDeviceLocation SET V2_SpeedLimitState='M', V2_SpeedLimitKmh=1, V2_SystemLastEditUser = 'E', V2_SystemLastEditTimeUtc = GETUTCDATE() WHERE V2_PK <> '{locationId}';
UPDATE dbo.GlbDeviceLocation SET V2_SpeedLimitState='S', V2_SpeedLimitKmh=1, V2_SystemLastEditUser = 'E', V2_SystemLastEditTimeUtc = GETUTCDATE() WHERE V2_PK =  '{locationId}';
UPDATE dbo.GlbDeviceLocation SET V2_SpeedLimitState='U', V2_SpeedLimitKmh=1, V2_SystemLastEditUser = 'E', V2_SystemLastEditTimeUtc = GETUTCDATE() WHERE V2_PK =  '00000001-0000-0000-0000-000000000001';
UPDATE dbo.GlbDeviceLocation SET V2_SpeedLimitState='M', V2_SpeedLimitKmh=1, V2_SystemLastEditUser = 'E', V2_SystemLastEditTimeUtc = GETUTCDATE() WHERE V2_PK =  '00000001-0000-0000-0000-000000000002';
");

			// Act
			var result = (
				Count: Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.TelSpeedingWithEntity('2017-01-15', '2017-02-15')"),
				Pk: Db.Connection.ExecuteScalar<Guid>("SELECT V2_PK FROM dbo.TelSpeedingWithEntity('2017-01-15', '2017-02-15')")
			);

			// Assert
			AssertEquals(1, result.Count);
			AssertEquals(Guid.Parse(locationId), result.Pk);
		}

		public void TestSelectsWithExceedingSpeedLimit()
		{
			// Arrange
			const string locationId = "00000001-0000-0000-0000-000000000007";
			CreateData();
			Db.Connection.ExecuteNonQuery($"UPDATE dbo.GlbDeviceLocation SET V2_SpeedLimitKmh=10, V2_SystemLastEditUser = 'E', V2_SystemLastEditTimeUtc = GETUTCDATE() WHERE V2_PK <> '{locationId}'");

			// Act
			var result = (
				Count: Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.TelSpeedingWithEntity('2017-01-15', '2017-02-15')"),
				Pk: Db.Connection.ExecuteScalar<Guid>("SELECT V2_PK FROM dbo.TelSpeedingWithEntity('2017-01-15', '2017-02-15')")
			);

			// Assert
			AssertEquals(1, result.Count);
			AssertEquals(Guid.Parse(locationId), result.Pk);
		}

		public void TestDoesNotSelectLocationsWithoutSpeedLimits()
		{
			// Arrange
			CreateData();
			Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbDeviceLocation SET V2_SpeedLimitState='M', V2_SystemLastEditUser = 'E', V2_SystemLastEditTimeUtc = GETUTCDATE()");

			// Act
			var result = Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.TelSpeedingWithEntity('2017-01-15', '2017-02-15')");

			// Assert
			AssertEquals(0, result);
		}

		public void TestDoesNotSelectLocationsWithNotLoadedSpeedLimits()
		{
			// Arrange
			CreateData();
			Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbDeviceLocation SET V2_SpeedLimitState='U', V2_SystemLastEditUser = 'E', V2_SystemLastEditTimeUtc = GETUTCDATE()");

			// Act
			var result = Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.TelSpeedingWithEntity('2017-01-15', '2017-02-15')");

			// Assert
			AssertEquals(0, result);
		}

		public void TestSelectsWithinTimeInterval()
		{
			SelectAndAssertWithinTimeInterval("2017-01-09", "2017-01-11", new[]
			{
				Guid.Parse("00000001-0000-0000-0000-000000000005"),
				Guid.Parse("00000002-0000-0000-0000-000000000005"),
				Guid.Parse("00000003-0000-0000-0000-000000000005"),
				Guid.Parse("00000004-0000-0000-0000-000000000005"),
				Guid.Parse("00000005-0000-0000-0000-000000000005")
			});
		}

		public void TestSelectsWithinInvertedTimeInterval()
		{
			SelectAndAssertWithinTimeInterval("2017-01-11", "2017-01-09", Array.Empty<Guid>());
		}

		void SelectAndAssertWithinTimeInterval(string dateFrom, string dateTo, Guid[] expectedPks)
		{
			// Arrange
			CreateData();

			// Act
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM dbo.TelSpeedingWithEntity('{dateFrom}', '{dateTo}') ORDER BY V3_PK")
				.Rows.Cast<DataRow>()
				.Select(row => Guid.Parse(row["V2_PK"].ToString()))
				.ToArray();

			// Assert
			AssertArrayEqualsByElements(expectedPks, result);
		}

		protected override string ArrangeQuery()
		{
			return TestDataQueries.LocationQuery(Devices) + "; UPDATE dbo.GlbDeviceLocation SET V2_SpeedLimitState='S', V2_SpeedLimitKmh=1, V2_SystemLastEditUser = 'E', V2_SystemLastEditTimeUtc = GETUTCDATE();";
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.LocationBlockQuery(pksToBlock);
		}

		protected override string ActQuery(Guid entityPk)
		{
			return "SELECT * FROM dbo.TelSpeedingWithEntity('2016-01-01', '2018-01-01') ORDER BY V3_PK, V2_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			var resultArray = result
				.Select(row => new
				{
					V7_ParentTableCode = row["V7_ParentTableCode"].Equals(DBNull.Value) ? null : row["V7_ParentTableCode"].ToString(),
					V7_ParentID = Guid.TryParse(Convert.ToString(row["V7_ParentID"]), out var g) ? g : (Guid?)null,
					V3_PK = Guid.Parse(row["V3_PK"].ToString()),
					V7_StartTimeUtc = row["V7_StartTimeUtc"],
					V7_EndTimeUtc = row["V7_EndTimeUtc"],
					V2_PK = Guid.Parse(row["V2_PK"].ToString()),
					V2_MeasurementTimeUtc = row["V2_MeasurementTimeUtc"],
					V2_Location = row["V2_Location"],
					V2_SpeedLimitKmh = row["V2_SpeedLimitKmh"],
					V2_SpeedLimitState = row["V2_SpeedLimitState"],
					V2_AccuracyInMetres = row["V2_AccuracyInMetres"],
					V2_Speed = row["V2_Speedkmh"],
					V2_CompassHeadingDegrees = row["V2_CompassHeadingDegrees"],
					V2_SatNumber = row["V2_SatelliteQuantity"],
					V2_HDOP = row["V2_HDOP"]
				})
				.ToList();

			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.V7_ParentTableCode, arg.V7_ParentID, arg.V3_PK, arg.V2_PK)).ToArray());
		}
	}
}
