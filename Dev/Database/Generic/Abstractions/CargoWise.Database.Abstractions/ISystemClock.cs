using System;

namespace CargoWise.Database.Abstractions
{
	public interface ISystemClock
	{
		DateTime UtcNow { get; }
	}
}
