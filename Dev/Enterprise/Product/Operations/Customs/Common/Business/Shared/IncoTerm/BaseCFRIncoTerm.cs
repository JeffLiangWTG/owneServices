using CargoWise.Types;

namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public class CFRITOTIncoTermCalculator : IITOTIncoTermCalculator
	{
		public ZString IncoTerm => Core.Constants.IncoTerms.CostAndFreight;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(CustomsChargeTypeList.Codes.OverseasFreight))
			{
				result = new FOBITOTIncoTermCalculator().Calculate(invoice);
			}
			return result;
		}
	}
}
