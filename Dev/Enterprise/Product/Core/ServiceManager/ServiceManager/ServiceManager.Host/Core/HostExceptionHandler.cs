using System;
using System.Net;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Integration.ServiceHostClient.Exceptions;

namespace Enterprise.ServiceManager.Host
{
	public class HostExceptionHandler : ExceptionHandler
	{
		protected override ExceptionAction EvaluateExceptionAction(Exception ex)
		{
			switch (ex)
			{
				case ServiceHostDeserializeException _:
				case HttpListenerException _:
					return ExceptionAction.LogError;
				case HostInternalException _:
					return ExceptionAction.Ignore;
				default:
					return ExceptionAction.Report;
			}
		}
	}
}
