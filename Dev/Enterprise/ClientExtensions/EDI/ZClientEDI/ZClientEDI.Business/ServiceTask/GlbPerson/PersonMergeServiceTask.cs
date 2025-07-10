using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Client.EDI.ServiceTask.PersonMergeServiceTask.Code,
	Enterprise.Client.EDI.ServiceTask.PersonMergeServiceTask.Description,
	"CSP",
	typeof(Enterprise.Client.EDI.ServiceTask.PersonMergeServiceTask),
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = true,
	MinimumPeriod = "1Seconds",
	DefaultScheduleRunEvery = "1minute"
	)]

namespace Enterprise.Client.EDI.ServiceTask
{
	public class PersonMergeServiceTask : ServiceProviderImpl
	{
		public const string Code = "PMM";
		public const string Description = "Person Merge Service Task";
		const string AppLockKey = "PersonMergeServiceTask:EdiPersonMergeQueue";
		public const int BatchSize = 30;

		public override void RunTask(CancellationToken cancellationToken)
		{
			ServiceLogger.Log(LogType.Information, "Begin processing");

			var mergedCount = 0;
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var query = new ZDBOnlyQuery(typeof(EdiPersonMergeQueue));
			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(factory);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			using (var loadWithAppLockResult = factory.LoadWithApplocks<EdiPersonMergeQueue>(AppLockKey, query, BatchSize))
			{
				var queueItems = loadWithAppLockResult.Values;
				if (queueItems.Any())
				{
					factory.AddFetchHint(GlbPersonSchema.Instance, new ZQuery(GlbPersonSchema.PK, queueItems.Select(x => x.EMQ_PER_RetainPerson).Union(queueItems.Select(x => x.EMQ_PER_DissolvePerson)).ToArray()));

					var processedItemPkList = new List<ZGuid>(BatchSize);
					foreach (var item in queueItems)
					{
						if (cancellationToken.CanBeCanceled && cancellationToken.IsCancellationRequested)
						{
							cancellationToken.ThrowIfCancellationRequested();
						}

						if (item.RetainPerson != null && item.DissolvePerson != null && item.EMQ_PER_RetainPerson != item.EMQ_PER_DissolvePerson)
						{
							try
							{
								var logMessage = FormattableString.Invariant($"Merged [{item.DissolvePerson.PER_FullName}]({item.EMQ_PER_DissolvePerson}) => [{item.RetainPerson.PER_FullName}]({item.EMQ_PER_RetainPerson})");

								using (var merger = new PersonMerger(item.RetainPerson, item.DissolvePerson))
								{
									merger.Merge();
								}
								processedItemPkList.Add(item.PK);
								mergedCount++;

								ServiceLogger.Log(LogType.Debug, logMessage);
							}
							catch (Exception ex) when (!ex.IsCriticalException())
							{
								ServiceLogger.Log(LogType.Error, ex.ToString());
								ErrorReporter.ReportOnce("Failed to merge persons", ex.Message, ex);
							}
						}
						else
						{
							processedItemPkList.Add(item.PK);
						}
					}

					var deleteFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var itemsToDelete = deleteFactory.Load<EdiPersonMergeQueue>(new ZQuery(EdiPersonMergeQueueSchema.PK, processedItemPkList.ToArray()));
					foreach (var item in itemsToDelete)
					{
						item.Delete();
					}
					deleteFactory.Save();
				}
			}

			ServiceLogger.Log(LogType.Information, FormattableString.Invariant($"{mergedCount} person pair(s) merged"));
			ServiceLogger.Log(LogType.Information, "End processing");
		}
	}
}
