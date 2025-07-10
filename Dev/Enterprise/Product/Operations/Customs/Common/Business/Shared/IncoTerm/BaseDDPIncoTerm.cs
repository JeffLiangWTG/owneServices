using CargoWise.Types;

namespace Enterprise.Customs.Common.ITOTIncoTerm
{
	public class DDPITOTIncoTermCalculator : BaseDeliveryIncoterm
	{
		public override ZString IncoTerm => Core.Constants.IncoTerms.DeliveredDutyPaid;
	}
}
