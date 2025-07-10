using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class ReleaseGateKeeper : ReleaseGateServiceTaskProcessor
	{
		protected internal ReleaseGateKeeper(BMComponent buffer, ILogger logger, ReleaseGateLogger releaseGateLogger, bool enableDataRefresh = false)
			: base(logger, new ReleaseGateKeeperFactoryProviderWrapper(logger, enableDataRefresh, ReleaseGateRunnerServiceTask.Code))
		{
			Buffer = buffer;
			ReleaseGateLogger = releaseGateLogger;
		}

		protected BMComponent Buffer { get; }
		protected ReleaseGateLogger ReleaseGateLogger { get; }
		protected virtual bool SaveCapacityCacheForAllStaff => false;
		readonly int BatchSize = BMSRegistry.Instance.ReleaseGateBatchSize.Value;

		#region ProcessHeaderProcessor Overrides

		internal static RestrictedTableBusinessObjectFactory GetFactoryForSavingLogs()
		{
			var factory = new RestrictedTableBusinessObjectFactory(allowedTableNamesToLoad: new[]
			{
				BMComponentResourceLinkSchema.Constants.TableName,

				StmDocDataOverrideSchema.Constants.TableName,
				StmNoteSchema.Constants.TableName,
				StmUniversalCopySchema.Constants.TableName,
			});

			factory.NameForDebugging = "ReleaseGateKeeper.FactoryForSavingLogs";
			factory.SuspendValidation();

			return factory;
		}

		#endregion

		#region Process

		Dictionary<string, IResourceCapacity> capacities = new Dictionary<string, IResourceCapacity>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task logging is never translated, Log details.")]
		public override void ProcessCore(CancellationToken token)
		{
			var workflowPKs = new List<ZGuid>();

			Log((NoResString)"Determining workflows eligible for Release Gate.");
			Dictionary<ZGuid, ReleaseGateRequest> requestsByPK = null;

			try
			{
				requestsByPK = GetRequestsByWorkflowPK(token);
			}
			catch (ApplicationException ex)
			{
				var msg = (NoResString)"Error on getting Workflows";
				Logger.Log(LogType.Error, msg, ex);

				using (DisposableEnvironment.ForBranch(Buffer.FC_GB_AgingBranch.ToGuid()))
				{
					ErrorReporter.ReportOnce("ReleaseGateKeeper.Process_GetRequestsByWorkflowPK", msg, ex);
				}

				WorkflowPKs = Enumerable.Empty<ZGuid>();

				return;
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Buffer.FC_GB_AgingBranch.ToGuid(), Buffer.FC_GE_AgingDepartment.ToGuid()))
			{
				var staffOnBoard = GetStaffOnBoards(Buffer);
				var staffWithTasksInReleaseGate = WorkingResourcesCalculator.GetResourcesWithTasksInReleaseGate(Buffer);
				var staffToCalculateCapacity = staffOnBoard.Union(staffWithTasksInReleaseGate).ToArray();

				if (!BMSRegistry.Instance.DisableCapacityCalculations.Value && staffToCalculateCapacity.Length > 0)
				{
					Log($"Calculating Capacity for {staffToCalculateCapacity.Length} staff.");
					capacities = CapacityCalculatorImpl.GetUtilisedCapacityBreakdowns(staffToCalculateCapacity, Buffer, logger: Logger);
				}

				Log((NoResString)"Assembling reservation tracker.");

				var staffCapacities = staffToCalculateCapacity.ToDictionary(s => s, s => capacities[s.GS_Code]);
				var reservationTracker = new CapacityReservationTracker(Buffer, staffCapacities, trackResourceQueues: true);

				if (requestsByPK.Count > 0)
				{
					if (BMSRegistry.Instance.WorkflowManagementMode.Value == WorkflowManagementModes.Codes.PlanningManagement)
					{
						ApprovedCcpmReleaseCache.CacheInFactory(Buffer.Factory, requestsByPK.Keys);
					}

					Log((NoResString)"Sorting workflows into release sequence.");
					var requestsInReleaseSequence = WorkflowTransferOrderSorter.Sort(requestsByPK.Values);

					FactoryProvider.CreateNewWithoutSave();
					Log(string.Format(CultureInfo.InvariantCulture, (NoResString)"Starting Release Gate run with {0} workflows to process.", requestsInReleaseSequence.Count));

					try
					{
						var allReleasedWorkflowPKs = DoPass(requestsInReleaseSequence, reservationTracker, token);
						workflowPKs.AddRange(allReleasedWorkflowPKs);

						if (BMSRegistry.Instance.CacheCalculatedCapacity.Value && capacities.Count > 0)
						{
							var staffCodes = (SaveCapacityCacheForAllStaff ? staffToCalculateCapacity : staffOnBoard)
								.Select(s => s.GS_Code);

							var capacitiesToSave = capacities
								.Where(c => staffCodes.Contains(c.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
							Log("Persisting updated capacity after Release Gate run.");
							BufferCapacityCache.SaveCapacityOnCache(Buffer.PK, capacitiesToSave);
						}
					}
					catch (Exception ex2) when (!ex2.IsCriticalException())
					{
						var newLine = System.Environment.NewLine;
						var messages = string.Join(newLine + newLine, ex2.FlattenInnerExceptions().Select(e => $"{e.Message}: {newLine} {e.StackTrace}"));
						Log(LogType.Error, $"Error when running Release Gate: {messages}");
						throw;
					}
					finally
					{
						FactoryProvider.CreateNewWithoutSave();
					}
				}
				else
				{
					Log((NoResString)"There are no workflows eligible for release.");
				}

				WorkflowPKs = workflowPKs;
			}
		}

		#endregion

		#region Release Eligibility by Transfer Rules

		Dictionary<ZGuid, ReleaseGateRequest> GetRequestsByWorkflowPK(CancellationToken token)
		{
			var requestsByPK = new Dictionary<ZGuid, ReleaseGateRequest>();
			var requestGroups = GetWorkflowsEligibleForRelease(token).GroupBy(request => request.WorkflowPK);

			foreach (var requests in requestGroups)
			{
				if (requests.Take(2).Count() > 1)
				{
					var requestBuffers = requests.Select(r => GetBuffer(r.BufferPK));
					var buffer = requestBuffers.OrderBy(b => b.FC_DisplaySequence).FirstOrDefault();

					if (buffer != null)
					{
						var request = requests.First(r => r.BufferPK == buffer.PK);
						requestsByPK[requests.Key] = request;
					}
				}
				else
				{
					requestsByPK[requests.Key] = requests.ElementAt(0);
				}

				EnsureBuffersForRequestsAreTheSameSpecifiedBuffer(requests.Key, requests); // I believe, the logic above is excessive and should be deleted as the buffer for requests are always one and the same buffer passed to the constructor
			}

			return requestsByPK;
		}

#if DEBUG
		public void EnsureBuffersForRequestsAreTheSameSpecifiedBuffer_Exposed(ZGuid workflowPK, IEnumerable<ReleaseGateRequest> requests) => EnsureBuffersForRequestsAreTheSameSpecifiedBuffer(workflowPK, requests);
#endif

		void EnsureBuffersForRequestsAreTheSameSpecifiedBuffer(ZGuid workflowPK, IEnumerable<ReleaseGateRequest> requests)
		{
			var bufferPKs = requests.Select(r => r.BufferPK).Distinct().ToArray();

			if (bufferPKs.Length > 1 || bufferPKs.Length == 1 && bufferPKs[0] != Buffer.PK)
			{
				var builder = new ZStringBuilder((NoResString)"I believe, the buffer for requests are always one and the same buffer passed to the constructor, but this reported proves this assumption is wrong.");
				builder.Append($"Buffer for ReleaseGateKeeper: {Buffer.FC_Name}, PK={Buffer.PK}");
				builder.Append($"Workflow PK = {workflowPK}");
				builder.Append((NoResString)"Requests for the workflow:");

				foreach (var request in requests)
				{
					builder.Append($"FromComponent PK = {request.FromComponentPK}, Buffer PK (ComponentTo PK) = {request.BufferPK}, Buffer = {GetBuffer(request.BufferPK).FC_Name}, ComponentLink PK = {request.ComponentLink.PK}");
				}

				ErrorReporter.ReportOnce("EnsureBuffersForRequestsAreTheSameSpecifiedBuffer", builder.ToStringWithNewLineBetweenAppends());
			}
		}

		IEnumerable<ReleaseGateRequest> GetWorkflowsEligibleForRelease(CancellationToken token)
		{
			var eligibleWorkflowsForReleaseGate = new HashSet<ReleaseGateRequest>();
			var logger = new TransferRuleRunnerLogger(Logger, Buffer.System);
			var dataAccessor = new ReleaseGateTransferRuleRunnerDataAccessor(logger);
			var transferRuleRunner = GetLinksProcessor(eligibleWorkflowsForReleaseGate, logger, dataAccessor);
			transferRuleRunner.Process(token);

			return eligibleWorkflowsForReleaseGate;
		}

		protected virtual LinksProcessorWithDeactivation GetLinksProcessor(HashSet<ReleaseGateRequest> eligibleWorkflowsForReleaseGate, TransferRuleRunnerLogger logger, ReleaseGateTransferRuleRunnerDataAccessor dataAccessor)
		{
			var linksToProcess = dataAccessor.GetComponentLinks(Buffer.System).Where(l => l.ComponentTo == Buffer.PK).ToArray();
			var processor = new LinksProcessorWithDeactivation(linksToProcess, dataAccessor, shouldUseSecondaryServerIfAllowed: true, logger, (workflow, link) =>
			{
				var releaseGateRequest = new ReleaseGateRequest((ProcessHeader)workflow, (BMComponentLink)link);

				if (!eligibleWorkflowsForReleaseGate.Contains(releaseGateRequest))
				{
					eligibleWorkflowsForReleaseGate.Add(releaseGateRequest);
				}
			});

			return processor;
		}

		#endregion

		#region DoPass

		List<ZGuid> DoPass(Queue<ReleaseGateRequest> requestsInReleaseSequence, CapacityReservationTracker reservationTracker, CancellationToken token)
		{
			var workflowPKs = new List<ZGuid>();

			var staleTime = ZDateTime.UtcNow.AddMinutes(BMSRegistry.Instance.ReleaseGateStaleTime.Value);

			try
			{
				var workflowsReleasedThisPass = DoPass(requestsInReleaseSequence, staleTime, reservationTracker, token);
				workflowPKs.AddRange(workflowsReleasedThisPass);
			}
			catch (ReleaseGateDataIsStaleException)
			{
				if (requestsInReleaseSequence.Count > 0)
				{
					Log(LogType.Warning, FormattableString.Invariant($"Release Gate was terminated with {requestsInReleaseSequence.Count} requests still to process.")); // Service task logging
				}
			}
			return workflowPKs;
		}

		List<ZGuid> DoPass(Queue<ReleaseGateRequest> requests, ZDateTime staleTime, CapacityReservationTracker reservationTracker, CancellationToken token)
		{
			var workflowPKs = new List<ZGuid>();

			while (requests.Count > 0)
			{
				token.ThrowIfCancellationRequested();
				EnsureWorkflowsLoaded(requests);
				var request = requests.Dequeue();

				if (TryRelease(request, reservationTracker))
				{
					workflowPKs.Add(request.WorkflowPK);
				}

				if (ZDateTime.UtcNow > staleTime)
				{
					throw new ReleaseGateDataIsStaleException();
				}
			}

			return workflowPKs;
		}

		void EnsureWorkflowsLoaded(Queue<ReleaseGateRequest> queue)
		{
			if (queue.Peek().Workflow == null)
			{
				FactoryProvider.CreateNewWithoutSave();
				foreach (var request in queue)
				{
					if (request.Workflow != null && request.Workflow.Factory != FactoryProvider.Current)
					{
						request.Workflow = null;
					}
				}

				var workflowPKsForLoading = new List<ZGuid>(BatchSize);
				var parentTableCodesForLoading = new HashSet<ZString>();

				foreach (var request in queue.Take(BatchSize))
				{
					workflowPKsForLoading.Add(request.WorkflowPK);
					parentTableCodesForLoading.Add(request.WorkflowParentTableCode);
				}

				if (workflowPKsForLoading.Count > ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION)
				{
					ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "There were {0} items in a workflow batch. That will cause excessive load on the query plan cache since the SQL IN predicate will have literals instead of being parameterised.", workflowPKsForLoading.Count));
				}

				RunPreWorkflowLoadActions(workflowPKsForLoading);

				var loadedWorkflows = FactoryProvider.Current.LoadFromPrimaryKeysAndTableCode(workflowPKsForLoading, parentTableCodesForLoading)
					.ToDictionary(w => w.PK);

				foreach (var request in queue)
				{
					var workflowPK = request.WorkflowPK;
					if (loadedWorkflows.ContainsKey(workflowPK))
					{
						var workflow = loadedWorkflows[workflowPK];
						request.Workflow = workflow;
					}
				}
			}
		}

		#endregion

		#region Workflow Capacity

		static GlbStaff[] GetStaffOnBoards(BMComponent buffer)
		{
			var resourceQuery = new ZDBOnlyQuery(typeof(GlbStaff));
			var channelSubQuery = new ZDBOnlySubQuery(typeof(BMBoardSectionChannel), BMBoardSectionChannelSchema.MSC_ParentID);
			var sectionSubQuery = new ZDBOnlySubQuery(typeof(BMBoardSection), BMBoardSectionChannelSchema.MSC_MS_Section);

			var sectionSubQuery_directlySpecified = new ZDBOnlySubQuery(typeof(BMBoardSection), BMBoardSectionSchema.PK);
			var sectionSubQuery_additionalComponents = new ZDBOnlySubQuery(typeof(BMBoardSection), BMBoardSectionSchema.PK);

			sectionSubQuery_directlySpecified.AddToFilter(BMBoardSectionSchema.MS_FC_Component, buffer.PK);

			var additionalComponentsSubQuery = new ZDBOnlySubQuery(typeof(BMBoardSectionAdditionalComponent), BMBoardSectionAdditionalComponentSchema.BSA_MS_Section);
			additionalComponentsSubQuery.AddToFilter(BMBoardSectionAdditionalComponentSchema.BSA_FC_Component, buffer.PK);
			sectionSubQuery_additionalComponents.AddSubQuery(additionalComponentsSubQuery, JoinCondition.And);

			sectionSubQuery.AddSubQuery(sectionSubQuery_directlySpecified, JoinCondition.And);
			sectionSubQuery.AddAsUnionQuery(sectionSubQuery_additionalComponents, addAsUnionAll: true);

			channelSubQuery.AddSubQuery(sectionSubQuery, JoinCondition.And);
			resourceQuery.AddSubQuery(channelSubQuery, JoinCondition.And);
			resourceQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);

			return buffer.Factory.Load<GlbStaff>(resourceQuery);
		}

		protected virtual void RunPreWorkflowLoadActions(IEnumerable<ZGuid> workflowPKs)
		{
			FactoryProvider.Current.Load<ViewProcessTask>(new ZQuery(ViewProcessTaskSchema.P9_FH_ProcessHeader, workflowPKs));
			FactoryProvider.Current.SeedQueryCache(ViewProcessTaskSchema.Constants.TableName, new ZQuery());
		}

		bool TryRelease(ReleaseGateRequest request, CapacityReservationTracker reservationTracker)
		{
			if (request.Workflow != null)
			{
				var requestBuffer = GetBuffer(request.BufferPK);
				if (requestBuffer != null)
				{
					var workflow = request.Workflow;

					var provider = Buffer as IBranchDepartmentProvider;
					var branchPK = provider.GetBranch(Buffer.Factory).PK.ToGuid();
					var departmentPK = provider.GetDepartment(Buffer.Factory).PK.ToGuid();

					CapacityReservationReport releasabilityReport;
					using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchPK, departmentPK))
					{
						releasabilityReport = reservationTracker.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations(workflow, workflow.Factory);
						var isReleasable = IsWorkflowReleasable(workflow, releasabilityReport) && request.FromComponentPK == request.Workflow.VFH_FC_CurrentComponent;

						if (isReleasable)
						{
							workflow.ReleaseToBuffer(requestBuffer, request.ComponentLink);  // If another service task moves the workflow to a different component between the previous check and the relase, this can cause a race condition. However the possibility of this happening is very small and deemed acceptable at this current time.
							DeductCapacities(releasabilityReport);

							if (workflow.Factory != FactoryProvider.Current)
							{
								throw new InvalidOperationException("Attempted to save the wrong factory");
							}

							FactoryProvider.Save(createNew: false);
						}

						CreateLogs(releasabilityReport, requestBuffer, isReleasable);

						return isReleasable;
					}
				}
			}
			else
			{
				Logger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Workflow PK '{0}' is no longer in database.", request.WorkflowPK));
			}

			return false;
		}

		void DeductCapacities(CapacityReservationReport report)
		{
			foreach (var line in report.ResourceCapacityReportLines)
			{
				var staffCode = line.Resource.GS_Code;

				if (!capacities.TryGetValue(staffCode, out var capacity))
				{
					return;
				}

				var multiplier = line.Report.ZoneMultiplier;

				capacities[staffCode] = capacity.Deduct(line.TotalCapacityHoursRequired / multiplier, 3, multiplier);
			}
		}

		protected virtual bool IsWorkflowReleasable(ViewProcessHeader workflow, CapacityReservationReport report)
		{
			return report.BufferReleaseOutcome.IsReleasable();
		}

		void CreateLogs(CapacityReservationReport releasabilityReport, BMComponent requestBuffer, bool wasReleased)
		{
			var log = releasabilityReport.CreateOutcomeLogMessage();
			var workflow = releasabilityReport.WorkflowBeingConsideredForRelease;

			if (wasReleased)
			{
				ReleaseGateLogger.ClearReleaseFailure(workflow, requestBuffer);

				Log(FormattableString.Invariant($"Workflow {workflow.Description} for job {workflow.JobDescription} has been released.")); // Service Task Logging

				if (BMSRegistry.Instance.LogReleaseIntoBufferDetails.Value)
				{
					ReleaseGateLogger.LogSuccessfulRelease(workflow.PK, requestBuffer, log);
				}
			}
			else
			{
				ReleaseGateLogger.LogReleaseFailure(workflow.PK, requestBuffer, log);
			}

			foreach (var line in releasabilityReport.ResourceCapacityReportLines)
			{
				var message = CapacityReservationLogCreator.CreateLogMessageForReportLine(line);
				ReleaseGateLogger.LogCapacityReservation(line.Resource.GS_Code, requestBuffer.PK, message);
			}
		}

		readonly Dictionary<ZGuid, BMComponent> buffers = new Dictionary<ZGuid, BMComponent>();
		BMComponent GetBuffer(ZGuid bufferPK)
		{
			if (!buffers.ContainsKey(bufferPK))
			{
				var otherBuffer = Buffer.Factory.Load<BMComponent>(bufferPK);

				return buffers[bufferPK] = otherBuffer;
			}

			return buffers[bufferPK];
		}

		#endregion

		#region Logging

		void Log(string message)
		{
			Log(LogType.Information, message);
		}

		void Log(LogType logType, string message)
		{
			message = FormattableString.Invariant($"{Buffer.System.FS_Name}.{Buffer.FC_Name}: {message}");

			Logger.Log(logType, message);
		}

		#endregion
	}
}
