using CargoWise.Types;
using Enterprise.Customs.Common;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CIFIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => IncoTerms.CostInsuranceAndFreight;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(AUChargeCodeList.Codes.PackingCost))
			{
				result = new UCIIncoTerm().Calculate(invoice);
			}
			else
			{
				bool isOFTIncludedInITOT = invoice.HasChargesIncludedInITOT(CustomsChargeTypeList.Codes.OverseasFreight);
				bool isONSIncludedInITOT = invoice.HasChargesIncludedInITOT(CustomsChargeTypeList.Codes.OverseasInsurance);
				if (!isOFTIncludedInITOT && isONSIncludedInITOT)
				{
					result = new CNIIncoTerm().Calculate(invoice);
				}
				else
				{
					result = new ITOTIncoTermWithOFT_ONSCalculator(IncoTerm).Calculate(invoice);
				}
			}
			return result;
		}
	}
}
