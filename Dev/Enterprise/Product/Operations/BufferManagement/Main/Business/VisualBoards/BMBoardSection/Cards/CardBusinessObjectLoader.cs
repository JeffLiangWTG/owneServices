using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public static class CardBusinessObjectLoader
	{
		public static CardLoadResult Load(ICardContent content, BMBoardSectionViewModel viewModel, BusinessObjectFactory factoryOverride = null)
		{
			var factory = factoryOverride ?? new BusinessObjectFactory { NameForDebugging = "CardBusinessObjectLoader" };

			var result = CardLoadFailureType.None;

			var task = content.GetTask(factory);
			if (task != null && task.ProcessHeader != null)
			{
				task.ProcessHeader.OverrideContextProvider = viewModel.Section;
			}

			var workflow = content.GetWorkflow(factory);
			var bizoContent = content as IBizoCardContent;

			if ((bizoContent != null && bizoContent.Workflow != null && bizoContent.Workflow.IsDeleted) || workflow == null)
			{
				result = CardLoadFailureType.WorkflowDeleted;
			}
			else if ((bizoContent != null && bizoContent.Task != null && bizoContent.Task.IsDeleted) || task == null)
			{
				result = TryLoadReplacementTaskForWorkflowCard(content, viewModel, CardLoadFailureType.TaskDeleted, out task, workflow);
			}
			else if (task.P9_FH_ProcessHeader != content.WorkflowIdentifier)
			{
				result = TryLoadReplacementTaskForWorkflowCard(content, viewModel, CardLoadFailureType.TaskReassigned, out task, workflow);
			}

			return new CardLoadResult(factory, task, workflow, result);
		}

		static CardLoadFailureType TryLoadReplacementTaskForWorkflowCard(ICardContent content, BMBoardSectionViewModel viewModel, CardLoadFailureType failureResult, out ProcessTask task, ProcessHeader workflow)
		{
			if (content.CardType == CardType.Workflow)
			{
				// Try find another task before we give up on this workflow card.
				task = GetBestTask(workflow, viewModel, false);

				if (task == null)
				{
					return CardLoadFailureType.NoEligibleTasks;
				}
				else
				{
					return CardLoadFailureType.None;
				}
			}
			else
			{
				task = null;
				return failureResult;
			}
		}

		static ProcessTask GetBestTask(ProcessHeader workflow, BMBoardSectionViewModel viewModel, bool throwOnFailure = true)
		{
			var taskCollection = workflow.GetTasksWithoutAccessingWorkflowParent();
			var bestTask = taskCollection.FirstOrDefault(t => t.IsOpen && TaskBelongsInSection(t, workflow, viewModel)) ?? taskCollection.FirstOrDefault(t => TaskBelongsInSection(t, workflow, viewModel));

			if (throwOnFailure && bestTask == null)
			{
				throw new ArgumentException("The workflow should contain at least one task in the collection not accessing workflow parent.", nameof(workflow)); // It's a message for developers.
			}

			return bestTask;
		}

		static bool TaskBelongsInSection(ProcessTask task, ProcessHeader workflow, BMBoardSectionViewModel viewModel)
		{
			return workflow.IsWorkflow || task.ProcessHeader != null && viewModel.AllShownComponentPKs.Contains(task.ProcessHeader.FH_FC_CurrentComponent);
		}
	}

	public enum CardLoadFailureType
	{
		None = 0,
		TaskDeleted,
		WorkflowDeleted,
		TaskReassigned,
		NoEligibleTasks,
	}

	public class CardLoadResult
	{
		internal CardLoadResult(BusinessObjectFactory factory, ProcessTask task, ProcessHeader workflow, CardLoadFailureType failureType)
		{
			succeeded = failureType == CardLoadFailureType.None;
			this.failureType = failureType;
			this.factory = factory;
			this.workflow = workflow;
			this.task = task;
		}

		#region Fields

		readonly BusinessObjectFactory factory;
		readonly bool succeeded;
		readonly CardLoadFailureType failureType;
		readonly ProcessHeader workflow;
		readonly ProcessTask task;

		#endregion

		#region Properties

		public BusinessObjectFactory Factory
		{
			get { return factory; }
		}

		public ProcessHeader Workflow
		{
			get { return workflow; }
		}

		public ProcessTask Task
		{
			get { return task; }
		}

		public bool LoadSucceeded
		{
			get { return succeeded; }
		}

		public CardLoadFailureType FailureType
		{
			get { return failureType; }
		}

		public ZString FailureMessage
		{
			get { return GetCardFailureMessage(FailureType); }
		}

		#endregion

		static ZString GetCardFailureMessage(CardLoadFailureType failureType)
		{
			switch (failureType)
			{
				case CardLoadFailureType.None:
					return ZString.Empty;

				case CardLoadFailureType.WorkflowDeleted:
					return Res.GetString("1087f123-8d02-49ca-9493-f5511dea0f42", "This card's workflow has been deleted and will now be removed.");

				case CardLoadFailureType.TaskDeleted:
					return Res.GetString("2cca3a5c-6a56-49ad-b8c3-c08c11aefacc", "This card's task has been deleted and will now be removed.");

				case CardLoadFailureType.TaskReassigned:
					return Res.GetString("91bb729b-368d-47a0-a10a-efbccba5f865", "This card's task has been assigned to a different workflow and will now be removed.");

				case CardLoadFailureType.NoEligibleTasks:
					return Res.GetString("d9149e81-3656-4a11-99b9-4c8a7e33b204", "This workflow no longer has tasks. Workflows without tasks cannot be displayed.");

				default:
					{
						ErrorReporter.ReportOnce("caff44e2-a81a-4dba-9465-a4b3344fa66b", string.Format(CultureInfo.InvariantCulture, "Card load failed for unrecognised reason [{0}]. Please add specific user message.", failureType));
						return Res.GetString("d5956024-6e7c-48b0-9599-d2786211a2d0", "This card could not be found and will now be removed.");
					}
			}
		}
	}
}
