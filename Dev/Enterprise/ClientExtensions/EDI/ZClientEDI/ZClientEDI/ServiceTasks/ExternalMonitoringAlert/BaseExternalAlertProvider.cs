using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert
{
	public abstract class BaseExternalAlertProvider
	{
		protected readonly DbConnection wiseGridReportingConnection;
		protected readonly ILogger serviceLogger;

		public IEnumerable<AlertDefault> AlertDefaults { get; private set; }

		public Dictionary<string, int> CapabilityCounts { get; }

		protected BaseExternalAlertProvider(ILogger serviceLogger, DbConnection connection, IEnumerable<AlertDefault> defaults)
		{
			this.serviceLogger = serviceLogger;
			wiseGridReportingConnection = connection;
			AlertDefaults = defaults;
		}

		public abstract string AlertTypeName { get; }

		public abstract string AlertTargetName { get; }

		public abstract AlertRuleType RuleType { get; }

		internal const string ProcessTaskInvestigation = "Investigation";
		internal const string ProcessTaskInvestigationType = "INV";

		public abstract AlertRule DetermineAlertRule(IList<AlertRule> alertRules, BaseAlert alert);

		public abstract void GetAlertAndProcess(IEnumerable<string> capabilitiesNotFull, Func<BaseAlert, bool> processAlertAction);

		public abstract void UpdateAlert(BaseAlert alert, string targetNumber, string owner);

		public virtual string GetAdditionalFields(BaseAlert alert) => string.Empty;

		public virtual string GetNotes(BaseAlert alert, bool includeAlertName) => string.Empty;

		public abstract (AddAlertTargetResult result, BusinessObject parent, string descriptivePath, ProcessTask task) FindAlertTargetCreateIfNotExists(BaseAlert alert, AlertRule matchedRule, BusinessObjectFactory boFactory, ZGuid glbCapabilityId, Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)> capabilityJobsList);

		public abstract string GetTargetKey(BusinessObject alertTarget);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public abstract Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)> GetCapabilityJobsList(IEnumerable<AlertRule> alertRules);
	}
}
