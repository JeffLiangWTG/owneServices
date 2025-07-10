using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IPaymentBasis
	{
		ZString CalculationBasis { get; }
		ZString Quantity { get; }
		ICodeDescription RateUnit { get; }
		ZDecimal RateValue { get; }
		ICodeDescription RateType { get; }
		ICodeDescription Currency { get; }
	}
}
