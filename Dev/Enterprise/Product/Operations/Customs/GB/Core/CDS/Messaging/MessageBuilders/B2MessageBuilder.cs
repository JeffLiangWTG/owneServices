using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class B2MessageBuilder : ExportsMessageBuilder
	{
		public B2MessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCodeNewAmendDelete)
			: base(cusEntryHeader, errorCollector, functionCodeNewAmendDelete)
		{
		}

		protected override void PopulateWareHouse()
		{
		}
	}
}
