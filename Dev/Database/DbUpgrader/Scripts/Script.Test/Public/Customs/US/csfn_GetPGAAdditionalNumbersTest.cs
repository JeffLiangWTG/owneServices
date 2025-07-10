using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(csfn_GetPGAAdditionalNumbers))]
	class csfn_GetPGAAdditionalNumbersTest : DbCreateScriptTest
	{
		public void TestGetCorrectPGAAFMData()
		{
			var b7_ParentPK = Guid.NewGuid();
			var b7_ParentPKOTH = Guid.NewGuid();
			var b7_PK1 = Guid.NewGuid();
			var b7_PK2 = Guid.NewGuid();
			var b7_PK3 = Guid.NewGuid();
			var b7_PK4 = Guid.NewGuid();
			var b7_PKOTH = Guid.NewGuid();
			var sql = @"
			INSERT INTO dbo.CusAddInfo(B7_PK, B7_Type, B7_AddInfoData, B7_ParentID, B7_ParentTableCode)
			VALUES
			(@b7_PK1, 'NTA', 'NHTAdditionalIdentityNumber=222*NHTAdditionalIdentityNumQualifier=AKG', @b7_ParentPK, 'B7'),
			(@b7_PK2, 'NTA', 'NHTAdditionalIdentityNumber=*NHTAdditionalIdentityNumQualifier=AKG', @b7_ParentPK, 'B7'),
			(@b7_PK3, 'NTA', 'NHTAdditionalIdentityNumber=222*NHTAdditionalIdentityNumQualifier=', @b7_ParentPK, 'B7'),
			(@b7_PK4, 'NTH', '', @b7_ParentPK, 'B7'),
			(@b7_PKOTH, 'NTA', 'NHTAdditionalIdentityNumber=222*NHTAdditionalIdentityNumQualifier=AKG', @b7_ParentPKOTH, 'B7')";

			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			{
				command.AddParameter("b7_ParentPK", System.Data.SqlDbType.UniqueIdentifier, b7_ParentPK);
				command.AddParameter("b7_ParentPKOTH", System.Data.SqlDbType.UniqueIdentifier, b7_ParentPKOTH);
				command.AddParameter("b7_PK1", System.Data.SqlDbType.UniqueIdentifier, b7_PK1);
				command.AddParameter("b7_PK2", System.Data.SqlDbType.UniqueIdentifier, b7_PK2);
				command.AddParameter("b7_PK3", System.Data.SqlDbType.UniqueIdentifier, b7_PK3);
				command.AddParameter("b7_PK4", System.Data.SqlDbType.UniqueIdentifier, b7_PK4);
				command.AddParameter("b7_PKOTH", System.Data.SqlDbType.UniqueIdentifier, b7_PKOTH);
				command.ExecuteNonQuery();
			}

			sql = @"select * from csfn_GetPGAAdditionalNumbers(@pgaPK)";
			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			{
				command.AddParameter("@pgaPK", System.Data.SqlDbType.UniqueIdentifier, b7_ParentPK);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("AKG - 222; AKG - ;  - 222", reader["Value"].ToString());
					AssertEquals(false, reader.Read());
				}
			}
		}
	}
}
