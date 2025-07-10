using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class ExplanationOnDelaySendingActionLookups : ZLookups
	{
		public ExplanationOnDelaySendingActionLookups(ExplanationOnDelaySendingAction parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ExplanationCodeList => Factory.GetCachedValue<EMCSExplanationOnDelayCodeList>();

		public CodeDescriptionPairList MessageRoleList => Factory.GetCachedValue<EMCSExplanationOnDelayMessageRoleCodeList>();
	}
}
