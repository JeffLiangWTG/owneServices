using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Nest;
using ZClientEDI.ElasticSearchAdaptor;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert
{
	public class SqlCpuUsageAlertProvider : BaseExternalAlertProvider
	{
		public SqlCpuUsageAlertProvider(ILogger serviceLogger, DbConnection connection, IEnumerable<AlertDefault> defaults, ISqlExecutionPlanRetriever sqlExecutionPlanRetriever = null) : base(serviceLogger, connection, defaults)
		{
			sqlCpuUsageExecutionPlanUri = EDIDataRegistry.Instance.ExternalMonitoringSqlCpuUsageExecutionPlanUri.Value;
			if (sqlCpuUsageExecutionPlanUri.IndexOf("WtgQueryHash") < 0)
			{
				sqlCpuUsageExecutionPlanUri = sqlCpuUsageExecutionPlanUri.Replace("QueryHash", "WtgQueryHash");
			}

			SqlExecutionPlanRetriever = sqlExecutionPlanRetriever ?? new SqlExecutionPlanRetriever(new HttpClient() { Timeout = TimeSpan.FromMilliseconds(EDIDataRegistry.Instance.ExternalMonitoringSqlExecutionPlanRetrieveTimeout.Value) });
		}

		public override string AlertTypeName => "SQL CPU Usage";

		public override string AlertTargetName => "workitem(s)";

		public override AlertRuleType RuleType => AlertRuleType.SqlCpuUsage;

		internal const string MaxiumNumberOfCpuUsageAlertPerDayPerSystemName = "MaxiumNumberOfCpuUsageAlertPerDayPerSystem";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public override void UpdateAlert(BaseAlert alert, string targetNumber, string owner)
		{
			if (!(alert is SqlCpuUsageAlert sqlAlert))
			{
				throw new ArgumentException("Argument 'alert' is expected to be of type SqlCpuUsageAlert");
			}

			using (var connection = AlertUtils.GetDbConnection(EDIDataRegistry.Instance.ExternalMonitoringWiseGridReportingConnectionString.Value))
			{
				using (var updateAlertCommand = connection.Command("EXEC dbo.usp_UpdateAlertAmnesty @System, @Key, @Expiry"))
				{
					updateAlertCommand.AddParameter("@System", SqlDbType.NVarChar, sqlAlert.AlertName);
					updateAlertCommand.AddParameter("@Key", SqlDbType.NVarChar, sqlAlert.Path);
					updateAlertCommand.AddParameter("@Expiry", SqlDbType.DateTime, DBNull.Value);
					updateAlertCommand.ExecuteNonQuery();
				}
			}
		}

		public override AlertRule DetermineAlertRule(IList<AlertRule> alertRules, BaseAlert alert)
		{
			var cpuUsageAlert = alert as SqlCpuUsageAlert;
			var matchedRule = alertRules.FirstOrDefault(x =>
				x.AlertName.Equals(AlertNameExpensiveSQL, StringComparison.OrdinalIgnoreCase)
				&& x.Path.Equals(cpuUsageAlert.AlertName, StringComparison.OrdinalIgnoreCase)
			);
			return matchedRule;
		}

		protected internal const string AlertNameExpensiveSQL = "EXPENSIVE_SQL";

		public override (
			AddAlertTargetResult result, BusinessObject parent, string descriptivePath, ProcessTask task
			) FindAlertTargetCreateIfNotExists(
			BaseAlert alert, AlertRule matchedRule, BusinessObjectFactory boFactory, ZGuid glbCapabilityId, Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)> capabilityJobsList)
		{
			var addTargetResult = AddAlertTargetResult.None;
			if (alert is SqlCpuUsageAlert cpuUsageAlert)
			{
				var capabilityJobs = capabilityJobsList.FirstOrDefault(c => string.Equals(c.Key, matchedRule.Capability, StringComparison.OrdinalIgnoreCase)).Value;
				var openWorkItemSummaries = capabilityJobs.Item3;

				if (openWorkItemSummaries.Count() >= capabilityJobs.Item1 * 2
					|| cpuUsageAlert.AffectedClient == "ALL" && openWorkItemSummaries.Count() >= capabilityJobs.Item1)
				{
					addTargetResult = AddAlertTargetResult.ReachedMaxAmount;
				}
				else if (openWorkItemSummaries.Any(x => StartsWithSameAlertTypeAndEndsWithSameQueryHash(x, alert.Path))
					|| (cpuUsageAlert.CurrentVersion != null && !CheckLatestWIIsPatchedToReleaseVersion(boFactory, alert.Path, glbCapabilityId, new Version(cpuUsageAlert.CurrentVersion))))
				{
					addTargetResult = AddAlertTargetResult.AlreadyCreatedRecently;
				}
				else
				{
					var workItem = CreateWorkItemAndPivot(cpuUsageAlert, matchedRule, boFactory, glbCapabilityId);
					var list = capabilityJobs.Item3.ToList();
					list.Add(workItem.WKI_Summary);
					capabilityJobs.Item3 = list;
					capabilityJobsList[matchedRule.Capability] = capabilityJobs;
					return (AddAlertTargetResult.TargetAdded, workItem, ProcessTaskInvestigation, null);
				}
			}

			return (addTargetResult, null, null, null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		bool CheckLatestWIIsPatchedToReleaseVersion(BusinessObjectFactory boFactory, string queryHash, ZGuid capabilityId, Version currentVersion)
		{
			var workItemQuery = new ZDBOnlyQuery(typeof(NewWorkItem));
			workItemQuery.AddToFilter(WorkItemSchema.WKI_Status, SQLComparisonOperator.Equal, new[]
				{
					ProcessTaskStatusCodeList.Codes.Cancelled,
					ProcessTaskStatusCodeList.Codes.Closed
				});
			workItemQuery.AddToFilter(WorkItemSchema.WKI_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThan, DateTime.UtcNow.AddMonths(-3));
			workItemQuery.AddToFilter(WorkItemSchema.WKI_Summary, SQLComparisonOperator.EndsWith, queryHash);

			var capabilityQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation1ID);
			capabilityQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, capabilityId);
			capabilityQuery.AddToFilter(GenPivotSchema.XX_RelationType, AlertPivotType);

			workItemQuery.AddSubQuery(capabilityQuery, JoinCondition.And);

			var latestWI = boFactory.Load<NewWorkItem>(workItemQuery)
				.OrderByDescending(w => w.WKI_WorkItemNumber).FirstOrDefault();
			if (latestWI != null)
			{
				return GetReleaseBuildContent().IsPatchedTo(latestWI, currentVersion) ||
					!latestWI.HasClosedCheckInTasks &&
					ProcessJobHeader.GetForParent(latestWI, boFactory).GetPrerequisitesUpTheTree().Where(h => h.Parent is NewWorkItem wi && wi.HasClosedCheckInTasks).All(h => GetReleaseBuildContent().IsPatchedTo(h.Parent as NewWorkItem, currentVersion));
			}
			return true;
		}

		protected virtual IReleaseBuildContent GetReleaseBuildContent()
		{
			if (releaseBuildContent == null)
			{
				releaseBuildContent = ReleaseBuildContent.New(new BusinessObjectFactory());
			}
			return releaseBuildContent;
		}
		IReleaseBuildContent releaseBuildContent;

		bool StartsWithSameAlertTypeAndEndsWithSameQueryHash(ZString workItemSummary, string queryHash)
		{
			return !workItemSummary.IsEmpty && workItemSummary.StartsWith(AlertTypeName, StringComparison.OrdinalIgnoreCase) && workItemSummary.EndsWith(queryHash, StringComparison.OrdinalIgnoreCase);
		}

		IEnumerable<ZString> GetListOfOpenWorkItemSummariesForCapability(ZGuid glbCapabilityId, BusinessObjectFactory boFactory)
		{
			var workItemQuery = new ZDBOnlyQuery(typeof(WorkItem));
			workItemQuery.AddToFilter(WorkItemSchema.WKI_Status, SQLComparisonOperator.NotEqual, new[]
				{
					ProcessTaskStatusCodeList.Codes.Cancelled,
					ProcessTaskStatusCodeList.Codes.Closed
				});

			var capabilityQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation1ID);
			capabilityQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, glbCapabilityId);
			capabilityQuery.AddToFilter(GenPivotSchema.XX_RelationType, AlertPivotType);

			workItemQuery.AddSubQuery(capabilityQuery, JoinCondition.And);

			return boFactory.Load<WorkItem>(workItemQuery).Select(x => x.WKI_Summary);
		}

		WorkItem CreateWorkItemAndPivot(SqlCpuUsageAlert alert, AlertRule matchedRule, BusinessObjectFactory boFactory, ZGuid glbCapabilityId)
		{
			var sqlExecutionPlanTask = RetrieveSqlExecutionPlan(alert);
			var workItem = boFactory.New<WorkItem>();
			workItem.WKI_Summary = GenerateSummary(alert);
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			workItem.WKI_WorkItemType = matchedRule.Product;
			workItem.WKI_WorkItemArea = matchedRule.ProgramArea;
			workItem.WKI_ActivityType = matchedRule.Module;
			workItem.WKI_ActivitySubtype = matchedRule.ChangeType;
			workItem.WKI_Priority = matchedRule.Priority;

			const int tableCellMargin = 100;
			const int tableCellBorder = 15;
			const int table1Column1Width = 2000;
			const int table1Column2Width = 7000;
			const int table2Column1Width = 2000;
			const int table2Column2Width = 5000;

			var detailsTemplate = FormattableString.Invariant($@"{{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081
{{\fonttbl
	{{\f0\fswiss\fprq2\fcharset0 Microsoft Sans Serif;}}
	{{\f1\fswiss\fprq2\fcharset0 Calibri;}}
	{{\f2\fnil Microsoft Sans Serif;}}
}}
{{\colortbl;\red0\green0\blue255;\red5\green99\blue193;}}

\viewkind4\uc1\pard\widctlpar\b\f0\fs20{{ Details}}\par

\trowd\trgaph{tableCellMargin}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table1Column1Width}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table1Column2Width}
\pard\intbl\widctlpar\b0 WTG Query Hash\b\cell
{{\b0\f1\fs22{{\field{{\*\fldinst{{ HYPERLINK ""{sqlCpuUsageExecutionPlanUri.Replace("{0}", alert.Path)}""}}}}{{\fldrslt{{\ul\cf1\cf2\ul\f0\fs20{{{alert.Path}}}}}}}}}}}\f0\fs20\cell
\row
\trowd\trgaph{tableCellMargin}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table1Column1Width}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table1Column2Width}
\pard\intbl\widctlpar\b0 Product\b\cell
\b0 {alert.Owner}\b\cell
\row
\trowd\trgaph{tableCellMargin}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table1Column1Width}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table1Column2Width}
\pard\intbl\widctlpar\b0 System\b\cell
\b0 {alert.AlertName}\b\cell
\row
\trowd\trgaph{tableCellMargin}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table1Column1Width}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table1Column2Width}
\pard\intbl\widctlpar\b0 Time Collected\b\cell
\b0 {new ZDateTime(alert.TimeAdded).ToString("u", CultureInfo.InvariantCulture)}\b\cell
\row
\trowd\trgaph{tableCellMargin}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table1Column1Width}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table1Column2Width}
\pard\intbl\widctlpar\b0 Rule Id\b\cell
\b0 {matchedRule.PK}\b\cell
\row
\trowd\trgaph{tableCellMargin}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table1Column1Width}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table1Column2Width}
\pard\intbl\widctlpar\b0 Affected Client\b\cell
\b0 {alert.AffectedClient}\b\cell
\row

