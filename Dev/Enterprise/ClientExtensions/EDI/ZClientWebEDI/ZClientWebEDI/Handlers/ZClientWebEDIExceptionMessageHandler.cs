using System;
using System.Web.Http.ExceptionHandling;
using CargoWise.Data;
using Enterprise.ZArchitecture.Web.Common;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;

namespace Enterprise.ZClientWebCargoWiseEDI.Handlers
{
	class ZClientWebEDIExceptionMessageHandler : TopLevelWebExceptionHandler
	{
		protected override void OnDatabaseUpgradedException()
		{
			// response to client differently from pipelined web applications
		}

		protected override void OnDatabaseUpgradeInProgressException()
		{
			// response to client differently from pipelined web applications
		}

		public static bool HandleException(ExceptionHandlerContext context)
		{
			var exceptionHandled = HandleUnhandledException(context.Exception);
			if (exceptionHandled)
			{
				switch (context.Exception)
				{
					case DatabaseUpgradedException _:
					case DatabaseUpgradeInProgressException _:
						context.Result = new EnterpriseWebApiExceptionHandler.DatabaseUpgradeResponse();
						break;
					default:
						if (context.Exception?.InnerException is DatabaseUpgradedException || context.Exception?.InnerException is DatabaseUpgradeInProgressException)
						{
							context.Result = new EnterpriseWebApiExceptionHandler.DatabaseUpgradeResponse();
						}

						break;
				}
			}

			return exceptionHandled;
		}

		protected override bool HandleWebApplicationException(Exception exceptionToHandle, Exception outermostExceptionForErrorReport)
		{
			var result = base.HandleWebApplicationException(exceptionToHandle, outermostExceptionForErrorReport);
			if (!result && (exceptionToHandle.InnerException is DatabaseUpgradedException || exceptionToHandle.InnerException is DatabaseUpgradeInProgressException))
			{
				return base.HandleWebApplicationException(exceptionToHandle.InnerException, exceptionToHandle.InnerException);
			}

			return result;
		}
	}
}
