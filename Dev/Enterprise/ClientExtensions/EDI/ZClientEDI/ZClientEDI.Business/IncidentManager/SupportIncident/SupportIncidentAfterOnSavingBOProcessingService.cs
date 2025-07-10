using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	internal class SupportIncidentAfterOnSavingBOProcessingService : IAfterOnSavingBOProcessingService
	{
		public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			ProcessLogs(businessObjectsInOnSavingOrder);
		}

		void ProcessLogs(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			var incidents = businessObjectsInOnSavingOrder?.OfType<SupportIncident>().Where(x => !x.IsDeleted);
			foreach (var incident in incidents)
			{
				DeleteExtraLog(incident);
				LogFieldChanges(incident);

				if (incident.IM_ResolutionCodeInfo.HasChanges || incident.IM_CategoryInfo.HasChanges)
				{
					switch (incident.IM_ResolutionCode)
					{
						case SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse:
							incident.Logs.AddLog(Events.IncidentAwaitingResponse, "Incident has been set to Closed Awaiting Client Response");
							break;
						case SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided:
							incident.Logs.AddLog(Events.IncidentDevelopmentEstimateProvided, "Incident has been set to Development Estimate Provided");
							break;
						case SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided:
							incident.Logs.AddLog(Events.IncidentFormalQuoteProvided, "Incident has been set to Formal Quote Provided");
							break;
					}

					if (incident.IM_Category == SupportIncidentCategoriesList.Codes.Support && incident.IM_Status == SupportIncidentLookups.Status.Working)
					{
						switch (incident.IM_ResolutionCode)
						{
							case SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress:
								incident.Logs.AddLog(Events.EntryWorkInProgress);
								break;

							case SupportIncidentLookups.DispositionList.Constants.CallBackClient:
								incident.Logs.AddLog(Events.CallBackClient);
								break;
						}
					}
					else if ((ZString)incident.IM_ResolutionCodeInfo.OriginalValue != SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed
						&& incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed)
					{
						UpdateTEAMetric(incident);
						if ((ZString)incident.IM_ResolutionCodeInfo.OriginalValue != SupportIncidentLookups.DispositionList.Constants.Closed.Resolved)
						{
							UpdateTRAAndNRAMetric(incident);
						}
					}
					else if ((ZString)incident.IM_ResolutionCodeInfo.OriginalValue != SupportIncidentLookups.DispositionList.Constants.Closed.Resolved
						&& incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.Resolved)
					{
						UpdateTRAAndNRAMetric(incident);
					}

					if (incident.awaitingResolutionCodes.Contains((ZString)incident.IM_ResolutionCodeInfo.OriginalValue) && !incident.awaitingResolutionCodes.Contains(incident.IM_ResolutionCode))
					{
						var incidentReopenedEvent = incident.Logs.AddLog(Events.IncidentReopened);
						UpdateNRTMetricStartTime(incident, incidentReopenedEvent);

						var mostRecentAwaitingLog = incident.GetMostRecentLogByEventCodes(incident.awaitingEventCodes);
						if (mostRecentAwaitingLog != null)
						{
							incident.CompleteMetric(incident, IncidentMetricConstants.AwaitingClientTime, mostRecentAwaitingLog.SL_PostedTimeUtc, incidentReopenedEvent.SL_PostedTimeUtc, metricCount: 1);
						}
					}

					var mostRecentDTCEvent = incident.GetMostRecentLogByEventCode(AutoEvents.DetachedCode);
					if ((ZString)incident.IM_ResolutionCodeInfo.OriginalValue != SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated
						&& incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated)
					{
						InitTDTMetric(incident);
					}
					else if (mostRecentDTCEvent != null && !mostRecentDTCEvent.IsInDatabase)
					{
						var excludedWorkItemNumber = mostRecentDTCEvent.SL_Reference.Split(' ')[0];
						var mostEarliestATCEvent = GetMostEarliestATCLog(incident, excludedWorkItemNumber);
						UpdateTDTMetricWithDTCEvent(incident, mostEarliestATCEvent);
					}
					else if (incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.Resolved || incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed || incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled)
					{
						UpdateTDTMetric(incident);
					}
				}

				if (!incident.IsFirstResponsePending)
				{
					var messages = incident.EConversation.GetTimeOrderedMessages();
					var newStaffMessages = messages.Where(msg => msg.MessageSubType == MessageSubType.UserMessage && msg.MessageType == MessageType.LocalPublished && !msg.IsInDatabase);
					var staffMessages = messages.Where(msg => msg.MessageSubType == MessageSubType.UserMessage && msg.MessageType == MessageType.LocalPublished);
					if (newStaffMessages.Any() && staffMessages.Count() > 1)
					{
						var firstNonSupportMsg = messages.Cast<JobConversationMessage>().FirstOrDefault(msg => msg.MessageType != MessageType.LocalPublished || msg.MessageSubType != MessageSubType.UserMessage && msg.IsInDatabase);
						var staffMessagesCount = messages.Cast<JobConversationMessage>().TakeWhile(msg => msg != firstNonSupportMsg).Count(msg => msg.MessageSubType == MessageSubType.UserMessage && msg.MessageType == MessageType.LocalPublished);
						var lastNewStaffMessage = newStaffMessages.Cast<JobConversationMessage>().FirstOrDefault();
						var firstSupportMsg = messages.Cast<JobConversationMessage>().TakeWhile(msg => msg != firstNonSupportMsg).LastOrDefault(msg => msg.MessageSubType == MessageSubType.UserMessage && msg.MessageType == MessageType.LocalPublished);
						if (incident.awaitingResolutionCodes.Contains(incident.IM_ResolutionCode))
						{
							var mostRecentAwaitingLog = incident.GetMostRecentLogByEventCodes(incident.awaitingEventCodes);
							if (mostRecentAwaitingLog != null)
							{
								UpdateNRTMetric(incident, mostRecentAwaitingLog.SL_PostedTimeUtc);
							}
							if (firstSupportMsg != null)
							{
								UpdateARTMetric(incident, firstSupportMsg.JCM_PostedTimeUtc, lastNewStaffMessage.JCM_PostedTimeUtc, staffMessagesCount - 1);
							}
						}
						else
						{
							if (firstSupportMsg != null)
							{
								UpdateNRTAndARTMetric(incident, firstSupportMsg.JCM_PostedTimeUtc, lastNewStaffMessage.JCM_PostedTimeUtc, staffMessagesCount - 1);
							}
						}
					}
					var hasNewClientMessage = messages.Any(msg => msg.MessageSubType == MessageSubType.UserMessage && msg.MessageType == MessageType.Remote && !msg.IsInDatabase);
					if (hasNewClientMessage)
					{
						if (!incident.awaitingResolutionCodes.Contains(incident.IM_ResolutionCode))
						{
							var firstClientMsgAfterReopen = messages.Cast<JobConversationMessage>().TakeWhile(msg => msg.MessageType != MessageType.LocalPublished).LastOrDefault(msg => msg.MessageSubType == MessageSubType.UserMessage && msg.MessageType == MessageType.Remote);
							if (firstClientMsgAfterReopen != null)
							{
								incident.InitMetric(IncidentMetricConstants.NextResponseTime, firstClientMsgAfterReopen.JCM_PostedTimeUtc);
							}
						}
					}
				}

				FallBackForMetricsIfNeeded(incident);
			}
		}

		void DeleteExtraLog(SupportIncident incident)
		{
			var stcLogNotInDb = incident.Logs.LogsNotInDB.Where(x => x.SL_SE_NKEvent == Events.StatusChangeCode && (x.SL_Reference.StartsWith($"{SupportIncident.ChangedFieldDescription.IM_ResolutionCode} -") || x.SL_Reference.StartsWith($"{SupportIncident.ChangedFieldDescription.IM_Status} -")) && !x.SL_IsCancelled);
			if (stcLogNotInDb.Any())
			{
				var objs = stcLogNotInDb.ToArray();
				for (var i = 0; i < objs.Length; i++)
				{
					objs[i].Delete();
				}
			}
		}

		void LogFieldChanges(SupportIncident incident)
		{
			var fieldChangedEventLogger = incident.FieldChangedEventLogger;
			fieldChangedEventLogger.LogFieldChange(Events.AssignedUserChanged, incident.IM_GS_NKAssignedToCurrentInfo);
			fieldChangedEventLogger.LogFieldChange(Events.AssignedUserChanged, incident.IM_GS_NKCustServiceContactInfo);
			fieldChangedEventLogger.LogFieldChange(Events.StatusChange, incident.IM_CategoryInfo);
			fieldChangedEventLogger.LogFieldChange(Events.StatusChange, incident.IM_SourceModuleIdInfo);
			fieldChangedEventLogger.LogFieldChange(Events.StatusChange, incident.IM_StatusInfo);
			fieldChangedEventLogger.LogFieldChange(Events.StatusChange, incident.IM_ResolutionCodeInfo);
			fieldChangedEventLogger.LogFieldChange(Events.StatusChange, incident.IM_ClosureResolutionInfo);
		}

		void FallBackForMetricsIfNeeded(SupportIncident incident)
		{
			var netResolutionAgeMetric = incident.MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).FirstOrDefault(m => m.IME_MetricCode == IncidentMetricConstants.NetResolutionAge && m.IME_CalculatedMetric <= 0);
			if (incident.IM_ResolutionCode == "CLS" && netResolutionAgeMetric != null)
			{
				var totalResolutionAgeMetric = incident.MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).FirstOrDefault(m => m.IME_MetricCode == IncidentMetricConstants.TotalResolutionAge);
				netResolutionAgeMetric.IME_EndTimeUtc = totalResolutionAgeMetric.IME_EndTimeUtc;
				netResolutionAgeMetric.IME_CalculatedMetric = totalResolutionAgeMetric.IME_CalculatedMetric;
				netResolutionAgeMetric.IME_MetricCount = totalResolutionAgeMetric.IME_MetricCount;
			}
		}

		void UpdateTEAMetric(SupportIncident incident)
		{
			var incidentClosedEvent = incident.Logs.AddLog(Events.IncidentClosed);
			var incidentClosedCount = incident.Logs.Find(f => f.SL_SE_NKEvent == Events.IncidentClosedCode).Count();
			incident.MetricsCollection.RefreshFromDb();
			incident.UpdateSingleUseMetric(IncidentMetricConstants.TotalERequestAge, incidentClosedEvent.SL_PostedTimeUtc, incidentClosedCount);
		}

		void UpdateTRAAndNRAMetric(SupportIncident incident)
		{
			var incidentResolvedEvent = incident.GetMostRecentLogByEventCode(Events.IncidentResolvedCode);
			if (incidentResolvedEvent != null)
			{
				var incidentResolvedCount = incident.Logs.Find(f => f.SL_SE_NKEvent == Events.IncidentResolvedCode).Count();
				incident.UpdateSingleUseMetric(IncidentMetricConstants.TotalResolutionAge, incidentResolvedEvent.SL_PostedTimeUtc, incidentResolvedCount);
			}

			var awaitingClientTimeMetrics = incident.MetricsCollection.Find(m => m.IME_MetricCode == IncidentMetricConstants.AwaitingClientTime);
			var totalResolutionAgeMetric = incident.MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).FirstOrDefault(m => m.IME_MetricCode == IncidentMetricConstants.TotalResolutionAge);
			if (awaitingClientTimeMetrics != null && totalResolutionAgeMetric != null)
			{
				var totalAwaitingClientTime = awaitingClientTimeMetrics.Sum(m => m.IME_CalculatedMetric);
				var netResolutionAgeMetric = incident.MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).FirstOrDefault(m => m.IME_MetricCode == IncidentMetricConstants.NetResolutionAge);
				if (netResolutionAgeMetric == null)
				{
					netResolutionAgeMetric = incident.MetricsCollection.AddNew();
					netResolutionAgeMetric.IME_IncidentNumber = incident.IM_IncidentNumber;
					netResolutionAgeMetric.IME_MetricCode = IncidentMetricConstants.NetResolutionAge;
				}
				netResolutionAgeMetric.IME_StartTimeUtc = totalResolutionAgeMetric.IME_StartTimeUtc;
				netResolutionAgeMetric.IME_EndTimeUtc = totalResolutionAgeMetric.IME_EndTimeUtc;
				netResolutionAgeMetric.IME_MetricCount = totalResolutionAgeMetric.IME_MetricCount;
				netResolutionAgeMetric.IME_CalculatedMetric = totalResolutionAgeMetric.IME_CalculatedMetric - totalAwaitingClientTime;
			}
		}

		void UpdateNRTAndARTMetric(SupportIncident incident, ZDateTime? postedTimeUTC, ZDateTime? lastStaffMessageTimeUTC, int messageCount)
		{
			var lastResponseTimeMetric = incident.MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).ThenBy(m => m.IME_MetricCode).FirstOrDefault(m => m.IME_MetricCode == IncidentMetricConstants.FirstResponseTime || m.IME_MetricCode == IncidentMetricConstants.NextResponseTime || m.IME_MetricCode == IncidentMetricConstants.AdditionalResponseTime);
			var nextResponseTimeMetric = incident.MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).FirstOrDefault(m => m.IME_MetricCode == IncidentMetricConstants.NextResponseTime && m.IME_EndTimeUtc.IsEmpty);
			var incidentReopenedEvent = incident.Logs.Find(f => f.SL_SE_NKEvent == Events.IncidentReopenedCode).OrderByDescending(log => log.SL_PostedTimeUtc.IsEmpty ? log.SL_EventTimeUtc : log.SL_PostedTimeUtc).FirstOrDefault();
			if (nextResponseTimeMetric != null && incidentReopenedEvent != null && !incidentReopenedEvent.SL_PostedTimeUtc.IsEmpty)
			{
				if (nextResponseTimeMetric.IME_StartTimeUtc == ZDateTime.Empty)
				{
					nextResponseTimeMetric.IME_StartTimeUtc = incidentReopenedEvent.SL_PostedTimeUtc;
				}
				nextResponseTimeMetric.IME_EndTimeUtc = postedTimeUTC.Value;
				nextResponseTimeMetric.IME_CalculatedMetric = (int)(nextResponseTimeMetric.IME_EndTimeUtc - nextResponseTimeMetric.IME_StartTimeUtc).TotalSeconds;
				nextResponseTimeMetric.IME_MetricCount = 1;
				if (messageCount != 0)
				{
					lastResponseTimeMetric = incident.MetricsCollection.AddNew();
					lastResponseTimeMetric.IME_IncidentNumber = incident.IM_IncidentNumber;
					lastResponseTimeMetric.IME_MetricCode = IncidentMetricConstants.AdditionalResponseTime;
					lastResponseTimeMetric.IME_StartTimeUtc = postedTimeUTC.Value;
				}
			}
			else if (lastResponseTimeMetric != null && lastResponseTimeMetric.IME_MetricCode != IncidentMetricConstants.AdditionalResponseTime && messageCount != 0)
			{
				lastResponseTimeMetric = incident.MetricsCollection.AddNew();
				lastResponseTimeMetric.IME_IncidentNumber = incident.IM_IncidentNumber;
				lastResponseTimeMetric.IME_MetricCode = IncidentMetricConstants.AdditionalResponseTime;
				lastResponseTimeMetric.IME_StartTimeUtc = postedTimeUTC.Value;
			}
			if (lastResponseTimeMetric != null)
			{
				UpdateARTMetric(lastResponseTimeMetric, lastStaffMessageTimeUTC, messageCount);
			}
		}

		static void UpdateARTMetric(IncidentMetrics additionalResponseTimeMetric, ZDateTime? lastStaffMessageTimeUTC, int messageCount)
		{
			if (messageCount != 0)
			{
				additionalResponseTimeMetric.IME_EndTimeUtc = lastStaffMessageTimeUTC.Value;
				additionalResponseTimeMetric.IME_CalculatedMetric = (int)(additionalResponseTimeMetric.IME_EndTimeUtc - additionalResponseTimeMetric.IME_StartTimeUtc).TotalSeconds;
				additionalResponseTimeMetric.IME_MetricCount = messageCount;
			}
		}

		void UpdateARTMetric(SupportIncident incident, ZDateTime? mostRecentAwaitingLogTime, ZDateTime? lastStaffMessageTime, int messageCount)
		{
			var lastResponseTimeMetric = incident.MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).ThenBy(m => m.IME_MetricCode).FirstOrDefault(m => m.IME_MetricCode == IncidentMetricConstants.FirstResponseTime || m.IME_MetricCode == IncidentMetricConstants.NextResponseTime || m.IME_MetricCode == IncidentMetricConstants.AdditionalResponseTime);
			if (lastResponseTimeMetric != null && lastResponseTimeMetric.IME_MetricCode != IncidentMetricConstants.AdditionalResponseTime && messageCount != 0)
			{
				lastResponseTimeMetric = incident.MetricsCollection.AddNew();
				lastResponseTimeMetric.IME_IncidentNumber = incident.IM_IncidentNumber;
				lastResponseTimeMetric.IME_MetricCode = IncidentMetricConstants.AdditionalResponseTime;
				lastResponseTimeMetric.IME_StartTimeUtc = mostRecentAwaitingLogTime.Value;
			}
			if (lastResponseTimeMetric != null)
			{
				UpdateARTMetric(lastResponseTimeMetric, lastStaffMessageTime, messageCount);
			}
		}

		void UpdateNRTMetricStartTime(SupportIncident incident, StmALog reopenEvent)
		{
			var metric = incident.MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).FirstOrDefault(m => m.IME_MetricCode == IncidentMetricConstants.NextResponseTime);
			if (metric != null && metric.IME_StartTimeUtc == ZDateTime.Empty)
			{
				metric.IME_StartTimeUtc = reopenEvent.SL_PostedTimeUtc;
			}
		}

		void UpdateNRTMetric(SupportIncident incident, ZDateTime postedTimeUTC)
		{
			var metric = incident.MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).FirstOrDefault(m => m.IME_MetricCode == IncidentMetricConstants.NextResponseTime && m.IME_EndTimeUtc.IsEmpty);
			var incidentReopenedEvent = incident.Logs.Find(f => f.SL_SE_NKEvent == Events.IncidentReopenedCode).OrderByDescending(log => log.SL_PostedTimeUtc.IsEmpty ? log.SL_EventTimeUtc : log.SL_PostedTimeUtc).FirstOrDefault();
			if (metric != null && incidentReopenedEvent != null && !incidentReopenedEvent.SL_PostedTimeUtc.IsEmpty)
			{
				if (metric.IME_StartTimeUtc == ZDateTime.Empty)
				{
					metric.IME_StartTimeUtc = incidentReopenedEvent.SL_PostedTimeUtc;
				}
				metric.IME_EndTimeUtc = postedTimeUTC;
				metric.IME_CalculatedMetric = (int)(metric.IME_EndTimeUtc - metric.IME_StartTimeUtc).TotalSeconds;
				metric.IME_MetricCount = 1;
			}
		}

		void InitTDTMetric(SupportIncident incident)
		{
			var relatedWorkItems = incident.RelatedWorkItems;
			var resolvedEvent = incident.Logs.Find(f => f.SL_SE_NKEvent == Events.IncidentResolvedCode).OrderByDescending(log => log.SL_PostedTimeUtc.IsEmpty ? log.SL_EventTimeUtc : log.SL_PostedTimeUtc).FirstOrDefault();
			var attachedEvent = incident.Logs.Find(f => f.SL_SE_NKEvent == Events.AttachedCode && (resolvedEvent == null || f.SL_PostedTimeUtc > resolvedEvent.SL_PostedTimeUtc) && relatedWorkItems.Any(wki => f.SL_Reference.Contains(((NewWorkItem)wki).WKI_WorkItemNumber))).OrderBy(log => log.SL_PostedTimeUtc.IsEmpty ? log.SL_EventTimeUtc : log.SL_PostedTimeUtc);
			var totalDevelopmentTimeMetric = incident.MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).FirstOrDefault(m => m.IME_MetricCode == IncidentMetricConstants.TotalDevelopmentTime && m.IME_EndTimeUtc.IsEmpty);
			if (totalDevelopmentTimeMetric == null && attachedEvent != null && attachedEvent.Any())
			{
				totalDevelopmentTimeMetric = incident.MetricsCollection.AddNew();
				totalDevelopmentTimeMetric.IME_IncidentNumber = incident.IM_IncidentNumber;
				totalDevelopmentTimeMetric.IME_MetricCode = IncidentMetricConstants.TotalDevelopmentTime;
				totalDevelopmentTimeMetric.IME_StartTimeUtc = attachedEvent.FirstOrDefault().SL_PostedTimeUtc;
				var totalOtherDevelopmentTimeCount = incident.MetricsCollection.Where(m => m.IME_MetricCode == IncidentMetricConstants.TotalDevelopmentTime && m != totalDevelopmentTimeMetric).Sum(m => m.IME_MetricCount);
				if (totalDevelopmentTimeMetric != null && attachedEvent != null)
				{
					totalDevelopmentTimeMetric.IME_MetricCount = relatedWorkItems.Count - totalOtherDevelopmentTimeCount;
				}
			}
		}

		void UpdateTDTMetric(SupportIncident incident)
		{
			var totalDevelopmentTimeMetric = incident.MetricsCollection.OrderBy(m => m.IME_SystemCreateTimeUtc).LastOrDefault(m => m.IME_MetricCode == IncidentMetricConstants.TotalDevelopmentTime);
			if (totalDevelopmentTimeMetric is null)
			{
				return;
			}

			var incidentResolvedEvents = incident.GetLogsByEventCode(Events.IncidentResolvedCode);
			var attachedEvent = incident.GetMostRecentLogByEventCode(Events.AttachedCode);

			var lastMatchingEvent = incidentResolvedEvents?.Where(e => e.SL_PostedTimeUtc > attachedEvent?.SL_PostedTimeUtc).OrderByDescending(e => e.SL_PostedTimeUtc).LastOrDefault();
			if (lastMatchingEvent is null)
			{
				return;
			}

			totalDevelopmentTimeMetric.IME_EndTimeUtc = lastMatchingEvent.SL_PostedTimeUtc;
			totalDevelopmentTimeMetric.IME_CalculatedMetric = (int)(totalDevelopmentTimeMetric.IME_EndTimeUtc - totalDevelopmentTimeMetric.IME_StartTimeUtc).TotalSeconds;
		}

		void UpdateTDTMetricWithDTCEvent(SupportIncident incident, StmALog mostEarliestATCEvent)
		{
			var incidentResolvedEvent = incident.GetMostRecentLogByEventCode(Events.IncidentResolvedCode);
			if (mostEarliestATCEvent != null && incidentResolvedEvent != null)
			{
				var totalDevelopmentTimeMetric = incident.MetricsCollection.OrderBy(m => m.IME_SystemCreateTimeUtc).LastOrDefault(m => m.IME_MetricCode == IncidentMetricConstants.TotalDevelopmentTime);
				if (totalDevelopmentTimeMetric != null)
				{
					totalDevelopmentTimeMetric.IME_StartTimeUtc = mostEarliestATCEvent.SL_PostedTimeUtc;
					totalDevelopmentTimeMetric.IME_EndTimeUtc = incidentResolvedEvent.SL_PostedTimeUtc;
					totalDevelopmentTimeMetric.IME_CalculatedMetric = (int)(totalDevelopmentTimeMetric.IME_EndTimeUtc - totalDevelopmentTimeMetric.IME_StartTimeUtc).TotalSeconds;
				}
			}
		}

		public StmALog GetMostEarliestATCLog(SupportIncident incident, string excludeReference)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, incident.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.AttachedCode);
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Ascending;
			var mostEarliestLog = incident.Factory.Load<StmALog>(query).FirstOrDefault(log => !log.SL_Reference.Contains(excludeReference));
			return mostEarliestLog;
		}

		public StmALog[] GetDTCEvents(SupportIncident incident)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, incident.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DetachedCode);
			return incident.Factory.Load<StmALog>(query);
		}
	}
}
