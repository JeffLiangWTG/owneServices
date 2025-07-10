using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Pipes;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class PartialRefreshPipeEngine : PipeEngine
	{
		public static PartialRefreshPipeEngine Create(BMBoardSectionViewModel viewModel, WorkflowUpdatedOperation context)
		{
			var options = BMSRegistry.Instance.BoardOnSecondaryServer.Value ? EngineConfigOptions.DisableAsync : EngineConfigOptions.None;
			var engine = new PartialRefreshPipeEngine(options, viewModel, context);

			engine.Initialise();

			return engine;
		}

		protected PartialRefreshPipeEngine(EngineConfigOptions engineConfigOptions, BMBoardSectionViewModel viewModel, WorkflowUpdatedOperation context)
			: base(engineConfigOptions, id: PipeEngineHelper.GetID())
		{
			this.viewModel = viewModel;
			this.context = context;

			initialAllocationMap = viewModel.ComponentGrid.CardAllocationMap;
		}

		readonly BMBoardSectionViewModel viewModel;
		readonly WorkflowUpdatedOperation context;
		readonly CardAllocationMap initialAllocationMap;

		#region Events

		public event EventHandler DisplayingUpdatedTickets;

		#endregion

		#region Initialization

		protected virtual BMBoardSection LoadSectionCore()
		{
			var factory = viewModel.FactoryProvider.GetNewBackgroundThreadLoaderFactory(nameof(PartialRefreshPipeEngine) + ": " + viewModel.SectionName);
			var section = factory.Load<BMBoardSection>(viewModel.SectionPK)
				// This section has been deleted, but the board has not yet done a full refresh. Force it to reload now so places that need the BMBoardSection don't throw NullReferenceExceptions.
				?? throw new BoardMustReloadException();

			section.Factory.RelinquishThreadOwnership();

			return section;
		}

		protected void Initialise()
		{
			var section = LoadSectionCore();

			// Synchronous setup actions
			this.AddSync(ShowInitialLoadingIndicator).SetName(nameof(ShowInitialLoadingIndicator));
			var sectionPipe = this.AddSync(() => section).SetName(nameof(section)).ThreadSentryHandover(s => s.Factory);

			// Async actions
			var dataTransformPipe = this.AddAsync(GetModifiedCellsAndRefreshChannels, sectionPipe).SetName(nameof(GetModifiedCellsAndRefreshChannels)).ThreadSentryHandover(x => section.Factory);

			// For subscriptions
			var beforeDisplayingUpdatedTicketsPipe = this.AddSync(BeforeDisplayingUpdatedTickets, dataTransformPipe).SetName(nameof(BeforeDisplayingUpdatedTickets));

			// Re-entrant synchronous actions
			this.AddPipeEnd(DisplayUpdatedTickets, dataTransformPipe, beforeDisplayingUpdatedTicketsPipe);
		}

		#endregion

		#region Pipe Definitions

		object ShowInitialLoadingIndicator()
		{
			viewModel.NotifyLoadingIndicatorNeedsUpdating(LoadingIndicatorEventArgs.ForExistingLayoutRefresh(BoardRefreshType.Partial));

			return null; // Because PipeEngine needs funcs not actions
		}

		Tuple<CardAllocationMap, HashSet<CellContent>> GetModifiedCellsAndRefreshChannels(BMBoardSection section)
		{
			if (section.SectionConfiguration == null)
			{
				var message = Res.GetString("0370a4f1-f5c8-4c5a-9e19-3a5a6b44acb6", "Configuration could not be found for this section.");
				return Tuple.Create(CardAllocationMap.Failure(message), new HashSet<CellContent>());
			}

			try
			{
				var channels = viewModel.AllChannels.Any() ? viewModel.AllChannels : new[] { new UnchanneledChannel(ChannelTypeList.Codes.NotChanneled) };
				var entities = LoadTasksAndOrWorkflowsToRefresh(section);
				var taskChannelMap = TaskChannelMap.Create(section, channels, entities);

				BMBoardSectionViewModel.CreateAndPopulatePropertyCache(taskChannelMap, viewModel, context);

				var newCardAllocationMap = CardAllocationMap.NewAllocationMap(section, viewModel, taskChannelMap);
				var modifiedData = GetModifiedData(newCardAllocationMap, section);

				RefreshChannels(modifiedData.Item2);

				return modifiedData;
			}
			catch (SqlException ex) when (ex.Number == 8623) // Query too complex
			{
				var message = Res.GetString("246a3b38-42dd-4e59-8818-02010d24a8c7", "The query for this section was rejected by the server due to excessive complexity.");
				return Tuple.Create(CardAllocationMap.Failure(message, ex), new HashSet<CellContent>());
			}
		}

		Tuple<CardAllocationMap, HashSet<CellContent>> GetModifiedData(CardAllocationMap newCardAllocationMap, BMBoardSection section)
		{
			if (initialAllocationMap == null)
			{
				return Tuple.Create(newCardAllocationMap, newCardAllocationMap.GetCells().ToHashSet());
			}

			if (newCardAllocationMap.FailureDetails == null)
			{
				var relatedEntities = context.GetRelatedEntityPKs(viewModel, section.Factory);

				return initialAllocationMap.MergeIntoAndFindCellsToRefresh(newCardAllocationMap, relatedEntities, context.Cells);
			}

			return Tuple.Create(newCardAllocationMap, new HashSet<CellContent>());
		}

		object BeforeDisplayingUpdatedTickets(Tuple<CardAllocationMap, HashSet<CellContent>> modifiedData)
		{
			DisplayingUpdatedTickets?.Invoke(this, EventArgs.Empty);

			return null; // Because PipeEngine needs funcs not actions
		}

		void DisplayUpdatedTickets(Tuple<CardAllocationMap, HashSet<CellContent>> modifiedData, object beforeDisplayingUpdatedTicketsNullResultBecausePipeEngineRequiresIt)
		{
			var mergedCardAllocationMap = modifiedData.Item1;
			var modifiedCells = modifiedData.Item2;

			if (mergedCardAllocationMap.FailureDetails != null)
			{
				viewModel.ComponentGrid.ShowLoadFailure(mergedCardAllocationMap.FailureDetails);
				return;
			}

			if (modifiedCells.Count > 0)
			{
				var message = Res.GetString("a7fb04e6-8312-41d7-8a7b-a192646c375a", "Updating tickets...");

				viewModel.NotifyLoadingIndicatorNeedsUpdating(LoadingIndicatorEventArgs.ForSpecificMessage(message, BoardRefreshType.Partial));
			}

			viewModel.ComponentGrid.ShowAllocatedTasks(context.MainThreadFactory, viewModel, mergedCardAllocationMap, modifiedCells);
			viewModel.NotifyLoadingIndicatorNeedsUpdating(LoadingIndicatorEventArgs.ForRemovingShownMessage(BoardRefreshType.Partial));
		}

		#endregion

		#region Refresh Implementation

		protected BoardSectionEntities LoadTasksAndOrWorkflowsToRefresh(BMBoardSection section)
		{
			ProcessTask[] matchingTasks;
			ProcessHeader[] matchingWorkflows;

			if (section.SectionConfiguration == null)
			{
				throw new InvalidOperationException("Invalid Section, SectionConfiguration is null: " + section.HumanReadableName);
			}

			var workflowFilter = viewModel.OverriddenWorkflowSectionFilter ?? section.WorkflowSectionFilter;
			var taskFilter = viewModel.OverriddenTaskSectionFilter ?? section.TaskSectionFilter;

			switch (section.SectionConfiguration.CardType)
			{
				case CardTypeList.Codes.Task:
					matchingTasks = WorkflowLoader.LoadTasks(section, viewModel.BMBoardChannels, workflowFilter, taskFilter, context.GetRelatedTaskPKs(section.Factory));
					var workflowPKs = matchingTasks.Select(t => t.P9_FH_ProcessHeader).Where(p => p.IsValid).Distinct();
					matchingWorkflows = section.Factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, workflowPKs));
					break;
				case CardTypeList.Codes.Workflow:
					if (section.SectionConfiguration.IsReleaseScheduler)
					{
						matchingWorkflows = WorkflowLoader.LoadReleaseSchedulerWorkflows(section, viewModel.BMBoardChannels, workflowFilter, taskFilter, context.GetRelatedWorkflowPKs(section.Factory));
					}
					else
					{
						matchingWorkflows = WorkflowLoader.LoadWorkflows(section, viewModel.BMBoardChannels, workflowFilter, taskFilter, context.GetRelatedWorkflowPKs(section.Factory));
					}
					matchingTasks = WorkflowLoader.LoadTasksForWorkflows(section, matchingWorkflows);
					break;
				case CardTypeList.Codes.JobLevelWorkflow:
					matchingWorkflows = WorkflowLoader.LoadJobLevelWorkflows(section, viewModel.BMBoardChannels, workflowFilter, taskFilter);
					matchingTasks = WorkflowLoader.LoadTasksForWorkflows(section, matchingWorkflows);
					break;
				default:
					throw new InvalidOperationException("Invalid CardType: " + section.SectionConfiguration.CardType);
			}

			return BoardSectionEntities.ForPartialRefresh(matchingTasks, matchingWorkflows);
		}

		void RefreshChannels(HashSet<CellContent> modifiedCells)
		{
			if (!context.IsSavingDetailedTicket)
			{
				var channelsAffectedByRefresh = modifiedCells.SelectMany(c => c.Channel.WrapWithEnumerable().Append(c.SecondaryChannel)).Distinct().OfType<VisualBoardChannel>();

				foreach (var channel in channelsAffectedByRefresh)
				{
					channel.ClearCacheAndReload();
				}
			}
		}

		#endregion
	}
}
