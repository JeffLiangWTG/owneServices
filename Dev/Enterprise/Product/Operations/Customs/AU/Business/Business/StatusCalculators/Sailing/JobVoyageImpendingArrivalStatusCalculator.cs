
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobVoyageImpendingArrivalStatusCalculator : CMRStatusCalculator<CustomsJobVoyageWrapper>
	{
		public JobVoyageImpendingArrivalStatusCalculator(CustomsJobVoyageWrapper voyageWrapper) : base(voyageWrapper)
		{
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.ImpendingArrivalStatus.CodeInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.AIRIAR };
	}
}
