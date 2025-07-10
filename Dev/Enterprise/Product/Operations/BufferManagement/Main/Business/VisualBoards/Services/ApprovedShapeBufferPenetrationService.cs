using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ApprovedShapeBufferPenetrationService : NonResettableService
	{
		public ApprovedShapeBufferPenetrationService(ICollection<ProcessHeader> allWorkflows)
		{
			//OMG THAT IS bad, TODO: remove all that Threading.Tasks
			if (allWorkflows != null)
			{
				var workflowsToConsider = allWorkflows.Concat(allWorkflows.Select(w => w.JobHeader)).Select(w => w.PK).Distinct().ToArray();

				bufferPenetrations = GetAllApprovedShapeDetails(workflowsToConsider);
			}
			else
			{
				bufferPenetrations = Task.FromResult(new Dictionary<ZGuid, ApprovedShapeDetails>());
			}
		}

		public void WaitAllApprovedShapeDetails()
		{
			Task.WaitAll(bufferPenetrations);
		}

		internal ApprovedShapeDetails GetApprovedShapeDetails(ProcessHeader workflow)
		{
			if (workflow == null)
			{
				return null;
			}

			var approvedShapes = bufferPenetrations.Result;

			if (approvedShapes == null || approvedShapes.Count == 0)
			{
				return null;
			}

			var stack = new Stack<ProcessHeader>();
			var visitedPKs = new HashSet<ZGuid>();

			stack.Push(workflow);

			while (stack.Count > 0)
			{
				var currentWorkflow = stack.Pop();

				if (!visitedPKs.Contains(currentWorkflow.PK) && approvedShapes.TryGetValue(currentWorkflow.PK, out var approvedShapeDetails))
				{
					return approvedShapeDetails;
				}

				visitedPKs.Add(currentWorkflow.PK);

				foreach (var link in currentWorkflow.ParentLinks.Where(l => !visitedPKs.Contains(l.FP_FH_HeaderTo)))
				{
					var parent = link.HeaderTo;

					if (parent != null && currentWorkflow.IsInSameJob(parent))
					{
						if (approvedShapes.TryGetValue(parent.PK, out var parentApprovedShapeDetails))
						{
							return parentApprovedShapeDetails;
						}

						stack.Push(parent);
						visitedPKs.Add(parent.PK);
					}
				}

				if (currentWorkflow.IsWorkflow)
				{
					var jobHeader = currentWorkflow.JobHeader;

					if (!visitedPKs.Contains(jobHeader.PK))
					{
						stack.Push(jobHeader);
					}
				}
			}

			return null;
		}

		internal class ApprovedShapeDetails
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
			internal ApprovedShapeDetails(ZGuid workflowPK, decimal bufferPenetration, ZDateTime startableTimeUtc, int plannedDurationInMinutes, bool hasRelatedBuffers)
			{
				WorkflowPK = workflowPK;
				BufferPenetration = bufferPenetration;
				StartableTimeUtc = startableTimeUtc;
				PlannedDurationInMinutes = plannedDurationInMinutes;
				HasRelatedBuffers = hasRelatedBuffers;
			}

			internal ZGuid WorkflowPK { get; }
			internal decimal BufferPenetration { get; }
			internal ZDateTime StartableTimeUtc { get; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
			internal int PlannedDurationInMinutes { get; }
			internal bool HasRelatedBuffers { get; }
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static Task<Dictionary<ZGuid, ApprovedShapeDetails>> GetAllApprovedShapeDetails(ZGuid[] workflowPKs)
		{
			Dictionary<ZGuid, ApprovedShapeDetails> GetDetails()
			{
				var result = new Dictionary<ZGuid, ApprovedShapeDetails>();
				var sql = @"
SELECT
	VWS_PK,
	VWS_BufferPenetration,
	VWS_ScheduledStartTimeUtc,
	VWS_ExplicitDurationMinutes,
	CASE
		WHEN VWS_BNS_Shape IN
		(
			SELECT BNA_BNS_ToShape
			FROM dbo.BMNCNAttachment
			WHERE BNA_Type = 'BUF'
		)
		THEN CONVERT(bit, 1)
		ELSE CONVERT(bit, 0)
	END as HasRelatedBuffers
FROM dbo.ViewApprovedWorkflowSchedule
WHERE VWS_PK IN (SELECT Value FROM @WorkflowPKs)
AND VWS_ApprovedScheduleType = 'CCPM'
";

				using (var command = Db.Connection.Command(sql)) // This is faster than attempting to load BMNCNShapes for every workflow.
				{
					command.AddTableValuedParameter("@WorkflowPKs", ViewApprovedWorkflowScheduleSchema.PK, workflowPKs);

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var pk = reader.GetGuid(0);
							var penetration = reader.IsDBNull(1) ? null : new decimal?(reader.GetDecimal(1));
							var scheduledStart = reader.IsDBNull(2) ? ZDateTime.Invalid : reader.GetDateTime(2);
							var duration = reader.IsDBNull(3) ? null : new int?(reader.GetInt32(3));
							var hasRelatedBuffers = reader.GetBoolean(4);

							if (penetration.HasValue && scheduledStart.IsValid && duration.HasValue)
							{
								result.Add(pk, new ApprovedShapeDetails(pk, penetration.Value, scheduledStart, duration.Value, hasRelatedBuffers));
							}
						}
					}
				}

				return result;
			}

			return BMSRegistry.Instance.BoardOnSecondaryServer.Value ? Task.FromResult(GetDetails()) : AsyncStrategy.Default.GetAsync(GetDetails, threadName: nameof(ApprovedShapeBufferPenetrationService));
		}

		readonly Task<Dictionary<ZGuid, ApprovedShapeDetails>> bufferPenetrations;

		#endregion
	}
}
