using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace Enterprise.Web.TestWebApplication.Common
{
	public class WebExceptionHandler : IExceptionHandler
	{
		public async Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
		{
			await Task.CompletedTask;
		}
	}
}
