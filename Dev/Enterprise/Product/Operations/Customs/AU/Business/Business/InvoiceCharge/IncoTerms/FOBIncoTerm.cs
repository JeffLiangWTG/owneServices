using CargoWise.Types;
using Enterprise.Customs.Common;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class FOBIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => IncoTerms.FreeOnBoard;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(AUChargeCodeList.Codes.PackingCost))
			{
				result = new UFBIncoTerm().Calculate(invoice);
			}
			else if (invoice.HasChargesExcludedInITOT(CustomsChargeTypeList.Codes.ForeignInlandFreight))
			{
				result = new PAFIncoTerm().Calculate(invoice);
			}
			return result;
		}
	}
}
