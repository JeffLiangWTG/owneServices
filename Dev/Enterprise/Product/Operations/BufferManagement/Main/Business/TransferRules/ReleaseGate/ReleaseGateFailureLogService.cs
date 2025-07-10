using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Cache;
using CargoWise.Types;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	public class ReleaseGateFailureLogService : IService
	{
		Lazy<IDistributedCache> lazyDistributedCache => new Lazy<IDistributedCache>(() => ObjectFactory.Get<IDistributedCacheFactory>().Create());

		protected virtual IDistributedCache DistributedCache => lazyDistributedCache.Value;

		internal Dictionary<Guid, ReleaseGateFailureCacheItem> Logs { get; private set; } = new Dictionary<Guid, ReleaseGateFailureCacheItem>();

		public void QueueFailureLog(ZGuid workflowPk, Dictionary<ZGuid, string> logs)
		{
			foreach (var log in logs)
			{
				var bufferPK = log.Key;
				var workflowLogs = Logs.GetOrAdd(bufferPK.ToGuid(), () => new ReleaseGateFailureCacheItem());
				var values = workflowLogs.Logs.GetOrAdd(workflowPk.ToGuid(), () => new ReleaseGateFailureLogs());

				values.Logs.Add(log.Value);
			}
		}

		public void SubmitQueueToRemoteCache() => SubmitQueueToRemoteCacheCore();

		static string GetKey(Guid bufferPK) => "ReleaseGateFailureLogPerBuffer:" + bufferPK;

		protected virtual void SubmitQueueToRemoteCacheCore()
		{
			var expireAt = ZDateTimeOffset.UtcNow.ToDateTimeOffset() + LogExpiryTime;
			var cacheOptions = new DistributedCacheOptions(expireAt);

			foreach (var item in Logs)
			{
				DistributedCache.Set(GetKey(item.Key), item.Value, cacheOptions, StaticCurrentFetcher.Instance.CurrentUserCode);
			}

			Logs.Clear();
		}

		public string[] GetReleaseFailureLogs(ZGuid bufferPK, ZGuid workflowPk)
		{
			var bufferLogs = DistributedCache.Get<ReleaseGateFailureCacheItem>(GetKey(bufferPK.ToGuid()));

			return bufferLogs?.Logs.GetValueSafe(workflowPk.ToGuid())?.Logs?.ToArray();
		}

		public const string ReleaseFailureLogReferenceKey = "ReleaseFailure";
		public const int CurrentReleaseFailureLogVersion = 1;

		protected static TimeSpan LogExpiryTime => SystemSchematicServiceTaskBase.LogExpiryTime;

		public static ReleaseGateFailureLogService GetOrAddReleaseGateFailureLogService(BusinessObjectFactory factory)
		{
			var containedService = factory.ServiceContainer.GetService<ReleaseGateFailureLogService>();
			return containedService ?? factory.ServiceContainer.AddService(new ReleaseGateFailureLogService());
		}
	}

	internal class ReleaseGateFailureCacheItem
	{
		public Dictionary<Guid, ReleaseGateFailureLogs> Logs { get; set; } = new Dictionary<Guid, ReleaseGateFailureLogs>();
	}

	internal class ReleaseGateFailureLogs
	{
		public Guid WorkflowPK { get; set; }
		public List<string> Logs { get; set; } = new List<string>();
	}
}
