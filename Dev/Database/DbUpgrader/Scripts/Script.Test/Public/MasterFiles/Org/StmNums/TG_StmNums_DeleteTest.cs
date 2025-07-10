using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org.StmNums;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.StmNums.Testing
{
	[TestedType(typeof(TG_StmNums_Delete))]
	class TG_StmNums_DeleteTest : DbCreateScriptTest
	{
		public void TestDelete()
		{
			var ownerPK = Guid.NewGuid();
			var stmNumsPK = helper.InsertView(ownerPK, "OrgOwned_SSC", 1, 6000, 0);

			AssertEquals(true, helper.ExistsInTable(ownerPK, "OrgOwned_SSC", 1, 6000));
			AssertEquals(true, helper.ExistsInView(stmNumsPK));

			var selectFountainIDSQL = string.Format("SELECT SN_ID FROM dbo.StmNums WHERE SN_Owner = '{0}' AND SN_Name = '{1}'", ownerPK, "OrgOwned_SSC");
			int fountainID = (int)TestConnection.ExecuteScalar(selectFountainIDSQL);

			var insertNumberCacheSQL = "INSERT dbo.StmNumberCache (SG_Value, SG_SN) VALUES (@value, @fountainID)";
			using (var command = TestConnection.Command(insertNumberCacheSQL))
			{
				command.AddParameter("@value", SqlDbType.Int, 10);
				command.AddParameter("@fountainID", SqlDbType.Int, fountainID);
				command.ExecuteNonQuery();
			}

			var selectNumberCacheSQL = string.Format("SELECT COUNT(*) FROM dbo.StmNumberCache WHERE SG_SN = '{0}'", fountainID);
			AssertEquals("Number cache exist in DB", 1, (int)TestConnection.ExecuteScalar(selectNumberCacheSQL));

			helper.DeleteView(stmNumsPK);

			AssertEquals(false, helper.ExistsInTable(ownerPK, "OrgOwned_SSC", 1, 6000));
			AssertEquals(false, helper.ExistsInView(stmNumsPK));

			AssertEquals("Number cache was deleted from DB", 0, (int)TestConnection.ExecuteScalar(selectNumberCacheSQL));
		}

		#region Implementation

		ViewStmNumsTestHelper helper;

		protected override void SetUp()
		{
			base.SetUp();
			helper = new ViewStmNumsTestHelper(TestConnection);
		}
		#endregion
	}
}

