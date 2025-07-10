using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public class ITOTIncoTermWithOFT_ONSCalculator : IITOTIncoTermCalculator
	{
		public ITOTIncoTermWithOFT_ONSCalculator(ZString incoTerm)
		{
			this.IncoTerm = Argument.NotNullOrEmpty(incoTerm, nameof(incoTerm));
		}

		public ZString IncoTerm
		{
			get;
			private set;
		}

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;

			bool isOFTIncludedInITOT = invoice.HasChargesIncludedInITOT(OverseasFreightChargeCode);
			bool isOFTExcludedFromITOT = invoice.HasChargesExcludedInITOT(OverseasFreightChargeCode);
			bool isONSIncludedInITOT = invoice.HasChargesIncludedInITOT(OverseasInsuranceChargeCode);
			bool isONSExcludedFromITOT = invoice.HasChargesExcludedInITOT(OverseasInsuranceChargeCode);

			if (isOFTIncludedInITOT && !isONSIncludedInITOT)
			{
				result = CostAndFreightIncoTermCalculator.Calculate(invoice);
			}
			else if (!isOFTIncludedInITOT && isONSIncludedInITOT)
			{
				result = IncoTermAndCustomsChargeFactory.ErrorIncoTermCode;
			}
			else if (!isOFTIncludedInITOT && isOFTExcludedFromITOT
				&& !isONSIncludedInITOT && isONSExcludedFromITOT)
			{
				result = FreeOnBoardIncoTermCalculator.Calculate(invoice);
			}
			return result;
		}

		protected virtual ZString OverseasFreightChargeCode => CustomsChargeTypeList.Codes.OverseasFreight;
		protected virtual ZString OverseasInsuranceChargeCode => CustomsChargeTypeList.Codes.OverseasInsurance;
		protected virtual IITOTIncoTermCalculator CostAndFreightIncoTermCalculator => new CFRITOTIncoTermCalculator();
		protected virtual IITOTIncoTermCalculator FreeOnBoardIncoTermCalculator => new FOBITOTIncoTermCalculator();
	}
}
