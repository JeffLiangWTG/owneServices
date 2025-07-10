using CargoWise.Types;

namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public class FOBITOTIncoTermCalculator : IITOTIncoTermCalculator
	{
		public ZString IncoTerm => Core.Constants.IncoTerms.FreeOnBoard;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(CustomsChargeTypeList.Codes.ForeignInlandFreight))
			{
				result = new EXWITOTIncoTermCalculator().Calculate(invoice);
			}
			return result;
		}
	}
}
