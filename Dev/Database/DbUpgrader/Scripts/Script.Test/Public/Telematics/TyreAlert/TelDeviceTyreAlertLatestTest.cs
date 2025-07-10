using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.TyreAlert;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.TyreAlert
{
	[TestedType(typeof(TelDeviceTyreAlertLatest))]
	class TelDeviceTyreAlertLatestTest : TelDeviceLatestMarksTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.TyreAlertQuery(Devices);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.BlockAllRecordsForDeviceQuery("GlbDeviceTyreAlert", "GDA", pksToBlock);
		}

		protected override string ActQuery(string entityPk)
		{
			return $@"select * from dbo.TelDeviceTyreAlertLatest('{entityPk}') order by V3_PK";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			var resultArray = result
				.Select(row => new
				{
					V7_ParentTableCode = row["V7_ParentTableCode"].Equals(DBNull.Value) ? null : row["V7_ParentTableCode"].ToString(),
					V7_ParentID = row["V7_ParentID"].Equals(DBNull.Value) ? (Guid?)null : Guid.Parse(row["V7_ParentID"].ToString()),
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

			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.V7_ParentTableCode, arg.V7_ParentID, arg.V3_PK, arg.GDA_PK)).ToArray());
		}
	}
}
