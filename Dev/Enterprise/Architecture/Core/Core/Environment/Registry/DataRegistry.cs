using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using CargoWise.Data.SqlServer;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	#region Enums

	public enum AccountOrderType
	{
		ProfitAndLoss,
		BalanceSheet
	}

	#endregion

	public sealed partial class DataRegistry
	{
		#region Construction

		internal DataRegistry()
		{
		}

		public RawDataRegistry RawRegistry
		{
			get
			{
				if (fRawRegistry == null)
				{
					fRawRegistry = new RawDataRegistry();
				}
				return fRawRegistry;
			}
		}
		RawDataRegistry fRawRegistry;

		public static DataRegistry Instance
		{
			get { return instance ?? (instance = new DataRegistry()); }
		}
		[ThreadStatic]
		static DataRegistry instance;

		#endregion

		#region System

		public bool DisableRobotDetection => RawRegistry.DisableRobotDetection.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

		public bool EnableRPAOnWiseCloud => RawRegistry.EnableRPAForWiseCloud.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

		[Conditional("DEBUG")]
		public void SetEnableRPAOnWiseCloudForTest(bool value)
		{
#if DEBUG
			RawRegistry.EnableRPAForWiseCloud.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		public int MaximumParametersPerFetchHint
		{
			get { return RawRegistry.MaximumParametersPerFetchHint.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public int OperationalActionsRecordBatchSize
		{
			get { return RawRegistry.OperationalActionsRecordBatchSize.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.OperationalActionsRecordBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public DateTime NativeXMLSupportTillDate
		{
			get { return RawRegistry.NativeXMLSupportTillDate.Value; }
			set { RawRegistry.NativeXMLSupportTillDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool IsNativeXMLSupported
		{
			get
			{
				var date = NativeXMLSupportTillDate;
				return date != DateTime.MinValue && date >= ZDateTime.UtcNow.Date.ToDateTime();
			}
		}

		public string DefaultDotNetVersionOnLaunch
		{
			get
			{
				if (RawRegistry.GetRegistryOptionForNetVersionSwitch() == RegistryOptions.IsHidden)
				{
					return DotNetBuildVersionTargetTypeList.DefaultVersion;
				}

				return RawRegistry.DefaultDotNetVersionOnLaunch.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}
			set { RawRegistry.DefaultDotNetVersionOnLaunch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool EnableDotNetVersionSwitchMenu
		{
			get
			{
				if (RawRegistry.GetRegistryOptionForNetVersionSwitch() == RegistryOptions.IsHidden)
				{
					return false;
				}
				return RawRegistry.EnableDotNetVersionSwitchMenu.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}
			set { RawRegistry.EnableDotNetVersionSwitchMenu.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string DotNetPreUpgradeCheckWhitelist
		{
			get { return RawRegistry.DotNetPreUpgradeCheckWhitelist.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DotNetPreUpgradeCheckWhitelist.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool EnableDotNetPreUpgradeCheck
		{
			get { return RawRegistry.EnableDotNetPreUpgradeCheck.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.EnableDotNetPreUpgradeCheck.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool EnableEDIMessageInterpreter
		{
			get { return RawRegistry.EnableEDIMessageInterpreter.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.EnableEDIMessageInterpreter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string ExpectedClientDLL
		{
			get { return RawRegistry.ExpectedClientDLL.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ExpectedClientDLL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ReportAllNonCriticalSqlErrors
		{
			get { return RawRegistry.ReportAllNonCriticalSqlErrors.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ReportAllNonCriticalSqlErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DatabaseMajorSchemaVersion
		{
			get { return RawRegistry.DatabaseMajorSchemaVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseMajorSchemaVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DatabaseMinorSchemaVersion
		{
			get { return (int)RawRegistry.DatabaseMinorSchemaVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseMinorSchemaVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DatabaseMajorScriptVersion
		{
			get { return (int)RawRegistry.DatabaseMajorScriptVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseMajorScriptVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DatabaseMinorScriptVersion
		{
			get { return (int)RawRegistry.DatabaseMinorScriptVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseMinorScriptVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DatabaseMajorAnalyticsReportProjectVersion
		{
			get { return (int)RawRegistry.DatabaseMajorAnalyticsReportProjectUpgradeVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseMajorAnalyticsReportProjectUpgradeVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DatabaseMinorAnalyticsReportProjectVersion
		{
			get { return (int)RawRegistry.DatabaseMinorAnalyticsReportProjectUpgradeVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseMinorAnalyticsReportProjectUpgradeVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DatabaseMajorAuditReportProjectVersion
		{
			get { return (int)RawRegistry.DatabaseMajorAuditReportProjectUpgradeVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseMajorAuditReportProjectUpgradeVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DatabaseMinorAuditReportProjectVersion
		{
			get { return (int)RawRegistry.DatabaseMinorAuditReportProjectUpgradeVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseMinorAuditReportProjectUpgradeVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DatabaseMajorTransformationVersion
		{
			get { return RawRegistry.DatabaseMajorTransformationVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseMajorTransformationVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DatabaseMinorTransformationVersion
		{
			get { return RawRegistry.DatabaseMinorTransformationVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseMinorTransformationVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DatabaseMajorClrAssembliesVersion
		{
			get { return RawRegistry.DatabaseMajorClrAssembliesVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseMajorClrAssembliesVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DatabaseMinorClrAssembliesVersion
		{
			get { return RawRegistry.DatabaseMinorClrAssembliesVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseMinorClrAssembliesVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public const int MinUpgradableDbMajorSchemaVersion = 7771; // SchemaVersion.cs minimum value of ApplicationMajor
		public const int MinDatabaseMajorTransformationVersion = 7701; // mapper.cs minimum transform must be higher than this

		public const string GeneralProductReleaseContainingUpgradeScripts = "24.4";
		public int DatabaseSystemDataVersionMajor
		{
			get { return RawRegistry.DatabaseSystemDataVersionMajor.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseSystemDataVersionMajor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DatabaseSystemDataVersionMinor
		{
			get { return RawRegistry.DatabaseSystemDataVersionMinor.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DatabaseSystemDataVersionMinor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string OnlineTransformationStatus
		{
			get { return RawRegistry.OnlineTransformationStatus.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.OnlineTransformationStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool SuppressDbFilesHealthCheckNotificationsForHostedSystems
		{
			get { return RawRegistry.SuppressDbFilesHealthCheckNotificationsForHostedSystems.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int HeartbeatDurationSeconds
		{
			get { return RawRegistry.HeartbeatDurationSeconds.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int ServiceTaskHeartbeatDurationSeconds
		{
			get { return RawRegistry.ServiceTaskHeartbeatDurationSeconds.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.ServiceTaskHeartbeatDurationSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int LockTimeout
		{
			get { return (int)RawRegistry.LockTimeout.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.LockTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DbccInitialTimeout
		{
			get { return RawRegistry.DbccInitialTimeout.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DbccInitialTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DbccUserWaitThreshold
		{
			get { return RawRegistry.DbccUserWaitThreshold.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DbccUserWaitThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DbccServiceTaskWaitThreshold
		{
			get { return RawRegistry.DbccServiceTaskWaitThreshold.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DbccServiceTaskWaitThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DbccPollingInterval
		{
			get { return RawRegistry.DbccPollingInterval.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DbccPollingInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool DbccRunCheckdbWithPhysicalOnly
		{
			get { return RawRegistry.DbccRunCheckdbWithPhysicalOnly.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DbccRunCheckdbWithPhysicalOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool DbccRunSecondariesInParallel
		{
			get { return RawRegistry.DbccRunSecondariesInParallel.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DbccRunSecondariesInParallel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool DbccRunSecondariesByParts
		{
			get { return RawRegistry.DbccRunSecondariesByParts.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DbccRunSecondariesByParts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int ISU_Rebuild_ThresholdPercentage
		{
			get { return RawRegistry.ISU_Rebuild_ThresholdPercentage.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ISU_Rebuild_ThresholdPercentage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int ISU_Rebuild_MaxDopPercentage
		{
			get { return RawRegistry.ISU_Rebuild_MaxDopPercentage.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ISU_Rebuild_MaxDopPercentage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ISU_Rebuild_Online
		{
			get { return RawRegistry.ISU_Rebuild_Online.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ISU_Rebuild_Online.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ISU_Rebuild_UseObserver
		{
			get { return RawRegistry.ISU_Rebuild_UseObserver.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ISU_Rebuild_UseObserver.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int ISU_Rebuild_MaxWaitInMinutes
		{
			get { return RawRegistry.ISU_Rebuild_MaxWaitInMinutes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ISU_Rebuild_MaxWaitInMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string ISU_Rebuild_AbortAfterWait
		{
			get { return RawRegistry.ISU_Rebuild_AbortAfterWait.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ISU_Rebuild_AbortAfterWait.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int ISU_Reorganize_Threshold
		{
			get { return RawRegistry.ISU_Reorganize_ThresholdPercentage.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ISU_Reorganize_ThresholdPercentage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int ISU_Reorganize_MaxConcurrentProcessesPercentage
		{
			get { return RawRegistry.ISU_Reorganize_MaxConcurrentProcessesPercentage.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ISU_Reorganize_MaxConcurrentProcessesPercentage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int ISU_Reorganize_PeriodInDays
		{
			get { return RawRegistry.ISU_Reorganize_PeriodInDays.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ISU_Reorganize_PeriodInDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int ISU_MinimumIndexPageCount
		{
			get { return RawRegistry.ISU_MinimumIndexPageCount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ISU_MinimumIndexPageCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int ISU_MaxBacklogWaitTime_InMinutes
		{
			get { return RawRegistry.ISU_MaxBacklogWaitTime_InMinutes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ISU_MaxBacklogWaitTime_InMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int ISU_EmptyStatisticsRowsThreshold
		{
			get { return RawRegistry.ISU_EmptyStatisticsRowsThreshold.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ISU_EmptyStatisticsRowsThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ISU_DisableAutoStatisticsDuringUpgrade
		{
			get { return RawRegistry.ISU_DisableAutoStatisticsDuringUpgrade.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ISU_DisableAutoStatisticsDuringUpgrade.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#region Modern Sql security system

		public bool UseModernSqlSecuritySystem
		{
			get { return RawRegistry.UseModernSqlSecuritySystem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.UseModernSqlSecuritySystem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion Modern Sql security system

		#region Always On Replica Cache

		public bool UseAlwaysOnReplicaCache
		{
			get { return RawRegistry.UseAlwaysOnReplicaCache.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.UseAlwaysOnReplicaCache.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public AlwaysOnReplicaInfo[] AlwaysOnReplicaCachedInfos
		{
			get { return AlwaysOnReplicaInfosRegistryHelper.Deserialise(RawRegistry.AlwaysOnReplicaCachedInfos.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { RawRegistry.AlwaysOnReplicaCachedInfos.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AlwaysOnReplicaInfosRegistryHelper.Serialise(value)); }
		}

		public string[] AvailabilityGroupInfo
		{
			get { return RawRegistry.AvailabilityGroupInfo.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AvailabilityGroupInfo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion //Always On Replica Cache

		public bool RemoveOldUpgradePackages
		{
			get { return RawRegistry.RemoveOldUpgradePackages.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public bool KeepOnlyLatestVersion
		{
			get { return RawRegistry.KeepOnlyLatestVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public string BackupDirectoryPath
		{
			get { return RawRegistry.BackupDirectoryPath.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.BackupDirectoryPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool BackupSystemDatabases
		{
			get { return !EnvProxy.IsHostedWithCargowise && RawRegistry.BackupSystemDatabases.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.BackupSystemDatabases.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool BackupReferenceDatabases
		{
			get { return RawRegistry.BackupReferenceDatabases.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.BackupReferenceDatabases.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool SkipLogShrinkingAndBackupInUpgrade
		{
			get { return RawRegistry.SkipLogShrinkingAndBackupInUpgrade.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public int PurgeDataTimeoutLimit
		{
			get { return RawRegistry.PurgeDataTimeoutLimit.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PurgeDataTimeoutLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PurgeDataRunStatusFlag
		{
			get => RawRegistry.PurgeDataRunStatusFlag.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			set => RawRegistry.PurgeDataRunStatusFlag.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int GhostRecordCleanerLockTimeoutSeconds
		{
			get { return RawRegistry.GhostRecordCleanerLockTimeoutSeconds.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.GhostRecordCleanerLockTimeoutSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "We require number of minutes for REBUILD INDEX statement")]
		public int GhostRecordCleanUpRebuildWaitMinutes
		{
			get { return RawRegistry.GhostRecordCleanUpRebuildWaitMinutes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public int GhostRecordCleanUpThreshold
		{
			get { return RawRegistry.GhostRecordCleanUpThreshold.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public int GhostRecordProcessingThreadInterval
		{
			get { return RawRegistry.GhostRecordProcessingThreadInterval.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}
		public int GhostRecordMaxProcessingThreadCount
		{
			get { return RawRegistry.GhostRecordMaxProcessingThreadCount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public bool GhostRecordCleanupOnlineRebuild
		{
			get { return RawRegistry.GhostRecordCleanupOnlineRebuild.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public int GhostRecordCleanupKillBlockersThreshold
		{
			get { return RawRegistry.GhostRecordCleanupKillBlockersThreshold.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public int GhostRecordCleanupCommandTimeOutThreshold
		{
			get { return RawRegistry.GhostRecordCleanupCommandTimeOutThreshold.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public int ZSqlSaverRowsToPostPerSqlStatement
		{
			get { return RawRegistry.ZSqlSaverRowsToPostPerSqlStatement.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.ZSqlSaverRowsToPostPerSqlStatement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool SkipBackupOfNonMainDatabasesWithoutActivity
		{
			get { return RawRegistry.SkipBackupOfNonMainDatabasesWithoutActivity.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.SkipBackupOfNonMainDatabasesWithoutActivity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool UseDbBackupCompression
		{
			get { return RawRegistry.UseDbBackupCompression.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.UseDbBackupCompression.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool BackupIncludesCheckSum
		{
			get { return RawRegistry.BackupIncludesCheckSum.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.BackupIncludesCheckSum.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int DaysOfLogBackupToKeep
		{
			get { return RawRegistry.DaysOfLogBackupToKeep.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public bool OnlySupportsSqlServerEnterpriseEdition
		{
			get { return RawRegistry.OnlySupportsSqlServerEnterpriseEdition.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.OnlySupportsSqlServerEnterpriseEdition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool ApplyOptionRecompile
		{
			get { return RawRegistry.ApplyOptionRecompile.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public bool DefaultToForceSeek
		{
			get { return RawRegistry.DefaultToForceSeek.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public bool ApplyIsNotNullToJoinOnFK
		{
			get { return RawRegistry.ApplyIsNotNullToJoinOnFK.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public bool ParameterizeInsertAndUpdateStatements
		{
			get { return RawRegistry.ParameterizeInsertAndUpdateStatements.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public string FieldsToLiteralize
		{
			get { return (string)RawRegistry.FieldsToLiteralize.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public TVPRule TVPRule
		{
			get { return new TVPRule(RawRegistry.TVPRule.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
		}

		public int PasteDebounceMs
		{
			get { return (int)RawRegistry.PasteDebounceMs.GetValueWithFallbackDefault(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public bool ConcatenateMultipleFetchHintTypes
		{
			get { return RawRegistry.ConcatenateMultipleFetchHintTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ConcatenateMultipleFetchHintTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ShowExactRowCountOnExcessResult
		{
			get { return RawRegistry.ShowExactRowCountOnExcessResult.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ShowExactRowCountOnExcessResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int ClientDocumentVersion
		{
			get { return RawRegistry.ClientDocumentVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ClientDocumentVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public IntRegistryItem ClientDocumentVersionRawItem
		{
			get { return RawRegistry.ClientDocumentVersion; }
		}

		public string ClientDocumentName
		{
			get { return (string)RawRegistry.ClientDocumentName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ClientDocumentName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public Guid UserRunningUpgrade
		{
			get { return (Guid)RawRegistry.UserRunningUpgrade.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.UserRunningUpgrade.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool UserEventTrackingEnterprise
		{
			get { return (bool)RawRegistry.UserEventTrackingEnterprise.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.UserEventTrackingEnterprise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool UserEventTrackingExternal
		{
			get { return (bool)RawRegistry.UserEventTrackingExternal.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.UserEventTrackingExternal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		/// <summary>
		/// The name of the SMTP Server
		/// </summary>
		public string SMTPServer
		{
			get { return (string)RawRegistry.SMTPServer.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.SMTPServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int SMTPPort
		{
			get { return (int)RawRegistry.SMTPPort.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.SMTPPort.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string SMTPEhloDomain
		{
			get { return (string)RawRegistry.SMTPEhloDomain.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.SMTPEhloDomain.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string SMTPUsername
		{
			get { return (string)RawRegistry.SMTPUsername.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.SMTPUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string SMTPPassword
		{
			get { return (string)RawRegistry.SMTPPassword.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.SMTPPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string SMTPDefaultReturnEmailAddress
		{
			get
			{
				return (string)RawRegistry.SMTPDefaultReturnEmailAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}
			set
			{
				RawRegistry.SMTPDefaultReturnEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			}
		}

		public string SMTPDefaultDoNotReplyEmailAddress
		{
			get
			{
				return EnvProxy.Instance.CurrentCompany != null
			? (string)RawRegistry.SMTPDefaultDoNotReplyEmailAddress.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty)
			: (string)RawRegistry.SMTPDefaultDoNotReplyEmailAddress.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
			}
			set
			{
				RawRegistry.SMTPDefaultDoNotReplyEmailAddress.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value);
			}
		}

		public int MaximumNumberOfMailItemsToSendInABatch
		{
			get { return (int)RawRegistry.MaximumNumberOfMailItemsToSendInABatch.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MaximumNumberOfMailItemsToSendInABatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int SMTPServerTimeout
		{
			get { return (int)RawRegistry.SMTPServerTimeout.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public string SMTPSecureConnection
		{
			get { return (string)RawRegistry.SMTPSecureConnection.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.SMTPSecureConnection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string EnterpriseMailboxEmailAddress
		{
			get { return (string)RawRegistry.MailboxEmailAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MailboxEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string AttachmentEncodingFormat
		{
			get { return (string)RawRegistry.AttachmentEncodingFormat.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AttachmentEncodingFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		/// <summary>
		/// The name of the physical location of the Odyssey server
		/// </summary>
		public string PhysicalServerID
		{
			get { return (string)RawRegistry.PhysicalServerID.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.PhysicalServerID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int StaffDetailsUpdateFrequency
		{
			get { return RawRegistry.StaffDetailsUpdateFrequency.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.StaffDetailsUpdateFrequency.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public byte[] CWSupportLoginTokenCertificate
		{
			get { return RawRegistry.CWSupportLoginTokenCertificate.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public IDictionary<string, ZDateTime> UserContextErrorEmailTimestamps
		{
			get
			{
				var rawTimestamps = RawRegistry.UserContextErrorEmailTimestamps.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				var result = new Dictionary<string, ZDateTime>(StringComparer.OrdinalIgnoreCase);
				if (string.IsNullOrWhiteSpace(rawTimestamps))
				{
					return result;
				}
				foreach (string rawTimestamp in rawTimestamps.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
				{
					string[] splittedRawTimestamp = rawTimestamp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					if (splittedRawTimestamp.Length != 2)
					{
						continue;
					}
					result[splittedRawTimestamp[0]] = new ZDateTime(long.Parse(splittedRawTimestamp[1])); // The timestamp is generated by the code only.
				}
				return result;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}
				StringBuilder rawTimestampsBuilder = new StringBuilder();
				if (value.Any())
				{
					foreach (KeyValuePair<string, ZDateTime> timestamp in value)
					{
						rawTimestampsBuilder.AppendFormat("{0},{1};", timestamp.Key, timestamp.Value.Ticks);
					}
					rawTimestampsBuilder.Remove(rawTimestampsBuilder.Length - 1, 1);
				}
				RawRegistry.UserContextErrorEmailTimestamps.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawTimestampsBuilder.ToString());
			}
		}

		public int UserContextErrorEmailInterval
		{
			get { return RawRegistry.UserContextErrorEmailInterval.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.UserContextErrorEmailInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool RemoveNonAsciiCharactersInEmailMessageHeader
		{
			get { return RawRegistry.RemoveNonAsciiCharactersInEmailMessageHeader.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.RemoveNonAsciiCharactersInEmailMessageHeader.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int MegabytesOfManagedMemoryBeforeAutomaticCollection
		{
			get { return RawRegistry.MegabytesOfManagedMemoryBeforeAutomaticCollection.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MegabytesOfManagedMemoryBeforeAutomaticCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public HelpMode UserHelpMode
		{
			get { return (HelpMode)Enum.Parse(typeof(HelpMode), RawRegistry.UserHelpMode.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty)); }
			set { RawRegistry.UserHelpMode.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		public bool TraningModeEnabled
		{
			get { return RawRegistry.TrainingModeEnabled.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser != null ? EnvProxy.Instance.CurrentUser.PK : Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.TrainingModeEnabled.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value); }
		}

		public bool DisplayUtcOffset
		{
			get => RawRegistry.DisplayUtcOffset.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			set => RawRegistry.DisplayUtcOffset.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		public bool ReportStatisticsLogExecutionPlan
		{
			get { return RawRegistry.ReportStatisticsLogExecutionPlan.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ReportStatisticsLogExecutionPlan.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int PurgeReportStatisticsLogs
		{
			get { return RawRegistry.PurgeReportStatisticsLogs.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.PurgeReportStatisticsLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int InternalApplicationActivityTrackingInterval
		{
			get { return RawRegistry.InternalApplicationActivityTrackingInterval.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public bool DisplayGMTOffsetOnF5UserInfo
		{
			get { return RawRegistry.DisplayGMTOffsetOnF5UserInfo.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DisplayGMTOffsetOnF5UserInfo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool RememberOpenedFormsAfterUpgrade
		{
			get { return RawRegistry.RememberOpenedFormsAfterUpgrade.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.RememberOpenedFormsAfterUpgrade.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public bool ShowSaveProgressBox
		{
			get { return RawRegistry.ShowSaveProgressBox.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ShowSaveProgressBox.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ShowCodeAtCompanyAndBranchName
		{
			get => RawRegistry.ShowCodeAtCompanyAndBranchName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			set => RawRegistry.ShowCodeAtCompanyAndBranchName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		public bool LightValidationEnabled
		{
			get { return RawRegistry.LightValidationEnabled.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.LightValidationEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool CalendarIntegration
		{
			get { return RawRegistry.CalendarIntegration.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.CalendarIntegration.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool CalendarInvitationSolutionForOrganisers
		{
			get { return RawRegistry.CalendarInvitationSolutionForOrganisers.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.CalendarInvitationSolutionForOrganisers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#region Licence

		public string LegacyEncryptedSystemRegistrationKey
		{
			get { return RawRegistry.LegacyEncryptedSystemRegistrationKey.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.LegacyEncryptedSystemRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		public string UseOAuth2ForIncoming
		{
			get { return RawRegistry.UseOAuth2ForIncoming.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.UseOAuth2ForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string UseOAuth2ForOutgoing
		{
			get { return RawRegistry.UseOAuth2ForOutgoing.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.UseOAuth2ForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string Ms365ApplicationIdForIncoming
		{
			get { return RawRegistry.Ms365ApplicationIdForIncoming.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.Ms365ApplicationIdForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string Ms365AppSecretForIncoming
		{
			get { return RawRegistry.Ms365AppSecretForIncoming.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set
			{
				RawRegistry.Ms365AppSecretForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				RawRegistry.Ms365OAuth2AppTokenForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			}
		}

		public string Ms365ApplicationIdForOutgoing
		{
			get { return RawRegistry.Ms365ApplicationIdForOutgoing.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.Ms365ApplicationIdForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string Ms365AppSecretForOutgoing
		{
			get { return RawRegistry.Ms365AppSecretForOutgoing.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set
			{
				RawRegistry.Ms365AppSecretForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				RawRegistry.Ms365OAuth2AppTokenForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			}
		}

		public bool UseGraphApiForIncoming
		{
			get { return RawRegistry.UseGraphApiForIncoming.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.UseGraphApiForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool UseGraphApiForOutgoing
		{
			get { return RawRegistry.UseGraphApiForOutgoing.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.UseGraphApiForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public Ms365OAuth2Token Ms365OAuth2TokenForIncoming
		{
			get { return RawRegistry.Ms365OAuth2TokenForIncoming.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.Ms365OAuth2TokenForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public Ms365OAuth2Token Ms365OAuth2TokenForOutgoing
		{
			get { return RawRegistry.Ms365OAuth2TokenForOutgoing.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.Ms365OAuth2TokenForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public byte[] Ms365OAuth2AppTokenForIncoming
		{
			get { return RawRegistry.Ms365OAuth2AppTokenForIncoming.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.Ms365OAuth2AppTokenForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public byte[] Ms365OAuth2AppTokenForOutgoing
		{
			get { return RawRegistry.Ms365OAuth2AppTokenForOutgoing.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.Ms365OAuth2AppTokenForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string Ms365OAuth2TenantId
		{
			get { return RawRegistry.Ms365OAuth2TenantId.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.Ms365OAuth2TenantId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string GmailDelegatedMailForIncoming
		{
			get { return RawRegistry.GmailDelegatedMailForIncoming.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.GmailDelegatedMailForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public GmailOAuth2JsonFile GmailServiceAccountKeyForIncoming
		{
			get { return RawRegistry.GmailServiceAccountKeyForIncoming.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.GmailServiceAccountKeyForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string GmailDelegatedMailForOutgoing
		{
			get { return RawRegistry.GmailDelegatedMailForOutgoing.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.GmailDelegatedMailForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public GmailOAuth2JsonFile GmailServiceAccountKeyForOutgoing
		{
			get { return RawRegistry.GmailServiceAccountKeyForOutgoing.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.GmailServiceAccountKeyForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool UseVerboseProtocolLogging
		{
			get { return (bool)RawRegistry.UseVerboseProtocolLogging.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.UseVerboseProtocolLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool UseMailKitPOP3AndIMAPProtocolLogging
		{
			get { return (bool)RawRegistry.UseMailKitPOP3AndIMAPProtocolLogging.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.UseMailKitPOP3AndIMAPProtocolLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string MailRetrievalProtocol
		{
			get { return (string)RawRegistry.MailRetrievalProtocol.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MailRetrievalProtocol.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string MailServer
		{
			get { return (string)RawRegistry.MailServer.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.MailServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int MailServerPort
		{
			get { return (int)RawRegistry.MailServerPort.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MailServerPort.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string MailboxUserName
		{
			get { return (string)RawRegistry.MailboxUserName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MailboxUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string MailboxPassword
		{
			get { return (string)RawRegistry.MailboxPassword.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.MailboxPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string MailboxEmailAddress
		{
			get { return (string)RawRegistry.MailboxEmailAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MailboxEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string SelectNDRPath
		{
			get { return RawRegistry.SelectNDRPath.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.SelectNDRPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string MailboxDisplayName
		{
			get { return (string)RawRegistry.MailboxDisplayName.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany != null ? EnvProxy.Instance.CurrentCompany.PK : Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MailboxDisplayName.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string POP3SecureConnection
		{
			get { return (string)RawRegistry.POP3SecureConnectionType.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.POP3SecureConnectionType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string IMAPSecureConnection
		{
			get { return (string)RawRegistry.IMAPSecureConnectionType.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.IMAPSecureConnectionType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool AllowEmailsToBeSentFromUsersAddress
		{
			get { return (bool)RawRegistry.AllowEmailsToBeSentFromUsersAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AllowEmailsToBeSentFromUsersAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ShowUnimplementedModule
		{
			get { return RawRegistry.ShowUnimplementedModule.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ShowUnimplementedModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public byte[] EagleMailPrivateKey
		{
			get { return RawRegistry.EagleMailPrivateKey.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public string EagleMailPrivateKeyPassword
		{
			get { return RawRegistry.EagleMailPrivateKeyPassword.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public byte[] EagleMailPublicCertificate
		{
			get { return RawRegistry.EagleMailPublicCertificate.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		#region Web

		public Guid WebBranch
		{
			get { return (Guid)RawRegistry.WebBranch.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.WebBranch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public Guid WebDepartment
		{
			get { return (Guid)RawRegistry.WebDepartment.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.WebDepartment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool WebHyperlinksEnabled
		{
			get => (bool)RawRegistry.WebHyperlinksEnabled.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			set => RawRegistry.WebHyperlinksEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		public bool AddDatabaseInfoToEdientUrls
		{
			get => (bool)RawRegistry.AddDatabaseInfoToEdientUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			set => RawRegistry.AddDatabaseInfoToEdientUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		public string[] ResourceStringUsageTrustedDomains
		{
			get { return RawRegistry.ResourceStringUsageTrustedDomains.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ResourceStringUsageTrustedDomains.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region FilterCriteria

		const string FilterCriteria = "FilterCriteria-"; // filter criteria name

		public void SetFilterCriteria(string formContext, string criteria)
		{
			SetFilterCriteria(EnvProxy.Instance.CurrentUser.PK, formContext, criteria);
		}

		public void SetFilterCriteria(Guid parentGuid, string formContext, string criteria)
		{
			RawRegistry.FilterCriteria.Name = FilterCriteria + formContext;
			RawRegistry.FilterCriteria.SetValue(parentGuid, Guid.Empty, Guid.Empty, criteria);
		}

		public string GetFilterCriteria(string formContext)
		{
			return GetFilterCriteria(EnvProxy.Instance.CurrentUser.PK, formContext);
		}

		public string GetFilterCriteria(Guid parentGuid, string formContext)
		{
			RawRegistry.FilterCriteria.Name = FilterCriteria + formContext;
			return (string)RawRegistry.FilterCriteria.GetValueWithoutFallback(parentGuid, Guid.Empty, Guid.Empty);
		}

		#endregion

		public int MaxRecommendedNumberOfRecordsToShowInDisplayGrids
		{
			get { return (int)RawRegistry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool RunSearchOnEnteringAModule
		{
			get { return (bool)RawRegistry.RunSearchOnEnteringAModule.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.RunSearchOnEnteringAModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool AutoRunSearchFromFindBox
		{
			get { return (bool)RawRegistry.AutoRunSearchFromFindBox.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AutoRunSearchFromFindBox.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string CurrentCheckedOutClientName
		{
			get { return (string)RawRegistry.CurrentCheckedOutClientName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.CurrentCheckedOutClientName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool RunSelectTopNAsRowNumberQuery
		{
			get { return (bool)RawRegistry.RunSelectTopNAsRowNumberQuery.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.RunSelectTopNAsRowNumberQuery.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ReportCrossThreadFactoryAccess
		{
			get { return RawRegistry.ReportCrossThreadFactoryAccess.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ReportCrossThreadFactoryAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool AllowHostedClientAccessToEmailSettings
		{
			get { return (bool)RawRegistry.AllowHostedClientAccessToEmailSettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public DateTime EnterpriseCDDate
		{
			get
			{
				try
				{
					return RawRegistry.EnterpriseCDDate.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				}
				catch (RegistryParsingException)
				{
					return CargoWise.Types.ZDateTime.UtcNow.AddMonths(-3).ToDateTime();
				}
			}
#if DEBUG
			set { RawRegistry.EnterpriseCDDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		internal ReadOnlyCodeDescriptionPairList StaffMembershipTypesList
		{
			get { return RawRegistry.StaffMembershipTypeList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public float TextOnlyNoteFontSize
		{
			get
			{
				return (float)RawRegistry.TextOnlyNoteFontSize.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		public string SystemFontRegItem
		{
			get { return (string)RawRegistry.SystemFontRegItem.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany != null ? EnvProxy.Instance.CurrentCompany.PK : Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.SystemFontRegItem.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string NavigationMenuFontRegItem
		{
			get { return (string)RawRegistry.NavigationMenuFontRegItem.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany != null ? EnvProxy.Instance.CurrentCompany.PK : Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public string NewsAnnouncementFontRegItem
		{
			get { return (string)RawRegistry.NewsAnnouncementFontRegItem.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany != null ? EnvProxy.Instance.CurrentCompany.PK : Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public string GraphicRenderingEngineRegItem
		{
			get { return (string)RawRegistry.GraphicRenderingEngineRegItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.GraphicRenderingEngineRegItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool ForceDialogRenderingOverTS
		{
			get { return RawRegistry.ForceDialogRenderingOverTS.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public bool ShouldForceDialogRendering
		{
			get { return ForceDialogRenderingOverTS && IsTerminalServiceMode; }
		}

		bool IsTerminalServiceMode
		{
			get
			{
				if (!isTerminalServiceMode.HasValue)
				{
					isTerminalServiceMode = new EnterpriseInformationRetriever().TerminalServerMode;
				}
				return isTerminalServiceMode.Value;
			}
		}
		bool? isTerminalServiceMode;

		[Conditional("DEBUG")]
		internal void SetIsTerminalServiceModeForTest(bool value)
		{
			isTerminalServiceMode = value;
		}

		public int RelatedNoteReadDelay
		{
			get { return (int)RawRegistry.RelatedNoteReadDelay.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public DateTime DateTimeStaffFormWasLastShown
		{
			get { return (DateTime)RawRegistry.DateTimeStaffFormWasLastShown.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DateTimeStaffFormWasLastShown.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string DebugBusinessObjectType
		{
			get { return RawRegistry.DebugBusinessObjectType.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DebugBusinessObjectType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

#if DEBUG
		public bool ExportGridLayoutDetails
		{
			get { return RawRegistry.ExportGridLayoutDetails.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ExportGridLayoutDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}
#endif

		public bool UCMoveLinkableOnlyEntitiesToProperties
		{
			get { return RawRegistry.UCMoveLinkableOnlyEntitiesToProperties.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public string TemplateRecordValidation
		{
			get { return RawRegistry.TemplateRecordValidation.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public bool ProductivityWiseModeEnabled
		{
			get => RawRegistry.ProductivityWiseModeEnabled.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			set => RawRegistry.ProductivityWiseModeEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		#region Number Fountain

		public string MultiSearchSeparator
		{
			get { return RawRegistry.MultiSearchSeparator.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MultiSearchSeparator.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion // Number Fountain

		#region Upgrade

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int UpgradeObserverMaxWaitInSeconds
		{
			get { return RawRegistry.UpgradeObserverMaxWaitInSeconds.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.UpgradeObserverMaxWaitInSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion // Upgrade

		#endregion // System

		#region External Validation Service

		public bool EnableExternalValidationService
		{
			get { return RawRegistry.EnableExternalValidationService.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.EnableExternalValidationService.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string ExternalValidationServiceUrl
		{
			get { return RawRegistry.ExternalValidationServiceUrl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ExternalValidationServiceUrl.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public int ExternalValidationServiceTimeout
		{
			get { return RawRegistry.ExternalValidationServiceTimeout.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ExternalValidationServiceTimeout.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region Address Validation Service

		public int AddressValidationWebServiceTimeout
		{
			get { return RawRegistry.AddressValidationWebServiceTimeout.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany?.PK ?? Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AddressValidationWebServiceTimeout.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public bool EnableAddressValidationWebService
		{
			get { return RawRegistry.EnableAddressValidationWebService.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany?.PK ?? Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.EnableAddressValidationWebService.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region Freight

		public FreightRegistry Freight
		{
			get
			{
				if (fFreightRegistry == null)
				{
					fFreightRegistry = new FreightRegistry(RawRegistry);
				}
				return fFreightRegistry;
			}
		}
		FreightRegistry fFreightRegistry;

		public string ShipmentScreenLayout
		{
			get { return (string)RawRegistry.ShipmentScreenLayout.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

#if DEBUG
		public void SetShipmentScreenLayout(string value)
		{
			if (new CodeDescriptionPairList(OLookUpEditType.ShipmentScreenLayout).ContainsCode(value))
			{
				RawRegistry.ShipmentScreenLayout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			}
		}
#endif

		/// <summary>
		/// Retrieved from dbo.stmdata with fall back, default is "STD".
		/// </summary>
		public Guid ServiceLevel
		{
			get { return (Guid)RawRegistry.ServiceLevel.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.ServiceLevel.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		/// <summary>
		/// Retrieved from dbo.stmdata with fall back, default is "GEN".
		/// </summary>
		public Guid CommodityCode
		{
			get { return (Guid)RawRegistry.CommodityCode.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.CommodityCode.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string ConsolPaymentTerm
		{
			get { return (string)RawRegistry.ConsolPaymentTerm.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.ConsolPaymentTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

#if DEBUG
		public void SetConsolPaymentTerm(string value)
		{
			RawRegistry.ConsolPaymentTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}
#endif

		public string FreightWeightUnit
		{
			get { return (string)RawRegistry.FreightWeightUnit.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.FreightWeightUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string FreightVolumeUnit
		{
			get { return (string)RawRegistry.FreightVolumeUnit.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.FreightVolumeUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool IsExpress
		{
			get { return (bool)RawRegistry.IsExpress.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.IsExpress.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public bool AllowManualShipmentEntry
		{
			get { return (bool)RawRegistry.AllowManualShipmentNumberEntry.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AllowManualShipmentNumberEntry.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string ManifestClientID
		{
			get { return (string)RawRegistry.ManifestClientID.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ManifestClientID.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public ReadOnlyCodeDescriptionPairList FCLEquipmentNeededList
		{
			get { return RawRegistry.FCLEquipmentNeeded.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.FCLEquipmentNeeded.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public ReadOnlyCodeDescriptionPairList LCLAIREquipmentNeededList
		{
			get { return RawRegistry.LCLAIREquipmentNeeded.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.LCLAIREquipmentNeeded.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Image HouseBillOfLadingLogo
		{
			get { return (Image)RawRegistry.HouseBillOfLadingLogo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.HouseBillOfLadingLogo.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public ReadOnlyCodeDescriptionPairList ContainerStorageClass
		{
			get { return RawRegistry.ContainerStorageClass.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public string OuterPacklinesMeasurementDefaultUnit
		{
			get { return (string)RawRegistry.OuterPacklinesMeasurementDefaultUnit.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public string InnerPacklinesMeasurementDefaultUnit
		{
			get { return (string)RawRegistry.InnerPacklinesMeasurementDefaultUnit.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public string GetIssuingCarrierAgentIATACode(Guid branchPK)
		{
			return (string)RawRegistry.IssuingCarrierAgentIATACode.GetValueWithoutFallback(Guid.Empty, branchPK, Guid.Empty);
		}

#if DEBUG
		public void SetIssuingCarrierAgentIATACode(Guid branchPK, string value)
		{
			RawRegistry.IssuingCarrierAgentIATACode.SetValue(Guid.Empty, branchPK, Guid.Empty, value);
		}
#endif

		#endregion

		#region Order Management

		public bool GetOrderLineContainersVisible(Guid buyerPK)
		{
			return (bool)RawRegistry.OrderLineContainersVisible.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, buyerPK);
		}

		public void SetOrderLineContainersVisible(Guid buyerPK, bool value)
		{
			RawRegistry.OrderLineContainersVisible.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, buyerPK, value);
		}

		public ReadOnlyCodeDescriptionPairList OrderHeaderStatusList
		{
			get { return RawRegistry.OrderHeaderStatusList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.OrderHeaderStatusList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public ReadOnlyCodeDescriptionPairList OrderLineStatusList
		{
			get { return RawRegistry.OrderLineStatusList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.OrderLineStatusList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region Documents

		public string ExcelPasswordForOpening
		{
			get { return (string)RawRegistry.ExcelPasswordForOpening.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public string ExcelPasswordForModifying
		{
			get { return (string)RawRegistry.ExcelPasswordForModifying.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public bool DeliverReportsInBackground
		{
			get { return (bool)RawRegistry.DeliverReportsInBackground.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.DeliverReportsInBackground.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string ExcelPrinterInternationalFormats
		{
			get { return (string)RawRegistry.ExcelPrinterInternationalFormats.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.ExcelPrinterInternationalFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string DisbursementNoteTitle
		{
			get { return (MultilingualString)RawRegistry.DisbursementNoteTitle.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public DocumentOpenCloseText CFSCartageAdvice
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.CFSCartageAdviceOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(MultilingualString)RawRegistry.CFSCartageAdviceClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText BookingCartageAdvice
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.BookingCartageAdviceOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(MultilingualString)RawRegistry.BookingCartageAdviceClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText LocalCartageCartageAdvice
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.LocalCartageCartageAdviceOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(MultilingualString)RawRegistry.LocalCartageCartageAdviceClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText CustomsDeclarationCartageAdvice
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.CustomsDeclarationCartageAdviceOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(MultilingualString)RawRegistry.CustomsDeclarationCartageAdviceClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText CartageAdviceExport
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.CartageAdviceExportOpeningText.Value,
					(MultilingualString)RawRegistry.CartageAdviceExportClosingText.Value);
			}
		}

		public DocumentOpenCloseText CartageAdviceImport
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.CartageAdviceImportOpeningText.Value,
					(MultilingualString)RawRegistry.CartageAdviceImportClosingText.Value);
			}
		}

		public DocumentOpenCloseText ConsignmentRequestForService
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.ConsignmentRequestForServiceOpeningText.Value,
					(MultilingualString)RawRegistry.ConsignmentRequestForServiceClosingText.Value);
			}
		}

		public DocumentOpenCloseText ConsignmentAuthorizationForService
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.ConsignmentAuthorizationForServiceOpeningText.Value,
					(MultilingualString)RawRegistry.ConsignmentAuthorizationForServiceClosingText.Value);
			}
		}

		public string CartageAdviceTimeSlotRequestExportOpeningText
		{
			get { return (MultilingualString)RawRegistry.CartageAdviceTimeSlotRequestExportOpeningText.Value; }
		}

		public string CartageAdviceTimeSlotRequestImportOpeningText
		{
			get { return (MultilingualString)RawRegistry.CartageAdviceTimeSlotRequestImportOpeningText.Value; }
		}

		public string CartageAdviceTimeSlotConfirmationOpeningText
		{
			get { return (MultilingualString)RawRegistry.CartageAdviceTimeSlotConfirmationOpeningText.Value; }
		}

		public string CartageAdviceTimeSlotConfirmationClosingText
		{
			get { return (MultilingualString)RawRegistry.CartageAdviceTimeSlotConfirmationClosingText.Value; }
		}

		public string ExportCertificationOpeningText
		{
			get { return (MultilingualString)RawRegistry.ExportCertificationOpeningText.Value; }
		}

		public string ExportCertificationClosingText
		{
			get { return (MultilingualString)RawRegistry.ExportCertificationClosingText.Value; }
		}

		public string SignOffText
		{
			get { return RawRegistry.SignOffText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public ColourDepth PDFTIFColourDepth
		{
			get
			{
				string colourDepthText = (string)RawRegistry.PDFTIFColourDepth.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				switch (colourDepthText)
				{
					case Constants.ColourDepth.BlackAndWhite:
						return ColourDepth.BlackAndWhite;
					case Constants.ColourDepth.TrueColour:
						return ColourDepth.TrueColour;
					case Constants.ColourDepth.Colour256:
					default:
						return ColourDepth.Colour256;
				}
			}
		}

		public int PDFTIFResolution
		{
			get { return (int)RawRegistry.PDFTIFResolution.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public DocumentOpenCloseText DefaultDocumentText
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.DocumentOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(MultilingualString)RawRegistry.DocumentClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public bool ShowSystemGeneratedContactsOnOrgDocuments
		{
			get { return (bool)RawRegistry.ShowSystemGeneratedContactsOnOrgDocuments.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ShowSystemGeneratedContactsOnOrgDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public DocumentOpenCloseText ImportAirPreAlert
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportAirPreAlertOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportAirPreAlertClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportAirArrivalNotice
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportAirArrivalNoticeOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportAirArrivalNoticeClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportAirUltimateConsigneePreAlert
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportAirUltimateConsigneePreAlertOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportAirUltimateConsigneePreAlertClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportAirUltimateConsigneeArrivalNotice
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportAirUltimateConsigneeArrivalNoticeOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportAirUltimateConsigneeArrivalNoticeClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportAirShippingAdvice
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportAirShippingAdviceOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportAirShippingAdviceClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportAirFreightDeliveryOrder
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportAirFreightDeliveryOrderOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportAirFreightDeliveryOrderClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportAirOutturnReport
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportAirOutturnReportOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportAirOutturnReportClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportSeaFreightPreAlert
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportSeaFreightPreAlertOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportSeaFreightPreAlertClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportSeaFreightArrivalNotice
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportSeaFreightArrivalNoticeOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportSeaFreightArrivalNoticeClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportSeaFreightDeliveryOrder
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportSeaFreightDeliveryOrderOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportSeaFreightDeliveryOrderClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportSeaUltimateConsigneePreAlert
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportSeaUltimateConsigneePreAlertOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportSeaUltimateConsigneePreAlertClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportSeaUltimateConsigneeArrivalNotice
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportSeaUltimateConsigneeArrivalNoticeOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportSeaUltimateConsigneeArrivalNoticeClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportSeaShippingAdvice
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportSeaShippingAdviceOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportSeaShippingAdviceClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportSeaOutturnReport
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ImportSeaOutturnReportOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ImportSeaOutturnReportClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ExportAirBookingConfirmation
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ExportAirBookingConfirmationOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ExportAirBookingConfirmationClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public bool ShowChargesOnBookingsBookingConfirmation
		{
			get { return (bool)RawRegistry.ShowChargesOnBookingsBookingConfirmation.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.ShowChargesOnBookingsBookingConfirmation.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, value); }
#endif
		}

		public bool ShowChargesOnForwardingBookingConfirmation
		{
			get { return (bool)RawRegistry.ShowChargesOnForwardingBookingConfirmation.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.ShowChargesOnForwardingBookingConfirmation.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, value); }
#endif
		}

		public bool ShowMarksAndNumbersForFCL
		{
			get { return (bool)RawRegistry.ShowMarksAndNumbersForFCL.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.ShowMarksAndNumbersForFCL.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool ShowMarksAndNumbersForNonFCL
		{
			get { return (bool)RawRegistry.ShowMarksAndNumbersForNonFCL.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.ShowMarksAndNumbersForNonFCL.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool ShowMarksAndNumbersForLSE
		{
			get { return (bool)RawRegistry.ShowMarksAndNumbersForLSE.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.ShowMarksAndNumbersForLSE.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool ShowMarksAndNumbersForNonLSE
		{
			get { return (bool)RawRegistry.ShowMarksAndNumbersForNonLSE.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.ShowMarksAndNumbersForNonLSE.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string MarksAndNumbersHeadingForFCL
		{
			get { return (MultilingualString)RawRegistry.MarksAndNumbersHeadingForFCL.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.MarksAndNumbersHeadingForFCL.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string MarksAndNumbersHeadingForNonFCL
		{
			get { return (MultilingualString)RawRegistry.MarksAndNumbersHeadingForNonFCL.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.MarksAndNumbersHeadingForNonFCL.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string MarksAndNumbersHeadingForLSE
		{
			get { return (MultilingualString)RawRegistry.MarksAndNumbersHeadingForLSE.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.MarksAndNumbersHeadingForLSE.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string MarksAndNumbersHeadingForNonLSE
		{
			get { return (MultilingualString)RawRegistry.MarksAndNumbersHeadingForNonLSE.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.MarksAndNumbersHeadingForNonLSE.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public DocumentOpenCloseText ExportAirFreightShipperDepartureNotice
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ExportAirFreightShipperDepartureNoticeOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ExportAirFreightShipperDepartureNoticeClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ExportAirLetterToOverseasAgent
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ExportAirLetterToOverseasAgentOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ExportAirLetterToOverseasAgentClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ExportAirFreightAgentDepartureNotice
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ExportAirFreightAgentDepartureNoticeOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ExportAirFreightAgentDepartureNoticeClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText AirAgentsInstruction
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.AirAgentsInstructionOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.AirAgentsInstructionClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ExportSeaBookingConfirmation
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ExportSeaBookingConfirmationOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ExportSeaBookingConfirmationClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ExportSeaFreightShipperDepartureNotice
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ExportSeaFreightShipperDepartureNoticeOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ExportSeaFreightShipperDepartureNoticeClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ExportSeaFreightAgentDepartureNotice
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ExportSeaFreightAgentDepartureNoticeOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ExportSeaFreightAgentDepartureNoticeClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ExportSeaLetterToOverseasAgent
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.ExportSeaLetterToOverseasAgentOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.ExportSeaLetterToOverseasAgentClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText SeaAgentsInstruction
		{
			get
			{
				return new DocumentOpenCloseText(
					(string)RawRegistry.SeaAgentsInstructionOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(string)RawRegistry.SeaAgentsInstructionClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText CustomsRequestForMissingDocuments
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.CustomsRequestForMissingDocumentsOpeningText.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty),
					(MultilingualString)RawRegistry.CustomsRequestForMissingDocumentsClosingText.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			}
#if DEBUG
			set
			{
				RawRegistry.CustomsRequestForMissingDocumentsOpeningText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.OpeningText);
				RawRegistry.CustomsRequestForMissingDocumentsClosingText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ClosingText);
			}
#endif
		}

		public string CustomsRequestForMissingDocumentsClause
		{
			get { return (MultilingualString)RawRegistry.CustomsRequestForMissingDocumentsClause.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public DocumentOpenCloseText SeaWeightsAndMeasurements
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.SeaWeightsAndMeasurementsOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(MultilingualString)RawRegistry.SeaWeightsAndMeasurementsClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText AirWeightsAndMeasurements
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.AirWeightsAndMeasurementsOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(MultilingualString)RawRegistry.AirWeightsAndMeasurementsClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText AWBSecurityDeclaration
		{
			get
			{
				MultilingualString result = (MultilingualString)RawRegistry.AWBSecurityDeclarationOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
				return new DocumentOpenCloseText(result ?? string.Empty, string.Empty);
			}
#if DEBUG
			set { RawRegistry.AWBSecurityDeclarationOpeningText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.OpeningText); }
#endif
		}

		#region Order Documents

		public DocumentOpenCloseText ImportOrderAdvice
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.ImportOrderAdviceOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(MultilingualString)RawRegistry.ImportOrderAdviceClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ExportOrderAdvice
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.ExportOrderAdviceOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(MultilingualString)RawRegistry.ExportOrderAdviceClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportOrderNotification
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.ImportOrderNotificationOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(MultilingualString)RawRegistry.ImportOrderNotificationClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ExportOrderNotification
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.ExportOrderNotificationOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(MultilingualString)RawRegistry.ExportOrderNotificationClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportOrderStatus
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.ImportOrderStatusOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(MultilingualString)RawRegistry.ImportOrderStatusClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ExportOrderStatus
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.ExportOrderStatusOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					(MultilingualString)RawRegistry.ExportOrderStatusClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportOrderShippedOnBoardAdvice
		{
			get
			{
				return new DocumentOpenCloseText(
						(MultilingualString)RawRegistry.ImportShippedOnBoardAdviceOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
						(MultilingualString)RawRegistry.ImportShippedOnBoardAdviceClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseText ImportOrderAmendmentToBooking
		{
			get
			{
				return new DocumentOpenCloseText(
						(MultilingualString)RawRegistry.ImportAmendmentToBookingOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
						(MultilingualString)RawRegistry.ImportAmendmentToBookingClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		#endregion

		#region WeightAndVolumeDisplay

		public string PreAlertWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.PreAlertWeightAndVolumeDisplay); }
#if DEBUG
			set { RawRegistry.PreAlertWeightAndVolumeDisplay.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string ArrivalNoticeWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.ArrivalNoticeWeightAndVolumeDisplay); }
		}

		public string ShippingAdviceWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.ShippingAdviceWeightAndVolumeDisplay); }
		}

		public string DeliveryOrderWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.DeliveryOrderWeightAndVolumeDisplay); }
		}

		public string OutturnReportWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.OutturnReportWeightAndVolumeDisplay); }
		}

		public string BookingConfirmationWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.BookingConfirmationWeightAndVolumeDisplay); }
		}

		public string ShipperDepartureNoticeWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.ShipperDepartureNoticeWeightAndVolumeDisplay); }
		}

		public string AgentsInstructionNoticeWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.AgentsInstructionNoticeWeightAndVolumeDisplay); }
		}

		public string CartageAdviceImportWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.CartageAdviceImportWeightAndVolumeDisplay); }
		}

		public string CartageAdviceExportWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.CartageAdviceExportWeightAndVolumeDisplay); }
		}

		public string BillOfLadingWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.BillOfLadingWeightAndVolumeDisplay); }
		}

		public string CoLoadMasterManifestWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.CoLoadMasterManifestWeightAndVolumeDisplay); }
		}

		public bool CoLoadMasterManifestDisplayVolumeWhenAir
		{
			get { return (bool)RawRegistry.CoLoadMasterManifestDisplayVolumeWhenAir.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public bool CoLoadMasterManifestDisplayChargeableWhenAir
		{
			get { return (bool)RawRegistry.CoLoadMasterManifestDisplayChargeableWhenAir.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public bool CoLoadMasterManifestDisplayChargeableWhenSea
		{
			get { return (bool)RawRegistry.CoLoadMasterManifestDisplayChargeableWhenSea.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public DocumentOpenCloseText ShipmentRequestForMissingDocuments
		{
			get
			{
				return new DocumentOpenCloseText(
					(MultilingualString)RawRegistry.ShipmentRequestForMissingDocumentsOpeningText.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty),
					(MultilingualString)RawRegistry.ShipmentRequestForMissingDocumentsClosingText.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			}
#if DEBUG
			set
			{
				RawRegistry.ShipmentRequestForMissingDocumentsOpeningText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.OpeningText);
				RawRegistry.ShipmentRequestForMissingDocumentsClosingText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ClosingText);
			}
#endif
		}

		public string ShipmentRequestForMissingDocumentsClause
		{
			get { return (MultilingualString)RawRegistry.ShipmentRequestForMissingDocumentsClause.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public string AuthorisationReleaseClause
		{
			get { return (MultilingualString)RawRegistry.AuthorisationReleaseClause.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public string ConsolManifestConsolImportWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.ConsolManifestConsolImportWeightAndVolumeDisplay); }
		}

		public string ConsolManifestConsolExportWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.ConsolManifestConsolExportWeightAndVolumeDisplay); }
		}

		public bool ConsolManifestConsolExportDisplayVolumewhenAir
		{
			get { return (bool)RawRegistry.ConsolManifestConsolExportDisplayVolumewhenAir.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.ConsolManifestConsolExportDisplayVolumewhenAir.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool ConsolManifestConsolExportDisplayChargeableWhenAir
		{
			get { return (bool)RawRegistry.ConsolManifestConsolExportDisplayChargeableWhenAir.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public bool ConsolManifestConsolExportDisplayChargeableWhenSea
		{
			get { return (bool)RawRegistry.ConsolManifestConsolExportDisplayChargeableWhenSea.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public string ConsolForwardingInstructionWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.ConsolForwardingInstructionWeightAndVolumeDisplay); }
#if DEBUG
			set { RawRegistry.ConsolForwardingInstructionWeightAndVolumeDisplay.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		string GetWeightAndVolumeDisplayType(IRegistryItem item)
		{
			string value = (string)item.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			return string.IsNullOrEmpty(value) ? item.DefaultValue.ToString() : value;
		}

		public string ConsolShipperDepartureNoticeWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.ConsolShipperDepartureNoticeWeightAndVolumeDisplay); }
		}

		public string ConsolAgentDepartureNoticeWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.ConsolAgentDepartureNoticeWeightAndVolumeDisplay); }
		}

		public string ConsolCargoLoadListWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.ConsolCargoLoadListWeightAndVolumeDisplay); }
		}

		public string ConsolLetterToOverseasAgentWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.ConsolLetterToOverseasAgentWeightAndVolumeDisplay); }
		}

		public string NotifyPartyDefaultText
		{
			get { return (MultilingualString)RawRegistry.NotifyPartyDefaultText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.NotifyPartyDefaultText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int WeightMinimumDecimalPlacesToDisplay
		{
			get { return (int)RawRegistry.WeightMinimumDecimalPlacesToDisplay.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public int VolumeMinimumDecimalPlacesToDisplay
		{
			get { return (int)RawRegistry.VolumeMinimumDecimalPlacesToDisplay.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public bool DisplayTareAndGrossWeightOnHBOL
		{
			get { return (bool)RawRegistry.DisplayTareAndGrossWeightOnHBOL.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.DisplayTareAndGrossWeightOnHBOL.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		public string RoutingOrderOpeningText
		{
			get { return (MultilingualString)RawRegistry.RoutingOrderOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public string RoutingOrderClosingText
		{
			get { return (MultilingualString)RawRegistry.RoutingOrderClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public string AgentReplacementRoutingOrderOpeningText
		{
			get { return (MultilingualString)RawRegistry.AgentReplacementRoutingOrderOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.AgentReplacementRoutingOrderOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string AgentReplacementRoutingOrderClosingText
		{
			get { return (MultilingualString)RawRegistry.AgentReplacementRoutingOrderClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.AgentReplacementRoutingOrderClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string RoutingRecommendationOpeningText
		{
			get { return (MultilingualString)RawRegistry.RoutingRecommendationOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public string RoutingRecommendationClosingText
		{
			get { return (MultilingualString)RawRegistry.RoutingRecommendationClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public string CoverPageText
		{
			get { return (MultilingualString)RawRegistry.CoverPageText.Value; }
		}

		public string CoverPageTextOneOff
		{
			get { return (MultilingualString)RawRegistry.CoverPageTextOneOff.Value; }
		}

		public string CoverPageTextNew
		{
			get { return (MultilingualString)RawRegistry.CoverPageTextNew.Value; }
		}

		public string CoverPageTextOneOffNew
		{
			get { return (MultilingualString)RawRegistry.CoverPageTextOneOffNew.Value; }
		}

		#endregion

		#region AutoRating

		public Guid GetFreightChargeCode(Guid companyPK)
		{
			return RawRegistry.FreightChargeCode.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
		}

		/// <summary>
		/// Retrieved from dbo.Stmdata with fall back, default is "FRT".
		/// </summary>
		public Guid FreightChargeCode
		{
			get { return GetFreightChargeCode(EnvProxy.Instance.CurrentCompany.PK); }
#if DEBUG
			set { RawRegistry.FreightChargeCode.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region Customs

		public string BrokerageID
		{
			get { return (string)RawRegistry.BrokerageID.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.BrokerageID.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public bool RequestOfficialCustomsPaymentReceipt
		{
			get { return (bool)RawRegistry.RequestOfficialCustomsPaymentReceipt.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.RequestOfficialCustomsPaymentReceipt.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool UpdateCMRReferenceFilesWhenModifiedByCustoms
		{
			get { return (bool)RawRegistry.UpdateCMRReferenceFilesWhenModifiedByCustoms.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.UpdateCMRReferenceFilesWhenModifiedByCustoms.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid CustomsPaymentBankAccount
		{
			get { return (Guid)RawRegistry.CustomsPaymentBankAccount.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public Guid CustomsSecondPaymentBankAccount
		{
			get { return (Guid)RawRegistry.CustomsSecondPaymentBankAccount.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public void SetCustomsPaymentBankAccountForCurrentCompany(Guid bankAccountPK)
		{
			RawRegistry.CustomsPaymentBankAccount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, bankAccountPK);
		}

		public void SetCustomsSecondPaymentBankAccountForCurrentCompany(Guid bankAccountPK)
		{
			RawRegistry.CustomsSecondPaymentBankAccount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, bankAccountPK);
		}

		public int InterchangeResendDelay
		{
			get { return (int)RawRegistry.InterchangeResendDelay.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.InterchangeResendDelay.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public int InterchangeMaxSends
		{
			get { return (int)RawRegistry.InterchangeMaxSends.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.InterchangeMaxSends.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public bool EnableCustomsDiagnostics
		{
			get { return RawRegistry.EnableCustomsDiagnostics.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public string CommercialInvoiceLineMergeMethod
		{
			get { return (string)RawRegistry.CommercialInvoiceLineMergeMethod.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public string GetCommercialInvoiceLineMergeMethod(Guid companyPK)
		{
			return (string)RawRegistry.CommercialInvoiceLineMergeMethod.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
		}

		public void SetCommercialInvoiceLineMergeMethod(Guid companyPK, string newValue)
		{
			RawRegistry.CommercialInvoiceLineMergeMethod.SetValue(companyPK, Guid.Empty, Guid.Empty, newValue);
		}

		public string ediTariffInstallationDirectory
		{
			get { return (string)RawRegistry.ediTariffInstallationDirectory.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public string ExternalBorderComplianceTool
		{
			get { return RawRegistry.ExternalBorderComplianceTool.Value; }
#if DEBUG
			set { RawRegistry.ExternalBorderComplianceTool.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public const string BorderWiseAPIKey = "NjAyMDRmN2MtMjA1OC00YjY4LWI1ZTEtZmQ0NDY5ZGE3ZTA2";

		public string BorderWiseUmpApiBaseAddress
		{
			get { return RawRegistry.BorderWiseUmpApiBaseAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.BorderWiseUmpApiBaseAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string BorderWiseWebAddress
		{
			get { return RawRegistry.BorderWiseWebAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public string BorderWiseWebSocketHubUrl
		{
			get { return RawRegistry.BorderWiseWebSocketHubUrl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.BorderWiseWebSocketHubUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool BorderWiseEnableWebSocketClient
		{
			get { return RawRegistry.BorderWiseEnableWebSocketClient.Value; }
#if DEBUG
			set { RawRegistry.BorderWiseEnableWebSocketClient.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool BorderWiseEnableMultilineTariffClassification
		{
			get { return RawRegistry.BorderWiseEnableMultilineTariffClassification.Value; }
#if DEBUG
			set { RawRegistry.BorderWiseEnableMultilineTariffClassification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool BorderWiseEnableMultilineTariffClassificationServiceTask
		{
			get { return RawRegistry.BorderWiseEnableMultilineTariffClassificationServiceTask.Value; }
#if DEBUG
			set { RawRegistry.BorderWiseEnableMultilineTariffClassificationServiceTask.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string BorderWiseApiKey
		{
			get { return RawRegistry.BorderWiseApiKey.Value; }
#if DEBUG
			set { RawRegistry.BorderWiseApiKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool AutoPopulateHAWBsOnConsol
		{
			get { return RawRegistry.AutoPopulateHAWBsOnConsol.Value; }
#if DEBUG
			set { RawRegistry.AutoPopulateHAWBsOnConsol.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool LandedCostingFallbackExRatesToJobInvoicing
		{
			get { return RawRegistry.LandedCostingFallbackExRatesToJobInvoicing.Value; }
#if DEBUG
			set { RawRegistry.LandedCostingFallbackExRatesToJobInvoicing.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int MessagesPerInterchange
		{
			get { return RawRegistry.MessagesPerInterchange.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.MessagesPerInterchange.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int InterchangesPerRun
		{
			get { return RawRegistry.InterchangesPerRun.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		#endregion

		#region AUCustoms

		public int MaximumConsignmentsLines
		{
			get { return RawRegistry.MaximumConsignmentsLines.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MaximumConsignmentsLines.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int AUCustomsTolerancePercentage
		{
			get { return (int)RawRegistry.AUCustomsTolerancePercentage.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AUCustomsTolerancePercentage.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public decimal AUCustomsToleranceAmount
		{
			get { return (decimal)RawRegistry.AUCustomsToleranceAmount.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AUCustomsToleranceAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public decimal AUCustomsRefundToleranceAmount
		{
			get { return (decimal)RawRegistry.AUCustomsRefundToleranceAmount.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AUCustomsRefundToleranceAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string AUCustomsSenderID
		{
			get { return (string)RawRegistry.AUCustomsSenderID.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AUCustomsSenderID.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string GetAUCustomsSenderID(Guid companyPK, Guid branchPK)
		{
			return (string)RawRegistry.AUCustomsSenderID.GetFallBackValueAtAllLevels(companyPK, branchPK, EnvProxy.Instance.CurrentDepartment.PK);
		}

		public string GetAUCustomsSeaCargoDepotMailbox(Guid companyPK, Guid branchPK)
		{
			return (string)RawRegistry.AUCustomsSeaCargoDepotMailbox.GetFallBackValueAtAllLevels(companyPK, branchPK, EnvProxy.Instance.CurrentDepartment.PK);
		}

		public void SetAUCustomsSenderIDForBranch(Guid branchPK, string value)
		{
			RawRegistry.AUCustomsSenderID.SetValue(Guid.Empty, branchPK, Guid.Empty, value);
		}

		public void SetAUCustomsSenderIDForCpmpany(Guid companyPK, string value)
		{
			RawRegistry.AUCustomsSenderID.SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		public void SetAUCustomsSeaCargoDepotMailboxForBranch(Guid branchPK, string value)
		{
			RawRegistry.AUCustomsSeaCargoDepotMailbox.SetValue(Guid.Empty, branchPK, Guid.Empty, value);
		}

		public void SetAUCustomsSeaCargoDepotMailboxForCompany(Guid companyPK, string value)
		{
			RawRegistry.AUCustomsSeaCargoDepotMailbox.SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		public string AUCustomsEdificeSenderID
		{
			get { return (string)RawRegistry.AUCustomsEdificeSenderID.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AUCustomsEdificeSenderID.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string GetAUCustomsEdificeSenderID(Guid companyPK, Guid branchPK)
		{
			return (string)RawRegistry.AUCustomsEdificeSenderID.GetFallBackValueAtAllLevels(companyPK, branchPK, EnvProxy.Instance.CurrentDepartment.PK);
		}

		public void SetAUCustomsEdificeSenderIDForBranch(Guid branchPK, string value)
		{
			RawRegistry.AUCustomsEdificeSenderID.SetValue(Guid.Empty, branchPK, Guid.Empty, value);
		}

		public void SetAUCustomsEdificeSenderIDForCompany(Guid companyPK, string value)
		{
			RawRegistry.AUCustomsEdificeSenderID.SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		public string AUCustomsCompileSiteID(Guid branchPK)
		{
			return (string)RawRegistry.AUCustomsCompileSiteID.GetValueWithoutFallback(Guid.Empty, branchPK, Guid.Empty);
		}

		public void SetAUCustomsCompileSiteID(Guid branchPK, string siteID)
		{
			RawRegistry.AUCustomsCompileSiteID.SetValue(Guid.Empty, branchPK, Guid.Empty, siteID);
		}

		public bool CMRTestMode
		{
			get { return (bool)RawRegistry.CMRTestMode.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK); }
			set { RawRegistry.CMRTestMode.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public bool GetCMRTestMode(Guid companyPK)
		{
			return (bool)RawRegistry.CMRTestMode.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
		}

		public void SetCMRTestModeForCompany(Guid companyPK, bool value)
		{
			RawRegistry.CMRTestMode.SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		#region Air Cargo

		public bool AUCustomsAirCargoTestMode
		{
			get { return (bool)RawRegistry.AUCustomsAirCargoTestMode.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
			set { RawRegistry.AUCustomsAirCargoTestMode.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
		}

		public bool GetAUCustomsAirCargoTestMode(Guid companyPK, Guid branchPK)
		{
			return (bool)RawRegistry.AUCustomsAirCargoTestMode.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		public void SetAUCustomsAirCargoTestModeForCompany(Guid companyPK, bool value)
		{
			RawRegistry.AUCustomsAirCargoTestMode.SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		public void SetAUCustomsAirCargoTestModeForBranch(Guid branchPK, bool value)
		{
			RawRegistry.AUCustomsAirCargoTestMode.SetValue(Guid.Empty, branchPK, Guid.Empty, value);
		}

		#endregion

		public bool AUCustomsSeaCargoTestMode
		{
			get { return (bool)RawRegistry.AUCustomsSeaCargoTestMode.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
			set { RawRegistry.AUCustomsSeaCargoTestMode.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
		}

		public void SetAUCustomsSeaCargoTestModeForCompany(Guid companyPK, bool value)
		{
			RawRegistry.AUCustomsSeaCargoTestMode.SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		public void SetAUCustomsSeaCargoTestModeForBranch(Guid branchPK, bool value)
		{
			RawRegistry.AUCustomsSeaCargoTestMode.SetValue(Guid.Empty, branchPK, Guid.Empty, value);
		}

		public bool GetAUCustomsSeaCargoTestMode(Guid companyPK, Guid branchPK)
		{
			return (bool)RawRegistry.AUCustomsSeaCargoTestMode.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		public bool AQISMessagingTestMode
		{
			get { return (bool)RawRegistry.AQISMessagingTestMode.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
			set { RawRegistry.AQISMessagingTestMode.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public void SetAQISMessagingTestModeForCompany(Guid companyPK, bool value)
		{
			RawRegistry.AQISMessagingTestMode.SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		public bool GetAQISMessagingTestMode(Guid companyPK)
		{
			return (bool)RawRegistry.AQISMessagingTestMode.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
		}

		public AUCustomsRegistry AUCustoms
		{
			get
			{
				if (fAUCustoms == null)
				{
					fAUCustoms = new AUCustomsRegistry(RawRegistry);
				}
				return fAUCustoms;
			}
		}
		AUCustomsRegistry fAUCustoms;
#if DEBUG
		public string AUCustomsImportsMessagingMode
		{
			get { return (string)RawRegistry.AUImportsMessagingMode.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
			set { RawRegistry.AUImportsMessagingMode.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}
#endif
		#endregion

		#region MYCustoms

		public MYCustomsRegistry MYCustoms
		{
			get
			{
				if (fMYCustoms == null)
				{
					fMYCustoms = new MYCustomsRegistry(RawRegistry);
				}
				return fMYCustoms;
			}
		}
		MYCustomsRegistry fMYCustoms;

		#endregion

		#region ZACustoms
		public Enterprise.Core.Environment.ZA.Registry ZACustoms
		{
			get
			{
				if (fZACustoms == null)
				{
					fZACustoms = new Enterprise.Core.Environment.ZA.Registry(RawRegistry);
				}
				return fZACustoms;
			}
		}
		Enterprise.Core.Environment.ZA.Registry fZACustoms;
		#endregion

		#region AECustoms
		public AECustomsRegistry AECustoms
		{
			get
			{
				if (fAECustoms == null)
				{
					fAECustoms = new AECustomsRegistry(RawRegistry);
				}
				return fAECustoms;
			}
		}
		AECustomsRegistry fAECustoms;
		#endregion

		#region CMREdifact

		public byte[] AUCCompanyCertificateData
		{
			get { return RawRegistry.AUCCompanyCertificateData.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AUCCompanyCertificateData.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string AUCCompanyCertificatePassword
		{
			get { return (string)RawRegistry.AUCCompanyCertificatePassword.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AUCCompanyCertificatePassword.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region ReferenceFiles

		public ReferenceFilesRegistry ReferenceFiles
		{
			get
			{
				if (fReferenceFilesRegistry == null)
				{
					fReferenceFilesRegistry = new ReferenceFilesRegistry(RawRegistry);
				}
				return fReferenceFilesRegistry;
			}
		}
		ReferenceFilesRegistry fReferenceFilesRegistry;

		#endregion

		#region Rating

		public RatingRegistry Rating
		{
			get
			{
				if (fRatingRegistry == null)
				{
					fRatingRegistry = new RatingRegistry(RawRegistry);
				}
				return fRatingRegistry;
			}
		}

		RatingRegistry fRatingRegistry;

		public Image GetQuotationDocumentLogo(Guid companyPK, Guid branchPK)
		{
			return (Image)RawRegistry.QuotationDocumentLogo.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		public Image QuotationDocumentLogo
		{
			get { return (Image)RawRegistry.QuotationDocumentLogo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.QuotationDocumentLogo.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string NotChargedText
		{
			get { return (MultilingualString)RawRegistry.NotChargedText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public MultilingualString NotChargedMultilingualText
		{
			get { return (MultilingualString)RawRegistry.NotChargedText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		#endregion

		#region Notification

		public Guid InfrastructureErrorsNotificationGroup
		{
			get { return RawRegistry.InfrastructureErrorsNotificationGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public Guid PostMasterGroup
		{
			get { return (Guid)RawRegistry.NotificationGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public Guid NotificationGroup(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return (Guid)RawRegistry.NotificationGroup.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
		}

		#endregion

		#region Organisation

		#region Organisation Form Lists

		public ReadOnlyCodeDescriptionPairList PayablesCreditAgreedPaymentMethodsList
		{
			get { return RawRegistry.PayablesCreditAgreedPaymentMethodsList.Value; }
		}

		public ReadOnlyCodeDescriptionPairList AddressAccessPointList
		{
			get { return RawRegistry.AddressAccessPointList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList AddressCommunicationRequiredList
		{
			get { return RawRegistry.AddressCommunicationRequiredList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList AddressContainerHandlingList
		{
			get { return RawRegistry.AddressContainerHandlingList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList AddressDockHeightList
		{
			get { return RawRegistry.AddressDockHeightList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList AddressLabourRequiredList
		{
			get { return RawRegistry.AddressLabourRequiredList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList DeliveryRoutesListSorted
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var deliveryRouteListSorted = DeliveryRoutesList.Cast<ICodeDescription>().ToList();
				deliveryRouteListSorted.Sort((x, y) => string.Compare(x.Code, y.Code, StringComparison.Ordinal));
				foreach (var codeDescription in deliveryRouteListSorted)
				{
					result.Add(codeDescription);
				}
				return new ReadOnlyCodeDescriptionPairList(result);
			}
		}

		public ReadOnlyCodeDescriptionPairList DeliveryRoutesList
		{
			get { return RawRegistry.DeliveryRoutesList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.DeliveryRoutesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public ReadOnlyCodeDescriptionPairList AgentCategoryList
		{
			get { return RawRegistry.AgentCategoryList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AgentCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public ReadOnlyCodeDescriptionPairList ARCreditRatingList
		{
			get { return RawRegistry.ARCreditRatingList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList CompetitorActivityList
		{
			get { return RawRegistry.CompetitorActivityList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList CompetitorCategoryList
		{
			get { return RawRegistry.CompetitorCategoryList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList ExporterCategoryList
		{
			get { return RawRegistry.ExporterCategoryList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList ImporterCategoryList
		{
			get { return RawRegistry.ImporterCategoryList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList SalesCategoryList
		{
			get { return RawRegistry.SalesCategoryList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList PayablesCategoryList
		{
			get { return RawRegistry.PayablesCategoryList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList ReceivablesCategoryList
		{
			get { return RawRegistry.ReceivablesCategoryList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList SalesGrowthOutlookList
		{
			get { return RawRegistry.SalesGrowthOutlookList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList SalesEffectOnCostList
		{
			get { return RawRegistry.SalesEffectOnCostList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList CarrierCategoryList
		{
			get { return RawRegistry.CarrierCategoryList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList ServicesCategoryList
		{
			get { return RawRegistry.ServicesCategoryList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList SalesTerritoryList
		{
			get { return RawRegistry.SalesTerritoryList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList SalesStyleList
		{
			get { return RawRegistry.SalesStyleList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList OrgListOfInterests
		{
			get { return RawRegistry.OrgListOfInterests.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList OrgListOfContactAllocations
		{
			get { return RawRegistry.OrgListOfContactAllocations.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList OrgStaffMemberAssignmentRoles
		{
			get { return RawRegistry.OrgStaffMemberAssignmentRoles.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.OrgStaffMemberAssignmentRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region NextCallFollowUpDays

		public int NextCallFollowUpDays
		{
			get { return (int)RawRegistry.NextCallFollowUpDays.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.NextCallFollowUpDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region OrgAllowMixedCase

		public bool OrgAllowMixedCase
		{
			get { return (bool)RawRegistry.OrgAllowMixedCase.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public void SetOrgAllowMixedCase(bool value)
		{
			RawRegistry.OrgAllowMixedCase.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		#endregion

		#region OrgUsePhoneNumberFormatting

		public bool OrgUsePhoneNumberFormatting
		{
			get { return (bool)RawRegistry.OrgUsePhoneNumberFormatting.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public void SetOrgUsePhoneNumberFormatting(bool value)
		{
			RawRegistry.OrgUsePhoneNumberFormatting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		#endregion

		#region DowngradeInvalidPhoneNumbersToAWarning

		public bool DowngradeInvalidPhoneNumbersToAWarning
		{
			get { return (bool)RawRegistry.DowngradeInvalidPhoneNumbersToAWarning.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public void SetDowngradeInvalidPhoneNumbersToAWarning(bool value)
		{
			RawRegistry.DowngradeInvalidPhoneNumbersToAWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		#endregion

		#region NumericValuesOnlyForPhoneNumberFields

		public bool NumericValuesOnlyForPhoneNumberFields
		{
			get { return (bool)RawRegistry.NumericValuesOnlyForPhoneNumberFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public void SetNumericValuesOnlyForPhoneNumberFields(bool value)
		{
			RawRegistry.NumericValuesOnlyForPhoneNumberFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		#endregion

		#region DefaultNoteContextFromCurrentlyLoggedInDepartment

		public bool DefaultNoteContextFromCurrentlyLoggedInDepartment
		{
			get { return (bool)RawRegistry.DefaultNoteContextFromCurrentlyLoggedInDepartment.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public void SetDefaultNoteContextFromCurrentlyLoggedInDepartment(bool value)
		{
			RawRegistry.DefaultNoteContextFromCurrentlyLoggedInDepartment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		#endregion

		#region DefaultNoteCompanyFromCurrentlyLoggedInCompany

		public bool DefaultNoteCompanyFromCurrentlyLoggedInCompany
		{
			get { return (bool)RawRegistry.DefaultNoteCompanyFromCurrentlyLoggedInCompany.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public void SetDefaultNoteCompanyFromCurrentlyLoggedInCompany(bool value)
		{
			RawRegistry.DefaultNoteCompanyFromCurrentlyLoggedInCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		#endregion

		#region OrgClientIntelligenceTabsRegistry

		public bool OrgShowConsigneeConsignorTab
		{
			get { return (bool)RawRegistry.OrgShowConsigneeConsignorTab.Value; }
		}

		public void SetOrgShowConsigneeConsignorTab(bool value)
		{
			RawRegistry.OrgShowConsigneeConsignorTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		public bool OrgShowARTab
		{
			get { return (bool)RawRegistry.OrgShowARTab.Value; }
		}

#if DEBUG

		public void SetOrgShowARTab(bool value)
		{
			RawRegistry.OrgShowARTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

#endif

		#endregion

		#region OrgUserFlag Labels

		public int OrgUserFlagCount
		{
			get { return RawRegistry.OrgUserFlagCount; }
		}

		public string OrgUserFlag1Label
		{
			get { return RawRegistry.OrgUserFlag1Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag2Label
		{
			get { return RawRegistry.OrgUserFlag2Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag3Label
		{
			get { return RawRegistry.OrgUserFlag3Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag4Label
		{
			get { return RawRegistry.OrgUserFlag4Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag5Label
		{
			get { return RawRegistry.OrgUserFlag5Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag6Label
		{
			get { return RawRegistry.OrgUserFlag6Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag7Label
		{
			get { return RawRegistry.OrgUserFlag7Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag8Label
		{
			get { return RawRegistry.OrgUserFlag8Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag9Label
		{
			get { return RawRegistry.OrgUserFlag9Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag10Label
		{
			get { return RawRegistry.OrgUserFlag10Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag11Label
		{
			get { return RawRegistry.OrgUserFlag11Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag12Label
		{
			get { return RawRegistry.OrgUserFlag12Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag13Label
		{
			get { return RawRegistry.OrgUserFlag13Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag14Label
		{
			get { return RawRegistry.OrgUserFlag14Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag15Label
		{
			get { return RawRegistry.OrgUserFlag15Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag16Label
		{
			get { return RawRegistry.OrgUserFlag16Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag17Label
		{
			get { return RawRegistry.OrgUserFlag17Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag18Label
		{
			get { return RawRegistry.OrgUserFlag18Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag19Label
		{
			get { return RawRegistry.OrgUserFlag19Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag20Label
		{
			get { return RawRegistry.OrgUserFlag20Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag21Label
		{
			get { return RawRegistry.OrgUserFlag21Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag22Label
		{
			get { return RawRegistry.OrgUserFlag22Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag23Label
		{
			get { return RawRegistry.OrgUserFlag23Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag24Label
		{
			get { return RawRegistry.OrgUserFlag24Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag25Label
		{
			get { return RawRegistry.OrgUserFlag25Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag26Label
		{
			get { return RawRegistry.OrgUserFlag26Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag27Label
		{
			get { return RawRegistry.OrgUserFlag27Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag28Label
		{
			get { return RawRegistry.OrgUserFlag28Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag29Label
		{
			get { return RawRegistry.OrgUserFlag29Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag30Label
		{
			get { return RawRegistry.OrgUserFlag30Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag31Label
		{
			get { return RawRegistry.OrgUserFlag31Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}

		public string OrgUserFlag32Label
		{
			get { return RawRegistry.OrgUserFlag32Label.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString(); }
		}
		#endregion

		#region TempOrg Required Fields

		#region Get Required Fields

		public OrgRequiredFields GetTempOrgDebtorRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.TempOrgDebtorRequiredFields.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetTempOrgCreditorRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.TempOrgCreditorRequiredFields.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetTempOrgConsigneeRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.TempOrgConsigneeRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetTempOrgConsignorRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.TempOrgConsignorRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetTempOrgSalesRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.TempOrgSalesRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		#endregion

		#region Set Required Fields

		public void SetTempOrgDebtorRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.TempOrgDebtorRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetTempOrgCreditorRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.TempOrgCreditorRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetTempOrgConsigneeRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.TempOrgConsigneeRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetTempOrgConsignorRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.TempOrgConsignorRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetTempOrgSalesRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.TempOrgSalesRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		#endregion

		#endregion

		#region Org Required Fields

		#region Get Required Fields

		public OrgRequiredFields GetOrgBrokerRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgBrokerRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetOrgCarrierRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgCarrierRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetOrgConsigneeRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgConsigneeRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetOrgConsignorRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgConsignorRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetOrgCompetitorRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgCompetitorRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetOrgContainerYardRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgContainerYardRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetOrgCreditorRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgCreditorRequiredFields.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetOrgCTORequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgCTORequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetOrgDebtorRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgDebtorRequiredFields.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetOrgForwarderRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgForwarderRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public void SetOrgBrokerRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgBrokerRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public OrgRequiredFields GetOrgPackDepotRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgPackDepotRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetOrgSalesLeadRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgSalesLeadRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetOrgTransportClientRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgTransportClientRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		public OrgRequiredFields GetOrgWarehouseRequiredFields()
		{
			byte[] values = (byte[])RawRegistry.OrgWarehouseRequiredFields.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			return new OrgRequiredFields(values);
		}

		#endregion

		#region Set Required Fields

		public void SetOrgCarrierRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgCarrierRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgConsigneeRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgConsigneeRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgConsignorRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgConsignorRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgCompetitorRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgCompetitorRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgContainerYardRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgContainerYardRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgCreditorRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgCreditorRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgCreditorRequiredFieldsForSpecificCompany(Guid companyPK, OrgRequiredFields fields)
		{
			RawRegistry.OrgCreditorRequiredFields.SetValue(companyPK, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgCTORequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgCTORequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgDebtorRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgDebtorRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgDebtorRequiredFieldsForSpecificCompany(Guid companyPK, OrgRequiredFields fields)
		{
			RawRegistry.OrgDebtorRequiredFields.SetValue(companyPK, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgForwarderRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgForwarderRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgPackDepotRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgPackDepotRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgSalesLeadRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgSalesLeadRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgTransportClientRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgTransportClientRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		public void SetOrgWarehouseRequiredFields(OrgRequiredFields fields)
		{
			RawRegistry.OrgWarehouseRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fields.GetRegistryValue());
		}

		#endregion

		#endregion

		#region Org Codes

		public bool CanUserEditOrganisationCode
		{
			get { return (bool)RawRegistry.CanUserEditOrganisationCode.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.CanUserEditOrganisationCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region Buyer Supplier Relationships

		public bool UseBuyerSupplierRelationships
		{
			get { return (bool)RawRegistry.UseBuyerSupplierRelationships.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.UseBuyerSupplierRelationships.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool PromptToSaveBuyerSupplier
		{
			get { return (bool)RawRegistry.PromptToSaveBuyerSupplier.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PromptToSaveBuyerSupplier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		public byte GlobalTariffDefault
		{
			get { return (byte)(int)RawRegistry.GlobalTariffDefault.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.GlobalTariffDefault.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, (int)value); }
#endif
		}

		#endregion

		#region UserInterface

		public NavBarStyles GetNavBarState(Guid userPK)
		{
			string state = (string)RawRegistry.NavBarState.GetValueWithoutFallback(userPK, Guid.Empty, Guid.Empty);
			return (NavBarStyles)Enum.Parse(typeof(NavBarStyles), state, true);
		}

		public void SetNavBarState(Guid userPK, NavBarStyles style)
		{
			RawRegistry.NavBarState.SetValue(userPK, Guid.Empty, Guid.Empty, style.ToString());
		}

		public string GetTimeLineView(Guid webUserPK)
		{
			return (string)RawRegistry.TimeLineView.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, webUserPK);
		}

		public void SetTimeLineView(Guid webUserPK, string style)
		{
			RawRegistry.TimeLineView.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, webUserPK, style);
		}

		public string GetShowUnshippedOrdersMode(Guid webUserPK)
		{
			return (string)RawRegistry.ShowUnshippedOrders.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, webUserPK);
		}

		public void SetShowUnshippedOrdersMode(Guid webUserPK, string mode)
		{
			RawRegistry.ShowUnshippedOrders.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, webUserPK, mode);
		}

		public WebUserDefaultSettingsForDocAddress GetWebUserDefaultSettingsForDocAddress(Guid webUserPK)
		{
			string value = (string)RawRegistry.WebUserDefaultSettingsForDocAddress.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, webUserPK);
			return new WebUserDefaultSettingsForDocAddress(value);
		}

		public void SetWebUserDefaultSettingsForDocAddress(Guid webUserPK, WebUserDefaultSettingsForDocAddress settings)
		{
			RawRegistry.WebUserDefaultSettingsForDocAddress.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, webUserPK, settings.ToString());
		}

		public string LastAccessedModule
		{
			get { return (string)RawRegistry.LastAccessedModule.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.LastAccessedModule.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string GlobalFormTopCaption
		{
			get { return (string)RawRegistry.GlobalFormTopCaption.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.GlobalFormTopCaption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ShowDatabaseName
		{
			get { return (bool)RawRegistry.ShowDatabaseName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ShowDatabaseName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ShowBranchName
		{
			get { return (bool)RawRegistry.ShowBranchName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ShowBranchName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ShowCompanyName
		{
			get { return (bool)RawRegistry.ShowCompanyName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ShowCompanyName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ShowDepartmentName
		{
			get { return (bool)RawRegistry.ShowDepartmentName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ShowDepartmentName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ShowUserName
		{
			get { return (bool)RawRegistry.ShowUserName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ShowUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region Region Number Format

		public string NumberGroupSeparator
		{
			get { return RawRegistry.NumberGroupSeparator.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.NumberGroupSeparator.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string NumberDecimalSeparator
		{
			get { return RawRegistry.NumberDecimalSeparator.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.NumberDecimalSeparator.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string NumberGroupSizes
		{
			get { return RawRegistry.NumberGroupSizes.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.NumberGroupSizes.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string CurrencyGroupSeparator
		{
			get { return RawRegistry.CurrencyGroupSeparator.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.CurrencyGroupSeparator.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string CurrencyDecimalSeparator
		{
			get { return RawRegistry.CurrencyDecimalSeparator.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.CurrencyDecimalSeparator.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string CurrencyGroupSizes
		{
			get { return RawRegistry.CurrencyGroupSizes.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.CurrencyGroupSizes.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region Password Control

		public int PasswordHistoryCount
		{
			get { return (int)RawRegistry.PasswordHistoryCount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PasswordHistoryCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int PasswordHashingIterationsCount
		{
			get { return (int)RawRegistry.PasswordHashingIterationsCount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PasswordHashingIterationsCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int PasswordChangeDays
		{
			get { return (int)RawRegistry.PasswordChangeDays.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PasswordChangeDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int PromptPasswordChangeBeforeExpireDays
		{
			get { return (int)RawRegistry.PromptPasswordChangeBeforeExpireDays.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PromptPasswordChangeBeforeExpireDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int LoginAttempts
		{
			get { return (int)RawRegistry.LoginAttempts.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int LoginLockoutMinutes
		{
			get { return (int)RawRegistry.LoginLockoutMinutes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public int PasswordMinLength
		{
			get { return (int)RawRegistry.PasswordMinLength.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.PasswordMinLength.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int PasswordMinUpperAlphas
		{
			get { return (int)RawRegistry.PasswordMinUpperAlphas.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.PasswordMinUpperAlphas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int PasswordMinLowerAlphas
		{
			get { return (int)RawRegistry.PasswordMinLowerAlphas.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.PasswordMinLowerAlphas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int PasswordMinNumeric
		{
			get { return (int)RawRegistry.PasswordMinNumeric.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.PasswordMinNumeric.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int PasswordMinNonAlphNums
		{
			get { return (int)RawRegistry.PasswordMinNonAlphNums.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.PasswordMinNonAlphNums.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ShowAvailableBranchesOnly
		{
			get { return (bool)RawRegistry.ShowAvailableBranchesOnly.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ShowAvailableBranchesOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ShowAvailableDepartmentsOnly
		{
			get { return (bool)RawRegistry.ShowAvailableDepartmentsOnly.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ShowAvailableDepartmentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region DocumentScanning

		public BurnCDSettingsStruct BurnCDSettings
		{
			get
			{
				byte[] registryValue = (byte[])RawRegistry.BurnCDSettings.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty);
				return new BurnCDSettingsStruct(registryValue);
			}
			set { RawRegistry.BurnCDSettings.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value.GetValue()); }
		}

		public string DoNotDisplayReleaseNotesVersion
		{
			get { return (string)RawRegistry.DoNotDisplayReleaseNotesVersion.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DoNotDisplayReleaseNotesVersion.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value); }
		}

		public bool DoNotDisplayUpdatesOnLogin
		{
			get { return (bool)RawRegistry.DoNotDisplayUpdatesOnLogin.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DoNotDisplayUpdatesOnLogin.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value); }
		}

		public bool DoNotDisplayUpdatesOnModuleEntry
		{
			get { return (bool)RawRegistry.DoNotDisplayUpdatesOnModuleEntry.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DoNotDisplayUpdatesOnModuleEntry.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value); }
		}

		public bool ShowReadNotes
		{
			get { return (bool)RawRegistry.ShowReadNotes.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ShowReadNotes.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string ModuleToShowNotesFor
		{
			get { return (string)RawRegistry.ModuleToShowNotesFor.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ModuleToShowNotesFor.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value); }
		}

		public DMMagnifyingGlassSettingsStruct DMMagnifyingGlassSettings
		{
			get
			{
				byte[] registryValue = (byte[])RawRegistry.DMMagnifyingGlassSettings.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty);
				return new DMMagnifyingGlassSettingsStruct(registryValue);
			}
			set { RawRegistry.DMMagnifyingGlassSettings.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value.GetValue()); }
		}

		public DMThumbnailSettingsStruct DMThumbnailSettings
		{
			get
			{
				byte[] registryValue = (byte[])RawRegistry.DMThumbnailSettings.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty);
				return new DMThumbnailSettingsStruct(registryValue);
			}
			set { RawRegistry.DMThumbnailSettings.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value.GetValue()); }
		}

		public DMImportConfigurationSettingsStruct DMImportConfigurationSettings
		{
			get
			{
				byte[] registryValue = (byte[])RawRegistry.DMImportConfigurationSettings.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty);
				return new DMImportConfigurationSettingsStruct(registryValue);
			}
			set { RawRegistry.DMImportConfigurationSettings.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value.GetValue()); }
		}

		public DMImportConfigurationSettingsStruct GetDMImportConfigurationSettings(Guid userPK)
		{
			byte[] registryValue = (byte[])RawRegistry.DMImportConfigurationSettings.GetValueWithoutFallback(userPK, Guid.Empty, Guid.Empty);
			return new DMImportConfigurationSettingsStruct(registryValue);
		}

		public DMScanningFileOutputOptionSettingsStruct DMScanningFileOutputOptionSettings
		{
			get
			{
				byte[] registryValue = (byte[])RawRegistry.DMScanningFileOutputOptionSettings.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty);
				return new DMScanningFileOutputOptionSettingsStruct(registryValue);
			}
			set { RawRegistry.DMScanningFileOutputOptionSettings.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value.GetValue()); }
		}

		#endregion

		#region CFS

		public byte CFSSeaFreightLCLStorageFreeDays
		{
			get { return (byte)(int)RawRegistry.CFSSeaFreightLCLStorageFreeDays.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public byte CFSSeaFreightDGLCLStorageFreeDays
		{
			get { return (byte)(int)RawRegistry.CFSSeaFreightDGLCLStorageFreeDays.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public byte CFSAirFreightLCLStorageFreeDays
		{
			get { return (byte)(int)RawRegistry.CFSAirFreightLCLStorageFreeDays.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public byte CFSAirFreightDGLCLStorageFreeDays
		{
			get { return (byte)(int)RawRegistry.CFSAirFreightDGLCLStorageFreeDays.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public AvailableCommence CFSSeaFreightLCLAvailableCommences
		{
			get { return (AvailableCommence)Enum.Parse(typeof(AvailableCommence), (string)RawRegistry.CFSSeaFreightLCLAvailableCommences.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK)); }
		}

		public AvailableCommence CFSSeaFreightDGLCLAvailableCommences
		{
			get { return (AvailableCommence)Enum.Parse(typeof(AvailableCommence), (string)RawRegistry.CFSSeaFreightDGLCLAvailableCommences.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK)); }
		}

		public AvailableCommence CFSAirFreightLCLAvailableCommences
		{
			get { return (AvailableCommence)Enum.Parse(typeof(AvailableCommence), (string)RawRegistry.CFSAirFreightLCLAvailableCommences.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK)); }
		}

		public AvailableCommence CFSAirFreightDGLCLAvailableCommences
		{
			get { return (AvailableCommence)Enum.Parse(typeof(AvailableCommence), (string)RawRegistry.CFSAirFreightDGLCLAvailableCommences.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK)); }
		}

		public Guid DefaultDepotForBuildHVLV
		{
			get { return (Guid)RawRegistry.DefaultDepotForBuildHVLV.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DefaultDepotForBuildHVLV.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region Warehouse

		public string PackageWeightUnit
		{
			get { return (string)RawRegistry.PackageWeightUnit.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.PackageWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackageVolumeUnit
		{
			get { return (string)RawRegistry.PackageVolumeUnit.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.PackageVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string DefaultStockUnit
		{
			get { return (string)RawRegistry.DefaultStockUnits.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.DefaultStockUnits.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool UnitConversionPackTypesValidation => RawRegistry.UnitConversionPackTypesValidation.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);

		#endregion

		#region Fax and Email Overrides

		public string FaxDestinationOverride
		{
			get { return RawRegistry.FaxDestinationOverride.Value; }
			set { RawRegistry.FaxDestinationOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string HostedNotificationsEmailOverride
		{
			get { return RawRegistry.HostedNotificationsEmailOverride.Value; }
			set { RawRegistry.HostedNotificationsEmailOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string EmailDestinationOverride
		{
			get { return RawRegistry.EmailDestinationOverride.Value; }
			set { RawRegistry.EmailDestinationOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool SystemEmailDestinationOverride
		{
			get { return RawRegistry.SystemEmailDestinationOverride.Value; }
			set { RawRegistry.SystemEmailDestinationOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region HRM

		public ReadOnlyCodeDescriptionPairList LanguageSkillLevelList
		{
			get { return RawRegistry.LanguageSkillLevel.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public TimeSpan StandardWorkingHoursDuration
		{
			get
			{
				var stdWorkingHours = RawRegistry.StandardWorkingHours.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty).Split(':');
				return new TimeSpan(int.Parse(stdWorkingHours[0]), int.Parse(stdWorkingHours[1]), 0);
			}
			set { RawRegistry.StandardWorkingHours.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, $"{(int)value.TotalHours}:{value.Minutes}"); } // StandardWorkingHoursDuration format
		}

		public TimeSpan StandardWorkingHoursDurationForBranch(Guid branchPK)
		{
			var stdWorkingHours = RawRegistry.StandardWorkingHours.GetFallBackValueAtAllLevels(Guid.Empty, branchPK, Guid.Empty).Split(':');
			return new TimeSpan(int.Parse(stdWorkingHours[0]), int.Parse(stdWorkingHours[1]), 0);
		}

		#endregion

		public bool EHubTesting
		{
			get { return RawRegistry.EHubTesting.Value; }
			set { RawRegistry.EHubTesting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

#if DEBUG
		public bool RedirectReferenceDataForTests
		{
			get { return RawRegistry.RedirectReferenceDataForTests.Value; }
			set { RawRegistry.RedirectReferenceDataForTests.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}
#endif

		#region RemoteDesktop

		#region MicrosoftOffice365

		public string[] OpenInMicrosoftOffice365FileTypeList
		{
			get { return RawRegistry.OpenInMicrosoftOffice365FileTypeList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.OpenInMicrosoftOffice365FileTypeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string MicrosoftOffice365ApplicationIdForDragDrop
		{
			get { return RawRegistry.MicrosoftOffice365ApplicationIdForDragDrop.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MicrosoftOffice365ApplicationIdForDragDrop.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string MicrosoftOffice365TenantIdForDragDrop
		{
			get { return RawRegistry.MicrosoftOffice365TenantIdForDragDrop.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MicrosoftOffice365TenantIdForDragDrop.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int MicrosoftOffice365AttachmentEmailFetchLimitForDragDrop
		{
			get { return RawRegistry.MicrosoftOffice365AttachmentEmailFetchLimitForDragDrop.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		#endregion MicrosoftOffice365

		public bool RemoteAppEnableDragDropLite
		{
			get { return RawRegistry.RemoteAppEnableDragDropLite.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.RemoteAppEnableDragDropLite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool RemoteAppAlwaysShowSelectionFormOnFileDrop
		{
			get { return RawRegistry.RemoteAppAlwaysShowSelectionFormOnFileDrop.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.RemoteAppAlwaysShowSelectionFormOnFileDrop.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool RemoteAppSendTestingMessageOnInitialize
		{
			get { return RawRegistry.RemoteAppSendTestingMessageOnInitialize.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.RemoteAppSendTestingMessageOnInitialize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int RemoteAppShowFormViaMenuDelayMilliseconds
		{
			get { return RawRegistry.RemoteAppShowFormViaMenuDelayMilliseconds.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.RemoteAppShowFormViaMenuDelayMilliseconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int RemoteAppWaitingForReconnectionTimeoutInSeconds
		{
			get { return RawRegistry.RemoteAppWaitingForReconnectionTimeoutInSeconds.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.RemoteAppWaitingForReconnectionTimeoutInSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public TimeSpan RemoteAppCheckDriveMappingTimeoutInSeconds
			=> TimeSpan.FromSeconds(
				RawRegistry.RemoteAppCheckDriveMappingTimeoutInSeconds.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

		public string RemoteAppAllowEDocAccessWithoutConnectorMode
		{
			get
			{
				if (EnvProxy.IsHostedWithCargowise)
				{
					return RemoteConnectingModes.ConnectorOnly;
				}

				var result = RawRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				switch (result)
				{
					case "ENB":
						return RemoteConnectingModes.ConnectorOrServer;
					case "DISB":
						return RemoteConnectingModes.ConnectorOnly;
					case "SKIP":
						return RemoteConnectingModes.ServerOnly;
					default:
						return result;
				}
			}
			set
			{
				if (EnvProxy.IsHostedWithCargowise)
				{
					throw new InvalidOperationException("eDoc access is not allowed without connector if hosted with CargoWise.");
				}
				RawRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			}
		}

		#endregion RemoteDesktop

		public string EnglishSpelling
		{
			get { return EnglishSpellingForCompany(EnvProxy.Instance.CurrentCompany); }
		}

		public string EnglishSpellingForCompany(ICompany company)
		{
			return RawRegistry.EnglishSpelling.GetValueWithoutFallback(company?.PK ?? Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public int UberFactoryTimeoutPeriod
		{
			get { return RawRegistry.UberFactoryTimeoutPeriod.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.UberFactoryTimeoutPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public int DbConnectionTimeoutPeriod
		{
			get { return RawRegistry.DbConnectionTimeoutPeriod.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.DbConnectionTimeoutPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool FeatureTestModeEnabled
		{
			get => RawRegistry.FeatureTestModeEnabled.GetValueWithoutFallback(default, default, default);
			set => RawRegistry.FeatureTestModeEnabled.SetValue(default, default, default, value);
		}

		public bool WebVersionUserMonitoringEnabled
		{
			get => RawRegistry.WebVersionUserMonitoringEnabled.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			set => RawRegistry.WebVersionUserMonitoringEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}
		public string WebVersionUserMonitoringUrl
		{
			get => RawRegistry.WebVersionUserMonitoringUrl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			set => RawRegistry.WebVersionUserMonitoringUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		public string WebVersionSiteOfflineMessage
		{
			get => RawRegistry.WebVersionSiteOfflineMessage.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			set => RawRegistry.WebVersionSiteOfflineMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		public string WebVersionLaunchUrl
		{
			get => RawRegistry.WebVersionLaunchUrl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			set => RawRegistry.WebVersionLaunchUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		public string BlazorUrl
		{
			get => RawRegistry.BlazorUrl.GetValueWithoutFallback(default, default, default);
			set => RawRegistry.BlazorUrl.SetValue(default, default, default, value);
		}

		public TimeSpan BlazorBackchannelConnectionTimeout
		{
			get => TimeSpan.FromSeconds(RawRegistry.BlazorBackchannelConnectionTimeout.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			set => RawRegistry.BlazorBackchannelConnectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)(value.TotalSeconds));
		}

		#region SCIM

		public string ScimAudienceId
		{
			get { return RawRegistry.ScimAudienceId.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ScimAudienceId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string ScimKnownEndpointPath
		{
			get { return RawRegistry.ScimKnownEndpointPath.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ScimKnownEndpointPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string ScimIssuer
		{
			get { return RawRegistry.ScimIssuer.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ScimIssuer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string ScimLoggingKafkaBrokers
		{
			get { return RawRegistry.ScimLoggingKafkaBrokers.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ScimLoggingKafkaBrokers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string ScimLoggingKafkaTopic
		{
			get { return RawRegistry.ScimLoggingKafkaTopic.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ScimLoggingKafkaTopic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string ScimLoggingKafkaTopicUsername
		{
			get { return RawRegistry.ScimLoggingKafkaTopicUsername.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ScimLoggingKafkaTopicUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string ScimLoggingKafkaTopicPassword
		{
			get { return RawRegistry.ScimLoggingKafkaTopicPassword.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ScimLoggingKafkaTopicPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool ScimCanLogin
		{
			get { return RawRegistry.ScimCanLogin.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ScimCanLogin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string ScimCodeGenerationCharacters
		{
			get { return RawRegistry.ScimCodeGenerationCharacters.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

	}
}
