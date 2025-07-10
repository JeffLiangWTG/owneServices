using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(PLAppropriationAccountAcc))]
	class PLAppropriationAccountAccTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestPLAppropriationAccount()
		{
			TestConnection.ExecuteNonQuery($@"INSERT {ScriptDbName}.Customs.BAS__Account(AccountID, AccountKey, AccountType, GLAccountKey)VALUES(newid(), 1, 'GL_PL_APPROPRIATION_ACCOUNT', 111)");
			var sqlText = $"SELECT [{ScriptDbName}].[dbo].PLAppropriationAccountAcc() as PLAA";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("GLAccountKey of GL_PL_APPROPRIATION_ACCOUNT", 111L, result.Rows[0]["PLAA"]);
		}
	}
}

