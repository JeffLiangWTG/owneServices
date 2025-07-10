using Enterprise.ServiceManager.Shared.Interfaces;
using NLog;
using NLog.Targets;
using NLog.Targets.ElasticSearch;
using NLog.Targets.Wrappers;
using ServiceManager.Shared.CW;

namespace ServiceManager.Logging.CW
{
	class NLogElasticsearchTargetFactory : INLogTargetFactory
	{
		public Target? GetOrCreateTarget()
		{
			var registry = (ILoggerRegistrySettings)SharedRegistry.Instance;
			if (!registry.ElasticSearchLoggingEnabled)
			{
				return null;
			}

			const string targetName = "elasticsearch";

			var existingBufferingTarget = LogManager.Configuration.FindTargetByName<BufferingTargetWrapper>(targetName);
			if (existingBufferingTarget != null)
			{
				return existingBufferingTarget;
			}

			var elasticSearchTarget = new ElasticSearchTarget
			{
				Name = targetName,
				Uri = registry.ElasticsearchServiceUri,
				Index = registry.ElasticsearchIndex + "${date:universalTime=true:format=yyyyMMdd}",
				Username = registry.ElasticsearchServerUserName,
				Password = registry.ElasticsearchServerPassword,
				RequireAuth = true,
				IncludeAllProperties = true,
				IncludeDefaultFields = true,
				Layout = "${message}",
			};

			var elasticSearchBufferWrapper = new BufferingTargetWrapper
			{
				WrappedTarget = elasticSearchTarget,
				BufferSize = 50,
				FlushTimeout = 1000,
				SlidingTimeout = true,
				Name = targetName,
			};

			return elasticSearchBufferWrapper;
		}
	}
}
