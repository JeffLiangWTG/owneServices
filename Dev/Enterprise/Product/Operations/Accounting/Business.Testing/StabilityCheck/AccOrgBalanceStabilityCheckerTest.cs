using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.StabilityCheck.Testing
{
	public class AccOrgBalanceStabilityCheckerTest : TestCaseWithFactory
	{
		public void TestStabilityChecker()
		{
			var testObjCreator = new TestObjectCreator(Factory);
			var invoice = testObjCreator.CreateARInvoice<ARInvoice>("00001001", testObjCreator.AUD, 1m, testObjCreator.ABIGAS);
			testObjCreator.CreateARInvoiceLine(invoice, null, testObjCreator.CC1, testObjCreator.AUD, 1m, "ArInvoice Line", 25m);
			Factory.Save();

			AssertStabilityChecker();
		}

		public void TestStabilityChecker_OnlyClaimTotalDiff()
		{
			var testObjCreator = new TestObjectCreator(Factory);
			var debtor = testObjCreator.ABIGAS;
			Factory.Save();
			var orgContact = testObjCreator.CreateContact(debtor);

			var invoice = testObjCreator.CreateARInvoice<ARInvoice>("00001001", testObjCreator.AUD, 1m, debtor);
			testObjCreator.CreateARInvoiceLine(invoice, null, testObjCreator.CC1, testObjCreator.AUD, 1m, "ArInvoice Line", 0m);
			testObjCreator.CreateClaim(typeof(ARAccQueryClaim), 10m, invoice.PK, debtor.PK, orgContact.PK, QueryClaimStatusCodeList.Codes.QCStatus1Open);
			Factory.Save();

			AssertStabilityChecker();
		}

		void AssertStabilityChecker()
		{
			var result = new AccOrgBalanceStabilityChecker().Check();
			AssertEquals("Stability Result Count when evertyhing is working", 0, result.Length);

			using (var cmd = Db.Connection.Command("DELETE FROM dbo.AccOrgBalanceChanges")) // This is a test and we don't have access to these tables in ZArchitecture.
			{
				cmd.ExecuteNonQuery();
			}

			result = new AccOrgBalanceStabilityChecker().Check();

			AssertEquals("Stability Result Count when there is a problem", 1, result.Length);
			AssertEquals("StabilityLevel", StabilityChecker.StabilityResultLevel.Warning, result[0].StabilityLevel);
			AssertEquals("Description", "Stored accounting balances did not match balances calculated from detailed records. This has been repaired successfully and now stored balances match the balances calculated from detailed records.", result[0].Description);
		}

		public void TestStabilityCheckerHandlesSQLException()
		{
			try
			{
				var firstAttempt = DataUtils.GetDataTableFromQuery(Db.Connection, "DROP PROCEDURE CheckAndRepairAccOrgBalance");
				var result = new AccOrgBalanceStabilityChecker().Check();
				AssertEquals("Stability Result Count when there is an exception", 1, result.Length);
				AssertEquals("StabilityLevel", StabilityChecker.StabilityResultLevel.Exception, result[0].StabilityLevel);
				AssertEquals("Description", "There was a problem accessing the Database. Could not find stored procedure 'CheckAndRepairAccOrgBalance'.", result[0].Description);
			}
			catch (System.Data.Common.DbException ex)
			{
				Fail("Sql Exception should be caught in Check Method: " + ex.Message);
			}
		}
	}
}
