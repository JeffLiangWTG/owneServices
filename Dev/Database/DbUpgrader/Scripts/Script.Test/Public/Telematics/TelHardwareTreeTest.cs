using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics
{
	[TestedType(typeof(TelHardwareTree))]
	class TelHardwareTreeTest : TelEdgeTestCase
	{
		public void TestReturnsTelSubEquipment()
		{
			// Arrange
			GenerateData();

			// Act
			var result = DataUtils.GetDataTableFromQuery(TestConnection, @"
SELECT
	THT_PK,
	THT_EntityTableCodeFrom,
	THT_EntityIdFrom,
	THT_HardwareIdFrom,
	THT_TemplateFrom,
	THT_EntityTableCodeTo,
	THT_EntityIdTo,
	THT_HardwareIdTo,
	THT_StartTime,
	THT_EndTime,
	THT_TemplateMapping,
	THT_Depth
FROM
	dbo.TelHardwareTree('0102030405060708090A0B0C', '2017-01-15')
ORDER BY
	THT_Depth
")
				.Rows.Cast<DataRow>()
				.Select(row => Tuple.Create(
					row["THT_EntityTableCodeFrom"].ToString(), Guid.Parse(row["THT_EntityIdFrom"].ToString()), row["THT_HardwareIdFrom"].ToString(), row["THT_TemplateFrom"].ToString(),
					row["THT_EntityTableCodeTo"].ToString(), Guid.Parse(row["THT_EntityIdTo"].ToString()), row["THT_HardwareIdTo"].ToString(),
					int.Parse(row["THT_Depth"].ToString())))
				.ToArray();

			// Assert
			AssertArrayEqualsByElements(new[]
			{
				Tuple.Create("RQ", new Guid("00000000-1000-1000-0000-000000000000"), "0102030405060708090A0B0C", "<root><ThisIsMyTemplate /></root>", "A", new Guid("00000000-0000-0000-1111-000000000000"), "00000000", 0),
				Tuple.Create("A", new Guid("00000000-0000-0000-1111-000000000000"), "00000000", "", "W", new Guid("00000000-0000-A000-0000-000000000000"), "10000001", 1),
				Tuple.Create("A", new Guid("00000000-0000-0000-1111-000000000000"), "00000000", "", "", new Guid("00000000-0000-B000-0000-000000000000"), "", 1),
				Tuple.Create("", new Guid("00000000-0000-B000-0000-000000000000"), "", "", "O", new Guid("00000000-0000-C000-0000-000000000000"), "20000001", 2)
			}, result);
		}

		protected override void GenerateData()
		{
			base.GenerateData();

			using (var command = Db.Connection.Command(@"
INSERT dbo.RefEquipmentTemplate (RET_PK, RET_Template, RET_Description) VALUES
	('00000000-1000-1000-0000-000000000000', '<root><ThisIsMyTemplate/></root>', 'd1')
;

INSERT dbo.TelSubEquipment (TSE_PK, TSE_Type, TSE_Id, TSE_Configuration) VALUES
	('00000000-1000-1000-0000-000000000000', 'RQ', '0102030405060708090A0B0C', '<EmptyXml />'),

	('00000000-0000-0000-1111-000000000000', 'A', '00000000', '<EmptyXml />'),
	('00000000-0000-A000-0000-000000000000', 'W', '10000001', ''),
	--('00000000-0000-B000-0000-000000000000', 'W', '10000002', ''),
	('00000000-0000-C000-0000-000000000000', 'O', '20000001', '')
;
"))
			{
				command.ExecuteNonQuery();
			}
		}
	}
}
