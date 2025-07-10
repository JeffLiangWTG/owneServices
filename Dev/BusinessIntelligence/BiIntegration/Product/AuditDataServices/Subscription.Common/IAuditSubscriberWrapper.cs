using System.Collections.Generic;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.AuditDataServices.Subscription.Common
{
	public interface IAuditSubscriberWrapper : IAuditSubscriber
	{
		bool ExistsInSubscriberControlTable { get; }
		ILogger Logger { get; }
		DbConnection auditConnection { get; }
		int BatchSize { get; }

		byte[] NextLsnHighWaterMark { get; set; }
		byte[] NextSeqValHighWaterMark { get; set; }
		int NextPeriodHighWaterMark { get; set; }
		bool IsValid { get; }

		bool HasChangesToProcess();

		void ValidateSubscriber();
		void AddSubscriberToSubscriberControlTable();
		bool FetchDataAndProcessChanges();
		void UpdateLsnHighWaterMark();
		bool ShouldRunSubscriber();
		bool HasChangesToProcessFromCache(Dictionary<string, Lsn> tableStateLsnHighWaterMarkCache, Dictionary<string, Lsn> subscriberLsnHighWaterMarkCache, Dictionary<string, int> subscriberCommandIdHighWaterMarkCache);
	}
}
