using Enterprise.DataTransfer.Common;
using Enterprise.DataTransfer.Native.Common.Logging;

namespace Enterprise.DataTransfer.Native.Business.Responses
{
	public static class LogBufferExtensions
	{
		public static NativeResponseStatus GetStatus(this ILogBuffer logs)
		{
			NativeResponseStatus status;
			if (logs.HasError())
			{
				status = NativeResponseStatus.Rejected;
			}
			else if (logs.HasWarning())
			{
				status = NativeResponseStatus.Warning;
			}
			else
			{
				status = NativeResponseStatus.Accepted;
			}

			return status;
		}
	}
}