using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public abstract class SummaryDeclarationsCommonSendMessageWrapper : EntryHeaderCommonSendMessageWrapper, ISummaryDeclarationsCommonMessageDataProvider
	{
		public SummaryDeclarationsCommonSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
		}

		public ZString SenderId => certificate.CertificateID;
	}
}
