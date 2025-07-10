using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(csfn_CAConvertEntryTypeToLongCode))]
	class csfn_CAConvertEntryTypeToLongCodeTest : DbCreateScriptTest
	{
		public void Testcsfn_CAConvertEntryTypeToLongCodeTest()
		{
			CombineAssertions(() =>
			{
				AssertEntryTypeFixed("", "");
				AssertEntryTypeFixed("1", "1");
				AssertEntryTypeFixed("ab", "ab");
				AssertEntryTypeFixed("101", "10-1");
				AssertEntryTypeFixed("401", "401");
			});
		}

		void AssertEntryTypeFixed(string originCode, string expectedLongCode)
		{
			const string sql = @"select * from dbo.csfn_CAConvertEntryTypeToLongCode(@EntryType)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@EntryType", System.Data.SqlDbType.Char, originCode);
				var havingResult = false;
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					havingResult = true;
					AssertEquals(expectedLongCode, reader.GetString(0).TrimEnd());
				}
				Assert("Should have result", havingResult);
			}
		}
	}
}
