using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestDate(2014, 6, 11, 9, 0, 0)]
	public class CCPMReleaseTest : BMSTestCaseWithFactory
	{
		public void TestProcess_WhenCalculatingCCPMReadyToReleaseCache_ShouldUseTVPRatherThanManyParameters()
		{
			CreateTestData(nudgeOfCCPMItem: 10, nudgeOfNonCCPMItem: 0);

			using (TestConnection.TrackExecutedCommands())
			{
				using (var settings = TestEntityFrameworkSettings.Get())
				{
					settings.TVPRule = new TVPRule("0");
					RunReleaseGate();

					var statement = TestConnection.ExecutedCommands.Single(s => s.Contains("SELECT FH_PK FROM dbo.ProcessHeader WHERE ("));

					AssertContains("This statement should use a TVP so that if there are many parameters we don't overload the query plan compiler.", "FH_PK in (SELECT Value FROM @", statement);
				}
			}
		}

		public void TestProcess_CCPMItemNudgedFirst_ShouldBlockCapacityOfSubsequentItems()
		{
			CreateTestData(nudgeOfCCPMItem: 10, nudgeOfNonCCPMItem: 0);
			var logger = RunReleaseGate();

			AssertEquals("Should release CCPM workflow first", buffer, ccpmWorkflow.CurrentComponent);
			AssertEquals("Should not release non-CCPM workflow since CCPM workflow has reserved capacity", bucket, nonCcpmWorkflow.CurrentComponent);
			AssertEquals("Should be no available capacity remaining", 0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, buffer).AvailableCapacity);

			AssertMultilineASCIIEquals("Release gate log",
@"Dat System.buffer: Determining workflows eligible for Release Gate.
Dat System.buffer: Calculating Capacity for 1 staff.
Using Old Capacity Query...
Finished loading workflows in 00:00:00
Dat System.buffer: Assembling reservation tracker.
Dat System.buffer: Sorting workflows into release sequence.
Dat System.buffer: Starting Release Gate run with 2 workflows to process.
Dat System.buffer: Workflow ccpmWorkflow for job MAIFRISTORG has been released.
Dat System.buffer: Persisting updated capacity after Release Gate run.", logger.ToString());
		}

		public void TestProcess_CCPMItemNudgedSecond_ShouldBeReleasedInSpiteOfNoCapacity()
		{
			CreateTestData(nudgeOfCCPMItem: 0, nudgeOfNonCCPMItem: 10);
			var logger = RunReleaseGate();

			AssertEquals("Should release non-CCPM workflow", buffer, nonCcpmWorkflow.CurrentComponent);
			AssertEquals("Should also release CCPM workflow since it ignores capacity restrictions", buffer, ccpmWorkflow.CurrentComponent);
			AssertEquals("Resource should be over-booked", -4m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, buffer).AvailableCapacity);

			AssertMultilineASCIIEquals("Release gate log",
@"Dat System.buffer: Determining workflows eligible for Release Gate.
Dat System.buffer: Calculating Capacity for 1 staff.
Using Old Capacity Query...
Finished loading workflows in 00:00:00
Dat System.buffer: Assembling reservation tracker.
Dat System.buffer: Sorting workflows into release sequence.
Dat System.buffer: Starting Release Gate run with 2 workflows to process.
Dat System.buffer: Workflow nonCcpmWorkflow for job MAISCNDORG has been released.
Dat System.buffer: Workflow ccpmWorkflow for job MAIFRISTORG has been released.
Dat System.buffer: Persisting updated capacity after Release Gate run.", logger.ToString());
		}

		#region Implementation

		GlbStaff resource;
		BMComponent bucket, buffer;
		ProcessHeader ccpmWorkflow, nonCcpmWorkflow;

		public void CreateTestData(int nudgeOfCCPMItem, int nudgeOfNonCCPMItem)
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			system.FS_Name = "Dat System";
			bucket = CreateBucket(system);
			buffer = CreateBuffer(system, timespanMinutes: BMConstants.WorkingHoursPerDay * 60);
			LinkComponents(bucket, buffer);

			var rtrTag = TagProvider.GetCCPMReadyToReleaseTag(TagProvider.GetCCPMReleaseTagGroup(Factory));

			var tagDefForNudge = BMSTestHelper.CreateTagDefinition(Factory, "NUD");
			var ccpmTagForNudge = BMSTestHelper.CreateTagMagnitude(tagDefForNudge, "CC", nudge: nudgeOfCCPMItem);
			var nonCcpmTagForNudge = BMSTestHelper.CreateTagMagnitude(tagDefForNudge, "NCC", nudge: nudgeOfNonCCPMItem);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "MAIFRISTORG";
			org2.OH_Code = "MAISCNDORG";

			var jobHeader1 = ProcessJobHeader.GetForParent(org1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(org2, Factory);

			ccpmWorkflow = jobHeader1.ProcessHeaders[0];
			nonCcpmWorkflow = jobHeader2.ProcessHeaders[0];

			ccpmWorkflow.FH_CompletionStatement = "ccpmWorkflow";
			nonCcpmWorkflow.FH_CompletionStatement = "nonCcpmWorkflow";

			var task1 = CreateTask(ccpmWorkflow, resource.GS_Code, buffer.FC_BufferTimespanInMinutes / 2, estVariationFactor: 1); // Each task takes 50% of the buffer timespan, or 100% of available capacity, so just one workflow can be released according to normal capacity rules
			var task2 = CreateTask(nonCcpmWorkflow, resource.GS_Code, buffer.FC_BufferTimespanInMinutes / 2, estVariationFactor: 1);

			Factory.Save();

			BMSTestHelper.CreateRuleRunnerTagLink(ccpmTagForNudge, ccpmWorkflow);
			BMSTestHelper.CreateRuleRunnerTagLink(rtrTag, ccpmWorkflow);
			BMSTestHelper.CreateRuleRunnerTagLink(nonCcpmTagForNudge, nonCcpmWorkflow);

			Factory.Save();
		}

		ILogger RunReleaseGate()
		{
			var logger = new BufferManagementLogger();
			ReleaseGateKeeperTest.RunReleaseGate(bucket.System, logger);

			Factory.ReloadBusinessObjects(ProcessHeaderSchema.Instance, ccpmWorkflow, nonCcpmWorkflow);

			return logger;
		}

		protected BMSystem system;

		protected override void SetUp()
		{
			base.SetUp();
			BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			system = BMSTestHelper.CreateSystem(Factory, "ORG");
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}

		#endregion
	}
}
