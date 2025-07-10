using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.Battery;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.Battery
{
	[TestedType(typeof(TelDeviceBatteryForEntity))]
	class TelDeviceBatteryForEntityTest : TelDeviceMarksForEntityTestBase
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
			return $@"SELECT * FROM dbo.TelDeviceBatteryForEntity('RQ', '{entityPk}', '2017-01-15', '2017-02-15') ORDER BY GDB_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid, Guid, Guid>[] expectedPks)
		{
			var resultArray = result
				.Select(row => new
				{
					V7_ParentTableCode = row["V7_ParentTableCode"].ToString(),
					V7_ParentID = Guid.Parse(row["V7_ParentID"].ToString()),
					V3_PK = Guid.Parse(row["V3_PK"].ToString()),
					V7_StartTimeUtc = row["V7_StartTimeUtc"],
					V7_EndTimeUtc = row["V7_EndTimeUtc"],
					GDB_PK = Guid.Parse(row["GDB_PK"].ToString()),
					GDB_MeasurementTimeUtc = row["GDB_MeasurementTimeUtc"],
					GDB_IsCharging = row["GDB_IsCharging"],
					GDB_Voltage = row["GDB_Voltage"],
					GDB_CurrentA = row["GDB_CurrentA"],
					GDB_TemperatureC = row["GDB_TemperatureC"],
					GDB_ChargeRemaining = row["GDB_ChargeRemaining"]
				})
				.ToList();

			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.V7_ParentTableCode, arg.V7_ParentID, arg.V3_PK, arg.GDB_PK)).ToArray());
		}
	}
}
