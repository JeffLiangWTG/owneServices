using System;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class PreValidateTraderResponse
	{
		public ExciseTraderValidationResponse ExciseTraderValidationResponse { get; set; }
		public DateTimeOffset ValidationTimestamp { get; set; }
		public ExciseTraderResponse[] ExciseTraderResponse { get; set; }
	}

	public sealed class ExciseTraderValidationResponse
	{
		public DateTimeOffset ValidationTimestamp { get; set; }
		public ExciseTraderResponse[] ExciseTraderResponse { get; set; }
	}

	public sealed class ExciseTraderResponse
	{
		public string ExciseRegistrationNumber { get; set; }
		public string EntityGroup { get; set; }
		public bool ValidTrader { get; set; }
		public string TraderType { get; set; }
		public string ErrorCode { get; set; }
		public string ErrorText { get; set; }
		public ValidateProductAuthorisationResponse ValidateProductAuthorisationResponse { get; set; }
	}

	public sealed class ValidateProductAuthorisationResponse
	{
		public bool Valid { get; set; }
		public ExciseProductError[] ProductError { get; set; }
	}

	public sealed class ExciseProductError
	{
		public string ExciseProductCode { get; set; }
		public string ErrorCode { get; set; }
		public string ErrorText { get; set; }
	}
}
