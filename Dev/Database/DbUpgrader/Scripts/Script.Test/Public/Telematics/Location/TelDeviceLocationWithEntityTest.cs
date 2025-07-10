using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.Location;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.Location
{
	[TestedType(typeof(TelDeviceLocationWithEntity))]
	class TelDeviceLocationWithEntityTest : TelDeviceMarksWithEntityTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.LocationQuery(Devices);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.LocationBlockQuery(pksToBlock);
		}

		protected override string ActQuery(Guid entityPk)
		{
			return "SELECT * FROM dbo.TelDeviceLocationWithEntity ORDER BY TLL_DeviceID, TLL_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			var resultArray = result
				.Select(row => new
				{
					TLL_ParentTableCode = row["TLL_ParentTableCode"].Equals(DBNull.Value) ? null : row["TLL_ParentTableCode"].ToString(),
					TLL_ParentID = Guid.TryParse(Convert.ToString(row["TLL_ParentID"]), out var g) ? g : (Guid?)null,
					TLL_DeviceID = Guid.Parse(row["TLL_DeviceID"].ToString()),
					TLL_StartTimeUtc = row["TLL_StartTimeUtc"],
					TLL_EndTimeUtc = row["TLL_EndTimeUtc"],
					TLL_PK = Guid.Parse(row["TLL_PK"].ToString()),
					TLL_MeasurementTimeUtc = row["TLL_MeasurementTimeUtc"],
					TLL_Location = row["TLL_Location"],
					TLL_SpeedLimitKmh = row["TLL_SpeedLimitKmh"],
					TLL_SpeedLimitState = row["TLL_SpeedLimitState"],
					TLL_AccuracyInMetres = row["TLL_AccuracyInMetres"],
					TLL_Speed = row["TLL_Speedkmh"],
					TLL_CompassHeadingDegrees = row["TLL_CompassHeadingDegrees"],
					TLL_SatNumber = row["TLL_SatelliteQuantity"],
					TLL_HDOP = row["TLL_HDOP"]
				})
				.ToList();

			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.TLL_ParentTableCode, arg.TLL_ParentID, arg.TLL_DeviceID, arg.TLL_PK)).ToArray());
		}
	}
}
