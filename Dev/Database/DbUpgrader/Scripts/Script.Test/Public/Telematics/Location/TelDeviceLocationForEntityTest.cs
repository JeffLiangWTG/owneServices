using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.Location;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.Location
{
	[TestedType(typeof(TelDeviceLocationForEntity))]
	class TelDeviceLocationForEntityTest : TelDeviceMarksForEntityTestBase
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
			return $@"SELECT * FROM dbo.TelDeviceLocationForEntity('RQ', '{entityPk}', '2017-01-15', '2017-02-15') ORDER BY V2_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid, Guid, Guid>[] expectedPks)
		{
			// Act
			var resultArray = result
				.Select(row => new
				{
					V7_ParentTableCode = row["V7_ParentTableCode"].ToString(),
					V7_ParentID = Guid.Parse(row["V7_ParentID"].ToString()),
					V7_StartTimeUtc = row["V7_StartTimeUtc"],
					V7_EndTimeUtc = row["V7_EndTimeUtc"],
					V3_PK = Guid.Parse(row["V3_PK"].ToString()),
					V3_HardwareKind = row["V3_HardwareKind"],
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

			// Assert
			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.V7_ParentTableCode, arg.V7_ParentID, arg.V3_PK, arg.V2_PK)).ToArray());
		}
	}
}
