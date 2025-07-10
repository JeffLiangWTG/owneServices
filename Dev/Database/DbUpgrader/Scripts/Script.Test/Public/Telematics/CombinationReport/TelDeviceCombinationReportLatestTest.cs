using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.CombinationReport;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.CombinationReport
{
	[TestedType(typeof(TelDeviceCombinationReportLatest))]
	class TelDeviceCombinationReportLatestTest : TelDeviceLatestMarksTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.CombinationReportQuery(Devices);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.BlockAllRecordsForDeviceQuery("GlbDeviceCombinationReport", "GDC", pksToBlock);
		}

		protected override string ActQuery(string entityPk)
		{
			return $@"SELECT * FROM dbo.TelDeviceCombinationReportLatest('{entityPk}') ORDER BY V3_PK";
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
					GDC_PK = Guid.Parse(row["GDC_PK"].ToString()),
					GDC_MeasurementTimeUtc = row["GDC_MeasurementTimeUtc"],
					GDC_CombinationCode = row["GDC_CombinationCode"]
				})
				.ToList();

			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.V7_ParentTableCode, arg.V7_ParentID, arg.V3_PK, arg.GDC_PK)).ToArray());
		}
	}
}
