using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefRateCodeView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefRateCodeView.Testing
{
	[TestedType(typeof(trgCusRefRateCodeView_Ins))]
	class trgCusRefRateCodeView_Ins_Test : DbCreateScriptTest
	{
		public void TestCusRefRateCodeView_Ins()
		{
			var createRecordsSql = @"
				DECLARE @dataGrouping UNIQUEIDENTIFIER = NEWID()

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'EUN')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (@dataGrouping, 'EUN', 'Europe Uinion', NULL)

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'IT')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (newid(), 'IT', 'Italy', @dataGrouping)

				IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateType where ZZR_RateType = 'DTY' and ZZR_ZZZ_NKDataGrouping = 'EUN')
				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) 
				VALUES(newid(), 'DTY', 'Duty', 1, 'EUN', 'CV')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			var insertSql =
				"INSERT dbo.CusRefRateCodeView (ZY1_PK, ZY1_RateCode, ZY1_Description, ZY1_RateType, ZY1_ZZZ_NKDataGrouping, ZY1_DataSet) VALUES(newid(), 'CUS', 'Cus RateCode', 'DTY', 'IT', 'Z')";
			var exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(insertSql));
			AssertContains("Cannot insert the system-defined Rate Code record with ZY1_DataSet = 'Z'.",
				exceptionSql.Message);

			insertSql =
				@"INSERT dbo.CusRefRateCodeView (ZY1_PK, ZY1_RateCode, ZY1_Description, ZY1_RateType, ZY1_ZZZ_NKDataGrouping, ZY1_DataSet, ZY1_SystemCreateTimeUtc, ZY1_SystemCreateUser, ZY1_SystemLastEditTimeUtc, ZY1_SystemLastEditUser)
VALUES(newid(), 'CUS', 'Cus RateCode', 'DTY', 'IT', 'O', '2019-1-1', '~E', '2020-07-03', '~F')";

			AssertNoExceptionThrown(() =>
			{
				AssertEquals(1, connection.ExecuteNonQuery(insertSql));
			});

			connection.ExecuteReader("SELECT * FROM dbo.CusRefRateCodeView",
				reader =>
				{
					AssertEquals("CUS", (string)reader["ZY1_RateCode"]);
					AssertEquals("Cus RateCode", (string)reader["ZY1_Description"]);
					AssertEquals("DTY", (string)reader["ZY1_RateType"]);
					AssertEquals("IT", (string)reader["ZY1_ZZZ_NKDataGrouping"]);
					AssertEquals("O", (string)reader["ZY1_DataSet"]);
					AssertEquals(false, (bool)reader["ZY1_IsSystem"]);
					AssertEquals(new DateTime(2019, 1, 1), (DateTime)reader["ZY1_SystemCreateTimeUtc"]);
					AssertEquals("~E", (string)reader["ZY1_SystemCreateUser"]);
					AssertEquals(new DateTime(2020, 7, 3), (DateTime)reader["ZY1_SystemLastEditTimeUtc"]);
					AssertEquals("~F", (string)reader["ZY1_SystemLastEditUser"]);
				});
		}
	}
}

