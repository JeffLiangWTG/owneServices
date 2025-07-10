using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefPreferenceView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefPreferenceView
{
	[TestedType(typeof(trgCusRefPreferenceView_Del))]
	class trgCusRefPreferenceView_Del_Test : DbCreateScriptTest
	{
		public void TestDelete()
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
			var delExp = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery("DELETE FROM dbo.CusRefPreferenceView WHERE ZZS_PK='D251DAA0-5A76-4E8D-AA44-501F4B705776'"));
			AssertEquals("Should have thrown a correct exception with correct message", "Cannot delete a system-defined RefPreference.", delExp.Message);

			AssertNoExceptionThrown("Should be able to delete item from CusDB.", () => connection.ExecuteNonQuery("DELETE FROM dbo.CusRefPreferenceView WHERE ZZS_PK='15789BE9-AA4E-410E-AC10-8BCF3882481B'"));
			connection.ExecuteReader("SELECT ZZS_PK FROM dbo.CusRefPreferenceView",
			reader =>
			{
				AssertEquals("Should only be RefDb item left.", "D251DAA0-5A76-4E8D-AA44-501F4B705776", reader["ZZS_PK"].ToString().ToUpper());
			});
		}
	}
}

