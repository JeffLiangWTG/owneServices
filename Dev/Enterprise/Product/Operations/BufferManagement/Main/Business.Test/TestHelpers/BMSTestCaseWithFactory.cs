using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.PAVE.Common.TestUtils;
using CargoWise.Pipes.Test;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	public abstract class BMSTestCaseWithFactory : VisualBoardsTestCase
	{
		#region Factory Helpers

		public static void AssertFactoryWasGarbageCollected(params string[] nameParts)
		{
			GC.Collect(); // We are testing against memory leaks.
			GC.WaitForFullGCComplete();
			var stats = new PerformanceStatistic();
			stats.Load();
			var original = ErrorReporter.SuppressReportingOfErrors;
			ErrorReporter.SuppressReportingOfErrors = true;
			try
			{
				foreach (var factoryNamePart in nameParts)
				{
					var factories = stats.FactoryStatistics.Cast<BusinessObjectFactoryStatistic>().Where(s => s.Name.Contains(factoryNamePart, StringComparison.InvariantCultureIgnoreCase)).ToArray();
					AssertEquals($"A factory containing [{factoryNamePart}] ought to have been garbage collected. Found {string.Join(System.Environment.NewLine, factories.Select(f => f.Name))}", 0, factories.Length);
				}
			}
			finally
			{
				ErrorReporter.SuppressReportingOfErrors = original;
			}
		}

		public static void AssertBusinessObjectsHeldInFactory(BusinessObjectFactory factory, params Tuple<Type, int>[] expectedNumberOfBizosByType)
		{
			AssertBusinessObjectsHeldInFactory(string.Empty, factory, expectedNumberOfBizosByType);
		}

		public static void AssertBusinessObjectsHeldInFactory(BusinessObjectFactory factory, bool compareTypesDirectly, params Tuple<Type, int>[] expectedNumberOfBizosByType)
		{
			AssertBusinessObjectsHeldInFactory(string.Empty, factory, compareTypesDirectly, expectedNumberOfBizosByType);
		}

		public static void AssertBusinessObjectsHeldInFactory(string message, BusinessObjectFactory factory, params Tuple<Type, int>[] expectedNumberOfBizosByType)
		{
			AssertBusinessObjectsHeldInFactory(message, factory, false, expectedNumberOfBizosByType);
		}

		public static void AssertBusinessObjectsHeldInFactory(string message, BusinessObjectFactory factory, bool compareTypesDirectly, params Tuple<Type, int>[] expectedNumberOfBizosByType)
		{
			CombineAssertions(message, () =>
			{
				var internals = (IBusinessObjectFactoryInternals)factory;

				foreach (var tuple in expectedNumberOfBizosByType)
				{
					AssertEquals(tuple.Item1.Name, tuple.Item2, internals.AllBusinessObjects.Count(b => MatchesType(b, tuple.Item1, compareTypesDirectly)));
				}
			});
		}

		static bool MatchesType(BusinessObject bizo, Type expectedType, bool compareTypesDirectly)
		{
			var actualType = bizo.GetType();

			if (compareTypesDirectly)
			{
				return actualType == expectedType;
			}
			else
			{
				return expectedType.IsAssignableFrom(actualType);
			}
		}

		public static bool IsAcceptabilityBandCalculationFactory(BusinessObjectFactory factory)
		{
			return factory.NameForDebugging.Contains("Acceptability");
		}

		#endregion

		#region Tag Rule

		public static void AssertRuleMatches(string message, TagRule rule, params ProcessHeader[] matches)
		{
			new DummyTagRuleRunner(new DummyLogger(), rule).Process();

			CombineAssertions(() =>
			{
				foreach (var workflow in matches)
				{
					if (rule.TGR_ActionType == TagRuleActionTypeList.Codes.AddTag)
					{
						AssertTagApplied(workflow, rule.TagTemplate.Magnitude);
					}
					else if (rule.TGR_ActionType == TagRuleActionTypeList.Codes.RemoveTag)
					{
						AssertTagNotApplied(workflow, rule.TagTemplate.Magnitude);
					}
				}
			});
		}

		public static void RunAllTagRules()
		{
			using (Env.Instance.TemporaryServiceTaskContext(TagServiceTask.Code, canRunInAnyBranch: true))
			{
				new TagRuleRunner(new DummyLogger()).Process();
			}
		}

		public static void RunTagRulesRegardlessOfLastRunTimeConsiderations(params TagRule[] tagRules)
		{
			using (Env.Instance.TemporaryServiceTaskContext(TagServiceTask.Code, canRunInAnyBranch: true))
			{
				new DummyTagRuleRunner(runRulesRegardlessOfLastRunTimeConsiderations: true, tagRulesToRun_ForTest: tagRules).Process();
			}
		}

		public static void RunTagRules(params TagRule[] tagRules)
		{
			using (Env.Instance.TemporaryServiceTaskContext(TagServiceTask.Code, canRunInAnyBranch: true))
			{
				new DummyTagRuleRunner(runRulesRegardlessOfLastRunTimeConsiderations: false, tagRulesToRun_ForTest: tagRules).Process();
			}
		}

		public static void RunCCPMAndNCNTagRules(BusinessObjectFactory factory)
		{
			var query = new ZQuery(TagRuleSchema.TGR_Name, SQLComparisonOperator.StartsWith, "CCPM ");
			query.AddToFilter(new ZQuery(TagRuleSchema.TGR_Name, SQLComparisonOperator.StartsWith, "NCN "), JoinCondition.Or);
			query.AddToFilter(TagRuleSchema.TGR_IsSystem, true);

			var rules = factory.Load<TagRule>(query);
			rules.ForEach(x =>
			{
				x.Schedule.Recurrence.TaskPeriod = ScheduleRecurrenceType.Second;
				x.Schedule.Recurrence.TaskPeriodCount = 30;
				x.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			});

			RunTagRules(rules);
		}

		#endregion

		#region Bitmaps

		public static void AssertBitmapNotEquals(string message, Bitmap image1, Bitmap image2)
		{
			AssertBitmapEquals(message, image1, image2, false);
		}

		public static void AssertBitmapEquals(string message, Bitmap image1, Bitmap image2)
		{
			AssertBitmapEquals(message, image1, image2, true);
		}

		static void AssertBitmapEquals(string message, Bitmap image1, Bitmap image2, bool isEqual)
		{
			byte[] image1Bytes;
			byte[] image2Bytes;

			using (var mstream = new MemoryStream())
			{
				image1.Save(mstream, ImageFormat.Bmp);
				image1Bytes = mstream.ToArray();
			}

			using (var mstream = new MemoryStream())
			{
				image2.Save(mstream, ImageFormat.Bmp);
				image2Bytes = mstream.ToArray();
			}

			var image164 = Convert.ToBase64String(image1Bytes);
			var image264 = Convert.ToBase64String(image2Bytes);

			var result = image164 == image264;
			AssertEquals(message, isEqual, result);
		}

		#endregion

		#region Component Grid

		public static void AssertTasksInCell<T>(BMBoardSection section, BMBoardSectionViewModel viewModel, T[] tasksOrWorkflowsInSection, params Tuple<int, int, T[]>[] tasksOrWorkflowsInCells)
			where T : BusinessObject, ITagable // Because workflows and tasks are both tagable...
		{
			AssertTasksInCell(string.Empty, section, viewModel, tasksOrWorkflowsInSection, tasksOrWorkflowsInCells);
		}

		public static void AssertTasksInCell<T>(string message, BMBoardSection section, BMBoardSectionViewModel viewModel, T[] tasksOrWorkflowsInSection, params Tuple<int, int, T[]>[] tasksOrWorkflowsInCells)
			where T : BusinessObject, ITagable // Because workflows and tasks are both tagable...
		{
			var factory = section.Factory;

			if (BMSRegistry.Instance.WorkflowManagementMode.Value == WorkflowManagementModes.Codes.PlanningManagement && factory.ServiceContainer.GetService<ApprovedShapeBufferPenetrationService>() == null)
			{
				var allWorkflows = tasksOrWorkflowsInSection.OfType<ProcessHeader>().Concat(tasksOrWorkflowsInSection.OfType<ProcessTask>().Select(t => t.GetProcessHeader())).ToArray();
				factory.ServiceContainer.AddService(new ApprovedShapeBufferPenetrationService(allWorkflows));
			}

			factory.Save(); // Ensure cache is cleared on save. We calculate buffer penetration when constructing things so that should be cleared before using it.

			var tasksOrWorkflowsAddedToCells = new Dictionary<Tuple<int, int>, T[]>();

			foreach (var cell in viewModel.ComponentGrid.Cells.Where(c => c.ContentType == CellContentType.Cards))
			{
				cell.ContentRefreshed += (s, e) => tasksOrWorkflowsAddedToCells.Add(
					Tuple.Create(cell.Row, cell.Column),
					viewModel.ComponentGrid.CardAllocationMap
						.GetCards(cell)
						.Select(c => viewModel.ShowWorkflowOrJobWorkflowCards ? (T)(BusinessObject)c.GetWorkflow(section.Factory) : (T)(BusinessObject)c.GetTask(section.Factory))
						.ToArray()
						);
			}

			var setup = new LoadCardContentSetup(viewModel, () => section.Factory);
			var mockDispatcher = new MockDispatcher();

			var workflows = viewModel.ShowWorkflowOrJobWorkflowCards
				? tasksOrWorkflowsInSection.Cast<ProcessHeader>()
				: tasksOrWorkflowsInSection.Cast<ProcessTask>().Select(t => t.GetProcessHeaderForCardType(section.SectionConfiguration.ShowJobWorkflowCards));

			var entities = BoardSectionEntities.ForTest(workflows.ToArray());
			var channels = viewModel.AllChannels.Any() ? viewModel.AllChannels : new[] { new UnchanneledChannel(ChannelTypeList.Codes.NotChanneled) };

			BoardSectionRefreshPipeEngine.Create(new BoardRefreshEventArgs(), setup, null, TaskChannelMap.Create(section, channels, entities)).ExecuteAll(mockDispatcher, mockDispatcher);
			mockDispatcher.DispatchAll();

			if (tasksOrWorkflowsAddedToCells.Count == 0)
			{
				Assert("No tasks were added to cells - there should have been", false);
			}
			else
			{
				foreach (var tuple in tasksOrWorkflowsInCells)
				{
					var key = Tuple.Create(tuple.Item1, tuple.Item2);
					var tasksOrWorkflowsAddedToCell = tasksOrWorkflowsAddedToCells.ContainsKey(key) ? tasksOrWorkflowsAddedToCells[key].Select(c => c.PK) : null;

					if (tasksOrWorkflowsAddedToCell == null || tuple.Item3.Select(c => c.PK).Any(g => !tasksOrWorkflowsAddedToCell.Contains(g)))
					{
						var expectedTimeIndex = viewModel.ComponentGrid[tuple.Item1, tuple.Item2].TimeIndex;
						var messageBuilder = new StringBuilder(string.Format(@"Cell at row={0} col={1} with TimeIndex={2} should have the following tasks:
{3}

They were allocated here instead:
", tuple.Item1, tuple.Item2, expectedTimeIndex, string.Join("\r\n", tuple.Item3.Select(t => t.Description))));

						foreach (var taskOrWorkflow in tuple.Item3)
						{
							var found = false;

							foreach (var kvp in tasksOrWorkflowsAddedToCells)
							{
								if (kvp.Value.Any(t => t.PK == taskOrWorkflow.PK))
								{
									var actualTimeIndex = viewModel.ComponentGrid[kvp.Key.Item1, kvp.Key.Item2].TimeIndex;
									messageBuilder.AppendFormat("\r\nRow={0} Col={1}, TimeIndex={2}, Task='{3}'", kvp.Key.Item1, kvp.Key.Item2, actualTimeIndex, taskOrWorkflow.Description);
									found = true;
									break;
								}
							}

							if (!found)
							{
								messageBuilder.AppendFormat("\r\nTask = '{0}' was Nowhere!!!", taskOrWorkflow.Description);
							}
						}

						messageBuilder.AppendLine();

						if (!string.IsNullOrEmpty(message))
						{
							messageBuilder.Append(message);
						}

						Fail(messageBuilder.ToString());
					}
					else
					{
						Assert("We allocated the tasks to the right cell. Awesome.", true);
					}
				}
			}

			if (!factory.ThreadSentry.IsOwner)
			{
				factory.ThreadSentry.TakeThreadOwnership();
			}
		}

		public static void AssertTasksInCell(Tuple<BMBoardSection, BMBoardSectionViewModel> pair, ProcessTask[] tasks, params Tuple<int, int, ProcessTask[]>[] tasksInCells)
		{
			AssertTasksInCell(pair.Item1, pair.Item2, tasks, tasksInCells);
		}

		#endregion

		#region Workflow

		public static IProcessTaskIterationLink CreateIterationLink(ProcessTask task, ProcessHeader iterationWorkflow, ProcessTask iterationTask = null, string linkType = IterationLinkTypeList.Codes.QualityIterationTask, string outcome = null)
		{
			var pivot = (IProcessTaskIterationLink)task.IterationLinks.AddNew();

			if (iterationWorkflow != null)
			{
				pivot.P9I_FH_IterationWorkflow = iterationWorkflow.PK;
			}

			if (iterationTask != null)
			{
				pivot.P9I_P9_IterationTask = iterationTask.PK;
			}

			pivot.P9I_LinkType = linkType;

			if (outcome != null)
			{
				pivot.P9I_Outcome = outcome;
			}

			return pivot;
		}

		public static ProcessHeader CreateQualityIteration(IProcessTask iterateFromTask, IProcessTask containmentBarrierTask, string iterationWorkflowDescription = "", string resourceUnderReviewStaffCode = null, string iterationReasonCode = null, bool shouldCreateWorkflowForIteration = true)
		{
			var workflow = ObjectFactory.Get<IWorkflowTestHelper>().CreateQualityIteration(containmentBarrierTask, iterateFromTask, resourceUnderReviewStaffCode, iterationReasonCode, shouldCreateWorkflowForIteration);

			if (shouldCreateWorkflowForIteration && !string.IsNullOrEmpty(iterationWorkflowDescription))
			{
				workflow.FH_CompletionStatement = iterationWorkflowDescription;
			}

			return (ProcessHeader)workflow;
		}

		public static void CreatePassedContainmentBarrierRecord(IProcessTask containmentBarrierTask, string resourceUnderReviewStaffCode = null)
		{
			ObjectFactory.Get<IWorkflowTestHelper>().CreatePassedContainmentBarrierRecord(containmentBarrierTask, resourceUnderReviewStaffCode);
		}

		public static StmALog AssertComponentChangedEventRaised(EnterpriseBusinessObject bizo, ComponentChangeMode mode, ZGuid fromComponentPK, ZGuid toComponentPK, ZGuid componentLinkPK = default, string status = WorkflowStatusList.Codes.Open, ConstraintStatus? constraintStatus = null, decimal? bufferPenetration = null, int? bufferZone = null, string deferReason = null)
		{
			return AssertComponentChangedEventRaised(string.Empty, bizo, mode, fromComponentPK, toComponentPK, componentLinkPK, status, constraintStatus, bufferPenetration, bufferZone, deferReason);
		}

		public static StmALog AssertComponentChangedEventRaised(string assertionMessage, EnterpriseBusinessObject bizo, ComponentChangeMode mode, ZGuid fromComponentPK, ZGuid toComponentPK, ZGuid componentLinkPK = default, string status = WorkflowStatusList.Codes.Open, ConstraintStatus? constraintStatus = null, decimal? bufferPenetration = null, int? bufferZone = null, string deferReason = null)
		{
			var reference = new StringBuilder();

			if (constraintStatus != null)
			{
				reference.Append("|CCR=");
				reference.Append(constraintStatus.Value.ToCode());
			}

			reference.AppendFormat($"|FRM={fromComponentPK}");

			if (componentLinkPK.IsValid)
			{
				reference.AppendFormat($"|LNK={componentLinkPK}");
			}

			reference.AppendFormat($"|MOD={mode.ToCode()}");

			if (bufferPenetration != null)
			{
				reference.AppendFormat("|PEN={0}", bufferPenetration.Value.ToString("0.00", CultureInfo.InvariantCulture));
			}

			if (deferReason != null)
			{
				reference.AppendFormat("|RES={0}", deferReason);
			}

			reference.AppendFormat("|STS={0}|TO={1}", status, toComponentPK);

			if (bufferZone != null)
			{
				reference.AppendFormat("|ZON={0}", bufferZone.Value);
			}

			return AssertEventRaised(assertionMessage, bizo, Events.WorkflowTransferredBetweenSystemComponentsCode, reference.ToString(), deferFiringWorkflow: true); // XFR logs should not fire within the main process since we don't want to slow down BMS/BMG service tasks etc.
		}

		public static StmALog AssertTagEventRaised(EnterpriseBusinessObject bizo, TagActionType actionType, TagMagnitude tag, TagRule addedByRule = null)
		{
			return AssertTagEventRaised(string.Empty, bizo, actionType, tag, addedByRule);
		}

		public static StmALog AssertTagEventRaised(string assertionMessage, EnterpriseBusinessObject bizo, TagActionType actionType, TagMagnitude tag, TagRule addedByRule = null)
		{
			var reference = new StringBuilder();

			reference.Append("|ACT=");
			reference.Append(actionType.ToCode());

			reference.Append("|GRP=");
			reference.Append(tag.Definition.TGD_Code);

			if (addedByRule != null)
			{
				reference.Append("|RUL=");
				reference.Append(addedByRule.PK);
			}

			reference.Append("|TAG=");
			reference.Append(tag.TGM_Code);

			var log = AssertEventRaised(assertionMessage, bizo, Events.TagWasAddedOrRemovedCode, reference.ToString(), deferFiringWorkflow: true); // TAG logs should not fire within the main process since we don't want to slow down TAG service task etc.
			Assert("SL_EventTimeUtc should have value", log.SL_EventTimeUtc.IsValid);
			var humanReadableReference = new StringBuilder();

			switch (actionType)
			{
				case TagActionType.AddTag:
					humanReadableReference.Append("Added tag ");
					break;

				case TagActionType.RemoveTag:
					humanReadableReference.Append("Removed tag ");
					break;

				case TagActionType.ModifyTag:
					humanReadableReference.Append("Modified tag ");
					break;
			}

			humanReadableReference.Append(tag.TGM_Code);
			humanReadableReference.Append(" from tag group ");
			humanReadableReference.Append(tag.Definition.TGD_Code);

			if (addedByRule != null)
			{
				humanReadableReference.AppendFormat(" by tag rule [{0}].", addedByRule.TGR_Name);
			}
			else
			{
				humanReadableReference.Append(".");
			}

			AssertEquals(humanReadableReference.ToString(), log.DisplayEventReference);

			return log;
		}

		public static StmALog AssertEventRaised(EnterpriseBusinessObject bizo, string eventCode, string reference = null, bool deferFiringWorkflow = false)
		{
			return AssertEventRaised(string.Empty, bizo, eventCode, reference, deferFiringWorkflow);
		}

		public static StmALog AssertEventRaised(string assertionMessage, EnterpriseBusinessObject bizo, string eventCode, string reference = null, bool deferFiringWorkflow = false)
		{
			return MasterFilesTestHelper.AssertEventRaised(assertionMessage, bizo, eventCode, reference, deferFiringWorkflow);
		}

		public static void AssertNoEventRaised(EnterpriseBusinessObject bizo, string eventCode)
		{
			MasterFilesTestHelper.AssertNoEventRaised(bizo, eventCode);
		}

		public static void AssertIsPrerequisite(ProcessHeader fromHeader, ProcessHeader toHeader)
		{
			AssertEquals($"Expected [{fromHeader.Description}] to be a pre-requisite of [{toHeader.Description}], and yet...", true, fromHeader.IsPrerequisiteOf(toHeader));
		}

		public static void AssertIsNotPrerequisite(ProcessHeader fromHeader, ProcessHeader toHeader)
		{
			AssertEquals($"Expected [{fromHeader.Description}] to NOT be a pre-requisite of [{toHeader.Description}], and yet...", false, fromHeader.IsPrerequisiteOf(toHeader));
		}

		public static void AssertIsParent(ProcessHeader childHeader, ProcessHeader parentHeader)
		{
			AssertIsParent(string.Empty, childHeader, parentHeader);
		}

		public static void AssertIsParent(string message, ProcessHeader childHeader, ProcessHeader parentHeader)
		{
			if (!string.IsNullOrEmpty(message))
			{
				message = message + System.Environment.NewLine;
			}

			message += $"Expected [{parentHeader.Description}] to be a parent of [{childHeader.Description}], and yet...";

			AssertEquals(message, true, parentHeader.IsParentOf(childHeader));
		}

		public static void AssertIsNotParent(ProcessHeader childHeader, ProcessHeader parentHeader)
		{
			AssertIsNotParent(string.Empty, childHeader, parentHeader);
		}

		public static void AssertIsNotParent(string message, ProcessHeader childHeader, ProcessHeader parentHeader)
		{
			if (!string.IsNullOrEmpty(message))
			{
				message = message + System.Environment.NewLine;
			}

			message += $"Expected [{parentHeader.Description}] to NOT be a parent of [{childHeader.Description}], and yet...";

			AssertEquals(message, false, parentHeader.IsParentOf(childHeader));
		}

		#endregion

		#region Integrated Buffers

		public static void AssertRelatedBuffers<T>(IBufferedItem buffered, params T[] expectedRelatedBuffers)
			where T : IBuffer
		{
			var message = string.Format("Related buffers for entity [{0}]", buffered.Name);
			AssertRelatedBuffers(message, buffered, expectedRelatedBuffers);
		}

		public static void AssertRelatedBuffers<T>(string message, IBufferedItem buffered, params T[] expectedRelatedBuffers)
			where T : IBuffer
		{
			var buffers = buffered.GetRelatedBuffers().ToArray();
			AssertNullOrEmpty(BufferAssertionHelper.GetRelatedBuffersErrors(buffered, expectedRelatedBuffers));
		}

		protected void AssertNonCachedBufferPenetration<T>(IBufferedItem buffered, params Tuple<T, decimal>[] bufferPenetrations)
			where T : IBuffer
		{
			AssertBufferPenetration(buffered, false, bufferPenetrations);
		}

		protected void AssertNonCachedBufferPenetration<T>(string message, IBufferedItem buffered, params Tuple<T, decimal>[] bufferPenetrations)
			where T : IBuffer
		{
			AssertBufferPenetration(message, buffered, false, bufferPenetrations);
		}

		protected void AssertBufferPenetration<T>(IBufferedItem buffered, params Tuple<T, decimal>[] bufferPenetrations)
			where T : IBuffer
		{
			AssertBufferPenetration(buffered, true, bufferPenetrations);
		}

		protected void AssertBufferPenetration<T>(string message, IBufferedItem buffered, params Tuple<T, decimal>[] bufferPenetrations)
			where T : IBuffer
		{
			AssertBufferPenetration(message, buffered, true, bufferPenetrations);
		}

		void AssertBufferPenetration<T>(IBufferedItem buffered, bool useCache, params Tuple<T, decimal>[] bufferPenetrations)
			where T : IBuffer
		{
			var message = string.Format("Buffer penetration for entity [{0}]", buffered.Name);
			AssertBufferPenetration(message, buffered, useCache, bufferPenetrations);
		}

		void AssertBufferPenetration<T>(string message, IBufferedItem buffered, bool useCache, params Tuple<T, decimal>[] bufferPenetrations)
			where T : IBuffer
		{
			var context = new WorkingTimeContextWithFactory(WorkingTimeContext.Create(Factory), Factory);
			var failureMessage = BufferAssertionHelper.GetBufferPenetrationErrors(buffered, context, ZDateTime.UtcNow.ToDateTime(), bufferPenetrations);

			AssertNullOrEmpty(failureMessage);
		}

		protected void AssertZone(int zone, IBufferedItem item, bool useCachedPenetration = false)
		{
			AssertZone(string.Format("Buffer zone for " + item.Name), zone, item, useCachedPenetration);
		}

		protected void AssertZone(string message, int zone, IBufferedItem item, bool useCachedPenetration = false)
		{
			AssertEquals(message, zone, ZoneCalculator.CalculateZone(item, WorkingTimeContext.Create(Factory), Factory, useCachedPenetration));
		}

		#endregion

		#region Capacity

		public static void AssertFullCapacity(GlbStaff resource, BMComponent buffer, decimal expectedFullCapacity, decimal? expectedFullCapacityForWorkInvolvingCCR = null)
		{
			AssertFullCapacity(string.Empty, resource, buffer, expectedFullCapacity, expectedFullCapacityForWorkInvolvingCCR);
		}

		public static void AssertFullCapacity(string message, GlbStaff resource, BMComponent buffer, decimal expectedFullCapacity, decimal? expectedFullCapacityForWorkInvolvingCCR = null)
		{
			var capacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, buffer);

			CombineAssertions(message, () =>
			{
				AssertEquals("FullCapacity", expectedFullCapacity, capacity.FullCapacity);

				if (expectedFullCapacityForWorkInvolvingCCR != null)
				{
					AssertEquals("FullCapacityForWorkInvolvingCCR", expectedFullCapacityForWorkInvolvingCCR.Value, capacity.FullCapacityForWorkInvolvingCCR);
				}
			});
		}

		public static void AssertAvailableCapacity(GlbStaff resource, BMComponent buffer, decimal expectedAvailableCapacity)
		{
			AssertAvailableCapacity(string.Empty, resource, buffer, expectedAvailableCapacity);
		}

		public static void AssertAvailableCapacity(string message, GlbStaff resource, BMComponent buffer, decimal expectedAvailableCapacity)
		{
			var capacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, buffer);

			AssertEquals(message, expectedAvailableCapacity, capacity.AvailableCapacity);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static void AssertReservedCapacityBreakdown(IResourceCapacity capacity, decimal zone3Hours = 0m, decimal zone2Hours = 0m, decimal zone1Hours = 0m, decimal zone0Hours = 0m)
			=> AssertReservedCapacityBreakdown(string.Empty, capacity, zone3Hours, zone2Hours, zone1Hours, zone0Hours);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static void AssertReservedCapacityBreakdown(string message, IResourceCapacity capacity, decimal zone3Hours = 0m, decimal zone2Hours = 0m, decimal zone1Hours = 0m, decimal zone0Hours = 0m)
			=> CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown(message, capacity, zone3Hours, zone2Hours, zone1Hours, zone0Hours);

		#endregion

		#region Release to Buffers

		public static void RunReleaseGateAndAssertReleaseOutcome(ProcessHeader workflow, BMComponent buffer, bool shouldBeReleasedToBuffer = true, string expectedLogMessage = null, string expectedSuccessfullyReleasedNotes = null, ReleaseGateFailureLogService_ForTest failureLogService = null)
		{
			RunReleaseGateAndAssertReleaseOutcome(string.Empty, workflow, buffer, shouldBeReleasedToBuffer, expectedLogMessage, expectedSuccessfullyReleasedNotes, failureLogService);
		}

		public static void RunReleaseGateAndAssertReleaseOutcome(string assertionMessage, ProcessHeader workflow, BMComponent buffer, bool shouldBeReleasedToBuffer = true, string expectedLogMessage = null, string expectedSuccessfullyReleasedNotes = null, ReleaseGateFailureLogService_ForTest failureLogService = null)
		{
			var startingComponent = workflow.CurrentComponent;
			var logger = new SimpleLogger();

			AssertNotEquals("Pre-condition: workflow should not already be released", buffer, startingComponent);

			ReleaseGateKeeperTest.RunReleaseGate(buffer.System, logger, failureLogService: failureLogService);

			workflow.Reload();

			CombineAssertions(assertionMessage, () =>
			{
				AssertEquals("Expected CurrentComponent", shouldBeReleasedToBuffer ? buffer : startingComponent, workflow.CurrentComponent);

				if (expectedLogMessage != null)
				{
					AssertMultilineASCIIEquals("Expected service task log message", expectedLogMessage.StripTaskIds(), logger.ToString().StripTaskIds());
				}

				if (expectedSuccessfullyReleasedNotes != null)
				{
					AssertMultilineASCIIEquals("Expected SuccessfulReleaseNotes", expectedSuccessfullyReleasedNotes.StripTaskIds(), workflow.GetSuccessfulReleaseNotes().StripTaskIds());
				}
			});
		}

		public static void AssertResourceCapacityReservationDetails(string staffCode, BMComponent buffer, string expectedCapacityReservationDetails)
		{
			AssertResourceCapacityReservationDetails(string.Empty, staffCode, buffer, expectedCapacityReservationDetails);
		}

		public static void AssertResourceCapacityReservationDetails(string assertionMessage, string staffCode, BMComponent buffer, string expectedCapacityReservationDetails)
		{
			var factory = new BusinessObjectFactory(); // Ensures we bypass cached data from previous calls to this assertion.
			buffer = factory.Load<BMComponent>(buffer.PK);

			var link = buffer.GetOrCreateResourceLink(staffCode);
			var reservationDetails = link?.CapacityReservationDetails.ToString();

			AssertMultilineASCIIEquals(assertionMessage, expectedCapacityReservationDetails, reservationDetails);
		}

		public static void AssertCanReleaseToBuffer(ProcessHeader workflow, BMComponent buffer, BufferReleaseOutcome expectedReleaseOutcome = BufferReleaseOutcome.Releasable, string expectedLogMessage = null, CapacityReservationTracker tracker = null)
		{
			AssertCanReleaseToBuffer(string.Empty, workflow, buffer, expectedReleaseOutcome, expectedLogMessage, tracker);
		}

		public static void AssertCanReleaseToBuffer(string message, ProcessHeader workflow, BMComponent buffer, BufferReleaseOutcome expectedReleaseOutcome = BufferReleaseOutcome.Releasable, string expectedLogMessage = null, CapacityReservationTracker tracker = null)
		{
			var report = GetBufferReservationReport(workflow, buffer, tracker);

			CombineAssertions(message, () =>
			{
				var actualLogMessage = Lazy.Create(() => report.CreateOutcomeLogMessage());
				var bufferReleaseOutcomeMessage = new StringBuilder("BufferReleaseOutcome");

				if (expectedLogMessage == null && expectedReleaseOutcome != report.BufferReleaseOutcome)
				{
					bufferReleaseOutcomeMessage.AppendLine();
					bufferReleaseOutcomeMessage.Append("Log message from the CapacityReservationReport:");
					bufferReleaseOutcomeMessage.AppendLine();
					bufferReleaseOutcomeMessage.AppendLine();

					bufferReleaseOutcomeMessage.Append(actualLogMessage.Value);
					bufferReleaseOutcomeMessage.AppendLine();
				}

				AssertEquals(bufferReleaseOutcomeMessage.ToString(), expectedReleaseOutcome, report.BufferReleaseOutcome);

				if (expectedLogMessage != null)
				{
					AssertMultilineASCIIEquals("LogMessage", expectedLogMessage.StripTaskIds(), actualLogMessage.Value.StripTaskIds());
				}
			});
		}

		public static CapacityReservationReport GetBufferReservationReport(ProcessHeader workflow, BMComponent buffer, CapacityReservationTracker tracker = null)
		{
			var newFactory = workflow.Factory.CreateNewFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			if (tracker == null)
			{
				tracker = CapacityReservationTrackerTest.Create_ForTest(buffer);
			}

			return tracker.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations(loadedWorkflow, newFactory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static void AssertCapacityBreakdown(string message, GlbStaff resource, BMComponent buffer, decimal fullCapacityHours, decimal availableCapacityHours, decimal availableCapacityHoursForCCRWork,
			decimal zone3ReservedHours = 0m, decimal zone2ReservedHours = 0m, decimal zone1ReservedHours = 0m, decimal zone0ReservedHours = 0m,
			decimal zone3AllocatedHours = 0m, decimal zone2AllocatedHours = 0m, decimal zone1AllocatedHours = 0m, decimal zone0AllocatedHours = 0m)
		{
			var breakdown = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, buffer);

			AssertCapacityBreakdown(message, breakdown, fullCapacityHours, availableCapacityHours, availableCapacityHoursForCCRWork,
				zone3ReservedHours, zone2ReservedHours, zone1ReservedHours, zone0ReservedHours,
				zone3AllocatedHours, zone2AllocatedHours, zone1AllocatedHours, zone0AllocatedHours);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static void AssertCapacityBreakdown(string message, IResourceCapacity capacity, decimal fullCapacityHours, decimal availableCapacityHours, decimal availableCapacityHoursForCCRWork,
			decimal zone3ReservedHours = 0m, decimal zone2ReservedHours = 0m, decimal zone1ReservedHours = 0m, decimal zone0ReservedHours = 0m,
			decimal zone3AllocatedHours = 0m, decimal zone2AllocatedHours = 0m, decimal zone1AllocatedHours = 0m, decimal zone0AllocatedHours = 0m)
		{
			CombineAssertions(message, () =>
			{
				AssertNotNull(capacity);

				if (capacity != null)
				{
					AssertEquals("Full capacity hours", fullCapacityHours, capacity.FullCapacity);
					AssertEquals("Available capacity hours", availableCapacityHours, capacity.AvailableCapacity);
					AssertEquals("Available capacity hours (for CCR work)", availableCapacityHoursForCCRWork, capacity.AvailableCapacityForWorkInvolvingCCR);

					AssertEquals("Zone 3 reserved hours", zone3ReservedHours, capacity.GetZoneReservedCapacity(3));
					AssertEquals("Zone 2 reserved hours", zone2ReservedHours, capacity.GetZoneReservedCapacity(2));
					AssertEquals("Zone 1 reserved hours", zone1ReservedHours, capacity.GetZoneReservedCapacity(1));
					AssertEquals("Zone 0 reserved hours", zone0ReservedHours, capacity.GetZoneReservedCapacity(0));

					AssertEquals("Zone 3 allocated hours", zone3AllocatedHours, capacity.GetZoneAllocatedCapacity(3));
					AssertEquals("Zone 2 allocated hours", zone2AllocatedHours, capacity.GetZoneAllocatedCapacity(2));
					AssertEquals("Zone 1 allocated hours", zone1AllocatedHours, capacity.GetZoneAllocatedCapacity(1));
					AssertEquals("Zone 0 allocated hours", zone0AllocatedHours, capacity.GetZoneAllocatedCapacity(0));
				}
			});
		}

		#endregion

		#region Acceptability Bands

		public static void AssertAcceptabilityBandResult(AcceptabilityBandResult result, ComponentAcceptabilityStatus status, bool resultFound, decimal? value)
		{
			AssertAcceptabilityBandResult(string.Empty, result, status, resultFound, value);
		}

		public static void AssertAcceptabilityBandResult(string message, AcceptabilityBandResult result, ComponentAcceptabilityStatus status, bool resultFound, decimal? value)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Status", status, result.Status);
				AssertEquals("ResultFound", resultFound, result.ResultFound);

				if (value == null)
				{
					AssertEquals("Value", value, result.Value);
				}
				else
				{
					AssertEquals("Value", Math.Round(value.Value, 4), Math.Round(result.Value.Value, 4)); // This is a unit test. Go away.
				}
			});
		}

		#endregion

		#region Tasks

		public static void IgnoreTaskTypesForProviderType(string workflowProviderType, params string[] taskTypesToIgnore)
		{
			var taskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypesForWorkflowType = taskTypes.AddNew();
			taskTypesForWorkflowType.Code = workflowProviderType;

			foreach (var type in taskTypesToIgnore)
			{
				var taskType = taskTypesForWorkflowType.TaskTypes.AddNew();
				taskType.Code = type;
				taskType.IsExcludedFromTransferRules = true;
			}

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, taskTypes);
		}

		#endregion

		#region Confirmation Messages

		public static void AssertNoConfirmationNotificationShown()
		{
			AssertNoConfirmationNotificationShown(string.Empty);
		}

		public static void AssertNoConfirmationNotificationShown(string assertionMessage)
		{
			var descriptor = UnitTestUserNotification.Instance.LastMessage.ConfirmationDialogDescriptor;

			if (descriptor != null)
			{
				var message = new StringBuilder(assertionMessage);
				message.Append("<br />No confirmation with notifications should have been shown, and yet...<br />");

				AddShownNotifications(message, descriptor);

				AssertionCount++;
				HtmlFail(message.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		public static void AssertConfirmationNotificationShown(NotificationTypes notificationType, string notificationText)
		{
			AssertConfirmationNotificationShown(null, notificationType, notificationText);
		}

		public static void AssertConfirmationNotificationShown(string assertionMessage, NotificationTypes notificationType, string notificationText)
		{
			var descriptor = UnitTestUserNotification.Instance.LastMessage.ConfirmationDialogDescriptor;

			AssertNotNull("A confirmation with notifications should have been shown, but none was. " + assertionMessage, descriptor);

			var matchingNotification = descriptor.ConfirmationNotifications.SingleOrDefault(n => n.Message.Contains(notificationText)); // There are things other than exceptions which have messages, you know?
			var message = new StringBuilder(assertionMessage);

			if (matchingNotification != null)
			{
				AssertEquals(message.ToString(), notificationType, matchingNotification.NotificationType);
			}
			else
			{
				message.Append("<br />A confirmation should have been shown with message:<br />");
				message.Append(HtmlFormatGoodValue(notificationText));

				if (descriptor.ConfirmationNotifications.Count > 0)
				{
					AddShownNotifications(message, descriptor);
				}
				else
				{
					message.AppendFormat("<br />No notifications were shown.");
				}

				AssertionCount++;
				HtmlFail(message.ToString());
			}
		}

		static void AddShownNotifications(StringBuilder message, ConfirmationDialogDescriptor descriptor)
		{
			message.Append("<br />The following notifications were shown:<br />");

			foreach (var notification in descriptor.ConfirmationNotifications)
			{
				message.Append("<br />");
				message.Append(HtmlFormatBadValue(notification.Message));
			}
		}

		#endregion

		#region Service Tasks

		public static void RunTransferRules(BMSystem system, ITransferRuleRunnerParams transferRuleRunnerParams = null)
		{
			using (DummySecondaryServerConnectionProvider.TemporarilyEnableDummyProvider())
			{
				var runner = new TestTransferRuleRunner(system, new DummyLogger(), transferRuleRunnerParams ?? new TransferRuleRunnerParams());

				runner.Process_ForTest();
			}
		}

		#endregion

		#region Emails

		public static void AssertEmailContent(EmailDef email, string expectedSubject, string expectedBody)
		{
			AssertEquals(expectedSubject, email.Subject);

			var body = XDocument.Parse(email.Body);
			var contentElement = (
				from element in body.Root.DescendantNodes().OfType<XElement>()
				where element.Name.LocalName == "td"
				from attribute in element.Attributes()
				where attribute.Name.LocalName == "class"
				where attribute.Value == "content"
				select element
				).FirstOrDefault();

			AssertNotNull(contentElement);
			AssertMultilineASCIIEquals("Email body", expectedBody, contentElement.Value);
		}

		#endregion

		#region Release Gate

		public static void SetFactoryReleaseLogFailureService(BusinessObjectFactory factory, ReleaseGateFailureLogService service) => factory.ServiceContainer.AddService(service, typeof(ReleaseGateFailureLogService));

		protected ReleaseGateFailureLogService_ForTest ReleaseLogFailureServiceForTest { get; private set; }

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.Instance.BoardShowTaskTags.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSTestHelper.DisableAcceptabilityBandResultCache();
			ReleaseLogFailureServiceForTest = new ReleaseGateFailureLogService_ForTest();
			SetFactoryReleaseLogFailureService(Factory, ReleaseLogFailureServiceForTest);

			disposables = new DisposableList(new[] { DummySecondaryServerConnectionProvider.TemporarilyEnableDummyProvider() });

			EnableBMSInRegistry();

			if (ShouldDisableAsyncBehaviour)
			{
				disposables.Add(DisableAsyncBehaviour());
			}
		}

		DisposableList disposables;

		protected virtual bool ShouldDisableAsyncBehaviour => false;

		#endregion

		#region TearDown

		protected override void TearDown()
		{
			base.TearDown();

			disposables.Dispose();

			BufferCapacityCache.Clear();
			TagProvider.ResetCCPMReadyToReleaseTagMagnitudePK_ForTest();
		}

		#endregion

		#region Client Assembly and Security Checkpoints

		public IDisposable OverrideClientAssemblyAndSecurityCheckpointsForTest(Clients clientId)
		{
			var security = Env.Instance.Security;
			security?.HookClientHookChanged();

			var disposeClientAssembly = ClientHookLoader.Instance.OverrideClientAssemblyForTest(clientId);

			return new DisposableAction(() =>
			{
				disposeClientAssembly.Dispose();
				security?.UnHookClientHookChanged();
			});
		}

		#endregion

		#region Tag Definitions and Magnitudes

		protected void BulkCreateTagDefinitionsAndMagnitudes(params ITagable[] tagables)
		{
			for (var i = 0; i < 100; i++)
			{
				var definition = BMSTestHelper.CreateTagDefinition(Factory, $"{i}", $"Definition {i}");
				for (var j = 0; j < 5; j++)
				{
					var magnitude = BMSTestHelper.CreateTagMagnitude(definition, $"{i}{j}", $"Magnitude {i}{j}");
					foreach (var tagable in tagables)
					{
						tagable.AddTag(magnitude);
					}
				}
			}
		}

		#endregion
	}
}
