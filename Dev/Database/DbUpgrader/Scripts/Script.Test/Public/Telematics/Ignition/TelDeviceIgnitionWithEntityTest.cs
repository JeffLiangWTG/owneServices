using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.Ignition;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.Ignition
{
	[TestedType(typeof(TelDeviceIgnitionWithEntity))]
	class TelDeviceIgnitionWithEntityTest : TelDeviceMarksWithEntityTestBase
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
			return "SELECT * FROM dbo.TelDeviceIgnitionWithEntity ORDER BY TLI_DeviceID, TLI_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			var resultArray = result
				.Select(row => new
				{
					TLI_ParentTableCode = row["TLI_ParentTableCode"].Equals(DBNull.Value) ? null : row["TLI_ParentTableCode"].ToString(),
					TLI_ParentID = Guid.TryParse(Convert.ToString(row["TLI_ParentID"]), out var g) ? g : (Guid?)null,
					TLI_DeviceID = Guid.Parse(row["TLI_DeviceID"].ToString()),
					TLI_StartTimeUtc = row["TLI_StartTimeUtc"],
					TLI_EndTimeUtc = row["TLI_EndTimeUtc"],
					TLI_PK = Guid.Parse(row["TLI_PK"].ToString()),
					TLI_MeasurementTimeUtc = row["TLI_MeasurementTimeUtc"],
					TLI_Voltage = row["TLI_State"]
				})
				.ToList();

			// Assert
			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.TLI_ParentTableCode, arg.TLI_ParentID, arg.TLI_DeviceID, arg.TLI_PK)).ToArray());
		}
	}
}
