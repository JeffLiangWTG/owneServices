using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"IRP",
	"Incident Resolved Processor",
	"CSP",
	typeof(Enterprise.Client.EDI.IncidentManager.BatchProcessor.IncidentResolvedServiceTask),
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1hour",
	CanRunInAnyBranch = true)
]

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	[NeedsDataRefresh]
	class IncidentResolvedServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			var factory = new BusinessObjectFactory();
			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(factory);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var incidentsNeedToClosed = GetResolvedIncidents(factory);
				foreach (var incident in incidentsNeedToClosed)
				{
					token.ThrowIfCancellationRequested();
					try
					{
						using (incident.SetTempContext(SupportIncident.Context.InIncidentResolvedServiceTask))
						{
							ProcessIncident(incident);
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture,
																		"Failed to process incident {0}. Exception Details : {1}",
																		incident.IM_IncidentNumber,
																		ex.ToString()));
						ErrorReporter.ReportOnce("Failed to process Incident", FormattableString.Invariant($"{incident.IM_IncidentNumber} {ex.Message}"), ex);
					}
				}

				var incidentsNeedToEscalated = GetNeedToAutoEscalationIncidents(factory);
				foreach (var incident in incidentsNeedToEscalated)
				{
					token.ThrowIfCancellationRequested();
					try
					{
						incident.Escalate(SupportIncidentCategoriesList.Codes.ContentDevelopment, "Auto-escalated to content stage by IRP service task.");
						incident.Factory.Save();
						ServiceLogger.Log(LogType.Information, string.Format("Incident {0} escalate success.", incident.IM_IncidentNumber));
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture,
																		"Failed to escalate incident {0}. Exception Details : {1}",
																		incident.IM_IncidentNumber,
																		ex.ToString()));
						ErrorReporter.ReportOnce("Failed to escalate Incident", FormattableString.Invariant($"{incident.IM_IncidentNumber} {ex.Message}"), ex);
					}
				}
			}
		}

		IEnumerable<SupportIncident> GetResolvedIncidents(BusinessObjectFactory resolvedIncidentFactory)
		{
			var filter = new ZQuery(IncidentMainSchema.IM_IncidentType, IncidentConstants.IncidentType.SupportIncident);
			filter.AddToFilter(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.Resolved);
			filter.AddToFilter(IncidentMainSchema.IM_ResolveTimeUtc, SQLComparisonOperator.NotEqual, null);
			return resolvedIncidentFactory.Load<SupportIncident>(filter);
		}

		IEnumerable<SupportIncident> GetNeedToAutoEscalationIncidents(BusinessObjectFactory needToEscalatedIncidentFactory)
		{
			var filter = new ZQuery(IncidentMainSchema.IM_IncidentType, IncidentConstants.IncidentType.SupportIncident);
			filter.AddToFilter(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed);
			filter.AddToFilter(IncidentMainSchema.IM_ClosureResolution, SupportIncidentLookups.DispositionList.Constants.Closed.TrainingFlaggedForContentDevelopment);
			filter.AddToFilter(IncidentMainSchema.IM_Category, SQLComparisonOperator.NotEqual, SupportIncidentCategoriesList.Codes.ContentDevelopment);
			return needToEscalatedIncidentFactory.Load<SupportIncident>(filter);
		}

		void ProcessIncident(SupportIncident incident)
		{
			var isProcessed = false;
			var daysResolvedToClosed = incident.GetResolutionAndClosureBehaviour().DaysResolvedToClosed;
			var resolvedTime = incident.IM_ResolveTimeUtc;
			var expiredTime = resolvedTime.AddDays(daysResolvedToClosed);

			if (ZDateTime.Now >= expiredTime)
			{
				incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
				incident.AddStatusUpdatedEvent(IncidentConstants.LogFreeText.CloseIncidentFromResolvedByIRPLog);
				isProcessed = true;
			}

			if (isProcessed)
			{
				incident.Factory.Save();
				ServiceLogger.Log(LogType.Information, string.Format("Processed incident {0}.", incident.IM_IncidentNumber));
			}
		}
	}
}
