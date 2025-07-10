using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class B1MessageBuilder : ExportsMessageBuilder
	{
		public B1MessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCodeNewAmendDelete)
			: base(cusEntryHeader, errorCollector, functionCodeNewAmendDelete)
		{
		}
	}
}
