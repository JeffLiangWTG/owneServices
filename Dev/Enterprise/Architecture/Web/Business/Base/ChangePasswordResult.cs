namespace Enterprise.ZArchitecture.Web.Business
{
	public sealed class ChangePasswordResult
	{
		public static ChangePasswordResult Success(string message) => new ChangePasswordResult(isSuccess: true, partiallyFailed: false, message);
		public static ChangePasswordResult Failure(string message) => new ChangePasswordResult(isSuccess: false, partiallyFailed: false, message);
		public static ChangePasswordResult PartialSuccess(string message) => new ChangePasswordResult(isSuccess: true, partiallyFailed: true, message);

		ChangePasswordResult(bool isSuccess, bool partiallyFailed, string message)
		{
			IsSuccess = isSuccess;
			PartiallyFailed = partiallyFailed;
			Message = message;
		}

		public bool IsSuccess { get; }
		public bool PartiallyFailed { get; }
		public string Message { get; }
	}
}
