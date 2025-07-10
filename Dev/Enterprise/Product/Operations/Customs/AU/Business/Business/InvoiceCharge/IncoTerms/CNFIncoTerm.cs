using CargoWise.Types;
using Enterprise.Customs.Common;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CNFIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => IncoTerms.CostFreightWithAmpersand;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(AUChargeCodeList.Codes.PackingCost))
			{
				result = new UCFIncoTerm().Calculate(invoice);
			}
			else if (invoice.HasChargesExcludedInITOT(AUChargeCodeList.Codes.OverseasFreight))
			{
				result = new FOBIncoTerm().Calculate(invoice);
			}
			return result;
		}
	}
}
