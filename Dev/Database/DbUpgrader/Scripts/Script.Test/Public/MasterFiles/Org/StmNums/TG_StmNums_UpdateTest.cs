using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org.StmNums;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using FountainHelper = CargoWise.Data.Testing.NumberFountainTestHelper;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.StmNums.Testing
{
	[TestedType(typeof(TG_StmNums_Update))]
	class TG_StmNums_UpdateTest : DbCreateScriptTest
	{
		public void TestUpdate()
		{
			var ownerPK = Guid.NewGuid();
			var stmNumsPK = helper.InsertView(ownerPK, "OrgOwned_SSC", 100, 6000, 0);

			AssertEquals(true, helper.ExistsInTable(ownerPK, "OrgOwned_SSC", 100, 6000));
			AssertEquals(true, helper.ExistsInView(stmNumsPK));

			helper.UpdateView(stmNumsPK, 100, 500);

			AssertEquals(true, helper.ExistsInTable(ownerPK, "OrgOwned_SSC", 100, 500, 100));
			AssertEquals(true, helper.ExistsInView(stmNumsPK));
		}

		public void TestUpdate_CurrentValueBelowMinValue()
		{
			var ownerPK = Guid.NewGuid();
			var stmNumsPK = helper.InsertView(ownerPK, "OrgOwned_SSC", 1, 6000, 0);
			var id = helper.GetStmNumsId(stmNumsPK);

			AssertEquals(true, helper.ExistsInTable(ownerPK, "OrgOwned_SSC", 1, 6000));
			AssertEquals(true, helper.ExistsInView(stmNumsPK));

			FountainHelper.GenerateNumbers_Direct(TestConnection, fountainId: id, amount: 200);

			AssertEquals(true, helper.ExistsInTable(ownerPK, "OrgOwned_SSC", 1, 6000, 201));

			helper.UpdateView(stmNumsPK, 120, 500);

			AssertEquals("First current value is changed to 120", true, helper.ExistsInTable(ownerPK, "OrgOwned_SSC", 120, 500, 201));
			AssertEquals(true, helper.ExistsInView(stmNumsPK));
		}

		#region Implementation

		ViewStmNumsTestHelper helper;

		void Cleanup()
		{
			TestConnection.ExecuteNonQuery("/* Cleanup */ DELETE dbo.StmNums WHERE SN_Name = 'OrgOwned_SSC';");
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new ViewStmNumsTestHelper(TestConnection);
		}

		protected override void FinalTearDown()
		{
			Cleanup();
			DbCommitTracker.Ignore("/* Cleanup */");

			base.FinalTearDown();
		}
		#endregion
	}
}

