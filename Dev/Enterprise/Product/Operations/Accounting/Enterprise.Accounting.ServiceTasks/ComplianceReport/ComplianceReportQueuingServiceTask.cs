using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.ComplianceReport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport;

[assembly: HostedService(
	ComplianceReportQueuingServiceTask.Code,
	"Compliance Report Transaction Queuing Service Task",
	"ACC",
	typeof(ComplianceReportQueuingServiceTask),
	IsMandatory = false,
	MinimumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour")
]

[assembly: HostedServiceBusinessObjectBinding(ComplianceReportQueuingServiceTask.Code, AccComplianceReportSchema.Constants.TableName,
	new[]
	{
		AccComplianceReportSchema.Constants.ACR_Status + "=" + AccComplianceReport.Status.ReportCreated,
	},
	"Compliance Report Queuing")]

[assembly: HostedServiceBusinessObjectBinding(ComplianceReportQueuingServiceTask.Code, AccComplianceReportSchema.Constants.TableName,
	new[]
	{
		AccComplianceReportSchema.Constants.ACR_Status + "=" + AccComplianceReport.Status.ReportPendingQueueing,
	},
	"Compliance Report Re-queuing")]

namespace Enterprise.Accounting.ServiceTasks.ComplianceReport
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
	public class ComplianceReportQueuingServiceTask : ServiceProviderImpl
	{
		public const string Code = "CRQ";

		public ComplianceReportQueuingServiceTask(Func<Guid> getSessionId) : this()
		{
			this.getSessionId = getSessionId;
		}

		readonly Func<Guid> getSessionId = Guid.NewGuid;

		public ComplianceReportQueuingServiceTask()
		{
			maxTaskRunTime = TimeSpan.FromSeconds(AccountingMasterFilesRegistry.Instance.MaximumTimeForCRQServiceTaskToRun.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			OrgCusCodeHelper = new OrgCusCodeHelper();
		}

		readonly TimeSpan maxTaskRunTime;

		[HostedServiceRequirement]
		public static string CheckApplicableConfigurations()
		{
			return (FastComplianceReportConfigurationLoader.Load(Db.Connection)).IsDataAvailable();
		}

#if DEBUG
		public static bool CheckApplicableConfigurations_ForTestOnly()
		{
			return (ComplianceReportConfigurationLoader.Load(new BusinessObjectFactory())).IsDataAvailable();
		}
#endif

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Compliance report transaction queuing process starting.");

			try
			{
				using (EnvProxy.Instance.SuspendBranchAccessError())
				{
					sessionID = getSessionId();
					var iterationCounter = 0;
					var taskStopwatch = Stopwatch.StartNew();

					factory = new BusinessObjectFactory
					{
						NameForDebugging = $"Compliance Report Factory for Queuing #{iterationCounter}",
					};
					complianceReportConfigurationLoader = ComplianceReportConfigurationLoader.Load(factory);

					do
					{
						iterationCounter++;

						ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Compliance report transaction queuing process iteration {0} started.", iterationCounter));

						// IMPORTANT: The order must be: Queue -> Generate -> Purge New -> Purge Old.
						if (!TryQueueNextReport() && !TryGenerateNextReport() && !TryPurgeFreshQueue() && !TryPurgeOldQueue())
						{
							break;
						}

						token.ThrowIfCancellationRequested();
					} while (taskStopwatch.Elapsed < maxTaskRunTime);

					if (taskStopwatch.Elapsed >= maxTaskRunTime)
					{
						ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Compliance report transaction queuing process ended after {0} iterations as it reached timeout: {1}.", iterationCounter, maxTaskRunTime));
					}
					else
					{
						ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Compliance report transaction queuing process completed after {0} iterations.", iterationCounter));
					}
				}
			}
			catch (OperationCanceledException)
			{
				ServiceLogger.Log(LogType.Warning, "Compliance report transaction queuing process canceled.");
				throw;
			}
		}

		bool TryQueueNextReport()
		{
			var (reportConfig, report) = GetNextReportDataForQueueing();

			if (reportConfig == null || report == null)
			{
				return false;
			}

			var dateFrom = report.ACR_DateFrom.ToShortDateString();
			var dateTo = report.ACR_DateTo.ToShortDateString();
			var connection = ((IDbConnected)factory).Connection;

			try
			{
				using (var transactionManager = connection.BeginTransactionWithManager())
				{
					var newStatus = AccComplianceReport.Status.ReportDataQueued;
					var newStatusMessage = string.Empty;
					var validationSuccess = true;

					using (var accComplianceReportUsageCollector = ObjectFactory.Get<IAccComplianceReportUsageCollectorFactory>().GetAccComplianceReportUsageCollector(report))
					{
						DbCommand cmd;
						if (reportConfig.IsAllTransactionConfig())
						{
							cmd = GetCommandForAllTransactions(connection, report, reportConfig);
						}
						else if (reportConfig.IsTransactionPaymentConfig())
						{
							cmd = GetCommandForTransactionPayments(connection, report,
								reportConfig.Settings.Cast<ComplianceReportConfigurationSetting>().FirstOrDefault());
						}
						else if (reportConfig.IsPTRS2024_ReportableConfig())
						{
							cmd = GetCommandForReportablePTRS2024(connection, report);
						}
						else if (reportConfig.IsPTRS2024_AllPaymentsConfig())
						{
							cmd = GetCommandForAllPaymentsPTRS2024(connection, report);
						}
						else if (reportConfig.IsPTRS_ReportableConfig())
						{
							cmd = GetCommandForReportablePTRS(connection, report);
						}
						else if (reportConfig.IsPTRS_AllPaymentsConfig())
						{
							cmd = GetCommandForAllPaymentsPTRS(connection, report);
						}
						else if (reportConfig.IsZMGermanyConfig())
						{
							cmd = GetCommandForTransactionZMReport(connection, report);
						}
						else
						{
							return false;
						}

						ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture,
							"Started queue building for company: '{0}', report type '{1}', dates from {2} to {3}.",
							report.Company.GC_Code, report.ACR_ReportType, dateFrom, dateTo));

						cmd.ExecuteNonQuery();

						if (reportConfig.IsTransactionPaymentConfig() && reportConfig.Settings.Count > 1) // In case we will use this new TPA grouping for the transaction types other than AP Invoice
						{
							foreach (var setting in reportConfig.Settings.Cast<ComplianceReportConfigurationSetting>().Skip(1))
							{
								cmd = GetCommandForTransactionPayments(connection, report, setting, deleteExisting: false);
								cmd.ExecuteNonQuery();
							}
						}

						if (reportConfig.IsZMGermanyConfig())
						{
							var complianceReportInfoZM = new AccComplianceReportProcessingInfoZM(report);
							var validationResult = complianceReportInfoZM.Validate();
							report.DeleteNotes(validationResult.NotesDescription);

							if (validationResult.ReturnCode != ReturnCode.Success)
							{
								newStatus = validationResult.Status;
								report.AddNote(validationResult.NotesDescription, validationResult.NotesText);
								newStatusMessage = validationResult.StatusMessage;

								// rollback rows in the queue
								transactionManager.RollbackTransaction();
								validationSuccess = false;
							}
						}

						var oldStatus = report.ACR_Status;
						report.ACR_Status = newStatus;
						report.ACR_StatusMessage = newStatusMessage;

						accComplianceReportUsageCollector.AddChangedStatus(oldStatus, newStatus);
						if (!string.IsNullOrEmpty(newStatusMessage))
						{
							accComplianceReportUsageCollector.AddStatusMessage(newStatusMessage);
						}
						accComplianceReportUsageCollector.AddLogonUser(Env.CurrentUser);
						accComplianceReportUsageCollector.AddContext(AccComplianceReportUsageCollectorContext.CRQServiceTask);
						accComplianceReportUsageCollector.AddSessionId(sessionID);
						accComplianceReportUsageCollector.AddAction(AccComplianceReportUsageCollectorAction.Queueing);
					}

					factory.Save();

					ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture,
						"Completed queue building for company: '{0}', report type '{1}', dates from {2} to {3}.",
						report.Company.GC_Code, report.ACR_ReportType, dateFrom, dateTo));

					// commit changes incl. queue records only in case no validation error occurred
					if (validationSuccess)
					{
						transactionManager.CommitTransaction();
					}
				}
			}
			catch (Exception ex) when (ex is
										   System.Data.Common.DbException or
										   ZSaveException or
										   ZCannotSaveException)
			{
				ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture,
					"Failed queue building for company: '{0}', report type '{1}', dates from {2} to {3}.\r\nException: {4} Message: {5}",
					report.Company.GC_Code, report.ACR_ReportType, dateFrom, dateTo, ex.GetType(), ex.Message));
			}

			return true;
		}

		bool TryGenerateNextReport()
		{
			var (reportConfig, report) = GetNextReportDataForGenerating();

			if (reportConfig == null || report == null)
			{
				return false;
			}

			var dateFrom = report.ACR_DateFrom.ToShortDateString();
			var dateTo = report.ACR_DateTo.ToShortDateString();

			try
			{
				ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Started generating output for company '{0}', report type '{1}', dates from {2} to {3}.",
					report.Company.GC_Code, report.ACR_ReportType, dateFrom, dateTo));

				var stopwatch = Stopwatch.StartNew();

				using (factory.AddDisposableService())
				using (var accComplianceReportUsageCollector = ObjectFactory.Get<IAccComplianceReportUsageCollectorFactory>().GetAccComplianceReportUsageCollector(report))
				{
					var oldStatus = report.ACR_Status;
					var usageData = new AccComplianceReportUsageCollectorContextData
					{
						Action = AccComplianceReportUsageCollectorAction.GeneratingStatusChanged,
						SessionId = sessionID,
						Context = AccComplianceReportUsageCollectorContext.CRQServiceTask
					};

					switch (report.ACR_ReportType)
					{
						case ReportTypes.IDEA:
							report.GenerateIDEAFromQueue(ServiceLogger, usageData);
							break;
						case ReportTypes.FEC:
							report.GenerateFECFromQueue(ServiceLogger, usageData);
							break;
						case ReportTypes.JPKV7M:
							report.GenerateJPKV7MFile(new JPKV7MReport(), ServiceLogger, usageData);
							break;
						default:
							throw new NotSupportedException($"Report type {report.ACR_ReportType} does not support asynchronous generation. Use the 'Generate Report' button instead.");
					}

					accComplianceReportUsageCollector.AddChangedStatus(oldStatus, report.ACR_Status);
					accComplianceReportUsageCollector.AddLogonUser(Env.CurrentUser);
					accComplianceReportUsageCollector.AddContext(AccComplianceReportUsageCollectorContext.CRQServiceTask);
					accComplianceReportUsageCollector.AddSessionId(sessionID);
					accComplianceReportUsageCollector.AddAction(AccComplianceReportUsageCollectorAction.Generating);
				}

				factory.Save(); // This Save is required for AccComplianceReportUsageCollector

				ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture,
					"Completed generating output for company '{0}', report type '{1}', dates from {2} to {3} within {4:N1} seconds.",
					report.Company.GC_Code, report.ACR_ReportType, dateFrom, dateTo, stopwatch.Elapsed.TotalSeconds));
			}
			catch (Exception ex) when (ex is
										   System.Data.Common.DbException or
										   ZSaveException or
										   ZCannotSaveException or
										   NotSupportedException or
										   InvalidOperationException)
			{
				ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture,
					"Failed report generation for company: '{0}', report type '{1}', dates from {2} to {3}.\r\nException: {4} Message: {5}",
					report.Company.GC_Code, report.ACR_ReportType, dateFrom, dateTo, ex.GetType(), ex.Message));
			}

			return true;
		}

		bool TryPurgeFreshQueue()
		{
			var reportConfigurationsForPurgingAsArray = complianceReportConfigurationLoader.GetReportConfigurationsForPurging().ToArray();

			if (reportConfigurationsForPurgingAsArray.Length == 0)
			{
				return false;
			}

			ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, "Started purging fresh queue records."));

			var sqlFindReportToPurge =
				FormattableString.Invariant($@"SELECT TOP 1 ACR_PK FROM dbo.AccComplianceReport
WHERE ACR_ReportType = @ReportType
AND ACR_Status = @ReportStatus
AND ACR_GC_Company = @CompanyPK
AND EXISTS (SELECT 1 FROM dbo.AccTransactionComplianceReportQueue
WHERE ACQ_ReportType = ACR_ReportType AND (ACR_GB_Branch IS NULL OR ACR_GB_Branch = ACQ_GB_Branch) AND ACQ_Date BETWEEN ACR_DateFrom AND ACR_DateTo)");

			var sqlPurge = FormattableString.Invariant($@"DELETE FROM dbo.AccTransactionComplianceReportQueue
WHERE ACQ_PK IN (SELECT TOP {MaxRecordsToPurge} ACQ_PK FROM dbo.AccTransactionComplianceReportQueue
JOIN dbo.AccComplianceReport ON ACR_ReportType = ACQ_ReportType
AND ACR_GC_Company = ACQ_GC_Company
AND(ACR_GB_Branch IS NULL OR ACR_GB_Branch = ACQ_GB_Branch)
AND ACQ_Date BETWEEN ACR_DateFrom AND ACR_DateTo
WHERE ACR_PK = @reportPK)
SELECT @@ROWCOUNT");

			foreach ((ZGuid companyPK, ComplianceReportConfiguration[] reportConfigs) in reportConfigurationsForPurgingAsArray)
			{
				foreach (var reportConfig in reportConfigs)
				{
					try
					{
						var connection = Db.Connection;
						var stopwatch = Stopwatch.StartNew();

						var cmdFind = connection.Command(sqlFindReportToPurge);
						cmdFind.AddParameterBasedOnDbColumn("@CompanyPK", companyPK.ToGuid(), AccComplianceReportSchema.ACR_GC_Company);
						cmdFind.AddParameterBasedOnDbColumn("@ReportType", reportConfig.ReportCode.ToString(), AccComplianceReportSchema.ACR_ReportType);
						cmdFind.AddParameterBasedOnDbColumn("@ReportStatus", Status.ReportGenerated, AccComplianceReportSchema.ACR_Status);
						var cmdFindQueryResult = cmdFind.ExecuteScalar();

						if (cmdFindQueryResult == null || !ZGuid.TryParse(cmdFindQueryResult.ToString(), out var reportPK))
						{
							continue;
						}

						ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"Purging report. ReportPK: '{reportPK}'; CompanyPK: '{companyPK}'; ReportType: '{reportConfig.ReportCode}'"));

						using (var transactionManager = connection.BeginTransactionWithManager())
						{
							var cmdPurge = connection.Command(sqlPurge);
							cmdPurge.AddParameter("@reportPK", SqlDbType.UniqueIdentifier, reportPK.ToGuid());
							var cmdPurgeQueryResult = cmdPurge.ExecuteScalar();

							if (cmdPurgeQueryResult != null && int.TryParse(cmdPurgeQueryResult.ToString(), out var purgedCount) && purgedCount > 0)
							{
								ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"Purged {purgedCount} queue records within {(stopwatch.Elapsed.TotalSeconds):N1} seconds."));

								transactionManager.CommitTransaction();

								return true;
							}
						}
					}
					catch (Exception ex) when (ex is
												   System.Data.Common.DbException or
												   ZSaveException or
												   ZCannotSaveException)
					{
						ServiceLogger.Log(LogType.Error,
							FormattableString.Invariant(
								$"Failed purging queue records for company '{companyPK}' and report type '{reportConfig.ReportCode}'.\r\nException: {ex.GetType()} Message: {ex.Message}"));
					}
				}
			}

			return false;
		}

		bool TryPurgeOldQueue()
		{
			var reportConfigurationsForPurgingAsArray = complianceReportConfigurationLoader.GetOldReportConfigurationsForPurging().ToArray();

			if (reportConfigurationsForPurgingAsArray.Length == 0)
			{
				return false;
			}

			ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, "Started purging old queue records."));

			foreach ((ZGuid companyPK, ComplianceReportConfiguration[] reportConfigs) in reportConfigurationsForPurgingAsArray)
			{
				var typesGroupedByPrefix = reportConfigs
					.GroupBy(x => x.ReportBaseTablePrefix)
					.OrderBy(x => x.Key);

				foreach (var typesForPrefix in typesGroupedByPrefix)
				{
					var reportTypes = BuildReportTypes(typesForPrefix.ToArray());
					var today = ZDateTime.Today;
					var purgeDate = today.AddDays(-today.Day + 1).AddMonths(-24);

					var sqlPurge = FormattableString.Invariant($@"DELETE
FROM [dbo].[AccTransactionComplianceReportQueue]
WHERE ACQ_PK IN (SELECT TOP {MaxRecordsToPurge} ACQ_PK FROM [dbo].[AccTransactionComplianceReportQueue]
	LEFT JOIN [dbo].[AccComplianceReport] ON [ACR_GC_Company] = @CompanyPK -- We are after the reports only for this Company
		AND [ACR_ReportType] = [ACQ_ReportType] 
		AND ([ACR_GB_Branch] IS NULL OR [ACR_GB_Branch] = [ACQ_GB_Branch])
		AND [ACQ_Date] BETWEEN [ACR_DateFrom] AND [ACR_DateTo]
		AND [ACR_ReportType] IN ({reportTypes}) -- Report Types from Configuration to narrow down the selection
		AND [ACR_DateFrom] < @PurgeBeforeDate -- We are not interested in the newer reports
	WHERE [ACQ_GC_Company] = @CompanyPK 
		AND [ACQ_Date] < @PurgeBeforeDate
		AND [ACR_PK] IS NULL -- Check that queue could not be used by existing Compliance Report
		AND ([ACQ_ReportType] IN ({reportTypes}) AND [ACQ_ParentTableCode] = '{typesForPrefix.Key}'))
SELECT @@ROWCOUNT");

					try
					{
						var connection = Db.Connection;

						using (var transactionManager = connection.BeginTransactionWithManager())
						{
							var cmdPurge = connection.Command(sqlPurge);
							cmdPurge.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK.ToGuid());
							cmdPurge.AddParameter("@PurgeBeforeDate", SqlDbType.Date, purgeDate);
							var cmdPurgeQueryResult = cmdPurge.ExecuteScalar();

							if (cmdPurgeQueryResult != null && int.TryParse(cmdPurgeQueryResult.ToString(), out var purgedCount) && purgedCount > 0)
							{
								ServiceLogger.Log(LogType.Debug, $"Purged {purgedCount} old queue records for company: '{companyPK}', report types {reportTypes}, purge date {purgeDate}.");

								transactionManager.CommitTransaction();

								return true;
							}
						}
					}
					catch (Exception ex) when (ex is
												   System.Data.Common.DbException or
												   ZSaveException or
												   ZCannotSaveException)
					{
						ServiceLogger.Log(LogType.Error,
							FormattableString.Invariant(
								$"Failed purging old queue records for company: '{companyPK}', report types {reportTypes}, purge date {purgeDate}.\r\nException: {ex.GetType()} Message: {ex.Message}"));
					}
				}
			}

			return false;
		}

		(ComplianceReportConfiguration reportConfig, AccComplianceReport report) GetNextReportDataForQueueing()
		{
			var reportConfigurations = complianceReportConfigurationLoader.GetReportConfigurationsForQueueing();

			foreach (var (companyPK, companyReportConfigs) in reportConfigurations)
			{
				var reportCodes = companyReportConfigs.Select(rc => rc.ReportCode).ToArray();

				var reportFilter = new ZQuery(AccComplianceReportSchema.ACR_ReportType, reportCodes)
					.AddToFilter(AccComplianceReportSchema.ACR_GC_Company, companyPK)
					.AddToFilter(AccComplianceReportSchema.ACR_Status, new[] { AccComplianceReport.Status.ReportCreated, AccComplianceReport.Status.ReportPendingQueueing });
				reportFilter.OrderBy = AccComplianceReportSchema.Constants.ACR_SystemLastEditTimeUtc;

				var report = factory.LoadTop1<AccComplianceReport>(reportFilter);

				if (report != null)
				{
					var reportConfig = companyReportConfigs.FirstOrDefault(rc => rc.ReportCode == report.ACR_ReportType);

					if (reportConfig != null)
					{
						return (reportConfig, report);
					}
				}
			}

			return default;
		}

		(ComplianceReportConfiguration reportConfig, AccComplianceReport report) GetNextReportDataForGenerating()
		{
			var reportConfigurations = complianceReportConfigurationLoader.GetReportConfigurationsForGenerating();

			foreach (var (companyPK, companyReportConfigs) in reportConfigurations)
			{
				foreach (var companyReportConfig in companyReportConfigs)
				{
					var reportFilter = new ZQuery(AccComplianceReportSchema.ACR_ReportType, companyReportConfig.ReportCode)
							.AddToFilter(AccComplianceReportSchema.ACR_GC_Company, companyPK)
							.AddToFilter(AccComplianceReportSchema.ACR_Status, companyReportConfig.GetStatusForReportOutputGeneration());
					reportFilter.OrderBy = AccComplianceReportSchema.Constants.ACR_SystemLastEditTimeUtc;

					var report = factory.LoadTop1<AccComplianceReport>(reportFilter);

					if (report != null)
					{
						return (companyReportConfig, report);
					}
				}
			}

			return default;
		}

		string BuildReportTypes(ComplianceReportConfiguration[] reportConfigurations)
		{
			var reportTypesBuilder = new ZStringBuilder();
			foreach (var reportConfiguration in reportConfigurations.OrderBy(x => x.ReportCode))
			{
				reportTypesBuilder.Append($"'{reportConfiguration.ReportCode}'");
			}

			return reportTypesBuilder.ToStringWithDelimiterBetweenAppends(",");
		}

		DbCommand GetCommandForAllTransactions(DbConnection connection, AccComplianceReport report, ComplianceReportConfiguration reportConfig)
		{
			var result = connection.Command("QueueAllForComplianceReport", 1200000); // Executing stored procedure directly giving 20 minutes timeout
			result.CommandType = CommandType.StoredProcedure;
			result.AddParameter("@ReportPK", SqlDbType.UniqueIdentifier, report.PK.ToGuid());
			result.AddParameter("@IncludeAllPresentationJournals", SqlDbType.Bit, reportConfig.ReportLineGrouping == ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithPresentation ? 1 : 0);

			if (reportConfig.ReportLineGrouping == ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook
				|| reportConfig.ReportLineGrouping == ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping)
			{
				var openingCategory = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetFallBackValueAtAllLevels(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty)?.OpeningCategory;
				var openingCategoryCode = openingCategory?.Code ?? string.Empty;
				var closingCategory = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetFallBackValueAtAllLevels(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty)?.ClosingCategory;
				var closingCategoryCode = closingCategory?.Code ?? string.Empty;
				result.AddParameter("@OpeningCategory", SqlDbType.VarChar, openingCategoryCode);
				result.AddParameter("@ClosingCategory", SqlDbType.VarChar, closingCategoryCode);
			}

			return result;
		}

		DbCommand GetCommandForTransactionPayments(DbConnection connection, AccComplianceReport report, ComplianceReportConfigurationSetting setting, bool deleteExisting = true)
		{
			var result = connection.Command("QueuePaidTransactionsForComplianceReport", 1200000); // Executing stored procedure directly giving 20 minutes timeout
			result.CommandType = CommandType.StoredProcedure;
			result.AddParameter("@ReportPK", SqlDbType.UniqueIdentifier, report.PK.ToGuid());

			result.AddParameter("@Ledger", SqlDbType.Char, setting.LedgerType.ToString());
			result.AddParameter("@TransactionType", SqlDbType.Char, setting.InvoiceType.ToString());
			result.AddParameter("@DeleteExistingPivotAndQueue", SqlDbType.Bit, deleteExisting);

			return result;
		}

		DbCommand GetCommandForReportablePTRS(DbConnection connection, AccComplianceReport report)
		{
			var result = connection.Command("QueueFullyPaidAPInvoicesForPTRS", 1200000); // Executing stored procedure directly giving 20 minutes timeout
			result.CommandType = CommandType.StoredProcedure;
			result.AddParameter("@ReportPK", SqlDbType.UniqueIdentifier, report.PK.ToGuid());

			return result;
		}

		DbCommand GetCommandForReportablePTRS2024(DbConnection connection, AccComplianceReport report)
		{
			var result = connection.Command("QueueFullyPaidAPInvoicesForPTRS2024", 1200000); // Executing stored procedure directly giving 20 minutes timeout
			result.CommandType = CommandType.StoredProcedure;
			result.AddParameter("@ReportPK", SqlDbType.UniqueIdentifier, report.PK.ToGuid());

			return result;
		}

		DbCommand GetCommandForAllPaymentsPTRS(DbConnection connection, AccComplianceReport report)
		{
			var result = connection.Command("QueueAllPaidAPInvoicesForPTRS", 1200000); // Executing stored procedure directly giving 20 minutes timeout
			result.CommandType = CommandType.StoredProcedure;
			result.AddParameter("@ReportPK", SqlDbType.UniqueIdentifier, report.PK.ToGuid());

			return result;
		}

		DbCommand GetCommandForAllPaymentsPTRS2024(DbConnection connection, AccComplianceReport report)
		{
			var result = connection.Command("QueueAllPaidAPInvoicesForPTRS2024", 1200000); // Executing stored procedure directly giving 20 minutes timeout
			result.CommandType = CommandType.StoredProcedure;
			result.AddParameter("@ReportPK", SqlDbType.UniqueIdentifier, report.PK.ToGuid());

			return result;
		}

		DbCommand GetCommandForTransactionZMReport(DbConnection connection, AccComplianceReport report)
		{
			var result = connection.Command("QueueTransactionsForZMReport", 1200000); // Executing stored procedure directly giving 20 minutes timeout
			var mappingTable = OrgCusCodeHelper.GetConsumptionTaxRegistrationCodeForEUCountriesExcludingOne("DE");
			result.CommandType = CommandType.StoredProcedure;
			result.AddParameter("@ReportPK", SqlDbType.UniqueIdentifier, report.PK.ToGuid());
			result.AddTableValuedParameter("@CountryBusinessRegCodeMapping", "dbo.TVP_CountryAndBusinessRegType", mappingTable);

			return result;
		}

		public readonly IReadOnlyList<string> SupportedReportTypes = new[] { ReportTypes.IDEA, ReportTypes.FEC, ReportTypes.ZMGermany, ReportTypes.JPKV7M };

		public static readonly int MaxRecordsToPurge =
