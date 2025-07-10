using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.Common
{
	public class BaseWebExceptionReporter : BaseExceptionReporter, IErrorReporter
	{
		public BaseWebExceptionReporter(TopLevelExceptionHandler exceptionHandler)
			: base(exceptionHandler)
		{
		}
	}
}
