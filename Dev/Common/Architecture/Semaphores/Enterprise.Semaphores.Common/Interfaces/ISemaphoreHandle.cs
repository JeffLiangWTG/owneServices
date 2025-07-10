using System;

namespace Enterprise.Semaphores.Common
{
	public interface ISemaphoreHandle : IDisposable
	{
		/// <summary>
		/// Semaphore Type
		/// </summary>
		ISemaphoreType Semaphore { get; }

		/// <summary>
		/// Indicate semaphore handle was created successfully or not.
		/// </summary>
		/// <returns>True if successfully created or False if not.</returns>
		bool Success { get; }

		/// <summary>
		/// Create exception if semaphore handle was not created successfully.
		/// </summary>
		/// <returns>An exception if failed to be created and Null if not</returns>
		Exception CreateException { get; }
	}

	interface ISemaphoreDisposal
	{
		OnSemaphoreDisposedDelegate OnSemaphoreDisposed { set; }
	}

	delegate void OnSemaphoreDisposedDelegate(ISemaphoreHandle disposedSemaphore);
}
