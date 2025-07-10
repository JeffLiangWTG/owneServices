using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.TyreReport;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.TyreReport
{
	[TestedType(typeof(TelDeviceTyreReportWithEntity))]
	class TelDeviceTyreReportWithEntityTest : TelDeviceMarksWithEntityTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.TyreReportQuery(Devices);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.TyreReportBlockQuery(pksToBlock);
		}

		protected override string ActQuery(Guid entityPk)
		{
			return "SELECT * FROM dbo.TelDeviceTyreReportWithEntity ORDER BY TLR_DeviceID, TLR_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			var resultArray = result
				.Select(row => new
				{
					TLR_ParentTableCode = row["TLR_ParentTableCode"].Equals(DBNull.Value) ? null : row["TLR_ParentTableCode"].ToString(),
					TLR_ParentID = Guid.TryParse(Convert.ToString(row["TLR_ParentID"]), out var g) ? g : (Guid?)null,
					TLR_DeviceID = Guid.Parse(row["TLR_DeviceID"].ToString()),
					TLR_StartTimeUtc = row["TLR_StartTimeUtc"],
					TLR_EndTimeUtc = row["TLR_EndTimeUtc"],
					TLR_PK = Guid.Parse(row["TLR_PK"].ToString()),
					TLR_MeasurementTimeUtc = row["TLR_MeasurementTimeUtc"],
					TLR_PressureKPa = row["TLR_PressureKPa"],
					TLR_TemperatureC = row["TLR_TemperatureC"]
				})
				.ToList();

			// Assert
			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.TLR_ParentTableCode, arg.TLR_ParentID, arg.TLR_DeviceID, arg.TLR_PK)).ToArray());
		}
	}
}
