using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ErrorReporting.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.ErrorReporting;

[assembly: HostedService
	(
		Enterprise.ErrorReporting.ServiceTasks.ReportErrorsServiceTask.Code,
		"Report Queued Errors to WiseTech Global",
		"SYS",
		typeof(Enterprise.ErrorReporting.ServiceTasks.ReportErrorsServiceTask),
		MinimumPeriod = "1hour",
		CanRunInAnyBranch = true,
		IsMandatory = true,
		DefaultScheduleRunEvery = "1hour",
		ActiveByDefault = true
	)
]

[assembly: HostedServiceBusinessObjectBinding
	(
		Enterprise.ErrorReporting.ServiceTasks.ReportErrorsServiceTask.Code,
		StmErrorReportSchema.Constants.TableName,
		new[] { StmErrorReportSchema.Constants.QER_TransmitStatus + "=" + StmErrorReportTransmitStatus.Codes.Queued },
		"Queued Errors"
	)
]
namespace Enterprise.ErrorReporting.ServiceTasks
{
	public class ReportErrorsServiceTask : ServiceProviderImpl
	{
		public const string Code = "RET";
		public const int BatchSize = 10;

		public ReportErrorsServiceTask()
		{
			ErrorReportingClientProvider = new ErrorReportingClientProvider();
		}

		public override void RunTask(CancellationToken token)
		{
			SendReports(token);
			DeleteSentReportsOlderThan(TimeSpan.FromDays(7));
		}

		#region Implementation

		public IErrorReportingClientProvider ErrorReportingClientProvider { get; set; }

		static ZQuery BuildQueryForReportsToSend(int batchSize)
		{
			return new ZQuery(StmErrorReportSchema.QER_TransmitStatus, SQLComparisonOperator.Equal, StmErrorReportTransmitStatus.Codes.Queued)
			{
				MaximumRows = batchSize,
				OrderBy = StmErrorReportSchema.Constants.QER_SystemCreateTimeUtc,
			};
		}

