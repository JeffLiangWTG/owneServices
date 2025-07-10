using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USGetCusAddInfoDataForAPHISReport))]
	class USGetCusAddInfoDataForAPHISReportTest : DbCreateScriptTest
	{
		public void TestUSGetCusAddInfoDataForAPHISReportTest()
		{
			var jiPK = Guid.NewGuid();
			var aphisSPK = TestDataCreator.CreateCusAddInfo("APH", "A", "JI", jiPK);
			var product1PK = TestDataCreator.CreateCusAddInfo(new Guid("C3E25996-CB49-42B7-BEE3-0001CA0AE11B"), "APP", "B", "B7", aphisSPK);

			var reportSql = @"SELECT * FROM USGetCusAddInfoDataForAPHISReport(@parentID, @type)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, aphisSPK);
				command.AddParameter("@type", SqlDbType.VarChar, "APP");
				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						AssertEquals("B", reader["AddInfoData1"].ToString());
					}
					AssertEquals(1, count);
				}
			}

			var product2PK = TestDataCreator.CreateCusAddInfo(new Guid("6A3FBE12-E6D4-49EF-AA97-00028A6E845F"), "APP", "C", "B7", aphisSPK);

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, aphisSPK);
				command.AddParameter("@type", SqlDbType.VarChar, "APP");
				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						AssertEquals("B", reader["AddInfoData1"].ToString());
						AssertEquals("C", reader["AddInfoData2"].ToString());
					}
					AssertEquals(1, count);
				}
			}
		}
	}
}
