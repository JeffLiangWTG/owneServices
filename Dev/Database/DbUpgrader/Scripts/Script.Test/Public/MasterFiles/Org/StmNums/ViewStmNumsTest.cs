using System;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org.StmNums;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.StmNums.Testing
{
	[TestedType(typeof(ViewStmNums))]
	class ViewStmNumsTest : DbCreateScriptTest
	{
		#region TestCalculatedColumns

		public void TestCalculatedColumns()
		{
			var ownerPK = Guid.NewGuid();
			Helper.InsertView(ownerPK, "OrgOwned_TRF_1234567", 1, 6000, 0);
			AssertEquals("Prefix and Type should be calculated.", true, helper.ExistsInView("TRF", "1234567"));

			AssertEquals("Precondition", false, helper.ExistsInView(Guid.Empty, "OrgOwned_TRF_88888888", "", ""));
			AssertEquals(Guid.Empty, helper.InsertView(Guid.Empty, "NotOwned_TRF_88888888", 1, 3, 0, expectCountExists: false));
			AssertEquals("Not started with OrgOwned, prefix and type should return empty strings.", false, helper.ExistsInView(Guid.Empty, "NotOwned_TRF_88888888", "", ""));

			AssertEquals("Precondition", false, helper.ExistsInView(ownerPK, "OrgOwned_TRF_", "TRF", ""));
			Helper.InsertView(ownerPK, "OrgOwned_TRF_", 1, 4, 0);
			AssertEquals("Type should be calculated without prefix.", true, helper.ExistsInView(ownerPK, "OrgOwned_TRF_", "TRF", ""));

			AssertEquals("Precondition", false, helper.ExistsInView(ownerPK, "OrgOwned_SSC", "SSC", ""));
			Helper.InsertView(ownerPK, "OrgOwned_SSC", 1, 5, 0);
			AssertEquals("Type should be calculated without prefix.", true, helper.ExistsInView(ownerPK, "OrgOwned_SSC", "SSC", ""));

			AssertEquals("Precondition", false, helper.ExistsInView(ownerPK, "C#IT-CEN _BOB NUMBER", "CEN", "BOB NUMBER"));
			Helper.InsertView(ownerPK, "C#IT-CEN _BOB NUMBER", 1, 5, 0);
			AssertEquals("Type should be calculated without prefix.", true, helper.ExistsInView(ownerPK, "C#IT-CEN _BOB NUMBER", "CEN", "BOB NUMBER"));

			AssertEquals("Precondition", false, helper.ExistsInView(ownerPK, "C#IT-CENR_BOB NUMBER", "CENR", "BOB NUMBER"));
			Helper.InsertView(ownerPK, "C#IT-CENR_BOB NUMBER", 1, 5, 0);
			AssertEquals("Type should be calculated without prefix.", true, helper.ExistsInView(ownerPK, "C#IT-CENR_BOB NUMBER", "CENR", "BOB NUMBER"));

			var newOwnerPK = Guid.NewGuid();
			AssertEquals("Precondition", false, helper.ExistsInView(newOwnerPK, "small", "", ""));
			AssertEquals(Guid.Empty, helper.InsertView(newOwnerPK, "small", 1, 5000, 0, expectCountExists: false));
			AssertEquals("Small names should return empty strings for prefix and types. ", false, helper.ExistsInView(newOwnerPK, "small", "", ""));
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

