using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.CombinationReport;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.CombinationReport
{
	[TestedType(typeof(TelDeviceCombinationReportWithEntity))]
	class TelDeviceCombinationReportWithEntityTest : TelDeviceMarksWithEntityTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.CombinationReportQuery(Devices);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.CombinationReportBlockQuery(pksToBlock);
		}

		protected override string ActQuery(Guid entityPk)
		{
			return "SELECT * FROM dbo.TelDeviceCombinationReportWithEntity ORDER BY TLC_DeviceID, TLC_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			var resultArray = result
				.Select(row => new
				{
					TLC_ParentTableCode = row["TLC_ParentTableCode"].Equals(DBNull.Value) ? null : row["TLC_ParentTableCode"].ToString(),
					TLC_ParentID = Guid.TryParse(Convert.ToString(row["TLC_ParentID"]), out var g) ? g : (Guid?)null,
					TLC_DeviceID = Guid.Parse(row["TLC_DeviceID"].ToString()),
					TLC_StartTimeUtc = row["TLC_StartTimeUtc"],
					TLC_EndTimeUtc = row["TLC_EndTimeUtc"],
					TLC_PK = Guid.Parse(row["TLC_PK"].ToString()),
					TLC_MeasurementTimeUtc = row["TLC_MeasurementTimeUtc"],
					TLC_CombinationCode = row["TLC_CombinationCode"],
				})
				.ToList();

			// Assert
			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.TLC_ParentTableCode, arg.TLC_ParentID, arg.TLC_DeviceID, arg.TLC_PK)).ToArray());
		}
	}
}
