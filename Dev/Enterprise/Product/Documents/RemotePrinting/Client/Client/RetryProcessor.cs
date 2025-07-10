using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.Services.Protocols;
using System.Xml;
using CargoWise.Common;

namespace Enterprise.RemotePrinting.Client
{
	public class RetryProcessor
	{
		public RetryProcessor(INotifications notifications = null)
		{
			this.notifications = notifications;
		}

		readonly INotifications notifications;

		public IErrorResponseWebRequestProcessor ResponseProcessor { get; set; }

		public Delegate SimpleRequestDelegate { get; set; }

		public HashSet<string> RetrySuccessfullyOperations { get; } = new HashSet<string>();

		public T3 InvokeFunc<T1, T2, T3>(Func<T1, T2, T3> func, int retries, TimeSpan maxTimeSpan, T1 arg1, T2 arg2)
		{
			var result = Retry<T3>(func, retries, maxTimeSpan, arg1, arg2);
			return result;
		}

		public T2 InvokeFunc<T1, T2>(Func<T1, T2> func, int retries, TimeSpan maxTimeSpan, T1 arg)
		{
			var result = Retry<T2>(func, retries, maxTimeSpan, arg);
			return result;
		}

		public T InvokeFunc<T>(Func<T> func, int retries, TimeSpan maxTimeSpan)
		{
			var result = Retry<T>(func, retries, maxTimeSpan);
			return result;
		}

		public void InvokeAction<T>(Action<T> action, int retries, TimeSpan maxTimeSpan, T arg)
		{
			Retry<object>(action, retries, maxTimeSpan, arg);
		}

		public void InvokeAction<T1, T2>(Action<T1, T2> action, int retries, TimeSpan maxTimeSpan, T1 arg1, T2 arg2)
		{
			Retry<object>(action, retries, maxTimeSpan, arg1, arg2);
		}

		public void InvokeAction<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, int retries, TimeSpan maxTimeSpan, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			Retry<object>(action, retries, maxTimeSpan, arg1, arg2, arg3, arg4);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Notification add message")]
		protected virtual T Retry<T>(Delegate action, int retries, TimeSpan timeLimit, params object[] args)
		{
			var retryCounter = 0;
			var maxRetries = retries > MaxRetries ? MaxRetries : retries;
			var methodName = action.Method.Name;

			T result = default;
			MethodDelegate processAction = () =>
			{
				result = (T)action.DynamicInvoke(args);
				RetrySuccessfullyOperations.Add(methodName);
				return true;
			};
			MethodDelegate retryAction = null;
			var success = false;
			var isTemporaryRetryAction = false;

			Exception outException = null;

			Stopwatch stopwatch = null;
			if (timeLimit > TimeSpan.Zero)
			{
				stopwatch = new Stopwatch();
				stopwatch.Start();
			}

			while (true)
			{
				var forceRetry = false;

				try
				{
					if (ResponseProcessor != null)
					{
						success = ResponseProcessor.Process(processAction, out retryAction, out outException, isTemporaryRetryAction);
						if (!success && retryAction != null && retryAction != processAction)
						{
							isTemporaryRetryAction = forceRetry = true; // Force retry even if MaxRetries == 0
						}

						retryCounter++;
					}
					else
					{
						success = processAction();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					retryCounter++;
					if (retryCounter >= maxRetries ||
						(stopwatch != null && stopwatch.Elapsed > timeLimit) ||
					   !ShouldRetry(ex))
					{
						throw new OperationRetryException(methodName, ex);
					}

					notifications?.AddMessage("  - retrying operation, attempt #" + retryCounter);

					Thread.Sleep(GetDelay(retryCounter));
				}
				finally
				{
					stopwatch?.Stop();
				}

				if (success)
				{
					return result;
				}

				if (retryCounter > MaxRetries && !forceRetry && outException != null)
				{
					throw new OperationRetryException(methodName, outException);
				}

				if (retryAction != null)
				{
					processAction = retryAction;
				}
			}
		}

		int GetDelay(int retryCounter)
		{
			var delayIndex = retryCounter - 1;
			if (delayIndex < 0)
			{
				delayIndex = 0;
			}
			if (delayIndex >= retryDelays.Length)
			{
				delayIndex = retryDelays.Length - 1;
			}

			return retryDelays[delayIndex];
		}

		public const int MaxRetries = 3;
		public const int MinRetries = 1;
		readonly int[] retryDelays = { 100, 500, 1000 };

#if DEBUG
		protected
#endif
		bool ShouldRetry(Exception exception)
		{
			while (exception != null)
			{
				switch (exception)
				{
					case WebException wex when ShouldRetryWebException(wex):
					case SoapException _:
					case XmlException _:
					case IOException _:
						{
							return true;
						}
				}

				exception = exception.InnerException;
			}

			return false;
		}

#if DEBUG
		protected
#endif
		bool ShouldRetryWebException(WebException exception)
		{
			switch (exception.Status)
			{
				case WebExceptionStatus.ProtocolError when (exception.Response is HttpWebResponse httpResponse && httpResponse.StatusCode == HttpStatusCode.RequestEntityTooLarge):
				case WebExceptionStatus.MessageLengthLimitExceeded:
					{
						SimpleRequestDelegate?.DynamicInvoke(); // Do simple request first to establish connection and do handshakes and authentication, then repeat
						return true;
					}
				case WebExceptionStatus.ConnectFailure:
				case WebExceptionStatus.ConnectionClosed:
				case WebExceptionStatus.SecureChannelFailure:
				case WebExceptionStatus.ReceiveFailure:
				case WebExceptionStatus.KeepAliveFailure:
				case WebExceptionStatus.SendFailure:
				case WebExceptionStatus.RequestCanceled:
				case WebExceptionStatus.Timeout:
					return true;
				default:
					return false;
			}
		}
	}
}
