using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class EarlyReleaseMiscMessageSendingObjectLookups : ZLookups
	{
		public EarlyReleaseMiscMessageSendingObjectLookups(EarlyReleaseMiscMessageSendingObject parent) : base(parent)
		{
		}

		public CodeDescriptionPairList SecurityTypeList => Factory.GetCachedValue<SecurityTypeCodeList>();
		public CodeDescriptionPairList ReasonForEarLyRemovalList => Factory.GetCachedValue<ReasonForEarlyRemovalCodeList>();
	}
}
