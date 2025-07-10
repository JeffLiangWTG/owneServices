using CargoWise.Types;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class UAFIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => IncoTerms.UnpackedAtFactory;

		public ZString Calculate(Common.ICommonInvoice invoice)
		{
			return IncoTerm;
		}
	}
}
