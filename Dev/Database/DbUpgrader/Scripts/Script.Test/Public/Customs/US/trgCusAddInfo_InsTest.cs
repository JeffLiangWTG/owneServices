using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US.Testing
{
	[TestedType(typeof(trgCusAddInfo_Ins))]
	class trgCusAddInfo_Ins_Test : DbCreateScriptTest
	{
		public void TestInsertDuplicatedCodes()
		{
			var b7AddInfoData = "Description=SCREW*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI";
			var b7AddInfoData2 = "*Description=SCREW*IntendedUseCode=081.001*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI*";
			var b7AddInfoData3 = "Description=SCREW*IntendedUseCode=081.001*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI*ProdCountry=DE";
			const string insertQuery = @"
INSERT INTO dbo.CusAddInfo(B7_PK, B7_Type,B7_AddInfoData,B7_ParentTableCode,B7_ParentID) VALUES
(newid(),'FDA',@B7AddInfoData,'CI',newid()),
(newid(),'FDA',@B7AddInfoData2,'CI',newid()),
(newid(),'PGA',@B7AddInfoData3,'CI',newid())
";
			using (var command = TestConnection.Command(insertQuery))
			{
				command.AddParameter("@B7AddInfoData", SqlDbType.VarChar, b7AddInfoData);
				command.AddParameter("@B7AddInfoData2", SqlDbType.VarChar, b7AddInfoData2);
				command.AddParameter("@B7AddInfoData3", SqlDbType.VarChar, b7AddInfoData3);
				var exceptionSql = AssertExceptionThrown<SqlException>(() => command.ExecuteNonQuery());
				AssertContains(@"Insert duplicated codes operation NOT allowed on CusAddInfo.", exceptionSql.Message);
			}
		}

		public void TestInsertManyDuplicatedCodes()
		{
			var b7AddInfoData = "*Description=SCREW*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI**";
			var b7AddInfoData2 = "Description=SCREW*IntendedUseCode=081.001*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI";
			var b7AddInfoData3 = "*Description=SCREW*IntendedUseCode=081.001*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI*ProdCountry=DE*";
			const string insertQuery = @"
INSERT INTO dbo.CusAddInfo(B7_PK, B7_Type,B7_AddInfoData,B7_ParentTableCode,B7_ParentID) VALUES
(newid(),'FDA',@B7AddInfoData,'CI',newid()),
(newid(),'PGA',@B7AddInfoData2,'CI',newid()),
(newid(),'FDA',@B7AddInfoData3,'CI',newid())
";
			using (var command = TestConnection.Command(insertQuery))
			{
				command.AddParameter("@B7AddInfoData", SqlDbType.VarChar, b7AddInfoData);
				command.AddParameter("@B7AddInfoData2", SqlDbType.VarChar, b7AddInfoData2);
				command.AddParameter("@B7AddInfoData3", SqlDbType.VarChar, b7AddInfoData3);
				var exceptionSql = AssertExceptionThrown<SqlException>(() => command.ExecuteNonQuery());
				AssertContains(@"Insert duplicated codes operation NOT allowed on CusAddInfo.", exceptionSql.Message);
			}
		}

		public void TestInsertNoneDuplicatedCodes()
		{
			var b7AddInfoData = "Description=SCREW*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI";
			var b7AddInfoData2 = "Description=SCREW*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI";
			var b7AddInfoData3 = "Description=SCREW*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI";
			const string insertQuery = @"
INSERT INTO dbo.CusAddInfo(B7_PK, B7_Type,B7_AddInfoData,B7_ParentTableCode,B7_ParentID,B7_SystemCreateTimeUtc,B7_SystemCreateUser,B7_SystemLastEditTimeUtc,B7_SystemLastEditUser) VALUES
(newid(),'PGA',@B7AddInfoData,'CI',newid(),GetUtcDate(),'~BP',GetUtcDate(),'~BP'),
(newid(),'FDA',@B7AddInfoData2,'CI',newid(),GetUtcDate(),'~BP',GetUtcDate(),'~BP'),
(newid(),'FDA',@B7AddInfoData3,'CI',newid(),GetUtcDate(),'~BP',GetUtcDate(),'~BP')
";
			using (var command = TestConnection.Command(insertQuery))
			{
				command.AddParameter("@B7AddInfoData", SqlDbType.VarChar, b7AddInfoData);
				command.AddParameter("@B7AddInfoData2", SqlDbType.VarChar, b7AddInfoData2);
				command.AddParameter("@B7AddInfoData3", SqlDbType.VarChar, b7AddInfoData3);
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}

