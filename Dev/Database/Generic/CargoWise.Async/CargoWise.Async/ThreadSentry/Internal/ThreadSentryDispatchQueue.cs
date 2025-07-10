using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using CargoWise.Common;

namespace CargoWise.Async
{
	class ThreadSentryDispatchQueue
	{
		public ThreadSentryDispatchQueue(ThreadSentry threadSentry)
		{
			Argument.NotNull(threadSentry, nameof(threadSentry));
			sentry = threadSentry;
			delegates = new ConcurrentQueue<(Delegate, Delegate, IThreadSentry, object, string)>();
		}

		readonly object syncObject = new object();
		readonly ThreadSentry sentry;
		readonly ConcurrentQueue<(Delegate method, Delegate callback, IThreadSentry callbackSentry, object state, string callingMethod)> delegates;
		volatile bool isRunning;

		public bool Enqueue(Delegate method, object state, IThreadSentry callbackSentry, Delegate callback, string callingMethod)
		{
			Argument.NotNull(method, nameof(method));

			if (sentry.IsOwner && isRunning)
			{
				ErrorReporter.ReportOnce("3a2ec01f-6446-477b-afe3-ef8880f71f50", string.Format(CultureInfo.InvariantCulture, "Posts to the ThreadSentry Dispatcher should never add more Posts to the ThreadSentry Dispatcher. Delegate [{0}], Args [{1}], Stack [{2}]", method, string.Join(",", state), new StackTrace().ToString()));
				return false;
			}
			else
			{
				delegates.Enqueue((method, callback, callbackSentry, state, callingMethod));
				return true;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Eating exception caused by Thread Sentry Dispatch so weird async related bugs don't crash the program.")]
		internal void ExecuteAll()
		{
			lock (syncObject)
			{
				if (isRunning)
				{
					return;
				}

				isRunning = true;
			}

			try
			{
				while (true)
				{
					(Delegate method, Delegate callback, IThreadSentry callbackSentry, object state, string callingMethod) tuple;
					lock (syncObject)
					{
						if (!delegates.TryDequeue(out tuple))
						{
							isRunning = false;
							return;
						}
					}

					try
					{
						tuple.method.DynamicInvoke(tuple.state);
						if (tuple.callback != null)
						{
							tuple.callbackSentry?.Post((SendOrPostCallback)tuple.callback, tuple.state, tuple.callingMethod);
						}
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						ErrorReporter.ReportOnce(FormattableString.Invariant($"Error processing action dispatched to ThreadSentry. Calling Method: {tuple.callingMethod}"), e);
					}
				}
			}
			finally
			{
				isRunning = false;
			}
		}
	}
}
