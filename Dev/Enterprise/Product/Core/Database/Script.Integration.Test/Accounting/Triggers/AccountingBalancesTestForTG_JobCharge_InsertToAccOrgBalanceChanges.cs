using System;
using Enterprise.Build.Database.Script.Public.Accounting.Balances;
using Enterprise.Build.Database.Script.TestFramework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Test
{
	public class AccountingBalancesTestForTG_JobCharge_InsertToAccOrgBalanceChanges : AccOrgBalancesTestCase
	{
		public void TestUnrecognizedTotalForReceivablesTransactions()
		{
			var ledger = "AR";

			Assert_vw_AccOrgBalances(ledger, "Empty Table before aggregation", 0, 0, 0, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "Empty Table after aggregation", 0, 0, 0, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);

			var shipment = helper.InsertShipment("S99999999", new DateTime(2012, 01, 01));
			var job = helper.InsertJob("S99999999", TestDbHelper.DefaultCompanyPK, branch1, department1, "JS", shipment, "WRK", new DateTime(2012, 07, 25));
			var chargeCode = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
			var chargePK = Guid.NewGuid();

			InsertJobCharge(chargePK, job, branch1, company1, department1, chargeCode, org1, 33.333m, Guid.Empty, 0m);
			Assert_vw_AccOrgBalances(ledger, "After insert, before aggregation", 1, 0, 1, org1UnrecognizedAmt: 33.333m, org2UnrecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After insert, after aggregation", 0, 1, 1, org1UnrecognizedAmt: 33.333m, org2UnrecognizedAmt: 0m);

			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_LocalSellAmt = 20.555,
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
WHERE
	JR_PK = '{0}'", chargePK));
			Assert_vw_AccOrgBalances(ledger, "After updating JR_LocalSellAmt, before aggregation", 1, 1, 1, org1UnrecognizedAmt: 20.555m, org2UnrecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating JR_LocalSellAmt, after aggregation", 0, 1, 1, org1UnrecognizedAmt: 20.555m, org2UnrecognizedAmt: 0m);

			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_OH_SellAccount = '{0}',
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
WHERE
	JR_PK = '{1}'", org2, chargePK));
			Assert_vw_AccOrgBalances(ledger, "After updating JR_OH_SellAccount, before aggregation", 2, 1, 2, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 20.555m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating JR_OH_SellAccount, after aggregation", 0, 1, 1, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 20.555m);

			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_ProFormaRevenue = 1,
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
WHERE
	JR_PK = '{0}'", chargePK));
			Assert_vw_AccOrgBalances(ledger, "After setting JR_ProFormaRevenue = 1, before aggregation", 1, 1, 1, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After setting JR_ProFormaRevenue = 1, after aggregation", 0, 0, 0, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);

			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_ProFormaRevenue = 0,
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
WHERE
	JR_PK = '{0}'", chargePK));
			Assert_vw_AccOrgBalances(ledger, "After setting JR_ProFormaRevenue = 0, before aggregation", 1, 0, 1, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 20.555m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After setting JR_ProFormaRevenue = 0, after aggregation", 0, 1, 1, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 20.555m);

			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_OH_SellAccount = NULL,
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
WHERE
	JR_PK = '{0}'", chargePK));
			Assert_vw_AccOrgBalances(ledger, "After setting JR_OH_SellAccount = NULL, before aggregation", 1, 1, 1, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After setting JR_OH_SellAccount = NULL, after aggregation", 0, 0, 0, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);

			TestConnection.ExecuteNonQuery(string.Format("DELETE FROM dbo.JobCharge WHERE JR_PK = '{0}'", chargePK));
			Assert_vw_AccOrgBalances(ledger, "After deleting, before aggregation", 0, 0, 0, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After deleting, after aggregation", 0, 0, 0, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);
		}

		public void TestUnrecognizedTotalForPayablesTransactions()
		{
			var ledger = "AP";

			Assert_vw_AccOrgBalances(ledger, "Empty Table before aggregation", 0, 0, 0, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "Empty Table after aggregation", 0, 0, 0, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);

			var shipment = helper.InsertShipment("S99999999", new DateTime(2012, 01, 01));
			var job = helper.InsertJob("S99999999", TestDbHelper.DefaultCompanyPK, branch1, department1, "JS", shipment, "WRK", new DateTime(2012, 07, 25));
			var chargeCode = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
			var chargePK = Guid.NewGuid();

			InsertJobCharge(chargePK, job, branch1, company1, department1, chargeCode, Guid.Empty, 0m, org1, 44.444m);
			Assert_vw_AccOrgBalances(ledger, "After insert, before aggregation", 1, 0, 1, org1UnrecognizedAmt: 44.444m, org2UnrecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After insert, after aggregation", 0, 1, 1, org1UnrecognizedAmt: 44.444m, org2UnrecognizedAmt: 0m);

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.JobCharge SET JR_LocalCostAmt = 30.222, JR_SystemLastEditUser = '~BP', JR_SystemLastEditTimeUtc = GETUTCDATE() WHERE JR_PK = '{0}'", chargePK));
			Assert_vw_AccOrgBalances(ledger, "After updating JR_LocalCostAmt, before aggregation", 1, 1, 1, org1UnrecognizedAmt: 30.222m, org2UnrecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating JR_LocalCostAmt, after aggregation", 0, 1, 1, org1UnrecognizedAmt: 30.222m, org2UnrecognizedAmt: 0m);

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.JobCharge SET JR_OH_CostAccount = '{0}', JR_SystemLastEditUser = '~BP', JR_SystemLastEditTimeUtc = GETUTCDATE() WHERE JR_PK = '{1}'", org2, chargePK));
			Assert_vw_AccOrgBalances(ledger, "After updating JR_OH_CostAccount, before aggregation", 2, 1, 2, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 30.222m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating JR_OH_CostAccount, after aggregation", 0, 1, 1, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 30.222m);

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.JobCharge SET JR_ProFormaCost = 1, JR_SystemLastEditUser = '~BP', JR_SystemLastEditTimeUtc = GETUTCDATE() WHERE JR_PK = '{0}'", chargePK));
			Assert_vw_AccOrgBalances(ledger, "After setting JR_ProFormaCost = 1, before aggregation", 1, 1, 1, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After setting JR_ProFormaCost = 1, after aggregation", 0, 0, 0, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.JobCharge SET JR_ProFormaCost = 0, JR_SystemLastEditUser = '~BP', JR_SystemLastEditTimeUtc = GETUTCDATE() WHERE JR_PK = '{0}'", chargePK));
			Assert_vw_AccOrgBalances(ledger, "After setting JR_ProFormaCost = 0, before aggregation", 1, 0, 1, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 30.222m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After setting JR_ProFormaCost = 0, after aggregation", 0, 1, 1, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 30.222m);

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.JobCharge SET JR_OH_CostAccount = NULL, JR_SystemLastEditUser = '~BP', JR_SystemLastEditTimeUtc = GETUTCDATE() WHERE JR_PK = '{0}'", chargePK));
			Assert_vw_AccOrgBalances(ledger, "After setting JR_OH_CostAccount = NULL, before aggregation", 1, 1, 1, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After setting JR_OH_CostAccount = NULL, after aggregation", 0, 0, 0, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);

			TestConnection.ExecuteNonQuery(string.Format("DELETE FROM dbo.JobCharge WHERE JR_PK = '{0}'", chargePK));
			Assert_vw_AccOrgBalances(ledger, "After deleting, before aggregation", 0, 0, 0, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After deleting, after aggregation", 0, 0, 0, org1UnrecognizedAmt: 0m, org2UnrecognizedAmt: 0m);
		}
	}
}

