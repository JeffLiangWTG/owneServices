using System;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.StmNums.Testing
{
	class TG_StmNums_PreventChangeUsedRows_UpdateIntegrationTest : TransactionedTestCase
	{
		public void TestCannotUpdateWhenUsedInMatchingDetails()
		{
			var ownerPK = Guid.NewGuid();
			var ownerPK2 = Guid.NewGuid();
			var usedStmNums = Helper.InsertView(ownerPK, "OrgOwned_TRF_1234567", 1, 6000, 0);
			var notUsedStmNums = Helper.InsertView(ownerPK, "OrgOwned_TRF_8888888", 8, 800, 0);
			var notUsedStmNumsOwner2 = Helper.InsertView(ownerPK2, "OrgOwned_TRF_2222222", 2, 200, 0);
			var helperMatchingDetail = new StmNumberRangeMatchingDetailsTestHelper(TestConnection);
			var stmMatchingDetailsPK = helperMatchingDetail.InsertStmNumberRangeMatchingDetails(ownerPK, "OH", "TRF", "1234567");

			AssertEquals("Precondition", true, Helper.ExistsInView("TRF", "1234567"));
			AssertEquals("Precondition", true, Helper.ExistsInView("TRF", "8888888"));
			AssertEquals("Precondition", true, helperMatchingDetail.ExistsInTable(ownerPK, "OH", "TRF", "1234567"));

			AssertNoExceptionThrown(() => Helper.UpdateStmNumsName("OrgOwned_TRF_8888888", "OrgOwned_TRF_8888889"));
			AssertNoExceptionThrown(() => Helper.UpdateStmNumsName("OrgOwned_TRF_2222222", "OrgOwned_TRF_2222223"));
			var ex = AssertExceptionThrown<SqlException>(() => Helper.UpdateStmNumsName("OrgOwned_TRF_1234567", "OrgOwned_TRF_1234568"));
			AssertEquals(@"Type and Prefix used in StmNumberRangeMatchingDetail.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
		}

		#region Helper

		ViewStmNumsTestHelper Helper
		{
			get { return helper ?? (helper = new ViewStmNumsTestHelper(TestConnection)); }
		}
		ViewStmNumsTestHelper helper;
		#endregion
	}
}
