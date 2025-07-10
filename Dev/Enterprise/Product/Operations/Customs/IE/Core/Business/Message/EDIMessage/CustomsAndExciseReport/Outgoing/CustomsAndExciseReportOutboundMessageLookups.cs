using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class CustomsAndExciseReportOutboundMessageLookups : EDIMessageLookups
	{
		public CustomsAndExciseReportOutboundMessageLookups(CustomsAndExciseReportOutboundMessage parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ReportTypeList => Factory.GetCachedValue<CustomsAndExciseReportTypeList>();
	}
}
