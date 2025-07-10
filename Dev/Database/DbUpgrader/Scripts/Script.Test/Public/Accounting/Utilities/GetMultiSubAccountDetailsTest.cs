using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Utilities.Testing
{
	[TestedType(typeof(GetMultiSubAccountDetails))]
	class GetMultiSubAccountDetailsTest : DbCreateScriptTest
	{
		public void TestCalculation()
		{
			var helper = new TestDbHelper(TestConnection);

			var jnlPK = helper.InsertTransactionHeader("AP", "JNL", "AA1", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), glAccountPK: new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"));
			var bankAccount = helper.InsertBankAccount("BAA", new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"));
			var gjlPK = helper.InsertTransactionHeader("GL", "GJL", "AA2", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"));
			var gjlLinePK = helper.InsertTransactionLine(gjlPK, null, null, new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "GJL", null, null);

			helper.InsertAccTransactionHeaderSubAccount(jnlPK, "OH", new Guid("0daab61b-255e-4ad7-afc5-4e7b03c3bda1"));
			helper.InsertAccTransactionHeaderSubAccount(jnlPK, "AR", new Guid("333810a4-215d-49c0-be06-92dc8138bc60"));
			helper.InsertAccTransactionLineSubAccount(gjlLinePK, "GG", new Guid("55896e13-12be-4fd4-ac94-2956795a5be2"));
			helper.InsertAccTransactionLineSubAccount(gjlLinePK, "GS", new Guid("ABE1D8D8-A709-4BFA-88E3-53997AA925E2"));
			helper.InsertAccGLHeaderSubAccount(new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"), true, "OH");
			helper.InsertAccGLHeaderSubAccount(new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"), true, "GG");
			helper.InsertAccGLHeaderSubAccount(new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"), false, "GS");
			helper.InsertAccGLHeaderSubAccount(new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"), false, "AR");

			var rows = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM GetMultiSubAccountDetails ('{jnlPK}','245EE66C-4BCC-424D-956B-34077A3AAB96')").AsEnumerable();
			AssertNotNull(rows);
			AssertEquals(1, rows.Count());
			AssertEquals("ORG: ABIGAS, SEG: ADMIN, STR: , SGP: ", rows.ToArray()[0].Field<string>("SubAccountCodeTypes"));
			AssertEquals("ABIGAS", rows.ToArray()[0].Field<string>("OrganisationSubAccount"));
			AssertEquals("ADMIN", rows.ToArray()[0].Field<string>("SalesExpenseGroupsSubAccount"));
			AssertNull(rows.ToArray()[0].Field<string>("StaffAndResourcesSubAccount"));
			AssertNull(rows.ToArray()[0].Field<string>("StaffGroupSubAccount"));

			rows = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM GetMultiSubAccountDetails ('{gjlLinePK}','245EE66C-4BCC-424D-956B-34077A3AAB96')").AsEnumerable();
			AssertEquals(1, rows.Count());
			AssertEquals("ORG: , SEG: , STR: E, SGP: PMG", rows.ToArray()[0].Field<string>("SubAccountCodeTypes"));
			AssertNull(rows.ToArray()[0].Field<string>("OrganisationSubAccount"));
			AssertNull(rows.ToArray()[0].Field<string>("SalesExpenseGroupsSubAccount"));
			AssertEquals("E", rows.ToArray()[0].Field<string>("StaffAndResourcesSubAccount"));
			AssertEquals("PMG", rows.ToArray()[0].Field<string>("StaffGroupSubAccount"));
		}
	}
}

