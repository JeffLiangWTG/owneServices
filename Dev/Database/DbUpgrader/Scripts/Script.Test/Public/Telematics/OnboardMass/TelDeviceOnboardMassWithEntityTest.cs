using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.OnboardMass;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.OnboardMass
{
	[TestedType(typeof(TelDeviceOnboardMassWithEntity))]
	class TelDeviceOnboardMassWithEntityTest : TelDeviceMarksWithEntityTestBase
	{
		protected override string ArrangeQuery()
		{
			return TestDataQueries.OnboardMassQuery(Devices, SubEquipments);
		}

		protected override string BlockingQueryPattern(IEnumerable<Guid> pksToBlock)
		{
			return TestDataQueries.OnboardMassBlockQuery(pksToBlock);
		}

		protected override string ActQuery(Guid entityPk)
		{
			return "SELECT * FROM dbo.TelDeviceOnboardMassWithEntity ORDER BY TLM_DeviceID, TLM_MeasurementTimeUtc";
		}

		protected override void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			var resultArray = result
				.Select(row => new
				{
					TLM_ParentTableCode = row["TLM_ParentTableCode"].Equals(DBNull.Value) ? null : row["TLM_ParentTableCode"].ToString(),
					TLM_ParentID = Guid.TryParse(Convert.ToString(row["TLM_ParentID"]), out var g) ? g : (Guid?)null,
					TLM_DeviceID = Guid.Parse(row["TLM_DeviceID"].ToString()),
					TLM_StartTimeUtc = row["TLM_StartTimeUtc"],
					TLM_EndTimeUtc = row["TLM_EndTimeUtc"],
					TLM_PK = Guid.Parse(row["TLM_PK"].ToString()),
					TLM_MeasurementTimeUtc = row["TLM_MeasurementTimeUtc"],
					TLM_PressureKPa = row["TLM_PressureKPa"],
					TLM_DeviationPercent = row["TLM_DeviationPercent"]
				})
				.ToList();

			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.TLM_ParentTableCode, arg.TLM_ParentID, arg.TLM_DeviceID, arg.TLM_PK)).ToArray());
		}
	}
}
