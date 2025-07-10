using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.Temperature;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.Temperature
{
	[TestedType(typeof(TelDeviceTemperatureWithEntity))]
	class TelDeviceTemperatureWithEntityTest : TelDeviceMarksWithEntityTestBase
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
			return "SELECT * FROM dbo.TelDeviceTemperatureWithEntity ORDER BY TLT_DeviceID, TLT_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			var resultArray = result
				.Select(row => new
				{
					TLT_ParentTableCode = row["TLT_ParentTableCode"].Equals(DBNull.Value) ? null : row["TLT_ParentTableCode"].ToString(),
					TLT_ParentID = Guid.TryParse(Convert.ToString(row["TLT_ParentID"]), out var g) ? g : (Guid?)null,
					TLT_DeviceID = Guid.Parse(row["TLT_DeviceID"].ToString()),
					TLT_StartTimeUtc = row["TLT_StartTimeUtc"],
					TLT_EndTimeUtc = row["TLT_EndTimeUtc"],
					TLT_PK = Guid.Parse(row["TLT_PK"].ToString()),
					TLT_MeasurementTimeUtc = row["TLT_MeasurementTimeUtc"],
					TLT_TemperatureC = row["TLT_TemperatureC"]
				})
				.ToList();

			// Assert
			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.TLT_ParentTableCode, arg.TLT_ParentID, arg.TLT_DeviceID, arg.TLT_PK)).ToArray());
		}
	}
}
