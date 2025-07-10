using System;
using CargoWise.Application;
using Enterprise.Integration.Billing;
using Enterprise.Integration.Licensing;

namespace Enterprise.Billing.StlCollector.Retriever
{
	class ScriptWithConfig : IStlScriptWithConfig
	{
		public ScriptWithConfig(IStlItem script, IStlItemRegistrySettings highWaterMarkSettings, IStlItemRegistrySettings legacyHighWaterMarkSettings = null)
			 : this(script, highWaterMarkSettings, legacyHighWaterMarkSettings, new CollectionTimeProvider())
		{
		}

		public ScriptWithConfig(IStlItem script, IStlItemRegistrySettings highWaterMarkSettings, IStlItemRegistrySettings legacyHighWaterMarkSettings, CollectionTimeProvider timeProvider)
		{
			Script = script;
			HighWaterMarkSettings = highWaterMarkSettings;
			LegacyHighWaterMarkSettings = legacyHighWaterMarkSettings;
			this.timeProvider = timeProvider;
		}

		readonly CollectionTimeProvider timeProvider;

		public IStlItem Script { get; }
		public DateTime NextStartTimeUtc
		{
			get
			{
				var nextStartTimeUtc = (HighWaterMarkSettings.HighWaterMark != DateTime.MinValue) ? HighWaterMarkSettings.HighWaterMark : Script.CollectionStartDateUtc;
				if (nextStartTimeUtc == DateTime.MinValue)
				{
					nextStartTimeUtc = (LegacyHighWaterMarkSettings != null) ? LegacyHighWaterMarkSettings.HighWaterMark : DateTime.MinValue;
				}

				if (nextStartTimeUtc != DateTime.MinValue && (!ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem() || Script.StlGrain == StlDataGrain.Snapshot))
				{
					return nextStartTimeUtc;
				}

				var defaultTime = timeProvider.GetDefaultStartTime(Script);
				return (nextStartTimeUtc > defaultTime) ? nextStartTimeUtc : defaultTime;
			}
		}

		public bool IsExceptionWithinThreshold(TimeSpan exceptionThreshold)
		{
			var timeElapsed = timeProvider.MaximumSafeEndDateTimeExclusive - HighWaterMarkSettings.HighWaterMark;
			return Script.CollectionException != null && timeElapsed <= exceptionThreshold;
		}
		public IStlItemRegistrySettings HighWaterMarkSettings { get; private set; }
		public IStlItemRegistrySettings LegacyHighWaterMarkSettings { get; private set; }
	}
}
