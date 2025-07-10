using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.TyreAlert;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.TyreAlert
{
	[TestedType(typeof(TelDeviceTyreAlertWithEntity))]
	class TelDeviceTyreAlertWithEntityTest : TelDeviceMarksWithEntityTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.TyreAlertQuery(Devices);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.TyreAlertBlockQuery(pksToBlock);
		}

		protected override string ActQuery(Guid entityPk)
		{
			return "SELECT * FROM dbo.TelDeviceTyreAlertWithEntity ORDER BY TLA_DeviceID, TLA_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			var resultArray = result
				.Select(row => new
				{
					TLA_ParentTableCode = row["TLA_ParentTableCode"].Equals(DBNull.Value) ? null : row["TLA_ParentTableCode"].ToString(),
					TLA_ParentID = Guid.TryParse(Convert.ToString(row["TLA_ParentID"]), out var g) ? g : (Guid?)null,
					TLA_DeviceID = Guid.Parse(row["TLA_DeviceID"].ToString()),
					TLA_StartTimeUtc = row["TLA_StartTimeUtc"],
					TLA_EndTimeUtc = row["TLA_EndTimeUtc"],
					TLA_PK = Guid.Parse(row["TLA_PK"].ToString()),
					TLA_MeasurementTimeUtc = row["TLA_MeasurementTimeUtc"],
					TLA_OperationMode = row["TLA_OperationMode"],
					TLA_IsADCOverflow = row["TLA_IsADCOverflow"],
					TLA_IsLowBatteryVoltage = row["TLA_IsLowBatteryVoltage"],
					TLA_SpecificHardwareFault = row["TLA_SpecificHardwareFault"],
					TLA_PressureKPa = row["TLA_PressureKPa"],
					TLA_TemperatureC = row["TLA_TemperatureC"]
				})
				.ToList();

			// Assert
			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.TLA_ParentTableCode, arg.TLA_ParentID, arg.TLA_DeviceID, arg.TLA_PK)).ToArray());
		}
	}
}
