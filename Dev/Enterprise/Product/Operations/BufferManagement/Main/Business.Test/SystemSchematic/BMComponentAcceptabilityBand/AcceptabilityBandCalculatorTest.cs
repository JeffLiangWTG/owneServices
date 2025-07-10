using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.PAVE.MENT.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class AcceptabilityBandCalculatorTest : BMSTestCaseWithFactory
	{
		#region Use Ment Results

		[UseSnapshotProtection(true)]
		[TestDate(2023, 8, 23, 10, 0, 0)]
		public void TestBandCalculation_ShouldGetCorrectMENT_Result_WhenTwoBandsWithSameMENT_Code()
		{
			BMSRegistry.Instance.AcceptabilityBandUseMENT.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var band1 = Factory.New<BMComponentAcceptabilityBand>();
			band1.BAB_Name = "A big band the best one Jazz Band";
			band1.BAB_Type = "SQL";
			band1.BAB_SqlText = "select 1 as Value, null as ReleaseGroup, null as Component";

			var band2 = Factory.New<BMComponentAcceptabilityBand>();
			band2.BAB_Name = "A big band the best one Rock Band";
			band2.BAB_Type = "SQL";
			band2.BAB_SqlText = "select 2 as Value, null as ReleaseGroup, null as Component";

			var mentAcceptabilityBandViewModel1 = new MENTAcceptabilityBandViewModel(band1);
			mentAcceptabilityBandViewModel1.MentEnabled = true;
			mentAcceptabilityBandViewModel1.Query.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);

			var mentAcceptabilityBandViewModel2 = new MENTAcceptabilityBandViewModel(band2);
			mentAcceptabilityBandViewModel2.MentEnabled = true;
			mentAcceptabilityBandViewModel2.Query.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);

			Factory.Save();

			AssertEquals("PRECONDITION", mentAcceptabilityBandViewModel1.Query.MAQ_Code, mentAcceptabilityBandViewModel2.Query.MAQ_Code);

			var parameters1 = new AcceptabilityBandSqlBuilderParameters(band1, shouldFilterByReleaseGroup: AcceptabilityBandVisualizationOption.No, shouldFilterBySection: AcceptabilityBandVisualizationOption.No);
			var parameters2 = new AcceptabilityBandSqlBuilderParameters(band2, shouldFilterByReleaseGroup: AcceptabilityBandVisualizationOption.No, shouldFilterBySection: AcceptabilityBandVisualizationOption.No);
			var result1 = 0m;
			var result2 = 0m;

			var mentServiceTask = new AgedScoresServiceTaskForTest();
			mentServiceTask.Run();

			using (Db.Connection.TrackExecutedCommands())
			{
				result1 = CalculateStatusForSingleResult(band1, parameters1).Value.Value;
				result2 = CalculateStatusForSingleResult(band2, parameters2).Value.Value;
				var abCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("-- Acceptability Band")).ToArray();
				AssertEquals("only 2 AB query was executed", 2, abCommands.Length);
				AssertContains("A MENT query", "-- Acceptability Band Calculation Using MENT", abCommands[0]);
				AssertContains("A MENT query", "-- Acceptability Band Calculation Using MENT", abCommands[1]);
			}

			AssertEquals(1m, result1);
			AssertEquals(2m, result2);
		}

		[UseSnapshotProtection(true)]
		[TestDate(2023, 8, 23, 10, 0, 0)]
		public void TestBandCalculation_ShouldUseMENT_Result_WhenEnabled()
		{
			BMSRegistry.Instance.AcceptabilityBandUseMENT.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, workflowType: WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, name: "band", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_FiltersByReleaseGroup = false;
			band.BAB_FiltersBySection = false;

			var mentAcceptabilityBandViewModel = new MENTAcceptabilityBandViewModel(band);

			mentAcceptabilityBandViewModel.MentEnabled = true;
			mentAcceptabilityBandViewModel.Query.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			Factory.Save();

			var parameters = new AcceptabilityBandSqlBuilderParameters(band, shouldFilterByReleaseGroup: AcceptabilityBandVisualizationOption.No, shouldFilterBySection: AcceptabilityBandVisualizationOption.No);
			var result = 0m;

			var mentMetricCount = (int)Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.MENTAgedScoreMetric WHERE MAS_BAB_AcceptabilityBand = '{band.PK}'");
			AssertEquals("MENT service task did not run yet, so not metrics", 0, mentMetricCount);

			using (Db.Connection.TrackExecutedCommands())
			{
				result = CalculateStatusForSingleResult(band, parameters).Value.Value;
				var abCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("-- Acceptability Band")).ToArray();
				AssertEquals("2 AB query was executed", 2, abCommands.Length);
				AssertContains("A MENT query that returned null", "-- Acceptability Band Calculation Using MENT", abCommands.Single(c => c.Contains("Using MENT")));
				AssertContains("and a normal AB query", "-- Acceptability Band name", abCommands.Single(c => !c.Contains("Using MENT")));
			}

			AssertEquals(3m, result);

			var mentServiceTask = new AgedScoresServiceTaskForTest();
			mentServiceTask.Run();

			mentMetricCount = (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.MENTAgedScoreMetric");
			AssertEquals("Ment service task run, so 2 metrics should be stored, one for workflows without release group and other with release group",
				2, mentMetricCount);

			using (Db.Connection.TrackExecutedCommands())
			{
				result = CalculateStatusForSingleResult(band, parameters).Value.Value;
				var abCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("-- Acceptability Band")).ToArray();
				AssertEquals("only 1 AB query was executed", 1, abCommands.Length);
				AssertContains("A MENT query", "-- Acceptability Band Calculation Using MENT", abCommands[0]);
			}

			AssertEquals(3m, result);
		}

		[UseSnapshotProtection(true)]
		[TestDate(2023, 8, 23, 10, 0, 0)]
		public void TestBandCalculation_ShouldUseMENT_Result_WhenEnabled_AndConsiderReleaseGroup()
		{
			BMSRegistry.Instance.AcceptabilityBandUseMENT.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, workflowType: WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, name: "band", type: AcceptabilityBandTypes.Codes.Count);
			var otherGroup = BMSTestHelper.CreateGroup(Factory, "Other");
			band.BAB_FiltersByReleaseGroup = true;
			band.BAB_FiltersBySection = false;

			var mentAcceptabilityBandViewModel = new MENTAcceptabilityBandViewModel(band);

			mentAcceptabilityBandViewModel.MentEnabled = true;
			mentAcceptabilityBandViewModel.Query.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = CreateWorkflow(jobHeader, "workflow3", releaseGroupPK: otherGroup.PK);
			var workflow4 = CreateWorkflow(jobHeader, "workflow4", releaseGroupPK: otherGroup.PK);

			Factory.Save();

			var parameters = new AcceptabilityBandSqlBuilderParameters(band, shouldFilterByReleaseGroup: AcceptabilityBandVisualizationOption.Yes, shouldFilterBySection: AcceptabilityBandVisualizationOption.No)
			{
				ReleaseGroupPK = config.ReleaseGroup.PK,
			};

			var result = 0m;

			var mentMetricCount = (int)Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.MENTAgedScoreMetric WHERE MAS_BAB_AcceptabilityBand = '{band.PK}'");
			AssertEquals("MENT service task did not run yet, so not metrics", 0, mentMetricCount);

			using (Db.Connection.TrackExecutedCommands())
			{
				result = CalculateStatusForSingleResult(band, parameters).Value.Value;
				var abCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("-- Acceptability Band")).ToArray();
				AssertEquals("2 AB query was executed", 2, abCommands.Length);
				AssertContains("A MENT query that returned null", "-- Acceptability Band Calculation Using MENT", abCommands.Single(c => c.Contains("Using MENT")));
				AssertContains("and a normal AB query", "-- Acceptability Band name", abCommands.Single(c => !c.Contains("Using MENT")));
			}

			AssertEquals("Only 2 workflows are in the release group", 2m, result);

			var mentServiceTask = new AgedScoresServiceTaskForTest();
			mentServiceTask.Run();

			mentMetricCount = (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.MENTAgedScoreMetric");
			AssertEquals("Ment service task run, so 3 metrics should be stored, one for workflows without release group, one for config Release group and other for otherGroup",
				3, mentMetricCount);

			using (Db.Connection.TrackExecutedCommands())
			{
				result = CalculateStatusForSingleResult(band, parameters).Value.Value;
				var abCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("-- Acceptability Band")).ToArray();
				AssertEquals("only 1 AB query was executed", 1, abCommands.Length);
				AssertContains("A MENT query", "-- Acceptability Band Calculation Using MENT", abCommands[0]);
			}

			AssertEquals("From MENT still should be 2", 2m, result);
		}

		[UseSnapshotProtection(true)]
		[TestDate(2023, 8, 23, 10, 0, 0)]
		public void TestBandCalculation_ShouldUseMENT_Result_WhenEnabled_AndConsiderComponent()
		{
			BMSRegistry.Instance.AcceptabilityBandUseMENT.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, workflowType: WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, name: "band", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_FC_Component = config.Buffer.PK;
			band.BAB_FiltersByReleaseGroup = false;
			band.BAB_FiltersBySection = false;

			var mentAcceptabilityBandViewModelForBand = new MENTAcceptabilityBandViewModel(band);
			mentAcceptabilityBandViewModelForBand.MentEnabled = true;
			mentAcceptabilityBandViewModelForBand.Query.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", currentComponent: config.Buffer);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", currentComponent: config.Buffer);
			var workflow3 = CreateWorkflow(jobHeader, "workflow3", currentComponent: config.Buffer);
			var workflow4 = CreateWorkflow(jobHeader, "workflow4", currentComponent: config.Bucket);

			Factory.Save();

			var parameters = new AcceptabilityBandSqlBuilderParameters(band, shouldFilterByReleaseGroup: AcceptabilityBandVisualizationOption.Yes, shouldFilterBySection: AcceptabilityBandVisualizationOption.No);
			var result = 0m;

			var mentMetricCount = (int)Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.MENTAgedScoreMetric WHERE MAS_BAB_AcceptabilityBand = '{band.PK}'");
			AssertEquals("MENT service task did not run yet, so not metrics", 0, mentMetricCount);

			using (Db.Connection.TrackExecutedCommands())
			{
				result = CalculateStatusForSingleResult(band, parameters).Value.Value;
				var abCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("-- Acceptability Band")).ToArray();
				AssertEquals("2 AB query was executed", 2, abCommands.Length);
				AssertContains("A MENT query that returned null", "-- Acceptability Band Calculation Using MENT", abCommands.Single(c => c.Contains("Using MENT")));
				AssertContains("and a normal AB query", "-- Acceptability Band name", abCommands.Single(c => !c.Contains("Using MENT")));
			}

			AssertEquals("3 workflows are in the buffer", 3m, result);

			band.BAB_FC_Component = ZGuid.Empty;
			Factory.Save();

			var mentServiceTask = new AgedScoresServiceTaskForTest();
			mentServiceTask.Run();

			mentMetricCount = (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.MENTAgedScoreMetric");
			AssertEquals("Ment service task run, 3 metrics should be stored, one for not component, one for buffer and other for bucket", 3, mentMetricCount);

			band.BAB_FC_Component = config.Buffer.PK;
			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			{
				result = CalculateStatusForSingleResult(band, parameters).Value.Value;
				var abCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("-- Acceptability Band")).ToArray();
				AssertEquals("only 1 AB query was executed", 1, abCommands.Length);
				AssertContains("A MENT query", "-- Acceptability Band Calculation Using MENT", abCommands[0]);
			}

			AssertEquals("From MENT result still should be 3", 3m, result);
		}

		[UseSnapshotProtection(true)]
		[TestDate(2023, 8, 23, 10, 0, 0)]
		public void TestBandCalculation_ShouldUseMENT_Result_OnlyIfResultsAreNotExpired() //based on the local cache timeout or new registry
		{
			BMSRegistry.Instance.AcceptabilityBandUseMENT.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.AcceptabilityBandUseMentExpirationMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 30);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, workflowType: WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, name: "band", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_FiltersByReleaseGroup = false;
			band.BAB_FiltersBySection = false;

			TestDateAttribute.Date = new ZDateTime(Db.Connection.ExecuteScalar("SELECT GETUTCDATE()")).ToDateTime();

			var mentAcceptabilityBandViewModel = new MENTAcceptabilityBandViewModel(band);

			mentAcceptabilityBandViewModel.MentEnabled = true;
			mentAcceptabilityBandViewModel.Query.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			Factory.Save();

			var parameters = new AcceptabilityBandSqlBuilderParameters(band, shouldFilterByReleaseGroup: AcceptabilityBandVisualizationOption.No, shouldFilterBySection: AcceptabilityBandVisualizationOption.No);
			var result = 0m;

			var mentServiceTask = new AgedScoresServiceTaskForTest();
			mentServiceTask.Run();

			var mentMetricCount = (int)Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.MENTAgedScoreMetric WHERE MAS_BAB_AcceptabilityBand = '{band.PK}'");
			AssertEquals("Ment service task run, so 2 metrics should be stored, one for workflows without release group and other for the release config release group",
				2, mentMetricCount);

			using (Db.Connection.TrackExecutedCommands())
			{
				result = CalculateStatusForSingleResult(band, parameters).Value.Value;
				var abCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("-- Acceptability Band")).ToArray();
				AssertEquals("only 1 AB query was executed", 1, abCommands.Length);
				AssertContains("A MENT query", "-- Acceptability Band Calculation Using MENT", abCommands[0]);
			}

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(40);

			using (Db.Connection.TrackExecutedCommands())
			{
				result = CalculateStatusForSingleResult(band, parameters).Value.Value;
				var abCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("-- Acceptability Band")).ToArray();
				AssertEquals("2 AB query was executed", 2, abCommands.Length);
				AssertContains("A MENT query that returned null because it was expired!", "-- Acceptability Band Calculation Using MENT", abCommands.Single(c => c.Contains("Using MENT")));
				AssertContains("Only a normal AB query was executed", "-- Acceptability Band name", abCommands.Single(c => !c.Contains("Using MENT")));
			}

			AssertEquals(3m, result);
		}

		[TestDate(2023, 8, 23, 10, 0, 0)]
		public void TestBandCalculation_ShouldNotUseMENT_Result_WhenBandFilterByWorkflowOnSections()
		{
			BMSRegistry.Instance.AcceptabilityBandUseMENT.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, workflowType: WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, name: "band", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_FiltersByReleaseGroup = false;
			band.BAB_FiltersBySection = false;

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			Factory.Save();

			var parameters = new AcceptabilityBandSqlBuilderParameters(band, shouldFilterByReleaseGroup: AcceptabilityBandVisualizationOption.No, shouldFilterBySection: AcceptabilityBandVisualizationOption.Yes);
			var result = 0m;

			using (Db.Connection.TrackExecutedCommands())
			{
				result = CalculateStatusForSingleResult(band, parameters).Value.Value;
				var abCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("-- Acceptability Band")).ToArray();
				AssertEquals("1 AB query was executed", 1, abCommands.Length);
				AssertContains("Only a normal AB query was executed", "-- Acceptability Band name", abCommands.Single(c => !c.Contains("Using MENT")));
			}

			AssertEquals(3m, result);
		}

		public void TestBandCalculation_ShouldNotUseMENT_Result_WhenBandHasAdditionalAggregator()
		{
			BMSRegistry.Instance.AcceptabilityBandUseMENT.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, workflowType: WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, name: "band", type: AcceptabilityBandTypes.Codes.SQL);
			band.BAB_FiltersByReleaseGroup = false;
			band.BAB_FiltersBySection = false;
			band.BAB_SqlText = "SELECT GS_Code Value, GS_PK Component, GS_PK ReleaseGroup, GS_Code AdditionalAggregator FROM dbo.GlbStaff";

			Factory.Save();

			var parameters = new AcceptabilityBandSqlBuilderParameters(band, shouldFilterByReleaseGroup: AcceptabilityBandVisualizationOption.No, shouldFilterBySection: AcceptabilityBandVisualizationOption.Yes);

			using (Db.Connection.TrackExecutedCommands())
			{
				CalculateStatusForSingleResult(band, parameters);
				var abCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("-- Acceptability Band")).ToArray();
				AssertEquals("1 AB query was executed", 1, abCommands.Length);
				AssertContains("Only a normal AB query was executed", "-- Acceptability Band name", abCommands.Single(c => !c.Contains("Using MENT")));
			}
		}

		#endregion

		#region AllowCompanyFiltersInTagRules and Filtered by Tag

		public void TestCalculateStatus_WhenFilteredByTag()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var anotherGroup = Factory.NewWithValidTestData<GlbGroup>();
			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);

			// Two workflows in the buffer - one with the specified tag, one without.
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			workflow1.AddTag(config.RedTag);

			// Two workflows in another component - both with the specified tag.
			var workflow3 = CreateWorkflow(jobHeader, "workflow3", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow4 = CreateWorkflow(jobHeader, "workflow4", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			workflow3.AddTag(config.RedTag);
			workflow4.AddTag(config.RedTag);

			// Two workflows in the buffer - both with the specified tag, but in other release groups.
			var workflow5 = CreateWorkflow(jobHeader, "workflow5", config.Buffer, releaseGroupPK: anotherGroup.PK);
			var workflow6 = CreateWorkflow(jobHeader, "workflow6", config.Buffer, releaseGroupPK: anotherGroup.PK);
			workflow5.AddTag(config.RedTag);
			workflow6.AddTag(config.RedTag);

			var band = CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 10, 10, 10, "Ban this band immediband", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_FiltersByReleaseGroup = true;

			Factory.Save();

			var resultNotIncludingTagOrReleaseGroup = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));
			AssertEquals(4m, resultNotIncludingTagOrReleaseGroup.Value.Value);

			var parameters = new AcceptabilityBandSqlBuilderParameters(band) { ReleaseGroupPK = config.ReleaseGroup.PK };
			var resultNotIncludingTag = CalculateStatusForSingleResult(band, parameters);
			AssertEquals(2m, resultNotIncludingTag.Value.Value);

			parameters = new AcceptabilityBandSqlBuilderParameters(band) { ReleaseGroupPK = config.ReleaseGroup.PK, Tag = config.RedTag };
			var resultIncludingTag = CalculateStatusForSingleResult(band, parameters);
			AssertEquals(1m, resultIncludingTag.Value.Value);
		}

		public void TestCalculateStatus_WhenFilteredByTagOnJobHeader()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var anotherGroup = Factory.NewWithValidTestData<GlbGroup>();
			var jobHeader1 = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);

			var workflow1_1 = CreateWorkflow(jobHeader1, "workflow1_1", config.Buffer);
			var workflow1_2 = CreateWorkflow(jobHeader1, "workflow1_2", config.Buffer);
			var workflow2_1 = CreateWorkflow(jobHeader2, "workflow2_1", config.Buffer);

			jobHeader1.AddTag(config.RedTag);

			var band = CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 10, 10, 10, "Ban this band immediband", type: AcceptabilityBandTypes.Codes.Count);

			Factory.Save();

			var parameters = new AcceptabilityBandSqlBuilderParameters(band) { Tag = config.RedTag };
			var result = CalculateStatusForSingleResult(band, parameters);
			AssertEquals(2m, result.Value.Value);
		}

		#endregion

		#region Types: NumWorkflows, Duration, NumberAsPercentage, PlannedDurationPercentage

		public void TestStatusPolarity_ForNumWorkflowsBand()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = CreateAcceptabilityBand_WorkflowsInComponent(config.Buffer, 1, 2, 3, 3, 4, 5);
			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);

			Factory.Save();
			AssertAcceptabilityBandResult(band, ComponentAcceptabilityStatus.HighRisk, AcceptabilityStatusPolarity.Low);

			CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			Factory.Save();
			AssertAcceptabilityBandResult(band, ComponentAcceptabilityStatus.Caution, AcceptabilityStatusPolarity.Low);

			CreateWorkflow(jobHeader, "workflow2", config.Buffer);
			Factory.Save();
			AssertAcceptabilityBandResult(band, ComponentAcceptabilityStatus.Good, AcceptabilityStatusPolarity.Low);

			CreateWorkflow(jobHeader, "workflow3", config.Buffer);
			Factory.Save();
			AssertAcceptabilityBandResult(band, ComponentAcceptabilityStatus.Excellent, AcceptabilityStatusPolarity.Middle);

			CreateWorkflow(jobHeader, "workflow4", config.Buffer);
			Factory.Save();
			AssertAcceptabilityBandResult(band, ComponentAcceptabilityStatus.Good, AcceptabilityStatusPolarity.High);

			CreateWorkflow(jobHeader, "workflow5", config.Buffer);
			Factory.Save();
			AssertAcceptabilityBandResult(band, ComponentAcceptabilityStatus.Caution, AcceptabilityStatusPolarity.High);

			CreateWorkflow(jobHeader, "workflow6", config.Buffer);
			Factory.Save();
			AssertAcceptabilityBandResult(band, ComponentAcceptabilityStatus.HighRisk, AcceptabilityStatusPolarity.High);
		}

		public void TestCalculateStatus_ForTotalDurationBand()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(config.Buffer, 1, 3, 5, 5, 7, 9, "Total Planned Duration", type: AcceptabilityBandTypes.Codes.TotalPlannedDuration);

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = CreateWorkflow(jobHeader, "workflow4");
			var workflow5 = CreateWorkflow(jobHeader, "workflow5");

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 120, estVariationFactor: 1);
			var task3 = CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 180, estVariationFactor: 1);
			var task4 = CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 240, estVariationFactor: 1);
			var task5 = CreateTask(workflow5, GlbStaff.CurrentUser.GS_Code, 300, estVariationFactor: 1);

			var result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));
			AssertAcceptabilityBandResult("No items match the AB filter (nothing in buffer)", result, ComponentAcceptabilityStatus.HighRisk, true, 0);

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));
			AssertAcceptabilityBandResult("1 item is in buffer", result, ComponentAcceptabilityStatus.Caution, true, 1m);

			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));
			AssertAcceptabilityBandResult("2 items are in buffer", result, ComponentAcceptabilityStatus.Good, true, 3m);

			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));
			AssertAcceptabilityBandResult("3 items are in buffer", result, ComponentAcceptabilityStatus.Good, true, 6m);

			workflow4.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));
			AssertAcceptabilityBandResult("4 items are in buffer", result, ComponentAcceptabilityStatus.HighRisk, true, 10m);

			workflow5.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));
			AssertAcceptabilityBandResult("5 items are in buffer", result, ComponentAcceptabilityStatus.HighRisk, true, 15m);

			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));
			AssertAcceptabilityBandResult("5 items are in buffer, but one is closed", result, ComponentAcceptabilityStatus.HighRisk, true, 10m);

			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));
			AssertAcceptabilityBandResult("5 items are in buffer, but two are closed", result, ComponentAcceptabilityStatus.Good, true, 6m);
		}

		public void TestCalculateStatus_ForNumberAsPercentageBand_NoReleaseGroup()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(config.Buffer, 40, 50, 66, 67, 80, 90, "Planned Duration Percentage", type: AcceptabilityBandTypes.Codes.NumberAsPercentage);

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = CreateWorkflow(jobHeader, "workflow4", releaseGroupPK: config.ReleaseGroup.PK);
			workflow1.AddTag(config.RedTag);
			workflow3.AddTag(config.RedTag);

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);

			var bandParams = new AcceptabilityBandSqlBuilderParameters(band) { Tag = config.RedTag };

			var result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("No items match the AB filter (nothing in buffer)", result, ComponentAcceptabilityStatus.HighRisk, true, 0);

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("1 item is in buffer", result, ComponentAcceptabilityStatus.HighRisk, true, 100.00m);

			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("2 items are in buffer", result, ComponentAcceptabilityStatus.Good, true, 50.00m);

			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("3 items are in buffer", result, ComponentAcceptabilityStatus.Excellent, true, 66.67m);

			workflow4.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();
		}

		public void TestCalculateStatus_ForNumberAsPercentageBand_WithoutComponent()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 40, 50, 66, 67, 80, 90, "Planned Duration Percentage", type: AcceptabilityBandTypes.Codes.NumberAsPercentage);
			band.BAB_FiltersByReleaseGroup = true;

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = CreateWorkflow(jobHeader, "workflow3", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow4 = CreateWorkflow(jobHeader, "workflow4", releaseGroupPK: config.ReleaseGroup.PK);
			workflow1.AddTag(config.RedTag);
			workflow3.AddTag(config.RedTag);

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);

			var bandParams = new AcceptabilityBandSqlBuilderParameters(band) { ReleaseGroupPK = config.ReleaseGroup.PK, Tag = config.RedTag };
			Factory.Save();

			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;

			var result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("One item is in buffer, component does not matter.", result, ComponentAcceptabilityStatus.Good, true, 50.00m);
		}

		public void TestCalculateStatus_ForNumberAsPercentageBand()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(config.Buffer, 40, 50, 66, 67, 80, 90, "Number As Percentage", type: AcceptabilityBandTypes.Codes.NumberAsPercentage);
			band.BAB_FiltersByReleaseGroup = true;

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = CreateWorkflow(jobHeader, "workflow3", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow4 = CreateWorkflow(jobHeader, "workflow4", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow5 = CreateWorkflow(jobHeader, "workflow5", releaseGroupPK: config.ReleaseGroup.PK);
			var workflowNotInRg = CreateWorkflow(jobHeader, "workflowNotInRG", releaseGroupPK: Factory.New<GlbGroup>().PK);
			workflow1.AddTag(config.RedTag);
			workflow3.AddTag(config.RedTag);

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 120, estVariationFactor: 1);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 180, estVariationFactor: 1);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 240, estVariationFactor: 1);
			CreateTask(workflow5, GlbStaff.CurrentUser.GS_Code, 300, estVariationFactor: 1);

			var bandParams = new AcceptabilityBandSqlBuilderParameters(band) { ReleaseGroupPK = config.ReleaseGroup.PK, Tag = config.RedTag };

			var result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("No items match the AB filter (nothing in buffer)", result, ComponentAcceptabilityStatus.HighRisk, true, 0);

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("1 item is in buffer", result, ComponentAcceptabilityStatus.HighRisk, true, 100.0m);

			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("2 items are in buffer", result, ComponentAcceptabilityStatus.Good, true, 50.0m);

			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("3 items are in buffer", result, ComponentAcceptabilityStatus.Excellent, true, 66.67m);

			workflow4.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("4 items are in buffer", result, ComponentAcceptabilityStatus.Good, true, 50.00m);

			workflow5.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("5 items are in buffer", result, ComponentAcceptabilityStatus.Caution, true, 40.00m);
		}

		public void TestCalculateStatus_ForPlannedDurationAsPercentageBand_WithoutComponent()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 40, 50, 66, 67, 80, 90, "Planned Duration Percentage", type: AcceptabilityBandTypes.Codes.PlannedDurationPercentage);
			band.BAB_FiltersByReleaseGroup = true;

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = CreateWorkflow(jobHeader, "workflow4", releaseGroupPK: config.ReleaseGroup.PK);
			workflow1.AddTag(config.RedTag);
			workflow3.AddTag(config.RedTag);

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);

			var bandParams = new AcceptabilityBandSqlBuilderParameters(band) { ReleaseGroupPK = config.ReleaseGroup.PK, Tag = config.RedTag };
			Factory.Save();

			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;

			var result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("One item is in buffer, component does not matter.", result, ComponentAcceptabilityStatus.Good, true, 50.00m);
		}

		public void TestCalculateStatus_ForPlannedDurationAsPercentageBand_WithoutReleaseGroup()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(config.Buffer, 40, 50, 66, 67, 80, 90, "Planned Duration Percentage", type: AcceptabilityBandTypes.Codes.PlannedDurationPercentage);

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = CreateWorkflow(jobHeader, "workflow4", releaseGroupPK: config.ReleaseGroup.PK);
			workflow1.AddTag(config.RedTag);
			workflow3.AddTag(config.RedTag);

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);

			var bandParams = new AcceptabilityBandSqlBuilderParameters(band) { Tag = config.RedTag };

			var result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("No items match the AB filter (nothing in buffer)", result, ComponentAcceptabilityStatus.HighRisk, true, 0);

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("1 item is in buffer", result, ComponentAcceptabilityStatus.HighRisk, true, 100.00m);

			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("2 items are in buffer", result, ComponentAcceptabilityStatus.Good, true, 50.00m);

			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("3 items are in buffer", result, ComponentAcceptabilityStatus.Excellent, true, 66.67m);

			workflow4.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("4 items are in buffer", result, ComponentAcceptabilityStatus.Good, true, 50.00m);
		}

		public void TestCalculateStatus_ForPlannedDurationAsPercentageBand()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(config.Buffer, 40, 50, 66, 67, 80, 90, "Planned Duration Percentage", type: AcceptabilityBandTypes.Codes.PlannedDurationPercentage);

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = CreateWorkflow(jobHeader, "workflow3", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow4 = CreateWorkflow(jobHeader, "workflow4", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow5 = CreateWorkflow(jobHeader, "workflow5", releaseGroupPK: config.ReleaseGroup.PK);
			var workflowNotInRg = CreateWorkflow(jobHeader, "workflowNotInRG", releaseGroupPK: Factory.New<GlbGroup>().PK);
			workflow1.AddTag(config.RedTag);
			workflow3.AddTag(config.RedTag);

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);
			CreateTask(workflow5, GlbStaff.CurrentUser.GS_Code, 100, estVariationFactor: 1);

			var bandParams = new AcceptabilityBandSqlBuilderParameters(band) { ReleaseGroupPK = config.ReleaseGroup.PK, Tag = config.RedTag };

			var result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("No items match the AB filter (nothing in buffer)", result, ComponentAcceptabilityStatus.HighRisk, true, 0);

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("1 item is in buffer", result, ComponentAcceptabilityStatus.HighRisk, true, 100.00m);

			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("2 items are in buffer", result, ComponentAcceptabilityStatus.Good, true, 50.00m);

			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("3 items are in buffer", result, ComponentAcceptabilityStatus.Excellent, true, 66.67m);

			workflow4.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("4 items are in buffer", result, ComponentAcceptabilityStatus.Good, true, 50.00m);

			workflow5.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			result = CalculateStatusForSingleResult(band, bandParams);
			AssertAcceptabilityBandResult("5 items are in buffer", result, ComponentAcceptabilityStatus.Caution, true, 40.00m);
		}

		#endregion

		#region Check for inactivity

		public void TestGetOrCalculateStatus_CheckInactiveAcceptabilityBand_ShouldNotCalculate()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 1, 3, 5, 5, 7, 9, name: "CheckInactiveBand");
			band.BAB_IsActive = false;
			Factory.Save();
			Assert(!band.BAB_IsActive);

			var result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));
			AssertAcceptabilityBandResult("Acceptability band is inactive", result, ComponentAcceptabilityStatus.None, resultFound: false, value: null);
			AssertEquals("No error should be reported if the AB Acceptability band is inactive", string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestGetOrCalculateStatus_ToggleAcceptabilityBandActiveStatus()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 1, 3, 5, 5, 7, 9, name: "CheckToggledBand");
			band.BAB_IsActive = true;
			Factory.Save();
			Assert(band.BAB_IsActive);

			var result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));
			AssertAcceptabilityBandResult("Acceptability band is active", result, ComponentAcceptabilityStatus.HighRisk, resultFound: true, value: 0.00M);

			band.BAB_IsActive = false;
			Factory.Save();
			Assert(!band.BAB_IsActive);

			result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));
			AssertAcceptabilityBandResult("Acceptability band is inactive", result, ComponentAcceptabilityStatus.None, resultFound: false, value: null);
			AssertEquals("No error should be reported if the AB Acceptability band is inactive", string.Empty, ErrorReporter.LastMessageReported);
		}

		#endregion

		#region Exception Handling

		public void TestGetScalarResult_ThrowSqlException()
		{
			var postmasterQuery = new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.Equal, User.PostMasterUserName);
			var postmaster = Factory.Load<GlbStaff>(postmasterQuery).Single();
			postmaster.GS_EmailAddress = "x@wisetechglobal.com";
			Factory.Save();

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var acceptabilityBand = CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 10, 10, 10, "Ban this band immediband", sql: "SELECT * FROM InvalidTableName" + DbCommand.ExecuteAsReaderFlagComments);
			var parameters = new AcceptabilityBandSqlBuilderParameters(acceptabilityBand);
			CalculateStatusForSingleResult(acceptabilityBand, parameters);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var body = Env.OutgoingMailManager.EmailsCreated[0].Body;
			var executeAsReaderFlag = DbCommand.ExecuteAsReaderFlagComments.Replace("\n", "").Trim();
			AssertNotContains(executeAsReaderFlag, body);
			AssertContains(DbCommand.ExecuteAsReaderFlagMask, body);
		}

		#endregion

		#region Helpers

		static AcceptabilityBandResult CalculateStatusForSingleResult(
			BMComponentAcceptabilityBand band,
			AcceptabilityBandSqlBuilderParameters parameters,
			AcceptabilityBandDataProvider provider = null)
		{
			if (provider == null)
			{
				provider = new AcceptabilityBandDataProvider(SecondaryServerConnectionProviderProvider.GetProvider());
			}

			return new AcceptabilityBandCalculator().CalculateStatus(band, provider, parameters).Single();
		}

		void AssertAcceptabilityBandResult(
			BMComponentAcceptabilityBand band,
			ComponentAcceptabilityStatus status,
			AcceptabilityStatusPolarity polarity, decimal? expectedValue = null)
		{
			var result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));

			CombineAssertions(() =>
			{
				AssertEquals("Status", status, result.Status);
				AssertEquals("StatusPolarity", polarity, result.StatusPolarity);

				if (expectedValue != null)
				{
					AssertEquals("Value", expectedValue.Value, result.Value);
				}
			});
		}

		#endregion
	}
}
