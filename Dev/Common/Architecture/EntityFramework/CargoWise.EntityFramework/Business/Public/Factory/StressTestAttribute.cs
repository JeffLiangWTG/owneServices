#if DEBUG
using System;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class StressTestAttribute : Attribute
	{
	}
}

#endif
