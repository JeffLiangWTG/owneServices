using System;
using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestUtcOffset(10, 0, 0)]
	[TestDate(2014, 9, 10)]
	class VeryComplexNetworkTest : NetworkTestCase
	{
		#region Correctness

		[ExpectNoExceptions]
		public void TestClone()
		{
			network.DiagramEntity.CloneDiagramAndAllDescendants();
			network.DiagramShape.Factory.Save();
		}

		public void TestRelatedBuffers()
		{
			var iterationBuffer = (BMNCNBufferShape)network["Iteration Buffer"];
			var subDiagramBuffer = (BMNCNBufferShape)network["Sub-diagram Feeding Buffer"];
			var ccBranchBuffer = (BMNCNBufferShape)network["CC Branch Workflow Feeding Buffer"];
			var nonCCBranchBuffer2 = (BMNCNBufferShape)network["Non-CC Branch 2 Feeding Buffer"];
			var nonCCBranchBuffer3 = (BMNCNBufferShape)network["Non-CC Branch 3 Feeding Buffer"];
			var projectBuffer = (BMNCNBufferShape)network["Project Buffer"];
			network.Refresh(RefreshType.RedrawDiagram);

			CombineAssertions(() =>
			{
				// CC and friends
				AssertRelatedBuffers(network["HLD"], iterationBuffer, ccBranchBuffer, subDiagramBuffer, projectBuffer);
				AssertRelatedBuffers(network["Design"], iterationBuffer, ccBranchBuffer, subDiagramBuffer, projectBuffer);
				AssertRelatedBuffers(network["Unbuffered Workflow"], iterationBuffer, ccBranchBuffer, subDiagramBuffer, projectBuffer);
				AssertRelatedBuffers(network["Coding"], projectBuffer);
				AssertRelatedBuffers(network["Review"], projectBuffer);
				AssertRelatedBuffers(network["Resource Conflict Workflow"], projectBuffer);
				AssertRelatedBuffers(network["Publish"], projectBuffer);

				// Branch from the CC
				AssertRelatedBuffers(network["CC Branch Workflow"], ccBranchBuffer, subDiagramBuffer, projectBuffer);
				AssertRelatedBuffers(network["Sub-diagram"], subDiagramBuffer, projectBuffer);
				AssertRelatedBuffers(network["Workflow1"], subDiagramBuffer, projectBuffer);
				AssertRelatedBuffers(network["Workflow2"], subDiagramBuffer, projectBuffer);
				AssertRelatedBuffers(network["Workflow3"], subDiagramBuffer, projectBuffer);
				AssertRelatedBuffers(network["Workflow4"], subDiagramBuffer, projectBuffer);
				AssertRelatedBuffers(network["Workflow5"], subDiagramBuffer, projectBuffer);
				AssertRelatedBuffers(network["Workflow Wot Enters Halfway"], subDiagramBuffer, projectBuffer);

				// Non-CC branch
				AssertRelatedBuffers(network["Non-CC Branch 1"], nonCCBranchBuffer2, nonCCBranchBuffer3, projectBuffer);
				AssertRelatedBuffers(network["Non-CC Branch 2"], nonCCBranchBuffer2, projectBuffer);
				AssertRelatedBuffers(network["Non-CC Branch 3"], nonCCBranchBuffer3, projectBuffer);
				AssertRelatedBuffers(network["Unapproved Workflow"], Array.Empty<IBuffer>());
			});
		}

		public void TestBufferPenetration_ForInterestingPermutations_ZoneZeroinessShouldFlowToPostreqBuffers()
		{
			var iterationBuffer = (BMNCNBufferShape)network["Iteration Buffer"];
			var subDiagramBuffer = (BMNCNBufferShape)network["Sub-diagram Feeding Buffer"];
			var ccBranchBuffer = (BMNCNBufferShape)network["CC Branch Workflow Feeding Buffer"];
			var nonCCBranchBuffer2 = (BMNCNBufferShape)network["Non-CC Branch 2 Feeding Buffer"];
			var nonCCBranchBuffer3 = (BMNCNBufferShape)network["Non-CC Branch 3 Feeding Buffer"];
			var projectBuffer = (BMNCNBufferShape)network["Project Buffer"];

			// CC and friends
			AssertBufferPenetration(network["HLD"], Tuple.Create(iterationBuffer, 2.5m), Tuple.Create(ccBranchBuffer, 5.0m), Tuple.Create(subDiagramBuffer, 2.0m), Tuple.Create(projectBuffer, 0.75m));
			AssertBufferPenetration(network["Design"], Tuple.Create(iterationBuffer, 2.0m), Tuple.Create(ccBranchBuffer, 4.0m), Tuple.Create(subDiagramBuffer, 1.5m), Tuple.Create(projectBuffer, 0.5m));
			AssertBufferPenetration(network["Unbuffered Workflow"], Tuple.Create(iterationBuffer, 2.3333m), Tuple.Create(ccBranchBuffer, 4.6667m), Tuple.Create(subDiagramBuffer, 1.8333m), Tuple.Create(projectBuffer, 1.1667m));
			AssertBufferPenetration(network["Coding"], Tuple.Create(projectBuffer, 0.1667m));
			AssertBufferPenetration(network["Review"], Tuple.Create(projectBuffer, 0.0m));
			AssertBufferPenetration(network["Resource Conflict Workflow"], Tuple.Create(projectBuffer, 0.0m));
			AssertBufferPenetration(network["Publish"], Tuple.Create(projectBuffer, 0.0m));

			// Branch from the CC
			AssertBufferPenetration(network["CC Branch Workflow"], Tuple.Create(ccBranchBuffer, 3.3333m), Tuple.Create(subDiagramBuffer, 1.1667m), Tuple.Create(projectBuffer, 0.0833m));
			AssertBufferPenetration(network["Sub-diagram"], Tuple.Create(subDiagramBuffer, 0.3333m), Tuple.Create(projectBuffer, 0.0m));
			AssertBufferPenetration(network["Workflow1"], Tuple.Create(subDiagramBuffer, 0.5m), Tuple.Create(projectBuffer, 0.0m));
			AssertBufferPenetration(network["Workflow2"], Tuple.Create(subDiagramBuffer, 0.3333m), Tuple.Create(projectBuffer, 0.0m));
			AssertBufferPenetration(network["Workflow3"], Tuple.Create(subDiagramBuffer, 0.1667m), Tuple.Create(projectBuffer, 0.0m));
			AssertBufferPenetration(network["Workflow Wot Enters Halfway"], Tuple.Create(subDiagramBuffer, 0.6667m), Tuple.Create(projectBuffer, 0.0m));

			// Non-CC branch
			AssertBufferPenetration(network["Non-CC Branch 1"], Tuple.Create(nonCCBranchBuffer2, 0.5385m), Tuple.Create(nonCCBranchBuffer3, 1.4m), Tuple.Create(projectBuffer, 0.1667m));
		}

		public void TestNoErrors()
		{
			AssertNoErrors("Very complex network should be error-free", network.DiagramEntity);
			AssertNoWarnings("Very complex network should be warning-free", network.DiagramEntity);
		}

		public void TestEntityDurations()
		{
			CombineAssertions(() =>
			{
				AssertDurationInDays(network["HLD"], 3);
				AssertDurationInDays(network["Design"], 2);
				AssertDurationInDays(network["Unbuffered Workflow"], 2);
				AssertDurationInDays(network["Coding"], 9);
				AssertDurationInDays(network["Review"], 2);
				AssertDurationInDays(network["Resource Conflict Workflow"], 3);
				AssertDurationInDays(network["Publish"], 6);

				// Branch from the CC
				AssertDurationInDays(network["CC Branch Workflow"], 4);
				AssertDurationInDays(network["Sub-diagram"], 6);
				AssertDurationInDays(network["Workflow1"], 1);
				AssertDurationInDays(network["Workflow2"], 1);
				AssertDurationInDays(network["Workflow3"], 1);
				AssertDurationInDays(network["Workflow4"], 1);
				AssertDurationInDays(network["Workflow5"], 1);
				AssertDurationInDays(network["Workflow Wot Enters Halfway"], 3);

				// Non-CC branch
				AssertDurationInDays(network["Non-CC Branch 1"], 3);
				AssertDurationInDays(network["Non-CC Branch 2"], 5);
				AssertDurationInDays(network["Non-CC Branch 3"], 4);
				AssertDurationInDays(network["Unapproved Workflow"], 4);
			});
		}

		#endregion

		#region Performance

		[TestDate(2017, 10, 11)]
		public void TestDbHits_SuggestBuffers()
		{
			var cleanFactory = Factory.CreateNewFactory();

			var diagram = cleanFactory.Load<BMNCNShape>(network.DiagramEntity.PK);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);

			var hits = new Dictionary<string, int>
			{
				{ BMNCNAttachmentSchema.Constants.TableName, 2 },
				{ BMNCNChannelSchema.Constants.TableName, 1 },
				{ BMNCNScheduleSchema.Constants.TableName, 1 },
				{ BMNCNShapeSchema.Constants.TableName, 3 }, // One for the root diagram, one for all its descendants (via BNS_BNS_RootShape) and another to pre-fetch shown shapes' children so that ChildShapes collections can be used in various places without extra db hits.
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
			};

			AssertDbHits(hits, cleanFactory);

			networkViewModel.SuggestAndAcceptAllBuffers();

			hits.Add(GlbWorkTimeSchema.Constants.TableName, 1);

			AssertDbHits(hits, cleanFactory, ignoreHitsFromTablesCachedInUberFactory: true);
		}

		#endregion

		#region Setup

		IJobNetwork network;

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			network = CreateVeryComplexNetwork(Factory);
			Factory.Save();
		}

		#endregion
	}

	class VeryComplexNetworkPerformanceTest : NetworkTestCase
	{
		[TestDate(2017, 10, 11)]
		public void TestDbHits_Save()
		{
			var cleanFactory = Factory.CreateNewFactory();
			var network = CreateVeryComplexNetwork(cleanFactory);

			cleanFactory.ResetDatabaseLoadCount();
			cleanFactory.Save();

			AssertMaxDbHits(new Dictionary<string, int>
			{
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ GlbGroupLinkSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 2 },  // 2 hits because of Custom Fields added to GlbGroup
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ StmALogSchema.Constants.TableName, 1 }, // +1 ProcessHeaderLogger.AddStartabilityLog > MostRecentLogByEventTime
			}, cleanFactory);
		}

		[TestDate(2017, 10, 11)]
		public void TestDbHits_Validation()
		{
			var network = CreateVeryComplexNetwork(Factory);
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(network.DiagramShape.PK);

			newFactory.ResetDatabaseLoadCount();
			loadedDiagram.RunPreSaveValidation();

			AssertDbHits(new Dictionary<string, int>
			{
				{ BMNCNScheduleSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 },
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 1 },
			}, newFactory);
		}
	}
}
