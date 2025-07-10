using CargoWise.Data;

namespace Enterprise.DocumentScanning.Business
{
#if DEBUG
	public
#endif
	static class SqlFailureChecker
	{
		public static bool IsAcceptableFailure(SqlException ex)
		{
			var errorType = new DbErrorHandler(ex, null).ExceptionType;
			switch (errorType)
			{
				case DbErrorType.InvalidObjectName:
				case DbErrorType.InaccessibleFiles:
				case DbErrorType.DeviceActivationError:
					{
						return true;
					}
				default:
					return false;
			}
		}
	}
}
