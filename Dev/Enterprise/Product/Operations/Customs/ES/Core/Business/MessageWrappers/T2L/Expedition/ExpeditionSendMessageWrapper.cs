using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ExpeditionSendMessageWrapper : GenericExpeditionSendMessageWrapper, IExpeditionMessageDataProvider
	{
		public ExpeditionSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData)
			: base(cusEntryHeader, certificateData)
		{
		}

		public IExpeditionHeader Header => header ?? (header = new ExpeditionHeaderWrapper(entryHeader));
		ExpeditionHeaderWrapper header;
	}
}
