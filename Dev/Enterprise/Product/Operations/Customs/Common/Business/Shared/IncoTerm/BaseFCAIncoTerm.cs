using CargoWise.Types;

namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public class FCAITOTIncoTermCalculator : IITOTIncoTermCalculator
	{
		public ZString IncoTerm => Core.Constants.IncoTerms.FreeCarrier;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(CustomsChargeTypeList.Codes.PackingCost))
			{
				result = new EXWITOTIncoTermCalculator().Calculate(invoice);
			}
			return result;
		}
	}
}
