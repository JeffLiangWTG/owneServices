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
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"FRQ",
	"Feature Request Quotation Auto Expire Service",
	"CSP",
	typeof(Enterprise.Client.EDI.IncidentManager.BatchProcessor.FeatureRequestQuotationAutoExpireServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Minutes",
	DefaultScheduleRunEvery = "1hour",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true)
]

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	public class FeatureRequestQuotationAutoExpireServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			var incidents = GetIncidents();

			foreach (var incident in incidents)
			{
				token.ThrowIfCancellationRequested();
				try
				{
					SupportIncident loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
					var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						ProcessIncident(loadedIncident);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture,
																	"Failed to process incident {0}. Exception Details : {1}",
																	incident.IM_IncidentNumber,
																	ex.ToString()));
					ErrorReporter.ReportOnce("Failed to process Feature Request Incident", FormattableString.Invariant($"{incident.IM_IncidentNumber} {ex.Message}"), ex);
				}
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		IEnumerable<SupportIncident> GetIncidents()
		{
			var filter = new ZQuery(IncidentMainSchema.IM_IncidentType, IncidentConstants.IncidentType.SupportIncident);
			filter.AddToFilter(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed);
			filter.AddToFilter(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided);
			return new BusinessObjectFactory().Load<SupportIncident>(filter);
		}

		void ProcessIncident(SupportIncident incident)
		{
			var autoDeclinePeriod = EDIDataRegistry.Instance.FeatureRequestQuotationAutoExpirePeriod.Value;

			var dispositionChangeLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode);
			dispositionChangeLogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.EndsWith, SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided);
			dispositionChangeLogQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			var lastQuotationProvidedTime = incident.Logs.Find(dispositionChangeLogQuery)[0].SL_PostedTimeUtc;

			if (lastQuotationProvidedTime.AddDays(autoDeclinePeriod) <= ZDateTime.UtcNow)
			{
				incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
				IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.FormalQuotationExpired, incident);
				incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.FormalQuotationExpired;
				incident.IM_Status = incident.WorkflowItems.AllTasksClosedOrCancelled ? SupportIncidentLookups.Status.Closed : SupportIncidentLookups.Status.Working;

				var logText = string.Format("No action from client for {0} days - quotation is expired", autoDeclinePeriod);
				incident.AddInternalMessage(logText);

				incident.Factory.Save();
				ServiceLogger.Log(LogType.Information, string.Format("Processed incident {0}. {1}.", incident.IM_IncidentNumber, logText));
			}
		}
	}
}
