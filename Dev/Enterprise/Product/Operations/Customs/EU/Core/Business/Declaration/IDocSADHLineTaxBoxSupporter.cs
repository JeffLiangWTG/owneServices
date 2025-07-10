using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface IDocSADHLineTaxBoxSupporter
	{
		ZString Type { get; }
		ZString TaxBase { get; }
		ZString Rate { get; }
		ZString RateDuty { get; }
		ZString RateOverride { get; }
		ZString AmountInDeclarationCurrency { get; }
		ZString MethodOfPayment { get; }
		ZString NationalFeeTypeCode { get; }
		ZString DeclarationMethodOfPayment { get; }
	}
}
