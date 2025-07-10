using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public abstract class SystemSchematicServiceTaskBase : BMSServiceTaskBase
	{
		public static TimeSpan LogExpiryTime => TimeSpan.FromMinutes(60);

		protected override void RunTaskCore(CancellationToken token)
		{
			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				RunCore(token);
			}
		}

		void RunCore(CancellationToken token)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "Buffer Management Service Task - System Factory", RefreshEnabled = false };
			var systems = factory.Load<BMSystem>(new ZQuery(BMSystemSchema.FS_IsLive, true));

			OnSystemsLoaded();

			var currentTypeName = GetType().Name;

			foreach (var system_unsafe in systems)
			{
				if (token.IsCancellationRequested)
				{
					return;
				}

				var newFactory = new BusinessObjectFactory { NameForDebugging = currentTypeName + ":" + system_unsafe.FS_Name, RefreshEnabled = false };
				var system = newFactory.Load<BMSystem>(GetOneSystemQuery(system_unsafe)).SingleOrDefault();

				if (system == null)
				{
					continue;
				}

				var failureService = GetReleaseGateFailureLogService(newFactory);
				var releaseGateLogger = new ReleaseGateLoggerWithFailureServices(failureService);
				var processor = GetProcessor(system, releaseGateLogger);

				processor.Process(token);
				releaseGateLogger.CommitAllLogs(ReleaseGateKeeper.GetFactoryForSavingLogs());
			}

			ZQuery GetOneSystemQuery(BMSystem system)
			{
				var query = new ZQuery(BMSystemSchema.PK, system.PK);
				query.AddToFilter(new ZQuery(BMSystemSchema.FS_IsLive, true));

				return query;
			}

			OnSystemsProcessed();
		}

		protected virtual ReleaseGateFailureLogService GetReleaseGateFailureLogService(BusinessObjectFactory factory)
		{
			return ReleaseGateFailureLogService.GetOrAddReleaseGateFailureLogService(factory);
		}

		protected abstract IPAVEProcessor GetProcessor(BMSystem system, ReleaseGateLogger releaseGateLogger);

		protected virtual void OnSystemsLoaded()
		{
		}

		protected virtual void OnSystemsProcessed()
		{
		}
	}
}
