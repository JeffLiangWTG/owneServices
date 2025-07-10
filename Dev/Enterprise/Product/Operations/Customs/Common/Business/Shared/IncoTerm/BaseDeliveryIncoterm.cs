using CargoWise.Types;

namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public abstract class BaseDeliveryIncoterm : IITOTIncoTermCalculator
	{
		public abstract ZString IncoTerm { get; }

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;

			if (invoice.HasChargesIncludedInITOT(CustomsChargeTypeList.Codes.LandingCharges))
			{
				if (invoice.HasChargesExcludedInITOT(CustomsChargeTypeList.Codes.OverseasFreight))
				{
					result = IncoTermAndCustomsChargeFactory.ErrorIncoTermCode;
				}
			}
			else if (invoice.HasChargesExcludedInITOT(CustomsChargeTypeList.Codes.LandingCharges))
			{
				if (invoice.HasChargesIncludedInITOT(CustomsChargeTypeList.Codes.OverseasInsurance))
				{
					result = new CIFITOTIncoTermCalculator().Calculate(invoice);
				}
				else
				{
					result = new CFRITOTIncoTermCalculator().Calculate(invoice);
				}
			}
			else if (!invoice.HasChargesWithCurrency(CustomsChargeTypeList.Codes.LandingCharges))
			{
				if (invoice.HasChargesExcludedInITOT(CustomsChargeTypeList.Codes.OverseasFreight) || invoice.HasChargesExcludedInITOT(CustomsChargeTypeList.Codes.OverseasInsurance))
				{
					result = new ITOTIncoTermWithOFT_ONSCalculator(IncoTerm).Calculate(invoice);
				}
			}

			return result;
		}
	}
}
