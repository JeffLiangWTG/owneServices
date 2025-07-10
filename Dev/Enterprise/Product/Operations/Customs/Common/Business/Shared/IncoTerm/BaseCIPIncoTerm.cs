namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public class CIPITOTIncoTermCalculator : ITOTIncoTermWithOFT_ONSCalculator
	{
		public CIPITOTIncoTermCalculator()
			: base(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo)
		{
		}
	}
}
