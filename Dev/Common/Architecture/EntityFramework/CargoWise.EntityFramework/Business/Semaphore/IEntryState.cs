using System;

namespace CargoWise.EntityFramework
{
	public interface IEntryState : IDisposable
	{
		bool IsAllowed { get; }
	}
}
