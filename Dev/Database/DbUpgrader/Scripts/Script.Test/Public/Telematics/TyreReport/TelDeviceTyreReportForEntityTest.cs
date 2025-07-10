using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.TyreReport;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.TyreReport
{
	[TestedType(typeof(TelDeviceTyreReportForEntity))]
	class TelDeviceTyreReportForEntityTest : TelDeviceMarksForEntityTestBase
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
			return $@"SELECT * FROM dbo.TelDeviceTyreReportForEntity('RQ', '{entityPk}', '2017-01-15', '2017-02-15') ORDER BY GDR_MeasurementTimeUtc";
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
					GDR_PK = Guid.Parse(row["GDR_PK"].ToString()),
					GDR_MeasurementTimeUtc = row["GDR_MeasurementTimeUtc"],
					GDR_PressureKPa = row["GDR_PressureKPa"],
					GDR_TemperatureC = row["GDR_TemperatureC"]
				})
				.ToList();

			// Assert
			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.V7_ParentTableCode, arg.V7_ParentID, arg.V3_PK, arg.GDR_PK)).ToArray());
		}
	}
}
