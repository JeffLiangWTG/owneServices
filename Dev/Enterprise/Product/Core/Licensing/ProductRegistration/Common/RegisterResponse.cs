
namespace Enterprise.ProductRegistration.Common
{
	public class RegisterResponse
	{
		public int Status { get; set; }
		public string Key { get; set; }
		public string ErrorMsg { get; set; }
	}

	/// <summary>
	/// Values for the Status field in replies from the registration service.
	/// E.g., RegisterResponse.Status and VerifyResponse.Status.
	/// </summary>
	public enum RegisterStatus
	{
		Success = 0,
		ProductKeyNotFound = 1,
		ProductKeyUnavailable = 2,
		UniqueKeyUpdated = 3, // Verify only: UniqueKey only partially matched since server had moved within an availability group
		UniqueKeyNotMatched = 4, // Verify only: UniqueKey did not current match registration
		Unregistered = 5, // Verify only: The database has been unregistered
		ProductNotLicensed = 6,

		InternalError = 100,
		RequestInvalid = 101, // Request parameters not valid
	}
}
