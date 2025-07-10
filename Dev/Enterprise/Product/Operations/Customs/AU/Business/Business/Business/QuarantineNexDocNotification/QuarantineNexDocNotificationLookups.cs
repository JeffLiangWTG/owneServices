using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineNexDocNotificationLookups : AutoQuarantineNexDocNotificationLookups
	{
		public QuarantineNexDocNotificationLookups(AutoQuarantineNexDocNotification parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList NEXDOCMessageStatusList => Factory.GetCachedValue<NEXDOCMessageStatus>();

		public CodeDescriptionPairList NEXDOCAcknowledgeStatusList => Factory.GetCachedValue<NEXDOCAcknowledgeStatus>();
	}
}
