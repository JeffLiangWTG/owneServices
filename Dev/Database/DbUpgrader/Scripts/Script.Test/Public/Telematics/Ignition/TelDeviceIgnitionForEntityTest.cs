using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.Ignition;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.Ignition
{
	[TestedType(typeof(TelDeviceIgnitionForEntity))]
	class TelDeviceIgnitionForEntityTest : TelDeviceMarksForEntityTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.IgnitionQuery(Devices);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.IgnitionBlockQuery(pksToBlock);
		}

		protected override string ActQuery(Guid entityPk)
		{
			return $@"SELECT * FROM dbo.TelDeviceIgnitionForEntity('RQ', '{entityPk}', '2017-01-15', '2017-02-15') ORDER BY GDI_MeasurementTimeUtc";
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
					GDI_PK = Guid.Parse(row["GDI_PK"].ToString()),
					GDI_MeasurementTimeUtc = row["GDI_MeasurementTimeUtc"],
					GDI_Voltage = row["GDI_State"]
				})
				.ToList();

			// Assert
			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.V7_ParentTableCode, arg.V7_ParentID, arg.V3_PK, arg.GDI_PK)).ToArray());
		}
	}
}
