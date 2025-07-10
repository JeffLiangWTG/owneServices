using System;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice.Export;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"ZW2",
	"Export Commercial Invoice Line Data",
	"CSP",
	typeof(Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice.ExportDeclarationInvoiceServiceTask),
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true
	)]

namespace Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice
{
	public class ExportDeclarationInvoiceServiceTask : ServiceProviderImpl
	{
		#region Override

		static readonly int DefaultInterval = -1;

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			if (AreAllRegistrySetupCorrectly)
			{
				var runTime = ZDateTime.UtcNow;
				var lastRunTime = WowDataRegistry.Instance.DeclarationInvoiceExportLastRun == DateTime.MinValue ? runTime.AddDays(DefaultInterval) : WowDataRegistry.Instance.DeclarationInvoiceExportLastRun;
				lastRunTime = lastRunTime.ToUtc();
				WowDataRegistry.Instance.DeclarationInvoiceExportLastRun = runTime.ToLocal();

				var factory = new BusinessObjectFactory();
				var declarations = GetDeclarationsForExport(factory, lastRunTime, runTime);
				if (declarations.Count > 0)
				{
					var collectionReader = new CollectionWrapperBusinessObjectReader(declarations);
					var exporter = new DeclarationInvoiceFlatFileDataExporter(factory);
					var buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber());

					try
					{
						exporter.Export(collectionReader, buffer);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						buffer.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
					}

					if (!buffer.HasErrors && !buffer.HasWarnings && !buffer.AsString.Contains(Constants.NothingToExport))
					{
						ServiceLogger.Log(LogType.Information, Constants.SuccessfullyExport);
					}
				}
				else
				{
					ServiceLogger.Log(LogType.Information, Constants.NothingToExport);
				}
			}
		}

		#endregion

		#region Implementation

		JobDeclarationCollection GetDeclarationsForExport(BusinessObjectFactory factory, ZDateTime lastRunTime, ZDateTime runTime)
		{
			var filter = new ZQuery(StmALogSchema.SL_Table, JobDeclaration.Schema.TableName);
			filter.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, lastRunTime);
			filter.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThan, runTime);
			var eventTypeFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code);
			eventTypeFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.EditedARecord.Code);
			filter.AddToFilter(eventTypeFilter);
			var logs = factory.Load<StmALog>(filter);

			var jE_PKs = (
				from log in logs
				select log.SL_Parent)
				.Distinct();

			var declarations = new JobDeclarationCollection(factory);

			foreach (ZGuid jE_PK in jE_PKs)
			{
				declarations.Add(factory.Load<JobDeclaration>(jE_PK));
			}

			return declarations;
		}

		#endregion

		public bool AreAllRegistrySetupCorrectly
		{
			get
			{
				var mesgBuilder = new ZStringBuilder();

				if (WowDataRegistry.Instance.DeclarationInvoiceExportDirectory.IsEmpty || !Directory.Exists(WowDataRegistry.Instance.DeclarationInvoiceExportDirectory))
				{
					mesgBuilder.Append(Constants.ExportDirectoryHasInvalidPath);
				}

				if (!mesgBuilder.IsEmpty)
				{
					ServiceLogger.Log(LogType.Error, mesgBuilder.ToStringWithNewLineBetweenAppends());
				}

				return mesgBuilder.IsEmpty;
			}
		}
	}
}
