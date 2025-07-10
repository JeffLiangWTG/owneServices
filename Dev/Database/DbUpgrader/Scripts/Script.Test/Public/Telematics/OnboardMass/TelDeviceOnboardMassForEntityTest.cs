using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics.OnboardMass;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.OnboardMass
{
	[TestedType(typeof(TelDeviceOnboardMassForEntity))]
	class TelDeviceOnboardMassForEntityTest : TelDeviceMarksForEntityTestBase
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
			return $@"SELECT * FROM dbo.TelDeviceOnboardMassForEntity('RQ', '{entityPk}', '2017-01-15', '2017-02-15') ORDER BY GDM_MeasurementTimeUtc";
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
					GDM_PK = Guid.Parse(row["GDM_PK"].ToString()),
					GDM_MeasurementTimeUtc = row["GDM_MeasurementTimeUtc"],
					GDM_PressureKPa = row["GDM_PressureKPa"],
					GDM_DeviationPercent = row["GDM_DeviationPercent"]
				})
				.ToList();

			// Assert
			AssertArrayEqualsByElements(expectedPks, resultArray.Select(arg => Tuple.Create(arg.V7_ParentTableCode, arg.V7_ParentID, arg.V3_PK, arg.GDM_PK)).ToArray());
		}
	}
}
