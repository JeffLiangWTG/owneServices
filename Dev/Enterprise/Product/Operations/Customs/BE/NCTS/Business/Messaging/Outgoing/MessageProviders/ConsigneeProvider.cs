using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class ConsigneeProvider : PartyProvider
	{
		public ConsigneeProvider(JobDocAddress jobDocAddress, bool isTransitionPeriodAES30 = false) : base(jobDocAddress, isTransitionPeriodAES30: isTransitionPeriodAES30)
		{
		}

		protected override ZBool IncludeContactPerson => false;
	}
}
