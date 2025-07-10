using System.Web.Http.ExceptionHandling;
using CargoWise.Common;

namespace Enterprise.ZClientWebCargoWiseEDI.Handlers
{
	class ZClientWebEDIExceptionHandler : ExceptionHandler
	{
		public override void Handle(ExceptionHandlerContext context)
		{
			if (ZClientWebEDIExceptionMessageHandler.HandleException(context))
			{
				ErrorReporter.ReportOnce("ZClientWebEDIExceptionHandler", context.Exception);
			}

			base.Handle(context);
		}
	}
}
