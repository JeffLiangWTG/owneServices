using System.Net;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class HttpListenerExceptionHandler : IHttpListenerExceptionHandler
	{
		public bool HandleException(HttpListenerException httpListenerException, IHostLogger hostLogger)
		{
			switch (httpListenerException.NativeErrorCode)
			{
				case (int)HttpListenerErrorCodes.ErrorNonexistentNetworkConnection:
					hostLogger.Log(LogLevel.Warning, httpListenerException.Message);
					return true;

				case (int)HttpListenerErrorCodes.HandleIsInvalid:
				case (int)HttpListenerErrorCodes.ErrorNetNameDeleted:
				case (int)HttpListenerErrorCodes.ErrorOperationAborted:
					return true;

				default:
					return false;
			}
		}
	}
}
