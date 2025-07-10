using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.AuditDataServices.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMReleaseSequenceItem))]
	class BMReleaseSequenceItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetBusinessObjectBaseTypeFromTablePrefixForBMI()
		{
			var baseType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(BMReleaseSequenceItemSchema.Constants.Prefix);
			AssertNotNull(baseType);
			AssertEquals(typeof(BMReleaseSequenceItem), baseType);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var (system, buffer) = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "lalala", buffer);
			var releaseSequenceItem = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			releaseSequenceItem.BMI_FH_ProcessHeader = workflow.PK;
			Factory.Save();
			AssertNotNull(Factory.Load<BMReleaseSequenceItem>(releaseSequenceItem.PK));

			var pk = releaseSequenceItem.PK;
			releaseSequenceItem.Delete();
			Factory.Save();
			AssertNull(Factory.Load<BMReleaseSequenceItem>(pk));
		}

		public void TestCalculatedPropertiesDuration()
		{
			AssertEquals(1.875m, CalculatedPropertiesSetup().Duration);
		}

		public void TestCalculatedPropertiesScore()
		{
			AssertEquals(50m, CalculatedPropertiesSetup().Score);
		}

		public void TestCalculatedPropertiesStatus()
		{
			AssertEquals("OPN", CalculatedPropertiesSetup().Status);
		}

		BMReleaseSequenceItem CalculatedPropertiesSetup()
		{
			var (system, buffer) = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG");
			var staff = BMSTestHelper.CreateStaff(Factory, staffCode: "ABC");
			var capability = BMSTestHelper.CreateCapability(Factory, code: "CAP", isGroupScope: false);
			var group = BMSTestHelper.CreateGroup(Factory, code: "GRP");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "lalala", buffer);
			var task1 = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 60, staffCode: "ABC", taskStatus: "WRK");
			var task2 = BMSTestHelper.CreateTask(workflow);
			var task3 = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 15, capability: capability, taskStatus: "ASN");

			var releaseSequence = Factory.NewWithValidTestData<BMReleaseSequence>();
			releaseSequence.BMR_Name = "Seq1";
			releaseSequence.BMR_GG_ReleaseGroup = group.PK;

			var releaseSequenceItem = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			releaseSequenceItem.BMI_FH_ProcessHeader = workflow.PK;
			releaseSequenceItem.BMI_Value = 10;
			releaseSequenceItem.BMI_Investment = 20;
			releaseSequence.Items.Add(releaseSequenceItem);

			return releaseSequenceItem;
		}

		#region Data Version Logging

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestLoggingSequenceItemValue()
		{
			LoggingTestWithAuditLog("BMI_Value", "10", "20");
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestLoggingSequenceItemInvestment()
		{
			LoggingTestWithAuditLog("BMI_Investment", "5", "25");
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestLoggingSequenceItemPosition()
		{
			LoggingTestWithAuditLog("BMI_Position", "1", "3");
		}

		void LoggingTestWithAuditLog(string fieldName, string oldValue, string newValue)
		{
			using var adminConnection = Db.NewAdminConnection();
			var factory = new BusinessObjectFactory(adminConnection);
			var auditLogsHelperForTesting = new AuditLogsHelperForTesting(factory, BMReleaseSequenceItemSchema.Instance);

			var (system, buffer) = BMSTestHelper.CreateSystemAndBuffer(factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "lalala", buffer);
			var releaseSequence = factory.NewWithValidTestData<BMReleaseSequence>();
			var releaseSequenceItem = factory.NewWithValidTestData<BMReleaseSequenceItem>();
			releaseSequenceItem.BMI_FH_ProcessHeader = workflow.PK;
			releaseSequenceItem.BMI_Value = 10;
			releaseSequenceItem.BMI_Investment = 5;
			releaseSequenceItem.BMI_Position = 1;
			releaseSequence.Items.Add(releaseSequenceItem);
			factory.Save();

			releaseSequenceItem.BMI_Value = 20;
			releaseSequenceItem.BMI_Investment = 25;
			releaseSequenceItem.BMI_Position = 3;
			factory.Save();

			var dataVersionLogs = auditLogsHelperForTesting.GetAuditLogCollection(releaseSequenceItem);

			var log = dataVersionLogs.Cast<AuditEvent>().MaxBy(l => l.TimeUtc);
			var versionLogs = log.ChangeCollection.Cast<AuditChange>();
			var logFields = versionLogs.Where(x => x.RealColumnName == fieldName);
			AssertEquals(1, logFields.Count());
			AssertEquals(fieldName + " old value", oldValue, logFields.First().ValueBefore);
			AssertEquals(fieldName + " new value", newValue, logFields.First().ValueAfter);
		}

		#endregion
	}
}