		void SendReports(CancellationToken token)
		{
			var query = BuildQueryForReportsToSend(BatchSize);

			bool successful;
			do
			{
				token.ThrowIfCancellationRequested();
				successful = false;
				var reportsToSend = new BusinessObjectFactory().Load<StmErrorReport>(query);
				if (reportsToSend.Length > 0)
				{
					successful = TrySendReports(reportsToSend);
				}
			}
			while (successful);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Got a better idea? Check out the source code of HttpRequestException, there's nothing there.")]
		bool TrySendReports(IEnumerable<StmErrorReport> reports)
		{
			var reportsArray = reports as StmErrorReport[] ?? reports.ToArray();
			var sentReports = new List<StmErrorReport>(capacity: reportsArray.Length); // Let's be optimistic.
			var hardFailReports = new List<StmErrorReport>();

			var uri = SystemDataRegistry.Instance.ErrorReportingServiceUri.Value;
			var timeout = TimeSpan.FromSeconds(SystemDataRegistry.Instance.RETServiceTaskHttpTimeout.Value);
			var timeoutRegistryLocation = ((IMultilingualRegistryItem)SystemDataRegistry.Instance.RETServiceTaskHttpTimeout).LocationMultilingual;

			using (var client = ErrorReportingClientProvider.CreateClient(new Uri(uri), timeout))
			{
				foreach (var report in reportsArray)
				{
					try
					{
						var reportType = (ErrorReportType)(int)report.QER_ReportType;
						//we can get into this invalid situation when there is an error in a non-upgraded client after the SetErrorReportTypeOnOldErrors runs (possibly during upgrade).
						//Which would mean that the error would be created with the QER_ReportType SQL default (0) which equals ErrorReportType.Invalid
						reportType = reportType == ErrorReportType.Invalid ? ErrorReportType.EnterpriseXml : reportType;
						var storedReport = new StoredErrorReport(reportType, report.QER_ReportXml);
						client.PostCrashReportAsync(storedReport).GetAwaiter().GetResult();
						sentReports.Add(report);
					}
					catch (TaskCanceledException)
					{
						hardFailReports.Add(report);
					}
					catch (HttpRequestException ex) when (ex.Message.IndexOf("404", StringComparison.Ordinal) >= 0)
					{
						// The actual response and HTTP status codes are not available from the exception object. Not even with reflection.

						ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Failed to upload error report with 404. Report is {0} characters long.", report.QER_ReportXml.Length), ex);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ServiceLogger.Log(LogType.Error, "Failed to upload error report.", ex);
					}
				}

				if (hardFailReports.Count == reportsArray.Length)
				{
					ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.CurrentCulture, "There was a timeout error for each report. The system will retry to process these items on the next run. Please check your network performance. If error persists, contact WiseTech Global support about increasing timeout limits, or manually edit the timeout value through: {0}", timeoutRegistryLocation));
					ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, "Timeout: {0} milliseconds", timeout.TotalMilliseconds));
					return false;
				}
				else if (hardFailReports.Count > 0)
				{
					SetStatusOnReports(StmErrorReportTransmitStatus.Codes.Failed, hardFailReports);
					ServiceLogger.Log(LogType.Error, "Some error reports could not be delivered due to timeout errors and were marked as failed.");
					ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, "Timeout: {0} milliseconds", timeout.TotalMilliseconds));
				}

				if (sentReports.Count == 0)
				{
					ServiceLogger.Log(LogType.Warning, "Failed to send any reports.");
					return false;
				}
				else
				{
					ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Sent {0} error reports", sentReports.Count));
					SetStatusOnReports(StmErrorReportTransmitStatus.Codes.Sent, sentReports);
				}
				return true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void SetStatusOnReports(string status, IEnumerable<StmErrorReport> reports)
		{
			var reportsArray = reports.ToArray();
			var sql = string.Format(
				"UPDATE [{0}] SET [{1}] = @newStatus WHERE [{2}] IN ({3})",
				StmErrorReportSchema.Constants.TableName,
				StmErrorReportSchema.Constants.QER_TransmitStatus,
				StmErrorReportSchema.Constants.PK,
				string.Join(", ", Enumerable.Range(0, reportsArray.Length).Select(x => string.Format("@reportPK{0}", x)).ToArray()));
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@newStatus", SqlDbType.VarChar, status);

				for (var i = 0; i < reportsArray.Length; i++)
				{
					cmd.AddParameter("@reportPK" + i, SqlDbType.UniqueIdentifier, reportsArray[i].PK.ToGuid());
				}

				cmd.ExecuteNonQuery();
			}

			ServiceLogger.Log(LogType.Debug, string.Format("Marked {0} error reports as '{1}'", reports.Count(), status));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void DeleteSentReportsOlderThan(TimeSpan timeSpan)
		{
			var sql = string.Format(
				"DELETE TOP(@topRowCount) [{0}] WHERE [{1}] < CONVERT(datetime, DATEADD(ms, -@milliseconds, SYSUTCDATETIME())) AND [{2}] = @sentTransmitStatus; SELECT @@ROWCOUNT",
				StmErrorReportSchema.Constants.TableName,
				StmErrorReportSchema.Constants.QER_SystemCreateTimeUtc,
				StmErrorReportSchema.Constants.QER_TransmitStatus);

			using (var cmd = Db.Connection.Command(sql, DbCommand.Timeout.Infinite))
			{
				cmd.AddParameter((NoResString)"@milliseconds", SqlDbType.Int, (int)timeSpan.TotalMilliseconds);
				cmd.AddParameter("@sentTransmitStatus", SqlDbType.Char, 3, StmErrorReportTransmitStatus.Codes.Sent);
				cmd.AddParameter("@topRowCount", SqlDbType.BigInt, deleteBatchSize);

				var numRowsAffected = deleteBatchSize;
				while (numRowsAffected == deleteBatchSize)
				{
					numRowsAffected = (int)cmd.ExecuteScalar();

					if (numRowsAffected > 0)
					{
						ServiceLogger.Log(LogType.Debug, string.Format("Deleted {0:N0} stale error reports", numRowsAffected));
					}
				}
			}
		}

		internal int deleteBatchSize = 3000;

		sealed class StoredErrorReport : IOpaqueErrorReport
		{
			public StoredErrorReport(ErrorReportType type, ZString reportText)
			{
				this.ErrorReportType = type;
				this.reportText = reportText;
			}

			readonly ZString reportText;

			public ErrorReportType ErrorReportType { get; }

			public bool ShouldCompress()
			{
				return reportText.Length >= EnterpriseErrorReportBuilder.ReportCompressionThreshold;
			}

			public async Task WriteToAsync(Stream stream)
			{
				using (var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 4096, leaveOpen: true))
				{
					await writer.WriteAsync(reportText).ConfigureAwait(false);
					await writer.FlushAsync().ConfigureAwait(false);
				}
			}
		}

		#endregion
	}
}
