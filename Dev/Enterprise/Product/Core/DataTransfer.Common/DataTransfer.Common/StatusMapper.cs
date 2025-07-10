using CargoWise.Common;

namespace Enterprise.DataTransfer.Common
{
	public static class StatusMapper
	{
		public static string MapNativeStatusToUniversal(NativeResponseStatus status)
		{
			switch (status)
			{
				case NativeResponseStatus.Accepted:
				case NativeResponseStatus.Warning:
					return UniversalResponseStatus.ProcessedOK;

				case NativeResponseStatus.Rejected:
					return UniversalResponseStatus.Error;

				default:
					ErrorReporter.ReportOnce("Unknown NativeResponseStatus in method MapNativeStatusToUniversal");
					return UniversalResponseStatus.Error;
			}
		}
	}
}
