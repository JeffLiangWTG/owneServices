using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class LocalCartageUpgradeTaskTest : TransactionedTestCase
	{
		public void TestDuplicateIsDeleted()
		{
			LocalCartageUpgradeTask task = new LocalCartageUpgradeTask();
			task.DeleteCartageTypeAndItsAffiliates("ISCF"); //remove system ISCF

			//Add !System ISCF
			Guid departmentPK = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 GE_PK FROM dbo.GLBDEPARTMENT");
			var newISCFJobTypePK = new LocalCartageJobType(departmentPK)
			{
				E3_JobType = "ISCF",
				E3_IsSystem = false,
				E3_Description = "Some Description"
			}.InsertAndReturnObject(TestConnection).PK;

			var newISCFJobOrgPK = new LocalCartageJobOrg(newISCFJobTypePK)
			{
				E5_OrgType = "ORG"
			}.InsertAndReturnObject(TestConnection).PK;

			var newISCFJobLeg1PK = new LocalCartageJobLegType(newISCFJobTypePK)
			{
				E4_E5_FromOrg = newISCFJobOrgPK,
				E4_E5_ToOrg = newISCFJobOrgPK
			}.InsertAndReturnObject(TestConnection).PK;

			var newISCFJobLeg2PK = new LocalCartageJobLegType(newISCFJobTypePK)
			{
				E4_E5_FromOrg = newISCFJobOrgPK,
				E4_E5_ToOrg = newISCFJobOrgPK
			}.InsertAndReturnObject(TestConnection).PK;

			task.Run();

			string selectOrgsOfSpecificJobType = @"
SELECT Count(*) From dbo.LocalCartageJobOrg Inner Join dbo.LocalCartageJobType
On E3_PK = E5_E3
WHERE E3_JobType = @E3_JobType";
			string selectLegsOfSpecificJobType = @"
SELECT Count(*) From dbo.LocalCartageJobLegType Inner Join dbo.LocalCartageJobType
On E3_PK = E4_E3
WHERE E3_JobType = @E3_JobType";

			DbCommand iSCFcmd = Db.Connection.Command(SelectJobTypeSql);
			iSCFcmd.AddParameter("@E3_JobType", SqlDbType.NVarChar, "ISCF");
			int iSCFCount = Convert.ToInt32(iSCFcmd.ExecuteScalar());
			AssertEquals("Searching for ISCF job type", 1, iSCFCount);

			DbCommand iSCFOrgcmd = Db.Connection.Command(selectOrgsOfSpecificJobType);
			iSCFOrgcmd.AddParameter("@E3_JobType", SqlDbType.NVarChar, "ISCF");
			int iSCFOrgsCount = Convert.ToInt32(iSCFOrgcmd.ExecuteScalar());
			AssertEquals("Searching for ISCF job type", 3, iSCFOrgsCount);

			DbCommand iSCFLegcmd = Db.Connection.Command(selectLegsOfSpecificJobType);
			iSCFLegcmd.AddParameter("@E3_JobType", SqlDbType.NVarChar, "ISCF");
			int iSCFLegsCount = Convert.ToInt32(iSCFLegcmd.ExecuteScalar());
			AssertEquals("Searching for ISCF job type", 3, iSCFLegsCount);

			using (DbCommand iALCcmd = Db.Connection.Command(SelectJobTypeSql))
			{
				iALCcmd.AddParameter("@E3_JobType", SqlDbType.NVarChar, "ISCF");
				int iALCCount = Convert.ToInt32(iALCcmd.ExecuteScalar());
				AssertEquals("Searching for ISCF job type", 1, iALCCount);
			}
		}

		public void TestE3_DescriptionColumnIsIgnoredForUpdate()
		{
			var cartageTypePK = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 E3_PK FROM dbo.LocalCartageJobType");
			var cartageTypeCount = (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM dbo.LocalCartageJobType WHERE E3_PK = '" + cartageTypePK + "'");
			AssertEquals("Precondition: Should have one for this PK", 1, cartageTypeCount);

			var newDescription = "New Description";
			Db.Connection.ExecuteNonQuery("UPDATE dbo.LocalCartageJobType SET E3_Description = '" + newDescription + "' WHERE E3_PK = '" + cartageTypePK + "'");
			var afterUpdatedDescription = (string)Db.Connection.ExecuteScalar("SELECT E3_Description FROM dbo.LocalCartageJobType WHERE E3_PK = '" + cartageTypePK + "'");

			new LocalCartageUpgradeTask().Run();
			var afterRunDescription = (string)Db.Connection.ExecuteScalar("SELECT E3_Description FROM dbo.LocalCartageJobType WHERE E3_PK = '" + cartageTypePK + "'");
			AssertEquals("No LocalCartageJobTypes should have been modified", afterUpdatedDescription, afterRunDescription);
		}

		public void TestE3_GEColumnIsIgnoredForUpdate()
		{
			CreateLocalCartageJobTypeAndItsAffiliates();
			Guid originalDepartmentPK = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 GE_PK FROM dbo.GLBDEPARTMENT");
			Guid newDepartmentPK = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 GE_PK FROM dbo.GLBDEPARTMENT WHERE GE_PK <> '" + originalDepartmentPK + "'");

			LocalCartageUpgradeTask task = new LocalCartageUpgradeTask();
			task.Run();

			Db.Connection.ExecuteNonQuery("UPDATE dbo.LocalCartageJobType SET E3_GE = '" + originalDepartmentPK + "'");
			int count = (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM dbo.LocalCartageJobType WHERE E3_GE = '" + originalDepartmentPK + "'");
			Assert("count should be greater than 0", count > 0);

			task.Run();

			int countAfterRun = (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM dbo.LocalCartageJobType WHERE E3_GE = '" + originalDepartmentPK + "'");
			AssertEquals("No LocalCartageJobTypes should have been modified", count, countAfterRun);

			Db.Connection.ExecuteNonQuery("UPDATE dbo.LocalCartageJobType SET E3_GE = '" + newDepartmentPK + "'");
			count = (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM dbo.LocalCartageJobType WHERE E3_GE = '" + newDepartmentPK + "'");
			Assert("count should be greater than 0", count > 0);

			task.Run();

			countAfterRun = (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM dbo.LocalCartageJobType WHERE E3_GE = '" + newDepartmentPK + "'");
			AssertEquals("No LocalCartageJobTypes should have been modified", count, countAfterRun);
		}

		public void TestBookingAddressesIgnoredForUpdate()
		{
			CreateLocalCartageJobTypeAndItsAffiliates();

			Guid jobTypePK = (Guid)Db.Connection.ExecuteScalar("SELECT E3_PK FROM dbo.LocalCartageJobType WHERE E3_JobType = 'ISCF'");
			Guid bookingPK = (Guid)Db.Connection.ExecuteScalar("SELECT E4_PK FROM dbo.LocalCartageJobLegType WHERE E4_IsBooking = 1 AND E4_E3 = '" + jobTypePK + "'");
			Guid orgPK = (Guid)Db.Connection.ExecuteScalar("SELECT top 1 E5_PK FROM dbo.LocalCartageJobOrg WHERE E5_E3 = '" + jobTypePK + "'");

			LocalCartageUpgradeTask task = new LocalCartageUpgradeTask();

			task.Run();

			Db.Connection.ExecuteNonQuery("UPDATE dbo.LocalCartageJobLegType SET E4_E5_ToOrg = '" + orgPK + "' WHERE E4_PK = '" + bookingPK + "'");
			Db.Connection.ExecuteNonQuery("UPDATE dbo.LocalCartageJobType SET E3_Description = 'SomethingElse' WHERE E3_PK = '" + jobTypePK + "'");

			int count = (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM dbo.LocalCartageJobLegType WHERE E4_PK = '" + bookingPK + "'");//  WHERE E4_E5_ToOrg = '" + orgPK + "'").ExecuteScalar();
			AssertEquals("count should be greater than 0", 1, count);

			task.Run();

			int countAfterRun = (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM dbo.LocalCartageJobLegType WHERE E4_E5_ToOrg = '" + orgPK + "' AND E4_IsBooking = 1");
			AssertEquals("No LocalCartageJobLegType should have been modified", count, countAfterRun);

			Db.Connection.ExecuteNonQuery("UPDATE dbo.LocalCartageJobLegType SET E4_E5_ToOrg = '" + orgPK + "' WHERE E4_E3 = '" + jobTypePK + "' AND E4_IsBooking = 0 AND E4_E5_ToOrg <> '" + orgPK + "'");
			count = (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM dbo.LocalCartageJobLegType WHERE E4_E3 = '" + jobTypePK + "' AND E4_E5_ToOrg = '" + orgPK + "' AND E4_IsBooking = 0");
			AssertEquals("count should be greater than 0", 2, count);

			task.Run();

			countAfterRun = (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM dbo.LocalCartageJobLegType WHERE E4_E3 = '" + jobTypePK + "' AND E4_E5_ToOrg = '" + orgPK + "' AND E4_IsBooking = 0");
			AssertEquals("No LocalCartageJobLegType should have been modified", 1, countAfterRun);
		}

		public void TestDeleteCartageTypeAndItsAffiliates()
		{
			CreateLocalCartageJobTypeAndItsAffiliates();

			LocalCartageUpgradeTask task = new LocalCartageUpgradeTask();
			task.DeleteCartageTypeAndItsAffiliates(JobTypePK);

			CheckExistanceOfTheObjects(0);
		}

		public void TestTaskRun()
		{
			string selectOrgsOfSpecificJobType = @"
SELECT Count(*) From dbo.LocalCartageJobOrg Inner Join dbo.LocalCartageJobType
On E3_PK = E5_E3
WHERE E3_JobType = @E3_JobType";
			string selectLegsOfSpecificJobType = @"
SELECT Count(*) From dbo.LocalCartageJobLegType Inner Join dbo.LocalCartageJobType
On E3_PK = E4_E3
WHERE E3_JobType = @E3_JobType";
			LocalCartageUpgradeTask task = new LocalCartageUpgradeTask();
			task.DeleteCartageTypeAndItsAffiliates("ISCF");
			task.DeleteCartageTypeAndItsAffiliates("IALC");
			task.Run();

			DbCommand iSCFcmd = Db.Connection.Command(SelectJobTypeSql);
			iSCFcmd.AddParameter("@E3_JobType", SqlDbType.NVarChar, "ISCF");
			int iSCFCount = Convert.ToInt32(iSCFcmd.ExecuteScalar());
			AssertEquals("Searching for ISCF job type", 1, iSCFCount);

			DbCommand iSCFOrgcmd = Db.Connection.Command(selectOrgsOfSpecificJobType);
			iSCFOrgcmd.AddParameter("@E3_JobType", SqlDbType.NVarChar, "ISCF");
			int iSCFOrgsCount = Convert.ToInt32(iSCFOrgcmd.ExecuteScalar());
			AssertEquals("Searching for ISCF job type", 3, iSCFOrgsCount);

			DbCommand iSCFLegcmd = Db.Connection.Command(selectLegsOfSpecificJobType);
			iSCFLegcmd.AddParameter("@E3_JobType", SqlDbType.NVarChar, "ISCF");
			int iSCFLegsCount = Convert.ToInt32(iSCFLegcmd.ExecuteScalar());
			AssertEquals("Searching for ISCF job type", 3, iSCFLegsCount);

			DbCommand iALCcmd = Db.Connection.Command(SelectJobTypeSql);
			iALCcmd.AddParameter("@E3_JobType", SqlDbType.NVarChar, "ISCF");
			int iALCCount = Convert.ToInt32(iALCcmd.ExecuteScalar());
			AssertEquals("Searching for ISCF job type", 1, iALCCount);

			DbCommand iALCOrgcmd = Db.Connection.Command(selectOrgsOfSpecificJobType);
			iALCOrgcmd.AddParameter("@E3_JobType", SqlDbType.NVarChar, "IALC");
			int iALCOrgsCount = Convert.ToInt32(iALCOrgcmd.ExecuteScalar());
			AssertEquals("Searching for IALC job type", 2, iALCOrgsCount);

			DbCommand iALCLegcmd = Db.Connection.Command(selectLegsOfSpecificJobType);
			iALCLegcmd.AddParameter("@E3_JobType", SqlDbType.NVarChar, "IALC");
			int iALCLegsCount = Convert.ToInt32(iALCLegcmd.ExecuteScalar());
			AssertEquals("Searching for IALC job type", 2, iALCLegsCount);
		}

		#region Implementation

		void CreateLocalCartageJobTypeAndItsAffiliates()
		{
			Guid departmentPK = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 GE_PK FROM dbo.GLBDEPARTMENT");
			JobTypePK = new LocalCartageJobType(departmentPK)
			{
				E3_JobType = "SJTT",
				E3_IsSystem = true,
				E3_Description = "Some description"
			}.InsertAndReturnObject(TestConnection).PK;

			JobOrgPK = new LocalCartageJobOrg(JobTypePK)
			{
				E5_OrgType = "ORG"
			}.InsertAndReturnObject(TestConnection).PK;

			JobLeg1PK = new LocalCartageJobLegType(JobTypePK)
			{
				E4_E5_FromOrg = JobOrgPK,
				E4_E5_ToOrg = JobOrgPK
			}.InsertAndReturnObject(TestConnection).PK;

			JobLeg2PK = new LocalCartageJobLegType(JobTypePK)
			{
				E4_E5_FromOrg = JobOrgPK,
				E4_E5_ToOrg = JobOrgPK
			}.InsertAndReturnObject(TestConnection).PK;

			CheckExistanceOfTheObjects(1);
		}

		void CheckExistanceOfTheObjects(int expectedCount)
		{
			DbCommand selectJobTypeCmd = Db.Connection.Command(SelectJobTypeSql);
			selectJobTypeCmd.AddParameter("@E3_JobType", SqlDbType.NVarChar, "SJTT");
			int jobTypeCountBeforeUpgrade = Convert.ToInt32(selectJobTypeCmd.ExecuteScalar());
			AssertEquals("Count of LocalCartageJobType with TypeCode = 'SJTT'", expectedCount, jobTypeCountBeforeUpgrade);

			DbCommand selectOrgTypeCmd = Db.Connection.Command(SelectJobOrgTypeSql);
			selectOrgTypeCmd.AddParameter("@E5_PK", SqlDbType.UniqueIdentifier, JobOrgPK);
			int orgTypeCountBeforeUpgrade = Convert.ToInt32(selectOrgTypeCmd.ExecuteScalar());
			AssertEquals("Count of LocalCartageJobOrg associted with JobType 'SJTT'", expectedCount, orgTypeCountBeforeUpgrade);

			DbCommand selectLegType1Cmd = Db.Connection.Command(SelectJobLegSql);
			selectLegType1Cmd.AddParameter("@E4_PK", SqlDbType.UniqueIdentifier, JobLeg1PK);
			int legTypeCountBeforeUpgrade = Convert.ToInt32(selectLegType1Cmd.ExecuteScalar());
			AssertEquals("Count of LocalCartageJobLegType associted with JobType 'SJTT'", expectedCount, legTypeCountBeforeUpgrade);

			DbCommand selectLegType2Cmd = Db.Connection.Command(SelectJobLegSql);
			selectLegType2Cmd.AddParameter("@E4_PK", SqlDbType.UniqueIdentifier, JobLeg2PK);
			legTypeCountBeforeUpgrade = Convert.ToInt32(selectLegType2Cmd.ExecuteScalar());
			AssertEquals("Count of LocalCartageJobLegType associted with JobType 'SJTT'", expectedCount, legTypeCountBeforeUpgrade);
		}

		Guid JobTypePK;
		Guid JobOrgPK;
		Guid JobLeg1PK;
		Guid JobLeg2PK;

		const string SelectJobTypeSql = @"
SELECT Count(*) From dbo.LocalCartageJobType 
WHERE E3_JobType = @E3_JobType";
		const string SelectJobOrgTypeSql = @"
SELECT Count(*) From dbo.LocalCartageJobOrg
WHERE E5_PK = @E5_PK";
		const string SelectJobLegSql = @"
SELECT Count(*) From dbo.LocalCartageJobLegType
WHERE E4_PK = @E4_PK";
		#endregion
	}
}
