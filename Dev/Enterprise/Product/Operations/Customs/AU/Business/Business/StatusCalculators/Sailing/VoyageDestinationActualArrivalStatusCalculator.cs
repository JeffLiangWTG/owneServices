
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class VoyageDestinationActualArrivalStatusCalculator : CMRStatusCalculator<CustomsVoyageDestinationWrapper>
	{
		public VoyageDestinationActualArrivalStatusCalculator(CustomsVoyageDestinationWrapper destinationWrapper) : base(destinationWrapper)
		{
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.ActualArrivalStatus.CodeInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.AIRAAR };
	}
}
