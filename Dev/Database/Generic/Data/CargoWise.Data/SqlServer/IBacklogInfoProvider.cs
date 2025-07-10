using System;

namespace CargoWise.Data.SqlServer
{
	public interface IBacklogInfoProvider
	{
		AttemptInGettingBacklog GetCurrentBacklog();
		Int64 AcceptableBacklog { get; }
		TimeSpan[] Timespans { get; }
	}
}