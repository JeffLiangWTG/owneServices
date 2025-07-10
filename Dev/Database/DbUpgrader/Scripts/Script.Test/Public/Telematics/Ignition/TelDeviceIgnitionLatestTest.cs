using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.Ignition;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.Ignition
{
	[TestedType(typeof(TelDeviceIgnitionLatest))]
	class TelDeviceIgnitionLatestTest : TelDeviceLatestMarksTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.IgnitionQuery(Devices);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.BlockAllRecordsForDeviceQuery("GlbDeviceIgnition", "GDI", pksToBlock);
		}

		protected override string ActQuery(string entityPk)
		{
			return $@"select * from dbo.TelDeviceIgnitionLatest('{entityPk}') order by V3_PK";
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
					GDI_PK = Guid.Parse(row["GDI_PK"].ToString()),
					GDI_MeasurementTimeUtc = row["GDI_MeasurementTimeUtc"],
					GDI_State = row["GDI_State"]
				})
				.ToList();

			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.V7_ParentTableCode, arg.V7_ParentID, arg.V3_PK, arg.GDI_PK)).ToArray());
		}
	}
}
