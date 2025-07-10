using NLog.Targets;

namespace ServiceManager.Logging.CW
{
	interface INLogTargetFactory
	{
		Target? GetOrCreateTarget();
	}
}
