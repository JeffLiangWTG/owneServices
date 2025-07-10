using CargoWise.Types;
using Enterprise.Customs.Common.ITOTIncoTerm;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ITOTIncoTermWithOFT_ONSCalculator : Common.ITOTIncoTerm.ITOTIncoTermWithOFT_ONSCalculator
	{
		public ITOTIncoTermWithOFT_ONSCalculator(ZString incoTerm)
			: base(incoTerm)
		{ }

		protected override ZString OverseasFreightChargeCode => AUChargeCodeList.Codes.OverseasFreight;
		protected override ZString OverseasInsuranceChargeCode => AUChargeCodeList.Codes.OverseasInsurance;
		protected override IITOTIncoTermCalculator CostAndFreightIncoTermCalculator => new CNFIncoTerm();
		protected override IITOTIncoTermCalculator FreeOnBoardIncoTermCalculator => new FOBIncoTerm();
	}
}
