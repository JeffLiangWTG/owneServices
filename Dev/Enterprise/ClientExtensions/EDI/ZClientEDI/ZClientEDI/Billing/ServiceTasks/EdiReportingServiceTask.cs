using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Client.EDI.Billing.ServiceTasks.EdiReportingServiceTask.Code,
	"Edi Reporting",
	"CSP",
	typeof(Enterprise.Client.EDI.Billing.ServiceTasks.EdiReportingServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "5Minutes",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Client.EDI.Billing.ServiceTasks.EdiReportingServiceTask.Code, EdiReportingQueueSchema.Constants.TableName,
		new string[] {
		EdiReportingQueueSchema.Constants.ERQ_Status + "=" + EdiReportingQueue.QueueStatus.NewReport,
		},
	null,
	ClientSpecificCode = Clients.EDI)]

namespace Enterprise.Client.EDI.Billing.ServiceTasks
{
	[NeedsDataRefresh]
	public class EdiReportingServiceTask : ServiceProviderImpl
	{
		public const string Code = "ERS";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Begin processing");

			if (!string.IsNullOrEmpty(EDIDataRegistry.Instance.MyAccountReportsShareFolderPath.Value))
			{
				try
				{
					var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						ProcessNewReports(token);
						RemoveOldReports(token);
					}
				}
				catch (Exception ex)
				{
					ReportException(ex);
					throw;
				}
			}

			ServiceLogger.Log(LogType.Information, "End processing");
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		void ProcessNewReports(CancellationToken token)
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery(EdiReportingQueueSchema.ERQ_Status, EdiReportingQueue.QueueStatus.NewReport);
			query.MaximumRows = 5;
			query.OrderBy = EdiReportingQueueSchema.Constants.ERQ_CreateTimeUtc;
			var newReports = factory.Load<EdiReportingQueue>(query);

			foreach (var report in newReports)
			{
				token.ThrowIfCancellationRequested();
				try
				{
					ServiceLogger.Log(LogType.Information, ZString.Format("Generating... [{0}]", report.PK.ToString()));
					report.GenerateReport();
					factory.Save();
					ServiceLogger.Log(LogType.Information, ZString.Format("Generated... [{0}] [{1}]", report.ERQ_ReportName, report.ERQ_ReportFileFullName));
				}
				catch (Exception ex)
				{
					ServiceLogger.Log(LogType.Error, ex.Message, ex);
					if (!report.CanRetryOnError)
					{
						MarkFailedReport(report.PK);
						ServiceLogger.Log(LogType.Error, ZString.Format("Faild... [{0}]", report.PK.ToString()));
					}
					else
					{
						ServiceLogger.Log(LogType.Information, ZString.Format("Retry will be attempted later... [{0}]", report.PK.ToString()));
					}
				}
			}
		}

		void MarkFailedReport(ZGuid pk)
		{
			var factory = new BusinessObjectFactory();
			var rpt = factory.Load<EdiReportingQueue>(pk);
			if (rpt != null)
			{
				rpt.ERQ_Status = EdiReportingQueue.QueueStatus.Failed;
				factory.Save();
			}
		}

		void RemoveOldReports(CancellationToken token)
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery(EdiReportingQueueSchema.ERQ_CreateTimeUtc, SQLComparisonOperator.LessThan, ZDateTime.Now.AddDays(-5));
			query.MaximumRows = 5;
			var oldReports = factory.Load<EdiReportingQueue>(query);

			foreach (var report in oldReports)
			{
				token.ThrowIfCancellationRequested();
				ServiceLogger.Log(LogType.Information, ZString.Format("Removing... [{0}][{1}]", report.ERQ_ReportName, report.ERQ_ReportFileFullName));
				report.Delete();
				factory.Save();
				ServiceLogger.Log(LogType.Information, ZString.Format("Removed"));
			}
		}

		void ReportException(Exception ex)
		{
			ErrorReporter.ReportOnce("Unhandled exception from EdiReportingServiceTask", ex.Message, ex);
		}
	}
}
