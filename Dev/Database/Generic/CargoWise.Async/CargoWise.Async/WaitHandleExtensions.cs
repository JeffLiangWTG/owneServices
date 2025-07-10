using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;

namespace CargoWise.Async
{
	public static class WaitHandleExtensions
	{
		public static Task<bool> WaitOneAsync(this WaitHandle handle, CancellationToken cancel = default(CancellationToken))
		{
			Argument.NotNull(handle, nameof(handle));

			return WaitOneCoreAsync(handle, Timeout.Infinite, cancel);
		}

		public static Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			Argument.NotNull(handle, nameof(handle));

			return handle.WaitOneAsync(Math.Max(0, (int)timeout.TotalMilliseconds), token);
		}

		public static Task<bool> WaitOneAsync(this WaitHandle handle, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			Argument.NotNull(handle, nameof(handle));
			if (millisecondsTimeout < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(millisecondsTimeout));
			}

			return WaitOneCoreAsync(handle, millisecondsTimeout, token);
		}

		async static Task<bool> WaitOneCoreAsync(WaitHandle wait, int millisecondsTimeout, CancellationToken cancel)
		{
			Argument.NotNull(wait, nameof(wait));

			var taskSource = new TaskCompletionSource<bool>();

			if (cancel.IsCancellationRequested)
			{
				taskSource.TrySetCanceled();
				return await taskSource.Task;
			}

			if (millisecondsTimeout == 0)
			{
				taskSource.TrySetResult(wait.WaitOne(0));
				return await taskSource.Task;
			}

			if (wait.WaitOne(0))
			{
				taskSource.TrySetResult(true);
				return await taskSource.Task;
			}

			/*
			 * There is a known race condition between the WaitHandle callback and the cancellation
			 * callback, however it will probably only have meaningful effect for auto reset events
			 * (if the cancellation callback calls TrySetCancelled after the signal callback starts
			 * but before it calls TrySetResult then the returned task will be marked as cancelled
			 * and the event will have auto-reset.)
			 *
			 * The solution is probably to have the cancellation callback unregister the event
			 * callback before calling TrySetCancelled (be sure to check that that the callback has
			 * not already started/has completed before calling TrySetCancelled).
			 *
			 * Doing this in a robut way, however, will just add extra complexity to support a
			 * situation that probably won't be needed anyway.
			 */

			CancellationTokenRegistration ctr = default;
			if (cancel.CanBeCanceled)
			{
				ctr = cancel.Register(delegate { taskSource.TrySetCanceled(); });
			}

			var handle = ThreadPool.RegisterWaitForSingleObject(
				wait,
				(_, timedOut) => taskSource.TrySetResult(!timedOut),
				null,
				millisecondsTimeout,
				true);

			try
			{
				return await taskSource.Task.ConfigureAwait(false);
			}
			finally
			{
				ctr.Dispose();
				handle.Unregister(null);
			}
		}
	}
}
