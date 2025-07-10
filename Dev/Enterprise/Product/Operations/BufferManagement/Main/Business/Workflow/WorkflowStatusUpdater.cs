using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class WorkflowStatusUpdater
	{
		static readonly ImmutableArray<ZString> types = ImmutableArray.Create<ZString>("MIL", "EXC", "TRG");
		static readonly ImmutableArray<ZString> statuses = ImmutableArray.Create<ZString>("ASN", "OPN", "SUS", "WRK" );

		internal static void UpdateWorkflowStatuses(IEnumerable<ProcessHeader> headersWithChanges, bool reloadStatusRelatedDataFromDb = false)
		{
			// FH_Status status consists of two separate pair of statuses, these being
			// 0. Closed or Open
			// 1. Blocked or Unblocked
			// There is a dependency between closed/open and blocked/unblocked.
			// That is, blocked/unblocked depends on closed/open in that items become unblocked when they have no closed prerequisites.
			//
			// Because of this we need to calculate the closed/open status *before* the blocked/unblocked status.
			// Therefore this algorithm needs two loops.
			//
			// Another important factor is avoiding an endless loop we should avoid an endless loop as:
			// in the closed/open loop things in loops will either all become closed or all become open
			// in the blocked/unblocked loop all things will become blocked.
			var collector = new WorkflowJobStatusChangeLogCollector();
			var closedOpenStack = new Stack<ProcessHeader>(); // Important to use a stack to avoid endless loops due to tail-chasing updates.
			var newlyClosedHeaders = new HashSet<ProcessHeader>();

			var headers = headersWithChanges.Where(h => !h.IsTemplate).ToArray();

			if (headers.Length == 0)
			{
				return;
			}

			foreach (var header in headers)
			{
				closedOpenStack.Push(header);
				if (!header.IsInDatabase)
				{
					collector.Collect(header); // Ensure JOP log for all new workflows.
				}
				else
				{
					header.Factory.AddFetchHint(typeof(ProcessTask), header.GetTasksQuery());
				}
			}

			var itemsToCheckForUnblocking = UpdateOpenClosedStatus(collector, closedOpenStack, newlyClosedHeaders, reloadStatusRelatedDataFromDb);
			var thingsAffectedByStatusChange = itemsToCheckForUnblocking.SelectMany(p => p.DirectPostrequisites)
					.Concat(itemsToCheckForUnblocking.SelectMany(p => p.ChildHeaders)).ToList();
			itemsToCheckForUnblocking.UnionWith(headers);
			itemsToCheckForUnblocking.UnionWith(thingsAffectedByStatusChange);

			var blockedUnblockedStack = new Stack<ProcessHeader>(); // Not needed to use a stack to avoid endless loops because only one candidate coalescing state.
			foreach (var item in itemsToCheckForUnblocking)
			{
				blockedUnblockedStack.Push(item);
			}
			UpdateBlockedUnblockedStatus(collector, blockedUnblockedStack, reloadStatusRelatedDataFromDb);

			foreach (var header in headers)
			{
				if (header.FH_Status == WorkflowStatusList.Codes.Open || header.FH_Status == WorkflowStatusList.Codes.Blocked)
				{
					var tasks = header.GetTasksWithoutAccessingWorkflowParent()
						.Where(t =>
							!types.Contains(t.P9_Type) &&
							statuses.Contains(t.P9_Status) &&
							t.P9_ParentTableCode != "P0").ToArray();

					if (header.FH_Status == WorkflowStatusList.Codes.Open)
					{
						header.FH_TaskLowestOpenSequenceNumber = tasks.Select(t => t.P9_Sequence).DefaultIfEmpty(-1).Min();
					}
					else
					{
						header.FH_TaskLowestOpenSequenceNumber = -1;
					}

					header.FH_RemainingMinutesToComplete = Math.Max(0, tasks.Sum(t => t.GetEstimatedMinutesToComplete()));
				}
				else
				{
					header.FH_TaskLowestOpenSequenceNumber = -1;
					header.FH_RemainingMinutesToComplete = 0;
				}
			}

			foreach (var header in newlyClosedHeaders)
			{
				header.UpdateBufferPenetrationPercentage();
			}

			collector?.CommitAllLogs();
		}

		#region Status Updating Procedures

		static void Close(ProcessHeader header)
		{
			if (header.FH_Status == WorkflowStatusList.Codes.Blocked)
			{
				// We assume that the blocked/unblocked status won't change by default.
				header.FH_Status = WorkflowStatusList.Codes.ClosedWithOpenPrerequisites;
			}
			else
			{
				header.FH_Status = WorkflowStatusList.Codes.Closed;
			}
		}

		static void Open(ProcessHeader header)
		{
			if (header.FH_Status == WorkflowStatusList.Codes.ClosedWithOpenPrerequisites)
			{
				// We assume that the blocked/unblocked status won't change by default.
				header.FH_Status = WorkflowStatusList.Codes.Blocked;
			}
			else
			{
				header.FH_Status = WorkflowStatusList.Codes.Open;
			}
		}

		static void Block(ProcessHeader header)
		{
			if (header.FH_Status == WorkflowStatusList.Codes.Closed)
			{
				header.FH_Status = WorkflowStatusList.Codes.ClosedWithOpenPrerequisites;
			}
			else if (header.FH_Status == WorkflowStatusList.Codes.Open)
			{
				header.FH_Status = WorkflowStatusList.Codes.Blocked;
			}
			else
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Unexpected status of {header} was {header.FH_Status}"));
			}
		}

		static void Unblock(ProcessHeader header)
		{
			if (header.FH_Status == WorkflowStatusList.Codes.ClosedWithOpenPrerequisites)
			{
				header.FH_Status = WorkflowStatusList.Codes.Closed;
			}
			else if (header.FH_Status == WorkflowStatusList.Codes.Blocked)
			{
				header.FH_Status = WorkflowStatusList.Codes.Open;
			}
			else
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Unexpected status of {header} was {header.FH_Status}"));
			}
		}

		#endregion

		#region Update Status Looping Procedures

		static HashSet<ProcessHeader> UpdateOpenClosedStatus(WorkflowJobStatusChangeLogCollector collector, Stack<ProcessHeader> stack, HashSet<ProcessHeader> newlyClosedHeaders, bool reloadStatusRelatedDataFromDb)
		{
			var processedHeaders = new HashSet<ProcessHeader>();
			var headersWithChildrenAddedToStack = new HashSet<ProcessHeader>();
			var updatedHeaders = new HashSet<ProcessHeader>();

			while (stack.Count > 0)
			{
				var header = stack.Pop();

				void PushHeaderAndMaybeAddFetchHints(ProcessHeader header)
				{
					stack.Push(header);

					if (!reloadStatusRelatedDataFromDb)
					{
						header.Factory.AddFetchHint(typeof(ProcessHeaderLink), new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, header.PK) { IsForceSeek = true }); // we'll load ChildHeaders of pushed headers
						header.Factory.AddFetchHint(typeof(ProcessHeaderLink), new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, header.PK) { IsForceSeek = true }); // we'll possibly load DirectParents of pushed headers
					}
				}

				void OnOpenClosedChanged()
				{
					collector.Collect(header);
					updatedHeaders.Add(header);

					if (reloadStatusRelatedDataFromDb)
					{
						ProcessJobHeader.ReloadJobRelatedLinksFromDbOnSaveIfNeeded(header);
					}
					foreach (var parent in header.DirectParents)
					{
						PushHeaderAndMaybeAddFetchHints(parent);
					}
				}

				if (processedHeaders.Contains(header))
				{
					continue;
				}

				if (reloadStatusRelatedDataFromDb)
				{
					ProcessJobHeader.ReloadJobRelatedTasksFromDbOnSaveIfNeeded(header);
				}

				var childHeaders = new Lazy<ProcessHeader[]>(() =>
				{
					if (reloadStatusRelatedDataFromDb)
					{
						ProcessJobHeader.ReloadJobRelatedLinksFromDbOnSaveIfNeeded(header);
					}
					return header.ChildHeaders.ToArray();
				});

				if (WorkflowDataRegistry.Instance.EnhancedWorkflowStatusUpdateMode.Value && !headersWithChildrenAddedToStack.Contains(header) && childHeaders.Value.Length > 0)
				{
					headersWithChildrenAddedToStack.Add(header);
					stack.Push(header);

					foreach (var child in childHeaders.Value)
					{
						// below we check the statuses of the child headers to decide whether a parent header should be open or not - so, let's update the statuses of the child headers first then
						PushHeaderAndMaybeAddFetchHints(child);
					}
					continue;
				}
				processedHeaders.Add(header);

				var hasOpenTasks = header.IsWorkflow && header.GetTasksWithoutAccessingWorkflowParent().Any(t => t.IsOpen);
				var shouldBeOpen = hasOpenTasks || childHeaders.Value.Any(c => c.HasOpenStatus);

				if (shouldBeOpen)
				{
					if (!header.HasOpenStatus)
					{
						Open(header);
						OnOpenClosedChanged();
					}
					// else header has an open status already so do nothing.
				}
				else
				{
					if (!header.HasClosedStatus)
					{
						Close(header);
						OnOpenClosedChanged();
						newlyClosedHeaders.Add(header);
					}
					// else header has a closed status already so do nothing.
				}
			}

			return updatedHeaders;
		}

		static void UpdateBlockedUnblockedStatus(WorkflowJobStatusChangeLogCollector collector, Stack<ProcessHeader> stack, bool reloadStatusRelatedDataFromDb)
		{
			var processedHeaders = new HashSet<ProcessHeader>();
			var headersWithPrerequisitesAndParentsAddedToStack = new HashSet<ProcessHeader>();

			while (stack.Count > 0)
			{
				var header = stack.Pop();

				void PushHeaderAndMaybeAddFetchHints(ProcessHeader header)
				{
					stack.Push(header);

					if (!reloadStatusRelatedDataFromDb)
					{
						header.Factory.AddFetchHint(typeof(ProcessHeaderLink), new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, header.PK) { IsForceSeek = true }); // we'll possibly load DirectPrerequisites and ChildHeaders of pushed headers
						header.Factory.AddFetchHint(typeof(ProcessHeaderLink), new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, header.PK) { IsForceSeek = true }); // we'll load DirectParents and possibly DirectPostrequisites of pushed headers
					}
				}

				void OnBlockedUnblockedChanged()
				{
					collector.Collect(header);
					foreach (var postreqOrChild in header.DirectPostrequisites.Union(header.ChildHeaders))
					{
						PushHeaderAndMaybeAddFetchHints(postreqOrChild);
					}
				}

				if (processedHeaders.Contains(header))
				{
					continue;
				}

				if (reloadStatusRelatedDataFromDb)
				{
					ProcessJobHeader.ReloadJobRelatedLinksFromDbOnSaveIfNeeded(header);
				}

				var directParents = header.DirectParents.ToArray();
				var prerequisites = new Lazy<ProcessHeader[]>(() => header.DirectPrerequisites.ToArray());

				if (WorkflowDataRegistry.Instance.EnhancedWorkflowStatusUpdateMode.Value && !headersWithPrerequisitesAndParentsAddedToStack.Contains(header)
					&& (directParents.Length > 0 || prerequisites.Value.Length > 0))
				{
					headersWithPrerequisitesAndParentsAddedToStack.Add(header);
					stack.Push(header);

					foreach (var prereqOrParent in directParents.Union(prerequisites.Value))
					{
						// below we check the statuses of the parents and prerequisites to decide whether the header should be open or not - so, let's update the statuses of the parents and prerequisites first then
						PushHeaderAndMaybeAddFetchHints(prereqOrParent);
					}
					continue;
				}
				processedHeaders.Add(header);

				var hasBlockedParents = header.DirectParents.Any(p => p.HasBlockedStatus);
				var shouldBeBlocked = hasBlockedParents || prerequisites.Value.Any(p => p.HasBlockingStatus);

				if (shouldBeBlocked)
				{
					if (!header.HasBlockedStatus)
					{
						Block(header);
						OnBlockedUnblockedChanged();
					}
					// else has correct blocked status so do nothing.
				}
				else
				{
					if (!header.HasUnblockedStatus)
					{
						Unblock(header);
						OnBlockedUnblockedChanged();
					}
					// else has correct unblocked status so do nothing.
				}
			}
		}

		#endregion
	}
}
