using CargoWise.Types;

namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public class DATITOTIncoTermCalculator : BaseDeliveryIncoterm
	{
		public override ZString IncoTerm => Core.Constants.IncoTerms.DeliveredAtTerminal;
	}
}
