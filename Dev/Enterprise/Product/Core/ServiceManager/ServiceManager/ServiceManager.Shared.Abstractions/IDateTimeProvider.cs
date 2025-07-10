using System;

namespace ServiceManager.Shared.Abstractions
{
	public interface IDateTimeProvider
	{
		DateTime CurrentDateTimeUtc { get; }
	}
}
