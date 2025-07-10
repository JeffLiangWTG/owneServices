using CargoWise.Types;

namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public class FASITOTIncoTermCalculator : IITOTIncoTermCalculator
	{
		public ZString IncoTerm => Core.Constants.IncoTerms.FreeAlongsideShip;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(CustomsChargeTypeList.Codes.PackingCost)
				|| invoice.HasChargesExcludedInITOT(CustomsChargeTypeList.Codes.ForeignInlandFreight))
			{
				result = new EXWITOTIncoTermCalculator().Calculate(invoice);
			}
			return result;
		}
	}
}
