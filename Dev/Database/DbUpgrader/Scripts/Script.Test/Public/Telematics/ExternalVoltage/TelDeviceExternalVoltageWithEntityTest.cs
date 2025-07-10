using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.ExternalVoltage;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.ExternalVoltage
{
	[TestedType(typeof(TelDeviceExternalVoltageWithEntity))]
	class TelDeviceExternalVoltageWithEntityTest : TelDeviceMarksWithEntityTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.ExternalVoltageQuery(Devices);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.ExternalVoltageBlockQuery(pksToBlock);
		}

		protected override string ActQuery(Guid entityPk)
		{
			return "SELECT * FROM dbo.TelDeviceExternalVoltageWithEntity ORDER BY TLV_DeviceID, TLV_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			var resultArray = result
				.Select(row => new
				{
					TLV_ParentTableCode = row["TLV_ParentTableCode"].Equals(DBNull.Value) ? null : row["TLV_ParentTableCode"].ToString(),
					TLV_ParentID = Guid.TryParse(Convert.ToString(row["TLV_ParentID"]), out var g) ? g : (Guid?)null,
					TLV_DeviceID = Guid.Parse(row["TLV_DeviceID"].ToString()),
					TLV_StartTimeUtc = row["TLV_StartTimeUtc"],
					TLV_EndTimeUtc = row["TLV_EndTimeUtc"],
					TLV_PK = Guid.Parse(row["TLV_PK"].ToString()),
					TLV_MeasurementTimeUtc = row["TLV_MeasurementTimeUtc"],
					TLV_Voltage = row["TLV_Voltage"]
				})
				.ToList();

			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.TLV_ParentTableCode, arg.TLV_ParentID, arg.TLV_DeviceID, arg.TLV_PK)).ToArray());
		}
	}
}
