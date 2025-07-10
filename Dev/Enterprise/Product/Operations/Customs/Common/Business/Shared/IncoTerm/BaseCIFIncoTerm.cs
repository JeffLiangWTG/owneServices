namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public class CIFITOTIncoTermCalculator : ITOTIncoTermWithOFT_ONSCalculator
	{
		public CIFITOTIncoTermCalculator()
			: base(Core.Constants.IncoTerms.CostInsuranceAndFreight)
		{
		}
	}
}
