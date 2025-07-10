using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefPreferenceView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefPreferenceView
{
	[TestedType(typeof(trgCusRefPreferenceView_Upd))]
	class trgCusRefPreferenceView_Upd_Test : DbCreateScriptTest
	{
		public void TestUpdate()
		{
			var connection = Db.Connection;
			connection.ExecuteNonQuery(@"

IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'CN')
INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
VALUES (NEWID(), 'CN', 'China', NULL)

INSERT INTO RefDatabase_RefCusPreference(ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
VALUES('D251DAA0-5A76-4E8D-AA44-501F4B705776','PRX','Test Preference ZZ','CN')

INSERT INTO dbo.CusRefPreference(CR8_PK,CR8_Preference,CR8_Description,CR8_RN_NKCountryCode, CR8_SystemCreateTimeUtc, CR8_SystemCreateUser, CR8_SystemLastEditTimeUtc, CR8_SystemLastEditUser)
VALUES('15789BE9-AA4E-410E-AC10-8BCF3882481B','PRY','Test Preference Cus','CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
");
			var updExp = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery("UPDATE dbo.CusRefPreferenceView SET ZZS_Preference='PR1',ZZS_DataSet='O' WHERE ZZS_PK='D251DAA0-5A76-4E8D-AA44-501F4B705776'"));
			AssertEquals("Should have thrown a correct exception with correct message", "Cannot update a system-defined RefPreference.", updExp.Message);

			updExp = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery("UPDATE dbo.CusRefPreferenceView SET ZZS_Preference='PR1',ZZS_ZZZ_NKDataGrouping='EUN' WHERE ZZS_PK='15789BE9-AA4E-410E-AC10-8BCF3882481B'"));
			AssertEquals("Should have thrown a correct exception with correct message", "Country code cannot be longer than 2 characters.", updExp.Message);

			AssertNoExceptionThrown(() => connection.ExecuteNonQuery(@"
UPDATE dbo.CusRefPreferenceView SET ZZS_Preference='PR1',ZZS_Description='DESC2',ZZS_ZZZ_NKDataGrouping='IT', ZZS_SystemLastEditTimeUtc='2020-07-03', ZZS_SystemLastEditUser='NEW' WHERE ZZS_PK='15789BE9-AA4E-410E-AC10-8BCF3882481B'
"));
			connection.ExecuteReader("SELECT * FROM dbo.CusRefPreferenceView WHERE ZZS_PK='15789BE9-AA4E-410E-AC10-8BCF3882481B'",
			reader =>
			{
				AssertEquals("PR1", (string)reader["ZZS_Preference"]);
				AssertEquals("DESC2", (string)reader["ZZS_Description"]);
				AssertEquals("IT", (string)reader["ZZS_ZZZ_NKDataGrouping"]);
				AssertEquals("O", (string)reader["ZZS_DataSet"]);
				AssertEquals(false, (bool)reader["ZZS_IsSystem"]);
				AssertEquals(new DateTime(2020, 7, 3), (DateTime)reader["ZZS_SystemLastEditTimeUtc"]);
				AssertEquals("NEW", (string)reader["ZZS_SystemLastEditUser"]);
			});
		}
	}
}