#if DEBUG
			ZArchitecture.Environment.Globals.IsTest ? 25 :
#endif
			10000;

		Guid sessionID;
		BusinessObjectFactory factory;
		readonly OrgCusCodeHelper OrgCusCodeHelper;
		ComplianceReportConfigurationLoader complianceReportConfigurationLoader;
	}

	sealed class ComplianceReportConfigurationLoader
	{
		ComplianceReportConfigurationLoader(GlbCompany[] activeCompanies)
		{
			companiesWithComplianceReportConfigs = activeCompanies.Select(company => (company.PK.ToGuid(),
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration
					.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty)
					.Cast<ComplianceReportConfiguration>()));
		}

		readonly IEnumerable<(Guid companyPK, IEnumerable<ComplianceReportConfiguration> reportConfigs)> companiesWithComplianceReportConfigs;

		public static ComplianceReportConfigurationLoader Load(BusinessObjectFactory businessObjectFactory)
		{
			var activeCompanies = AccountingUtils.GetAllActiveCompanies(businessObjectFactory);
			return new ComplianceReportConfigurationLoader(activeCompanies);
		}

		public bool IsDataAvailable()
		{
			return GetReportConfigurationsForQueueing().Any()
				   || GetReportConfigurationsForGenerating().Any()
				   || GetReportConfigurationsForPurging().Any()
				   || GetOldReportConfigurationsForPurging().Any();
		}

		public IEnumerable<(Guid companyPK, ComplianceReportConfiguration[] reportConfigs)> GetReportConfigurationsForQueueing()
		{
			var companiesFullListWithReportConfigurations = companiesWithComplianceReportConfigs.Select(c => (c.companyPK, c.reportConfigs
				.Where(crc => crc.IsServiceTaskQueuingConfig())));
			return FilterConfigs(companiesFullListWithReportConfigurations);
		}

		public IEnumerable<(Guid companyPK, ComplianceReportConfiguration[] reportConfigs)> GetReportConfigurationsForGenerating()
		{
			var companiesFullListWithReportConfigurations = companiesWithComplianceReportConfigs.Select(c => (c.companyPK, c.reportConfigs
				.Where(crc => crc.IsServiceTaskOutputGeneratingConfig())));
			return FilterConfigs(companiesFullListWithReportConfigurations);
		}

		public IEnumerable<(Guid companyPK, ComplianceReportConfiguration[] reportConfigs)> GetReportConfigurationsForPurging()
		{
			var companiesFullListWithReportConfigurations = companiesWithComplianceReportConfigs.Select(c => (c.companyPK, c.reportConfigs
				.Where(crc => crc.IsServiceTaskOutputGeneratingConfig() && crc.IsServiceTaskQueuingConfig())));
			return FilterConfigs(companiesFullListWithReportConfigurations);
		}

		public IEnumerable<(Guid companyPK, ComplianceReportConfiguration[] reportConfigs)> GetOldReportConfigurationsForPurging()
		{
			var companiesFullListWithReportConfigurations = companiesWithComplianceReportConfigs.Select(c => (c.companyPK, c.reportConfigs
				.Where(crc => crc.IsQueueOnPostingConfig())));
			return FilterConfigs(companiesFullListWithReportConfigurations);
		}

		IEnumerable<(Guid, ComplianceReportConfiguration[])> FilterConfigs(
			IEnumerable<(Guid companyPK, IEnumerable<ComplianceReportConfiguration> reportConfigs)> companiesFullListWithReportConfigurations)
		{
			return companiesFullListWithReportConfigurations
				.Where(tuple => tuple.reportConfigs.Any())
				.Select(tuple => (tuple.companyPK, reportConfigs: tuple.reportConfigs.ToArray()));
		}
	}

	sealed class FastComplianceReportConfigurationLoader
	{
		FastComplianceReportConfigurationLoader(DbConnection dbConn)
		{
			dbConnection = dbConn;
		}

		readonly DbConnection dbConnection;

		public static FastComplianceReportConfigurationLoader Load(DbConnection dbConn)
		{
			return new FastComplianceReportConfigurationLoader(dbConn);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Using direct SQL for performance")]
		public string IsDataAvailable()
		{
			var activeCountries = AccountingUtils.GetAllActiveCompaniesCountries(dbConnection);

			var configuredCountries = new HashSet<string>(ComplianceReportConfigurationRegistryItem.CountryCodesForServiceTask);
			configuredCountries.Remove(Core.Constants.CountryCodes.Taiwan); //Not compliant with any of GetReportConfigurationsFor***()

			const string selectRegistryItemsSql = @"SELECT DISTINCT GC_RN_NKCountryCode
FROM dbo.StmData LEFT JOIN dbo.GlbCompany ON SD_Owner = GC_PK
WHERE SD_Name = 'ComplianceReportConfiguration'";

			using (var command = dbConnection.Command(selectRegistryItemsSql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					configuredCountries.Add(reader["GC_RN_NKCountryCode"].ToString());
				}
			}

			return configuredCountries.Intersect(activeCountries).Any()
				? string.Empty : (NoResString)"There are no active login companies with compliance report configuration.";
		}
	}
}
