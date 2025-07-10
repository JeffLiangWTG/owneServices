using CargoWise.Types;

namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public interface IITOTIncoTermCalculator
	{
		ZString IncoTerm { get; }
		ZString Calculate(ICommonInvoice invoice);
	}
}
