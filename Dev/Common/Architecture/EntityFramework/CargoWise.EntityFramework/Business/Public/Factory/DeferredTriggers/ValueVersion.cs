using System;

namespace CargoWise.EntityFramework
{
	[Flags]
	public enum ValueVersion
	{
		Current = 1,
		Original = 2,
		Both = Current | Original
	}
}
