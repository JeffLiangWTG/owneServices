using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow.Portals.GPS;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Portals.GPS
{
	[TestedType(typeof(RetrieveLatestOperatorOnEquipment))]
	class RetrieveLatestOperatorOnEquipmentTest : DbCreateScriptTest
	{
		public void TestRetrieveLatestOperatorOnEquipmentTest()
		{
			CreateData();
			var expectedResults = new[]
			{
				(new Guid("00000000-0000-0001-0000-000000000000"), "RQ", "Abt", "Ab Test"),
				(new Guid("00000000-0000-0002-0000-000000000000"), "RQ", "Bat", "Ba Test"),
			};
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $@"select * from dbo.RetrieveLatestOperatorOnEquipment").Rows.Cast<DataRow>();
			var resultArray = result
				.Select(row => new
				{
					TE_EntityIdFrom = Guid.Parse(row["TE_EntityIdFrom"].ToString()),
					TE_EntityTableCodeFrom = row["TE_EntityTableCodeFrom"].ToString(),
					OperatorCode = row["GS_Code"].ToString(),
					OperatorName = row["GS_FullName"].ToString(),
				})
				.ToList();

			AssertArrayEqualsByElements(expectedResults, resultArray.Select(arg => (arg.TE_EntityIdFrom, arg.TE_EntityTableCodeFrom, arg.OperatorCode, arg.OperatorName)).ToArray());
		}

		protected void CreateData()
		{
			using (var command = Db.Connection.Command(FormattableString.Invariant($@"
					INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
					VALUES
					('{Staffs[0]}', 'Abt', 'Ab Test', 'AbTest', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{Staffs[1]}', 'Bat', 'Ba Test', 'BaTest', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

					INSERT INTO dbo.TelEdge 
					(TE_PK, TE_EntityTableCodeFrom, TE_EntityIdFrom, TE_EntityTableCodeTo, TE_EntityIdTo, TE_StartTime, TE_RelationshipType)
					VALUES
					(NEWID(),'RQ','{Equipments[0]}', 'GS', '{Staffs[0]}', GETUTCDATE(), 'OPT'),
					(NEWID(),'RQ','{Equipments[1]}', 'GS', '{Staffs[1]}', GETUTCDATE(), 'OPT');

					INSERT INTO dbo.TelEdge 
					(TE_PK, TE_EntityTableCodeFrom, TE_EntityIdFrom, TE_EntityTableCodeTo, TE_EntityIdTo, TE_StartTime, TE_EndTime, TE_RelationshipType)
					VALUES
					(NEWID(),'RQ','{Equipments[0]}', 'GS', '{Staffs[1]}', GETUTCDATE(),  GETUTCDATE() +1,  'OPT'),
					(NEWID(),'RQ','{Equipments[1]}', 'GS', '{Staffs[0]}', GETUTCDATE(),  GETUTCDATE() +2,  'OPT');")))
			{
				command.ExecuteNonQuery();
			}
		}

		protected readonly Guid[] Equipments =
		{
			Guid.Parse("00000000-0000-0001-0000-000000000000"),
			Guid.Parse("00000000-0000-0002-0000-000000000000"),
		};

		protected readonly Guid[] Staffs =
		{
			Guid.Parse("00000001-0001-0001-0000-000000000000"),
			Guid.Parse("00000002-0002-0002-0000-000000000000"),
		};
	}
}
