using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.Build.Database.Script.Public.Accounting.Balances
{
	public abstract class AccOrgBalancesTestCase : TransactionedTestCase
	{
		protected void TestBalanceTotal(string ledger)
		{
			decimal originalAmount = 100.111m, originalGSTAmt = 10.222m, newInvoiceAmount = 111.111m, newOutstandingAmount = 10m;

			Assert_vw_AccOrgBalances(ledger, "Empty Table before aggregation", 0, 0, 0, org1BalanceAmt: 0m, org2BalanceAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "Empty Table after aggregation", 0, 0, 0, org1BalanceAmt: 0m, org2BalanceAmt: 0m);

			var transactionPK = helper.InsertTransactionHeader("AR", "INV", "00001001", originalAmount, DateTime.Now, branch1, department1, companyPK: company1, org: org1, gstAmount: originalGSTAmt, outstandingAmount: originalAmount + originalGSTAmt);
			Assert_vw_AccOrgBalances(ledger, "After insert, before aggregation", 1, 0, 1, org1BalanceAmt: originalAmount + originalGSTAmt, org2BalanceAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After insert, after aggregation", 0, 1, 1, org1BalanceAmt: originalAmount + originalGSTAmt, org2BalanceAmt: 0m);

			//TODO: Hello, fellow developer! If this test is failing, uncomment the "DISABLE TRIGGER" command below and remove this TODO line
			//HACK: Temporarily allow UPDATE of AH_InvoiceAmount to test AggregateAccOrgBalanceChangesIntoAccOrgBalance functionality. Refer to WI00559931, WI00482153.
			//TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_AccTransactionHeader_ProtectCriticalFieldsFromUpdating ON AccTransactionHeader");

			// Changing the AH_InvoiceAmount should do nothing because the calculation is based on AH_OutstandingAmount
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.AccTransactionHeader SET AH_InvoiceAmount = {0}, AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK = '{1}'", newInvoiceAmount, transactionPK));
			Assert_vw_AccOrgBalances(ledger, "After updating AH_InvoiceAmount, before aggregation", 0, 1, 1, org1BalanceAmt: originalAmount + originalGSTAmt, org2BalanceAmt: 0m);

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.AccTransactionHeader SET AH_OutstandingAmount = {0}, AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK = '{1}'", newOutstandingAmount, transactionPK));
			Assert_vw_AccOrgBalances(ledger, "After updating AH_OutstandingAmount, before aggregation", 1, 1, 1, org1BalanceAmt: newOutstandingAmount, org2BalanceAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating AH_OutstandingAmount, after aggregation", 0, 1, 1, org1BalanceAmt: newOutstandingAmount, org2BalanceAmt: 0m);

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.AccTransactionHeader SET AH_OH = '{0}', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK = '{1}'", org2, transactionPK));
			Assert_vw_AccOrgBalances(ledger, "After updating AH_OH, before aggregation", 2, 1, 2, org1BalanceAmt: 0m, org2BalanceAmt: newOutstandingAmount);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating AH_OH, after aggregation", 0, 1, 1, org1BalanceAmt: 0m, org2BalanceAmt: newOutstandingAmount);

			TestConnection.ExecuteNonQuery(string.Format("DELETE FROM dbo.AccTransactionHeader WHERE AH_PK = '{0}'", transactionPK));
			Assert_vw_AccOrgBalances(ledger, "After deleting, before aggregation", 1, 1, 1, org1BalanceAmt: 0m, org2BalanceAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After deleting, after aggregation", 0, 0, 0, org1BalanceAmt: 0m, org2BalanceAmt: 0m);

			transactionPK = helper.InsertTransactionHeader("AR", "INV", "00001001", originalAmount, DateTime.Now, branch1, department1, companyPK: company1, org: org1, gstAmount: originalGSTAmt, outstandingAmount: originalAmount + originalGSTAmt);
			RunAggregation();
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.AccTransactionHeader SET AH_InvoiceAmount = {0}, AH_FullyPaidDate = '{1}', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK = '{2}'", newInvoiceAmount, ZDateTime.Now, transactionPK));
			Assert_vw_AccOrgBalances(ledger, "After updating AH_OutstandingAmount and AH_FullyPaidDate, before aggregation", 0, 1, 1, org1BalanceAmt: originalAmount + originalGSTAmt, org2BalanceAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating AH_OutstandingAmount and AH_FullyPaidDate, after aggregation", 0, 1, 1, org1BalanceAmt: originalAmount + originalGSTAmt, org2BalanceAmt: 0m);
		}

		protected void TestRecognizedTotal(string ledger)
		{
			decimal originalLineAmount = 50.444m, newLineAmount = 20.555m;

			Assert_vw_AccOrgBalances(ledger, "Empty Table before aggregation", 0, 0, 0, org1RecognizedAmt: 0m, org2RecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "Empty Table after aggregation", 0, 0, 0, org1RecognizedAmt: 0m, org2RecognizedAmt: 0m);

			var linePK = Guid.NewGuid();
			InsertTransactionLine(linePK, "WIP", org1, originalLineAmount, branch1, department1, company1, new DateTime(2012, 01, 01));
			Assert_vw_AccOrgBalances(ledger, "After insert, before aggregation", 1, 0, 1, org1RecognizedAmt: originalLineAmount, org2RecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After insert, after aggregation", 0, 1, 1, org1RecognizedAmt: originalLineAmount, org2RecognizedAmt: 0m);

			//TODO: Hello, fellow developer! If this test is failing on the next line, uncomment the "DISABLE TRIGGER" command below, remove this TODO line, and resurrect this discussion: https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/121989?_a=files&path=%2FEnterprise%2FProduct%2FCore%2FDatabase%2FScript.Integration.Test%2FAccounting%2FBalances%2FAccOrgBalancesTestCaseTest.cs&discussionId=692939
			//HACK: Temporarily allow UPDATE of AL_LineAmount to test aggregation by TG_AccTransactionLines_InsertToAccOrgBalanceChanges. Note that this is not a valid production scenario. Refer to WI00559931, WI00482153.
			//TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_AccTransactionLines_ProtectCriticalFieldsFromUpdating ON AccTransactionLines");

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.AccTransactionLines SET AL_LineAmount = {0}, AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{1}'", ledger == "AP" ? newLineAmount : -newLineAmount, linePK));
			Assert_vw_AccOrgBalances(ledger, "After updating AL_LineAmount, before aggregation", 1, 1, 1, org1RecognizedAmt: newLineAmount, org2RecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating AL_LineAmount, after aggregation", 0, 1, 1, org1RecognizedAmt: newLineAmount, org2RecognizedAmt: 0m);

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.AccTransactionLines SET AL_OH = '{0}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{1}'", org2, linePK));
			Assert_vw_AccOrgBalances(ledger, "After updating AL_OH, before aggregation", 2, 1, 2, org1RecognizedAmt: 0m, org2RecognizedAmt: newLineAmount);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating AL_OH, after aggregation", 0, 1, 1, org1RecognizedAmt: 0m, org2RecognizedAmt: newLineAmount);

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.AccTransactionLines SET AL_ReverseDate = '2012-01-01', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{0}'", linePK));
			Assert_vw_AccOrgBalances(ledger, "After setting AL_ReverseDate, before aggregation", 1, 1, 1, org1RecognizedAmt: 0m, org2RecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After setting AL_ReverseDate, after aggregation", 0, 0, 0, org1RecognizedAmt: 0m, org2RecognizedAmt: 0m);

			TestConnection.ExecuteNonQuery(string.Format(@"UPDATE dbo.AccTransactionLines SET AL_LineType = '{0}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{1}'
DELETE FROM dbo.AccTransactionLines WHERE AL_PK = '{1}'", TransactionLineTypes.UnapprovedCost, linePK));
			Assert_vw_AccOrgBalances(ledger, "After deleting, before aggregation", 0, 0, 0, org1RecognizedAmt: 0m, org2RecognizedAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After deleting, after aggregation", 0, 0, 0, org1RecognizedAmt: 0m, org2RecognizedAmt: 0m);
		}

		protected void TestOverflowExceptionDoesNotHappenWhileAggregatingBigNumbers(string ledger)
		{
			var maximumMoneyValue = 922337203685477M;
			var minimumMoneyValue = -922337203685477M;

			var shipment = helper.InsertShipment("S99999999", new DateTime(2012, 01, 01));
			var job = helper.InsertJob("S99999999", TestDbHelper.DefaultCompanyPK, branch1, department1, "JS", shipment, "WRK", new DateTime(2012, 07, 25));
			var chargeCode = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");

			InsertJobCharge(Guid.NewGuid(), job, branch1, company1, department1, chargeCode, org1, ledger == "AR" ? maximumMoneyValue : 0M, org1, ledger == "AP" ? maximumMoneyValue : 0M);
			InsertJobCharge(Guid.NewGuid(), job, branch1, company1, department1, chargeCode, org1, ledger == "AR" ? 1M : 0M, org1, ledger == "AP" ? 1M : 0M);
			InsertJobCharge(Guid.NewGuid(), job, branch1, company1, department1, chargeCode, org1, ledger == "AR" ? minimumMoneyValue : 0M, org1, ledger == "AP" ? minimumMoneyValue : 0M);

			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After insert, after aggregation", 0, 1, 1, org1UnrecognizedAmt: 1M);

			var lineType = ledger == "AP" ? "ACR" : "WIP";
			InsertTransactionLine(Guid.NewGuid(), lineType, org1, maximumMoneyValue, branch1, department1, company1, ZDateTime.Today.ToDateTime());
			InsertTransactionLine(Guid.NewGuid(), lineType, org1, 2M, branch1, department1, company1, ZDateTime.Today.ToDateTime());
			InsertTransactionLine(Guid.NewGuid(), lineType, org1, minimumMoneyValue, branch1, department1, company1, ZDateTime.Today.ToDateTime());

			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After insert, after aggregation", 0, 1, 1, org1UnrecognizedAmt: 1M, org1RecognizedAmt: 2M);

			var multiplier = ledger == "AP" ? -1m : 1m;
			helper.InsertTransactionHeader(ledger, "INV", "00001001", maximumMoneyValue * multiplier, DateTime.Now, branch1, department1, companyPK: company1, org: org1, outstandingAmount: maximumMoneyValue * multiplier);
			helper.InsertTransactionHeader(ledger, "INV", "00001002", 3M * multiplier, DateTime.Now, branch1, department1, companyPK: company1, org: org1, outstandingAmount: 3M * multiplier);
			helper.InsertTransactionHeader(ledger, "INV", "00001003", minimumMoneyValue * multiplier, DateTime.Now, branch1, department1, companyPK: company1, org: org1, outstandingAmount: minimumMoneyValue * multiplier);

			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After insert, after aggregation", 0, 1, 1, org1UnrecognizedAmt: 1M, org1RecognizedAmt: 2M, org1BalanceAmt: 3M);
		}

		protected void TestClaimTotalWithOpenClaim(string ledger, string claimStatus, string transactionNumber, string claimNumber, string currency, decimal exchangeRate)
		{
			decimal originalClaimAmount = 20m / exchangeRate, newClaimAmount = 10m / exchangeRate, invAmount = 50m;

			Assert_vw_AccOrgBalances(ledger, "Empty Table before aggregation", 0, 0, 0, org1BalanceAmt: 0m, org2BalanceAmt: 0m, org1ClaimAmt: 0m, org2ClaimAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "Empty Table after aggregation", 0, 0, 0, org1BalanceAmt: 0m, org2BalanceAmt: 0m, org1ClaimAmt: 0m, org2ClaimAmt: 0m);

			var multiplier = ledger == "AP" ? -1m : 1m;
			var transactionPK = helper.InsertTransactionHeader(ledger, "INV", transactionNumber, invAmount * multiplier, DateTime.Now, branch1, department1, companyPK: company1, org: org1, currency: currency, exchangeRate: exchangeRate, outstandingAmount: invAmount * multiplier);

			var claimPK = helper.InsertQueryClaim(claimNumber, claimStatus, originalClaimAmount * exchangeRate, transactionPK, branch1, orgContact1, org1);
			Assert_vw_AccOrgBalances(ledger, "After insert, before aggregation", 2, 0, 1, org1BalanceAmt: invAmount, org2BalanceAmt: 0m, org1ClaimAmt: originalClaimAmount, org2ClaimAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After insert, after aggregation", 0, 1, 1, org1BalanceAmt: invAmount, org2BalanceAmt: 0m, org1ClaimAmt: originalClaimAmount, org2ClaimAmt: 0m);

			TestConnection.ExecuteNonQuery(Invariant($"UPDATE dbo.AccQueryClaim SET AY_QueryClaimAmount = {newClaimAmount * exchangeRate}, AY_SystemLastEditTimeUtc = GETUTCDATE(), AY_SystemLastEditUser = 'TST' WHERE AY_PK = '{claimPK}'"));
			Assert_vw_AccOrgBalances(ledger, "After updating AY_QueryClaimAmount, before aggregation", 1, 1, 1, org1BalanceAmt: invAmount, org2BalanceAmt: 0m, org1ClaimAmt: newClaimAmount, org2ClaimAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating AY_QueryClaimAmount, after aggregation", 0, 1, 1, org1BalanceAmt: invAmount, org2BalanceAmt: 0m, org1ClaimAmt: newClaimAmount, org2ClaimAmt: 0m);

			TestConnection.ExecuteNonQuery(Invariant($"UPDATE dbo.AccTransactionHeader SET AH_OH = '{org2}', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK = '{transactionPK}'"));
			TestConnection.ExecuteNonQuery(Invariant($"UPDATE dbo.AccQueryClaim SET AY_OH_Debtor = '{org2}', AY_SystemLastEditTimeUtc = GETUTCDATE(), AY_SystemLastEditUser = 'TST' WHERE AY_PK = '{claimPK}'"));
			Assert_vw_AccOrgBalances(ledger, "After updating AY_OH_Debtor, before aggregation", 4, 1, 2, org1BalanceAmt: 0m, org2BalanceAmt: invAmount, org1ClaimAmt: 0m, org2ClaimAmt: newClaimAmount);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating AY_OH_Debtor, after aggregation", 0, 1, 1, org1BalanceAmt: 0m, org2BalanceAmt: invAmount, org1ClaimAmt: 0m, org2ClaimAmt: newClaimAmount);

			TestConnection.ExecuteNonQuery(Invariant($"UPDATE dbo.AccQueryClaim SET AY_QueryClaimStatus = 'XXX', AY_SystemLastEditTimeUtc = GETUTCDATE(), AY_SystemLastEditUser = 'TST' WHERE AY_PK = '{claimPK}'"));
			Assert_vw_AccOrgBalances(ledger, "After updating AY_QueryClaimStatus, before aggregation", 1, 1, 1, org1BalanceAmt: 0m, org2BalanceAmt: invAmount, org1ClaimAmt: 0m, org2ClaimAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating AY_QueryClaimStatus, after aggregation", 0, 1, 1, org1BalanceAmt: 0m, org2BalanceAmt: invAmount, org1ClaimAmt: 0m, org2ClaimAmt: 0m);

			TestConnection.ExecuteNonQuery(Invariant($"UPDATE dbo.AccQueryClaim SET AY_QueryClaimStatus = '{claimStatus}', AY_SystemLastEditTimeUtc = GETUTCDATE(), AY_SystemLastEditUser = 'TST' WHERE AY_PK = '{claimPK}'"));
			Assert_vw_AccOrgBalances(ledger, "After updating AY_QueryClaimStatus, before aggregation", 1, 1, 1, org1BalanceAmt: 0m, org2BalanceAmt: invAmount, org1ClaimAmt: 0m, org2ClaimAmt: newClaimAmount);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating AY_QueryClaimStatus, after aggregation", 0, 1, 1, org1BalanceAmt: 0m, org2BalanceAmt: invAmount, org1ClaimAmt: 0m, org2ClaimAmt: newClaimAmount);

			TestConnection.ExecuteNonQuery(Invariant($"DELETE FROM dbo.AccQueryClaim WHERE AY_PK = '{claimPK}'"));
			Assert_vw_AccOrgBalances(ledger, "After deleting, before aggregation", 1, 1, 1, org1BalanceAmt: 0m, org2BalanceAmt: invAmount, org1ClaimAmt: 0m, org2ClaimAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After deleting, after aggregation", 0, 1, 1, org1BalanceAmt: 0m, org2BalanceAmt: invAmount, org1ClaimAmt: 0m, org2ClaimAmt: 0m);

			TestConnection.ExecuteNonQuery("DELETE dbo.AccOrgBalanceChanges");
			TestConnection.ExecuteNonQuery("DELETE dbo.AccOrgBalance");
		}

		protected void TestClaimTotalWithNonOpenClaim(string ledger, string claimStatus, string transactionNumber, string claimNumber)
		{
			decimal originalClaimAmount = 20m, newClaimAmount = 10m, invAmount = 50m;

			Assert_vw_AccOrgBalances(ledger, "Empty Table before aggregation", 0, 0, 0, org1BalanceAmt: 0m, org2BalanceAmt: 0m, org1ClaimAmt: 0m, org2ClaimAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "Empty Table after aggregation", 0, 0, 0, org1BalanceAmt: 0m, org2BalanceAmt: 0m, org1ClaimAmt: 0m, org2ClaimAmt: 0m);

			var multiplier = ledger == "AP" ? -1m : 1m;
			var transactionPK = helper.InsertTransactionHeader(ledger, "INV", transactionNumber, invAmount * multiplier, DateTime.Now, branch1, department1, companyPK: company1, org: org1, outstandingAmount: invAmount * multiplier);
			var claimPK = helper.InsertQueryClaim(claimNumber, claimStatus, originalClaimAmount, transactionPK, branch1, orgContact1, org1);
			Assert_vw_AccOrgBalances(ledger, "After insert, before aggregation", 1, 0, 1, org1BalanceAmt: invAmount, org2BalanceAmt: 0m, org1ClaimAmt: 0m, org2ClaimAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After insert, after aggregation", 0, 1, 1, org1BalanceAmt: invAmount, org2BalanceAmt: 0m, org1ClaimAmt: 0m, org2ClaimAmt: 0m);

			TestConnection.ExecuteNonQuery(Invariant($"UPDATE dbo.AccQueryClaim SET AY_QueryClaimAmount = {newClaimAmount}, AY_SystemLastEditTimeUtc = GETUTCDATE(), AY_SystemLastEditUser = 'TST' WHERE AY_PK = '{claimPK}'"));
			Assert_vw_AccOrgBalances(ledger, "After updating AY_QueryClaimAmount, before aggregation", 0, 1, 1, org1BalanceAmt: invAmount, org2BalanceAmt: 0m, org1ClaimAmt: 0m, org2ClaimAmt: 0m);
			RunAggregation();
			Assert_vw_AccOrgBalances(ledger, "After updating AY_QueryClaimAmount, after aggregation", 0, 1, 1, org1BalanceAmt: invAmount, org2BalanceAmt: 0m, org1ClaimAmt: 0m, org2ClaimAmt: 0m);

			TestConnection.ExecuteNonQuery("DELETE dbo.AccOrgBalanceChanges");
			TestConnection.ExecuteNonQuery("DELETE dbo.AccOrgBalance");
		}

		protected void InsertJobCharge(Guid jobChargePK, Guid jobPK, Guid branchPK, Guid companyPK, Guid departmentPK, Guid chargeCodePK, Guid jR_OH_SellAccount, decimal jR_LocalSellAmt, Guid jR_OH_CostAccount, decimal jR_LocalCostAmt)
		{
			helper.Insert(JobChargeSchema.Constants.TableName, new
			{
				JR_PK = jobChargePK,
				JR_JH = jobPK,
				JR_GB = branchPK,
				JR_GC = companyPK,
				JR_GE = departmentPK,
				JR_AC = chargeCodePK,
				JR_OH_SellAccount = jR_OH_SellAccount != Guid.Empty ? jR_OH_SellAccount : (object)DBNull.Value,
				JR_LocalSellAmt = jR_LocalSellAmt,
				JR_OH_CostAccount = jR_OH_CostAccount != Guid.Empty ? jR_OH_CostAccount : (object)DBNull.Value,
				JR_LocalCostAmt = jR_LocalCostAmt,
			});
		}

		protected void InsertTransactionLine(Guid pK, string lineType, Guid org, decimal lineAmount, Guid branch, Guid department, Guid company, DateTime postdate)
		{
			var multiplier = lineType == "ACR" ? 1 : -1;

			helper.Insert(AccTransactionLinesSchema.Constants.TableName, new
			{
				AL_PK = pK,
				AL_LineType = lineType,
				AL_OH = org,
				AL_LineAmount = lineAmount * multiplier,
				AL_GB = branch,
				AL_GE = department,
				AL_PostDate = postdate,
				AL_GC = company,
				AL_AG = helper.GLAccountPK1
			});
		}

		protected void RunAggregation()
		{
			using (var cmd = TestConnection.Command("AggregateAccOrgBalanceChangesIntoAccOrgBalance"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
				cmd.ExecuteNonQuery();
			}
		}

		protected void Assert_vw_AccOrgBalances(string ledger, string description, int accOrgBalanceChangesRowCount, int accOrgBalanceRowCount, int vw_AccOrgBalanceRowCount,
			decimal org1UnrecognizedAmt = 0, decimal org1RecognizedAmt = 0, decimal org1BalanceAmt = 0, decimal org1ClaimAmt = 0,
			decimal org2UnrecognizedAmt = 0, decimal org2RecognizedAmt = 0, decimal org2BalanceAmt = 0, decimal org2ClaimAmt = 0)
		{
			CombineAssertions(description, () =>
			{
				AssertEquals("Number of rows in AccOrgBalanceChanges", accOrgBalanceChangesRowCount, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.AccOrgBalanceChanges"));
				AssertEquals("Number of rows in AccOrgBalance", accOrgBalanceRowCount, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.AccOrgBalance"));
				AssertEquals("Number of rows in vw_AccOrgBalance", vw_AccOrgBalanceRowCount, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.vw_AccOrgBalance"));
			});
			Assert_vw_AccOrgBalance(description, org1, company1, ledger, org1UnrecognizedAmt, org1RecognizedAmt, org1BalanceAmt, org1ClaimAmt);
			Assert_vw_AccOrgBalance(description, org2, company1, ledger, org2UnrecognizedAmt, org2RecognizedAmt, org2BalanceAmt, org2ClaimAmt);
		}

		protected void Assert_vw_AccOrgBalance(string description, Guid org, Guid company, string ledger, decimal unrecognized, decimal recognized, decimal balance, decimal claim)
		{
			var sql = Invariant($"SELECT * FROM dbo.vw_AccOrgBalance WHERE OrganizationPK = '{org}' AND CompanyPK = '{company}' AND Ledger = '{ledger}'");
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			if (result.Rows.Count > 0)
			{
				AssertEquals(description + ": UnrecognizedTotal", unrecognized, result.Rows[0]["UnrecognizedTotal"]);
				AssertEquals(description + ": RecognizedTotal", recognized, result.Rows[0]["RecognizedTotal"]);
				AssertEquals(description + ": BalanceTotal", balance, result.Rows[0]["BalanceTotal"]);
				AssertEquals(description + ": ClaimTotal", claim, result.Rows[0]["ClaimTotal"]);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			helper = new TestDbHelper(TestConnection);
			org1 = helper.InsertOrgHeader("ZZC", "Org1");
			org2 = helper.InsertOrgHeader("ZZD", "Org2");
			orgContact1 = helper.InsertOrgContact("OC1", "", false, org1);
			orgContact2 = helper.InsertOrgContact("OC2", "", false, org2);
			company1 = TestDbHelper.DefaultCompanyPK;
			branch1 = helper.InsertBranch("ZZB", company1);
			department1 = helper.InsertDepartment("ZZD");
		}

		protected TestDbHelper helper;
		protected Guid org1, org2, company1, branch1, department1, orgContact1, orgContact2;
	}
}
