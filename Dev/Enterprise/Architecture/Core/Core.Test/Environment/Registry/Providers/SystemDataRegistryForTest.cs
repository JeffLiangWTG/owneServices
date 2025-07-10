using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public sealed class SystemDataRegistryForTest : ISystemDataRegistry
	{
		SystemDataRegistryForTest()
		{
			parent = ObjectFactory.Get<ISystemDataRegistry>();
		}

		public static SystemDataRegistryForTest Get()
		{
			SystemDataRegistryForTest result = ObjectFactory.Get<ISystemDataRegistry>() as SystemDataRegistryForTest;
			if (result == null)
			{
				result = new SystemDataRegistryForTest();
				ObjectFactory.Substitute<ISystemDataRegistry>(result);
			}
			return result;
		}

		public bool AsynchronousBilling
		{
			get { return true; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int RegistryRefreshFrequencyInSeconds
		{
			get { return 600; }
		}

		public string StatisticsCollectionEnabled
		{
			get { return "Disabled"; }
			set { }
		}

		public Dictionary<string, string> StatisticsAggregationParameters
		{
			get { return new Dictionary<string, string>(); }
		}

		public bool RunOMSInSimulationMode { get; set; }
		public bool ServiceTaskBusinessObjectBindingEnabled { get; set; }
		public bool ServiceTaskParallelWebRequestsEnabled { get; set; }
		public bool ServiceTaskRunnerConnectionPoolingEnabled { get; set; }
		public bool ProcessControllerVerboseLoggingEnabled { get; set; }

		public Image HtmlEmailBannerImage
		{
			get { return htmlEmailBannerImageOverridden ? htmlEmailBannerImage : parent.GetHtmlEmailBannerImage(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
			set
			{
				htmlEmailBannerImage = value;
				htmlEmailBannerImageOverridden = true;
			}
		}
		Image htmlEmailBannerImage;
		bool htmlEmailBannerImageOverridden;

		public Image HtmlEmailFooterImage
		{
			get { return htmlEmailFooterImageOverridden ? htmlEmailFooterImage : parent.GetHtmlEmailFooterImage(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
			set
			{
				htmlEmailFooterImage = value;
				htmlEmailFooterImageOverridden = true;
			}
		}
		Image htmlEmailFooterImage;
		bool htmlEmailFooterImageOverridden;

		public Image GetHtmlEmailBannerImage(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return HtmlEmailBannerImage;
		}

		public Image GetHtmlEmailFooterImage(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return HtmlEmailFooterImage;
		}

		public string GetHtmlEmailStyleSheet(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return HtmlEmailStyleSheet;
		}

		public string HtmlEmailStyleSheet
		{
			get { return htmlEmailStyleSheetOverridden ? htmlEmailStyleSheet : parent.GetHtmlEmailStyleSheet(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
			set
			{
				htmlEmailStyleSheet = value;
				htmlEmailStyleSheetOverridden = true;
			}
		}
		string htmlEmailStyleSheet;
		bool htmlEmailStyleSheetOverridden;

		public string[] GetReportingDbServerNames()
		{
			return ReportingDbServerNames ?? parent.GetReportingDbServerNames();
		}

		public bool DataVersionLogsEnabled(string tableName) => false;

		public (bool IsEnabledForADD, bool IsEnabledForEDT, bool IsEnabledForDEL)? AuditLogsEnabledFor(string tableName) => (false, false, false);

		public string[] ReportingDbServerNames;

		public TimeSpan ReportingDbServerThreshold
		{
			get { return parent.ReportingDbServerThreshold; }
		}

		public IntRegistryItem CdcLatencyProviderAcceptableBacklog
		{
			get { return parent.CdcLatencyProviderAcceptableBacklog; }
		}

		public int ReportMaxConnections
		{
			get { return reportMaxConnections ?? parent.ReportMaxConnections; }
			set { reportMaxConnections = value; }
		}
		int? reportMaxConnections;

		public bool ReportConcurrencyErrors
		{
			get { return reportConcurrencyErrors ?? parent.ReportConcurrencyErrors; }
			set { reportConcurrencyErrors = value; }
		}
		bool? reportConcurrencyErrors;

		public bool UserIdleWorkerEnabled
		{
			get { return userIdleWorkerEnabled ?? parent.UserIdleWorkerEnabled; }
			set { userIdleWorkerEnabled = value; }
		}
		bool? userIdleWorkerEnabled;

		public bool StoreCertificatesUnderCurrentUser
		{
			get { return storeCertificatesUnderCurrentUser ?? parent.StoreCertificatesUnderCurrentUser; }
			set { storeCertificatesUnderCurrentUser = value; }
		}
		bool? storeCertificatesUnderCurrentUser;

		public IColorTheme ColorTheme
		{
			get { return colorTheme ?? parent.ColorTheme; }
			set { colorTheme = value; }
		}
		IColorTheme colorTheme;

		public string CachedTables
		{
			get;
			set;
		}

		public string WindowPersisterData
		{
			get;
			set;
		}

		public int ActivityLogMaximumPastYears
		{
			get;
			set;
		}

		bool allowCTWhenCDCEnabled;
		public bool AllowCTWhenCDCEnabled
		{
			get => allowCTWhenCDCEnabled;
			set => allowCTWhenCDCEnabled = value;
		}

		public TimeSpan UpdateDatabaseTaskScheduleThreshold { get; set; }

		public bool UseReportingDbServerNames { get; set; }

		public DateTime LastDatabaseRestoreDate { get; set; }

		public BooleanRegistryItem UnionOrOrFilter => parent.UnionOrOrFilter;

		public BooleanRegistryItem RecompileFilter => parent.RecompileFilter;

		public BooleanRegistryItem CardinalityFilter => parent.CardinalityFilter;

		public BooleanRegistryItem AllowScalarFunctionsInCustomSql => parent.AllowScalarFunctionsInCustomSql;

		public string AzureApplicationClientId => parent.AzureApplicationClientId;

		public bool EnableModuleQueryFromSecondaryDbReplica { get; set; }

		public string[] ModuleQueryDbServerNames { get; set; }

		readonly ISystemDataRegistry parent;
	}
}
