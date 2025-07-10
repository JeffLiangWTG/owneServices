using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefPreferenceView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefPreferenceView
{
	[TestedType(typeof(trgCusRefPreferenceView_Ins))]
	class trgCusRefPreferenceView_Ins_Test : DbCreateScriptTest
	{
		public void TestInsert()
		{
			var connection = Db.Connection;
			connection.ExecuteNonQuery(@"
IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'CN')
INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
VALUES (NEWID(), 'CN', 'China', NULL)");

			var insExp = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(@"
INSERT INTO dbo.CusRefPreferenceView (ZZS_PK,ZZS_Preference,ZZS_Description,ZZS_ZZZ_NKDataGrouping,ZZS_DataSet,ZZS_IsSystem)
VALUES(NEWID(),'PRX','DESC','CN','Z',CONVERT(BIT, 0))"));
			AssertEquals("Cannot insert a system-defined RefPreference.", insExp.Message);

			insExp = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(@"
INSERT INTO dbo.CusRefPreferenceView (ZZS_PK,ZZS_Preference,ZZS_Description,ZZS_ZZZ_NKDataGrouping,ZZS_DataSet,ZZS_IsSystem)
VALUES(NEWID(),'PRX','DESC','EUN','O',CONVERT(BIT, 0))"));
			AssertEquals("Country code cannot be longer than 2 characters.", insExp.Message);

			AssertNoExceptionThrown(() => connection.ExecuteNonQuery(@"
INSERT INTO dbo.CusRefPreferenceView (ZZS_PK,ZZS_Preference,ZZS_Description,ZZS_ZZZ_NKDataGrouping,ZZS_DataSet,ZZS_IsSystem, ZZS_SystemCreateTimeUtc, ZZS_SystemCreateUser, ZZS_SystemLastEditTimeUtc, ZZS_SystemLastEditUser)
VALUES('17AB4076-F3EF-4926-9F3E-825654C8E5F5','PRY','DESC1','CN','O',CONVERT(BIT, 0), '2019-1-1', '~E', '2020-07-03', '~F')
"));
			connection.ExecuteReader("SELECT * FROM dbo.CusRefPreferenceView",
			reader =>
			{
				AssertEquals("17AB4076-F3EF-4926-9F3E-825654C8E5F5", reader["ZZS_PK"].ToString().ToUpper());
				AssertEquals("PRY", (string)reader["ZZS_Preference"]);
				AssertEquals("DESC1", (string)reader["ZZS_Description"]);
				AssertEquals("CN", (string)reader["ZZS_ZZZ_NKDataGrouping"]);
				AssertEquals("O", (string)reader["ZZS_DataSet"]);
				AssertEquals(false, (bool)reader["ZZS_IsSystem"]);
				AssertEquals(new DateTime(2019, 1, 1), (DateTime)reader["ZZS_SystemCreateTimeUtc"]);
				AssertEquals("~E", (string)reader["ZZS_SystemCreateUser"]);
				AssertEquals(new DateTime(2020, 7, 3), (DateTime)reader["ZZS_SystemLastEditTimeUtc"]);
				AssertEquals("~F", (string)reader["ZZS_SystemLastEditUser"]);
			});
		}
	}
}

