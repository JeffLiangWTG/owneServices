using System;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.StmNums.Testing
{
	class TG_StmNums_PreventChangeUsedRows_DeleteIntegrationTest : TransactionedTestCase
	{
		#region TestCannotDeleteIsUsedInMatchingDetails

		public void TestCannotDeleteIsUsedInMatchingDetails()
		{
			var ownerPK = Guid.NewGuid();
			var usedStmNums = Helper.InsertView(ownerPK, "OrgOwned_TRF_1234567", 1, 6000, 0);
			var notUsedStmNums = Helper.InsertView(ownerPK, "OrgOwned_TRF_8888888", 8, 800, 0);
			var helperMatchingDetail = new StmNumberRangeMatchingDetailsTestHelper(TestConnection);
			var stmMatchingDetailsPK = helperMatchingDetail.InsertStmNumberRangeMatchingDetails(ownerPK, "OH", "TRF", "1234567");

			AssertEquals("Precondition", true, Helper.ExistsInView("TRF", "1234567"));
			AssertEquals("Precondition", true, Helper.ExistsInView("TRF", "8888888"));
			AssertEquals("Precondition", true, helperMatchingDetail.ExistsInTable(ownerPK, "OH", "TRF", "1234567"));

			AssertNoExceptionThrown(() => Helper.DeleteView(notUsedStmNums));
			var ex = AssertExceptionThrown<SqlException>(() => Helper.DeleteView(usedStmNums));
			AssertEquals(@"Type and Prefix used in StmNumberRangeMatchingDetail.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
		}

		#endregion

		#region Helper

		ViewStmNumsTestHelper Helper
		{
			get { return helper ?? (helper = new ViewStmNumsTestHelper(TestConnection)); }
		}
		ViewStmNumsTestHelper helper;
		#endregion
	}
}

