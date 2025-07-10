using System.Collections;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ContractRevocation5ULLookups : Customs.Business.CusSupportingInfoLookups
	{
		public ContractRevocation5ULLookups(ContractRevocation5UL parent)
			: base(parent)
		{
		}

		public override ICollection CodeList => Factory.GetCachedValue<CancelReasonCodeList>();
	}
}
