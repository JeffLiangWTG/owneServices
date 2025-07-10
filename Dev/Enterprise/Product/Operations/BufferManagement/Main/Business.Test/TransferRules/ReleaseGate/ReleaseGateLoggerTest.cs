using System;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	class ReleaseGateLoggerTest : BMSTestCaseWithFactory
	{
		#region Parallel Access

		public void TestParallelAccessForGoldenRules_ShouldNotThrowExceptions()
		{
			var workflowPK = ZGuid.NewZGuid();
			var component = BMSTestHelper.CreateBuffer(BMSTestHelper.CreateSystem(Factory), name: $"buffer X");

			var logger = new ReleaseGateLogger();

			Parallel.For(0, 100, (index) =>
			{
				for (var i = 0; i < 100; i++)
				{
					AssertNoExceptionThrown("Accessing log from multithreads should not cause exception", () => logger.LogReleaseFailure(workflowPK, component, "noteText ...", goldenRulePK: ZGuid.NewZGuid()));
				}
			});
		}

		public void TestParallelAccessForNonGoldenRules_ShouldNotThrowExceptions()
		{
			var workflowPK = ZGuid.NewZGuid();
			var system = BMSTestHelper.CreateSystem(Factory);

			var componentsCounts = 100;
			var components = new BMComponent[componentsCounts];
			for (var i = 0; i < componentsCounts; i++)
			{
				var component = BMSTestHelper.CreateBuffer(system, name: $"buffer{i}");
				components[i] = component;
			}

			var logger = new ReleaseGateLogger();

			var totalOuterIteration = 10;
			var totalInnerIteration = 10;
			AssertEquals("componentsCounts = iteration1 x iteration2", componentsCounts, totalOuterIteration * totalInnerIteration);

			Parallel.For(0, totalOuterIteration, (outerIteration) =>
			{
				for (var innerIteration = 0; innerIteration < totalInnerIteration; innerIteration++)
				{
					AssertNoExceptionThrown("Accessing log from multithreads should not cause exception", () => logger.LogReleaseFailure(workflowPK, components[outerIteration * 10 + innerIteration], "noteText ..."));
				}
			});
		}

		#endregion

		public void TestShowingDateTimeForAllBuffers()
		{
			var system = CreateSystem();
			var buffer1 = CreateBuffer(system, "buffer1");
			var buffer2 = CreateBuffer(system, "buffer2");
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "Azog The Defiler";

			var logger = new ReleaseGateLogger();

			logger.LogCapacityReservation(staff.GS_Code, buffer1.PK, "Some dummy text1");
			logger.LogCapacityReservation(staff.GS_Code, buffer2.PK, "Some dummy text2");

			logger.CommitAllLogs(Factory);

			var query = new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff.GS_Code);
			var componentLinks = Factory.Load<BMComponentResourceLink>(query);

			foreach (var componentLink in componentLinks)
			{
				AssertStartsWith("Should start with date and time for all buffers", "Release Gate Run at", componentLink.CapacityReservationDetails);
			}
		}

		public void TestLogReleaseFailure_ShouldNotAddStmNote()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", config.Bucket);
			Factory.Save();

			var query = new ZQuery(StmNoteSchema.ST_ParentID, workflow.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, ProcessHeaderSchema.Constants.TableName);
			var results = Factory.Load<StmNote>(query);

			AssertContainsExactElementsInAnyOrder("There should be no notes for the new workflow, and yet...", Array.Empty<StmNote>(), results);

			var logger = new ReleaseGateLogger();
			logger.LogReleaseFailure(workflow, config.Buffer, "You suck");
			logger.CommitAllLogs(Factory);

			results = Factory.Load<StmNote>(query);
			AssertContainsExactElementsInAnyOrder("No StmNote should have been saved because the logger should now use a memory cache for release failures, and yet...", Array.Empty<StmNote>(), results);
		}

		public void TestLogExpiryTime() => AssertEquals(TimeSpan.FromHours(1), SystemSchematicServiceTaskBase.LogExpiryTime);
	}
}
