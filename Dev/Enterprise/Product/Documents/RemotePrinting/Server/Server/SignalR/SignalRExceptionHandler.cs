using System;
using System.Collections;
using System.Linq;
using System.Web;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.RemotePrinting.Server
{
	public class SignalRExceptionHandler : IExtraExceptionHandler
	{
		public static void RegisterHandler()
		{
			if (ObjectFactory.Get(TopLevelExceptionHandler.ExtraExceptionHandlersListName) is ArrayList extraExceptionHandlers)
			{
				lock (registerLock)
				{
					if (!extraExceptionHandlers.OfType<SignalRExceptionHandler>().Any())
					{
						extraExceptionHandlers.Add(new SignalRExceptionHandler());
					}
				}
			}
		}

		public static void UnregisterHandler()
		{
			if (ObjectFactory.Get(TopLevelExceptionHandler.ExtraExceptionHandlersListName) is ArrayList extraExceptionHandlers)
			{
				lock (registerLock)
				{
					var handler = extraExceptionHandlers.OfType<SignalRExceptionHandler>().FirstOrDefault();
					if (handler != null)
					{
						extraExceptionHandlers.Remove(handler);
					}
				}
			}
		}

		static readonly object registerLock = new object();

		public bool HandleException(Exception exceptionToHandle)
		{
			if (exceptionToHandle is HttpException && IsFromSignalRAsyncTask(exceptionToHandle.StackTrace))
			{
				// Abandoned failed async task originated in SignalR.
				// https://github.com/SignalR/SignalR/issues/4217
				// They say this exception can be logged or ignored.
				// There is nothing to do to handle it, SignalR will continue and retry operation or try to reconnect.
				return true;
			}

			return false;
		}

		bool IsFromSignalRAsyncTask(string stackTrace)
		{
			return stackTrace != null && stackTrace.Contains("Microsoft.AspNet.SignalR.TaskAsyncHelper", StringComparison.OrdinalIgnoreCase);
		}
	}
}
