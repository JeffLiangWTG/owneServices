using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Utilities.Testing
{
	[TestedType(typeof(GetMultiSubAccountDetailsForGLHeader))]
	class GetMultiSubAccountDetailsForGLHeaderTest : DbCreateScriptTest
	{
		public void TestMultiSubAccountsForGLHeader()
		{
			var helper = new TestDbHelper(TestConnection);
			var accountPK1 = helper.InsertGLAccount("2221.00.00", "Test account 1");
			var accountPK2 = helper.InsertGLAccount("2222.00.00", "Test account 2");
			var accountPK3 = helper.InsertGLAccount("2223.00.00", "Test account 2");

			helper.InsertAccGLHeaderSubAccount(accountPK2, true, "OH");
			helper.InsertAccGLHeaderSubAccount(accountPK2, false, "GG");
			helper.InsertAccGLHeaderSubAccount(accountPK3, true, "OH");
			helper.InsertAccGLHeaderSubAccount(accountPK3, false, "GG");
			helper.InsertAccGLHeaderSubAccount(accountPK3, false, "GS");
			helper.InsertAccGLHeaderSubAccount(accountPK3, false, "AR");

			var rows = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM GetMultiSubAccountDetailsForGLHeader ('{accountPK1}')").AsEnumerable();
			AssertEquals(1, rows.Count());
			AssertEquals(null, rows.ToArray()[0].Field<string>("SubAccountCodeTypes"));

			rows = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM GetMultiSubAccountDetailsForGLHeader ('{accountPK2}')").AsEnumerable();
			AssertEquals(1, rows.Count());
			AssertEquals("ORG: Y, SGP: N", rows.ToArray()[0].Field<string>("SubAccountCodeTypes"));

			rows = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM GetMultiSubAccountDetailsForGLHeader ('{accountPK3}')").AsEnumerable();
			AssertEquals(1, rows.Count());
			AssertEquals("ORG: Y, SEG: N, STR: N, SGP: N", rows.ToArray()[0].Field<string>("SubAccountCodeTypes"));
		}
	}
}

