using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefRateCodeView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefRateCodeView.Testing
{
	[TestedType(typeof(trgCusRefRateCodeView_Del))]
	class trgCusRefRateCodeView_Del_Test : DbCreateScriptTest
	{
		public void TestCusRefRateCodeView_Del()
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

				IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateType where ZZR_RateType = 'DTY' and ZZR_ZZZ_NKDataGrouping = 'EUN')
				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) 
				VALUES(@rateTypePK, 'DTY', 'Duty', 1, 'EUN', 'CV')

				IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateCode where ZY1_RateCode = 'ZZ1')
				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
				VALUES(newid(), 'ZZ1', @rateTypePK, 'ZZ RateCode')

				INSERT dbo.CusRefRateCodeView (ZY1_PK, ZY1_RateCode, ZY1_Description, ZY1_RateType, ZY1_ZZZ_NKDataGrouping, ZY1_DataSet, ZY1_SystemCreateTimeUtc, ZY1_SystemCreateUser, ZY1_SystemLastEditTimeUtc, ZY1_SystemLastEditUser)
				VALUES(newid(), 'CUS', 'Cus RateCode', 'DTY', 'IT', 'O', '2020-07-01', '~E1', '2021-07-01', '~F1')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			var deleteSql = "DELETE dbo.CusRefRateCodeView WHERE ZY1_DataSet = 'Z'";
			var exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(deleteSql));
			AssertContains("Cannot delete the system-defined Rate Code record with ZY1_DataSet = 'Z'.", exceptionSql.Message);

			deleteSql = "DELETE dbo.CusRefRateCodeView WHERE ZY1_DataSet = 'O'";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(1, connection.ExecuteNonQuery(deleteSql));
			});
		}
	}
}

