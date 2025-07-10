using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public abstract class IncidentEvent : IIncidentEvent
	{
		protected IncidentEvent(IIncidentEventConsumer incident)
		{
			this.incident = incident;
		}
		protected readonly IIncidentEventConsumer incident;

		#region Properties

		public abstract ZString Code { get; }

		#endregion

		#region Apply Event

		public void Trigger()
		{
			try
			{
				ApplyWorkflowTemplate();
				SetAssignee();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce($"{ex.GetType()} occurred within the trigger event.", ex);
			}
		}

		//For more details on the following methods, see the following link: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/15377/Incident-Event?anchor=workflow-template
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected virtual bool ApplyWorkflowTemplate()
		{
			bool foundTasksFromMatchedTemplates = false;

			IProcessJobHeader jobHeader = ProcessJobHeaderProvider.GetForParent(incident, incident.Factory);
			var matchedTemplates = FindMatchedTemplates();

			if (jobHeader != null && matchedTemplates != null)
			{
				Dictionary<ZGuid, IProcessHeader> existingProcessHeadersMap = jobHeader.ProcessHeaders.Cast<IProcessHeader>().ToDictionary(x => x.PK);
				ProcessTaskCollectionView taskView = incident.WorkflowItems.Tasks;
				Dictionary<ZGuid, ProcessTask> existingTasksMap = taskView.Cast<ProcessTask>().ToDictionary(x => x.PK);

#pragma warning disable 0618
				// If this code was calling IWorkflowProvider.ApplyWorkflowTemplates we wouldn't need this (as well as the below 100 lines of code).
				// It could instead use TemplateApplicationParameters.jobAttributesMayHaveChangedSinceLastTemplateApplication to signal that the cache should be cleared.

				ProcessTask.Loader.ClearUserDefinedConditionCache((IBusiness)incident);
#pragma warning restore 0618

				foreach (ProcessTaskTemplate template in matchedTemplates)
				{
					ProcessTask[] tasksToBeCloned = taskView.GetItemsToCreateFromTemplate(template)
														.Where(task => task.ProcessHeader != null && IsProcessHeaderMetCondition(task.ProcessHeader))
														.ToArray();

					foundTasksFromMatchedTemplates = tasksToBeCloned.Length > 0;

					if (foundTasksFromMatchedTemplates)
					{
						var processHeaderPksToBeCloned = tasksToBeCloned.Select(task => task.ProcessHeader.PK).Distinct().ToDictionary(processHeaderPk => processHeaderPk);
						jobHeader.ApplyTemplate(template, x => IsProcessHeaderMetCondition(x) && processHeaderPksToBeCloned.ContainsKey(x.PK));
						var result = taskView.CreateItemsFromTemplate(tasksToBeCloned.Select(t => new TemplateItemApplication(template, new[] { t })).ToArray(), true, TemplateApplicationParameters.Default);
						result.ForEach(r => r.Dispose());

						// CreateItemsFromTemplate() method automatically assigns first task to current user if staff assingee is blank, which is not expected here
						var firstClonedTask = taskView.Cast<ProcessTask>().FirstOrDefault(task => task.P9_ParentTemplateID == tasksToBeCloned[0].PK);
						if (firstClonedTask != null)
						{
							if (firstClonedTask.P9_GS_NKAssignedStaffMember != tasksToBeCloned[0].P9_GS_NKAssignedStaffMember)
							{
								firstClonedTask.P9_GS_NKAssignedStaffMember = tasksToBeCloned[0].P9_GS_NKAssignedStaffMember;
								firstClonedTask.P9_Status = tasksToBeCloned[0].P9_Status;
							}

							if (firstClonedTask.P9_Status != tasksToBeCloned[0].P9_Status)
							{
								firstClonedTask.P9_Status = tasksToBeCloned[0].P9_Status;
							}
						}

						break;
					}
				}

				if (foundTasksFromMatchedTemplates || AlwaysCloseExistingTasks)
				{
					CloseOrCancelExistingTasks(existingTasksMap.Values);
				}

				if (foundTasksFromMatchedTemplates)
				{
					DeleteUnsavedEventTasks(existingTasksMap, existingProcessHeadersMap);

					var allProcessHeaders = jobHeader.ProcessHeaders.Cast<IProcessHeader>();
					var newProcessHeaders = allProcessHeaders.Where(x => !existingProcessHeadersMap.ContainsKey(x.PK));
					var existingProcessHeaders = allProcessHeaders.Where(x => existingProcessHeadersMap.ContainsKey(x.PK));

					foreach (var processHeader in newProcessHeaders.ToArray())
					{
						ZString newCompletionStatement = GetCompletionStatementWithoutEventCode(processHeader);

						var existingHeaderWithSameCompletionStatement = allProcessHeaders.FirstOrDefault(header => header.PK != processHeader.PK && header.FH_CompletionStatement == newCompletionStatement);
						if (existingHeaderWithSameCompletionStatement != null)
						{
							var newTasks = taskView.Cast<ProcessTask>().Where(task => task.ProcessHeader != null && task.ProcessHeader.PK == processHeader.PK);
							foreach (var task in newTasks.ToArray())
							{
								task.P9_FH_ProcessHeader = existingHeaderWithSameCompletionStatement.PK;
							}

							foreach (var link in processHeader.Links.ToArray())
							{
								var fromHeader = link.HeaderFrom;
								var toHeader = link.HeaderTo;
								var existingFromHeader = existingProcessHeaders.FirstOrDefault(header => header.PK != fromHeader.PK && GetCompletionStatementWithoutEventCode(header) == GetCompletionStatementWithoutEventCode(fromHeader));
								var existingToHeader = existingProcessHeaders.FirstOrDefault(header => header.PK != toHeader.PK && GetCompletionStatementWithoutEventCode(header) == GetCompletionStatementWithoutEventCode(toHeader));

								if (existingFromHeader != null && existingToHeader == null)
								{
									if(!existingFromHeader.Links.Any(l => l.FP_FH_HeaderTo == toHeader.PK))
									{
										link.FP_FH_HeaderFrom = existingFromHeader.PK;
									}
								}
								else if (existingFromHeader == null && existingToHeader != null)
								{
									if(!existingToHeader.Links.Any(l => l.FP_FH_HeaderFrom == fromHeader.PK))
									{
										link.FP_FH_HeaderTo = existingToHeader.PK;
									}
								}
							}

							jobHeader.ProcessHeaders.Delete(processHeader);
						}
						else
						{
							processHeader.FH_CompletionStatement = newCompletionStatement;
						}
					}

					var orderedNewTasks = taskView.Cast<ProcessTask>()
												.Where(task => !existingTasksMap.ContainsKey(task.PK))
												.OrderBy(task => task.P9_Sequence);
					ArrangeTasksSequence(existingTasksMap.Values, orderedNewTasks);

					foreach (var task in orderedNewTasks)
					{
						task.P9_ParentTemplateID = ZGuid.Empty; //Clear template id so it won't be merged when template task is copied again
						var eventProcessTask = task as IIncidentEventProcessTask;
						if (eventProcessTask != null)
						{
							eventProcessTask.EventCode = Code;
						}
					}
				}

				taskView.Sort(ProcessTasksSchema.Constants.P9_Sequence);
			}

			return foundTasksFromMatchedTemplates;
		}

		protected virtual void ArrangeTasksSequence(IEnumerable<ProcessTask> existingTasks, IOrderedEnumerable<ProcessTask> orderedNewTasks)
		{
			if (orderedNewTasks.Any())
			{
				int previousMaxTaskSequence = existingTasks.Any() ? existingTasks.Max(task => (int)task.P9_Sequence) : 0;
				int firstTaskSequence = orderedNewTasks.First().P9_Sequence;
				int sequenceIncrement = previousMaxTaskSequence + Math.Min(10, firstTaskSequence) - firstTaskSequence;
				foreach (var task in orderedNewTasks.ToArray())
				{
					task.P9_Sequence += sequenceIncrement;
				}
			}
		}

		protected virtual ProcessTaskTemplate[] FindMatchedTemplates()
		{
			return new ProcessTaskTemplate.Loader(incident.Factory).FindMatches(incident);
		}

		protected virtual void SetAssignee()
		{
		}

		protected virtual bool IsProcessHeaderMetCondition(IProcessHeader processHeader)
		{
			return processHeader.FH_CompletionStatement.StartsWith(Code, StringComparison.Ordinal);
		}

		protected virtual bool AlwaysCloseExistingTasks
		{
			get { return true; }
		}

		void DeleteUnsavedEventTasks(Dictionary<ZGuid, ProcessTask> existingTasksMap, Dictionary<ZGuid, IProcessHeader> existingProcessHeadersMap)
		{
			foreach (ProcessTask task in existingTasksMap.Values.ToArray())
			{
				if (!task.IsInDatabase)
				{
					var eventProcessTask = task as IIncidentEventProcessTask;
					if (eventProcessTask != null && !eventProcessTask.EventCode.IsEmpty)
					{
						var processHeader = task.ProcessHeader;
						existingTasksMap.Remove(task.PK);
						task.Delete();
						if (!processHeader.Tasks.Any())
						{
							existingProcessHeadersMap.Remove(processHeader.PK);
							processHeader.Delete();
						}
					}
				}
			}
		}

		void CloseOrCancelExistingTasks(IEnumerable<ProcessTask> existingTasks)
		{
			foreach (ProcessTask task in existingTasks)
			{
				if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Working || task.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended)
				{
					if (task.P9_GS_NKAssignedStaffMember.IsEmpty && task.P9_GG_AssignedGroup.IsEmpty)
					{
						task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
					}
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				}
				else if (task.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled && task.P9_Status != ProcessTaskStatusCodeList.Codes.Closed)
				{
					task.CancelAndSuspendValidationOnTaskCancellation();
				}
			}
		}

		ZString GetCompletionStatementWithoutEventCode(IProcessHeader processHeader)
		{
			ZString result = processHeader.FH_CompletionStatement;
			if (result.StartsWith(Code, StringComparison.Ordinal))
			{
				result = result.Substring(Code.Length, result.Length - Code.Length).Trim();
			}
			return result;
		}

		#endregion

		#region Get Task

		protected ProcessTask GetLastClosedTask()
		{
			return incident.WorkflowItems.Tasks.Cast<ProcessTask>()
				.Where(task => task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed)
				.OrderByDescending(task => task.P9_CompletedTimeUtc)
				.FirstOrDefault();
		}

		protected ProcessTask GetFirstNonClosedTask()
		{
			ProcessTask firstNonClosedTask = null;
			var orderedTasks = incident.WorkflowItems.Tasks.Cast<ProcessTask>().OrderBy(task => task.P9_Sequence);
			foreach (var task in orderedTasks)
			{
				if (task.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && task.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled)
				{
					firstNonClosedTask = task;
					break;
				}
			}
			return firstNonClosedTask;
		}

		#endregion
	}
}

