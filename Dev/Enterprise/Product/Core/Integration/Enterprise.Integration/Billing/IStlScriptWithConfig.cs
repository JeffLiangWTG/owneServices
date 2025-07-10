using System;

namespace Enterprise.Integration.Billing
{
	public interface IStlScriptWithConfig
	{
		IStlItem Script { get; }
		DateTime NextStartTimeUtc { get; }
		IStlItemRegistrySettings HighWaterMarkSettings { get; }
		IStlItemRegistrySettings LegacyHighWaterMarkSettings { get; }
		bool IsExceptionWithinThreshold(TimeSpan exceptionThreshold);
	}
}
