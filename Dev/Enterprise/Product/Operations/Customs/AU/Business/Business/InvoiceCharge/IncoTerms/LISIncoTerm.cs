using CargoWise.Types;
using Enterprise.Customs.Common;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class LISIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => IncoTerms.LandedIntoStore;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(AUChargeCodeList.Codes.LandingCharges))
			{
				result = new CIFIncoTerm().Calculate(invoice);
			}
			return result;
		}
	}
}
