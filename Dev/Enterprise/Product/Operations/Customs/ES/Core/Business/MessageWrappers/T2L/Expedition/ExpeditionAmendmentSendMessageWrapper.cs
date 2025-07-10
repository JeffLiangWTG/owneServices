using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ExpeditionAmendmentSendMessageWrapper : GenericExpeditionSendMessageWrapper, IExpeditionAmendmentMessageDataProvider
	{
		public ExpeditionAmendmentSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData)
			: base(cusEntryHeader, certificateData)
		{
		}

		public IExpeditionAmendmentHeader Header => header ?? (header = new ExpeditionAmendmentHeaderWrapper(entryHeader));
		ExpeditionAmendmentHeaderWrapper header;
	}
}
