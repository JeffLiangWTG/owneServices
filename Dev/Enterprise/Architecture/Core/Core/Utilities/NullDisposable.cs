using System;

namespace Enterprise.ZArchitecture.Core
{
	public sealed class NullDisposable : IDisposable
	{
		public void Dispose()
		{
		}

		// Used when we need an IDisposable for an interface but don't require any actual Dispose() logic.
		public static IDisposable Instance { get; } = new NullDisposable();
	}
}