\pard\widctlpar\b0\par\b{{ Statistics}}\par

\trowd\trgaph{tableCellMargin}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table2Column1Width}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table2Column2Width}
\pard\intbl\widctlpar\b0 Occurrences\cell 
\pard\intbl\widctlpar\qr {alert.Quantity:N0}\cell
\row
\trowd\trgaph{tableCellMargin}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table2Column1Width}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table2Column2Width}
\pard\intbl\widctlpar CPU Time (ms)\cell
\pard\intbl\widctlpar\qr {alert.CpuMilliSeconds:N0}\cell
\row
\trowd\trgaph{tableCellMargin}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table2Column1Width}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table2Column2Width}
\pard\intbl\widctlpar Reads\cell 
\pard\intbl\widctlpar\qr {alert.Reads:N0}\cell
\row
\trowd\trgaph{tableCellMargin}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table2Column1Width}
\clbrdrt\brdrw{tableCellBorder}\brdrs\clbrdrl\brdrw{tableCellBorder}\brdrs\clbrdrb\brdrw{tableCellBorder}\brdrs\clbrdrr\brdrw{tableCellBorder}\brdrs\cellx{table2Column2Width}
\pard\intbl\widctlpar Writes\cell
\pard\intbl\widctlpar\qr {alert.Writes:N0}\cell
\row

