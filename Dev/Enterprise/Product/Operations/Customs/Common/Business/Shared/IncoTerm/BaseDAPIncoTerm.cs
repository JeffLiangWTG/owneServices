using CargoWise.Types;

namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public class DAPITOTIncoTermCalculator : BaseDeliveryIncoterm
	{
		public override ZString IncoTerm => Core.Constants.IncoTerms.DeliveredAtPlace;
	}
}
