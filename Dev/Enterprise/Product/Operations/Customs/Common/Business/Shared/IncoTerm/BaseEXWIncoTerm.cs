using CargoWise.Types;

namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public class EXWITOTIncoTermCalculator : IITOTIncoTermCalculator
	{
		public ZString IncoTerm => Core.Constants.IncoTerms.ExWorks;

		public ZString Calculate(ICommonInvoice invoice)
		{
			return IncoTerm;
		}
	}
}