\pard\widctlpar\par
\b \ul Help\par
\ulnone\b0 {{\f1\fs22{{\field{{\*\fldinst{{HYPERLINK ""https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/CPUUsageDashboard.aspx"" }}}}{{\fldrslt{{\ul\cf1\cf2\ul\f0\fs20Working with kibana}}}}}}}}\f0\fs20\par
{{\f1\fs22{{\field{{\*\fldinst{{HYPERLINK ""https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/SQL%20Query%20Performance%20Tuning.aspx"" }}}}{{\fldrslt{{\ul\cf1\cf2\ul\f0\fs20SQL query performance tuning}}}}}}}}\f0\fs20\par
\pard\widctlpar\b0\par\b{{If this issue is duplicate or is addressed in another WI please add it as prerequisite to this WI and keep this WI open until the prerequisite is closed.}}\par
\pard\f2\lang1033\par
\line
}}");

			workItem.WKI_Details = ZBlob.FromUTF8(detailsTemplate);
			workItem.ApplyWorkflowTemplates();

			var capabilityPivot = boFactory.New<GenPivot>();
			capabilityPivot.XX_Relation1ID = workItem.PK;
			capabilityPivot.XX_Relation2ID = glbCapabilityId;
			capabilityPivot.XX_Relation1TableCode = WorkItemSchema.Constants.Prefix;
			capabilityPivot.XX_Relation2TableCode = GlbCapabilitySchema.Constants.Prefix;
			capabilityPivot.XX_RelationType = AlertPivotType;

			var sqlExecutionPlan = sqlExecutionPlanTask.GetAwaiter().GetResult();
			if (!string.IsNullOrEmpty(sqlExecutionPlan))
			{
				var contents = Encoding.ASCII.GetBytes(sqlExecutionPlan);
				var eDoc = workItem.DocManagerInfo.AddFileOrDocument(contents, FileNameForSqlExecutionPlan(alert), "LTR");
				eDoc.Description = "The execution plan of this SQL CPU Usage alert.";
				workItem.DocManagerInfo.Save();
			}

			return workItem;
		}

		string FileNameForSqlExecutionPlan(SqlCpuUsageAlert alert)
		{
			if (string.IsNullOrEmpty(alert.ServerInstanceName))
			{
				return "SQL Execution Plan.sqlplan";
			}
			var instanceNameWithoutIllegalChars = alert.ServerInstanceName.Replace('\\', '_');
			return $"SQL Execution Plan - {instanceNameWithoutIllegalChars}.sqlplan";
		}

		public ISqlExecutionPlanRetriever SqlExecutionPlanRetriever { get; }

		async Task<string> RetrieveSqlExecutionPlan(SqlCpuUsageAlert alert)
		{
			var serverInstanceName = alert.ServerInstanceName ?? string.Empty;
			var queryHash = alert.QueryHash;
			var queryHashSearchLinkTemplate = EDIDataRegistry.Instance.ExternalMonitoringSqlExecutionPlanQueryUri.Value;
			if (string.IsNullOrEmpty(queryHash) || string.IsNullOrEmpty(queryHashSearchLinkTemplate) || queryHash.Equals("0"))
			{
				return string.Empty;
			}

			var queryHashSearchLink = string.Format(queryHashSearchLinkTemplate, queryHash, serverInstanceName);
			try
			{
				return await SqlExecutionPlanRetriever.RetrieveExecutionPlan(queryHashSearchLink).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				serviceLogger.Error($"Failed to retrieve SQL execution plan from: {queryHashSearchLink}, QueryHash: {queryHash}, WtgQueryHash: {alert.Path}", ex);
				return string.Empty;
			}
		}

		ZString GenerateSummary(SqlCpuUsageAlert alert)
		{
			// need to ensure the query has is at the end of the summary and does not get truncated as it will affect duplicate checking.
			var maxSummaryPrefixLength = AutoWorkItem.Schema.WKI_SummaryMaxLength - alert.Path.Length - 3;
			var summaryPrefix = FormattableString.Invariant($"{AlertTypeName} alert for {alert.AlertName}");
			var summaryPrefixTruncated = summaryPrefix.Substring(0, (summaryPrefix.Length < maxSummaryPrefixLength) ? summaryPrefix.Length : maxSummaryPrefixLength);
			return new ZString(FormattableString.Invariant($"{summaryPrefixTruncated} - {alert.Path}"));
		}

		const string AlertPivotType = "DBP";
		protected string sqlCpuUsageExecutionPlanUri;

		public override string GetTargetKey(BusinessObject alertTarget) => ((WorkItem)alertTarget).WKI_WorkItemNumber;

		int GetMaxAlertPerDayPerSystem()
		{
			var maxiumNumberOfCpuUsageAlertPerDayPerSystemSetting = AlertDefaults.FirstOrDefault(x => x.Name.Equals(MaxiumNumberOfCpuUsageAlertPerDayPerSystemName, StringComparison.OrdinalIgnoreCase));

			if (
				maxiumNumberOfCpuUsageAlertPerDayPerSystemSetting == null
				|| !int.TryParse(maxiumNumberOfCpuUsageAlertPerDayPerSystemSetting.Value, out var max)
				|| max <= 0
			)
			{
				// If max alerts per day not set in AlertDefaults table,
				// use the highest capability user count
				max = CapabilityCounts.Max(x => x.Value);
			}
			else
			{
				max = Convert.ToInt32(maxiumNumberOfCpuUsageAlertPerDayPerSystemSetting.Value, CultureInfo.InvariantCulture);
			}

			return max;
		}

		int MaxiumNumberOfCpuUsageAlertPerDayPerSystem
		{
			get
			{
				if (maxiumNumberOfCpuUsageAlertPerDayPerSystem < 0)
				{
					maxiumNumberOfCpuUsageAlertPerDayPerSystem = GetMaxAlertPerDayPerSystem();
				}
				return maxiumNumberOfCpuUsageAlertPerDayPerSystem;
			}
		}
		int maxiumNumberOfCpuUsageAlertPerDayPerSystem = -1;

		public override void GetAlertAndProcess(IEnumerable<string> capabilitiesNotFull, Func<BaseAlert, bool> processAlertAction)
		{
			foreach (var alert in GetAlerts(capabilitiesNotFull, serviceLogger))
			{
				if (processAlertAction(alert))
				{
					break;
				}
			}
		}

		IEnumerable<SqlCpuUsageAlert> GetAlerts(IEnumerable<string> capabilitiesNotFull, ILogger serviceLogger)
		{
			var aggregations = GetResponse(capabilitiesNotFull);

			return ConvertToAlert(aggregations, MaxiumNumberOfCpuUsageAlertPerDayPerSystem, serviceLogger);
		}

		IEnumerable<SqlCpuUsageAlert> ConvertToAlert(IEnumerable<AggregateDictionary> aggregations, int maxiumNumberOfCpuUsageAlertPerDayPerSystem, ILogger serviceLogger)
		{
			foreach (var aggregation in aggregations)
			{
				var topLevelAggregationBuckets = aggregation?.Terms("topLevelAggregation")?.Buckets;
				if (topLevelAggregationBuckets?.Any() != true)
				{
					yield break;
				}
				foreach (var systemBucket in topLevelAggregationBuckets)
				{
					var systemId = systemBucket.Key;
					foreach (var ownerBucket in systemBucket.Terms("SubAggregation1")?.Buckets)
					{
						var owner = ownerBucket.Key;
						var clientBuckets = ownerBucket.Terms("SubAggregationClient")?.Buckets;
						if (clientBuckets != null)
						{
							foreach (var clientBucket in clientBuckets)
							{
								var affectedClient = clientBucket.Key;
								foreach (var alert in HandleQueryHashBuckets(clientBucket.Terms("SubAggregation2")?.Buckets, affectedClient))
								{
									yield return alert;
								}
							}
						}
						else
						{
							foreach (var alert in HandleQueryHashBuckets(ownerBucket.Terms("SubAggregation2")?.Buckets, "ALL"))
							{
								yield return alert;
							}
						}

						IEnumerable<SqlCpuUsageAlert> HandleQueryHashBuckets(IReadOnlyCollection<KeyedBucket<string>> queryHashBuckets, string affectedClient)
						{
							var queryHashCount = 0;
							foreach (var queryHashBucket in queryHashBuckets)
							{
								queryHashCount++;
								if (queryHashCount > maxiumNumberOfCpuUsageAlertPerDayPerSystem)
								{
									break;
								}
								var queryHashId = queryHashBucket.Key;

								var cpuUsageAlert = new SqlCpuUsageAlert();
								cpuUsageAlert.AlertName = systemId;
								cpuUsageAlert.Path = queryHashId;
								cpuUsageAlert.TimeAdded = queryHashBucket.Max("8")?.Value?.FromUnixTime();
								cpuUsageAlert.Quantity = Convert.ToInt64(queryHashBucket.Sum("5")?.Value, CultureInfo.InvariantCulture);
								cpuUsageAlert.Reads = Convert.ToInt64(queryHashBucket.Sum("6")?.Value, CultureInfo.InvariantCulture);
								cpuUsageAlert.Writes = Convert.ToInt64(queryHashBucket.Sum("7")?.Value, CultureInfo.InvariantCulture);
								cpuUsageAlert.CpuMilliSeconds = Convert.ToInt64(queryHashBucket.Sum("AggCPUTimeMS2")?.Value, CultureInfo.InvariantCulture);
								cpuUsageAlert.ExeDate = queryHashBucket.Max("10")?.Value?.FromUnixTime();
								cpuUsageAlert.Owner = owner;
								cpuUsageAlert.CurrentVersion = ElasticSearchAdaptor.ConvertDoubleToVersionString(queryHashBucket.Max("11")?.Value, serviceLogger);
								cpuUsageAlert.AffectedClient = affectedClient;
								var maxCpuTimeHit = queryHashBucket.TopHits("MaxCpuTimeHit")?.Documents<Dictionary<string, object>>()?.FirstOrDefault();
								if (maxCpuTimeHit?.TryGetValue("ServerInstanceName", out object serverInstanceNameObj) ?? false)
								{
									cpuUsageAlert.ServerInstanceName = serverInstanceNameObj?.ToString();
								}

								if (maxCpuTimeHit?.TryGetValue("QueryHash", out object queryHashObj) ?? false)
								{
									cpuUsageAlert.QueryHash = queryHashObj?.ToString();
								}
								yield return cpuUsageAlert;
							}
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		protected virtual IEnumerable<AggregateDictionary> GetResponse(IEnumerable<string> capabilitiesNotFull)
		{
			yield return ElasticSearchAdaptor.GetAggAlertPerDay(DateTime.UtcNow, MaxiumNumberOfCpuUsageAlertPerDayPerSystem * 2, capabilitiesNotFull, serviceLogger);
			yield return ElasticSearchAdaptor.GetAlertPerDayPerClient(DateTime.UtcNow, MaxiumNumberOfCpuUsageAlertPerDayPerSystem * 2, capabilitiesNotFull, serviceLogger);
		}

		public override Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)> GetCapabilityJobsList(IEnumerable<AlertRule> alertRules)
		{
			var result = new Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)>();
			var boFactory = new ReadOnlyBusinessObjectFactory { RefreshEnabled = false };
			var alertRulesGroup = alertRules.GroupBy(rule => rule.Capability).ToList();
			var glbCapabilityMap = ObtainGlbCapabilitiesIdBatch(alertRulesGroup.Select(group => group.Key), boFactory);

			foreach (var group in alertRulesGroup)
			{
				if (glbCapabilityMap.TryGetValue(group.Key, out var glbCapabilityId))
				{
					var workItemSummaries = GetListOfOpenWorkItemSummariesForCapability(glbCapabilityId, boFactory);
					var maxNumberOfAlertsAllowedForCapability = GetMaxNumberOfAlertsAllowedForCapability(group.Sum(c => c.MaxTargets));
					if (workItemSummaries.Count() < maxNumberOfAlertsAllowedForCapability)
					{
						result.Add(group.Key, (maxNumberOfAlertsAllowedForCapability, alertRules.Where(r => r.Capability == group.Key).Select(r => r.Path), workItemSummaries, glbCapabilityId));
					}
				}
			}
			return result;
		}

		internal Dictionary<string, ZGuid> ObtainGlbCapabilitiesIdBatch(IEnumerable<string> capabilities, BusinessObjectFactory boFactory)
		{
			var query = new ZQuery();
			query.AddToFilter(GlbCapabilitySchema.G4_Code, capabilities);
			var glbCapabilities = boFactory.Load<GlbCapability>(query);
			var glbCapabilityCodes = glbCapabilities.Select(e => e.G4_Code.ToString());
			var notExistGlbCapabilities = capabilities.Except(glbCapabilityCodes);

			var glbCapabilityResult = glbCapabilities.ToDictionary(e => e.G4_Code.ToString(), e => e.PK);

			if (notExistGlbCapabilities != null && notExistGlbCapabilities.Any())
			{
				query = new ZQuery(GlbCapabilitySchema.G4_Code, AlertProcessor.DefaultCapabilityCode);
				var defaultCapability = boFactory.LoadTop1<GlbCapability>(query);
				if (defaultCapability is null)
				{
					serviceLogger.Error(AlertProcessor.DefaultCapabilityMissingMessage);
				}
				else
				{
					notExistGlbCapabilities.ForEach(capabilityCode =>
					{
						glbCapabilityResult.Add(capabilityCode, defaultCapability.PK);
					});
				}
			}

			return glbCapabilityResult;
		}

		int GetMaxNumberOfAlertsAllowedForCapability(int sumOfMaxTargets)
		{
			return
				sumOfMaxTargets > 0
				? sumOfMaxTargets
				: MaxiumNumberOfCpuUsageAlertPerDayPerSystem;
		}
	}
}
