using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.Battery;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.Battery
{
	[TestedType(typeof(TelDeviceBatteryWithEntity))]
	class TelDeviceBatteryWithEntityTest : TelDeviceMarksWithEntityTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.BatteryQuery(Devices);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.BatteryBlockQuery(pksToBlock);
		}

		protected override string ActQuery(Guid entityPk)
		{
			return "SELECT * FROM dbo.TelDeviceBatteryWithEntity ORDER BY TLB_DeviceID, TLB_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			var resultArray = result
				.Select(row => new
				{
					TLB_ParentTableCode = row["TLB_ParentTableCode"].Equals(DBNull.Value) ? null : row["TLB_ParentTableCode"].ToString(),
					TLB_ParentID = Guid.TryParse(Convert.ToString(row["TLB_ParentID"]), out var g) ? g : (Guid?)null,
					TLB_DeviceID = Guid.Parse(row["TLB_DeviceID"].ToString()),
					TLB_StartTimeUtc = row["TLB_StartTimeUtc"],
					TLB_EndTimeUtc = row["TLB_EndTimeUtc"],
					TLB_PK = Guid.Parse(row["TLB_PK"].ToString()),
					TLB_MeasurementTimeUtc = row["TLB_MeasurementTimeUtc"],
					TLB_IsCharging = row["TLB_IsCharging"],
					TLB_Voltage = row["TLB_Voltage"],
					TLB_CurrentA = row["TLB_CurrentA"],
					TLB_TemperatureC = row["TLB_TemperatureC"],
					TLB_ChargeRemaining = row["TLB_ChargeRemaining"]
				})
				.ToList();

			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.TLB_ParentTableCode, arg.TLB_ParentID, arg.TLB_DeviceID, arg.TLB_PK)).ToArray());
		}
	}
}
