using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE871BodyAnalysis
	{
		ZString LineNumber { get; }

		ZString ExciseProductCode { get; }

		ZDecimal ActualQuantity { get; }

		ZString Explanation { get; }
	}
}
