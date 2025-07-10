using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics
{
	[TestedType(typeof(TelEquipmentMapping))]
	class TelEquipmentMappingTest : DbCreateScriptTest
	{
		public void TestReturnsTelSubEquipment()
		{
			// Arrange
			using (var command = Db.Connection.Command(@"
INSERT dbo.TelSubEquipment (TSE_PK, TSE_Type, TSE_Id, TSE_Configuration, TSE_SystemCreateTimeUtc, TSE_SystemCreateUser, TSE_SystemLastEditTimeUtc, TSE_SystemLastEditUser) VALUES
	('00000000-0000-0000-0000-000000000000', 'A', '00000000', '<EmptyXml />', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('00000000-1000-1000-0000-000000000000', 'W', '10000001', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('00000000-1000-2000-0000-000000000000', 'W', '10000002', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('00000000-2000-1000-0000-000000000000', 'O', '20000001', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
;
"))
			{
				command.ExecuteNonQuery();
			}

			// Act
			var result = DataUtils.GetDataTableFromQuery(TestConnection, @"
SELECT
	TEM_EntityTableCode,
	TEM_EntityId,
	TEM_HardwareId
FROM
	dbo.TelEquipmentMapping('2017-01-15')
ORDER BY
	TEM_EntityTableCode,
	TEM_EntityId,
	TEM_HardwareId
")
				.Rows.Cast<DataRow>()
				.Select(row => Tuple.Create(row["TEM_EntityTableCode"].ToString(), Guid.Parse(row["TEM_EntityId"].ToString()), row["TEM_HardwareId"].ToString()))
				.ToArray();

			// Assert
			AssertArrayEqualsByElements(new[]
			{
				Tuple.Create("A", new Guid("00000000-0000-0000-0000-000000000000"), "00000000"),
				Tuple.Create("O", new Guid("00000000-2000-1000-0000-000000000000"), "20000001"),
				Tuple.Create("W", new Guid("00000000-1000-1000-0000-000000000000"), "10000001"),
				Tuple.Create("W", new Guid("00000000-1000-2000-0000-000000000000"), "10000002")
			}, result);
		}
	}
}
