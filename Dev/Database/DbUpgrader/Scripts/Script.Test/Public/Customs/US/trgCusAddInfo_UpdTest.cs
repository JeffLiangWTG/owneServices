using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US.Testing
{
	[TestedType(typeof(trgCusAddInfo_Upd))]
	class trgCusAddInfo_Upd_Test : DbCreateScriptTest
	{
		public void TestUpdateDuplicatedCodes()
		{
			var b7AddInfoData = "Description=SCREW*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI";
			var b7AddInfoData2 = "Description=SCREW*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI";
			const string insertQuery = @"
INSERT INTO dbo.CusAddInfo(B7_PK, B7_Type,B7_AddInfoData,B7_ParentTableCode,B7_ParentID) VALUES
(newid(),'FDA',@B7AddInfoData,'CI',newid()),
(newid(),'PGA',@B7AddInfoData2,'CI',newid())
";
			using (var command = TestConnection.Command(insertQuery))
			{
				command.AddParameter("@B7AddInfoData", SqlDbType.VarChar, b7AddInfoData);
				command.AddParameter("@B7AddInfoData2", SqlDbType.VarChar, b7AddInfoData2);
				command.ExecuteNonQuery();
			}

			var newB7AddInfoData = "*Description=SCREW*IntendedUseCode=081.001*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI*";
			const string updateQuery = @"
UPDATE dbo.CusAddInfo 
SET 
    B7_AddInfoData = @NewB7AddInfoData, 
    B7_SystemLastEditTimeUtc = GETUTCDATE(), 
    B7_SystemLastEditUser = '~BP';";

			using (var command = TestConnection.Command(updateQuery))
			{
				command.AddParameter("@NewB7AddInfoData", SqlDbType.VarChar, newB7AddInfoData);
				var exceptionSql = AssertExceptionThrown<SqlException>(() => command.ExecuteNonQuery());
				AssertContains(@"Update duplicated codes operation NOT allowed on CusAddInfo.", exceptionSql.Message);
			}
		}

		public void TestUpdateManyDuplicatedCodes()
		{
			var b7AddInfoData = "Description=SCREW*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI";
			var b7AddInfoData2 = "Description=SCREW*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI";
			const string insertQuery = @"
INSERT INTO dbo.CusAddInfo(B7_PK, B7_Type,B7_AddInfoData,B7_ParentTableCode,B7_ParentID) VALUES
(newid(),'FDA',@B7AddInfoData,'CI',newid()),
(newid(),'PGA',@B7AddInfoData2,'CI',newid())
";
			using (var command = TestConnection.Command(insertQuery))
			{
				command.AddParameter("@B7AddInfoData", SqlDbType.VarChar, b7AddInfoData);
				command.AddParameter("@B7AddInfoData2", SqlDbType.VarChar, b7AddInfoData2);
				command.ExecuteNonQuery();
			}

			var newB7AddInfoData = "*Description=SCREW*IntendedUseCode=081.001*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI*ProdCountry=DE*";
			const string updateQuery = @"
UPDATE dbo.CusAddInfo 
SET 
    B7_AddInfoData = @NewB7AddInfoData, 
    B7_SystemLastEditTimeUtc = GETUTCDATE(), 
    B7_SystemLastEditUser = '~BP';";
			using (var command = TestConnection.Command(updateQuery))
			{
				command.AddParameter("@NewB7AddInfoData", SqlDbType.VarChar, newB7AddInfoData);
				var exceptionSql = AssertExceptionThrown<SqlException>(() => command.ExecuteNonQuery());
				AssertContains(@"Update duplicated codes operation NOT allowed on CusAddInfo.", exceptionSql.Message);
			}
		}

		public void TestUpdateNoneDuplicatedCodes()
		{
			var b7AddInfoData = "Description=SCREW*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI";
			var b7AddInfoData2 = "Description=SCREW*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI";
			const string insertQuery = @"
INSERT INTO dbo.CusAddInfo(B7_PK, B7_Type,B7_AddInfoData,B7_ParentTableCode,B7_ParentID) VALUES
(newid(),'FDA',@B7AddInfoData,'CI',newid()),
(newid(),'PGA',@B7AddInfoData2,'CI',newid())
";
			using (var command = TestConnection.Command(insertQuery))
			{
				command.AddParameter("@B7AddInfoData", SqlDbType.VarChar, b7AddInfoData);
				command.AddParameter("@B7AddInfoData2", SqlDbType.VarChar, b7AddInfoData2);
				command.ExecuteNonQuery();
			}

			var newB7AddInfoData = "Description=SCREW*IntendedUseCode=081.002*ProdCountry=DE*ProductCode=87M--NI";
			const string updateQuery = @"
UPDATE dbo.CusAddInfo 
SET 
    B7_AddInfoData = @NewB7AddInfoData, 
    B7_SystemLastEditTimeUtc = GETUTCDATE(), 
    B7_SystemLastEditUser = '~BP';";
			using (var command = TestConnection.Command(updateQuery))
			{
				command.AddParameter("@NewB7AddInfoData", SqlDbType.VarChar, newB7AddInfoData);
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}

