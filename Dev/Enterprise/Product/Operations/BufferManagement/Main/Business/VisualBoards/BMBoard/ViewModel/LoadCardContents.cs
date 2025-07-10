using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.Core;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Telemetry;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.BufferManagement.Business
{
	public static class LoadCardContents
	{
		static string FailedSectionLoadString => Res.GetString("AE26F2A4-BF8E-4928-904C-7C6A9B14B286", "This section cannot be displayed.");

		public static TaskChannelMapWrapper MakeTaskChannelMap(BusinessObjectFactory factory, BMBoardSectionViewModel viewModel)
		{
			using var activity = TelemetryService.ActivitySource.StartActivity($"{nameof(LoadCardContents)}.{nameof(MakeTaskChannelMap)}");
			var section = BMBoardSection.Load(factory, viewModel.SectionPK);

			if (section == null)
			{
				return new TaskChannelMapWrapper(new LoadFailureDetails(FailedSectionLoadString));
			}

			if (!viewModel.IsValid(section))
			{
				return new TaskChannelMapWrapper(new LoadFailureDetails(Res.GetString("5E2B66D2-BE26-4925-8BB1-E7209D6A83B6", "This section ({0}) has validation errors and cannot be shown. Please open the board configuration form and fix any validation errors.", viewModel.SectionName)));
			}

			if (section.Component != null)
			{
				if (!section.Component.FC_IsActive)
				{
					return new TaskChannelMapWrapper(new LoadFailureDetails(Res.GetString("e409a277-a6b0-4f42-bf40-bb42616b92eb", "The component [{0}] is inactive and cannot be displayed on a board section.", section.Component.FC_Name)));
				}
			}
			else
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Attempted to MakeTaskChannelMap but the section's component was null. Section: [{0}] Board: [{1}]", section.SectionName, section.Board.HumanReadableName));
				return new TaskChannelMapWrapper(new LoadFailureDetails(FailedSectionLoadString));
			}

			var maxNumberOfTickets = BMSRegistry.Instance.MaxNumberOfItemsOnBoards.Value;

			TaskChannelMap taskChannelMap = null;

			try
			{
				var dataSource = new BoardSectionDataSource(section, viewModel);
				if (viewModel.CellsPerSubsection == 0)
				{
					taskChannelMap = TaskChannelMap.Empty;
				}
				else if (viewModel.PrimaryChannels.Any())
				{
					taskChannelMap = dataSource.GetIncompleteTasksForCurrentChannels();
				}
				else
				{
					taskChannelMap = dataSource.GetIncompleteTasksForUnchannelled();
				}

				var query = string.Format((NoResString)"WorkflowFilter: [{0}], TaskFilter: [{1}]", dataSource.Parameters.WorkflowSectionFilter.LiteralTextSqlFormatted, dataSource.Parameters.TaskSectionFilter.LiteralTextSqlFormatted);
				ObjectFactory.Get<ISearchPerformedUsageCollector>()?.Report(SearchPerformedType.PaveSql, section.Board.HumanReadableName, query);
			}
			catch (SqlException ex) when (ex.Number == 8623)
			{
				var loadFailureDetails = new LoadFailureDetails(Res.GetString("6f69d1c1-c923-4477-bf50-eefd90e21a11", "This section could not be loaded because the generated filter exceeded the database query complexity limit. Please try adjusting the section's workflow filters and task filters."), ex);
				return new TaskChannelMapWrapper(loadFailureDetails);
			}
			catch (SqlException ex) when (ex.IsTimeoutExpired())
			{
				var loadFailureDetails = new LoadFailureDetails(Res.GetString("e4a187b5-50bc-4af4-9567-f898e25417f7", "This section could not be loaded because it took too long to get its data from the database. Please try adjusting the section's workflow filters and task filters."), ex);
				return new TaskChannelMapWrapper(loadFailureDetails);
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.GeneralNetworkError)
			{
				var loadFailureDetails = new LoadFailureDetails(new DbErrorMatch(ex).GetUserFriendlyMessage(Db.Connection));
				return new TaskChannelMapWrapper(loadFailureDetails);
			}

			var numberOfTicketsToShow = CalculateNumberOfTicketsToShow(viewModel, taskChannelMap);

			if (taskChannelMap.AllWorkflows == null || numberOfTicketsToShow > maxNumberOfTickets)
			{
				return new TaskChannelMapWrapper(new LoadFailureDetails(Res.GetString("7aeb3a2a-b329-4f8d-8705-f5b340fafa42", "This section ({0}) cannot be displayed, as the number of items to be shown exceeds the maximum allowed ({1}). Please revise filters and configuration of this section.", viewModel.SectionName, maxNumberOfTickets)));
			}
			else
			{
				var channels = taskChannelMap?
					.AllChannels
					.Select(channel => (Code: channel.ChannelEntityCode.ToString(), Type: channel.EntityType, taskChannelMap.TasksByChannel[channel]?.Count))
					.ToArray();

				viewModel.SetDetailsForLog(numberOfTicketsToShow, channels);

				return new TaskChannelMapWrapper(taskChannelMap);
			}
		}

		static int CalculateNumberOfTicketsToShow(BMBoardSectionViewModel viewModel, TaskChannelMap tasks)
		{
			return viewModel.ShowWorkflowOrJobWorkflowCards
				? tasks.AllWorkflows.Count
				: tasks.AllTasks.Count;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static CardAllocationMapResult CreateAllocationMap(BusinessObjectFactory factory, BMBoardSectionViewModel viewModel, TaskChannelMapWrapper taskChannelMap)
		{
			using var activity = TelemetryService.ActivitySource.StartActivity($"{nameof(LoadCardContents)}.{nameof(CreateAllocationMap)}");
			var resultSnapshot = new CardAllocationMapResult();

			if (taskChannelMap.Failure != null)
			{
				resultSnapshot.Failure = taskChannelMap.Failure;
			}
			else
			{
				try
				{
					var tasks = taskChannelMap.Map;

					if (viewModel.CellsPerSubsection == 0 && tasks.AllTasks.Count > 0)
					{
						ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Tasks were loaded, but there's nowhere to display them. Section: [{0}] Board: [{1}]", viewModel.SectionName, viewModel.BoardViewModel.BoardName));
					}
					else
					{
						var section = factory.Load<BMBoardSection>(viewModel.SectionPK);

						if (section != null && section.SectionConfiguration != null)
						{
							var tuple = SetupTasks(section, tasks, viewModel);
							resultSnapshot.Cache = tuple.Item1;
							resultSnapshot.AllocationMap = tuple.Item2;

							if (resultSnapshot.AllocationMap.FailureDetails != null)
							{
								resultSnapshot.Failure = resultSnapshot.AllocationMap.FailureDetails;
							}
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					resultSnapshot.Failure = GetUnexpectedFailure(ex);
					ErrorReporter.ReportOnce(ex.Message, ex);
				}
			}

			return resultSnapshot;
		}

		public static LoadFailureDetails GetUnexpectedFailure(Exception ex)
		{
			return new LoadFailureDetails(Res.GetString("d7cd5673-8bc2-49ea-b55f-238d8fe3d716", "An unexpected error has occurred during the loading of this board section."), ex);
		}

		static Tuple<PropertyCache, CardAllocationMap> SetupTasks(BMBoardSection section, TaskChannelMap tasks, BMBoardSectionViewModel viewModel)
		{
			BMBoardSectionViewModel.CreateAndPopulatePropertyCache(tasks, viewModel);

			var cardAllocationMap = CardAllocationMap.NewAllocationMap(section, viewModel, tasks);

			return Tuple.Create(viewModel.Cache, cardAllocationMap);
		}
	}

	[ThreadSafe]
	public class TaskChannelMapWrapper
	{
		public TaskChannelMapWrapper(TaskChannelMap map)
		{
			Map = map;
		}

		public TaskChannelMapWrapper(LoadFailureDetails failureDetails)
		{
			Failure = failureDetails;
		}

		public TaskChannelMap Map { get; }
		public LoadFailureDetails Failure { get; }
	}

	public class LoadCardContentSetup
	{
		public LoadCardContentSetup(BMBoardSectionViewModel viewModel, Func<BusinessObjectFactory> getFactoryInstance = null)
		{
			ViewModel = viewModel;
			GetFactoryInstance = getFactoryInstance ?? new Func<BusinessObjectFactory>(() => CreateSetupTasksFactory(viewModel));
		}

		static BusinessObjectFactory CreateSetupTasksFactory(BMBoardSectionViewModel viewModel)
		{
			return viewModel.FactoryProvider.GetNewBackgroundThreadLoaderFactory(nameof(LoadCardContents) + (NoResString)"SetupTasks (" + viewModel.SectionName + (NoResString)")"); // It's a debug factory name
		}

		public BMBoardSectionViewModel ViewModel { get; }
		public Func<BusinessObjectFactory> GetFactoryInstance { get; }
	}

	[ThreadSafe]
	public class CardAllocationMapResult
	{
		public PropertyCache Cache { get; set; }
		public CardAllocationMap AllocationMap { get; set; }
		public LoadFailureDetails Failure { get; set; }
	}

	public class LoadFailureDetails
	{
		public LoadFailureDetails(string failureText, Exception failureException = null)
		{
			this.failureText = failureText;
			this.failureException = failureException;
		}

		readonly string failureText;
		readonly Exception failureException;

		public string FailureText
		{
			get { return failureText; }
		}

		public Exception FailureException
		{
			get { return failureException; }
		}
	}
}
