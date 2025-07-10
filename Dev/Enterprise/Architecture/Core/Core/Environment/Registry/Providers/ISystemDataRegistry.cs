using System;
using System.Drawing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public interface ISystemDataRegistry
	{
		Image GetHtmlEmailBannerImage(Guid companyPK, Guid branchPK, Guid departmentPK);

		Image GetHtmlEmailFooterImage(Guid companyPK, Guid branchPK, Guid departmentPK);

		string AzureApplicationClientId { get; }

		string CachedTables { get; }

		string GetHtmlEmailStyleSheet(Guid companyPK, Guid branchPK, Guid departmentPK);

		string[] GetReportingDbServerNames();

		string HtmlEmailStyleSheet { get; }

		string StatisticsCollectionEnabled { get; set; }

		string WindowPersisterData { get; set; }

		int ActivityLogMaximumPastYears { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int RegistryRefreshFrequencyInSeconds { get; }

		int ReportMaxConnections { get; }

		(bool IsEnabledForADD, bool IsEnabledForEDT, bool IsEnabledForDEL)? AuditLogsEnabledFor(string tableName);

		bool AllowCTWhenCDCEnabled { get; }

		bool AsynchronousBilling { get; }

		bool EnableModuleQueryFromSecondaryDbReplica { get; }

		bool ReportConcurrencyErrors { get; }

		bool RunOMSInSimulationMode { get; }

		string[] ModuleQueryDbServerNames { get; }

		bool ServiceTaskBusinessObjectBindingEnabled { get; }

		bool ServiceTaskParallelWebRequestsEnabled { get; }

		bool ServiceTaskRunnerConnectionPoolingEnabled { get; }

		bool StoreCertificatesUnderCurrentUser { get; }

		bool UserIdleWorkerEnabled { get; }

		bool UseReportingDbServerNames { get; }

		IColorTheme ColorTheme { get; }

		TimeSpan ReportingDbServerThreshold { get; }

		DateTime LastDatabaseRestoreDate { get; }

		BooleanRegistryItem AllowScalarFunctionsInCustomSql { get; }

		BooleanRegistryItem UnionOrOrFilter { get; }

		BooleanRegistryItem RecompileFilter { get; }

		BooleanRegistryItem CardinalityFilter { get; }

		IntRegistryItem CdcLatencyProviderAcceptableBacklog { get; }
	}
}
