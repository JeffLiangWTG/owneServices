using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IValuation1_MethodData : IValuationMethodData
	{
		ZDecimal IndirectPaymentAmount { get; }
	}
}
