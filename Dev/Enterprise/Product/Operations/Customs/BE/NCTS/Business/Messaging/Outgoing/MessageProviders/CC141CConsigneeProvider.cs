using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC141CConsigneeProvider : ConsigneeProvider
	{
		public CC141CConsigneeProvider(JobDocAddress jobDocAddress, bool isTransitionPeriodAES30 = false) : base(jobDocAddress, isTransitionPeriodAES30: isTransitionPeriodAES30)
		{
		}

		protected override ZBool IncludeName => true;

		protected override ZBool IncludeAddress => true;
	}
}
