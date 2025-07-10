using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging.Billing;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class UsageSubmissionFailureResetterJob : ServiceTaskJob
	{
		readonly int batchSize;

		public UsageSubmissionFailureResetterJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, int batchSize = 1000)
			: base(serviceTaskSupport, notifier)
		{
			this.batchSize = batchSize;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal override void ExecuteInternal()
		{
			var recordsReset = 0;
			var bizOFactory = new BusinessObjectFactory();
			var query = new ZQuery(StmUsageDataSchema.SUD_Fail, SQLComparisonOperator.Equal, new ZBool(true));
			query.MaximumRows = batchSize;
			query.OrderBy = StmUsageDataSchema.Constants.SUD_PostedTimeUtc;

			var failedRecords = bizOFactory.Load<StmUsageData>(query);
			if (failedRecords.Length > 0)
			{
				foreach (var failedRecord in failedRecords)
				{
					if (FailedRecordShouldBeReset(failedRecord))
					{
						failedRecord.SUD_Fail = false;
						++recordsReset;
					}
					else
					{
						failedRecord.Delete();
					}
				}
				bizOFactory.Save();
				OnAtLeastOneMessageProcessed();
			}

			Notifier.Notify(new InfoNotification(Res.GetString("F932152E-553E-4B99-BFB8-A105F3FC8953", "Reset SUD_Fail=0 for {0} usage record(s).", recordsReset)));
			Notifier.Notify(new InfoNotification(Res.GetString("AA3590C7-B5D4-442E-BC3A-BB6F7FC6A360", "Deleted {0} expired usage record(s).", failedRecords.Length - recordsReset)));
		}

		bool FailedRecordShouldBeReset(StmUsageData data)
		{
			var script = scripts.Value.FirstOrDefault(s => s.Code == data.SUD_Code);
			var isMandatoryForMilestones = script == null || script.IsMandatoryForMilestones;
			var expiryTime = isMandatoryForMilestones ? ZDateTime.UtcNow.ToDateTime().AddMonths(-3) : ZDateTime.UtcNow.ToDateTime().AddDays(-7);
			return data.SUD_PostedTimeUtc >= expiryTime;
		}

		readonly Lazy<IEnumerable<IStlItem>> scripts = new(() => ObjectFactory.Get<IScriptFactory>().CreateScripts(new BusinessObjectFactory()));
	}
}
