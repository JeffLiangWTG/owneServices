using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class CancellationSendingActionLookups : ZLookups
	{
		public CancellationSendingActionLookups(CancellationSendingAction parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ReasonList => Parent.Factory.GetCachedValue<EMCSCancellationReasonList>();
	}
}
