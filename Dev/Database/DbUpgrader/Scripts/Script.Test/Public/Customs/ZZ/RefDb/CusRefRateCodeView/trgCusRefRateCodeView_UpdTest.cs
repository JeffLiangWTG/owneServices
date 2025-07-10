using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefRateCodeView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefRateCodeView.Testing
{
	[TestedType(typeof(trgCusRefRateCodeView_Upd))]
	class trgCusRefRateCodeView_Upd_Test : DbCreateScriptTest
	{
		public void TestCusRefRateCodeView_Upd()
		{
			var createRecordsSql = @"
				DECLARE @dataGrouping UNIQUEIDENTIFIER = NEWID()
				DECLARE @rateTypePK UNIQUEIDENTIFIER = NEWID()

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'EUN')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (@dataGrouping, 'EUN', 'Europe Uinion', NULL)

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'IT')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (newid(), 'IT', 'Italy', @dataGrouping)

				IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateType where ZZR_RateType = 'DTY' and ZZR_ZZZ_NKDataGrouping = 'IT')
				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) 
				VALUES(@rateTypePK, 'DTY', 'Other Duty', 1, 'IT', '11')

				IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateCode where ZY1_RateCode = 'ZZ1')
				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
				VALUES(newid(), 'ZZ1', @rateTypePK, 'ZZ RateCode')

				IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateType where ZZR_RateType = 'DTY' and ZZR_ZZZ_NKDataGrouping = 'EUN')
				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) 
				VALUES(newid(), 'DTY', 'Duty', 1, 'EUN', 'CV')

				INSERT dbo.CusRefRateCodeView (ZY1_PK, ZY1_RateCode, ZY1_Description, ZY1_RateType, ZY1_ZZZ_NKDataGrouping, ZY1_DataSet, ZY1_SystemCreateTimeUtc, ZY1_SystemCreateUser, ZY1_SystemLastEditTimeUtc, ZY1_SystemLastEditUser)
				VALUES(newid(), 'CUS', 'Cus RateCode', 'DTY', 'IT', 'O', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			var updateSql = "UPDATE dbo.CusRefRateCodeView SET ZY1_DataSet = 'Z' WHERE ZY1_RateCode = 'CUS'";
			var exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(updateSql));
			AssertContains("Cannot update the system-defined Rate Code record with ZY1_DataSet = 'Z'.", exceptionSql.Message);

			updateSql = "UPDATE dbo.CusRefRateCodeView SET ZY1_Description = 'Change ZZ Desc' WHERE ZY1_RateCode = 'ZZ1'";
			exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(updateSql));
			AssertContains("Cannot update the system-defined Rate Code record with ZY1_DataSet = 'Z'.", exceptionSql.Message);

			updateSql = "UPDATE dbo.CusRefRateCodeView SET ZY1_DataSet = 'Z' WHERE ZY1_RateCode = 'ZZ1'";
			exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(updateSql));
			AssertContains("Cannot update the system-defined Rate Code record with ZY1_DataSet = 'Z'.", exceptionSql.Message);

			updateSql = "UPDATE dbo.CusRefRateCodeView SET ZY1_DataSet = 'O', ZY1_Description = 'Change User Desc', ZY1_SystemLastEditTimeUtc='2020-07-03', ZY1_SystemLastEditUser='NEW' WHERE ZY1_RateCode = 'CUS'";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(1, connection.ExecuteNonQuery(updateSql));
			});

			var selectSql = "SELECT * FROM dbo.CusRefRateCodeView WHERE ZY1_RateCode = 'CUS'";
			var results = new List<string>();
			connection.ExecuteReader(selectSql, reader =>
			{
				results.Add($@"{reader["ZY1_IsSystem"]}, {reader["ZY1_DataSet"]}, {reader["ZY1_RateType"]}, {reader["ZY1_Description"]}, {reader["ZY1_ZZZ_NKDataGrouping"]}, {reader["ZY1_SystemLastEditTimeUtc"]}, {reader["ZY1_SystemLastEditUser"]}");
			});
			AssertContainsExactElementsInAnyOrder(new[] { "False, O, DTY, Change User Desc, IT, 3/07/2020 12:00:00 AM, NEW" }, results);
		}
	}
}

