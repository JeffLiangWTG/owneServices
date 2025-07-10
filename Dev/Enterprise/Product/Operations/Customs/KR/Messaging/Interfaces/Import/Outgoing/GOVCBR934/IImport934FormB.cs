using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport934FormB : IImport934_5SMFormD
	{
		ZDecimal ExpectedCustomsValue { get; }
		IValuationMethodData Method2_3ValuationData { get; }
		IValuation4_MethodData Method4ValuationData { get; }
		IValuationMethodData Method5_6ValuationData { get; }
	}
}
