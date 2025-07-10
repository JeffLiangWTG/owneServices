using CargoWise.Types;
using Enterprise.Customs.Common;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class UFBIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => IncoTerms.UnpackedFreeOnBoard;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(AUChargeCodeList.Codes.ForeignInlandFreight))
			{
				result = new UAFIncoTerm().Calculate(invoice);
			}
			return result;
		}
	}
}
