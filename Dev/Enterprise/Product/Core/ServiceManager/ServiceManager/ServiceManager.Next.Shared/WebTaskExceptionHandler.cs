using System.Net;
using Enterprise.ServiceManager.Shared;

namespace CargoWise.ServiceManager.Next.Shared;

public class WebTaskExceptionHandler : ExceptionHandler
{
	protected override ExceptionAction EvaluateExceptionAction(Exception ex)
	{
		return ex switch
		{
			HttpListenerException _ => ExceptionAction.LogError,
			_ => ExceptionAction.Report
		};
	}
}
