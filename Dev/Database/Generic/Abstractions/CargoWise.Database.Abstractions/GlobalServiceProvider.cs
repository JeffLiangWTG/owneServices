using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace CargoWise.Database.Abstractions
{
	public static class GlobalServiceProvider
	{
		public static IServiceProvider Instance => instance ?? throw new InvalidOperationException("The GlobalServiceProvider must be initialized before use.");

		[SuppressMessage("CargoWiseOne", "CW1021", Justification = "This needs to be global, but is thread-safe.")]
		static IServiceProvider instance;

		public static IDisposable Configure(IServiceProvider provider)
		{
			var originalValue = Interlocked.Exchange(ref instance, provider);
			return new GlobalServiceProviderResetter(originalValue, provider);
		}

		// Avoid polluting intellisense with rarely-needed method. This should only ever be used in careful code
		// before a service provider is initialised, such as very early in the application startup routines.
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool TryGetInstance(out IServiceProvider provider)
		{
			provider = Volatile.Read(ref instance);
			return provider != null;
		}

		sealed class GlobalServiceProviderResetter : IDisposable
		{
			public GlobalServiceProviderResetter(IServiceProvider originalValue, IServiceProvider currentValue)
			{
				this.originalValue = originalValue;
				this.currentValue = currentValue;
			}

			readonly IServiceProvider originalValue;
			readonly IServiceProvider currentValue;
			bool disposed;

			public void Dispose()
			{
				if (disposed)
				{
					return;
				}

				if (Interlocked.CompareExchange(ref instance, originalValue, currentValue) != currentValue)
				{
					throw new InvalidOperationException("The GlobalServiceProvider could not be set back to it's original value, as the value has been changed in the iterim.");
				}

				disposed = true;
			}
		}
	}
}
