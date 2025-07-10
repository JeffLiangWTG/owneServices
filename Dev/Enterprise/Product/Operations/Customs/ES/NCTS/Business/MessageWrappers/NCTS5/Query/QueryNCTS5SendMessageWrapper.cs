using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class QueryNCTS5SendMessageWrapper : NCTS5CommonSendMessageWrapper, IQueryNCTSMessageDataProvider
	{
		public QueryNCTS5SendMessageWrapper(NctsHeader header, ICertificateProvider certificateData) : base(header, certificateData)
		{
		}

		public INCTSCommonTransitOperationMRN TransitOperation => transitOperation ?? (transitOperation = new NCTS5CommonTransitOperationMRNWrapper(nctsHeader));
		NCTS5CommonTransitOperationMRNWrapper transitOperation;
	}
}
