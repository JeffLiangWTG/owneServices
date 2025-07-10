using System;

namespace CargoWise.Pipes
{
	[Flags]
	public enum EngineConfigOptions
	{
		None = 0,
		CacheAllResults = 1 << 0,
		DisableAsync = 1 << 1,
	}
}
