using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class AESInboundEDIMessageLookups : EDIMessageLookups
	{
		public AESInboundEDIMessageLookups(AESInboundEDIMessage parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<CodeDescriptionPairList>("AESIncomingMessageTypeList", () =>
		{
			var result = new AESOutgoingMessageTypeList();
			result.AddRange(new AESIncomingMessageTypeList());
			result.Sort();
			return result;
		});
	}
}
