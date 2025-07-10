using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.TyreAlert;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.TyreAlert
{
	[TestedType(typeof(TelDeviceTyreAlertForEntity))]
	class TelDeviceTyreAlertForEntityTest : TelDeviceMarksForEntityTestBase
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
			return $@"SELECT * FROM dbo.TelDeviceTyreAlertForEntity('RQ', '{entityPk}', '2017-01-15', '2017-02-15') ORDER BY GDA_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid, Guid, Guid>[] expectedPks)
		{
			// Act
			var resultArray = result
				.Select(row => new
				{
					V7_ParentTableCode = row["V7_ParentTableCode"].ToString(),
					V7_ParentID = Guid.Parse(row["V7_ParentID"].ToString()),
					V3_PK = Guid.Parse(row["V3_PK"].ToString()),
					V7_StartTimeUtc = row["V7_StartTimeUtc"],
					V7_EndTimeUtc = row["V7_EndTimeUtc"],
					GDA_PK = Guid.Parse(row["GDA_PK"].ToString()),
					GDA_MeasurementTimeUtc = row["GDA_MeasurementTimeUtc"],
					GDA_OperationMode = row["GDA_OperationMode"],
					GDA_IsADCOverflow = row["GDA_IsADCOverflow"],
					GDA_IsLowBatteryVoltage = row["GDA_IsLowBatteryVoltage"],
					GDA_SpecificHardwareFault = row["GDA_SpecificHardwareFault"],
					GDA_PressureKPa = row["GDA_PressureKPa"],
					GDA_TemperatureC = row["GDA_TemperatureC"]
				})
				.ToList();

			// Assert
			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.V7_ParentTableCode, arg.V7_ParentID, arg.V3_PK, arg.GDA_PK)).ToArray());
		}
	}
}
