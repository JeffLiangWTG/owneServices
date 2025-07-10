using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NCTSOutboundEDIMessageLookups : EDIMessageLookups
	{
		public NCTSOutboundEDIMessageLookups(NCTSOutboundEDIMessage parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<CodeDescriptionPairList>("NCTSOutgoingMessageTypeList", () =>
		{
			var result = new NCTSOutgoingMessageTypeList();
			result.RemoveCode(NCTSOutgoingMessageTypeList.Codes.QueryOnGuarantees);
			result.Sort();
			return result;
		});
	}
}
