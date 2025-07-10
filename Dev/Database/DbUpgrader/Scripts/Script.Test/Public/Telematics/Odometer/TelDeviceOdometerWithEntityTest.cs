using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.Odometer;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.Odometer
{
	[TestedType(typeof(TelDeviceOdometerWithEntity))]
	class TelDeviceOdometerWithEntityTest : TelDeviceMarksWithEntityTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.OdometerQuery(Devices);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.OdometerBlockQuery(pksToBlock);
		}

		protected override string ActQuery(Guid entityPk)
		{
			return "SELECT * FROM dbo.TelDeviceOdometerWithEntity ORDER BY TLO_DeviceID, TLO_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			var resultArray = result
				.Select(row => new
				{
					TLO_ParentTableCode = row["TLO_ParentTableCode"].Equals(DBNull.Value) ? null : row["TLO_ParentTableCode"].ToString(),
					TLO_ParentID = Guid.TryParse(Convert.ToString(row["TLO_ParentID"]), out var g) ? g : (Guid?)null,
					TLO_DeviceID = Guid.Parse(row["TLO_DeviceID"].ToString()),
					TLO_StartTimeUtc = row["TLO_StartTimeUtc"],
					TLO_EndTimeUtc = row["TLO_EndTimeUtc"],
					TLO_PK = Guid.Parse(row["TLO_PK"].ToString()),
					TLO_MeasurementTimeUtc = row["TLO_MeasurementTimeUtc"],
					TLO_OdometerKM = row["TLO_OdometerKM"]
				})
				.ToList();

			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.TLO_ParentTableCode, arg.TLO_ParentID, arg.TLO_DeviceID, arg.TLO_PK)).ToArray());
		}
	}
}
