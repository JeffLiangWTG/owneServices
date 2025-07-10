using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public class AmountAndTypeToBeGuaranteed
	{
		public ZDecimal AmountInDeclarationCurrency { get; set; }
		public GuaranteeDebitType DebitType { get; set; }
		public ZString Procedure { get; set; }
	}

	public enum GuaranteeDebitType
	{
		UNKNOWN,
		NORMAL,
		SUSPENDED
	}
}
