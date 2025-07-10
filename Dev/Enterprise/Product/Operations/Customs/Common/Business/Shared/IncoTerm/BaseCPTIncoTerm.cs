using CargoWise.Types;

namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public class CPTITOTIncoTermCalculator : IITOTIncoTermCalculator
	{
		public ZString IncoTerm => Core.Constants.IncoTerms.CarriagePaidTo;

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
