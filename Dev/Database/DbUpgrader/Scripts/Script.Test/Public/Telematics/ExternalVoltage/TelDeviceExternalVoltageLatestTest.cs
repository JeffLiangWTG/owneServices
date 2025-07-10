using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.ExternalVoltage;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.ExternalVoltage
{
	[TestedType(typeof(TelDeviceExternalVoltageLatest))]
	class TelDeviceExternalVoltageLatestTest : TelDeviceLatestMarksTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.ExternalVoltageQuery(Devices);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.BlockAllRecordsForDeviceQuery("GlbDeviceExternalVoltage", "GDV", pksToBlock);
		}

		protected override string ActQuery(string entityPk)
		{
			return $@"select * from dbo.TelDeviceExternalVoltageLatest('{entityPk}') order by V3_PK";
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
					GDV_PK = Guid.Parse(row["GDV_PK"].ToString()),
					GDV_MeasurementTimeUtc = row["GDV_MeasurementTimeUtc"],
					GDV_Voltage = row["GDV_Voltage"]
				})
				.ToList();

			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.V7_ParentTableCode, arg.V7_ParentID, arg.V3_PK, arg.GDV_PK)).ToArray());
		}
	}
}
