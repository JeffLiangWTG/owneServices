using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentCargoWiseReopenEvent : SupportIncidentEvent
	{
		public SupportIncidentCargoWiseReopenEvent(SupportIncident incident)
			: base(incident)
		{
		}

		public override ZString Code
		{
			get { return IncidentEventFactory.Codes.CargoWiseReopen; }
		}

		protected override bool ApplyWorkflowTemplate()
		{
			bool copiedTasksFromTemplate = base.ApplyWorkflowTemplate();
			bool resumedClosedTasks = ProcessAwaitingClientTasks();
			return copiedTasksFromTemplate || resumedClosedTasks;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void ArrangeTasksSequence(IEnumerable<ProcessTask> existingTasks, IOrderedEnumerable<ProcessTask> orderedNewTasks)
		{
			if (orderedNewTasks.Any())
			{
				var query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, SupportIncidentProcessTask.AwaitingClientResponseLogReference);
				var sequenceOrderedExistingTasksAsArray = existingTasks.OrderBy(task => task.P9_Sequence).ToArray();

				// Get last closed task by Awaiting Response action
				ProcessTask lastClosedAwaitingResponseTask = sequenceOrderedExistingTasksAsArray.LastOrDefault(task => task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed && task.Logs.Find(query).Length > 0);

				// Get first cancelled task by Awaiting Response action and previous task
				ProcessTask firstCancelledAwaitingReponseTask = null;
				ProcessTask taskBeforeFirstCancelledAwaitingResponseTask = null;
				if (lastClosedAwaitingResponseTask == null && existingTasks.Any())
				{
					int indexOfFirstCancelledAwaitingReponseTask = Array.FindIndex(sequenceOrderedExistingTasksAsArray, task => task.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled && task.Logs.Find(query).Length > 0);
					if (indexOfFirstCancelledAwaitingReponseTask >= 0)
					{
						firstCancelledAwaitingReponseTask = sequenceOrderedExistingTasksAsArray[indexOfFirstCancelledAwaitingReponseTask];
					}
					if (indexOfFirstCancelledAwaitingReponseTask > 0)
					{
						taskBeforeFirstCancelledAwaitingResponseTask = sequenceOrderedExistingTasksAsArray[indexOfFirstCancelledAwaitingReponseTask - 1];
					}
				}

				// Set process header of new tasks
				IProcessHeader resumingPointProcessHeader = null;
				if (lastClosedAwaitingResponseTask != null)
				{
					resumingPointProcessHeader = lastClosedAwaitingResponseTask.ProcessHeader;
				}
				else if (firstCancelledAwaitingReponseTask != null)
				{
					resumingPointProcessHeader = firstCancelledAwaitingReponseTask.ProcessHeader;
				}
				else if (existingTasks.Any())
				{
					resumingPointProcessHeader = sequenceOrderedExistingTasksAsArray.Last().ProcessHeader;
				}

				if (resumingPointProcessHeader != null)
				{
					foreach (var task in orderedNewTasks.ToArray())
					{
						var processHeader = task.ProcessHeader;
						if (processHeader.FH_CompletionStatement.IsEmpty)
						{
							task.P9_FH_ProcessHeader = resumingPointProcessHeader.PK;
							if (!processHeader.Tasks.Any())
							{
								processHeader.Delete();
							}
						}
					}
				}

				// Calculate task sequence for inserting new tasks
				int resumingPointTaskSequence = 0;
				if (lastClosedAwaitingResponseTask != null)
				{
					resumingPointTaskSequence = lastClosedAwaitingResponseTask.P9_Sequence;
				}
				else if (taskBeforeFirstCancelledAwaitingResponseTask != null)
				{
					resumingPointTaskSequence = taskBeforeFirstCancelledAwaitingResponseTask.P9_Sequence;
				}
				else if (firstCancelledAwaitingReponseTask != null)
				{
					resumingPointTaskSequence = 0;
				}
				else if (existingTasks.Any())
				{
					resumingPointTaskSequence = sequenceOrderedExistingTasksAsArray.Last().P9_Sequence;
				}

				// Update sequence of new tasks
				int firstNewTaskSequence = orderedNewTasks.First().P9_Sequence;
				int newTaskSequenceIncrement = RoundNumberToPreviousTen(resumingPointTaskSequence) + Math.Min(10, firstNewTaskSequence) - firstNewTaskSequence;
				foreach (var task in orderedNewTasks.ToArray())
				{
					task.P9_Sequence += newTaskSequenceIncrement;
				}

				// Update sequence of existing tasks
				int existingTaskSequenceIncrement = RoundNumberToNextTen(orderedNewTasks.Last().P9_Sequence - resumingPointTaskSequence);
				foreach (var task in existingTasks.ToArray())
				{
					if (task.P9_Sequence > resumingPointTaskSequence)
					{
						task.P9_Sequence += existingTaskSequenceIncrement;
					}
				}
			}
		}

		static int RoundNumberToNextTen(int number)
		{
			return ((int)Math.Ceiling(number / 10.0)) * 10;
		}

		static int RoundNumberToPreviousTen(int number)
		{
			return ((int)Math.Floor(number / 10.0)) * 10;
		}

		bool ProcessAwaitingClientTasks()
		{
			bool result = false;

			var tasks = incident.WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>();
			var query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, SupportIncidentProcessTask.AwaitingClientResponseLogReference);
			var lastClosedTask = GetLastClosedTask();

			foreach (var task in tasks.ToArray())
			{
				var taskClosedAwaitingClientLogs = task.Logs.Find(query);
				if (taskClosedAwaitingClientLogs.Length > 0)
				{
					if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled &&
						(lastClosedTask == null || task.P9_Sequence > lastClosedTask.P9_Sequence))
					{
						SetCalculatedTaskStatus(task);
					}

					foreach (var log in taskClosedAwaitingClientLogs)
					{
						log.UpdateReference(log.SL_Reference + " Processed");
					}

					result = true;
				}
			}

			return result;
		}

		protected static void SetCalculatedTaskStatus(SupportIncidentProcessTask task)
		{
			if (!task.P9_GS_NKAssignedStaffMember.IsEmpty || !task.P9_G4_RequiredCapability.IsEmpty || !task.P9_GG_AssignedGroup.IsEmpty)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			}
			else
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			}
		}

		protected override void SetAssignee()
		{
			var firstNonClosedTask = GetFirstNonClosedTask();

			if (firstNonClosedTask != null)
			{
				var supportIncident = incident as SupportIncident;
				var lastClosedTask = GetLastClosedTask();

				if (lastClosedTask != null)
				{
					if (lastClosedTask.AssignedStaffMember != null && lastClosedTask.AssignedStaffMember.GS_IsActive)
					{
						firstNonClosedTask.P9_GS_NKAssignedStaffMember = lastClosedTask.P9_GS_NKAssignedStaffMember;
					}
					if (lastClosedTask.RequiredCapability != null && lastClosedTask.RequiredCapability.G4_IsActive)
					{
						firstNonClosedTask.P9_G4_RequiredCapability = lastClosedTask.P9_G4_RequiredCapability;
					}
				}

				if (firstNonClosedTask.P9_GS_NKAssignedStaffMember.IsEmpty && firstNonClosedTask.P9_G4_RequiredCapability.IsEmpty)
				{
					//If incident has no tasks previously, the last closed task must be null and original assigned staff may have value
					//Use original value here because copying tasks from template re-calculates incident assigned staff
					var originIncidentAssignedStaffCode = supportIncident.IM_Category == SupportIncidentCategoriesList.Codes.Support
													? (ZString)supportIncident.IM_GS_NKCustServiceContactInfo.OriginalValue
													: (ZString)supportIncident.IM_GS_NKAssignedToCurrentInfo.OriginalValue;
					var originIncidentAssignedStaff = incident.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, originIncidentAssignedStaffCode);
					if (originIncidentAssignedStaff != null && originIncidentAssignedStaff.GS_IsActive)
					{
						firstNonClosedTask.P9_GS_NKAssignedStaffMember = originIncidentAssignedStaffCode;
					}
					else if (firstNonClosedTask.P9_G4_RequiredCapability.IsEmpty && supportIncident.ProductAreaAssignedStaff != null && supportIncident.ProductAreaAssignedStaff.GS_IsActive)
					{
						firstNonClosedTask.P9_GS_NKAssignedStaffMember = supportIncident.ProductAreaAssignedStaff.GS_Code;
					}
				}

				if (firstNonClosedTask.P9_Status == ProcessTaskStatusCodeList.Codes.Open
					&& (!firstNonClosedTask.P9_GS_NKAssignedStaffMember.IsEmpty || !firstNonClosedTask.P9_G4_RequiredCapability.IsEmpty))
				{
					firstNonClosedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				}
			}
		}
	}
}

