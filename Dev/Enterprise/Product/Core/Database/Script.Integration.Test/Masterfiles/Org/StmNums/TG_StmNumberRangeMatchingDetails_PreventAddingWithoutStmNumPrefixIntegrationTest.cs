using System;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.StmNums.Testing
{
	class TG_StmNumberRangeMatchingDetails_PreventAddingWithoutStmNumPrefixIntegrationTest : TransactionedTestCase
	{
		#region TestInsert_WhenStmNumsExists

		public void TestInsert_WhenStmNumsExists()
		{
			var ownerPK = Guid.NewGuid();
			var stmNumsPK = HelperStmNums.InsertView(ownerPK, "OrgOwned_TRF_1234567", 1, 6000, 0);
			AssertEquals("Precondition", true, HelperStmNums.ExistsInView("TRF", "1234567"));

			HelperMatchingDetails.InsertStmNumberRangeMatchingDetails(ownerPK, "OH", "TRF", "1234567");
			AssertEquals("Valid records should be inserted.", true, HelperMatchingDetails.ExistsInTable(ownerPK, "OH", "TRF", "1234567"));
		}

		#endregion

		#region TestInsert_ThrowsErrorIfStmNumsDoesNotExist

		public void TestInsert_ThrowsErrorIfStmNumsDoesNotExist()
		{
			var ownerPK = Guid.NewGuid();
			var stmNumsPK = HelperStmNums.InsertView(ownerPK, "OrgOwned_TRF_1234567", 1, 6000, 0);
			AssertEquals(true, HelperStmNums.ExistsInView("TRF", "1234567"));

			var ex = AssertExceptionThrown<SqlException>(() => HelperMatchingDetails.InsertStmNumberRangeMatchingDetails(ownerPK, "OH", "TRF", "88888888"));
			AssertEquals(@"Type and Prefix does not exists in stmNums table.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
		}

		#endregion

		#region TestInsert_TestThrowsErrorWhenNotMatch_Owner

		public void TestInsert_TestThrowsErrorWhenNotMatch_Owner()
		{
			var stmNumsOwnerPK = Guid.NewGuid();
			var matchingOwnerPK = Guid.NewGuid();
			var matchingType = "TRF";
			var matchingPrefix = "1234567";
			Insert_TestThrowsErrorWhenNotMatchCore("should not add for different owner.", stmNumsOwnerPK, matchingOwnerPK, matchingType, matchingPrefix);
		}

		#endregion

		#region TestInsert_TestThrowsErrorWhenNotMatch_RangeType

		public void TestInsert_TestThrowsErrorWhenNotMatch_RangeType()
		{
			var stmNumsOwnerPK = Guid.NewGuid();
			var matchingOwnerPK = stmNumsOwnerPK;
			var matchingType = "SSC";
			var matchingPrefix = "1234567";
			Insert_TestThrowsErrorWhenNotMatchCore("should not add for different type.", stmNumsOwnerPK, matchingOwnerPK, matchingType, matchingPrefix);
		}

		#endregion

		#region TestInsert_TestThrowsErrorWhenNotMatch_Prefix

		public void TestInsert_TestThrowsErrorWhenNotMatch_Prefix()
		{
			var stmNumsOwnerPK = Guid.NewGuid();
			var matchingOwnerPK = stmNumsOwnerPK;
			var matchingType = "TRF";
			var matchingPrefix = "888888";
			Insert_TestThrowsErrorWhenNotMatchCore("should not add for different prefix.", stmNumsOwnerPK, matchingOwnerPK, matchingType, matchingPrefix);
		}

		#endregion

		#region Insert_TestThrowsErrorWhenNotMatchCore

		void Insert_TestThrowsErrorWhenNotMatchCore(string message, Guid stmNumsOwnerPK, Guid matchingOwnerPK, string matchingType, string matchingPrefix)
		{
			var stmNumsPK = HelperStmNums.InsertView(stmNumsOwnerPK, "OrgOwned_TRF_1234567", 1, 6000, 0);
			AssertEquals(true, HelperStmNums.ExistsInView("TRF", "1234567"));

			var expectedError = @"Type and Prefix does not exists in stmNums table.
The transaction ended in the trigger. The batch has been aborted.";

			if (matchingType != OrgConstants.NumberFountains.Code.TransportReferenceNumbers)
			{
				HelperMatchingDetails.DropConstraintRangeType();
			}
			var ex = AssertExceptionThrown<SqlException>(() => HelperMatchingDetails.InsertStmNumberRangeMatchingDetails(matchingOwnerPK, "OH", matchingType, matchingPrefix));
			AssertEquals(message, expectedError, ex.Message);
		}

		#endregion

		#region TestUpdate_ThrowsErrorWhenStmNumsDoesNotExist_RangeType

		public void TestUpdate_ThrowsErrorWhenStmNumsDoesNotExist_RangeType()
		{
			var updateMatchingType = "SSC";
			var updateMatchingPrefix = "1234567";
			Update_ThrowsErrorWhenStmNumsDoesNotExistCore("should not update for different type.", updateMatchingType, updateMatchingPrefix);
		}

		#endregion

		#region TestUpdate_ThrowsErrorWhenStmNumsDoesNotExist_Prefix

		public void TestUpdate_ThrowsErrorWhenStmNumsDoesNotExist_Prefix()
		{
			var ownerPK = Guid.NewGuid();
			var updateMatchingOwnerPK = ownerPK;
			var updateMatchingType = "TRF";
			var updateMatchingPrefix = "888888";
			Update_ThrowsErrorWhenStmNumsDoesNotExistCore("should not update for different prefix.", updateMatchingType, updateMatchingPrefix);
		}

		#endregion

		#region Update_ThrowsErrorWhenStmNumsDoesNotExistCore

		void Update_ThrowsErrorWhenStmNumsDoesNotExistCore(string message, string updateMatchingType, string updateMatchingPrefix)
		{
			var ownerPK = Guid.NewGuid();
			HelperStmNums.InsertView(ownerPK, "OrgOwned_TRF_1234567", 1, 100, 0);
			HelperStmNums.ExistsInView("TRF", "1234567");
			var stmMatchingDetailsPK = HelperMatchingDetails.InsertStmNumberRangeMatchingDetails(ownerPK, "OH", "TRF", "1234567");
			AssertEquals("Precondition - StmNum must exist.", true, HelperStmNums.ExistsInView("TRF", "1234567"));
			AssertEquals("Precondition - Matching details must exist.", true, HelperMatchingDetails.ExistsInTable(ownerPK, "OH", "TRF", "1234567"));

			if (updateMatchingType != OrgConstants.NumberFountains.Code.TransportReferenceNumbers)
			{
				HelperMatchingDetails.DropConstraintRangeType();
			}
			var ex = AssertExceptionThrown<SqlException>(() => HelperMatchingDetails.UpdateStmNumberRangeMatchingDetails_Prefix(stmMatchingDetailsPK, updateMatchingType, updateMatchingPrefix));
			AssertEquals(message, @"Type and Prefix does not exists in stmNums table.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
		}

		#endregion

		#region TestUpdate_ValidPrefix

		public void TestUpdate_ValidPrefix()
		{
			var ownerPK = Guid.NewGuid();
			HelperStmNums.InsertView(ownerPK, "OrgOwned_TRF_1234567", 1, 100, 0);
			HelperStmNums.InsertView(ownerPK, "OrgOwned_TRF_8888888", 8, 800, 0);
			AssertEquals("Precondition", true, HelperStmNums.ExistsInView("TRF", "1234567"));
			AssertEquals("Precondition", true, HelperStmNums.ExistsInView("TRF", "8888888"));
			var stmMatchingDetailsPK = HelperMatchingDetails.InsertStmNumberRangeMatchingDetails(ownerPK, "OH", "TRF", "1234567");
			AssertEquals("Precondition", true, HelperStmNums.ExistsInView("TRF", "1234567"));
			AssertEquals("Precondition", true, HelperMatchingDetails.ExistsInTable(ownerPK, "OH", "TRF", "1234567"));

			HelperMatchingDetails.UpdateStmNumberRangeMatchingDetails_Prefix(stmMatchingDetailsPK, "TRF", "8888888");
			AssertEquals("Since a prefix exists, updated with no error.", true, HelperMatchingDetails.ExistsInTable(ownerPK, "OH", "TRF", "8888888"));
		}

		#endregion

		#region Helper

		StmNumberRangeMatchingDetailsTestHelper HelperMatchingDetails
		{
			get { return helperMatchingDetails ?? (helperMatchingDetails = new StmNumberRangeMatchingDetailsTestHelper(TestConnection)); }
		}
		StmNumberRangeMatchingDetailsTestHelper helperMatchingDetails;

		ViewStmNumsTestHelper HelperStmNums
		{
			get { return helperStmNums ?? (helperStmNums = new ViewStmNumsTestHelper(TestConnection)); }
		}
		ViewStmNumsTestHelper helperStmNums;
		#endregion
	}
}

