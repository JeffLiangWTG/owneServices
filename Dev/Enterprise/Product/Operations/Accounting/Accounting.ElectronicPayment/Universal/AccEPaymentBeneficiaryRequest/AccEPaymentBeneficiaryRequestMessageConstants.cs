using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicPayment.Universal
{
	public static class AccEPaymentBeneficiaryRequestMessageConstants
	{
		public static class XUEFieldNames
		{
			public static ZString BeneficiarySearchResult => nameof(BeneficiarySearchResult);
			public static ZString CompanyCode => nameof(CompanyCode);
			public static ZString BranchCode => nameof(BranchCode);
			public static ZString ErrorOriginatesAt => nameof(ErrorOriginatesAt);
			public static ZString ErrorMessage => nameof(ErrorMessage);
			public static ZString ErrorType => nameof(ErrorType);

			public static ZString[] GetRequiredFieldsForIAK() => new[] { BeneficiarySearchResult, CompanyCode, BranchCode };
			public static ZString[] GetRequiredFieldsForIRJ() => new[] { ErrorOriginatesAt, CompanyCode, ErrorMessage, ErrorType };
		}

		public static class MessageSubTypes
		{
			public const string SearchBeneficiary = "SBN";
		}
	}
}
