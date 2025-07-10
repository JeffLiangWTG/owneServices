using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(csfn_GetPGAAOCData))]
	class csfn_GetPGAAOCDataTest : DbCreateScriptTest
	{
		public void TestGetCorrectPGAAFMData()
		{
			var cy_ParentPK = Guid.NewGuid();
			var cy_ParentPKOTH = Guid.NewGuid();
			var cy_PK1 = Guid.NewGuid();
			var cy_PK2 = Guid.NewGuid();
			var cy_PK3 = Guid.NewGuid();
			var cy_PK4 = Guid.NewGuid();
			var cy_PK5 = Guid.NewGuid();
			var cy_PK6 = Guid.NewGuid();
			var cy_PK7 = Guid.NewGuid();
			var cy_PK8 = Guid.NewGuid();
			var cy_PKOTH = Guid.NewGuid();
			var sql = @"
			INSERT INTO dbo.CusCodeData(CY_PK, CY_Type, CY_Code, CY_ParentID, CY_ParentTableCode, CY_IsValid, CY_Order, CY_IsOverridden, CY_Data)
			VALUES
			(@cy_PK1, 'AFM', 'ABC', @cy_ParentPK, 'B7', 1, 1, 0, '001'),
			(@cy_PK2, 'AFM', 'DEF', @cy_ParentPK, 'B7', 0, 2, 0, '002'),
			(@cy_PK3, 'AFM', 'GHI', @cy_ParentPK, 'B7', 1, 3, 0, '003'),
			(@cy_PK4, 'AFM', 'JKM', @cy_ParentPK, 'B7', 0, 4, 0, '004'),
			(@cy_PK5, 'AFM', 'NOP', @cy_ParentPK, 'B7', 1, 5, 0, '005'),
			(@cy_PK6, 'AFM', 'QIS', @cy_ParentPK, 'B7', 0, 6, 0, '006'),
			(@cy_PK7, 'AFM', 'TUV', @cy_ParentPK, 'B7', 1, 7, 0, '007'),
			(@cy_PK8, 'BLA', 'WXY', @cy_ParentPK, 'B7', 1, 8, 0, '008'),
			(@cy_PKOTH, 'BLA', 'ZZZ', @cy_ParentPKOTH, 'B7', 1, 9, 0, '009')";

			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			{
				command.AddParameter("cy_ParentPK", System.Data.SqlDbType.UniqueIdentifier, cy_ParentPK);
				command.AddParameter("cy_ParentPKOTH", System.Data.SqlDbType.UniqueIdentifier, cy_ParentPKOTH);
				command.AddParameter("cy_PK1", System.Data.SqlDbType.UniqueIdentifier, cy_PK1);
				command.AddParameter("cy_PK2", System.Data.SqlDbType.UniqueIdentifier, cy_PK2);
				command.AddParameter("cy_PK3", System.Data.SqlDbType.UniqueIdentifier, cy_PK3);
				command.AddParameter("cy_PK4", System.Data.SqlDbType.UniqueIdentifier, cy_PK4);
				command.AddParameter("cy_PK5", System.Data.SqlDbType.UniqueIdentifier, cy_PK5);
				command.AddParameter("cy_PK6", System.Data.SqlDbType.UniqueIdentifier, cy_PK6);
				command.AddParameter("cy_PK7", System.Data.SqlDbType.UniqueIdentifier, cy_PK7);
				command.AddParameter("cy_PK8", System.Data.SqlDbType.UniqueIdentifier, cy_PK8);
				command.AddParameter("cy_PKOTH", System.Data.SqlDbType.UniqueIdentifier, cy_PKOTH);
				command.ExecuteNonQuery();
			}

			sql = @"select * from csfn_GetPGAAOCData(@pgaPK)";
			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			{
				command.AddParameter("@pgaPK", System.Data.SqlDbType.UniqueIdentifier, cy_ParentPK);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("ABC - 001; DEF - 002; GHI - 003; JKM - 004; NOP - 005", reader["Value"].ToString());
				}
			}
		}
	}
}
