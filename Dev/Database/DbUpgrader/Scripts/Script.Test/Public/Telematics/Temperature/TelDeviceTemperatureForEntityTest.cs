using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.Temperature;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.Temperature
{
	[TestedType(typeof(TelDeviceTemperatureForEntity))]
	class TelDeviceTemperatureForEntityTest : TelDeviceMarksForEntityTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.TemperatureQuery(Devices);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.TemperatureBlockQuery(pksToBlock);
		}

		protected override string ActQuery(Guid entityPk)
		{
			return $@"SELECT * FROM dbo.TelDeviceTemperatureForEntity('RQ', '{entityPk}', '2017-01-15', '2017-02-15') ORDER BY GDT_MeasurementTimeUtc";
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
					GDT_PK = Guid.Parse(row["GDT_PK"].ToString()),
					GDT_MeasurementTimeUtc = row["GDT_MeasurementTimeUtc"],
					GDT_TemperatureC = row["GDT_TemperatureC"]
				})
				.ToList();

			// Assert
			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.V7_ParentTableCode, arg.V7_ParentID, arg.V3_PK, arg.GDT_PK)).ToArray());
		}
	}
}
