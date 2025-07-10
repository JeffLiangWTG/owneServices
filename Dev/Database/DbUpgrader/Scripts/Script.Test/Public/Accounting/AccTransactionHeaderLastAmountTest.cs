using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(AccTransactionHeaderLastAmount))]
	class AccTransactionHeaderLastAmountTest : DbCreateScriptTest
	{
		public void TestAccTransactionHeaderLastReceiptAmount()
		{
			var helper = new TestDbHelper(TestConnection);

			var companyPK = TestDbHelper.DefaultCompanyPK;
			var orgPK = helper.InsertOrgHeader("ORG1", "Organization 1");
			helper.InsertTransactionHeader("AR", "REC", "INV1001", 100M, helper.ToDate("2020-10-01"), helper.DefaultBranchPK, helper.DefaultDepartmentPK, org: orgPK, gstAmount: 10M, localOtherTaxAmount: 1M);

			var sqlQuery = "SELECT * FROM AccTransactionHeaderLastAmount(@ob_gc, @ob_oh, @ledger, @transactionType) order by LastReceiptAmount";
			var command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@ob_gc", System.Data.SqlDbType.UniqueIdentifier, companyPK);
			command.AddParameter("@ob_oh", System.Data.SqlDbType.UniqueIdentifier, orgPK);
			command.AddParameter("@ledger", System.Data.SqlDbType.Char, "AR");
			command.AddParameter("@transactionType", System.Data.SqlDbType.Char, "REC");

			var result = DataUtils.GetDataTableFromCommand(command);

			AssertEquals("1 row returned", 1, result.Rows.Count);
			AssertEquals(-111M, result.Rows[0]["LastReceiptAmount"]);
		}
	}
}

